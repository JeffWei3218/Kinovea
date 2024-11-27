using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using Kinovea.Services;
using System.Drawing.Imaging;
using System.Diagnostics;
using peak;
using peak.core;
using peak.core.nodes;

namespace Kinovea.Camera.IDSpeak
{
    /// <summary>
    /// Class to discover and manage IDS cameras (IDSpeak API).
    /// </summary>
    public class CameraManagerIDSpeak : CameraManager
    {
        #region Properties
        public override bool Enabled
        {
            get { return true; }
        }
        public override string CameraType
        {
            get { return "B81EF110-D8BC-462B-9716-5CECD8B745F6"; }
        }
        public override string CameraTypeFriendlyName
        {
            get { return "IDS uEye+"; }
        }
        public override bool HasConnectionWizard
        {
            get { return false; }
        }
        #endregion

        #region Members
        private List<SnapshotRetriever> snapshotting = new List<SnapshotRetriever>();
        private Dictionary<string, CameraSummary> cache = new Dictionary<string, CameraSummary>();
        private Bitmap defaultIcon;
        private int discoveryStep = 0;
        private int discoverySkip = 5;
        private Dictionary<string, uint> deviceIds = new Dictionary<string, uint>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public CameraManagerIDSpeak()
        {
            defaultIcon = IconLibrary.GetIcon("ids");
            // initialize library
            Library.Initialize();
        }

        ~CameraManagerIDSpeak()
        {
            // close library before exiting program
            Library.Close();
        }
        public override bool SanityCheck()
        {
            bool result = false;
            try
            {
                // IDS peak version
                peak.core.Version peakVersion = Library.Version();
                log.DebugFormat("IDS peak uEye+ .NET wrapper version: {0}.", peakVersion.ToString());
                result = true;
            }
            catch
            {
                log.DebugFormat("IDS uEye+ Camera subsystem not available.");
            }

            return result;
        }

        public override List<CameraSummary> DiscoverCameras(IEnumerable<CameraBlurb> blurbs)
        {
            List<CameraSummary> summaries = new List<CameraSummary>();
            // We don't do the discover step every time to avoid UI freeze since Enumerate takes some time.
            if (discoveryStep > 0)
            {
                discoveryStep = (discoveryStep + 1) % discoverySkip;
                foreach (CameraSummary summary in cache.Values)
                    summaries.Add(summary);

                return summaries;
            }

            discoveryStep = 1;
            List<CameraSummary> found = new List<CameraSummary>();
            List<DeviceEnumerator.Device> devices = DeviceEnumerator.Instance().EnumerateDevices();
            foreach (DeviceEnumerator.Device device in devices)
            {
                string identifier = device.Name;
                bool cached = cache.ContainsKey(identifier);

                if (cached)
                {
                    deviceIds[identifier] = device.Index;
                    summaries.Add(cache[identifier]);
                    found.Add(cache[identifier]);
                    continue;
                }

                string alias = device.ModelName;
                Bitmap icon = null;
                SpecificInfo specific = new SpecificInfo();
                Rectangle displayRectangle = Rectangle.Empty;
                CaptureAspectRatio aspectRatio = CaptureAspectRatio.Auto;
                ImageRotation rotation = ImageRotation.Rotate0;
                bool mirror = false;
                deviceIds[identifier] = device.Index;

                if (blurbs != null)
                {
                    foreach (CameraBlurb blurb in blurbs)
                    {
                        if (blurb.CameraType != this.CameraType || blurb.Identifier != identifier)
                            continue;

                        // We already know this camera, restore the user custom values.
                        alias = blurb.Alias;
                        icon = blurb.Icon ?? defaultIcon;
                        displayRectangle = blurb.DisplayRectangle;
                        if (!string.IsNullOrEmpty(blurb.AspectRatio))
                            aspectRatio = (CaptureAspectRatio)Enum.Parse(typeof(CaptureAspectRatio), blurb.AspectRatio);
                        if (!string.IsNullOrEmpty(blurb.Rotation))
                            rotation = (ImageRotation)Enum.Parse(typeof(ImageRotation), blurb.Rotation);
                        mirror = blurb.Mirror;
                        specific = SpecificInfoDeserialize(blurb.Specific);
                        break;
                    }
                }

                icon = icon ?? defaultIcon;

                CameraSummary summary = new CameraSummary(alias, device.ModelName, identifier, icon, displayRectangle, aspectRatio, rotation, mirror, specific, this);

                summaries.Add(summary);
                found.Add(summary);
                cache.Add(identifier, summary);

                log.DebugFormat("IDS uEye device enumeration: {0} (id:{1}).", summary.Alias, identifier);

            }
            List<CameraSummary> lost = new List<CameraSummary>();
            foreach (CameraSummary summary in cache.Values)
            {
                if (!found.Contains(summary))
                    lost.Add(summary);
            }

            foreach (CameraSummary summary in lost)
                cache.Remove(summary.Identifier);

            return summaries;
        }

