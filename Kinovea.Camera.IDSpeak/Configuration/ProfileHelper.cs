using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kinovea.Services;
using peak.core;

namespace Kinovea.Camera.IDSpeak
{
    /// <summary>
    /// A helper class to manage import and export of collection of parameters.
    /// The IDS cameras do not automatically save their configuration between sessions.
    /// The API has helper methods to store and read .xml files with serialized parameters.
    /// </summary>
    public static class ProfileHelper
    {
        private static string profilesDirectory = Path.Combine(Software.CameraProfilesDirectory, "IDS");
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static ProfileHelper()
        {
            if (!Directory.Exists(profilesDirectory))
                Directory.CreateDirectory(profilesDirectory);
        }

        public static string GetProfileFilename(string identifier)
        {
            string filename = string.Format("{0}.cset", identifier);
            return Path.Combine(profilesDirectory, filename);
        }

        public static void Save(NodeMap nodeMap, string filename)
        {
            try
            {
                nodeMap.StoreToFile(filename);
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error while saving camera parameter set at {0}.", filename), e);
            }
        }

        public static bool Load(NodeMap nodeMap, string identifier)
        {
            string filename = GetProfileFilename(identifier);
            bool result = false;
            try
            {
                if (File.Exists(filename))
                {
                    log.DebugFormat("Loading IDS camera parameters from {0}.", Path.GetFileName(filename));
                    nodeMap.LoadFromFile(filename);
                    result = true;
                }
                else
                {
                    log.DebugFormat("Camera parameter set not found.");
                }
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error while loading camera parameter set at {0}.", filename), e);
            }

            return result;
        }

        public static void Delete(string identifier)
        {
            string filename = GetProfileFilename(identifier);

            try
            {
                if (File.Exists(filename))
                {
                    log.DebugFormat("Deleting IDS camera parameters at {0}.", Path.GetFileName(filename));
                    File.Delete(filename);
                }
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error while deleting camera parameter set at {0}.", filename), e);
            }
        }

        /// <summary>
        /// Replaces the profile used by Kinovea by an external one.
        /// </summary>
        public static void Replace(string identifier, string sourceFilename)
        {
            string destFilename = GetProfileFilename(identifier);
            try
            {
                //if (File.Exists(destFilename))
                //  File.Delete(filename);
                log.DebugFormat("Replacing IDS camera parameters {0} <- {1}.", Path.GetFileName(destFilename), Path.GetFileName(sourceFilename));
                File.Copy(sourceFilename, destFilename, true);
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error while importing camera parameter set at {0}.", destFilename), e);
            }
        }
    }
}
