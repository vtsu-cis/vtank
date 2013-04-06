using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Renderer.SceneTools.Effects;

namespace Renderer.SceneTools.Entities
{
    /// <summary>
    /// This class represents a group of VertexPositionColorTexture objects.
    /// </summary>
    public class VertexPositionColorTextureGroup : Entity
    {
        #region Members
        protected VertexBuffer vertexBuffer;
        protected static VertexDeclaration vertDeclaration;
        protected List<VertexPositionColorTexture> vertices;
        protected Texture2D texture;
        #endregion

        public EffectTechnique Technique
        {
            get;
            set;
        }

        #region Constructor
        /// <summary>
        /// Construct the vertex color group using an empty list of vertices.
        /// </summary>
        public VertexPositionColorTextureGroup()
            : this(new List<VertexPositionColorTexture>())
        {
        }

        /// <summary>
        /// Construct the vertex color group using a pre-defined list of vertices.
        /// </summary>
        /// <param name="_verts">Vertices that make up the image.</param>
        public VertexPositionColorTextureGroup(List<VertexPositionColorTexture> _verts)
        {
            updatable = false;
            vertices = _verts;
            Technique = RendererAssetPool.UniversalEffect.Techniques.Colored;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Once all verticies in this object are set, call ready() to prepare this entity for drawing.
        /// </summary>
        public void Ready()
        {
            vertexBuffer = new VertexBuffer(GraphicOptions.graphics.GraphicsDevice,
               vertices.Count * VertexPositionColorTexture.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices.ToArray());

            if (vertDeclaration == null)
            {
                vertDeclaration = new VertexDeclaration(GraphicOptions.graphics.GraphicsDevice,
                    VertexPositionColorTexture.VertexElements);
            }
        }

        public void Draw(EffectTechnique technique)
        {
            if (vertices.Count > 0)
            {
                UniversalEffect effect = RendererAssetPool.UniversalEffect;
                if (technique == null)
                    effect.CurrentTechnique = this.Technique;
                else
                    effect.CurrentTechnique = technique;

                Ready();
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)//TODO1 remove this when shading i
                {
                    pass.Begin();
                    GraphicsDevice device = GraphicOptions.graphics.GraphicsDevice;
                    device.VertexDeclaration = vertDeclaration;
                    device.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionColorTexture.SizeInBytes);
                    device.DrawUserPrimitives<VertexPositionColorTexture>(
                        PrimitiveType.LineList, vertices.ToArray(), 0, 2);
                    pass.End();
                }
                effect.End();
            }
        }
        #endregion
    }
}
