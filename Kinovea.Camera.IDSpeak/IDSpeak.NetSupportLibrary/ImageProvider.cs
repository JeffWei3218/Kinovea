using peak;
using peak.core;
using peak.core.nodes;
using peak.ipl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kinovea.Camera.IDSpeak
{
    /* The ImageProvider is responsible for opening and closing a device, it takes care of the grabbing and buffer handling,
     it notifies the user via events about state changes, and provides access to GenICam parameter nodes of the device.
     The grabbing is done in an internal thread. After an image is grabbed the image ready event is fired by the grab
     thread. The image can be acquired using GetCurrentImage(). After processing of the image it can be released via ReleaseImage.
     The image is then queued for the next grab.  */
    public class ImageProvider
    {
        /* Simple data class for holding image data. */
        public class Image
        {
            public Image(int newWidth, int newHeight, Byte[] newBuffer, bool color)
            {
                Width = newWidth;
                Height = newHeight;
                Buffer = newBuffer;
                Color = color;
            }

            public readonly int Width; /* The width of the image. */
            public readonly int Height; /* The height of the image. */
            public readonly Byte[] Buffer; /* The raw image data. */
            public readonly bool Color; /* If false the buffer contains a Mono8 image. Otherwise, RGBA8packed is provided. */
        }

        /* The class GrabResult is used internally to queue grab results. */
        protected class GrabResult
        {
            public Image ImageData; /* Holds the taken image. */
            public peak.core.Buffer Handle; /* Holds the handle of the image registered at the stream grabber. It is used to queue the buffer associated with itself for the next grab. */
        }

        protected Device m_hDevice;
        protected DataStream m_dataStream;
        protected peak.ipl.ImageConverter m_imageConverter;
        protected peak.ipl.GammaCorrector m_gammaCorrector;
        protected peak.ipl.ImageTransformer m_imageTransformer;
        protected NodeMap m_nodeMap;
        protected float m_gammaCorrectionValue = 1.0f;     /* The gamma correction value. */
        protected ImageTransformerConstants m_imageTransformerConstants = ImageTransformerConstants.None;     /* The image transformer value. */
        protected uint m_numberOfBuffersUsed = 5;          /* Number of m_buffers used in grab. */
        protected bool m_grabThreadRun = false;            /* Indicates that the grab thread is active.*/
        protected bool m_open = false;                     /* Indicates that the device is open and ready to grab.*/
        protected bool m_grabOnce = false;                 /* Use for single frame mode. */
        protected bool m_removed = false;                  /* Indicates that the device has been removed from the PC. */
        protected Thread m_grabThread;                     /* Thread for grabbing the images. */
        protected Object m_lockObject;                     /* Lock object used for thread synchronization. */
        protected List<GrabResult> m_grabbedBuffers; /* List of grab results already grabbed. */
        protected string m_lastError = "";                 /* Holds the error information belonging to the last exception thrown. */
        protected PixelFormat m_targetPixelFormat = new PixelFormat(PixelFormatName.BGR8);
        protected bool m_converterOutputFormatIsColor = false;/* The output format of the format converter. */
        /* Constructor with creation of basic objects. */
        public ImageProvider()
        {
            /* Create a thread for image grabbing. */
            m_grabThread = new Thread(Grab);
            /* Create objects used for buffer handling. */
            m_lockObject = new Object();
            m_grabbedBuffers = new List<GrabResult>();
            // The 'lost' event is only called for this application's opened devices if
            // a device is closed explicitly or if connection is lost while the reconnect is disabled,
            // otherwise the 'disconnected' event is triggered.
            // Other devices that were not opened or were opened by someone else still trigger
            // a 'lost' event.
            DeviceManager.Instance().DeviceLostEvent += DeviceManager_DeviceLostEvent;
            // Only called if the reconnect is enabled and if the device was previously opened by this
            // application instance.
            DeviceManager.Instance().DeviceDisconnectedEvent += DeviceManager_DeviceDisconnectedEvent;
            // When a device that was opened by the same application instance regains connection
            // after a previous disconnect the 'Reconnected' event is triggered.
            DeviceManager.Instance().DeviceReconnectedEvent += DeviceManager_DeviceReconnectedEvent;
        }
        
        /* Indicates that ImageProvider and device are open. */
        public bool IsOpen
        {
            get { return m_open; }
        }
        public Device Device
        {
            get { return m_hDevice; }
        }
        public NodeMap NodeMap
        {
            get { return m_nodeMap; }
        }
        public float GammaCorrectionValue
        {
            get { return m_gammaCorrectionValue; }
            set { m_gammaCorrectionValue = value; }
        }
        public ImageTransformerConstants ImageTransformerConstants
        {
            get { return m_imageTransformerConstants; }
            set { m_imageTransformerConstants = value; }
        }
        /* Open using index. Before ImageProvider can be opened using the index, Pylon.EnumerateDevices() needs to be called. */
        public void Open(uint index)
        {
            /* Get a handle for the device and proceed. */
            Open(DeviceEnumerator.Instance().GetDeviceByIndex(index));
        }

        /* Close the device */
        public void Close()
        {
            /* Notify that ImageProvider is about to close the device to give other objects the chance to do clean up operations. */
            OnDeviceClosingEvent();

            /* Try to close everything even if exceptions occur. Keep the last exception to throw when it is done. */
            Exception lastException = null;

            /* Reset the removed flag. */
            m_removed = false;
            // If device was opened, try to stop acquisition
            if (m_nodeMap != null)
            {
                try
                {
                    m_nodeMap.FindNode<CommandNode>("AcquisitionStop").Execute();
                    m_nodeMap.FindNode<CommandNode>("AcquisitionStop").WaitUntilDone();
                    // Unlock parameters after acquisition stop
                    m_nodeMap.FindNode<IntegerNode>("TLParamsLocked").SetValue(0);
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            } 
            if (m_dataStream != null)
            {
                /* Try to close the stream grabber. */
                try
                {
                    m_dataStream.KillWait();
                    if(m_dataStream.IsGrabbing())
                        m_dataStream.StopAcquisition(AcquisitionStopMode.Default);
                    m_dataStream.Flush(DataStreamFlushMode.DiscardAll);
                    foreach (var buffer in m_dataStream.AnnouncedBuffers())
                    {
                        m_dataStream.RevokeBuffer(buffer);
                    }
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            }

            if (m_hDevice != null)
            {
                /* Try to destroy the device. */
                try
                {
                    m_hDevice.Dispose();
                    m_hDevice = null;
                }
                catch (Exception e) 
                { 
                    lastException = e; 
                }
            }

            /* Notify that ImageProvider is now closed.*/
            OnDeviceClosedEvent();

            /* If an exception occurred throw it. */
            if (lastException != null)
            {
                throw lastException;
            }
        }

        /* Start the grab of one image. */
        public void OneShot()
        {
            if (m_open && !m_grabThread.IsAlive) /* Only start when open and not grabbing already. */
            {
                /* Set up the grabbing and start. */
                m_numberOfBuffersUsed = 1;
                m_grabOnce = true;
                m_grabThreadRun = true;
                m_grabThread = new Thread(Grab);
                m_grabThread.Start();
            }
        }

        /* Start the grab of images until stopped. */
        public void ContinuousShot()
        {
            if (m_open && !m_grabThread.IsAlive)  /* Only start when open and not grabbing already. */
            {
                /* Set up the grabbing and start. */
                m_numberOfBuffersUsed = 5;
                m_grabOnce = false;
                m_grabThreadRun = true;
                m_grabThread = new Thread(Grab);
                m_grabThread.Start();
            }
        }

        /* Stops the grabbing of images. */
        public void Stop()
        {
            if (m_open && m_grabThread.IsAlive) /* Only start when open and grabbing. */
            {
                m_grabThreadRun = false; /* Causes the grab thread to stop. */
                m_grabThread.Join(); /* Wait for it to stop. */
            }
        }

        /* Returns the next available image in the grab result queue. Null is returned if no result is available.
           An image is available when the ImageReady event is fired. */
        public Image GetCurrentImage()
        {
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                if (m_grabbedBuffers.Count > 0) /* If images available. */
                {
                    return m_grabbedBuffers[0].ImageData;
                }
            }
            return null; /* No image available. */
        }

        /* Returns the latest image in the grab result queue. All older images are removed. Null is returned if no result is available.
           An image is available when the ImageReady event is fired. */
        public Image GetLatestImage()
        {
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                /* Release all images but the latest. */
                while (m_grabbedBuffers.Count > 1)
                {
                    ReleaseImage();
                }
                if (m_grabbedBuffers.Count > 0) /* If images available. */
                {
                    return m_grabbedBuffers[0].ImageData;
                }
            }
            return null; /* No image available. */
        }

        /* After the ImageReady event has been received and the image was acquired by using GetCurrentImage,
        the image must be removed from the grab result queue and added to the stream grabber queue for the next grabs. */
        public bool ReleaseImage()
        {
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                if (m_grabbedBuffers.Count > 0) /* If images are available and grabbing is in progress.*/
                {
                    if (m_grabThreadRun)
                    {
                        /* Requeue the buffer. */
                        // Queue buffer so that it can be used again 
                        m_dataStream.QueueBuffer(m_grabbedBuffers[0].Handle);
                    }
                    /* Remove it from the grab result queue. */
                    m_grabbedBuffers.RemoveAt(0);
                    return true;
                }
            }
            return false;
        }

        /* Returns the last error message. Usually called after catching an exception. */
        public string GetLastErrorMessage()
        {
            string text = m_lastError;
            m_lastError = "";
            return text;
        }

        /* Open using device.*/
        public void Open(DeviceDescriptor deviceDescriptor)
        {
            try
            {
                m_hDevice = null;
                m_dataStream = null;
                m_nodeMap = null;
                if (deviceDescriptor == null || !deviceDescriptor.IsOpenable())
                {
                    throw new Exception("Camera could not be opened for configuring parameters and for grabbing images.");
                }
                /* Before using the device, it must be opened. Open it for configuring
                parameters and for grabbing images. */
                m_hDevice = deviceDescriptor.OpenDevice(DeviceAccessType.Control);
                var systemNodeMap = m_hDevice.ParentInterface().ParentSystem().NodeMaps()[0];

                if (!EnableReconnect(systemNodeMap))
                {
                    throw new Exception("Camera could not be enable for reconect.");
                }

                // Get nodemap of remote device for all accesses to the genicam nodemap tree
                m_nodeMap = m_hDevice.RemoteDevice().NodeMaps()[0];

                // To prepare for untriggered continuous image acquisition, load the UserSet1 user set if available
                // and wait until execution is finished
                try
                {
                    m_nodeMap.FindNode<EnumerationNode>("UserSetSelector").SetCurrentEntry("UserSet1");
                    m_nodeMap.FindNode<CommandNode>("UserSetLoad").Execute();
                    m_nodeMap.FindNode<CommandNode>("UserSetLoad").WaitUntilDone();
                }
                catch
                {
                    // UserSet is not available
                }
                // Get a list of all available entries of TriggerSelector
                var allEntries = m_nodeMap.FindNode<EnumerationNode>("TriggerSelector").Entries();
                List<string> availableEntries = new List<string>();
                for (int i = 0; i < allEntries.Count(); ++i)
                {
                    if ((allEntries[i].AccessStatus() != NodeAccessStatus.NotAvailable)
                            && (allEntries[i].AccessStatus() != NodeAccessStatus.NotImplemented))
                    {
                        availableEntries.Add(allEntries[i].SymbolicValue());
                    }
                }
                /* Disable acquisition start trigger if available. */
                if (availableEntries.Contains("AcquisitionStart"))
                {
                    m_nodeMap.FindNode<EnumerationNode>("TriggerSelector").SetCurrentEntry("AcquisitionStart");
                    m_nodeMap.FindNode<EnumerationNode>("TriggerMode").SetCurrentEntry("Off");
                }
                
                
                /* Disable frame burst start trigger if available */
                if (availableEntries.Contains("FrameBurstStart"))
                {
                    m_nodeMap.FindNode<EnumerationNode>("TriggerSelector").SetCurrentEntry("FrameBurstStart");
                    m_nodeMap.FindNode<EnumerationNode>("TriggerMode").SetCurrentEntry("Off");
                }

                /* Disable frame start trigger if available. */
                if (availableEntries.Contains("FrameStart"))
                {
                    m_nodeMap.FindNode<EnumerationNode>("TriggerSelector").SetCurrentEntry("FrameStart");
                    m_nodeMap.FindNode<EnumerationNode>("TriggerMode").SetCurrentEntry("Off");
                }
                /* Image grabbing is done using a stream grabber.
                  A device may be able to provide different streams. A separate stream grabber must
                  be used for each stream. In this sample, we create a stream grabber for the default
                  stream, i.e., the first stream ( index == 0 ).
                  */
                /* Get the number of streams supported by the device and the transport layer. */
                if (!m_hDevice.DataStreams().Any())
                {
                    throw new Exception("The transport layer doesn't support image streams.");
                }
                /* Create and open a stream grabber for the first channel. */
                m_dataStream = m_hDevice.DataStreams()[0].OpenDataStream();
            }
            catch
            {
                try
                {
                    Close(); /* Try to close any open handles. */
                }
                catch
                {
                    /* Another exception cannot be handled. */
                }
                throw;
            }

            /* Notify that the ImageProvider is open and ready for grabbing and configuration. */
            OnDeviceOpenedEvent();
        }

        private bool EnableReconnect(NodeMap systemNodeMap)
        {
            if (!systemNodeMap.HasNode("ReconnectEnable"))
            {
                return false;
            }

            var reconnectEnableNode = systemNodeMap.FindNode<BooleanNode>("ReconnectEnable");
            var reconnectEnableAccessStatus = reconnectEnableNode.AccessStatus();

            if (reconnectEnableAccessStatus == NodeAccessStatus.ReadWrite)
            {
                reconnectEnableNode.SetValue(true);
                return true;
            }

            if (reconnectEnableAccessStatus == NodeAccessStatus.ReadOnly)
            {
                if (reconnectEnableNode.Value())
                {
                    return true;
                }
            }
            return false;
        }

        /* Prepares everything for grabbing. */
        protected void SetupGrab()
        {
            /* Clear the grab result queue. This is not done when cleaning up to still be able to provide the
             images, e.g. in single frame mode.*/
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                m_grabbedBuffers.Clear();
            }

            /* Set the acquisition mode */
            if (m_grabOnce)
            {
                /* We will use the single frame mode, to take one image. */
                m_nodeMap.FindNode<EnumerationNode>("AcquisitionMode").SetCurrentEntry("SingleFrame");
            }
            else
            {
                /* We will use the Continuous frame mode, i.e., the camera delivers
                images continuously. */
                m_nodeMap.FindNode<EnumerationNode>("AcquisitionMode").SetCurrentEntry("Continuous");
            }

            /* Clear the grab buffers to assure proper operation (because they may
             still be filled if the last grab has thrown an exception). */
            // Flush queue and prepare all buffers for revoking
            m_dataStream.Flush(DataStreamFlushMode.DiscardAll);

            // Clear all old buffers
            foreach (var buffer in m_dataStream.AnnouncedBuffers())
            {
                m_dataStream.RevokeBuffer(buffer);
            }
            /* Determine the required size of the grab buffer. */
            var payloadSize = m_nodeMap.FindNode<IntegerNode>("PayloadSize").Value();

            /* We must tell the stream grabber the number and size of the m_buffers
                we are using. */
            // Get number of minimum required buffers
            var numBuffersMinRequired = m_dataStream.NumBuffersAnnouncedMinRequired();

            // Alloc buffers
            for (var count = 0; count < numBuffersMinRequired; count++)
            {
                var buffer = m_dataStream.AllocAndAnnounceBuffer((uint)payloadSize, IntPtr.Zero);
                m_dataStream.QueueBuffer(buffer);
            }

            // Lock critical features to prevent them from changing during acquisition
            m_nodeMap.FindNode<IntegerNode>("TLParamsLocked").SetValue(1);

            var inputPixelFormat = (PixelFormatName)m_nodeMap.FindNode<EnumerationNode>("PixelFormat").CurrentEntry().Value();
            var width = m_nodeMap.FindNode<IntegerNode>("Width").Value();
            var height = m_nodeMap.FindNode<IntegerNode>("Height").Value();

            // Pre-allocate conversion buffers to speed up first image conversion
            // while the acquisition is running
            // NOTE: Re-create the image converter, so old conversion buffers get freed
            m_imageConverter = new peak.ipl.ImageConverter();
            m_imageConverter.PreAllocateConversion(inputPixelFormat, m_targetPixelFormat, (uint)width, (uint)height);

            m_gammaCorrector = new GammaCorrector();
            m_gammaCorrector.SetGammaCorrectionValue(m_gammaCorrectionValue);
            m_imageTransformer = new peak.ipl.ImageTransformer();

            // Start acquisition
            m_dataStream.StartAcquisition();
            m_nodeMap.FindNode<CommandNode>("AcquisitionStart").Execute();
            m_nodeMap.FindNode<CommandNode>("AcquisitionStart").WaitUntilDone();
        }

        /* This method is executed using the grab thread and is responsible for grabbing, possible conversion of the image
        ,and queuing the image to the result queue. */
        protected void Grab()
        {
            /* Notify that grabbing has started. This event can be used to update the state of the GUI. */
            OnGrabbingStartedEvent();
            try
            {
                /* Set up everything needed for grabbing. */
                SetupGrab();

                while (m_grabThreadRun) /* Is set to false when stopping to end the grab thread. */
                {
                    // Get buffer from device's datastream
                    var buffer = m_dataStream.WaitForFinishedBuffer(1000);
                    /* Wait for the next buffer to be filled. Wait up to 1000 ms. */
                    if (!buffer.HasNewData() || !buffer.HasImage())
                    {
                        lock (m_lockObject)
                        {
                            if (m_grabbedBuffers.Count != m_numberOfBuffersUsed)
                            {
                                /* A timeout occurred. This can happen if an external trigger is used or
                                   if the programmed exposure time is longer than the grab timeout. */
                                throw new Exception("A grab timeout occurred.");
                            }
                            continue;
                        }
                    }
                    
                    /* Add result to the ready list. */
                    EnqueueTakenImage(buffer);
                    
                    /* Notify that an image has been added to the output queue. The receiver of the event can use GetCurrentImage() to acquire and process the image
                     and ReleaseImage() to remove the image from the queue and return it to the stream grabber.*/
                    OnImageReadyEvent();

                    /* Exit here for single frame mode. */
                    if (m_grabOnce)
                    {
                        m_grabThreadRun = false;
                        break;
                    }
                }

                /* Tear down everything needed for grabbing. */
                CleanUpGrab();
            }
            catch (Exception e)
            {
                /* The grabbing stops due to an error. Set m_grabThreadRun to false to avoid that any more buffers are queued for grabbing. */
                m_grabThreadRun = false;

                /* Get the last error message here, because it could be overwritten by cleaning up. */
                string lastErrorMessage = e.Message;

                try
                {
                    /* Try to tear down everything needed for grabbing. */
                    CleanUpGrab();
                }
                catch
                {
                    /* Another exception cannot be handled. */
                }

                /* Notify that grabbing has stopped. This event could be used to update the state of the GUI. */
                OnGrabbingStoppedEvent();

                if (!m_removed) /* In case the device was removed from the PC suppress the notification. */
                {
                    /* Notify that the grabbing had errors and deliver the information. */
                    OnGrabErrorEvent(e, lastErrorMessage);
                }
                return;
            }
            /* Notify that grabbing has stopped. This event could be used to update the state of the GUI. */
            OnGrabbingStoppedEvent();
        }

        protected void EnqueueTakenImage(peak.core.Buffer buffer)
        {
            /* Create a new grab result to enqueue to the grabbed buffers list. */
            GrabResult newGrabResultInternal = new GrabResult();
            newGrabResultInternal.Handle = buffer; /* Add the handle to requeue the buffer in the stream grabber queue. */

            // Create IDS peak IPL image
            // NOTE: This `peak.ipl.Image` still uses the underlying memory of `buffer`
            var iplImg = ids_peak_ipl_extension.BufferToImage(buffer);
            if (buffer.PixelFormat() == (ulong)PixelFormatName.BGR8 || buffer.PixelFormat() == (ulong)PixelFormatName.BGRa8)
            {
                m_converterOutputFormatIsColor = true;
                // Debayering and convert IDS peak IPL Image to RGB8 format
                // NOTE: Use `ImageConverter`, since the `ConvertTo` function re-allocates
                // the conversion buffers on every call
                iplImg = m_imageConverter.Convert(iplImg, m_targetPixelFormat);
            }
            else
            {
               m_converterOutputFormatIsColor = false;
            }
            if(m_gammaCorrector != null &&
                m_gammaCorrector.IsPixelFormatSupported(iplImg.PixelFormat().PixelFormatName()))
            {
                float gammaCorrectionValue = m_gammaCorrector.GammaCorrectionValue();
                iplImg = m_gammaCorrector.Process(iplImg);
            }
            if (m_imageTransformer != null && m_imageTransformerConstants != ImageTransformerConstants.None)
            {
                if(m_imageTransformerConstants == ImageTransformerConstants.Rotate180Deg)
                    iplImg = m_imageTransformer.Rotate(iplImg, ImageTransformer.RotationAngle.Degree180);
                else if (m_imageTransformerConstants == ImageTransformerConstants.MirrorLeftRight)
                    iplImg = m_imageTransformer.MirrorLeftRight(iplImg);
                else if (m_imageTransformerConstants == ImageTransformerConstants.MirrorUpDown)
                    iplImg = m_imageTransformer.MirrorUpDown(iplImg);
            }
            var byteCount = iplImg.ByteCount();
            byte[] bytes = new byte[byteCount];
            Marshal.Copy(iplImg.Data(), bytes, 0, (int)byteCount);
            newGrabResultInternal.ImageData = new Image((int)iplImg.Width(), (int)iplImg.Height(), bytes, m_converterOutputFormatIsColor);
            
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                m_grabbedBuffers.Add(newGrabResultInternal); /* Add the new grab result to the queue. */
            }
        }

        protected void CleanUpGrab()
        {
            /*  ... Stop the camera. */
            m_nodeMap.FindNode<CommandNode>("AcquisitionStop").Execute();
            m_nodeMap.FindNode<CommandNode>("AcquisitionStop").WaitUntilDone();

            /* ... Stop the image acquisition engine. */
            m_dataStream.StopAcquisition();

            /* Destroy the format converter if one was used. */
            if (m_imageConverter != null)
            {
                /* Destroy the converter. */
                m_imageConverter.Dispose();
                /* Set the handle invalid. The next grab cycle may not need a converter. */
                m_imageConverter = null;
            }
            /* Destroy the gamma corrector if one was used. */
            if (m_gammaCorrector != null)
            {
                /* Destroy the converter. */
                m_gammaCorrector.Dispose();
                /* Set the handle invalid. The next grab cycle may not need a converter. */
                m_gammaCorrector = null;
            }
            if(m_imageTransformer != null)
            {
                m_imageTransformer.Dispose();
                m_imageTransformer = null;
            }
            /* Clear the grab buffers to assure proper operation (because they may
             still be filled if the last grab has thrown an exception). */
            // Flush queue and prepare all buffers for revoking
            m_dataStream.Flush(DataStreamFlushMode.DiscardAll);

            // Clear all old buffers
            foreach (var buffer in m_dataStream.AnnouncedBuffers())
            {
                m_dataStream.RevokeBuffer(buffer);
            }

            /* After calling PylonStreamGrabberFinishGrab(), parameters that impact the payload size (e.g.,
            the AOI width and height parameters) are unlocked and can be modified again. */
        }

        private void DeviceManager_DeviceReconnectedEvent(object sender, DeviceDescriptor reconnectedDevice, DeviceReconnectInformation reconnectInformation)
        {
            if (m_hDevice == null || reconnectedDevice.Key() != m_hDevice.Key())
                return;
            // Using the `reconnectInformation` the user can tell whether they need to take actions
            // in order to resume the image acquisition.
            if (reconnectInformation.IsSuccessful())
            {
                // Device was reconnected successfully, nothing to do.
                return;
            }

            EnsureCompatibleBuffersAndRestartAcquisition(reconnectedDevice, reconnectInformation);
        }

        private void EnsureCompatibleBuffersAndRestartAcquisition(DeviceDescriptor reconnectedDevice, DeviceReconnectInformation reconnectInformation)
        {
            var device = reconnectedDevice.OpenedDevice();
            var nodeMapRemoteDevice = device.RemoteDevice().NodeMaps()[0];
            var dataStream = device.DataStreams()[0].OpenedDataStream();
            var payloadSize = (uint)nodeMapRemoteDevice.FindNode<IntegerNode>("PayloadSize").Value();

            bool hasPayloadSizeMismatch = payloadSize != dataStream.AnnouncedBuffers()[0].Size();

            // The payload size might have changed. In this case it's required to reallocate the buffers.
            if (hasPayloadSizeMismatch)
            {
                bool isDataSteamGrabbing = dataStream.IsGrabbing();
                if (isDataSteamGrabbing)
                {
                    dataStream.StopAcquisition();
                }

                // Discard all buffers from the acquisition engine.
                // They remain in the announced buffer pool.
                dataStream.Flush(DataStreamFlushMode.DiscardAll);
                var numBuffersBefore = dataStream.AnnouncedBuffers().Count;

                // Remove them from the announced pool.
                foreach (var buffer in dataStream.AnnouncedBuffers())
                {
                    dataStream.RevokeBuffer(buffer);
                }

                // Allocate and queue the buffers using the new "PayloadSize".
                var minBuffers = dataStream.NumBuffersAnnouncedMinRequired();
                var numBuffers = Math.Max(minBuffers, numBuffersBefore);
                for (int i = 0; i < numBuffers; i++)
                {
                    var buffer = dataStream.AllocAndAnnounceBuffer(payloadSize, IntPtr.Zero);
                    dataStream.QueueBuffer(buffer);
                }

                if (isDataSteamGrabbing)
                {
                    dataStream.StartAcquisition();
                }
            }

            if (!reconnectInformation.IsRemoteDeviceAcquisitionRunning())
            {
                nodeMapRemoteDevice.FindNode<CommandNode>("AcquisitionStart").Execute();
            }
        }

        private void DeviceManager_DeviceDisconnectedEvent(object sender, DeviceDescriptor disconnectedDevice)
        {
            if (m_hDevice == null || disconnectedDevice.Key() != m_hDevice.Key())
                return;
            
            /* Notify that the device has been removed from the PC. */
            OnDeviceRemovedEvent();
        }

        private void DeviceManager_DeviceLostEvent(object sender, string deviceKey)
        {
            if (m_hDevice == null || deviceKey != m_hDevice.Key())
                return;
            /* Notify that the device has been removed from the PC. */
            OnDeviceRemovedEvent();
        }
        /* The events fired by ImageProvider. See the invocation methods below for further information, e.g. OnGrabErrorEvent. */
        public delegate void DeviceOpenedEventHandler();
        public event DeviceOpenedEventHandler DeviceOpenedEvent;

        public delegate void DeviceClosingEventHandler();
        public event DeviceClosingEventHandler DeviceClosingEvent;

        public delegate void DeviceClosedEventHandler();
        public event DeviceClosedEventHandler DeviceClosedEvent;

        public delegate void GrabbingStartedEventHandler();
        public event GrabbingStartedEventHandler GrabbingStartedEvent;

        public delegate void ImageReadyEventHandler();
        public event ImageReadyEventHandler ImageReadyEvent;

        public delegate void GrabbingStoppedEventHandler();
        public event GrabbingStoppedEventHandler GrabbingStoppedEvent;

        public delegate void GrabErrorEventHandler(Exception grabException, string additionalErrorMessage);
        public event GrabErrorEventHandler GrabErrorEvent;

        public delegate void DeviceRemovedEventHandler();
        public event DeviceRemovedEventHandler DeviceRemovedEvent;

        /* Notify that ImageProvider is open and ready for grabbing and configuration. */
        protected void OnDeviceOpenedEvent()
        {
            m_open = true;
            if (DeviceOpenedEvent != null)
            {
                DeviceOpenedEvent();
            }
        }

        /* Notify that ImageProvider is about to close the device to give other objects the chance to do clean up operations. */
        protected void OnDeviceClosingEvent()
        {
            m_open = false;
            if (DeviceClosingEvent != null)
            {
                DeviceClosingEvent();
            }
        }

        /* Notify that ImageProvider is now closed.*/
        protected void OnDeviceClosedEvent()
        {
            m_open = false;
            if (DeviceClosedEvent != null)
            {
                DeviceClosedEvent();
            }
        }

        /* Notify that grabbing has started. This event could be used to update the state of the GUI. */
        protected void OnGrabbingStartedEvent()
        {
            if (GrabbingStartedEvent != null)
            {
                GrabbingStartedEvent();
            }
        }

        /* Notify that an image has been added to the output queue. The receiver of the event can use GetCurrentImage() to acquire and process the image
         and ReleaseImage() to remove the image from the queue and return it to the stream grabber.*/
        protected void OnImageReadyEvent()
        {
            if (ImageReadyEvent != null)
            {
                ImageReadyEvent();
            }
        }

        /* Notify that grabbing has stopped. This event could be used to update the state of the GUI. */
        protected void OnGrabbingStoppedEvent()
        {
            if (GrabbingStoppedEvent != null)
            {
                GrabbingStoppedEvent();
            }
        }

        /* Notify that the grabbing had errors and deliver the information. */
        protected void OnGrabErrorEvent(Exception grabException, string additionalErrorMessage)
        {
            if (GrabErrorEvent != null)
            {
                GrabErrorEvent(grabException, additionalErrorMessage);
            }
        }

        /* Notify that the device has been removed from the PC. */
        protected void OnDeviceRemovedEvent()
        {
            m_removed = true;
            m_grabThreadRun = false;
            if (DeviceRemovedEvent != null)
            {
                DeviceRemovedEvent();
            }
        }
    }
}
