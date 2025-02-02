﻿#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Windows.Forms;
using System.Globalization;
using Kinovea.Camera;
using Kinovea.Services;
using Kinovea.Camera.Languages;
using BGAPI2;
using System.IO;

namespace Kinovea.Camera.GenICam
{
    public partial class FormConfiguration : Form
    {
        #region Properties
        public bool AliasChanged
        {
            get { return iconChanged || tbAlias.Text != summary.Alias;}
        }
        
        public string Alias
        { 
            get { return tbAlias.Text; }
        }
        
        public Bitmap PickedIcon
        { 
            get { return (Bitmap)btnIcon.BackgroundImage; }
        }
        
        public bool SpecificChanged
        {
            get { return specificChanged; }
        }

        public string SelectedStreamFormat
        {
            get { return selectedStreamFormat; }
        }

        public bool Demosaicing
        {
            get { return demosaicing; }
        }

        public bool Compression
        {
            get { return compression; }
        }
                
        public Dictionary<string, CameraProperty> CameraProperties
        {
            get { return cameraProperties; }
        }
        #endregion

        #region Members
        private CameraSummary summary;
        private bool specificChanged;
        private bool iconChanged;
        private Device device;
        private Dictionary<string, CameraProperty> cameraProperties = new Dictionary<string, CameraProperty>();
        private Dictionary<string, AbstractCameraPropertyView> propertiesControls = new Dictionary<string, AbstractCameraPropertyView>();
        private Action disconnect;
        private Action connect;

        // Format and Debayering/Compression
        private string selectedStreamFormat;
        private bool demosaicing;
        private bool compression;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FormConfiguration(CameraSummary summary, Action disconnect, Action connect)
        {
            this.summary = summary;
            this.disconnect = disconnect;
            this.connect = connect;

            InitializeComponent();
            Localize();

            tbAlias.AutoSize = false;
            tbAlias.Height = 20;
            tbAlias.Text = summary.Alias;
            lblSystemName.Text = summary.Name;
            btnIcon.BackgroundImage = summary.Icon;

            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null || specific.Device == null || specific.Device.Id == null || !specific.Device.IsOpen)
                return;

            device = specific.Device;
            cameraProperties = CameraPropertyManager.ReadAll(device, summary.Identifier);
            if (cameraProperties.Count != specific.CameraProperties.Count)
                specificChanged = true;

            demosaicing = specific.Demosaicing;
            compression = specific.Compression;
            
            Populate();
            UpdateResultingFramerate();
        }

        private void Localize()
        {
            this.Text = CameraLang.FormConfiguration_Title;
            lblStreamFormat.Text = CameraLang.FormConfiguration_Properties_StreamFormat;
            cbCompression.Text = CameraLang.FormConfiguration_EnableHardwareCompression;
            cbDebayering.Text = CameraLang.FormConfiguration_EnableSoftwareDemosaicing;
            lblAuto.Text = CameraLang.FormConfiguration_Auto;
            lblResultingFramerate.Text = CameraLang.FormConfiguration_ResultingFramerate;
            btnGenicam.Text = "GenICam XML";
            btnReconnect.Text = CameraLang.FormConfiguration_Reconnect;
            btnApply.Text = CameraLang.Generic_Apply;
        }
        
        private void Populate()
        {
            try
            {
                PopulateStreamFormat();
                PopulateFormatExtraOptions();
                PopulateCameraControls();
            }
            catch
            {
                log.ErrorFormat("Error while populating configuration options.");
            }
        }
        
        private void BtnIcon_OnClick(object sender, EventArgs e)
        {
            FormIconPicker fip = new FormIconPicker(IconLibrary.Icons, 5);
            FormsHelper.Locate(fip);
            if(fip.ShowDialog() == DialogResult.OK)
            {
                btnIcon.BackgroundImage = fip.PickedIcon;
                iconChanged = true;
            }
            
            fip.Dispose();
        }

        private void PopulateStreamFormat()
        {
            bool readable = CameraPropertyManager.NodeIsReadable(device, "PixelFormat");
            if (!readable)
            {
                cmbFormat.Enabled = false;
                return;
            }

            List<string> streamFormats = new List<string>();
            NodeMap mapFormats = device.RemoteNodeList["PixelFormat"].EnumNodeList;
            for (ulong i = 0; i < mapFormats.Count; i++)
            {
                var node = mapFormats[i];
                if (node.IsReadable)
                    streamFormats.Add(node.Value);
            }

            if (streamFormats.Count == 0)
            {
                cmbFormat.Enabled = false;
                return;
            }

            // Sort correctly so that for example "Mono8" appears before "Mono10".
            // The selection is based on the string itself, not its index in the list.
            streamFormats.Sort(new AlphanumComparator());

            string currentValue = CameraPropertyManager.ReadString(device, "PixelFormat");
            cmbFormat.Items.Clear();
            foreach (var streamFormat in streamFormats)
            {
                cmbFormat.Items.Add(streamFormat);
                if (currentValue == streamFormat)
                {
                    selectedStreamFormat = streamFormat;
                    cmbFormat.SelectedIndex = cmbFormat.Items.Count - 1;
                }
            }
        }

        /// <summary>
        /// Setup the extra options related to debayering and compression.
        /// </summary>
        private void PopulateFormatExtraOptions()
        {
            // Enable/Disable
            cbDebayering.Enabled = CameraPropertyManager.IsBayer(selectedStreamFormat);
            bool supportsJPEG = CameraPropertyManager.SupportsJPEG(device);
            bool canCompress = CameraPropertyManager.FormatCanCompress(device, selectedStreamFormat);
            cbCompression.Enabled = supportsJPEG && canCompress;

            // Current values.
            cbDebayering.Checked = demosaicing;
            cbCompression.Checked = compression;
        }

