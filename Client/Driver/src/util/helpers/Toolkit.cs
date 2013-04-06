using System;
using VTankObject;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;
using System.IO;
using System.Collections.Generic;

namespace Client.src.util
{
    /// <summary>
    /// The Utility class provides a set of static functions meant to ease simple tasks.
    /// </summary>
    public static class Toolkit
    {
        /// <summary>
        /// Returns a string representation from a GameMode enumeration
        /// </summary>
        /// <param name="gameMode"></param>
        /// <returns>string</returns>
        public static string GameModeToString(GameMode gameMode)
        {
            switch (gameMode)
            {
                case GameMode.DEATHMATCH:
                    return "Deathmatch";
                case GameMode.TEAMDEATHMATCH:
                    return "Team Deathmatch";
                case GameMode.CAPTURETHEFLAG:
                    return "Capture the Flag";
                case GameMode.CAPTURETHEBASE:
                    return "Capture the Base";
            }

            return "Unknown";
        }

        /// <summary>
        /// Formats a time (in seconds) to appear as what one would see on a clock
        /// (e.g. M:SS) except in minutes:seconds, not hours:minutes..
        /// </summary>
        /// <param name="time">Time in seconds.</param>
        /// <returns>formatted string</returns>
        public static string FormatTime(double time)
        {
            int minutes = (int)(time / 60);
            int seconds = (int)(time % 60);

            if (minutes <= 0 && seconds <= 0)
            {
                return "0:00";
            }

            return String.Format("{0}:{1:00}", minutes, seconds);
        }

        /// <summary>
        /// Creates a VTankColor object from a XNA color object
        /// </summary>
        /// <param name="color">XNA color</param>
        /// <returns>corresponding VTankColor</returns>
        public static VTankObject.VTankColor GetVTankColor(Color color)
        {
            VTankColor newColor = new VTankColor();
            newColor.red = color.R;
            newColor.blue = color.B;
            newColor.green = color.G;
            return newColor;
        }

        /// <summary>
        /// Creates a VTankColor object from a XNA color object
        /// </summary>
        /// <param name="color">XNA color</param>
        /// <returns>corresponding VTankColor</returns>
        public static Color GetColor(VTankObject.VTankColor color)
        {
            return new Color(color.red / 255f, color.green / 255f, color.blue / 255f);
        }

        public static Color GetColor(Color defaultColor, GameSession.Alliance team)
        {
            switch (team)
            {
                case GameSession.Alliance.BLUE:
                    return Color.Blue;
                case GameSession.Alliance.RED:
                    return Color.Red;
                default:
                    return defaultColor;
            }
        }

        /// <summary>
        /// Get a skin texture, or a default skin if the given texture name does not exist.
        /// </summary>
        /// <param name="skinName"></param>
        /// <returns></returns>
        public static Texture2D GetSkin(string skinName)
        {
            Texture2D result;
            try
            {
                result = ServiceManager.Resources.Load<Texture2D>(
                    Constants.DEFAULT_SKIN_DIR + skinName);
            }
            catch (Exception)
            {
                result = ServiceManager.Resources.Load<Texture2D>(
                    Constants.DEFAULT_SKIN_DIR + Constants.DEFAULT_SKIN);
            }

            return result;
        }

        /// <summary>
        /// Obtains a list of skins.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSkinList()
        {
            List<string> result = new List<string>();
            DirectoryInfo dir = new DirectoryInfo(@"Content\" + Constants.DEFAULT_SKIN_DIR);
            FileInfo[] fileList = dir.GetFiles("*.xnb");
            foreach (FileInfo file in fileList)
            {
                string extensionlessName = file.Name.Replace(".xnb", "");

                if (!extensionlessName.Contains("dents"))
                    result.Add(extensionlessName);
            }

            return result;
        }

        /// <summary>
        /// Obtain a list of *all* skins. This is useful for loading skins into memory.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetFullSkinList()
        {
            List<string> result = new List<string>();
            DirectoryInfo dir = new DirectoryInfo(@"Content\" + Constants.DEFAULT_SKIN_DIR);
            FileInfo[] fileList = dir.GetFiles("*.xnb");
            foreach (FileInfo file in fileList)
            {
                string extensionlessName = file.Name.Replace(".xnb", "");

                result.Add(extensionlessName);
            }

            return result;
        }

        /// <summary>
        /// Get a list of event models.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetEventList()
        {
            List<string> result = new List<string>();
            DirectoryInfo dir = new DirectoryInfo(@"Content\models\events");
            FileInfo[] fileList = dir.GetFiles("*.xnb");
            foreach (FileInfo file in fileList)
            {
                string extensionlessName = file.Name.Replace(".xnb", "");

                result.Add(extensionlessName);
            }

            return result;
        }

        /// <summary>
        /// Check locally for the current version of the client.
        /// </summary>
        /// <param name="versionFile">Path to the version file</param>
        /// <returns>The version as a string, or null if not found.</returns>
        public static string GetClientVersion(string versionFile)
        {
            StreamReader reader = new StreamReader(versionFile);
            string line, version = null;

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                if (line.StartsWith("version"))
                {
                    version = line.Split('=')[1];
                }
            }

            return version;
        }

        /// <summary>
        /// Get the major revision number from a version string.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        public static int GetMajorRevisionFromVersion(string version)
        {
            string _version = version;
            if (version.StartsWith("R"))
            {
                _version.TrimStart('R');
            }

            char [] chars =  _version.ToCharArray();
            string majorRevision = "";

            foreach (char _char in chars)
            {
                if (Char.IsDigit(_char))
                    majorRevision += _char;
            }

            return int.Parse(majorRevision);
        }

        /// <summary>
        /// Get the minor revision from a version string
        /// </summary>
        /// <param name="version">The version string</param>
        /// <returns>The minor revision letter(s)</returns>
        public static string GetMinorRevisionFromVersion(string version)
        {
            string _version = version;
            if (_version.StartsWith("R"))
            {
                _version = _version.TrimStart('R');
            }

            char[] chars = _version.ToCharArray();
            string minorRevision = "";

            foreach (char _char in chars)
            {
                if (Char.IsDigit(_char))
                    continue;

                if (Char.IsLetter(_char))
                    minorRevision += _char;
            }

            return minorRevision;
        }
    }
}
