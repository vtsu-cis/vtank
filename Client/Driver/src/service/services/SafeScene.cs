using System;
using System.Collections.Generic;
using System.Text;
using Renderer.SceneTools;
using Renderer.SceneTools.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Renderer;

namespace Client.src.service.services
{
    /// <summary>
    /// SafeScene is a thread-safe solution for multiple threads attempting to access
    /// the same scene.
    /// </summary>
    public class SafeScene
    {
        #region Members
        private EntityRenderer renderer;
        private Scene scene;
        #endregion

        #region Constructor
        /// <summary>
        /// Construct a safe scene.
        /// </summary>
        /// <param name="_renderer">Renderer instance to work with.</param>
        public SafeScene(EntityRenderer _renderer)
        {
            renderer = _renderer;
            scene = _renderer.ActiveScene;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add an entity to the active scene. The new entity is automatically added to
        /// the second layer because it is a 3D object.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <returns>The ID assigned to the object.</returns>
        public int Add(Object3 entity)
        {
            return this.Add(entity, 2);
        }

        /// <summary>
        /// Add an entity to the active scene.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <param name="layer">Layer to assign to the object.</param>
        /// <returns>The ID assigned to the object.</returns>
        public int Add(Object3 entity, int layer) 
        {
            lock (this)
            {
                return scene.Add(entity, layer);
            }
        }

        /// <summary>
        /// Add an entity to the active scene.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <param name="layer">Layer to assign to the object.</param>
        /// <returns>The ID assigned to the object.</returns>
        public int Add(ParticleEmitter entity, int layer)
        {
            lock (this)
            {
                return scene.Add(entity, layer);
            }
        }

        /// <summary>
        /// Add an entity to the active scene. The new entity is automatically added to
        /// the second layer because it is a 3D object.
        /// </summary>
        /// <param name="model">Model to add.</param>
        /// <param name="position">Position of the object.</param>
        /// <returns>The ID assigned to the object.</returns>
        public int Add(Model model, Vector3 position)
        {
            return this.Add(model, position, 2);
        }

        /// <summary>
        /// Add an entity to the active scene.
        /// </summary>
        /// <param name="model">Model to add.</param>
        /// <param name="position">Position of the object.</param>
        /// <param name="layer">Layer to assign to the object.</param>
        /// <returns>The ID assigned to the object.</returns>
        public int Add(Model model, Vector3 position, int layer)
        {
            lock (this)
            {
                return scene.Add(model, position, layer);
            }
        }

        /// <summary>
        /// Add an entity to the active scene. The new entity is automatically added to
        /// the third layer because it is a 2D object.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <returns>The ID assigned to the object.</returns>
        public int Add(Object2 entity)
        {
            return this.Add(entity, 3);
        }

        /// <summary>
        /// Add an entity to the active scene.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <param name="layer">Layer to assign to the object.</param>
        /// <returns>The ID assigned to the object.</returns>
        public int Add(Object2 entity, int layer)
        {
            lock (this)
            {
                return scene.Add(entity, layer);
            }
        }

        /// <summary>
        /// Add an entity to the active scene. The new entity is automatically added to
        /// the third layer because it is a 2D object.
        /// </summary>
        /// <param name="texture">Texture to add.</param>
        /// <param name="position">Position of the texture.</param>
        /// <returns>The ID assigned to the object.</returns>
        public int Add(Texture2D texture, Vector3 position)
        {
            return this.Add(texture, position, 3);
        }

        /// <summary>
        /// Add an entity to the active scene.
        /// </summary>
        /// <param name="texture">Texture to add.</param>
        /// <param name="position">Position of the texture.</param>
        /// <param name="layer">Layer to assign to the texture.</param>
        /// <returns>The ID assigned to the object.</returns>
        public int Add(Texture2D texture, Vector3 position, int layer)
        {
            lock (this)
            {
                return scene.Add(texture, position, layer);
            }
        }

        /// <summary>
        /// Add a vertex color group to the renderer using the default layer of 2.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <returns>The ID assigned to the object.</returns>
        public int Add(VertexColorGroup entity)
        {
            lock (this)
            {
                return scene.Add(entity, 2);
            }
        }
        public int Add(VertexPositionColorTextureGroup entity)
        {
            lock (this)
            {
                return scene.Add(entity, 2);
            }
        }

        public int Add(VertexGroup entity)
        {
            lock (this)
            {
                return scene.Add(entity, 2);
            }
        }

        /// <summary>
        /// Access an entity of any sub-type Entity from the renderer.
        /// Note that this throws an exception if the object does not exist
        /// or if the object is cast to the wrong type (e.g. you try to access
        /// a DrawableTile and try to cast it to a PlayerTank).
        /// </summary>
        /// <typeparam name="T">Entity type or sub-type.</typeparam>
        /// <param name="id">ID of the object internal to the renderer.</param>
        /// <returns>The accessed object.</returns>
        public T Access<T>(int id) where T : Entity
        {
            lock (this)
            {
                return (T)scene.Access(id);
            }
        }

        /// <summary>
        /// Remove an object from the scene.
        /// </summary>
        /// <param name="id">ID of the object.</param>
        /// <returns>True if the object was found and removed; false otherwise.</returns>
        public bool Remove(int id)
        {
            lock (this)
            {
                return scene.Delete(id);
            }
        }

        /// <summary>
        /// Clear all objects from the scene.
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                scene.ClearAll();
            }
        }

        /// <summary>
        /// Updates the scene in a thread-safe way.
        /// </summary>
        /// <param name="time">Delta time (seconds).</param>
        public void Update(double time)
        {
            // TODO: Locking here causes deadlock.
            // Fix that, instead of using the cheap work-around.
            //lock (this)
            //{
                try
                {
                    renderer.Update();
                }
                catch (Exception ex) {
                    Console.Error.WriteLine(ex);
                }
            //}
        }

        /// <summary>
        /// Draws the scene in a thread-safe way.
        /// </summary>
        public void Draw()
        {
            //lock (this)
            //{
                try
                {
                    renderer.Draw();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            //}
        }
        #endregion
    }
}
