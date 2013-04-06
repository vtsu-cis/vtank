using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Renderer.SceneTools.Effects;

namespace Renderer.SceneTools.Entities
{
    /// <summary>
    /// Represents an object made out of 'VertexPositionColor' types of vertices.
    /// </summary>
    public class VertexColorGroup : Entity
    {
        #region Members
        protected VertexBuffer vertexBuffer;
        protected static VertexDeclaration vertDeclaration;
        protected List<VertexPositionColor> vertices;
        protected Texture2D texture;
        #endregion

        #region Constructor
        /// <summary>
        /// Construct the vertex color group using an empty list of vertices.
        /// </summary>
        public VertexColorGroup()
            : this(new List<VertexPositionColor>())
        {
        }

        /// <summary>
        /// Construct the vertex color group using a pre-defined list of vertices.
        /// </summary>
        /// <param name="_verts">Vertices that make up the image.</param>
        public VertexColorGroup(List<VertexPositionColor> _verts)
        {
            updatable = false;
            vertices = _verts;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Once all verticies in this object are set, call ready() to prepare this entity for drawing.
        /// </summary>
        public void Ready()
        {
            vertexBuffer = new VertexBuffer(GraphicOptions.graphics.GraphicsDevice,
               vertices.Count * VertexPositionColor.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColor>(vertices.ToArray());

            if (vertDeclaration == null)
            {
                vertDeclaration = new VertexDeclaration(GraphicOptions.graphics.GraphicsDevice,
                    VertexPositionColor.VertexElements);
            }

            List<Vector3> points = new List<Vector3>();
            foreach (VertexPositionColor vertex in vertices)
            {
                points.Add(vertex.Position);
            }
            bounds = BoundingSphere.CreateFromPoints(points);
            position = bounds.Center;
        }

        /// <summary>
        /// Draws all vertices in the vertex buffer.
        /// </summary>
        /// 
        public override void Draw(EffectTechnique technique)
        {
            UniversalEffect effect = RendererAssetPool.UniversalEffect;

            if (technique == effect.Techniques.UseDefault)
                effect.CurrentTechnique = RendererAssetPool.UniversalEffect.Techniques.Colored;
            else
                effect.CurrentTechnique = technique;

            effect.ColorParameters.TransparencyEnabled = this.TransparencyEnabled && GraphicOptions.TransparentWalls;
            
           effect.Begin();
           foreach (EffectPass pass in effect.CurrentTechnique.Passes)//TODO1 remove this when shading i
           {
               pass.Begin();
               GraphicsDevice device = GraphicOptions.graphics.GraphicsDevice;
               device.VertexDeclaration = vertDeclaration;
               device.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionColor.SizeInBytes);
               device.DrawUserPrimitives<VertexPositionColor>(
                   PrimitiveType.TriangleStrip, vertices.ToArray(), 0, 2);
               pass.End();
           }
           effect.End();  
           
        }

        #endregion
    }
}
