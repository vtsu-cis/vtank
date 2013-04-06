using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace Renderer.SceneTools.Effects
{

    public class UETechniques
    {
        public EffectTechnique TexturedClamp;
        public EffectTechnique TexturedWrap;
        public EffectTechnique Colored;
        public EffectTechnique ShadowMap;
        public EffectTechnique LazerBeam;
        public EffectTechnique UseDefault;

        public UETechniques(Effect effect)
        {
            TexturedClamp = effect.Techniques["TexturedClamp"];
            TexturedWrap = effect.Techniques["TexturedWrap"];
            Colored = effect.Techniques["SimpleColor"];
            ShadowMap = effect.Techniques["ShadowMap"];
            LazerBeam = effect.Techniques["Lazer"];
            UseDefault = effect.Techniques["oops"];
        }
    }
    public class UECameraParameters
    {
        private Effect universalEffect;
        public UECameraParameters(Effect effect)
        {
            universalEffect = effect;
        }

        public Vector3 Position
        {
            set
            {
                universalEffect.Parameters["xCameraPosition"].SetValue(value);
            }
        }
        public Matrix View
        {
            set
            {
                universalEffect.Parameters["xCameraView"].SetValue(value);
            }
        }
        public Matrix Projection
        {
            set
            {
                universalEffect.Parameters["xCameraProjection"].SetValue(value);
            }
        }
    }
    public class UETextureParameters
    {
        private Effect universalEffect;
        public UETextureParameters(Effect effect)
        {
            universalEffect = effect;
        }

        public Texture2D Texture
        {
            set { universalEffect.Parameters["xTexture"].SetValue(value); }
        }
    }
    public class UEShadowParameters
    {
        private Effect universalEffect;
        public UEShadowParameters(Effect effect)
        {
            universalEffect = effect;
        }

        public Texture2D ShadowMapTexture
        {
            set 
            { 
                universalEffect.Parameters["xShadowMapTexture"].SetValue(value);
            }
        }
        public bool EnableShadows
        {
            set { universalEffect.Parameters["xEnableShadows"].SetValue(value); }
        }
    }
    public class UELightParameters
    {
        private Effect universalEffect;
        public UELightParameters(Effect effect)
        {
            universalEffect = effect;
        }

        public bool EnableLighting
        {
            set 
            { 
                universalEffect.Parameters["xEnableLighting"].SetValue(value);
                if (!value)
                    universalEffect.Parameters["xEnableShadows"].SetValue(false);
            }
        }
        public Vector3 Position
        {
            set
            {
                universalEffect.Parameters["xLightPosition"].SetValue(value);
            }
        }
        public Matrix View
        {
            set
            {
                universalEffect.Parameters["xLightView"].SetValue(value);
            }
        }
        public Matrix Projection
        {
            set
            {
                universalEffect.Parameters["xLightProjection"].SetValue(value);
            }
        }
        public Color Color
        {
            set { universalEffect.Parameters["xLightColor"].SetValue(value.ToVector4()); }
        }
        public Color AmbientColor
        {
            set { universalEffect.Parameters["xAmbient"].SetValue(value.ToVector4()); }
        }
        public Vector3 FogSeedPosition
        {
            set { universalEffect.Parameters["xFogSeedPosition"].SetValue(value); }
        }
    }
    public class UEColorParameters
    {
        private Effect universalEffect;
        public UEColorParameters(Effect effect)
        {
            universalEffect = effect;
        }

        public Color ColorToUse
        {
            set { universalEffect.Parameters["xMeshColor"].SetValue(value.ToVector4()); }
        }
        public bool ForceColorToStored
        {
            set
            {
                universalEffect.Parameters["xForceMeshColor"].SetValue(value);
            }
        }
        public bool TransparencyEnabled
        {
            set { universalEffect.Parameters["xEnableTransparency"].SetValue(value); }
        }
    }
    public class UEMaterialParameters
    {
        private Effect universalEffect;
        public UEMaterialParameters(Effect effect)
        {
            universalEffect = effect;
        }

        public Color AmbientColor
        {
            set { universalEffect.Parameters["xAmbient"].SetValue(value.ToVector4()); }
        }
    }
    public class UEWorldParameters
    {
        private Effect universalEffect;
        public UEWorldParameters(Effect effect)
        {
            universalEffect = effect;
        }

        public Matrix WorldMatrix
        {
            set
            {
                universalEffect.Parameters["xWorld"].SetValue(value);
            }

        }
        public float CurrentTime
        {
            set { universalEffect.Parameters["xCurrentTime"].SetValue(value); }
        }
    }
    public class UESkinningParameters
    {
        private Effect universalEffect;
        public UESkinningParameters(Effect effect)
        {
            universalEffect = effect;
        }

        public bool IsSkinnedModel
        {
            set { universalEffect.Parameters["xSkinnedModel"].SetValue(value); }
        }
        public Matrix[] Bones
        {
            set { universalEffect.Parameters["xBones"].SetValue(value); }
        }
    }

    public class UniversalEffect
    {
        private Effect effect;

        public UECameraParameters CameraParameters;
        public UETextureParameters TextureParameters;
        public UEShadowParameters ShadowParameters;
        public UELightParameters LightParameters;
        public UEColorParameters ColorParameters;
        public UEMaterialParameters MaterialParameters;
        public UEWorldParameters WorldParameters;
        public UESkinningParameters SkinningParameters;
        public EffectTechnique CurrentTechnique
        {
            get { return effect.CurrentTechnique; }
            set 
            {
                if (effect.CurrentTechnique != value)
                {
                    effect.CurrentTechnique = value;
                }
            }
        }
        public UETechniques Techniques;

        private UniversalEffect(Effect universalEffect)
        {
            effect = universalEffect;
            CameraParameters = new UECameraParameters(effect);
            TextureParameters = new UETextureParameters(effect);
            ShadowParameters = new UEShadowParameters(effect);
            LightParameters = new UELightParameters(effect);
            ColorParameters = new UEColorParameters(effect);
            MaterialParameters = new UEMaterialParameters(effect);
            WorldParameters = new UEWorldParameters(effect);
            SkinningParameters = new UESkinningParameters(effect);
            Techniques = new UETechniques(effect);
        }

        public void Begin()
        {
            effect.Begin();
        }
        public void End() 
        { effect.End(); }

        public void RemapModel(Model model)
        {
            RemapModel(model, effect);
        }
        /// <summary>
        /// Changes the effect of a model and stored it's original mesh textures in their respective Tag fields.
        /// </summary>
        /// <param name="model">The model to remap.</param>
        /// <param name="effect">The effect to remap to.</param>
        private static void RemapModel(Model model, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                try
                {
                    if(model.Meshes[mesh.Name].Effects.Count > 0 && model.Meshes[mesh.Name].Effects[0] is BasicEffect)
                         mesh.Tag = ((BasicEffect)model.Meshes[mesh.Name].Effects[0]).Texture as Texture2D;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;//.Clone(GraphicOptions.graphics.GraphicsDevice);
                }
            }
        }
        public static UniversalEffect LoadEffect(ContentManager content)
        {
            return new UniversalEffect(content.Load<Effect>("effects/UniversalShader"));
        }
    }
}
