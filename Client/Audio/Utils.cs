using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Audio
{
    /// <summary>
    /// Collection of static utilities and struct definitions for audio library
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Definition of a sound target
        /// </summary>
        public struct xAudioTarget
        {
            public Vector3 Position;
            public Vector3 Velocity;
        }

        /// <summary>
        /// Metadata for an Audio Category
        /// </summary>
        public struct xAudioCategory
        {
            public String Name;
            public List<String> SoundList;
        }
    }
}
