using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Client.src.service;

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
            public object Resource
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
                Resource = resource;
            }
            #endregion
        }
        #endregion

        #region Members
        private Dictionary<string, Resource> cache;
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
        public T LoadResource<T>(string path, string assetName)
        {
            if (assetName.EndsWith(".xnb"))
            {
                // The extension is unneeded, so remove it.
                assetName = assetName.Substring(0, assetName.Length - 4);
            }

            // Check if the asset is already in cache. If it is, return it.
            if (cache.ContainsKey(assetName))
            {
                return (T)cache[assetName].Resource;
            }

            if (path.Length > 0 && (!path.EndsWith("\\") || !path.EndsWith("/")))
            {
                path += "\\";
            }

            // The resource doesn't exist, so load it.
            if (assetName.Contains("\\") || assetName.Contains("/"))
            {
                assetName = assetName.Replace('/', '\\');

                // Remove the leading path and add it to the 'path' instead.
                int index = assetName.LastIndexOf('\\');
                path += assetName.Substring(0, index + 1);
                assetName = assetName.Substring(index + 1);
            }

            path = String.Format("{0}{1}", path, assetName);

            T resourceObject = base.Load<T>(path);
            Resource resource = new Resource(assetName, resourceObject);

            cache[assetName] = resource;

            return resourceObject;
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
            return LoadResource<Model>("models\\", assetName);
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
