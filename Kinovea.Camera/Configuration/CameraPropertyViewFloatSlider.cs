using System;
using System.Windows.Forms;
using System.Globalization;
using Kinovea.Camera.Languages;
using Kinovea.Services;

namespace Kinovea.Camera
{
    public partial class CameraPropertyViewFloatSlider : AbstractCameraPropertyView
    {
        private bool updatingValue;
        private Func<float, string> valueMapper;
        private static Func<float, string> defaultValueMapper = (value) => value.ToString();

        public CameraPropertyViewFloatSlider(CameraProperty property, string text, Func<float, string> valueMapper)
        {
            this.prop = property;

            bool useDefaultMapper = valueMapper == null;
            this.valueMapper = useDefaultMapper ? defaultValueMapper : valueMapper;;
            
            InitializeComponent();
            lblValue.Left = nud.Left;

            // TODO: retrieve localized name from the localization token.
            lblName.Text = text;

            if (property.Supported)
                Populate();
            else
                this.Enabled = false;
            
            nud.Visible = useDefaultMapper;
            lblValue.Visible = !useDefaultMapper;
            NudHelper.FixNudScroll(nud);
        }

        public override void Repopulate(CameraProperty property)
        {
            this.prop = property;
            if (property.Supported)
                Populate();
        }

        private void Populate()
        {
            // FIXME: doesn't play well with non integer values.

            cbAuto.Enabled = prop.CanBeAutomatic;

            float min = float.Parse(prop.Minimum, CultureInfo.InvariantCulture);
            float max = float.Parse(prop.Maximum, CultureInfo.InvariantCulture);
            float value = float.Parse(prop.CurrentValue, CultureInfo.InvariantCulture);
            float step = float.Parse(prop.Step, CultureInfo.InvariantCulture);
            value = Math.Min(max, Math.Max(min, value));
            
            updatingValue = true;
            nud.Minimum = (decimal)min;
            nud.Maximum = (decimal)max;
            nud.Value = (decimal)value;
            nud.Increment = (decimal)step;
            tbValue.Precision = step;
            tbValue.Minimum = min;
            tbValue.Maximum = max;
            tbValue.Value = value;
            
            cbAuto.Checked = prop.Automatic;
            updatingValue = false;

            lblValue.Text = valueMapper(value);
        }

        private void tbValue_ValueChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            float numericValue = tbValue.Value;
            string strValue = numericValue.ToString(CultureInfo.InvariantCulture);

            prop.CurrentValue = strValue;
            lblValue.Text = valueMapper(numericValue);

            prop.Automatic = false;
            
            updatingValue = true;
            cbAuto.Checked = prop.Automatic;
            nud.Value = (decimal)numericValue;
            updatingValue = false;

            RaiseValueChanged();
        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            float numericValue = (float)nud.Value;
            string strValue = numericValue.ToString(CultureInfo.InvariantCulture);

            prop.CurrentValue = strValue;
            lblValue.Text = valueMapper(numericValue);

            prop.Automatic = false;

            updatingValue = true;
            cbAuto.Checked = prop.Automatic;
            tbValue.Value = numericValue;
            updatingValue = false;

            RaiseValueChanged();
        }

        private void cbAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (updatingValue)
                return;

            prop.Automatic = cbAuto.Checked;

            RaiseValueChanged();
        }
    }
}
