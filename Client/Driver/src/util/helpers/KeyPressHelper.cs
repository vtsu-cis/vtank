using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Client.src.util
{
    /// <summary>
    /// KeyPressHelper is a utility class which assists programmers in checking if a
    /// key has been "typed" (pressed, then released). The advantage of this class is
    /// that it helps check if a key was pressed only once -- it doesn't count repeated
    /// key events.
    /// </summary>
    public class KeyPressHelper
    {
        #region Members
        private static Dictionary<Keys, KeyPressHelper> list;
        private Keys key;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the key tracked by this helper.
        /// </summary>
        public Keys Key
        {
            get
            {
                return key;
            }

            set
            {
                if (value != key)
                {
                    HeldDown = false;
                }

                key = value;
            }
        }

        /// <summary>
        /// Gets whether or not the key is currently being held down.
        /// </summary>
        public bool HeldDown
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new key press helper.
        /// </summary>
        /// <param name="key">Key to keep track of.</param>
        public KeyPressHelper(Keys key)
        {
            Key = key;
        }

        /// <summary>
        /// Initialize static members.
        /// </summary>
        static KeyPressHelper()
        {
            list = new Dictionary<Keys, KeyPressHelper>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Check if the key was pressed.
        /// </summary>
        /// <returns>True if the key was pressed (once), false otherwise.</returns>
        public bool IsPressed()
        {
            bool pressed = false;

            bool keyIsDown = Keyboard.GetState().IsKeyDown(Key);
            if (keyIsDown && !HeldDown)
            {
                HeldDown = true;
                pressed = true;
            }
            else if (!keyIsDown && HeldDown)
            {
                HeldDown = false;
            }

            return pressed;
        }
        #endregion

        #region Static
        /// <summary>
        /// Start tracking a key.
        /// </summary>
        /// <param name="key">Key to track.</param>
        public static void Track(Keys key)
        {
            if (!list.ContainsKey(key))
            {
                list.Add(key, new KeyPressHelper(key));
            }
        }

        /// <summary>
        /// Clear all tracked keys.
        /// </summary>
        public static void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// Check if a key is pressed.
        /// </summary>
        /// <param name="key">Key to check.</param>
        /// <returns>True if the key was pressed for one frame;
        /// false otherwise.</returns>
        public static bool IsPressed(Keys key)
        {
            Track(key);
            return list[key].IsPressed();
        }

        /// <summary>
        /// Remove a key from the list of tracked keys.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        /// <returns>True if the key was removed; false otherwise.</returns>
        public static bool Remove(Keys key)
        {
            return list.Remove(key);
        }
        #endregion
    }
}
