/*!
    \file   VTankOptions.cs
    \brief  Class that holds all available VTank Options
    \author (C) Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace Client.src.utils
{
    using VAkos;
    using System.IO;

    public class VTankOptions
    {
        #region VideoOptions
        public struct VideoOptions
        {
            public String Resolution;
            public String Windowed;
            public String TextureQuality;
            public String AntiAliasing;
            public String ShadingEnabled;
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
            public String Volume;
            public String Muted;
        }
        #endregion

        #region Game Play Options
        public struct GamePlayOptions
        {
            public String ShowNames;
            public String ProfanityFilter;
            public String InterfacePlugin;
        }
        #endregion

        #region Key Bindings
        public struct KeyBindings
        {
            public Keys Forward;
            public Keys Backward;
            public Keys RotateLeft;
            public Keys RotateRight;
            public Keys FirePrimary;
            public Keys FireSecondary;
            public Keys Menu;
            public Keys Minimap;
            public Keys Score;
            public Keys Camera;
            public String Pointer;
        }
        #endregion

        #region Members

        public VideoOptions videoOptions;
        public AudioOptions audioOptions;
        public GamePlayOptions gamePlayOptions;
        public KeyBindings keyBindings;

        public String ServerAddress;
        public String ServerPort;
        public String DefaultAccount;
        public String MapsFolder;

        #endregion

        #region Constructors

        public VTankOptions()
        {
            videoOptions = getDefaultVideoOptions();
            audioOptions = getDefaultAudioOptions();
            gamePlayOptions = getDefaultGamePlayOptions();
            keyBindings = getDefaultKeyBindings();

            ServerAddress = "glacier2a.cis.vtc.edu";
            ServerPort = "4063";
            DefaultAccount = "";
            MapsFolder = "maps";
        }

        public VTankOptions(String sAddress, String sPort, String defaultAccount, String mapsFolder): this()
        {
            ServerAddress = sAddress;
            ServerPort = sPort;
            DefaultAccount = defaultAccount;
            MapsFolder = mapsFolder;
        }

        #endregion

        #region Default Options Init
        public static VideoOptions getDefaultVideoOptions()
        {

            return new VideoOptions
            {
                Resolution = "800x600",
                Windowed = "true",
                TextureQuality = "High",
                AntiAliasing = "Off",
                ShadingEnabled = "true",
            };
        }

        public static Sound getDefaultSoundOptions()
        {
            return new Sound
            {
                Volume = "5",
                Muted = "false"
            };
        }

        public static AudioOptions getDefaultAudioOptions()
        {
            return new AudioOptions
            {
                ambientSound = new Sound() { Muted = "false", Volume = "5" },
                backgroundSound = new Sound() { Muted = "false", Volume = "5" }
            };
        }

        public static GamePlayOptions getDefaultGamePlayOptions()
        {

            return new GamePlayOptions
            {
                ShowNames = "true",
                ProfanityFilter = "false",
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
                FirePrimary = Keys.LButton,
                FireSecondary = Keys.RButton,
                Menu = Keys.Escape,
                Minimap = Keys.M,
                Score = Keys.Tab,
                Camera = Keys.Space,
                Pointer = "crosshair_green_reddot"
            };
        }

        #endregion

        #region Static Methods for Read/Write files

        /// <summary>
        /// Gets the directory in which the client configuration file is stored.
        /// </summary>
        /// <returns></returns>
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

        public static void WriteOptions(VTankOptions options)
        {
            KeysConverter kc = new KeysConverter();
            string filename = GetConfigFilePath();
            Xmlconfig config = new Xmlconfig();

            config.Settings["ServerAddress"].Value = options.ServerAddress;
            config.Settings["ServerPort"].Value = options.ServerPort;
            config.Settings["DefaultAccount"].Value = options.DefaultAccount;
            config.Settings["MapsFolder"].Value = options.MapsFolder;
            config.Settings["options/video/Resolution"].Value = options.videoOptions.Resolution;
            config.Settings["options/video/Windowed"].Value = options.videoOptions.Windowed;
            config.Settings["options/video/TextureQuality"].Value = options.videoOptions.TextureQuality;
            config.Settings["options/video/AntiAliasing"].Value = options.videoOptions.AntiAliasing;
            config.Settings["options/video/ShadingEnabled"].Value = options.videoOptions.ShadingEnabled;
            config.Settings["options/audio/ambientSound/Volume"].Value = options.audioOptions.ambientSound.Volume;
            config.Settings["options/audio/ambientSound/Muted"].Value = options.audioOptions.ambientSound.Muted;
            config.Settings["options/audio/backgroundSound/Volume"].Value = options.audioOptions.backgroundSound.Volume;
            config.Settings["options/audio/backgroundSound/Muted"].Value = options.audioOptions.backgroundSound.Muted;
            config.Settings["options/gameplay/ShowNames"].Value = options.gamePlayOptions.ShowNames;
            config.Settings["options/gameplay/ProfanityFilter"].Value = options.gamePlayOptions.ProfanityFilter;
            config.Settings["options/gameplay/InterfacePlugin"].Value = options.gamePlayOptions.InterfacePlugin;
            config.Settings["options/keybindings/Forward"].Value = kc.ConvertToString(options.keyBindings.Forward);
            config.Settings["options/keybindings/Backward"].Value = kc.ConvertToString(options.keyBindings.Backward);
            config.Settings["options/keybindings/RotateLeft"].Value = kc.ConvertToString(options.keyBindings.RotateLeft);
            config.Settings["options/keybindings/RotateRight"].Value = kc.ConvertToString(options.keyBindings.RotateRight);
            //config.Settings["options/keybindings/FirePrimary"].Value = kc.ConvertToString(options.keyBindings.FirePrimary);
            //config.Settings["options/keybindings/FireSecondary"].Value = kc.ConvertToString(options.keyBindings.FireSecondary);
            config.Settings["options/keybindings/Menu"].Value = kc.ConvertToString(options.keyBindings.Menu);
            config.Settings["options/keybindings/Minimap"].Value = kc.ConvertToString(options.keyBindings.Minimap);
            config.Settings["options/keybindings/Score"].Value = kc.ConvertToString(options.keyBindings.Score);
            config.Settings["options/keybindings/Camera"].Value = kc.ConvertToString(options.keyBindings.Camera);
            config.Settings["options/keybindings/Pointer"].Value = options.keyBindings.Pointer;
            
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

        public static VTankOptions ReadOptions()
        {
            bool doCommit = false;
            VTankOptions options = new VTankOptions();
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

            // Configure options.
            // Format: options.Key = Get(config, "Key", "DefaultValue");
            options.ServerAddress = Get(config, "ServerAddress", "glacier2a.cis.vtc.edu");
            options.ServerPort = Get(config, "ServerPort", "4063");
            options.DefaultAccount = Get(config, "DefaultAccount");
            options.MapsFolder = Get(config, "MapsFolder", "maps");
            options.videoOptions.Resolution = Get(config, "options/video/Resolution", defaultVideo.Resolution);
            options.videoOptions.Windowed = Get(config, "options/video/Windowed", defaultVideo.Windowed);
            options.videoOptions.TextureQuality = Get(config, "options/video/TextureQuality", defaultVideo.TextureQuality);
            options.videoOptions.AntiAliasing = Get(config, "options/video/AntiAliasing", defaultVideo.AntiAliasing);
            options.videoOptions.ShadingEnabled = Get(config, "options/video/ShadingEnabled", defaultVideo.ShadingEnabled);
            options.audioOptions.ambientSound.Volume = Get(config, "options/audio/ambientSound/Volume", defaultAudio.ambientSound.Volume);
            options.audioOptions.ambientSound.Muted = Get(config, "options/audio/ambientSound/Muted", defaultAudio.ambientSound.Muted);
            options.audioOptions.backgroundSound.Volume = Get(config, "options/audio/backgroundSound/Volume", defaultAudio.backgroundSound.Volume);
            options.audioOptions.backgroundSound.Muted = Get(config, "options/audio/backgroundSound/Muted", defaultAudio.backgroundSound.Muted);
            options.gamePlayOptions.ShowNames = Get(config, "options/gameplay/ShowNames", defaultGame.ShowNames);
            options.gamePlayOptions.ProfanityFilter = Get(config, "options/gameplay/ProfanityFilter", defaultGame.ProfanityFilter);
            options.gamePlayOptions.InterfacePlugin = Get(config, "options/gameplay/InterfacePlugin", defaultGame.InterfacePlugin);
            options.keyBindings.Forward = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/Forward", defaultKeys.Forward.ToString()));
            options.keyBindings.Backward = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/Backward", defaultKeys.Backward.ToString()));
            options.keyBindings.RotateLeft = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/RotateLeft", defaultKeys.RotateLeft.ToString()));
            options.keyBindings.RotateRight = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/RotateRight", defaultKeys.RotateRight.ToString()));
            //options.keyBindings.FirePrimary = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/FirePrimary", ""));
            //options.keyBindings.FireSecondary = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/FireSecondary", ""));
            options.keyBindings.Menu = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/Menu", defaultKeys.Menu.ToString()));
            options.keyBindings.Minimap = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/Minimap", defaultKeys.Minimap.ToString()));
            options.keyBindings.Score = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/Score", defaultKeys.Score.ToString()));
            options.keyBindings.Camera = (Keys)kc.ConvertFromString(Get(config, "options/keybindings/Camera", defaultKeys.Camera.ToString()));
            options.keyBindings.Pointer = Get(config, "options/keybindings/Pointer", defaultKeys.Pointer);

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
                throw;
                // TODO: Should we do something here?
            }
        }
        #endregion
    }
}
