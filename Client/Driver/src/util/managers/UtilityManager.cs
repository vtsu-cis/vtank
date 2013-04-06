using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;

namespace Client.src.util
{
    public class UtilityManager
    {
        private Dictionary<int, UtilityIcon> utilities;

        #region Constructor

        public UtilityManager()
        {
            utilities = new Dictionary<int, UtilityIcon>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a utility to the renderer
        /// </summary>
        /// <param name="id">Utility reference ID</param>
        /// <param name="_util">The parameters of the utility</param>
        /// <param name="_position">The position to insert the utility</param>
        public void AddUtility(int id, string modelName, Vector3 _position)
        {
            Model mod = ServiceManager.Resources.GetModel("powerups\\" + modelName);
            UtilityIcon newUtility = new UtilityIcon(mod, _position + new Vector3(0,0,30));
            newUtility.RenderID = ServiceManager.Scene.Add(newUtility, 1);
            utilities[id] = newUtility;
        }

        /// <summary>
        /// TODO: Document me.
        /// </summary>
        /// <param name="id"></param>
        public void Remove(int id)
        {
            ServiceManager.Scene.Delete(utilities[id].RenderID);
            utilities.Remove(id);
        }

        /// <summary>
        /// TODO: Document me.
        /// </summary>
        public void Clear()
        {
            foreach (UtilityIcon ut in utilities.Values)
            {
                ServiceManager.Scene.Delete(ut.RenderID);
            }
            utilities.Clear();
        }

        /// <summary>
        /// Checks if the utilities collection contains the utility.
        /// </summary>
        /// <param name="id">The ID of the utility.</param>
        /// <returns>Whether or not it is in the collection.</returns>
        public bool HasUtility(int id)
        {
            return this.utilities.ContainsKey(id);
        }

        #endregion

        public IEnumerator<UtilityIcon> GetEnumerator()
        {
            return utilities.Values.GetEnumerator();
        }
    }
}
