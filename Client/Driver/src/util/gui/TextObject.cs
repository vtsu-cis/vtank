using System;
using System.Collections.Generic;
using System.Text;
using Renderer.SceneTools.Entities;
using Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;


namespace Client.src.util
{
    public class TextObject : VertexGroup
    {
        #region Members
        private int width;
        private int height;
        private string text;
        private bool flag = false;
        #endregion

        #region Constructor
        public TextObject()
        {
            Technique= RendererAssetPool.UniversalEffect.Techniques.TexturedClamp;
            this.CastsShadow = false;
        }
        #endregion

        #region Public methods

        public void SetText(string text)
        {
            this.text = text;
            width = (int)ServiceManager.Game.Font.MeasureString(text).X * 2;
            height = (int)ServiceManager.Game.Font.MeasureString(text).Y * 2;

            GraphicsDevice device = Renderer.GraphicOptions.graphics.GraphicsDevice;

            const int numberOfLevels = 1;
            const int multisampleQuality = 0;
            RenderTarget2D renderTarget = new RenderTarget2D(device, width,
                height, numberOfLevels, SurfaceFormat.Color,
                MultiSampleType.None, multisampleQuality, RenderTargetUsage.PreserveContents);

            device.SetRenderTarget(0, renderTarget);
            device.Clear(Color.TransparentWhite);
            ServiceManager.Game.Batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Texture, SaveStateMode.SaveState);
            //ServiceManager.Game.Batch.Begin();
            ServiceManager.Game.Batch.DrawString(ServiceManager.Game.Font, text, Vector2.Zero, Color.Black, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0f);
            ServiceManager.Game.Batch.End();

            device.SetRenderTarget(0, null);
            texture = renderTarget.GetTexture();

            //texture.Save("C:\\img.png", ImageFileFormat.Png);
            
            AdjustVertices();
            Ready();
        }

        public void AdjustVertices()
        {
            Matrix camWorld = Matrix.Invert(GraphicOptions.CurrentCamera.View);
            Vector3 right = camWorld.Right;
            Vector3 down = camWorld.Down;
            right.Normalize(); down.Normalize();
            right = right * width;
            down = down * height;
            Vector3 tl = position - right / 2;
            Vector3 tr = position + right / 2;
            Vector3 bl = tl + down;
            Vector3 br = tr + down;

            vertices.Clear();
            Vector3 normal = Vector3.Cross(down, right);
            vertices.Add(new VertexPositionNormalTexture(bl, normal, Vector2.UnitY));
            vertices.Add(new VertexPositionNormalTexture(tl, normal, Vector2.Zero));
            vertices.Add(new VertexPositionNormalTexture(tr, normal, Vector2.UnitX));
            vertices.Add(new VertexPositionNormalTexture(br, normal, Vector2.One));
            vertices.Add(new VertexPositionNormalTexture(bl, normal, Vector2.UnitY));
            vertices.Add(new VertexPositionNormalTexture(tr, normal, Vector2.UnitX));

            boundingBox = new BoundingBox(tl, br);
            Ready();
        }
        #endregion

        public override void Update(bool updateToggle)
        {
            if (updateToggle != HasUpdated)
            {
                base.Update(updateToggle);
                
                AdjustVertices();
                Ready();
            }
        }

        public override void Draw(EffectTechnique technique)
        {
            if (!flag)
                flag = true;
            else
            {
                flag = false;
                return;
            }

            if (ServiceManager.Game.Options.GamePlay.ShowNames)
            {
                /*GraphicsDevice device = GraphicOptions.graphics.GraphicsDevice;
                Vector3 offset = new Vector3(0, 0, 0);
                Vector3 pPosition = Vector3.Zero;// position + offset;
                Matrix projection = GraphicOptions.CurrentCamera.Projection;
                Matrix view = GraphicOptions.CurrentCamera.View;
                Matrix world = Matrix.CreateTranslation(position + offset);//Matrix.Identity;// Matrix.Invert(GraphicOptions.CurrentCamera.View);
                
                Vector3 projectedPosition = device.Viewport.Project(pPosition, projection, view, world);
                
                Vector2 newPosition = new Vector2(projectedPosition.X, projectedPosition.Y);
                newPosition.X = newPosition.X - (width / 2.0f);
                newPosition.Y = newPosition.Y - (height / 2.0f);

                ServiceManager.Game.Batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                ServiceManager.Game.Batch.DrawString(ServiceManager.Game.Font, text, newPosition, Color.Black);
                ServiceManager.Game.Batch.End();*/

                GraphicsDevice device = GraphicOptions.graphics.GraphicsDevice;
                device.RenderState.AlphaBlendEnable = true;
                device.RenderState.SourceBlend = Blend.SourceAlpha;
                device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                device.RenderState.AlphaFunction = CompareFunction.Greater;
                device.RenderState.SeparateAlphaBlendEnabled = false;
                device.RenderState.AlphaTestEnable = true;

                base.Draw(technique);

                device.RenderState.AlphaBlendEnable = false;
                device.RenderState.SourceBlend = Blend.One;
                device.RenderState.DestinationBlend = Blend.Zero;
            }
        }
    }
}
