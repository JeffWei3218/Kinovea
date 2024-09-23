#region License
/*
Copyright © Joan Charmant 2017.
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
using Kinovea.Camera;
using Kinovea.Services;
using Kinovea.Camera.Languages;
using System.IO;
using peak.core;
using System.Globalization;

namespace Kinovea.Camera.IDSpeak
{
    public partial class FormConfiguration : Form
    {
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

        public IDSEnum SelectedStreamFormat
        {
            get { return selectedStreamFormat; }
        }

        public Dictionary<string, CameraProperty> CameraProperties
        {
            get { return cameraProperties; }
        }

        public float GammaCorrectionValue
        {
            get { return gammaCorrectionValue; }
        }

        private CameraSummary summary;
        private bool iconChanged;
        private bool specificChanged;
        private NodeMap nodeMap;
        private IDSEnum selectedStreamFormat;
        private float gammaCorrectionValue;
        private Dictionary<string, CameraProperty> cameraProperties = new Dictionary<string, CameraProperty>();
        private Dictionary<string, AbstractCameraPropertyView> propertiesControls = new Dictionary<string, AbstractCameraPropertyView>();
        private Action disconnect;
        private Action connect;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FormConfiguration(CameraSummary summary, Action disconnect, Action connect)
        {
            this.summary = summary;
            this.disconnect = disconnect;
            this.connect = connect;

            InitializeComponent();
            tbAlias.AutoSize = false;
            tbAlias.Height = 20;
            
            tbAlias.Text = summary.Alias;
            lblSystemName.Text = summary.Name;
            btnIcon.BackgroundImage = summary.Icon;
            btnReconnect.Text = CameraLang.FormConfiguration_Reconnect;
            btnImport.Text = CameraLang.FormConfiguration_ImportParameters;
            btnImport.Visible = false;
            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null || specific.NodeMap == null)
                return;
            nodeMap = specific.NodeMap;
            cameraProperties = CameraPropertyManager.Read(nodeMap);

            gammaCorrectionValue = specific.GammaCorrectionValue;

            Populate();
            this.Text = CameraLang.FormConfiguration_Title;
            btnApply.Text = CameraLang.Generic_Apply;
            btnApply.Visible = false;
        }

        private void Populate()
        {
            try
            {
                PopulateStreamFormat();
                PopulateGammaCorrector();
                PopulateCameraControls();
            }
            catch (Exception e)
            {
                log.Error("Error while populating configuration options", e);
            }
        }
        
        private void BtnIconClick(object sender, EventArgs e)
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
            lblColorSpace.Text = CameraLang.FormConfiguration_Properties_StreamFormat;

            // Get the intersection of camera and Kinovea supported formats.
            List<IDSEnum> streamFormats = IDSHelper.GetSupportedStreamFormats(nodeMap);

            // Get currently selected option.
            int currentColorMode = IDSHelper.ReadCurrentStreamFormat(nodeMap);
            cmbFormat.Items.Clear();

            foreach (IDSEnum streamFormat in streamFormats)
            {
                cmbFormat.Items.Add(streamFormat);
                if (streamFormat.Value == currentColorMode)
                {
                    selectedStreamFormat = streamFormat;
                    cmbFormat.SelectedIndex = cmbFormat.Items.Count - 1;
                }
            }

            // TODO: if the current camera format is not supported in Kinovea, force the camera to switch to a supported mode.
            // What if none of the Kinovea modes are supported by the camera ?
        }

        private void PopulateCameraControls()
        {
            int top = lblAuto.Bottom;
            AddCameraProperty("offsetX", CameraLang.FormConfiguration_Properties_OffsetX, top);
            AddCameraProperty("offsetY", CameraLang.FormConfiguration_Properties_OffsetY, top + 30);
            AddCameraProperty("width", CameraLang.FormConfiguration_Properties_ImageWidth, top + 60);
            AddCameraProperty("height", CameraLang.FormConfiguration_Properties_ImageHeight, top + 90);
            AddCameraProperty("framerate", CameraLang.FormConfiguration_Properties_Framerate, top + 120);
            AddCameraProperty("exposure", CameraLang.FormConfiguration_Properties_ExposureMicro, top + 150);
            AddCameraProperty("gain", CameraLang.FormConfiguration_Properties_Gain, top + 180);
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
                case CameraPropertyRepresentation.FloatSlider:
                    control = new CameraPropertyViewFloatSlider(property, text, null);
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

        private void PopulateGammaCorrector()
        {
            CameraProperty property = new CameraProperty();
            property.Identifier = "gamma";
            property.Supported = true;
            property.ReadOnly = false;
            property.Type = CameraPropertyType.Float;
            property.Step ="0.01";
            property.CurrentValue = gammaCorrectionValue.ToString(CultureInfo.InvariantCulture);
            property.Minimum = "0.3";
            property.Maximum = "3.0";
            AbstractCameraPropertyView control = new CameraPropertyViewFloatSlider(property, CameraLang.FormConfiguration_Properties_Gamma, null);

            control.Tag = "Gamma";
            control.ValueChanged += cpvGammaControl_ValueChanged;
            control.Left = 20;
            control.Top = lblAuto.Bottom + 210;
            gbProperties.Controls.Add(control);
        }

        private void cpvGammaControl_ValueChanged(object sender, EventArgs e)
        {
            AbstractCameraPropertyView control = sender as AbstractCameraPropertyView;
            if (control == null || control.Property == null)
                return;

            gammaCorrectionValue = float.Parse(control.Property.CurrentValue);
            specificChanged = true;
        }

        private void ReloadProperty(string key)
        {
            if (!propertiesControls.ContainsKey(key) || !cameraProperties.ContainsKey(key))
                return;

            // Reload the property in case the range or current value changed.
            CameraProperty prop = CameraPropertyManager.Read(nodeMap, key);
            if (prop == null)
                return;

            cameraProperties[key] = prop;
            propertiesControls[key].Repopulate(prop);
        }

        private void cpvCameraControl_ValueChanged(object sender, EventArgs e)
        {
            AbstractCameraPropertyView control = sender as AbstractCameraPropertyView;
            if (control == null)
                return;

            string key = control.Tag as string;
            if (string.IsNullOrEmpty(key) || !cameraProperties.ContainsKey(key))
                return;

            CameraPropertyManager.Write(nodeMap, control.Property);

            if (key == "offsetX" || key == "offsetY" || key == "height" || key == "width")
            {
                ReloadProperty("framerate");
                ReloadProperty("exposure");
                if(key != "offsetX")
                    ReloadProperty("offsetX");
                if (key != "offsetY")
                    ReloadProperty("offsetY");
                if (key != "height")
                    ReloadProperty("height");
                if (key != "width")
                    ReloadProperty("width");
            }
            
            if (key == "exposure")
            {
                ReloadProperty("framerate");
            }
            specificChanged = true;
        }
        
        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            IDSEnum selected = cmbFormat.SelectedItem as IDSEnum;
            if (selected == null || selectedStreamFormat.Value == selected.Value)
                return;

            selectedStreamFormat = selected;
            specificChanged = true;
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

            info.StreamFormat = this.SelectedStreamFormat.Value;
            info.CameraProperties = this.CameraProperties;
            info.GammaCorrectionValue = this.GammaCorrectionValue;
            summary.UpdateDisplayRectangle(Rectangle.Empty);
            CameraTypeManager.UpdatedCameraSummary(summary);

            disconnect();
            connect();

            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null || specific.NodeMap == null)
                return;

            nodeMap = specific.NodeMap;
            cameraProperties = CameraPropertyManager.Read(nodeMap);

            RemoveCameraControls();
            PopulateCameraControls();
            specificChanged = false;
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            // Locate an .xml file.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = CameraLang.FormConfiguration_ImportParameters;
            //openFileDialog.InitialDirectory = Path.GetDirectoryName(ProfileHelper.GetProfileFilename(summary.Identifier));
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = Services.FilesystemHelper.OpenCSETFilter();
            openFileDialog.FilterIndex = 0;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            string filename = openFileDialog.FileName;
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                return;

            // The timing here is finnicky.
            // connect() will start the delay buffer allocation on the current image size and start receiving frames.
            // disconnect prevents reading the new values from the camera.
            // Load with new sizes while the camera is streaming will fail because the buffers are wrong.
            // So we need to load the new values with the camera opened but not streaming.

            this.SuspendLayout();

            disconnect();
            ProfileHelper.Replace(summary.Identifier, filename);


            // Load new parameters.
            ProfileHelper.Load(nodeMap, summary.Identifier);
            cameraProperties = CameraPropertyManager.Read(nodeMap);
            SpecificInfo info = summary.Specific as SpecificInfo;
            PopulateStreamFormat();
            info.StreamFormat = this.SelectedStreamFormat.Value;
            info.CameraProperties = cameraProperties;
            summary.UpdateDisplayRectangle(Rectangle.Empty);
            CameraTypeManager.UpdatedCameraSummary(summary);

            connect();
            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null || specific.NodeMap == null)
                return;

            nodeMap = specific.NodeMap;
            // Reload UI.
            RemoveCameraControls();
            PopulateCameraControls();

            this.ResumeLayout();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
          this.DialogResult = DialogResult.OK;
          this.Close();
        }
    }
}
