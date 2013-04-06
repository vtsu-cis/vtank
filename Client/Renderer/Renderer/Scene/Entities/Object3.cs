using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer.SceneTools.Effects;
using Renderer.Utils;
using SkinnedModel;


namespace Renderer.SceneTools.Entities
{
    /// <summary>
    /// Any 3D object to be displayed in game will extend this class. You must call Initialize() last in a constructor for a Renderer3DObject
    /// </summary>
    public class Object3 : Entity
    {
        #region Members
        private Model model;
        protected Color meshColor;
        protected Entity trackTo;
        protected Vector3 mountPosition;
        protected AnimationPlayer animationPlayer;
        protected Dictionary<string, AnimationClip> animations;
        protected List<Matrix> originalAbsoluteTransforms;
        protected Matrix rootTransform;
        protected Matrix[] boneTransforms;
        protected BoundingBox tileBox;
        protected Dictionary<string, Texture2D> textureMap;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new Renerer3DObject from a model and takes care of initialization.
        /// </summary>
        /// <param name="model">The model to create the object from</param>
        /// <param name="_position"> The world position to give the Object3.</param>
        public Object3(Model _model, Vector3 _position)
        {
            Model = _model;
            CalculateBoundingSphere();
            position = _position;
            bounds.Center = position;
            meshColor = Color.White;
            textureMap = new Dictionary<string, Texture2D>();
            Initialize();
        }

        /// <summary>
        /// Initializes the bone transform matrix and the bounding box as big as one tile.
        /// </summary>
        public void Initialize()
        {
            boneTransforms = new Matrix[model.Bones.Count];
            tileBox = new BoundingBox(Position - new Vector3(64, 64, 0), Position + new Vector3(64, 64, 64 * 4));
        }

        #endregion

        #region Properties
        /// <summary>
        /// Getter for the model that represents this object
        /// </summary>
        public Model Model
        {
            get { return model; }
            set
            {
                model = value;
                RendererAssetPool.UniversalEffect.RemapModel(model);
                SkinningData data = model.Tag as SkinningData;
                if (data != null)
                {
                    animationPlayer = new AnimationPlayer(data);
                    animations = data.AnimationClips;
                    originalAbsoluteTransforms = data.OriginalAbsoluteTransforms;
                    animationPlayer.StartClip(animations["Default Take"]);
                }
                else
                {
                    animationPlayer = null;
                    animations = null;
                    originalAbsoluteTransforms = null;
                }
                CalculateBoundingSphere(); 
            }
        }
    
        /// <summary>
        /// Sets the color that all meshes names color will draw
        /// </summary>
        public Color MeshColor
        {
            get
            {
                return meshColor;
            }

            set
            {
                meshColor = value;
            }
        }

        /// <summary>
        /// returns a box as big as a tile and 4 tiles high centered at the position of the object
        /// </summary>
        public BoundingBox TileBox
        {
            get { return tileBox; }
        }
        #endregion

        #region Animation
        /// <summary>
        /// Adds an animation to the list of animations 
        /// </summary>
        /// <param name="anm"></param>
        public void Animate(string animationName)
        {
            if(animationPlayer != null && animations.ContainsKey(animationName))
            {
                animationPlayer.StartClip(animations[animationName]);
            }
        }
        #endregion

        #region Update/Draw
        /// <summary>
        /// Updates the position and rotation of this object.
        /// </summary>
        public override void Update(Boolean updateToggle)
        {
            if (HasUpdated != updateToggle && updatable)
            {
                base.Update(updateToggle);

                rootTransform = rotation * Matrix.CreateTranslation(position);
                if (animationPlayer != null)
                {
                    animationPlayer.Update(
                        TimeSpan.FromSeconds(GraphicOptions.FrameLength), true, rotation * Matrix.CreateTranslation(Position));
                }
                else
                {
                    model.Root.Transform = rootTransform;
                    model.CopyAbsoluteBoneTransformsTo(boneTransforms);
                }
               
                HasUpdated = updateToggle;
            }
        }

