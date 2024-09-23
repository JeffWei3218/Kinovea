using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Drawing;
using peak.core;
using peak.core.nodes;
using Kinovea.Services;

namespace Kinovea.Camera.IDSpeak
{
    /// <summary>
    /// Reads and writes a list of supported camera properties from/to the device.
    /// </summary>
    public static class CameraPropertyManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Dictionary<string, CameraProperty> Read(NodeMap nodeMap)
        {
            Dictionary<string, CameraProperty> properties = new Dictionary<string, CameraProperty>();
            
            try
            {
                // Retrieve camera properties that we support.
                ReadROI(nodeMap, properties);
                ReadFramerate(nodeMap, properties);
                ReadExposure(nodeMap, properties);
                ReadGain(nodeMap, properties);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while reading IDS camera properties. {0}.", e.Message);
            }
            return properties;
        }
        /// <summary>
        /// Push values from XML based simple properties into current list of properties.
        /// </summary>
        public static void MergeProperties(Dictionary<string, CameraProperty> dest, Dictionary<string, CameraProperty> source)
        {
            // This is used to import values from simple XML based representation into properties instances.
            foreach (var pair in source)
            {
                if (!dest.ContainsKey(pair.Key))
                    continue;

                dest[pair.Key].Automatic = pair.Value.Automatic;
                dest[pair.Key].CurrentValue = pair.Value.CurrentValue;
            }
        }
        /// <summary>
        /// Read a single property and return it.
        /// This is used in the context of dependent properties, to update the master list with new values.
        /// </summary>
        public static CameraProperty Read(NodeMap nodeMap, string key)
        {
            try
            {
                if (key == "framerate")
                    return ReadFramerate(nodeMap, null);
                else if (key == "exposure")
                    return ReadExposure(nodeMap, null);
                else if (key == "gain")
                    return ReadGain(nodeMap, null);
                else if(key == "offsetX" ||  key == "offsetY" || key == "width" || key == "height")
                {
                    Dictionary<string, CameraProperty> properties = new Dictionary<string, CameraProperty>();
                    ReadROI(nodeMap, properties);
                    if(properties.ContainsKey(key))
                        return properties[key];
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while reading IDS camera properties. {0}.", e.Message);
            }
            return null;
        }

        /// <summary>
        /// Commit value of properties that can be written during streaming and don't require a reconnect to be applied.
        /// This is used by the configuration, to update the image while configuring.
        /// </summary>
        public static void Write(NodeMap nodeMap, CameraProperty property)
        {
            if (!property.Supported || string.IsNullOrEmpty(property.Identifier))
                return ;

            // Only write non critical properties: properties that don't change image size.

            try
            {
                switch (property.Identifier)
                {
                    case "framerate":
                        WriteFramerate(nodeMap, property);
                        break;
                    case "exposure":
                        WriteExposure(nodeMap, property);
                        break;
                    case "gain":
                        WriteGain(nodeMap, property);
                        break;
                    case "offsetX":
                    case "offsetY":
                    case "width":
                    case "height":
                        // Do nothing. These properties must be changed from WriteCriticalProperties below.
                        WriteROI(nodeMap, property);
                        break;
                    default:
                        log.ErrorFormat("IDS uEye property not supported: {0}.", property.Identifier);
                        break;
                }
            }
            catch
            {
                log.ErrorFormat("Error while writing IDS uEye property {0}.", property.Identifier);
            }
        }

        /// <summary>
        /// Commit value of all properties.
        /// This is used when reconnecting after the configuration was changed.
        /// It is assumed that when this is called the device is open but not streaming.
        /// </summary>
        public static void WriteCriticalProperties(NodeMap nodeMap, Dictionary<string, CameraProperty> properties)
        {
            if (properties == null || properties.Count == 0)
                return;
            try
            {
                // We actually need to write all the properties again from here.
                // Even framerate, gain and exposure which update in real time would be lost if we don't write them outside of freerun.
                if (properties.ContainsKey("offsetX"))
                    WriteROI(nodeMap, properties["offsetX"]);

                if (properties.ContainsKey("offsetY"))
                    WriteROI(nodeMap, properties["offsetY"]);

                if (properties.ContainsKey("width"))
                    WriteROI(nodeMap, properties["width"]);

                if (properties.ContainsKey("height"))
                    WriteROI(nodeMap, properties["height"]);

                if (properties.ContainsKey("framerate"))
                    WriteFramerate(nodeMap, properties["framerate"]);

                if (properties.ContainsKey("exposure"))
                    WriteExposure(nodeMap, properties["exposure"]);

                if (properties.ContainsKey("gain"))
                    WriteGain(nodeMap, properties["gain"]);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while writing IDS camera properties. {0}.", e.Message);
            }
        }

        private static void ReadROI(NodeMap nodeMap, Dictionary<string, CameraProperty> properties)
        {
            // Get the current ROI
            var x = nodeMap.FindNode<IntegerNode>("OffsetX").Value();
            var y = nodeMap.FindNode<IntegerNode>("OffsetY").Value();
            var w = nodeMap.FindNode<IntegerNode>("Width").Value();
            var h = nodeMap.FindNode<IntegerNode>("Height").Value();

            // Get the minimum ROI
            var x_min = nodeMap.FindNode<IntegerNode>("OffsetX").Minimum();
            var y_min = nodeMap.FindNode<IntegerNode>("OffsetY").Minimum();
            var w_min = nodeMap.FindNode<IntegerNode>("Width").Minimum();
            var h_min = nodeMap.FindNode<IntegerNode>("Height").Minimum();
            // Get the maximum ROI values
            var x_max = nodeMap.FindNode<IntegerNode>("OffsetX").Maximum();
            var y_max = nodeMap.FindNode<IntegerNode>("OffsetY").Maximum();
            var w_max = nodeMap.FindNode<IntegerNode>("Width").Maximum();
            var accessStatus = nodeMap.FindNode<IntegerNode>("WidthMax").AccessStatus(); 
            if (accessStatus == NodeAccessStatus.ReadOnly || accessStatus == NodeAccessStatus.ReadWrite)
            {
                w_max = nodeMap.FindNode<IntegerNode>("WidthMax").Value();
            }
            var h_max = nodeMap.FindNode<IntegerNode>("Height").Maximum();
            accessStatus = nodeMap.FindNode<IntegerNode>("HeightMax").AccessStatus();
            if (accessStatus == NodeAccessStatus.ReadOnly || accessStatus == NodeAccessStatus.ReadWrite)
            {
                h_max = nodeMap.FindNode<IntegerNode>("HeightMax").Value();
            }
            // Get the increment
            var x_inc = nodeMap.FindNode<IntegerNode>("OffsetX").Increment();
            var y_inc = nodeMap.FindNode<IntegerNode>("OffsetY").Increment();
            var w_inc = nodeMap.FindNode<IntegerNode>("Width").Increment();
            var h_inc = nodeMap.FindNode<IntegerNode>("Height").Increment();

            CameraProperty propOffsetX = new CameraProperty();
            propOffsetX.Identifier = "offsetX";
            propOffsetX.Supported = true;
            propOffsetX.ReadOnly = false;
            propOffsetX.Type = CameraPropertyType.Integer;
            propOffsetX.Minimum = x_min.ToString(CultureInfo.InvariantCulture);
            propOffsetX.Maximum = x_max.ToString(CultureInfo.InvariantCulture);
            propOffsetX.Step = x_inc.ToString(CultureInfo.InvariantCulture);
            propOffsetX.Representation = CameraPropertyRepresentation.LinearSlider;
            propOffsetX.CurrentValue = x.ToString(CultureInfo.InvariantCulture);

            properties.Add(propOffsetX.Identifier, propOffsetX);

            CameraProperty propOffsetY = new CameraProperty();
            propOffsetY.Identifier = "offsetY";
            propOffsetY.Supported = true;
            propOffsetY.ReadOnly = false;
            propOffsetY.Type = CameraPropertyType.Integer;
            propOffsetY.Minimum = y_min.ToString(CultureInfo.InvariantCulture);
            propOffsetY.Maximum = y_max.ToString(CultureInfo.InvariantCulture);
            propOffsetY.Step = y_inc.ToString(CultureInfo.InvariantCulture);
            propOffsetY.Representation = CameraPropertyRepresentation.LinearSlider;
            propOffsetY.CurrentValue = y.ToString(CultureInfo.InvariantCulture);

            properties.Add(propOffsetY.Identifier, propOffsetY);

            CameraProperty propWidth = new CameraProperty();
            propWidth.Identifier = "width";
            propWidth.Supported = true;
            propWidth.ReadOnly = false;
            propWidth.Type = CameraPropertyType.Integer;
            propWidth.Minimum = w_min.ToString(CultureInfo.InvariantCulture);
            propWidth.Maximum = w_max.ToString(CultureInfo.InvariantCulture);
            propWidth.Step = w_inc.ToString(CultureInfo.InvariantCulture);
            propWidth.Representation = CameraPropertyRepresentation.LinearSlider;
            propWidth.CurrentValue = w.ToString(CultureInfo.InvariantCulture);
            
            properties.Add(propWidth.Identifier, propWidth);

            CameraProperty propHeight = new CameraProperty();
            propHeight.Identifier = "height";
            propHeight.Supported = true;
            propHeight.ReadOnly = false;
            propHeight.Type = CameraPropertyType.Integer;
            propHeight.Minimum = h_min.ToString(CultureInfo.InvariantCulture);
            propHeight.Maximum = h_max.ToString(CultureInfo.InvariantCulture);
            propHeight.Step = h_inc.ToString(CultureInfo.InvariantCulture);
            propHeight.Representation = CameraPropertyRepresentation.LinearSlider;
            propHeight.CurrentValue = h.ToString(CultureInfo.InvariantCulture);
            
            properties.Add(propHeight.Identifier, propHeight);
        }

        private static CameraProperty ReadFramerate(NodeMap nodeMap, Dictionary<string, CameraProperty> properties)
        {
            // Determine the current AcquisitionFrameRate
            double value = nodeMap.FindNode<FloatNode>("AcquisitionFrameRate").Value();
            double min = nodeMap.FindNode<FloatNode>("AcquisitionFrameRate").Minimum();
            double max = nodeMap.FindNode<FloatNode>("AcquisitionFrameRate").Maximum();
            double inc = 1.0;
            if (nodeMap.FindNode<FloatNode>("AcquisitionFrameRate").HasConstantIncrement())
            {
                inc = nodeMap.FindNode<FloatNode>("AcquisitionFrameRate").Increment();
            }

            CameraProperty p = new CameraProperty();
            p.Identifier = "framerate";
            p.Supported = true;
            p.ReadOnly = false;
            p.Type = CameraPropertyType.Float;
            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Step = inc.ToString(CultureInfo.InvariantCulture);
            // Fix values that should be log.
            //double range = Math.Log10(max) - Math.Log10(min);
            //p.Representation = (range >= 4) ? CameraPropertyRepresentation.LogarithmicSlider : CameraPropertyRepresentation.LinearSlider;
            p.Representation = CameraPropertyRepresentation.FloatSlider;
            p.CurrentValue = value.ToString(CultureInfo.InvariantCulture);

            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        private static CameraProperty ReadExposure(NodeMap nodeMap, Dictionary<string, CameraProperty> properties)
        {
            // Determine the current ExposureTime
            double value = nodeMap.FindNode<FloatNode>("ExposureTime").Value();
            double min = nodeMap.FindNode<FloatNode>("ExposureTime").Minimum();
            double max = nodeMap.FindNode<FloatNode>("ExposureTime").Maximum();
            double inc = nodeMap.FindNode<FloatNode>("ExposureTime").Increment();

            double frameRate = nodeMap.FindNode<FloatNode>("AcquisitionFrameRate").Value();
            max = Math.Min(max, 1000000.0f / frameRate);
            CameraProperty p = new CameraProperty();
            p.Identifier = "exposure";
            p.Supported = true;
            p.ReadOnly = false;
            p.Type = CameraPropertyType.Float;
            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Step = inc.ToString(CultureInfo.InvariantCulture);
            // Fix values that should be log.
            double range = Math.Log10(max) - Math.Log10(min);
            p.Representation = (range >= 4) ? CameraPropertyRepresentation.LogarithmicSlider : CameraPropertyRepresentation.LinearSlider;
            p.CurrentValue = value.ToString(CultureInfo.InvariantCulture);

            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        private static CameraProperty ReadGain(NodeMap nodeMap, Dictionary<string, CameraProperty> properties)
        {
            if (nodeMap.Handle() == null)
                return null;
            // Before accessing Gain, make sure GainSelector is set correctly
            // Set GainSelector to "AnalogAll"
            nodeMap.FindNode<peak.core.nodes.EnumerationNode>("GainSelector").SetCurrentEntry("AnalogAll");
            // Determine the current Gain
            double value = nodeMap.FindNode<FloatNode>("Gain").Value();
            double min = nodeMap.FindNode<FloatNode>("Gain").Minimum();
            double max = nodeMap.FindNode<FloatNode>("Gain").Maximum();
            double inc = 1.0;
            if (nodeMap.FindNode<FloatNode>("Gain").HasConstantIncrement())
            {
                inc = nodeMap.FindNode<FloatNode>("Gain").Increment();
            }
            CameraProperty p = new CameraProperty();
            p.Identifier = "gain";
            p.Supported = true;
            p.ReadOnly = false;
            p.Type = CameraPropertyType.Float;
            p.Minimum = min.ToString(CultureInfo.InvariantCulture);
            p.Maximum = max.ToString(CultureInfo.InvariantCulture);
            p.Step = inc.ToString(CultureInfo.InvariantCulture);
            p.Representation = CameraPropertyRepresentation.LinearSlider;
            p.CurrentValue = value.ToString(CultureInfo.InvariantCulture);

            if (properties != null)
                properties.Add(p.Identifier, p);

            return p;
        }

        /// <summary>
        /// Write either width or height as a centered region of interest.
        /// </summary>
        private static void WriteSize(NodeMap nodeMap, CameraProperty property)
        {
            if (property.ReadOnly)
                return;

            long value = int.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            string identifierName = "", identifierOffset = ""; 
            if(property.Identifier == "width")
            {
                identifierName = "Width";
                identifierOffset = "OffsetX";
            }
            else if(property.Identifier == "height")
            {
                identifierName = "Height";
                identifierOffset = "OffsetY";
            }
            long min = nodeMap.FindNode<IntegerNode>(identifierName).Minimum();
            long max = nodeMap.FindNode<IntegerNode>(identifierName).Maximum();
            long step = nodeMap.FindNode<IntegerNode>(identifierName).Increment();

            value = FixValue(value, min, max, step);

            // Offset handling.
            // Some cameras have a CenterX/CenterY property.
            // When it is set, the offset is automatic and becomes read-only.
            
            var accessStatus = nodeMap.FindNode<IntegerNode>(identifierOffset).AccessStatus();
            bool setOffset = accessStatus == NodeAccessStatus.ReadWrite;
            

            if (setOffset)
            {
                long offset = (max - value) / 2;
                long minOffset = nodeMap.FindNode<IntegerNode>(identifierOffset).Minimum();
                long stepOffset = nodeMap.FindNode<IntegerNode>(identifierOffset).Increment();
                long remainderOffset = (offset - minOffset) % stepOffset;
                if (remainderOffset != 0)
                    offset = offset - remainderOffset + stepOffset;

                // We need to be careful with the order and not write a value that doesn't fit due to the offset, or vice versa.
                long currentValue = nodeMap.FindNode<IntegerNode>(identifierName).Value();
                if (value > currentValue)
                {
                    nodeMap.FindNode<IntegerNode>(identifierOffset).SetValue(offset);
                    nodeMap.FindNode<IntegerNode>(identifierName).SetValue(value);
                }
                else
                {
                    nodeMap.FindNode<IntegerNode>(identifierName).SetValue(value);
                    nodeMap.FindNode<IntegerNode>(identifierOffset).SetValue(offset);
                }
            }
            else
            {
                nodeMap.FindNode<IntegerNode>(identifierName).SetValue(value);
            }
        }

        private static void WriteROI(NodeMap nodeMap, CameraProperty property)
        {
            if (property.ReadOnly)
                return;
            try
            {
                // Get the current ROI
                var x = nodeMap.FindNode<IntegerNode>("OffsetX").Value();
                var y = nodeMap.FindNode<IntegerNode>("OffsetY").Value();
                var w = nodeMap.FindNode<IntegerNode>("Width").Value();
                var h = nodeMap.FindNode<IntegerNode>("Height").Value();

                // Get the minimum ROI
                var x_min = nodeMap.FindNode<IntegerNode>("OffsetX").Minimum();
                var y_min = nodeMap.FindNode<IntegerNode>("OffsetY").Minimum();
                var w_min = nodeMap.FindNode<IntegerNode>("Width").Minimum();
                var h_min = nodeMap.FindNode<IntegerNode>("Height").Minimum();

                // Set the minimum ROI. This removes any size restrictions due to previous ROI settings
                nodeMap.FindNode<IntegerNode>("OffsetX").SetValue(x_min);
                nodeMap.FindNode<IntegerNode>("OffsetY").SetValue(y_min);
                nodeMap.FindNode<IntegerNode>("Width").SetValue(w_min);
                nodeMap.FindNode<IntegerNode>("Height").SetValue(h_min);

                // Get the maximum ROI values
                var x_max = nodeMap.FindNode<IntegerNode>("OffsetX").Maximum();
                var y_max = nodeMap.FindNode<IntegerNode>("OffsetY").Maximum();
                var w_max = nodeMap.FindNode<IntegerNode>("Width").Maximum();
                var h_max = nodeMap.FindNode<IntegerNode>("Height").Maximum();

                // Get the increment
                var x_inc = nodeMap.FindNode<IntegerNode>("OffsetX").Increment();
                var y_inc = nodeMap.FindNode<IntegerNode>("OffsetY").Increment();
                var w_inc = nodeMap.FindNode<IntegerNode>("Width").Increment();
                var h_inc = nodeMap.FindNode<IntegerNode>("Height").Increment();

                // New ROI values
                var x_new = x;
                var y_new = y;
                var w_new = w;
                var h_new = h;
                if(property.Identifier == "offsetX")
                    x_new = int.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
                else if (property.Identifier == "offsetY")
                    y_new = int.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
                else if (property.Identifier == "width")
                    w_new = int.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
                else if (property.Identifier == "height")
                    h_new = int.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
                

                // Check that the ROI parameters are within their valid range
                if ((x_new % x_inc) > 0 || (y_new % y_inc) > 0 || (w_new % w_inc) > 0 || (h_new % h_inc) > 0)
                {
                    // adjust offset and size parameters to be divisible by their increment or break
                    x_new = x;
                    y_new = y;
                    w_new = w;
                    h_new = h;
                }
                if ((x_new < x_min) || (y_new < y_min) || (x_new > x_max) || (y_new > y_max))
                {
                    // adjust the offsets to be within the valid range or break
                    x_new = x;
                    y_new = y;
                    w_new = w;
                    h_new = h;
                }
                if ((w_new < w_min) || (h_new < h_min) || ((x_new + w_new) > w_max) || ((y_new + h_new) > h_max))
                {
                    // adjust the ROI to be within the valid bounds or break
                    x_new = x;
                    y_new = y;
                    w_new = w;
                    h_new = h;
                }

                // Set the valid ROI
                nodeMap.FindNode<IntegerNode>("OffsetX").SetValue(x_new);
                nodeMap.FindNode<IntegerNode>("OffsetY").SetValue(y_new);
                nodeMap.FindNode<IntegerNode>("Width").SetValue(w_new);
                nodeMap.FindNode<IntegerNode>("Height").SetValue(h_new);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while writing IDS camera ROI. {0}.", e.Message);
            }
        }

        private static void WriteFramerate(NodeMap nodeMap, CameraProperty property)
        {
            if (property.ReadOnly)
                return;
            double value = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            double min = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            double max = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            value = FixValue(value, min, max);
            nodeMap.FindNode<FloatNode>("AcquisitionFrameRate").SetValue(value);
        }

        private static void WriteExposure(NodeMap nodeMap, CameraProperty property)
        {
            if (property.ReadOnly)
                return;
            double value = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            double min = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            double max = double.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            value = FixValue(value, min, max);
            nodeMap.FindNode<FloatNode>("ExposureTime").SetValue(value);
        }

        private static long FixValue(long value, long min, long max, long step)
        {
            value = Math.Min(max, Math.Max(min, value));

            long remainder = value % step;
            if (remainder > 0)
                value = value - remainder;

            return value;
        }

        private static double FixValue(double value, double min, double max)
        {
            return Math.Min(max, Math.Max(min, value));
        }

        private static void WriteGain(NodeMap nodeMap, CameraProperty property)
        {
            if (property.ReadOnly)
                return;
            int value = (int)float.Parse(property.CurrentValue, CultureInfo.InvariantCulture);
            // Before accessing Gain, make sure GainSelector is set correctly
            // Set GainSelector to "AnalogAll"
            nodeMap.FindNode<EnumerationNode>("GainSelector").SetCurrentEntry("AnalogAll");
            nodeMap.FindNode<FloatNode>("Gain").SetValue(value);
        }

    }
}