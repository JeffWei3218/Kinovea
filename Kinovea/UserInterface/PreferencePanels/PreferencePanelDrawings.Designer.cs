#region License
/*
Copyright ?Joan Charmant 2011.
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
namespace Kinovea.Root
{
	partial class PreferencePanelDrawings
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the control.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
      this.tabSubPages = new System.Windows.Forms.TabControl();
      this.tabGeneral = new System.Windows.Forms.TabPage();
      this.chkEnableHSDS = new System.Windows.Forms.CheckBox();
      this.chkCustomToolsDebug = new System.Windows.Forms.CheckBox();
      this.chkEnableFiltering = new System.Windows.Forms.CheckBox();
      this.chkDrawOnPlay = new System.Windows.Forms.CheckBox();
      this.tabPersistence = new System.Windows.Forms.TabPage();
      this.chkAlwaysVisible = new System.Windows.Forms.CheckBox();
      this.nudFading = new System.Windows.Forms.NumericUpDown();
      this.nudOpaque = new System.Windows.Forms.NumericUpDown();
      this.nudMax = new System.Windows.Forms.NumericUpDown();
      this.lblOpaque = new System.Windows.Forms.Label();
      this.lblFading = new System.Windows.Forms.Label();
      this.lblMax = new System.Windows.Forms.Label();
      this.lblDefaultOpacity = new System.Windows.Forms.Label();
      this.tabTracking = new System.Windows.Forms.TabPage();
      this.lblDescription = new System.Windows.Forms.Label();
      this.cmbSearchWindowUnit = new System.Windows.Forms.ComboBox();
      this.cmbBlockWindowUnit = new System.Windows.Forms.ComboBox();
      this.tbSearchHeight = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.tbSearchWidth = new System.Windows.Forms.TextBox();
      this.tbBlockHeight = new System.Windows.Forms.TextBox();
      this.lblSearchWindow = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.tbBlockWidth = new System.Windows.Forms.TextBox();
      this.lblObjectWindow = new System.Windows.Forms.Label();
      this.tabUnits = new System.Windows.Forms.TabPage();
      this.cmbCadenceUnit = new System.Windows.Forms.ComboBox();
      this.lblCadenceUnit = new System.Windows.Forms.Label();
      this.tbCustomLengthAb = new System.Windows.Forms.TextBox();
      this.tbCustomLengthUnit = new System.Windows.Forms.TextBox();
      this.lblCustomLength = new System.Windows.Forms.Label();
      this.cmbAngularAccelerationUnit = new System.Windows.Forms.ComboBox();
      this.lblAngularAcceleration = new System.Windows.Forms.Label();
      this.cmbAngularVelocityUnit = new System.Windows.Forms.ComboBox();
      this.lblAngularVelocityUnit = new System.Windows.Forms.Label();
      this.cmbAngleUnit = new System.Windows.Forms.ComboBox();
      this.lblAngleUnit = new System.Windows.Forms.Label();
      this.cmbAccelerationUnit = new System.Windows.Forms.ComboBox();
      this.lblAccelerationUnit = new System.Windows.Forms.Label();
      this.cmbSpeedUnit = new System.Windows.Forms.ComboBox();
      this.lblSpeedUnit = new System.Windows.Forms.Label();
      this.cmbTimeCodeFormat = new System.Windows.Forms.ComboBox();
      this.lblTimeMarkersFormat = new System.Windows.Forms.Label();
      this.tabPresets = new System.Windows.Forms.TabPage();
      this.tabExport = new System.Windows.Forms.TabPage();
      this.lblPandocPath = new System.Windows.Forms.Label();
      this.tbPandocPath = new System.Windows.Forms.TextBox();
      this.btnPandocPath = new System.Windows.Forms.Button();
      this.cmbExportSpace = new System.Windows.Forms.ComboBox();
      this.lblExportSpace = new System.Windows.Forms.Label();
      this.cmbDelimiter = new System.Windows.Forms.ComboBox();
      this.lblCSVDelimiter = new System.Windows.Forms.Label();
      this.cbExportImagesInDocs = new System.Windows.Forms.CheckBox();
      this.gbDocuments = new System.Windows.Forms.GroupBox();
      this.gbSpreadsheet = new System.Windows.Forms.GroupBox();
      this.tabSubPages.SuspendLayout();
      this.tabGeneral.SuspendLayout();
      this.tabPersistence.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudFading)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudOpaque)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudMax)).BeginInit();
      this.tabTracking.SuspendLayout();
      this.tabUnits.SuspendLayout();
      this.tabExport.SuspendLayout();
      this.gbDocuments.SuspendLayout();
      this.gbSpreadsheet.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabSubPages
      // 
      this.tabSubPages.Controls.Add(this.tabGeneral);
      this.tabSubPages.Controls.Add(this.tabPersistence);
      this.tabSubPages.Controls.Add(this.tabTracking);
      this.tabSubPages.Controls.Add(this.tabUnits);
      this.tabSubPages.Controls.Add(this.tabPresets);
      this.tabSubPages.Controls.Add(this.tabExport);
      this.tabSubPages.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabSubPages.Location = new System.Drawing.Point(0, 0);
      this.tabSubPages.Name = "tabSubPages";
      this.tabSubPages.SelectedIndex = 0;
      this.tabSubPages.Size = new System.Drawing.Size(490, 322);
      this.tabSubPages.TabIndex = 0;
      // 
      // tabGeneral
      // 
      this.tabGeneral.Controls.Add(this.chkEnableHSDS);
      this.tabGeneral.Controls.Add(this.chkCustomToolsDebug);
      this.tabGeneral.Controls.Add(this.chkEnableFiltering);
      this.tabGeneral.Controls.Add(this.chkDrawOnPlay);
      this.tabGeneral.Location = new System.Drawing.Point(4, 22);
      this.tabGeneral.Name = "tabGeneral";
      this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
      this.tabGeneral.Size = new System.Drawing.Size(482, 296);
      this.tabGeneral.TabIndex = 0;
      this.tabGeneral.Text = "General";
      this.tabGeneral.UseVisualStyleBackColor = true;
      // 
      // chkEnableHSDS
      // 
      this.chkEnableHSDS.AutoSize = true;
      this.chkEnableHSDS.Location = new System.Drawing.Point(27, 94);
      this.chkEnableHSDS.Name = "chkEnableHSDS";
      this.chkEnableHSDS.Size = new System.Drawing.Size(285, 17);
      this.chkEnableHSDS.TabIndex = 55;
      this.chkEnableHSDS.Text = "Enable smoothing of derivatives for high speed footage";
      this.chkEnableHSDS.UseVisualStyleBackColor = true;
      this.chkEnableHSDS.CheckedChanged += new System.EventHandler(this.chkEnableHSDS_CheckedChanged);
      // 
      // chkCustomToolsDebug
      // 
      this.chkCustomToolsDebug.AutoSize = true;
      this.chkCustomToolsDebug.Location = new System.Drawing.Point(27, 128);
      this.chkCustomToolsDebug.Name = "chkCustomToolsDebug";
      this.chkCustomToolsDebug.Size = new System.Drawing.Size(148, 17);
      this.chkCustomToolsDebug.TabIndex = 54;
      this.chkCustomToolsDebug.Text = "Custom tools debug mode";
      this.chkCustomToolsDebug.UseVisualStyleBackColor = true;
      this.chkCustomToolsDebug.CheckedChanged += new System.EventHandler(this.chkCustomToolsDebug_CheckedChanged);
      // 
      // chkEnableFiltering
      // 
      this.chkEnableFiltering.AutoSize = true;
      this.chkEnableFiltering.Location = new System.Drawing.Point(27, 62);
      this.chkEnableFiltering.Name = "chkEnableFiltering";
      this.chkEnableFiltering.Size = new System.Drawing.Size(153, 17);
      this.chkEnableFiltering.TabIndex = 53;
      this.chkEnableFiltering.Text = "Enable coordinates filtering";
      this.chkEnableFiltering.UseVisualStyleBackColor = true;
      this.chkEnableFiltering.CheckedChanged += new System.EventHandler(this.chkEnableFiltering_CheckedChanged);
      // 
      // chkDrawOnPlay
      // 
      this.chkDrawOnPlay.AutoSize = true;
      this.chkDrawOnPlay.Location = new System.Drawing.Point(27, 27);
      this.chkDrawOnPlay.Name = "chkDrawOnPlay";
      this.chkDrawOnPlay.Size = new System.Drawing.Size(202, 17);
      this.chkDrawOnPlay.TabIndex = 52;
      this.chkDrawOnPlay.Text = "Show drawings when video is playing";
      this.chkDrawOnPlay.UseVisualStyleBackColor = true;
      this.chkDrawOnPlay.CheckedChanged += new System.EventHandler(this.chkDrawOnPlay_CheckedChanged);
      // 
      // tabPersistence
      // 
      this.tabPersistence.Controls.Add(this.chkAlwaysVisible);
      this.tabPersistence.Controls.Add(this.nudFading);
      this.tabPersistence.Controls.Add(this.nudOpaque);
      this.tabPersistence.Controls.Add(this.nudMax);
      this.tabPersistence.Controls.Add(this.lblOpaque);
      this.tabPersistence.Controls.Add(this.lblFading);
      this.tabPersistence.Controls.Add(this.lblMax);
      this.tabPersistence.Controls.Add(this.lblDefaultOpacity);
      this.tabPersistence.Location = new System.Drawing.Point(4, 22);
      this.tabPersistence.Name = "tabPersistence";
      this.tabPersistence.Padding = new System.Windows.Forms.Padding(3);
      this.tabPersistence.Size = new System.Drawing.Size(482, 296);
      this.tabPersistence.TabIndex = 1;
      this.tabPersistence.Text = "Opacity";
      this.tabPersistence.UseVisualStyleBackColor = true;
      // 
      // chkAlwaysVisible
      // 
      this.chkAlwaysVisible.AutoSize = true;
      this.chkAlwaysVisible.Location = new System.Drawing.Point(56, 65);
      this.chkAlwaysVisible.Name = "chkAlwaysVisible";
      this.chkAlwaysVisible.Size = new System.Drawing.Size(91, 17);
      this.chkAlwaysVisible.TabIndex = 65;
      this.chkAlwaysVisible.Text = "Always visible";
      this.chkAlwaysVisible.UseVisualStyleBackColor = true;
      this.chkAlwaysVisible.CheckedChanged += new System.EventHandler(this.chkAlwaysVisible_CheckedChanged);
      // 
      // nudFading
      // 
      this.nudFading.Location = new System.Drawing.Point(264, 167);
      this.nudFading.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
      this.nudFading.Name = "nudFading";
      this.nudFading.Size = new System.Drawing.Size(52, 20);
      this.nudFading.TabIndex = 64;
      this.nudFading.ValueChanged += new System.EventHandler(this.nudFading_ValueChanged);
      // 
      // nudOpaque
      // 
      this.nudOpaque.Location = new System.Drawing.Point(264, 132);
      this.nudOpaque.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
      this.nudOpaque.Name = "nudOpaque";
      this.nudOpaque.Size = new System.Drawing.Size(52, 20);
      this.nudOpaque.TabIndex = 63;
      this.nudOpaque.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.nudOpaque.ValueChanged += new System.EventHandler(this.nudOpaque_ValueChanged);
      // 
      // nudMax
      // 
      this.nudMax.Location = new System.Drawing.Point(264, 98);
      this.nudMax.Name = "nudMax";
      this.nudMax.Size = new System.Drawing.Size(52, 20);
      this.nudMax.TabIndex = 62;
      this.nudMax.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.nudMax.ValueChanged += new System.EventHandler(this.nudMax_ValueChanged);
      // 
      // lblOpaque
      // 
      this.lblOpaque.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblOpaque.Location = new System.Drawing.Point(53, 128);
      this.lblOpaque.Name = "lblOpaque";
      this.lblOpaque.Size = new System.Drawing.Size(194, 25);
      this.lblOpaque.TabIndex = 61;
      this.lblOpaque.Text = "Opaque duration (frames):";
      this.lblOpaque.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblFading
      // 
      this.lblFading.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblFading.Location = new System.Drawing.Point(53, 163);
      this.lblFading.Name = "lblFading";
      this.lblFading.Size = new System.Drawing.Size(205, 25);
      this.lblFading.TabIndex = 60;
      this.lblFading.Text = "Fading duration (frames):";
      this.lblFading.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblMax
      // 
      this.lblMax.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblMax.Location = new System.Drawing.Point(53, 94);
      this.lblMax.Name = "lblMax";
      this.lblMax.Size = new System.Drawing.Size(153, 25);
      this.lblMax.TabIndex = 59;
      this.lblMax.Text = "Maximum opacity (%):";
      this.lblMax.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblDefaultOpacity
      // 
      this.lblDefaultOpacity.Location = new System.Drawing.Point(20, 18);
      this.lblDefaultOpacity.Name = "lblDefaultOpacity";
      this.lblDefaultOpacity.Size = new System.Drawing.Size(362, 32);
      this.lblDefaultOpacity.TabIndex = 56;
      this.lblDefaultOpacity.Text = "Default opacity of new drawings:";
      this.lblDefaultOpacity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // tabTracking
      // 
      this.tabTracking.Controls.Add(this.lblDescription);
      this.tabTracking.Controls.Add(this.cmbSearchWindowUnit);
      this.tabTracking.Controls.Add(this.cmbBlockWindowUnit);
      this.tabTracking.Controls.Add(this.tbSearchHeight);
      this.tabTracking.Controls.Add(this.label5);
      this.tabTracking.Controls.Add(this.tbSearchWidth);
      this.tabTracking.Controls.Add(this.tbBlockHeight);
      this.tabTracking.Controls.Add(this.lblSearchWindow);
      this.tabTracking.Controls.Add(this.label4);
      this.tabTracking.Controls.Add(this.tbBlockWidth);
      this.tabTracking.Controls.Add(this.lblObjectWindow);
      this.tabTracking.Location = new System.Drawing.Point(4, 22);
      this.tabTracking.Name = "tabTracking";
      this.tabTracking.Padding = new System.Windows.Forms.Padding(3);
      this.tabTracking.Size = new System.Drawing.Size(482, 296);
      this.tabTracking.TabIndex = 2;
      this.tabTracking.Text = "Tracking";
      this.tabTracking.UseVisualStyleBackColor = true;
      // 
      // lblDescription
      // 
      this.lblDescription.AutoSize = true;
      this.lblDescription.Location = new System.Drawing.Point(16, 13);
      this.lblDescription.Name = "lblDescription";
      this.lblDescription.Size = new System.Drawing.Size(137, 13);
      this.lblDescription.TabIndex = 67;
      this.lblDescription.Text = "Default tracking parameters";
      // 
      // cmbSearchWindowUnit
      // 
      this.cmbSearchWindowUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbSearchWindowUnit.Location = new System.Drawing.Point(288, 72);
      this.cmbSearchWindowUnit.Name = "cmbSearchWindowUnit";
      this.cmbSearchWindowUnit.Size = new System.Drawing.Size(116, 21);
      this.cmbSearchWindowUnit.TabIndex = 66;
      this.cmbSearchWindowUnit.SelectedIndexChanged += new System.EventHandler(this.cmbSearchWindowUnit_SelectedIndexChanged);
      // 
      // cmbBlockWindowUnit
      // 
      this.cmbBlockWindowUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbBlockWindowUnit.Location = new System.Drawing.Point(288, 45);
      this.cmbBlockWindowUnit.Name = "cmbBlockWindowUnit";
      this.cmbBlockWindowUnit.Size = new System.Drawing.Size(116, 21);
      this.cmbBlockWindowUnit.TabIndex = 65;
      this.cmbBlockWindowUnit.SelectedIndexChanged += new System.EventHandler(this.cmbBlockWindowUnit_SelectedIndexChanged);
      // 
      // tbSearchHeight
      // 
      this.tbSearchHeight.Location = new System.Drawing.Point(241, 72);
      this.tbSearchHeight.Name = "tbSearchHeight";
      this.tbSearchHeight.Size = new System.Drawing.Size(30, 20);
      this.tbSearchHeight.TabIndex = 62;
      this.tbSearchHeight.TextChanged += new System.EventHandler(this.tbSearchHeight_TextChanged);
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(222, 75);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(13, 13);
      this.label5.TabIndex = 61;
      this.label5.Text = "?";
      // 
      // tbSearchWidth
      // 
      this.tbSearchWidth.Location = new System.Drawing.Point(182, 72);
      this.tbSearchWidth.Name = "tbSearchWidth";
      this.tbSearchWidth.Size = new System.Drawing.Size(30, 20);
      this.tbSearchWidth.TabIndex = 60;
      this.tbSearchWidth.TextChanged += new System.EventHandler(this.tbSearchWidth_TextChanged);
      // 
      // tbBlockHeight
      // 
      this.tbBlockHeight.Location = new System.Drawing.Point(241, 46);
      this.tbBlockHeight.Name = "tbBlockHeight";
      this.tbBlockHeight.Size = new System.Drawing.Size(30, 20);
      this.tbBlockHeight.TabIndex = 59;
      this.tbBlockHeight.TextChanged += new System.EventHandler(this.tbBlockHeight_TextChanged);
      // 
      // lblSearchWindow
      // 
      this.lblSearchWindow.AutoSize = true;
      this.lblSearchWindow.Location = new System.Drawing.Point(52, 76);
      this.lblSearchWindow.Name = "lblSearchWindow";
      this.lblSearchWindow.Size = new System.Drawing.Size(86, 13);
      this.lblSearchWindow.TabIndex = 58;
      this.lblSearchWindow.Text = "Search window :";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(222, 49);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(13, 13);
      this.label4.TabIndex = 57;
      this.label4.Text = "?";
      // 
      // tbBlockWidth
      // 
      this.tbBlockWidth.Location = new System.Drawing.Point(182, 46);
      this.tbBlockWidth.Name = "tbBlockWidth";
      this.tbBlockWidth.Size = new System.Drawing.Size(30, 20);
      this.tbBlockWidth.TabIndex = 55;
      this.tbBlockWidth.TextChanged += new System.EventHandler(this.tbBlockWidth_TextChanged);
      // 
      // lblObjectWindow
      // 
      this.lblObjectWindow.AutoSize = true;
      this.lblObjectWindow.Location = new System.Drawing.Point(52, 49);
      this.lblObjectWindow.Name = "lblObjectWindow";
      this.lblObjectWindow.Size = new System.Drawing.Size(83, 13);
      this.lblObjectWindow.TabIndex = 56;
      this.lblObjectWindow.Text = "Object window :";
      // 
      // tabUnits
      // 
      this.tabUnits.Controls.Add(this.cmbCadenceUnit);
      this.tabUnits.Controls.Add(this.lblCadenceUnit);
      this.tabUnits.Controls.Add(this.tbCustomLengthAb);
      this.tabUnits.Controls.Add(this.tbCustomLengthUnit);
      this.tabUnits.Controls.Add(this.lblCustomLength);
      this.tabUnits.Controls.Add(this.cmbAngularAccelerationUnit);
      this.tabUnits.Controls.Add(this.lblAngularAcceleration);
      this.tabUnits.Controls.Add(this.cmbAngularVelocityUnit);
      this.tabUnits.Controls.Add(this.lblAngularVelocityUnit);
      this.tabUnits.Controls.Add(this.cmbAngleUnit);
      this.tabUnits.Controls.Add(this.lblAngleUnit);
      this.tabUnits.Controls.Add(this.cmbAccelerationUnit);
      this.tabUnits.Controls.Add(this.lblAccelerationUnit);
      this.tabUnits.Controls.Add(this.cmbSpeedUnit);
      this.tabUnits.Controls.Add(this.lblSpeedUnit);
      this.tabUnits.Controls.Add(this.cmbTimeCodeFormat);
      this.tabUnits.Controls.Add(this.lblTimeMarkersFormat);
      this.tabUnits.Location = new System.Drawing.Point(4, 22);
      this.tabUnits.Name = "tabUnits";
      this.tabUnits.Padding = new System.Windows.Forms.Padding(3);
      this.tabUnits.Size = new System.Drawing.Size(482, 296);
      this.tabUnits.TabIndex = 4;
      this.tabUnits.Text = "Units";
      this.tabUnits.UseVisualStyleBackColor = true;
      // 
      // cmbCadenceUnit
      // 
      this.cmbCadenceUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbCadenceUnit.Location = new System.Drawing.Point(186, 259);
      this.cmbCadenceUnit.Name = "cmbCadenceUnit";
      this.cmbCadenceUnit.Size = new System.Drawing.Size(221, 21);
      this.cmbCadenceUnit.TabIndex = 61;
      this.cmbCadenceUnit.SelectedIndexChanged += new System.EventHandler(this.cmbCadenceUnit_SelectedIndexChanged);
      // 
      // lblCadenceUnit
      // 
      this.lblCadenceUnit.AutoSize = true;
      this.lblCadenceUnit.Location = new System.Drawing.Point(30, 264);
      this.lblCadenceUnit.Name = "lblCadenceUnit";
      this.lblCadenceUnit.Size = new System.Drawing.Size(56, 13);
      this.lblCadenceUnit.TabIndex = 60;
      this.lblCadenceUnit.Text = "Cadence :";
      // 
      // tbCustomLengthAb
      // 
      this.tbCustomLengthAb.Location = new System.Drawing.Point(358, 223);
      this.tbCustomLengthAb.Name = "tbCustomLengthAb";
      this.tbCustomLengthAb.Size = new System.Drawing.Size(49, 20);
      this.tbCustomLengthAb.TabIndex = 59;
      this.tbCustomLengthAb.TextChanged += new System.EventHandler(this.tbCustomLengthAb_TextChanged);
      // 
      // tbCustomLengthUnit
      // 
      this.tbCustomLengthUnit.Location = new System.Drawing.Point(214, 223);
      this.tbCustomLengthUnit.Name = "tbCustomLengthUnit";
      this.tbCustomLengthUnit.Size = new System.Drawing.Size(138, 20);
      this.tbCustomLengthUnit.TabIndex = 58;
      this.tbCustomLengthUnit.TextChanged += new System.EventHandler(this.tbCustomLengthUnit_TextChanged);
      // 
      // lblCustomLength
      // 
      this.lblCustomLength.AutoSize = true;
      this.lblCustomLength.Location = new System.Drawing.Point(30, 227);
      this.lblCustomLength.Name = "lblCustomLength";
      this.lblCustomLength.Size = new System.Drawing.Size(100, 13);
      this.lblCustomLength.TabIndex = 57;
      this.lblCustomLength.Text = "Custom length unit :";
      // 
      // cmbAngularAccelerationUnit
      // 
      this.cmbAngularAccelerationUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbAngularAccelerationUnit.Location = new System.Drawing.Point(186, 187);
      this.cmbAngularAccelerationUnit.Name = "cmbAngularAccelerationUnit";
      this.cmbAngularAccelerationUnit.Size = new System.Drawing.Size(221, 21);
      this.cmbAngularAccelerationUnit.TabIndex = 56;
      this.cmbAngularAccelerationUnit.SelectedIndexChanged += new System.EventHandler(this.cmbAngularAccelerationUnit_SelectedIndexChanged);
      // 
      // lblAngularAcceleration
      // 
      this.lblAngularAcceleration.AutoSize = true;
      this.lblAngularAcceleration.Location = new System.Drawing.Point(30, 192);
      this.lblAngularAcceleration.Name = "lblAngularAcceleration";
      this.lblAngularAcceleration.Size = new System.Drawing.Size(110, 13);
      this.lblAngularAcceleration.TabIndex = 55;
      this.lblAngularAcceleration.Text = "Angular acceleration :";
      // 
      // cmbAngularVelocityUnit
      // 
      this.cmbAngularVelocityUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbAngularVelocityUnit.Location = new System.Drawing.Point(186, 153);
      this.cmbAngularVelocityUnit.Name = "cmbAngularVelocityUnit";
      this.cmbAngularVelocityUnit.Size = new System.Drawing.Size(221, 21);
      this.cmbAngularVelocityUnit.TabIndex = 54;
      this.cmbAngularVelocityUnit.SelectedIndexChanged += new System.EventHandler(this.cmbAngularVelocityUnit_SelectedIndexChanged);
      // 
      // lblAngularVelocityUnit
      // 
      this.lblAngularVelocityUnit.AutoSize = true;
      this.lblAngularVelocityUnit.Location = new System.Drawing.Point(30, 158);
      this.lblAngularVelocityUnit.Name = "lblAngularVelocityUnit";
      this.lblAngularVelocityUnit.Size = new System.Drawing.Size(81, 13);
      this.lblAngularVelocityUnit.TabIndex = 53;
      this.lblAngularVelocityUnit.Text = "Angular speed :";
      // 
      // cmbAngleUnit
      // 
      this.cmbAngleUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbAngleUnit.Location = new System.Drawing.Point(186, 119);
      this.cmbAngleUnit.Name = "cmbAngleUnit";
      this.cmbAngleUnit.Size = new System.Drawing.Size(221, 21);
      this.cmbAngleUnit.TabIndex = 52;
      this.cmbAngleUnit.SelectedIndexChanged += new System.EventHandler(this.cmbAngleUnit_SelectedIndexChanged);
      // 
      // lblAngleUnit
      // 
      this.lblAngleUnit.AutoSize = true;
      this.lblAngleUnit.Location = new System.Drawing.Point(30, 124);
      this.lblAngleUnit.Name = "lblAngleUnit";
      this.lblAngleUnit.Size = new System.Drawing.Size(40, 13);
      this.lblAngleUnit.TabIndex = 51;
      this.lblAngleUnit.Text = "Angle :";
      // 
      // cmbAccelerationUnit
      // 
      this.cmbAccelerationUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbAccelerationUnit.Location = new System.Drawing.Point(186, 85);
      this.cmbAccelerationUnit.Name = "cmbAccelerationUnit";
      this.cmbAccelerationUnit.Size = new System.Drawing.Size(221, 21);
      this.cmbAccelerationUnit.TabIndex = 50;
      this.cmbAccelerationUnit.SelectedIndexChanged += new System.EventHandler(this.cmbAccelerationUnit_SelectedIndexChanged);
      // 
      // lblAccelerationUnit
      // 
      this.lblAccelerationUnit.AutoSize = true;
      this.lblAccelerationUnit.Location = new System.Drawing.Point(30, 90);
      this.lblAccelerationUnit.Name = "lblAccelerationUnit";
      this.lblAccelerationUnit.Size = new System.Drawing.Size(72, 13);
      this.lblAccelerationUnit.TabIndex = 49;
      this.lblAccelerationUnit.Text = "Acceleration :";
      // 
      // cmbSpeedUnit
      // 
      this.cmbSpeedUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbSpeedUnit.Location = new System.Drawing.Point(186, 51);
      this.cmbSpeedUnit.Name = "cmbSpeedUnit";
      this.cmbSpeedUnit.Size = new System.Drawing.Size(221, 21);
      this.cmbSpeedUnit.TabIndex = 48;
      this.cmbSpeedUnit.SelectedIndexChanged += new System.EventHandler(this.cmbSpeedUnit_SelectedIndexChanged);
      // 
      // lblSpeedUnit
      // 
      this.lblSpeedUnit.AutoSize = true;
      this.lblSpeedUnit.Location = new System.Drawing.Point(30, 56);
      this.lblSpeedUnit.Name = "lblSpeedUnit";
      this.lblSpeedUnit.Size = new System.Drawing.Size(50, 13);
      this.lblSpeedUnit.TabIndex = 47;
      this.lblSpeedUnit.Text = "Velocity :";
      // 
      // cmbTimeCodeFormat
      // 
      this.cmbTimeCodeFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbTimeCodeFormat.Location = new System.Drawing.Point(186, 17);
      this.cmbTimeCodeFormat.Name = "cmbTimeCodeFormat";
      this.cmbTimeCodeFormat.Size = new System.Drawing.Size(221, 21);
      this.cmbTimeCodeFormat.TabIndex = 46;
      this.cmbTimeCodeFormat.SelectedIndexChanged += new System.EventHandler(this.cmbTimeCodeFormat_SelectedIndexChanged);
      // 
      // lblTimeMarkersFormat
      // 
      this.lblTimeMarkersFormat.AutoSize = true;
      this.lblTimeMarkersFormat.Location = new System.Drawing.Point(30, 20);
      this.lblTimeMarkersFormat.Name = "lblTimeMarkersFormat";
      this.lblTimeMarkersFormat.Size = new System.Drawing.Size(108, 13);
      this.lblTimeMarkersFormat.TabIndex = 45;
      this.lblTimeMarkersFormat.Text = "Time markers format :";
      // 
      // tabPresets
      // 
      this.tabPresets.Location = new System.Drawing.Point(4, 22);
      this.tabPresets.Margin = new System.Windows.Forms.Padding(2);
      this.tabPresets.Name = "tabPresets";
      this.tabPresets.Size = new System.Drawing.Size(482, 296);
      this.tabPresets.TabIndex = 5;
      this.tabPresets.Text = "Presets";
      this.tabPresets.UseVisualStyleBackColor = true;
      // 
      // tabExport
      // 
      this.tabExport.Controls.Add(this.gbSpreadsheet);
      this.tabExport.Controls.Add(this.gbDocuments);
      this.tabExport.Location = new System.Drawing.Point(4, 22);
      this.tabExport.Name = "tabExport";
      this.tabExport.Padding = new System.Windows.Forms.Padding(3);
      this.tabExport.Size = new System.Drawing.Size(482, 296);
      this.tabExport.TabIndex = 3;
      this.tabExport.Text = "Export";
      this.tabExport.UseVisualStyleBackColor = true;
      // 
      // lblPandocPath
      // 
      this.lblPandocPath.AutoSize = true;
      this.lblPandocPath.Location = new System.Drawing.Point(28, 65);
      this.lblPandocPath.Name = "lblPandocPath";
      this.lblPandocPath.Size = new System.Drawing.Size(71, 13);
      this.lblPandocPath.TabIndex = 70;
      this.lblPandocPath.Text = "Pandoc path:";
      // 
      // tbPandocPath
      // 
      this.tbPandocPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbPandocPath.Location = new System.Drawing.Point(220, 62);
      this.tbPandocPath.Name = "tbPandocPath";
      this.tbPandocPath.Size = new System.Drawing.Size(183, 20);
      this.tbPandocPath.TabIndex = 71;
      this.tbPandocPath.TextChanged += new System.EventHandler(this.tbPandocPath_TextChanged);
      // 
      // btnPandocPath
      // 
      this.btnPandocPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPandocPath.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnPandocPath.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnPandocPath.FlatAppearance.BorderSize = 0;
      this.btnPandocPath.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnPandocPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnPandocPath.Image = global::Kinovea.Root.Properties.Resources.folder;
      this.btnPandocPath.Location = new System.Drawing.Point(409, 61);
      this.btnPandocPath.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnPandocPath.Name = "btnPandocPath";
      this.btnPandocPath.Size = new System.Drawing.Size(20, 20);
      this.btnPandocPath.TabIndex = 72;
      this.btnPandocPath.Tag = "";
      this.btnPandocPath.UseVisualStyleBackColor = true;
      this.btnPandocPath.Click += new System.EventHandler(this.btnPandocPath_Click);
      // 
      // cmbExportSpace
      // 
      this.cmbExportSpace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbExportSpace.Location = new System.Drawing.Point(287, 25);
      this.cmbExportSpace.Name = "cmbExportSpace";
      this.cmbExportSpace.Size = new System.Drawing.Size(116, 21);
      this.cmbExportSpace.TabIndex = 69;
      this.cmbExportSpace.SelectedIndexChanged += new System.EventHandler(this.cmbExportSpace_SelectedIndexChanged);
      // 
      // lblExportSpace
      // 
      this.lblExportSpace.AutoSize = true;
      this.lblExportSpace.Location = new System.Drawing.Point(28, 28);
      this.lblExportSpace.Name = "lblExportSpace";
      this.lblExportSpace.Size = new System.Drawing.Size(71, 13);
      this.lblExportSpace.TabIndex = 68;
      this.lblExportSpace.Text = "Export metric:";
      // 
      // cmbDelimiter
      // 
      this.cmbDelimiter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbDelimiter.Location = new System.Drawing.Point(287, 64);
      this.cmbDelimiter.Name = "cmbDelimiter";
      this.cmbDelimiter.Size = new System.Drawing.Size(116, 21);
      this.cmbDelimiter.TabIndex = 67;
      this.cmbDelimiter.SelectedIndexChanged += new System.EventHandler(this.cmbDelimiter_SelectedIndexChanged);
      // 
      // lblCSVDelimiter
      // 
      this.lblCSVDelimiter.AutoSize = true;
      this.lblCSVDelimiter.Location = new System.Drawing.Point(28, 67);
      this.lblCSVDelimiter.Name = "lblCSVDelimiter";
      this.lblCSVDelimiter.Size = new System.Drawing.Size(107, 13);
      this.lblCSVDelimiter.TabIndex = 66;
      this.lblCSVDelimiter.Text = "CSV�Decimal symbol:";
      // 
      // cbExportImagesInDocs
      // 
      this.cbExportImagesInDocs.AutoSize = true;
      this.cbExportImagesInDocs.Location = new System.Drawing.Point(31, 32);
      this.cbExportImagesInDocs.Name = "cbExportImagesInDocs";
      this.cbExportImagesInDocs.Size = new System.Drawing.Size(92, 17);
      this.cbExportImagesInDocs.TabIndex = 73;
      this.cbExportImagesInDocs.Text = "Export images";
      this.cbExportImagesInDocs.UseVisualStyleBackColor = true;
      this.cbExportImagesInDocs.CheckedChanged += new System.EventHandler(this.cbExportImagesInDocs_CheckedChanged);
      // 
      // gbDocuments
      // 
      this.gbDocuments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.gbDocuments.Controls.Add(this.cbExportImagesInDocs);
      this.gbDocuments.Controls.Add(this.lblPandocPath);
      this.gbDocuments.Controls.Add(this.tbPandocPath);
      this.gbDocuments.Controls.Add(this.btnPandocPath);
      this.gbDocuments.Location = new System.Drawing.Point(6, 124);
      this.gbDocuments.Name = "gbDocuments";
      this.gbDocuments.Size = new System.Drawing.Size(461, 109);
      this.gbDocuments.TabIndex = 74;
      this.gbDocuments.TabStop = false;
      this.gbDocuments.Text = "Documents";
      // 
      // gbSpreadsheet
      // 
      this.gbSpreadsheet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.gbSpreadsheet.Controls.Add(this.lblCSVDelimiter);
      this.gbSpreadsheet.Controls.Add(this.cmbDelimiter);
      this.gbSpreadsheet.Controls.Add(this.cmbExportSpace);
      this.gbSpreadsheet.Controls.Add(this.lblExportSpace);
      this.gbSpreadsheet.Location = new System.Drawing.Point(6, 15);
      this.gbSpreadsheet.Name = "gbSpreadsheet";
      this.gbSpreadsheet.Size = new System.Drawing.Size(461, 103);
      this.gbSpreadsheet.TabIndex = 75;
      this.gbSpreadsheet.TabStop = false;
      this.gbSpreadsheet.Text = "Spreadsheet";
      // 
      // PreferencePanelDrawings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabSubPages);
      this.Name = "PreferencePanelDrawings";
      this.Size = new System.Drawing.Size(490, 322);
      this.tabSubPages.ResumeLayout(false);
      this.tabGeneral.ResumeLayout(false);
      this.tabGeneral.PerformLayout();
      this.tabPersistence.ResumeLayout(false);
      this.tabPersistence.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudFading)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudOpaque)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudMax)).EndInit();
      this.tabTracking.ResumeLayout(false);
      this.tabTracking.PerformLayout();
      this.tabUnits.ResumeLayout(false);
      this.tabUnits.PerformLayout();
      this.tabExport.ResumeLayout(false);
      this.gbDocuments.ResumeLayout(false);
      this.gbDocuments.PerformLayout();
      this.gbSpreadsheet.ResumeLayout(false);
      this.gbSpreadsheet.PerformLayout();
      this.ResumeLayout(false);

		}
		private System.Windows.Forms.TabControl tabSubPages;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.TabPage tabPersistence;
		private System.Windows.Forms.CheckBox chkDrawOnPlay;
        private System.Windows.Forms.TabPage tabTracking;
        private System.Windows.Forms.TextBox tbSearchHeight;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbSearchWidth;
        private System.Windows.Forms.TextBox tbBlockHeight;
        private System.Windows.Forms.Label lblSearchWindow;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbBlockWidth;
        private System.Windows.Forms.Label lblObjectWindow;
        private System.Windows.Forms.ComboBox cmbSearchWindowUnit;
        private System.Windows.Forms.ComboBox cmbBlockWindowUnit;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblDefaultOpacity;
        private System.Windows.Forms.CheckBox chkEnableFiltering;
        private System.Windows.Forms.CheckBox chkCustomToolsDebug;
        private System.Windows.Forms.CheckBox chkAlwaysVisible;
        private System.Windows.Forms.NumericUpDown nudFading;
        private System.Windows.Forms.NumericUpDown nudOpaque;
        private System.Windows.Forms.NumericUpDown nudMax;
        private System.Windows.Forms.Label lblOpaque;
        private System.Windows.Forms.Label lblFading;
        private System.Windows.Forms.Label lblMax;
        private System.Windows.Forms.CheckBox chkEnableHSDS;
        private System.Windows.Forms.TabPage tabExport;
        private System.Windows.Forms.ComboBox cmbDelimiter;
        private System.Windows.Forms.Label lblCSVDelimiter;
        private System.Windows.Forms.TabPage tabUnits;
        private System.Windows.Forms.TextBox tbCustomLengthAb;
        private System.Windows.Forms.TextBox tbCustomLengthUnit;
        private System.Windows.Forms.Label lblCustomLength;
        private System.Windows.Forms.ComboBox cmbAngularAccelerationUnit;
        private System.Windows.Forms.Label lblAngularAcceleration;
        private System.Windows.Forms.ComboBox cmbAngularVelocityUnit;
        private System.Windows.Forms.Label lblAngularVelocityUnit;
        private System.Windows.Forms.ComboBox cmbAngleUnit;
        private System.Windows.Forms.Label lblAngleUnit;
        private System.Windows.Forms.ComboBox cmbAccelerationUnit;
        private System.Windows.Forms.Label lblAccelerationUnit;
        private System.Windows.Forms.ComboBox cmbSpeedUnit;
        private System.Windows.Forms.Label lblSpeedUnit;
        private System.Windows.Forms.ComboBox cmbTimeCodeFormat;
        private System.Windows.Forms.Label lblTimeMarkersFormat;
        private System.Windows.Forms.ComboBox cmbExportSpace;
        private System.Windows.Forms.Label lblExportSpace;
        private System.Windows.Forms.TabPage tabPresets;
        private System.Windows.Forms.Label lblPandocPath;
        private System.Windows.Forms.TextBox tbPandocPath;
        private System.Windows.Forms.Button btnPandocPath;
        private System.Windows.Forms.ComboBox cmbCadenceUnit;
        private System.Windows.Forms.Label lblCadenceUnit;
        private System.Windows.Forms.CheckBox cbExportImagesInDocs;
        private System.Windows.Forms.GroupBox gbDocuments;
        private System.Windows.Forms.GroupBox gbSpreadsheet;
    }
}