        /// <summary>
        /// Draws the model using Absolute Bone Positioning
        /// </summary>
        public override void Draw(EffectTechnique technique)
        {
            UniversalEffect effect = RendererAssetPool.UniversalEffect;
            if (technique == effect.Techniques.UseDefault)
                effect.CurrentTechnique = effect.Techniques.TexturedWrap;
            else
                effect.CurrentTechnique = technique;

            //Set transparent if blocking view
            effect.ColorParameters.TransparencyEnabled = this.TransparencyEnabled && GraphicOptions.TransparentWalls;

            bool usingAnimation = false;
            if (animationPlayer != null)
            {
                effect.SkinningParameters.Bones = animationPlayer.GetSkinTransforms();
                effect.SkinningParameters.IsSkinnedModel = true;
                usingAnimation = true;
            } 
            foreach (ModelMesh mesh in model.Meshes)
            {
                if (animationPlayer == null)
                {
                    effect.WorldParameters.WorldMatrix = boneTransforms[mesh.ParentBone.Index];
                }

                bool switchColor = false;

                if (mesh.Name.Contains("Color"))
                {
                    effect.ColorParameters.ColorToUse = meshColor;
                    effect.ColorParameters.ForceColorToStored = true;
                    switchColor = true;
                }

                effect.TextureParameters.Texture = mesh.Tag as Texture2D;

                Texture2D mappedTexture;
                if (textureMap.TryGetValue(mesh.Name, out mappedTexture))
                {
                    effect.TextureParameters.Texture = mappedTexture;
                }

                /*
                foreach (KeyValuePair<string, Texture2D> mapping in textureMap)
                {
                    if (mapping.Key == mesh.Name)
                    {
                        effect.TextureParameters.Texture = mapping.Value;
                        break;
                    }
                }*/

                mesh.Draw();

                if (switchColor == true)
                    effect.ColorParameters.ForceColorToStored = false;
            }

            if (usingAnimation)
                effect.SkinningParameters.IsSkinnedModel = false;
            effect.WorldParameters.WorldMatrix = Matrix.Identity;
        }
        #endregion

        /// <summary>
        /// Check if this Object3 has a given mesh.
        /// </summary>
        /// <param name="meshName">The name of the mesh</param>
        /// <returns>True if the mesh exists, false otherwise.</returns>
        private bool MeshExists(string meshName)
        {
            ModelBoneCollection.Enumerator collection = this.model.Bones.GetEnumerator();

            while(collection.MoveNext())
            {
                if (collection.Current.Name.Equals(meshName))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the position of the mount mesh on this model.
        /// </summary>
        /// <param name="meshName">The name of the mount mesh.</param>
        /// <returns>The position of the mount mesh.</returns>
        public Vector3 MountPosition(string meshName)
        {
            if ( this.MeshExists(meshName) )
            {
                int index = model.Meshes[meshName].ParentBone.Index;
                if (animationPlayer == null)
                {
                    mountTransform = Matrix.Identity;
                    mountTransform = boneTransforms[index];
                    return mountTransform.Translation;
                }
                else
                {
                    return (originalAbsoluteTransforms[index] * rootTransform).Translation;
                }
            }
            
            else
            {
                return position; 
            }

        }

        /// <summary>
        /// Calculated a single bounding sphere for this model, call whenever the model changes.
        /// </summary>
        private void CalculateBoundingSphere()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere meshBoundingSphere = mesh.BoundingSphere;
                BoundingSphere.CreateMerged(ref bounds, ref meshBoundingSphere, out bounds);
            }
        }

        /// <summary>
        /// Add a mapping to apply the given skin to the given mesh.
        /// </summary>
        /// <param name="meshName">Name of the mesh to apply the skin to.</param>
        /// <param name="skin">Skin to apply.</param>
        /// <returns>True if the mesh exists and was added successfully; false otherwise.</returns>
        public bool AddMeshSkin(string meshName, Texture2D skin)
        {
            if (!MeshExists(meshName))
            {
                return false;
            }

            if (textureMap.ContainsKey(meshName))
            {
                textureMap.Remove(meshName);
            }

            textureMap[meshName] = skin;

            return true;
        }

        /// <summary>
        /// Clear all applied mesh mappings.
        /// </summary>
        public void ClearMeshSkins()
        {
            textureMap.Clear();
        }
    }
}
