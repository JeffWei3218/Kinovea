﻿#region License
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
using System.Drawing;
using System.Threading;
using Kinovea.Pipeline;

using peak;
using peak.core;
using peak.core.nodes;
using Kinovea.Services;
using System.Diagnostics;

namespace Kinovea.Camera.IDSpeak
{
    /// <summary>
    /// Retrieve a single snapshot, simulating a synchronous function. Used for thumbnails.
    /// We use whatever settings are currently configured in the camera.
    /// </summary>
    public class SnapshotRetriever
    {
        public event EventHandler<CameraThumbnailProducedEventArgs> CameraThumbnailProduced;

        public string Identifier
        {
            get { return this.summary.Identifier; }
        }

        public string Alias
        {
            get { return summary.Alias; }
        }

        public Thread Thread
        {
            get { return snapperThread; }
        }

        #region Members
        private static readonly int timeoutGrabbing = 5000;
        private static readonly int timeoutOpening = 500;

        private Bitmap image;
        private ImageDescriptor imageDescriptor = ImageDescriptor.Invalid;
        private CameraSummary summary;
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        private ImageProvider imageProvider = new ImageProvider();
        private bool cancelled;
        private bool hadError;
        private Thread snapperThread;
        private object locker = new object();
        private Stopwatch stopwatch = new Stopwatch();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public SnapshotRetriever(CameraSummary summary, uint deviceIndex)
        {
            this.summary = summary;

            imageProvider.GrabErrorEvent += imageProvider_GrabErrorEvent;
            imageProvider.DeviceRemovedEvent += imageProvider_DeviceRemovedEvent;
            imageProvider.ImageReadyEvent += imageProvider_ImageReadyEvent;
            
            try
            {
                stopwatch.Start();
                imageProvider.Open(deviceIndex);
                log.DebugFormat("{0} opened in {1} ms.", summary.Alias, stopwatch.ElapsedMilliseconds);
                stopwatch.Stop();
            }
            catch (Exception e) 
            {
                LogError(e, imageProvider.GetLastErrorMessage());
            }
        }

        public void Start()
        {
            snapperThread = new Thread(Run) { IsBackground = true };
            snapperThread.Name = string.Format("{0} thumbnailer", summary.Alias);
            snapperThread.Start();
        }

        /// <summary>
        /// Start the device for a frame grab, wait a bit and then return the result.
        /// This method MUST raise a CameraThumbnailProduced event, even in case of error.
        /// </summary>
        public void Run(object data)
        {
            log.DebugFormat("Starting {0} for thumbnail.", summary.Alias);

            if (!imageProvider.IsOpen)
                Thread.Sleep(timeoutOpening);

            if (!imageProvider.IsOpen)
            {
                if (CameraThumbnailProduced != null)
                    CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, null, imageDescriptor, true, false));

                return;
            }

            try
            {
                imageProvider.OneShot(); 
            }
            catch (Exception e)
            {
                LogError(e, imageProvider.GetLastErrorMessage());
            }

            waitHandle.WaitOne(timeoutGrabbing, false);

            lock (locker)
            {
                if (!cancelled)
                {
                    imageProvider.GrabErrorEvent -= imageProvider_GrabErrorEvent;
                    imageProvider.DeviceRemovedEvent -= imageProvider_DeviceRemovedEvent;
                    imageProvider.ImageReadyEvent -= imageProvider_ImageReadyEvent;

                    Stop();
                    Close();
                }
            }

            if (CameraThumbnailProduced != null)
                CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, image, imageDescriptor, hadError, cancelled));
        }

        public void Cancel()
        {
            log.DebugFormat("Cancelling thumbnail for {0}.", Alias);

            if (!imageProvider.IsOpen)
                return;

            lock (locker)
            {
                imageProvider.GrabErrorEvent -= imageProvider_GrabErrorEvent;
                imageProvider.DeviceRemovedEvent -= imageProvider_DeviceRemovedEvent;
                imageProvider.ImageReadyEvent -= imageProvider_ImageReadyEvent;
                Stop();
                Close();

                cancelled = true;
            }
            
            waitHandle.Set();
        }

        private void Stop()
        {
            try
            {
                imageProvider.Stop();
            }
            catch (Exception e)
            {
                LogError(e, imageProvider.GetLastErrorMessage());
            }
        }

        private void Close()
        {
            try
            {
                imageProvider.Close();
            }
            catch (Exception e)
            {
                LogError(e, imageProvider.GetLastErrorMessage());
            }
        }

        private void LogError(Exception e, string additionalErrorMessage)
        {
            log.ErrorFormat("Camera {0} failure during thumbnail capture.", summary.Alias);
            log.Error(e.ToString());

            if (!string.IsNullOrEmpty(additionalErrorMessage))
                log.Error(additionalErrorMessage);
        }

        #region Camera events
        private void imageProvider_GrabErrorEvent(Exception grabException, string additionalErrorMessage)
        {
            LogError(grabException, additionalErrorMessage);
            
            hadError = true;
            waitHandle.Set();
        }

        private void imageProvider_DeviceRemovedEvent()
        {            
            hadError = true;
            waitHandle.Set();
        }

        private void imageProvider_ImageReadyEvent()
        {
            ImageProvider.Image idsImage = imageProvider.GetLatestImage();

            if (idsImage == null)
            {
                waitHandle.Set();
                return;
            }

            image = null;
            BitmapFactory.CreateBitmap(out image, idsImage.Width, idsImage.Height, idsImage.Color);
            BitmapFactory.UpdateBitmap(image, idsImage.Buffer, idsImage.Width, idsImage.Height, idsImage.Color);
            imageProvider.ReleaseImage();

            if (image != null)
            {
                int bufferSize = ImageFormatHelper.ComputeBufferSize(image.Width, image.Height, Kinovea.Services.ImageFormat.RGB24);
                imageDescriptor = new ImageDescriptor(Kinovea.Services.ImageFormat.RGB24, image.Width, image.Height, true, bufferSize);
            }

            waitHandle.Set();
        }
        #endregion


    }
}

