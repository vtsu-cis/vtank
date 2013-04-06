/*!
    \file   VTankOptions.cs
    \brief  Class that holds all available VTank Options
    \author (C) Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Windows.Forms;
using VAkos;

namespace Client.src.config
{
    using Microsoft.Xna.Framework.Input;
    using System.IO;

    public class Options
    {
        #region VideoOptions
        public struct VideoOptions
        {
            public String Resolution;
            public Boolean Windowed;
            public String TextureQuality;
            public String AntiAliasing;
            public Boolean ShadingEnabled;
        }

        #endregion

        #region AudioOptions
        public struct AudioOptions
        {
            public Sound ambientSound;
            public Sound backgroundSound;
        }
        #endregion

        #region Sound Options
        public struct Sound
        {
            public Int32 Volume;
            public Boolean Muted;
        }
        #endregion

        #region Game Play Options
        public struct GamePlayOptions
        {
            public Boolean ShowNames;
            public Boolean ProfanityFilter;
            public String InterfacePlugin;
            public String DirectionalPointer;
        }
        #endregion

        #region Key Bindings
        public struct KeyBindings
        {
            public Keys Forward;
            public Keys Backward;
            public Keys RotateLeft;
            public Keys RotateRight;
            //public Keys FirePrimary;
            //public Keys FireSecondary;
            public Keys Menu;
            public Keys Minimap;
            public Keys Score;
            public Keys Camera;
            public String Pointer;
        }
        #endregion

        #region Members

        public VideoOptions Video;
        public AudioOptions Audio;
        public GamePlayOptions GamePlay;
        public KeyBindings KeySettings;

        public String ServerAddress;
        public Int32 ServerPort;
        public String DefaultAccount;
        public String MapsFolder;
        public ClientVersion Version;

        #endregion

        #region Version
        /// <summary>
        /// Struct holding the version numbers for the client.
        /// </summary>
        public struct ClientVersion
        {
            int major;
            int minor;
            int build;
            int revision;

            public ClientVersion(int _major, int _minor, int _build, int _revision)
            {
                major = _major;
                minor = _minor;
                build = _build;
                revision = _revision;
            }

            public override string ToString()
            {
                return String.Format("{0}.{1}.{2}.{3}", major, minor, build, revision);
            }

            /// <summary>
            /// Convert a client version to it's VTankObject equivalent.
            /// </summary>
            /// <param name="version"></param>
            /// <returns></returns>
            public static VTankObject.Version ToIceVersion(ClientVersion version)
            {
                return new VTankObject.Version(
                    version.major, version.minor, version.build, version.revision);
            }
        }

        /// <summary>
        /// Convert a version string (e.g. "0.0.0.1" or "0.0.1" or "0.1") to a
        /// VTankObject.Version object. 
        /// </summary>
        /// <param name="p">String to convert.</param>
        /// <returns>Version</returns>
        private static ClientVersion StringToVersion(string _version)
        {
            string[] version = _version.Split('.');

            int[] final_version = { 0, 0, 0, 0 };

            for (int i = 0; i < version.Length; i++)
            {
                final_version[i] = Int32.Parse(version[i]);
            }

            return new ClientVersion(
                final_version[0], final_version[1], final_version[2], final_version[3]);
        }
        #endregion

        #region Constructors

        public Options()
        {
            Video = getDefaultVideoOptions();
            Audio = getDefaultAudioOptions();
            GamePlay = getDefaultGamePlayOptions();
            KeySettings = getDefaultKeyBindings();
        }

        #endregion

        #region Default Options Init
        public static VideoOptions getDefaultVideoOptions()
        {

            return new VideoOptions
            {
                Resolution = "800x600",
                Windowed = true,
                TextureQuality = "High",
                AntiAliasing = "Off",
                ShadingEnabled = true,
            };
        }

        public static Sound getDefaultSoundOptions()
        {
            return new Sound
            {
                Volume = 5,
                Muted = false
            };
        }

        public static AudioOptions getDefaultAudioOptions()
        {
            return new AudioOptions
            {
                ambientSound = new Sound() { Muted = false, Volume = 5 },
                backgroundSound = new Sound() { Muted = false, Volume = 5 }
            };
        }

        public static GamePlayOptions getDefaultGamePlayOptions()
        {

            return new GamePlayOptions
            {
                ShowNames = true,
                ProfanityFilter = false,
                InterfacePlugin = "default"
            };
        }

        public static KeyBindings getDefaultKeyBindings()
        {
            return new KeyBindings
            {
                Forward = Keys.W,
                Backward = Keys.S,
                RotateLeft = Keys.A,
                RotateRight = Keys.D,
                //FirePrimary = Keys.LButton,
                //FireSecondary = Keys.RButton,
                Menu = Keys.Escape,
                Minimap = Keys.M,
                Score = Keys.Tab,
                Camera = Keys.Space,
                Pointer = "crosshair_green_reddot"
            };
        }

        #endregion

        #region Static Methods for Read/Write files

        public static string GetConfigFilePath()
        {
            string path = String.Format(@"{0}\VTank\", Environment.GetEnvironmentVariable("APPDATA"));
            string configFile = "Client.exe.config";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return String.Format("{0}{1}", path, configFile);
        }

        public static void WriteOptions(Options options)
        {
            KeysConverter kc = new KeysConverter();
            string filename = GetConfigFilePath();
            Xmlconfig config = new Xmlconfig();

            config.Settings["ServerAddress"].Value = options.ServerAddress;
            config.Settings["ServerPort"].intValue = options.ServerPort;
            config.Settings["DefaultAccount"].Value = options.DefaultAccount;
            config.Settings["MapsFolder"].Value = options.MapsFolder;
            config.Settings["options/video/Resolution"].Value = options.Video.Resolution;
            config.Settings["options/video/Windowed"].boolValue = options.Video.Windowed;
            config.Settings["options/video/TextureQuality"].Value = options.Video.TextureQuality;
            config.Settings["options/video/AntiAliasing"].Value = options.Video.AntiAliasing;
            config.Settings["options/video/ShadingEnabled"].boolValue = options.Video.ShadingEnabled;
            config.Settings["options/audio/ambientSound/Volume"].intValue = options.Audio.ambientSound.Volume;
            config.Settings["options/audio/ambientSound/Muted"].boolValue = options.Audio.ambientSound.Muted;
            config.Settings["options/audio/backgroundSound/Volume"].intValue = options.Audio.backgroundSound.Volume;
            config.Settings["options/audio/backgroundSound/Muted"].boolValue = options.Audio.backgroundSound.Muted;
            config.Settings["options/gameplay/ShowNames"].boolValue = options.GamePlay.ShowNames;
            config.Settings["options/gameplay/ProfanityFilter"].boolValue = options.GamePlay.ProfanityFilter;
            config.Settings["options/gameplay/InterfacePlugin"].Value = options.GamePlay.InterfacePlugin;
            config.Settings["options/keybindings/Forward"].Value = kc.ConvertToString(options.KeySettings.Forward);
            config.Settings["options/keybindings/Backward"].Value = kc.ConvertToString(options.KeySettings.Backward);
            config.Settings["options/keybindings/RotateLeft"].Value = kc.ConvertToString(options.KeySettings.RotateLeft);
            config.Settings["options/keybindings/RotateRight"].Value = kc.ConvertToString(options.KeySettings.RotateRight);
            //config.Settings["options/keybindings/FirePrimary"].Value = kc.ConvertToString(options.KeySettings.FirePrimary);
            //config.Settings["options/keybindings/FireSecondary"].Value = kc.ConvertToString(options.KeySettings.FireSecondary);
            config.Settings["options/keybindings/Menu"].Value = kc.ConvertToString(options.KeySettings.Menu);
            config.Settings["options/keybindings/Minimap"].Value = kc.ConvertToString(options.KeySettings.Minimap);
            config.Settings["options/keybindings/Score"].Value = kc.ConvertToString(options.KeySettings.Score);
            config.Settings["options/keybindings/Camera"].Value = kc.ConvertToString(options.KeySettings.Camera);
            config.Settings["options/keybindings/Pointer"].Value = options.KeySettings.Pointer;

            /*if (!System.IO.File.Exists(filename))
            {
                System.IO.FileStream stream = System.IO.File.Create(filename);
                stream.Close();
            }
            
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = filename;

            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(
                map, ConfigurationUserLevel.None);

            foreach (String key in ConfigurationManager.AppSettings.AllKeys)
            {
                configuration.AppSettings.Settings[key].Value =
                    ConfigurationManager.AppSettings[key];
            }

            configuration.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");*/

            if (!config.Commit())
                config.Save(filename);
        }

        public static Options ReadOptions()
        {
            bool doCommit = false;
            Options options = new Options();
            KeysConverter kc = new KeysConverter();

            VideoOptions defaultVideo = getDefaultVideoOptions();
            AudioOptions defaultAudio = getDefaultAudioOptions();
            GamePlayOptions defaultGame = getDefaultGamePlayOptions();
            KeyBindings defaultKeys = getDefaultKeyBindings();

            string filename = GetConfigFilePath();
            if (!File.Exists(filename))
                doCommit = true;
            Xmlconfig config = new Xmlconfig(filename, true);
            try
            {
                if (config.Settings.Name == "configuration")
                {
                    // Old configuration file. Port to new configuration type.
                    config.NewXml("xml");
                    doCommit = true;
                    ConvertFromLegacyConfig(config, filename);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Warning: Your old configuration settings have been lost due to an unforeseen error.",
                    "Old configuration settings lost!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            options.ServerAddress = Get(config, "ServerAddress", "glacier2a.cis.vtc.edu");
            options.ServerPort = int.Parse(Get(config, "ServerPort", "4063"));
            options.DefaultAccount = Get(config, "DefaultAccount");
            options.MapsFolder = Get(config, "MapsFolder", "maps");
            options.Video.Resolution = Get(config, "options/video/Resolution", defaultVideo.Resolution);
            options.Video.Windowed = bool.Parse(Get(config, "options/video/Windowed", defaultVideo.Windowed.ToString()));
            options.Video.TextureQuality = Get(config, "options/video/TextureQuality", defaultVideo.TextureQuality);
            options.Video.AntiAliasing = Get(config, "options/video/AntiAliasing", defaultVideo.AntiAliasing);
            options.Video.ShadingEnabled = bool.Parse(Get(config, "options/video/ShadingEnabled", defaultVideo.ShadingEnabled.ToString()));
            options.Audio.ambientSound.Volume = int.Parse(Get(config, "options/audio/ambientSound/Volume", defaultAudio.ambientSound.Volume.ToString()));
            options.Audio.ambientSound.Muted = bool.Parse(Get(config, "options/audio/ambientSound/Muted", defaultAudio.ambientSound.Muted.ToString()));
            options.Audio.backgroundSound.Volume = int.Parse(Get(config, "options/audio/backgroundSound/Volume", defaultAudio.backgroundSound.Volume.ToString()));
            options.Audio.backgroundSound.Muted = bool.Parse(Get(config, "options/audio/backgroundSound/Muted", defaultAudio.backgroundSound.Muted.ToString()));
            options.GamePlay.ShowNames = bool.Parse(Get(config, "options/gameplay/ShowNames", defaultGame.ShowNames.ToString()));
            options.GamePlay.ProfanityFilter = bool.Parse(Get(config, "options/gameplay/ProfanityFilter", defaultGame.ProfanityFilter.ToString()));
            options.GamePlay.InterfacePlugin = Get(config, "options/gameplay/InterfacePlugin", defaultGame.InterfacePlugin);
            options.KeySettings.Forward = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/Forward", defaultKeys.Forward.ToString()));
            options.KeySettings.Backward = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/Backward", defaultKeys.Backward.ToString()));
            options.KeySettings.RotateLeft = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/RotateLeft", defaultKeys.RotateLeft.ToString()));
            options.KeySettings.RotateRight = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/RotateRight", defaultKeys.RotateRight.ToString()));
            //options.KeySettings.FirePrimary = (Keys)kc.ConvertFromString(Get(config, "options/keysettings/FirePrimary", ""));
            //options.KeySettings.FireSecondary = (Keys)kc.ConvertFromString(Get(config, "options/keysettings/FireSecondary", ""));
            options.KeySettings.Menu = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/Menu", defaultKeys.Menu.ToString()));
            options.KeySettings.Minimap = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/Minimap", defaultKeys.Minimap.ToString()));
            options.KeySettings.Score = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/Score", defaultKeys.Score.ToString()));
            options.KeySettings.Camera = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/Camera", defaultKeys.Camera.ToString()));
            options.KeySettings.Pointer = Get(config, "options/keybindings/Pointer", defaultKeys.Pointer);

            if (doCommit)
                WriteOptions(options);

            return options;
           
        }

        /// <summary>
        /// Gets the property of the given key.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <returns>The property at the key, or an empty string if it cannot be found.</returns>
        private static string Get(Xmlconfig config, string name)
        {
            return Get(config, name, "");
        }

        /// <summary>
        /// Gets the property of the given key.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="defaultValue">Value to use if it cannot be found.</param>
        /// <returns></returns>
        private static string Get(Xmlconfig config, string name, string defaultValue)
        {
            if (config.Settings.Contains(name))
                return config.Settings[name].Value;

            config.Settings[name].Value = defaultValue;
            return defaultValue;
        }

        /// <summary>
        /// Convert an old type of configuration file to a new one, porting the settings over.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="filename"></param>
        private static void ConvertFromLegacyConfig(Xmlconfig config, string filename)
        {
            try
            {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = filename;

                Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(
                    map, ConfigurationUserLevel.None);
                ConfigurationManager.RefreshSection("appSettings");

                foreach (String key in ConfigurationManager.AppSettings.AllKeys)
                {
                    if (key.Contains("ClientSettings"))
                        continue;
                    string newKey = key.Replace('.', '/');
                    try
                    {
                        config.Settings[newKey].Value = configuration.AppSettings.Settings[key].Value;
                    }
                    catch (NullReferenceException)
                    {
                        // Occasionally some obsolete fields will be removed from a config file but leave behind
                        // traces. This watches for that case.
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                string stackTrace = ex.StackTrace.ToString();
                Console.WriteLine("{0}/{1}", errorMessage, stackTrace);
                // TODO: Should we do something here?
            }
        }
        #endregion
    }
}