        public override void ForgetCamera(CameraSummary summary)
        {
            if (cache.ContainsKey(summary.Identifier))
                cache.Remove(summary.Identifier);

            ProfileHelper.Delete(summary.Identifier);
        }

        public override CameraSummary GetCameraSummary(string alias)
        {
            return cache.Values.FirstOrDefault(s => s.Alias == alias);
        }

        public override void StartThumbnail(CameraSummary summary)
        {
            SnapshotRetriever snapper = snapshotting.FirstOrDefault(s => s.Identifier == summary.Identifier);
            if (snapper != null)
                return;

            snapper = new SnapshotRetriever(summary, deviceIds[summary.Identifier]);
            snapper.CameraThumbnailProduced += SnapshotRetriever_CameraThumbnailProduced;
            snapshotting.Add(snapper);
            snapper.Start();
        }

        public override void StopAllThumbnails()
        {
            for (int i = snapshotting.Count - 1; i >= 0; i--)
            {
                SnapshotRetriever snapper = snapshotting[i];
                snapper.Cancel();
                snapper.Thread.Join(500);
                if (snapper.Thread.IsAlive)
                    snapper.Thread.Abort();

                snapper.CameraThumbnailProduced -= SnapshotRetriever_CameraThumbnailProduced;
                snapshotting.RemoveAt(i);
            }
        }

        public override CameraBlurb BlurbFromSummary(CameraSummary summary)
        {
            string specific = SpecificInfoSerialize(summary);
            CameraBlurb blurb = new CameraBlurb(CameraType, summary.Identifier, summary.Alias, summary.Icon, summary.DisplayRectangle, summary.AspectRatio.ToString(), summary.Rotation.ToString(), summary.Mirror, specific);
            return blurb;
        }

        public override ICaptureSource CreateCaptureSource(CameraSummary summary)
        {
            FrameGrabber grabber = new FrameGrabber(summary, deviceIds[summary.Identifier]);
            return grabber;
        }

        public override bool Configure(CameraSummary summary)
        {
            throw new NotImplementedException();
        }

        public override bool Configure(CameraSummary summary, Action disconnect, Action connect)
        {
            bool needsReconnection = false;
            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info == null)
                return false;

            FormConfiguration form = new FormConfiguration(summary, disconnect, connect);
            FormsHelper.Locate(form);
            if (form.ShowDialog() == DialogResult.OK)
            {
                if (form.AliasChanged)
                    summary.UpdateAlias(form.Alias, form.PickedIcon);

                if (form.SpecificChanged)
                {
                    info.StreamFormat = form.SelectedStreamFormat.Value;
                    info.GammaCorrectionValue = form.GammaCorrectionValue;
                    info.ImageTransformerConstants = form.ImageTransformerConstants;
                    info.CameraProperties = form.CameraProperties;

                    summary.UpdateDisplayRectangle(Rectangle.Empty);
                    needsReconnection = true;
                }

                CameraTypeManager.UpdatedCameraSummary(summary);
            }

            form.Dispose();
            return needsReconnection;
        }

        public override string GetSummaryAsText(CameraSummary summary)
        {
            string result = "";
            string alias = summary.Alias;

            SpecificInfo info = summary.Specific as SpecificInfo;

            try
            {
                if (info != null &&
                    info.CameraProperties.ContainsKey("width") &&
                    info.CameraProperties.ContainsKey("height") &&
                    info.CameraProperties.ContainsKey("framerate"))
                {
                    int format = info.StreamFormat;
                    int width = int.Parse(info.CameraProperties["width"].CurrentValue, CultureInfo.InvariantCulture);
                    int height = int.Parse(info.CameraProperties["height"].CurrentValue, CultureInfo.InvariantCulture);
                    double framerate = double.Parse(info.CameraProperties["framerate"].CurrentValue, CultureInfo.InvariantCulture);

                    peak.ipl.PixelFormatName colorMode = (peak.ipl.PixelFormatName)format;

                    result = string.Format("{0} - {1}×{2} @ {3:0.##} fps ({4}).", alias, width, height, framerate, colorMode.ToString());
                }
                else
                {
                    result = string.Format("{0}", alias);
                }
            }
            catch
            {
                result = string.Format("{0}", alias);
            }

            return result;
        }

        public override Control GetConnectionWizard()
        {
            throw new NotImplementedException();
        }

