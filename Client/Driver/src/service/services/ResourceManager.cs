using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Client.src.service;
using SkinnedModel;

namespace Client.src.util
{
    /// <summary>
    /// The resource manager is responsible for both loading content and caching content. If
    /// this class is the only means for the client to load resources, then the client must not
    /// be able to load the same resource into memory twice -- only one pointer to the resource
    /// should exist.
    /// </summary>
    public class ResourceManager : ContentManager
    {
        #region Resource Class
        /// <summary>
        /// The Resource class wraps a resource to make it easier to store a dynamic type
        /// within the resource manager.
        /// </summary>
        /// <typeparam name="T">A XNA graphics resource.</typeparam>
        protected class Resource
        {
            #region Properties
            /// <summary>
            /// Get the name of the resource.
            /// </summary>
            public string Name
            {
                get;
                private set;
            }

            /// <summary>
            /// Get the resource stored within the wrapper.
            /// </summary>
            public object ResourceObject
            {
                get;
                private set;
            }
            #endregion

            #region Constructors
            /// <summary>
            /// Create the resource. No special operations are performed.
            /// </summary>
            /// <param name="name">Name of the resource.</param>
            /// <param name="resource">The resource object.</param>
            public Resource(string name, object resource)
            {
                Name = name;
                ResourceObject = resource;
            }
            #endregion
        }
        #endregion

