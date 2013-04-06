using System;
using System.Collections.Generic;
using System.Text;

namespace VTankBotRunner.Util
{
    public class Property
    {
        #region Properties
        /// <summary>
        /// Key identifying the property.
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        /// Value of the property.
        /// </summary>
        public object Value
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs a property.
        /// </summary>
        /// <param name="key">Key identifying the property.</param>
        /// <param name="value">Value of the property.</param>
        public Property(string key, object value)
        {
            Key = key;
            Value = value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts the property to a string form. This form can be read back in using the 'FromString' method.
        /// </summary>
        /// <returns>Property taking the following format: "Key=Value"</returns>
        public override string ToString()
        {
            return String.Format("{0}={1}", Key, Value.ToString());
        }

        /// <summary>
        /// Attempts to create a property from a string with the following format:
        /// Key=Value.
        /// </summary>
        /// <param name="line">Line with the property data.</param>
        /// <returns>Property object, or null if the line is invalid.</returns>
        public static Property FromString(string line)
        {
            if (String.IsNullOrEmpty(line))
            {
                return null;
            }

            string[] data = line.Split(new string[] { "=" }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (data.Length != 2)
            {
                return null;
            }

            string key = data[0].Trim();
            object value = data[1].Trim();

            return new Property(key, value);
        }
        #endregion
    }
}