        private void SnapshotRetriever_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            Invoke((Action)delegate { ProcessThumbnail(sender, e); });
        }

        private void ProcessThumbnail(object sender, CameraThumbnailProducedEventArgs e)
        {
            SnapshotRetriever snapper = sender as SnapshotRetriever;
            if (snapper == null)
                return;

            log.DebugFormat("Received thumbnail event for {0}. Cancelled: {1}.", snapper.Alias, e.Cancelled);
            snapper.CameraThumbnailProduced -= SnapshotRetriever_CameraThumbnailProduced;
            if (snapshotting.Contains(snapper))
                snapshotting.Remove(snapper);

            OnCameraThumbnailProduced(e);
        }

        private SpecificInfo SpecificInfoDeserialize(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return null;

            SpecificInfo info = null;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(new StringReader(xml));
                info = new SpecificInfo();

                string streamFormat = "";
                XmlNode xmlStreamFormat = doc.SelectSingleNode("/IDS/StreamFormat");
                if (xmlStreamFormat != null)
                    streamFormat = xmlStreamFormat.InnerText;

                float gammaCorrectorValue = 1.0f;
                XmlNode xmlGammaCorrectionValue = doc.SelectSingleNode("/IDS/GammaCorrectionValue");
                if (xmlGammaCorrectionValue != null)
                    gammaCorrectorValue = float.Parse(xmlGammaCorrectionValue.InnerText);

                ImageTransformerConstants imageTransformerValue = ImageTransformerConstants.None;
                XmlNode xmlImageTransformerValue = doc.SelectSingleNode("/IDS/ImageTransformerValue");
                if (xmlImageTransformerValue != null)
                    imageTransformerValue = (ImageTransformerConstants)Enum.Parse(typeof(ImageTransformerConstants), xmlImageTransformerValue.InnerText);

                Dictionary<string, CameraProperty> cameraProperties = new Dictionary<string, CameraProperty>();

                XmlNodeList props = doc.SelectNodes("/IDS/CameraProperties/CameraProperty");
                foreach (XmlNode node in props)
                {
                    XmlAttribute keyAttribute = node.Attributes["key"];
                    if (keyAttribute == null)
                        continue;

                    string key = keyAttribute.Value;
                    CameraProperty property = new CameraProperty();

                    string xpath = string.Format("/IDS/CameraProperties/CameraProperty[@key='{0}']", key);
                    XmlNode xmlPropertyValue = doc.SelectSingleNode(xpath + "/Value");
                    if (xmlPropertyValue != null)
                        property.CurrentValue = xmlPropertyValue.InnerText;
                    else
                        property.Supported = false;

                    XmlNode xmlPropertyAuto = doc.SelectSingleNode(xpath + "/Auto");
                    if (xmlPropertyAuto != null)
                        property.Automatic = XmlHelper.ParseBoolean(xmlPropertyAuto.InnerText);
                    else
                        property.Supported = false;

                    cameraProperties.Add(key, property);
                }

                info.StreamFormat = int.Parse(streamFormat);
                info.GammaCorrectionValue = gammaCorrectorValue;
                info.ImageTransformerConstants = imageTransformerValue;
                info.CameraProperties = cameraProperties;
            }
            catch (Exception e)
            {
                log.ErrorFormat(e.Message);
            }

            return info;
        }

        private string SpecificInfoSerialize(CameraSummary summary)
        {
            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info == null)
                return null;

            XmlDocument doc = new XmlDocument();
            XmlElement xmlRoot = doc.CreateElement("IDS");
            XmlElement xmlStreamFormat = doc.CreateElement("StreamFormat");
            xmlStreamFormat.InnerText = info.StreamFormat.ToString();
            xmlRoot.AppendChild(xmlStreamFormat);

            XmlElement xmlGammaCorrectionValue = doc.CreateElement("GammaCorrectionValue");
            xmlGammaCorrectionValue.InnerText = info.GammaCorrectionValue.ToString();
            xmlRoot.AppendChild(xmlGammaCorrectionValue);

            XmlElement xmlImageTransformerValue = doc.CreateElement("ImageTransformerValue");
            xmlImageTransformerValue.InnerText = info.ImageTransformerConstants.ToString();
            xmlRoot.AppendChild(xmlImageTransformerValue);

            XmlElement xmlCameraProperties = doc.CreateElement("CameraProperties");

            foreach (KeyValuePair<string, CameraProperty> pair in info.CameraProperties)
            {
                XmlElement xmlCameraProperty = doc.CreateElement("CameraProperty");
                XmlAttribute attr = doc.CreateAttribute("key");
                attr.Value = pair.Key;
                xmlCameraProperty.Attributes.Append(attr);

                XmlElement xmlCameraPropertyValue = doc.CreateElement("Value");
                xmlCameraPropertyValue.InnerText = pair.Value.CurrentValue;
                xmlCameraProperty.AppendChild(xmlCameraPropertyValue);

                XmlElement xmlCameraPropertyAuto = doc.CreateElement("Auto");
                xmlCameraPropertyAuto.InnerText = pair.Value.Automatic.ToString().ToLower();
                xmlCameraProperty.AppendChild(xmlCameraPropertyAuto);

                xmlCameraProperties.AppendChild(xmlCameraProperty);
            }

            xmlRoot.AppendChild(xmlCameraProperties);
            doc.AppendChild(xmlRoot);

            return doc.OuterXml;
        }
    }
}