        #region Members
        private Dictionary<string, Resource> cache;
        public static readonly string ModelPlaceholder = "placeholder";
        public static readonly string TexturePlaceholder = "placeholder2d";
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current size of the resource cache.
        /// </summary>
        public int Count
        {
            get
            {
                return cache.Count;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create the resource manager, which initializes it's internal content manager.
        /// </summary>
        public ResourceManager()
            : base(ServiceManager.Game.Content.ServiceProvider, 
                   ServiceManager.Game.Content.RootDirectory)
        {
            cache = new Dictionary<string, Resource>(500);

            LoadPlaceholderModels();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Load models or other resources that will be used in the case that a user tries to
        /// load a resource that doesn't exist.
        /// </summary>
        private void LoadPlaceholderModels()
        {
            try 
            {
                Model placeholder = base.Load<Model>("models\\box");
                Resource resource = new Resource(ModelPlaceholder, placeholder);

                cache[ModelPlaceholder] = resource;
            }
            catch (ContentLoadException e) 
            {
                ServiceManager.Game.Console.DebugPrint(
                    "Warning: Unable to load placeholder model: {0}.", e);
            }

            try
            {
                string name = TexturePlaceholder;
                Texture2D placeholder = new Texture2D(ServiceManager.Game.GraphicsDevice, 64, 64, 0,
                    TextureUsage.Tiled, ServiceManager.Game.GraphicsDevice.DisplayMode.Format);
                cache[TexturePlaceholder] = new Resource(name, placeholder);
            }
            catch (ContentLoadException e)
            {
                ServiceManager.Game.Console.DebugPrint(
                    "Warning: Unable to load placeholder texture: {0}.", e);
            }
        }

        /// <summary>
        /// Handle null input. By default, it returns a placeholder model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T HandleNullInput<T>()
        {
            if (typeof(T) == typeof(Model))
                return (T)cache[ModelPlaceholder].ResourceObject;

            if (typeof(T) == typeof(Texture2D))
                return (T)cache[TexturePlaceholder].ResourceObject;

            throw new ArgumentNullException("assetName");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Load a resource into memory if it isn't already in memory, then return that 
        /// resource.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T LoadResource<T>(string path, string assetName, bool usePlaceholderIfNotExist)
        {
            if (String.IsNullOrEmpty(assetName))
            {
                return HandleNullInput<T>();
            }

            if (assetName.EndsWith(".xnb"))
            {
                // The extension is unneeded, so remove it.
                assetName = assetName.Substring(0, assetName.Length - 4);
            }

            // Check if the asset is already in cache. If it is, return it.
            if (cache.ContainsKey(assetName))
            {
                return (T)cache[assetName].ResourceObject;
            }

            if (path.Length > 0 && (!path.EndsWith("\\") || !path.EndsWith("/")))
            {
                path += "\\";
            }
            
            if (assetName.Contains("\\") || assetName.Contains("/"))
            {
                assetName = assetName.Replace('/', '\\');

                // Remove the leading path and add it to the 'path' instead.
                int index = assetName.LastIndexOf('\\');
                path += assetName.Substring(0, index + 1);
                assetName = assetName.Substring(index + 1);
            }

            path = String.Format("{0}{1}", path, assetName);

            // The resource doesn't exist, so load it.
            Resource resource = null;
            T resourceObject = default(T);
            try
            {
                resourceObject = base.Load<T>(path);
                /*if (resourceObject is Texture2D)
                {
                    Texture2D texture = resourceObject as Texture2D;
                    texture.LevelOfDetail = 2;
                }*/

                resource = new Resource(assetName, resourceObject);
            }
            catch (ContentLoadException e)
            {
                ServiceManager.Game.Console.DebugPrint(
                    "Warning: Unable to load model {0} -- loading placeholder instead.", 
                    assetName);
                ServiceManager.Game.Console.DebugPrint(
                    "   Exception details: {0}", e.Message);

                if (!usePlaceholderIfNotExist)
                    throw new KeyNotFoundException("Asset " + assetName + " not found.");

                if (typeof(T) == typeof(Model) && cache.ContainsKey(ModelPlaceholder))
                {
                    return (T)cache[ModelPlaceholder].ResourceObject;
                }
                else if (typeof(T) == typeof(Texture2D) && cache.ContainsKey(TexturePlaceholder))
                {
                    return (T)cache[TexturePlaceholder].ResourceObject;
                }
                else
                {
                    // We have no placeholder model or it's an unsupported type.
                    // The next best thing is to throw an exception.
                    throw new KeyNotFoundException("Asset " + assetName + " not found.");
                }
            }

            cache[assetName] = resource;

            return resourceObject;
        }

        /// <summary>
        /// Load a resource into memory if it isn't already in memory, then return that 
        /// resource.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T LoadResource<T>(string path, string assetName)
        {
            return LoadResource<T>(path, assetName, true);
        }

        /// <summary>
        /// Overrides the content manager's Load method.
        /// </summary>
        /// <typeparam name="T">Type to load. Must be a XNA-supported type.</typeparam>
        /// <param name="assetName">Asset to load, including the path to the asset.</param>
        /// <returns>The loaded object.</returns>
        /// <seealso>ResourceManager#LoadResource</seealso>
        public override T Load<T>(string assetName)
        {
                return LoadResource<T>("", assetName);
        }

        /// <summary>
        /// Helper method for loading a model into memory. If the model was already in memory,
        /// it simply returns that model. It automatically looks into the "models\\" directory.
        /// </summary>
        /// <param name="assetName">Name of the model to load.</param>
        /// <returns>The model object.</returns>
        public Model GetModel(string assetName)
        {
            return GetModel(assetName, true);
        }

        /// <summary>
        /// Helper method for loading a model into memory. If the model was already in memory,
        /// it simply returns that model. It automatically looks into the "models\\" directory.
        /// </summary>
        /// <param name="assetName">Name of the model to load.</param>
        /// <param name="usePlaceholderIfNotExist">True to accept a placeholder; otherwise, an
        /// exception is thrown if the model cannot be found.</param>
        /// <returns>The model object.</returns>
        public Model GetModel(string assetName, bool usePlaceholderIfNotExist)
        {
            return LoadResource<Model>("models\\", assetName, usePlaceholderIfNotExist);
        }

        /// <summary>
        /// Helper method for loading a texture into memory. It's expected that the user will
        /// prepend the relative path to the directory where the actual asset lies. The
        /// resource manager will handle this internally.
        /// </summary>
        /// <param name="assetName">Name of the asset, including the path to the folder.</param>
        /// <returns>The texture object.</returns>
        public Texture2D GetTexture2D(string assetName)
        {
            if (assetName.StartsWith("textures"))
            {
                return LoadResource<Texture2D>("", assetName);
            }

            return LoadResource<Texture2D>("textures\\", assetName);
        }

        public Texture2D GetTexture2D(string assetName, int width, int height)
        {
            return Texture2D.FromFile(ServiceManager.Game.GraphicsDevice, assetName, width, height);
        }

        /// <summary>
        /// Load a rank texture into memory.
        /// </summary>
        /// <param name="rankName"></param>
        /// <returns></returns>
        public Texture2D GetRank(string rankName)
        {
            if (rankName.EndsWith(".png"))
            {
                rankName = rankName.Substring(0, rankName.Length - 4);
            }

            return LoadResource<Texture2D>("ranks\\", rankName);
        }

        /// <summary>
        /// Get a texture from the textures\tiles directory.
        /// </summary>
        /// <param name="tileName">The tile texture to load.</param>
        /// <returns>The texture object.</returns>
        public Texture2D GetTileTexture(string tileName)
        {
            return LoadResource<Texture2D>("textures\\tiles\\", tileName);
        }

        /// <summary>
        /// Get a font from the fonts directory.
        /// </summary>
        /// <param name="assetName">Name of the font.</param>
        /// <returns>The font object.</returns>
        public SpriteFont GetFont(string assetName)
        {
            return LoadResource<SpriteFont>("fonts\\", assetName);
        }

        /// <summary>
        /// Load an effect from the effects directory, or return it if it has already
        /// been loaded.
        /// </summary>
        /// <param name="assetName">Name of the effect.</param>
        /// <returns>The effect object.</returns>
        public Effect GetEffect(string assetName)
        {
            return LoadResource<Effect>("effects\\", assetName);
        }

        /// <summary>
        /// Load a particle from the particle directory, or return it if it has already
        /// been loaded.
        /// </summary>
        /// <param name="assetName">Name of the particle.</param>
        /// <returns>The particle object.</returns>
        public Texture2D GetParticle(string assetName)
        {
            return LoadResource<Texture2D>("particles\\", assetName);
        }

        /// <summary>
        /// Dispose the content manager, clearing all of it's managed and unmanaged data.
        /// </summary>
        /// <param name="disposing">True it should dispose it's managed data too; otherwise
        /// it will only dispose unmanaged data.</param>
        protected override void Dispose(bool disposing)
        {
            cache.Clear();

            if (disposing)
            {
                cache = null;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
