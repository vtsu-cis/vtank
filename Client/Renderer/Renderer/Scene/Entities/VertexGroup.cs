using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Renderer.SceneTools.Effects;
using Microsoft.Xna.Framework;
using Renderer.Utils;

namespace Renderer.SceneTools.Entities
{
    /// <summary>
    /// This class represents a group of VertexPositionNormalTexture objects.
    /// </summary>
    public class VertexGroup : Entity
    {
        #region Members
        protected VertexBuffer vertexBuffer;
        protected static VertexDeclaration vertDeclaration;
        protected List<VertexPositionNormalTexture> vertices;
        protected Texture2D texture;
        protected BoundingBox boundingBox;
        #endregion

        public EffectTechnique Technique
        {
            get;
            set;
        }

        #region Constructor
        /// <summary>
        /// Creates a new VertexGroup with no Texture.
        /// </summary>
        public VertexGroup()
            : this(null, new List<VertexPositionNormalTexture>())
        {
        }

        /// <summary>
        /// Creates a new VertexGroup with a texture.
        /// </summary>
        /// <param name="tex">The texture to apply to this VertexGroup.</param>
        public VertexGroup(Texture2D tex)
            : this(tex, new List<VertexPositionNormalTexture>())
        {
        }

        /// <summary>
        /// Creates a new VertexGroup with a texture and a list of vertices.
        /// </summary>
        /// <param name="tex">The texture to apply to this VertexGroup.</param>
        /// <param name="_verts">The list of VertexPositionNormalTexture objects.</param>
        public VertexGroup(Texture2D tex, List<VertexPositionNormalTexture> _verts)
        {
            texture = tex;
            updatable = false;
            vertices = _verts;
            CastsShadow = true;
        }
        #endregion

        /// <summary>
        /// Adds a VertexPositionNormalTexture to the vertex list.
        /// </summary>
        /// <param name="vertex"></param>
        public void Add(VertexPositionNormalTexture vertex)
        {
            vertices.Add(vertex);
        }

        /// <summary>
        /// Once all verticies in this object are set, call ready() to prepare this entity for drawing.
        /// </summary>
        public void Ready()
        {
            vertexBuffer = new VertexBuffer(GraphicOptions.graphics.GraphicsDevice,
               vertices.Count * VertexPositionNormalTexture.SizeInBytes,
               BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices.ToArray());
            vertDeclaration = new VertexDeclaration(GraphicOptions.graphics.GraphicsDevice,
                VertexPositionNormalTexture.VertexElements);

            List<Vector3> points = new List<Vector3>();
            foreach (VertexPositionNormalTexture vertex in vertices)
            {
                points.Add(vertex.Position);
            }
            this.boundingBox = BoundingBox.CreateFromPoints(points) ;
            this.BoundingSphere = BoundingSphere.CreateFromBoundingBox(boundingBox);
        }

        /// <summary>
        /// Draws the Tile on screen
        /// Each tile is made up of two triangles that form a rectangle
        /// The vertices determine where the tile will be drawn
        /// the indexes determine how the texture will be applied to the tile
        /// </summary>
        /// 
        public override void Draw(EffectTechnique technique)
        {
            UniversalEffect effect = RendererAssetPool.UniversalEffect;

            if (technique == effect.Techniques.UseDefault)
                effect.CurrentTechnique = this.Technique;
            else
                effect.CurrentTechnique = technique;

            effect.ColorParameters.TransparencyEnabled = this.TransparencyEnabled && GraphicOptions.TransparentWalls;
            effect.TextureParameters.Texture = texture;
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)//TODO1 remove this when shading i
            {
                pass.Begin();
                GraphicsDevice device = GraphicOptions.graphics.GraphicsDevice;
                device.VertexDeclaration = vertDeclaration;
                device.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0,
                    vertexBuffer.SizeInBytes / VertexPositionNormalTexture.SizeInBytes / 3);
                pass.End();
            }
            effect.End();
        
        }

        /// <summary>
        /// Gets the bounding box for each tile
        /// The Bounding Box represents an imaginary box
        /// around the tile to determine if objects intersect it
        /// </summary>
        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }

        /// <summary>
        /// Public get for the VertexBuffer that this object uses.
        /// </summary>
        public VertexBuffer VBuffer
        {
            get { return vertexBuffer; }
        }

        /// <summary>
        /// Public get for the vertex list that this object uses.
        /// </summary>
        public int Verticies
        {
            get { return vertices.Count; }
        }

    }
}