        private void PopulateCameraControls()
        {
            int top = lblAuto.Bottom;
            AddCameraProperty("width", CameraLang.FormConfiguration_Properties_ImageWidth, top);
            AddCameraProperty("height", CameraLang.FormConfiguration_Properties_ImageHeight, top + 30);
            AddCameraProperty("framerate", CameraLang.FormConfiguration_Properties_Framerate, top + 60);
            AddCameraProperty("exposure", CameraLang.FormConfiguration_Properties_ExposureMicro, top + 90);
            AddCameraProperty("gain", CameraLang.FormConfiguration_Properties_Gain, top + 120);
            AddCameraProperty("compressionQuality", "Compression quality:", top + 150);
        }

        private void RemoveCameraControls()
        {
            foreach (var pair in propertiesControls)
            {
                pair.Value.ValueChanged -= cpvCameraControl_ValueChanged;
                gbProperties.Controls.Remove(pair.Value);
            }

            propertiesControls.Clear();
        }

        private void AddCameraProperty(string key, string text, int top)
        {
            if (!cameraProperties.ContainsKey(key))
                return;

            CameraProperty property = cameraProperties[key];

            AbstractCameraPropertyView control = null;
             
            switch (property.Representation)
            {
                case CameraPropertyRepresentation.LinearSlider:
                    control = new CameraPropertyViewLinear(property, text, null);
                    break;
                case CameraPropertyRepresentation.LogarithmicSlider:
                    control = new CameraPropertyViewLogarithmic(property, text, null);
                    break;
                case CameraPropertyRepresentation.Checkbox:
                    control = new CameraPropertyViewCheckbox(property, text);
                    break;

                default:
                    break;
            }

            if (control == null)
                return;

            control.Tag = key;
            control.ValueChanged += cpvCameraControl_ValueChanged;
            control.Left = 20;
            control.Top = top;
            gbProperties.Controls.Add(control);
            propertiesControls.Add(key, control);
        }

        private void cpvCameraControl_ValueChanged(object sender, EventArgs e)
        {
            AbstractCameraPropertyView control = sender as AbstractCameraPropertyView;
            if (control == null)
                return;

            string key = control.Tag as string;
            if (string.IsNullOrEmpty(key) || !cameraProperties.ContainsKey(key))
                return;

            CameraPropertyManager.Write(device, cameraProperties[key]);
            UpdateResultingFramerate();
            specificChanged = true;
        }
        
        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = cmbFormat.SelectedItem as string;
            if (selected == null)
                return;

            selectedStreamFormat = selected;
            specificChanged = true;
            UpdateResultingFramerate();
            PopulateFormatExtraOptions();
        }

        private void cbDebayering_CheckedChanged(object sender, EventArgs e)
        {
            // Handler specific to Baumer cameras.

            demosaicing = cbDebayering.Checked;
            specificChanged = true;
            UpdateResultingFramerate();
        }

        private void cbCompression_CheckedChanged(object sender, EventArgs e)
        {
            // Handler specific to Baumer cameras.

            compression = cbCompression.Checked;
            specificChanged = true;
            UpdateResultingFramerate();
        }

        private void BtnReconnect_Click(object sender, EventArgs e)
        {
            if (SelectedStreamFormat == null)
            {
                // This happens when we load the config window and the camera isn't connected.
                return;
            }

            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info == null)
                return;

            info.StreamFormat = selectedStreamFormat;
            info.Demosaicing = demosaicing;
            info.Compression = compression;
            info.CameraProperties = cameraProperties;

            summary.UpdateDisplayRectangle(Rectangle.Empty);
            CameraTypeManager.UpdatedCameraSummary(summary);

            disconnect();
            connect();

            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null || specific.Device == null || !specific.Device.IsOpen)
                return;

            device = specific.Device;
            cameraProperties = CameraPropertyManager.ReadAll(device, summary.Identifier);

            RemoveCameraControls();
            
            PopulateStreamFormat();
            PopulateFormatExtraOptions();
            PopulateCameraControls();
            
            UpdateResultingFramerate();
        }

        private void UpdateResultingFramerate()
        {
            float resultingFramerate = CameraPropertyManager.GetResultingFramerate(device);
            lblResultingFramerateValue.Text = string.Format("{0:0.##}", resultingFramerate);

            bool discrepancy = false;
            if (cameraProperties.ContainsKey("framerate") && cameraProperties["framerate"].Supported)
            {
                float framerate;
                bool parsed = float.TryParse(cameraProperties["framerate"].CurrentValue, NumberStyles.Any, CultureInfo.InvariantCulture, out framerate);
                if (parsed && Math.Abs(framerate - resultingFramerate) > 1)
                    discrepancy = true;
            }

            lblResultingFramerateValue.ForeColor = discrepancy ? Color.Red : Color.Black;
        }


        /// <summary>
        /// Get the GenICam XML file on the device and save it somewhere.
        /// </summary>
        private void btnGenicam_Click(object sender, EventArgs e)
        {
            if (device == null || !device.IsOpen)
                return;

            string xml = device.RemoteConfigurationFile;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = CameraLang.FormConfiguration_ExportDeviceGenICamXMLFile;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = summary.Name;
            saveFileDialog.Filter = FilesystemHelper.SaveXMLFilter();
            saveFileDialog.FilterIndex = 1;

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            File.WriteAllText(saveFileDialog.FileName, xml);
            log.DebugFormat("GenICam XML file exported to {0}", saveFileDialog.FileName);
        }
    }
}
