using System;
using System.IO;
using System.Collections.Generic;

namespace VTankBotRunner.Util
{
    /// <summary>
    /// Configurable options for the bot runner.
    /// </summary>
    internal static class BotRunnerOptions
    {
        #region Members
        internal static readonly string DEFAULT_PATH;
        private static string configFilePath;
        private static List<Property> properties;
        #endregion

        #region Static Methods
        /// <summary>
        /// Configure default values.
        /// </summary>
        static BotRunnerOptions()
        {
            DEFAULT_PATH = String.Format(@"{0}\VTank\Bot\options.config", Environment.GetEnvironmentVariable("APPDATA"));
            properties = new List<Property>();
        }

        /// <summary>
        /// Set the value of a key to the given value. If the key does not exist, a new property is created.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal static void SetValue(string key, object value)
        {
            Property prop = properties.Find((x) => { return x.Key == key; });
            if (prop == null)
                properties.Add(new Property(key, value));
            else
                prop.Value = value;
        }

        /// <summary>
        /// Get the value of the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static string GetValue(string key)
        {
            Property prop = properties.Find((x) => { return x.Key == key; });
            if (prop == null)
                return null;

            return prop.Value.ToString();
        }

        /// <summary>
        /// Get the value of the given key, convert it to a boolean, and return it.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static bool GetValueBool(string key)
        {
            Property prop = properties.Find((x) => { return x.Key == key; });
            if (prop == null)
                return false;

            bool result;
            if (bool.TryParse(prop.Value.ToString(), out result))
                return result;
            
            throw new InvalidOperationException("Property at key '" + key + "' is not a boolean!");
        }

        /// <summary>
        /// Get the value of the given key, convert it to an integer, then return it.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static Int32 GetValueInt32(string key)
        {
            Property prop = properties.Find((x) => { return x.Key == key; });
            if (prop == null)
                return 0;

            Int32 result;
            if (Int32.TryParse(prop.Value.ToString(), out result))
                return result;
            
            throw new InvalidOperationException("Property at key '" + key + "' is not an integer!");
        }

        /// <summary>
        /// Get the value of the given key, convert it to a float, and return it.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static float GetValueFloat(string key)
        {
            Property prop = properties.Find((x) => { return x.Key == key; });
            if (prop == null)
                return 0;

            float result;
            if (float.TryParse(prop.Value.ToString(), out result))
                return result;

            throw new InvalidOperationException("Property at key '" + key + "' is not a float!");
        }

        /// <summary>
        /// Read options using the default configuration file path.
        /// </summary>
        internal static void ReadOptions()
        {
            ReadOptions(DEFAULT_PATH);
        }

        /// <summary>
        /// Read options from the given file.
        /// </summary>
        /// <param name="path">Path to the file, including the file itself.</param>
        internal static void ReadOptions(string path)
        {
            configFilePath = path;
            FileStream input = null;
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                if (!fileInfo.Exists || fileInfo.Length == 0)
                {
                    string directories = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(directories);
                    GenerateDefaultOptions(path);
                }

                properties.Clear();
                input = File.OpenRead(path);
                StreamReader reader = new StreamReader(input);
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!GrammarIsCorrect(line))
                        continue;

                    Property property = Property.FromString(line);
                    properties.Add(property);
                }
            }
            finally
            {
                try
                {
                    if (input != null)
                        input.Close();
                }
                catch { }
            }
        }

        /// <summary>
        /// Check to see if the grammar of a property is correct.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>True if the grammar of the line is correct; false otherwise.</returns>
        private static bool GrammarIsCorrect(string line)
        {
            if (String.IsNullOrEmpty(line))
                return false;

            string _line = line.Trim();
            if (_line.StartsWith("#") || !_line.Contains("="))
                return false;

            if (_line.Split(new string[] { "=" }, 2, StringSplitOptions.RemoveEmptyEntries).Length != 2)
                return false;

            return true;
        }

        /// <summary>
        /// Generate some default options.
        /// </summary>
        /// <param name="path"></param>
        private static void GenerateDefaultOptions(string path)
        {
            List<Property> defaults = new List<Property>();
            defaults.Add(new Property("AutoBotRemoveEnabled", true));

            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter output = new StreamWriter(File.Create(path)))
            {
                foreach (Property prop in defaults)
                    output.WriteLine(prop.ToString());
            }
        }

        /// <summary>
        /// Save options to the last known configuration path.
        /// </summary>
        internal static void SaveOptions()
        {
            if (configFilePath == null)
                configFilePath = DEFAULT_PATH;

            SaveOptions(configFilePath);
        }

        /// <summary>
        /// Save options to the given configuration file.
        /// </summary>
        /// <param name="path">Path to write the contents to, including the file itself.</param>
        internal static void SaveOptions(string path)
        {
            FileStream output = null;
            try
            {
                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(path.Substring(0, path.LastIndexOf("\\")));
                    output = File.Create(path);
                }
                else
                {
                    output = File.OpenWrite(path);
                }

                StreamWriter writer = new StreamWriter(output);
                foreach (Property prop in properties)
                {
                    writer.WriteLine(prop.ToString());
                }
            }
            finally
            {
                try
                {
                    if (output != null)
                        output.Close();
                }
                catch { }
            }
        }
        #endregion
    }
}
