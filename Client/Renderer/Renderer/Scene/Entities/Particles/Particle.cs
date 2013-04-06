using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Renderer.SceneTools.Entities.Particles
{
    /// <summary>
    /// A PointSprite Particle Object containing information on position, velocity, tint, size, and creationTime
    /// </summary>
    public struct Particle
    {
        public const int SizeInBytes = 32;

        public Vector3 Position;
        public Vector3 Velocity;
        public Color RandomValues;
        public float Time;

        public static readonly VertexElement[] VertexElements =
        {
            new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default,
                                    VertexElementUsage.Position, 0),//Position

            new VertexElement(0, 12, VertexElementFormat.Vector3, VertexElementMethod.Default,
                                     VertexElementUsage.Normal, 0),//Velocity

            new VertexElement(0, 24, VertexElementFormat.Color, VertexElementMethod.Default,
                                     VertexElementUsage.Color, 0),//RandomValues

            new VertexElement(0, 28, VertexElementFormat.Single, VertexElementMethod.Default,
                                     VertexElementUsage.TextureCoordinate, 0),//creationTime
        };
    }
}
