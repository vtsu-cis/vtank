using System;
using System.Collections.Generic;
using System.Text;

namespace Renderer.Utils
{

    #region Structs
    /// <summary>
    /// A Parameter for an REffect
    /// </summary>
    public struct Parameter
    {
        public string name;
        public ParameterType type;
    }
    #endregion

    #region Enums
    /// <summary>
    /// The type of parameter, will be used by the renderer to pass the correct variable to the effect.
    /// </summary>
    public enum ParameterType
    {
        VIEW,
        PROJECTION,
        WORLD,
        TEXTURE,
        POSITION, //TODO vector3
        COLOR, //TODO vector4 4th is alpha
        TEXTURE_COORDINATES, //TODO
        NORMAL, //TODO
        AMBIENT_COLOR,
        DIFFUSE_COLOR,
        SPECULAR_COLOR,
        POWER,
        SUN_POSITION,
        RIGHT_WINDOW_POSITION,
        LEFT_WINDOW_POSITION
    }

    /// <summary>
    /// An aribitrary ranking system to track level of detail. 
    /// </summary>
    public enum TextureQuality
    {
        HIGH,
        MIDDLE,
        LOW
    }
    #endregion
}
