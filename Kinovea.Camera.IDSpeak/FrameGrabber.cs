#region License
/*
Copyright © Joan Charmant 2014.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Timers;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using Kinovea.Pipeline;
using Kinovea.Services;
using peak;
using peak.core;
using peak.core.nodes;
using peak.ipl;
using System.Reflection;
using System.Globalization;

namespace Kinovea.Camera.IDSpeak
{
    /// <summary>
    /// The main grabbing class for Basler devices.
    /// </summary>
    public class FrameGrabber : ICaptureSource
    {
        public event EventHandler<FrameProducedEventArgs> FrameProduced;
        public event EventHandler GrabbingStatusChanged;
        
        #region Property
        public bool Grabbing
        {
            get { return grabbing; }
        }
        public float Framerate
        {
            get { return resultingFramerate; }
        }
        public double LiveDataRate
        {
            // Note: this variable is written by the stream thread and read by the UI thread.
            // We don't lock because freshness of values is not paramount and torn reads are not catastrophic either.
            // We eventually get an approximate value good enough for the purpose.
            get { return dataRateAverager.Average; }
        }
        #endregion

        #region Members
        private CameraSummary summary;
        private uint deviceIndex;
        private Device device;
        private NodeMap nodeMap;
        private ImageProvider imageProvider = new ImageProvider();
        private bool grabbing;
        private bool firstOpen = true;
        private float resultingFramerate = 0;
        private Finishline finishline = new Finishline();
        private Stopwatch swDataRate = new Stopwatch();
        private Averager dataRateAverager = new Averager(0.02);
        private const double megabyte = 1024 * 1024;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Public methods
        public FrameGrabber(CameraSummary summary, uint deviceIndex)
        {
            this.summary = summary;
            this.deviceIndex = deviceIndex;
        }

        /// <summary>
        /// Configure device and report frame format that will be used during streaming.
        /// This method must return a proper ImageDescriptor so we can pre-allocate buffers.
        /// </summary>
        public ImageDescriptor Prepare()
        {
            Open();
            
            if (device == null || nodeMap == null)
                return ImageDescriptor.Invalid;

            firstOpen = false;

            // Get the configured framerate for recording support.
            resultingFramerate = IDSHelper.GetFramerate(nodeMap);

            SpecificInfo specific = summary.Specific as SpecificInfo;
            int streamFormatSymbol = specific.StreamFormat;
            
            bool hasWidth = nodeMap.HasNode("Width");
            bool hasHeight = nodeMap.HasNode("Height");
            bool hasPixelFormat = nodeMap.HasNode("PixelFormat");
            bool canComputeImageDescriptor = hasWidth && hasHeight && hasPixelFormat;

            if (!canComputeImageDescriptor)
                return ImageDescriptor.Invalid;

            var pixelFormat = (PixelFormatName)nodeMap.FindNode<EnumerationNode>("PixelFormat").CurrentEntry().Value();
            var width = nodeMap.FindNode<IntegerNode>("Width").Value();
            var height = nodeMap.FindNode<IntegerNode>("Height").Value();

            if (pixelFormat == PixelFormatName.Invalid)
                return ImageDescriptor.Invalid;

            var pixelType = new PixelFormat(pixelFormat);
            bool isBayer = pixelType.IsBayered();
            bool isBayer8 = IDSHelper.IsBayer8(pixelFormat);
            bool bayerColor = (isBayer && !isBayer8);
            bool color = !IDSHelper.IsMono(pixelFormat) || bayerColor;
            ImageFormat format = color ? ImageFormat.RGB32 : ImageFormat.Y800;
            imageProvider.GammaCorrectionValue = specific.GammaCorrectionValue;
            finishline.Prepare((int)width, (int)height, format, resultingFramerate);
            if (finishline.Enabled)
            {
                height = finishline.Height;
                resultingFramerate = finishline.ResultingFramerate;
            }

            int bufferSize = ImageFormatHelper.ComputeBufferSize((int)width, (int)height, format);
            bool topDown = true;
            
            return new ImageDescriptor(format, (int)width, (int)height, topDown, bufferSize);
        }

        /// <summary>
        /// In case of configure failure, we would have retrieved a single image and the corresponding image descriptor.
        /// A limitation of the single snapshot retriever is that the format is always RGB24, even though the grabber may
        /// use a different format.
        /// </summary>
        public ImageDescriptor GetPrepareFailedImageDescriptor(ImageDescriptor input)
        {
            return input;
        }

        public void Start()
        {
            if (!imageProvider.IsOpen)
                Open();

            if (device == null || !imageProvider.IsOpen)
                return;

            log.DebugFormat("Starting device {0}, {1}", summary.Alias, summary.Identifier);

            imageProvider.GrabErrorEvent += imageProvider_GrabErrorEvent;
            imageProvider.GrabbingStartedEvent += imageProvider_GrabbingStartedEvent;
            imageProvider.DeviceRemovedEvent += imageProvider_DeviceRemovedEvent;
            imageProvider.ImageReadyEvent += imageProvider_ImageReadyEvent;

            try
            {
                imageProvider.ContinuousShot();
            }
            catch (Exception e)
            {
                LogError(e, imageProvider.GetLastErrorMessage());
            }
        }

        public void Stop()
        {
            log.DebugFormat("Stopping device {0}", summary.Alias);
            
            imageProvider.GrabErrorEvent -= imageProvider_GrabErrorEvent;
            imageProvider.GrabbingStartedEvent -= imageProvider_GrabbingStartedEvent;
            imageProvider.DeviceRemovedEvent -= imageProvider_DeviceRemovedEvent;
            imageProvider.ImageReadyEvent -= imageProvider_ImageReadyEvent;

            try
            {
                imageProvider.Stop();
            }
            catch (Exception e)
            {
                LogError(e, imageProvider.GetLastErrorMessage());
            }

            grabbing = false;
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        public void Close()
        {
            Stop();

            try
            {
                imageProvider.Close();
            }
            catch (Exception e)
            {
                LogError(e, imageProvider.GetLastErrorMessage());
            }
        }
        #endregion

        #region Private methods

        private void Open()
        {
            // Unlike in the DirectShow module, we do not backup and restore camera configuration.
            // If the user configured the camera outside of Kinovea we respect the new settings.
            // Two reasons:
            // 1. In DirectShow we must do the backup/restore to work around drivers that inadvertently reset the camera properties.
            // 2. Industrial cameras have many properties that won't be configurable in Kinovea 
            // so the user is more likely to configure the camera from the outside.
            if (grabbing)
                Stop();

            try
            {
                imageProvider.Open(deviceIndex);
                device = imageProvider.Device;
                nodeMap = imageProvider.NodeMap;
                // Load parameter set.
                ProfileHelper.Load(nodeMap, summary.Identifier);
            }
            catch (Exception e)
            {
                log.Error("Could not open IDS device.");
                LogError(e, imageProvider.GetLastErrorMessage());
                return;
            }

            if (device == null || nodeMap == null)
                return;
            
            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null)
                return;
            // Store the handle into the specific info so that we can retrieve device informations from the configuration dialog.
            specific.NodeMap = nodeMap;

            int currentColorMode = IDSHelper.ReadCurrentStreamFormat(nodeMap);
            if (specific.StreamFormat != currentColorMode)
                IDSHelper.WriteStreamFormat(nodeMap, specific.StreamFormat);
            // Some properties can only be changed when the camera is opened but not streaming. Now is the time.
            // We store them in the summary when coming back from FormConfiguration, and we write them to the camera here.
            // Only do this if it's not the first time we open the camera, to respect any change that could have been done outside Kinovea.
            if (firstOpen)
            {
                // Restore camera parameters from the XML blurb.
                // Regular properties, including image size.
                // First we read the current properties from the API to get fully formed properties.
                // We merge the values saved in the XML into the properties.
                // (The restoration from the XML doesn't create fully formed properties, it just contains the values).
                // Then commit the properties to the camera.
                Dictionary<string, CameraProperty> cameraProperties = CameraPropertyManager.Read(nodeMap);
                CameraPropertyManager.MergeProperties(cameraProperties, specific.CameraProperties);
                specific.CameraProperties = cameraProperties;
                CameraPropertyManager.WriteCriticalProperties(nodeMap, specific.CameraProperties);
            }
            else
            {
                CameraPropertyManager.WriteCriticalProperties(nodeMap, specific.CameraProperties);
            }
            
        }

        private void ComputeDataRate(int bytes)
        {
            double rate = ((double)bytes / megabyte) / swDataRate.Elapsed.TotalSeconds;
            dataRateAverager.Post(rate);
            swDataRate.Reset();
            swDataRate.Start();
        }
        #endregion

        #region device event handlers
        private void imageProvider_GrabbingStartedEvent()
        {
            grabbing = true;

            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        private void imageProvider_ImageReadyEvent()
        {
            // Consume the Pylon queue (no copy).
            ImageProvider.Image pylonImage = imageProvider.GetLatestImage();
            if (pylonImage == null)
                return;
            
            if (finishline.Enabled)
            {
                bool flush = finishline.Consolidate(pylonImage.Buffer);
                imageProvider.ReleaseImage();

                if (flush)
                {
                    ComputeDataRate(finishline.BufferOutput.Length);

                    if (FrameProduced != null)
                        FrameProduced(this, new FrameProducedEventArgs(finishline.BufferOutput, finishline.BufferOutput.Length));
                }
            }
            else
            {
                ComputeDataRate(pylonImage.Buffer.Length);

                if (FrameProduced != null)
                    FrameProduced(this, new FrameProducedEventArgs(pylonImage.Buffer, pylonImage.Buffer.Length));

                imageProvider.ReleaseImage();
            }
        }

        private void imageProvider_GrabErrorEvent(Exception grabException, string additionalErrorMessage)
        {
            LogError(grabException, additionalErrorMessage);
        }

        private void imageProvider_DeviceRemovedEvent()
        {
            
        }

        private void LogError(Exception e, string additionalErrorMessage)
        {
            log.ErrorFormat("Error during Basler camera operation. {0}", summary.Alias);
            log.Error(e.ToString());
            log.Error(additionalErrorMessage);
        }
        #endregion

    }
}
