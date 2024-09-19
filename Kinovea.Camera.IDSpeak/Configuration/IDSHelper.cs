using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;
using peak.core;
using peak.core.nodes;
using peak.ipl;

namespace Kinovea.Camera.IDSpeak
{
    public static class IDSHelper
    {
        /// <summary>
        /// Returns the intersection between the camera's supported stream formats and Kinovea supported stream formats.
        /// </summary>
        public static List<IDSEnum> GetSupportedStreamFormats(NodeMap nodeMap)
        {
            List<IDSEnum> list = GetKinoveaSupportedStreamFormats();

            // Remove formats not supported by that specific camera.
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!isStreamFormatSupported(nodeMap, list[i].Value))
                    list.RemoveAt(i);
            }

            return list;
        }

        private static List<IDSEnum> GetKinoveaSupportedStreamFormats()
        {
            // Hard coded list of known good formats.
            // TODO: support JPG for the XS model.
            List<IDSEnum> list = new List<IDSEnum>();
            list.Add(new IDSEnum((int)PixelFormatName.Mono8, "Mono 8"));
            list.Add(new IDSEnum((int)PixelFormatName.BGR8, "RGB 24"));
            list.Add(new IDSEnum((int)PixelFormatName.BGRa8, "RGB 32"));
            return list;
        }

        private static bool isStreamFormatSupported(NodeMap nodeMap, int format)
        {
            // Get a list of all available entries of PixelFormat
            var allEntries = nodeMap.FindNode<EnumerationNode>("PixelFormat").Entries();
            List<string> availableEntries = new List<string>();
            for (int i = 0; i < allEntries.Count(); ++i)
            {
                if ((allEntries[i].AccessStatus() != NodeAccessStatus.NotAvailable)
                        && (allEntries[i].AccessStatus() != NodeAccessStatus.NotImplemented))
                {
                    availableEntries.Add(allEntries[i].SymbolicValue());
                }
            }
            bool supported = availableEntries.Contains(((PixelFormatName)format).ToString());
            
            return supported;
        }

        public static int ReadCurrentStreamFormat(NodeMap nodeMap)
        {
            // Determine the current entry of PixelFormat
            long value = nodeMap.FindNode<EnumerationNode>("PixelFormat").CurrentEntry().Value();

            return (int)value;
        }

        public static void WriteStreamFormat(NodeMap nodeMap, int format)
        {
            nodeMap.FindNode<EnumerationNode>("PixelFormat").SetCurrentEntry(format);
        }

        public static ImageFormat GetImageFormat(NodeMap nodeMap)
        {
            var inputPixelFormat = (PixelFormatName)ReadCurrentStreamFormat(nodeMap);
            return ConvertImageFormat(inputPixelFormat);
        }

        public static float GetFramerate(NodeMap nodeMap)
        {
            double value = nodeMap.FindNode<FloatNode>("AcquisitionFrameRate").Value();
            return (float)value;
        }
        public static bool IsMono(PixelFormatName pixelFormatName)
        {
            return pixelFormatName == PixelFormatName.Mono8 ||
                   pixelFormatName == PixelFormatName.Mono10 ||
                   pixelFormatName == PixelFormatName.Mono12 ||
                   pixelFormatName == PixelFormatName.Mono16 ||
                   pixelFormatName == PixelFormatName.Mono10p ||
                   pixelFormatName == PixelFormatName.Mono12p ||
                   pixelFormatName == PixelFormatName.Mono10g40IDS ||
                   pixelFormatName == PixelFormatName.Mono12g24IDS;
        }

        public static bool IsBayer8(PixelFormatName pixelFormatName)
        {
            return pixelFormatName == PixelFormatName.BayerBG8 ||
                   pixelFormatName == PixelFormatName.BayerGB8 ||
                   pixelFormatName == PixelFormatName.BayerGR8 ||
                   pixelFormatName == PixelFormatName.BayerRG8;
        }

        private static ImageFormat ConvertImageFormat(PixelFormatName inputPixelFormat)
        {
            ImageFormat format = ImageFormat.None;

            switch (inputPixelFormat)
            {
                case PixelFormatName.BGR8:
                    format = ImageFormat.RGB24;
                    break;
                case PixelFormatName.BGRa8:
                    format = ImageFormat.RGB32;
                    break;
                case PixelFormatName.Mono8:
                    format = ImageFormat.Y800;
                    break;
                case PixelFormatName.RGB8:
                case PixelFormatName.RGBa8:
                default:
                    format = ImageFormat.None;
                    break;
            }

            return format;
        }
    }
}
