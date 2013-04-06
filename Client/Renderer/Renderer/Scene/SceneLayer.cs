using System;
using System.Collections.Generic;
using System.Text;
using Renderer.SceneTools.Entities;

namespace Renderer.SceneTools
{
    /// <summary>
    /// A layer is an element of a scene that entities reside in. It determines what order groups of entities are 
    /// drawn onto the screen.  
    /// </summary>
    public class SceneLayer
    {
        #region Members
        public Dictionary<int, Object3> entityDictionary3D;
        public Dictionary<int, VertexGroup> entityDictionaryVertex;
        public Dictionary<int, ParticleEmitter> entityDictionaryEmitter;
        public Dictionary<int, VertexColorGroup> entityDictionaryVertexColor;
        public Dictionary<int, VertexPositionColorTextureGroup> entityDictionaryVPCT;
        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new RendererSceneLayer. The layer will draw by default.
        /// </summary>
        public SceneLayer()
        {
            entityDictionary3D = new Dictionary<int, Object3>();
            entityDictionaryVertex = new Dictionary<int, VertexGroup>();
            entityDictionaryEmitter = new Dictionary<int, ParticleEmitter>();
            entityDictionaryVertexColor = new Dictionary<int, VertexColorGroup>();
            entityDictionaryVPCT = new Dictionary<int, VertexPositionColorTextureGroup>();
            DrawLayer = true;
        }
        #endregion

        #region Properties
        /// <summary>
        /// If true, the layer will draw. If false it will not. 
        /// </summary>
        public Boolean DrawLayer
        {
            get;
            set;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Adds an Object3 to the layer.
        /// </summary>
        /// <param name="_entityID">The id to assign to the Object3.</param>
        /// <param name="_entity">The Object3 object.</param>
        public void Add(int _entityID, Object3 _entity)
        {
            entityDictionary3D.Add(_entityID, _entity);
        }

        /// <summary>
        /// Adds a VertexColorGroup object to the layer.
        /// </summary>
        /// <param name="_entityID">The id to assign to the VertexColorGroup.</param>
        /// <param name="_entity">The VertexColorGroup object.</param>
        public void Add(int _entityID, VertexColorGroup _entity)
        {
            entityDictionaryVertexColor.Add(_entityID, _entity);
        }

        /// <summary>
        /// Adds a VertexPositionColorTextureGroup object to the layer.
        /// </summary>
        /// <param name="_entityID">The id to assign to the VertexPositionColorTextureGroup.</param>
        /// <param name="_entity">The VertexPositionColorTextureGroup object.</param>
        public void Add(int _entityID, VertexPositionColorTextureGroup _entity)
        {
            entityDictionaryVPCT.Add(_entityID, _entity);
        }

        /// <summary>
        /// Adds a VertexGroup object to the layer.
        /// </summary>
        /// <param name="_entityID">The id to assign to the VertexGroup.</param>
        /// <param name="_entity">The VertexGroup object.</param>
        public void Add(int _entityID, VertexGroup _entity)
        {
            entityDictionaryVertex.Add(_entityID, _entity);
        }

        /// <summary>
        /// Adds a ParticleEmitter object to the layer.
        /// </summary>
        /// <param name="_entityID">The id to assign to the ParticleEmitter.</param>
        /// <param name="_entity">The ParticleEmitter object.</param>
        public void Add(int _entityID, ParticleEmitter _entity)
        {
            entityDictionaryEmitter.Add(_entityID, _entity);
        }

        /// <summary>
        /// Adds an int, Object3 KeyValuePair to the layer
        /// </summary>
        /// <param name="ent">The KeyValuePair to add. The int is the id of the Object3.</param>
        public void Add(KeyValuePair<int, Object3> ent)
        {
            entityDictionary3D.Add(ent.Key, ent.Value);
        }

        /// <summary>
        /// Adds an int, VertexGroup KeyValuePair to the layer
        /// </summary>
        /// <param name="ent">The KeyValuePair to add. The int is the id of the VertexGroup.</param>
        public void Add(KeyValuePair<int, VertexGroup> ent)
        {
            entityDictionaryVertex.Add(ent.Key, ent.Value);
        }

        /// <summary>
        /// Adds an int, ParticleEmitter KeyValuePair to the layer
        /// </summary>
        /// <param name="ent">The KeyValuePair to add. The int is the id of the ParticleEmitter.</param>
        public void Add(KeyValuePair<int, ParticleEmitter> ent)
        {
            entityDictionaryEmitter.Add(ent.Key, ent.Value);
        }

        /// <summary>
        /// Adds an int, VertexColorGroup KeyValuePair to the layer
        /// </summary>
        /// <param name="ent">The KeyValuePair to add. The int is the id of the VertexColorGroup.</param>
        public void Add(KeyValuePair<int, VertexColorGroup> ent)
        {
            entityDictionaryVertexColor.Add(ent.Key, ent.Value);
        }

        /// <summary>
        /// Adds an int, VertexPositionColorTexture KeyValuePair to the layer
        /// </summary>
        /// <param name="ent">The KeyValuePair to add. The int is the id of the VertexPositionColorTexture.</param>
        public void Add(KeyValuePair<int, VertexPositionColorTextureGroup> ent)
        {
            entityDictionaryVPCT.Add(ent.Key, ent.Value);
        }

        /// <summary>
        /// Removes a RendererEntity from the layer.
        /// </summary>
        /// <param name="_entity">The RendererEntity to remove.</param>
        public bool Delete(int _entityID)
        {
            return (entityDictionary3D.Remove(_entityID) 
                || entityDictionaryVertex.Remove(_entityID)
                || entityDictionaryEmitter.Remove(_entityID)
                || entityDictionaryVertexColor.Remove(_entityID))
                || entityDictionaryVPCT.Remove(_entityID);
        }

        /// <summary>
        /// Removes all entities from this layer.
        /// </summary>
        /// <returns>The list of id's removed from this layer.</returns>
        public List<int> Clear()
        {
            List<int> keys = new List<int>();
            keys.AddRange(entityDictionary3D.Keys);
            keys.AddRange(entityDictionaryVertex.Keys);
            keys.AddRange(entityDictionaryEmitter.Keys);
            keys.AddRange(entityDictionaryVertexColor.Keys);
            keys.AddRange(entityDictionaryVPCT.Keys);

            entityDictionary3D.Clear();
            entityDictionaryVertex.Clear();
            entityDictionaryEmitter.Clear();
            entityDictionaryVertexColor.Clear();
            entityDictionaryVPCT.Clear();

            return keys;
        }
                
        #endregion
    }
}