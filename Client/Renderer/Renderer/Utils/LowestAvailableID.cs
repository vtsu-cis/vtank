using System;
using System.Collections.Generic;

using System.Text;

namespace Renderer.Utils
{
    /// <summary>
    /// This class provides the lowest unused integer. 
    /// Used for giving unique ID numbers in an environment where ID's can be released.
    /// </summary>
    public class LowestAvailableID
    {
        #region Members
        private int id;
        private List<int> deleted;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new LowestAvailableID object and initializes the variables
        /// </summary>
        public LowestAvailableID()
        {
            id = 0;
            deleted = new List<int>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// This method returns the lowest available ID
        /// </summary>
        /// <returns>Lowest unique integer ID</returns>
        public int GetID()
        {
            lock (this)
            {
                if (deleted.Count != 0)
                {
                    int output = deleted[0];
                    deleted.RemoveAt(0);
                    return output;
                }
                else
                {
                    id++;
                    return id - 1;
                }
            }
        }

        /// <summary>
        /// When an ID is no longer being used it is released, becoming available again.
        /// </summary>
        /// <param name="_id">The integer ID to release.</param>
        public void ReleaseID(int _id)
        {
            lock (this)
            {
                deleted.Add(_id);
                deleted.Sort();
            }
        }

        /// <summary>
        /// Makes all id's available to use
        /// </summary>
        public void ReleaseAll()
        {
            lock (this)
            {
                deleted.Clear();
                id = 0;
            }
        }

        /// <summary>
        /// This method prevents the ID value from getting too high. If 100 IDs are checked out, and then the top
        /// 50 are checked back in, clean would remove the top 50 from the deleted list and set the ID value to 50;
        /// </summary>
        public void Clean()
        {
            lock (this)
            {
                deleted.Reverse(); // Deleted is now in decending order.
                while (deleted[0] == id - 1) // While the highest number in deleted is one less then the next available ID.
                {
                    deleted.RemoveAt(0);// Remove highest ID from deleted.
                    id--;
                }
                deleted.Reverse(); //Deleted is now back in ascending order.
            }
        }

        #endregion

    }
}
