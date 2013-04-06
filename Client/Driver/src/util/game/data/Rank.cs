using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.util.game
{
    public class Rank
    {
        #region Properties
        /// <summary>
        /// Get the ID of this rank.
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// Get the full title of this rank.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Get the abbreviation of this rank.
        /// </summary>
        public string Abbreviation { get; private set; }

        /// <summary>
        /// Get the filename (pathing not included) of this rank.
        /// </summary>
        public string Filename { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs a Rank object.
        /// </summary>
        /// <param name="ID">Identifying ID number of the rank.</param>
        /// <param name="title">Full title of the rank (e.g. Private First Class).</param>
        /// <param name="abbrev">Abbreviation of the rank (e.g. PFC).</param>
        /// <param name="filename">Filename of the rank texture (e.g. "pfc")</param>
        public Rank(int ID, string title, string abbrev, string filename)
        {
            this.ID = ID;
            Title = title;
            Abbreviation = abbrev;
            Filename = filename;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get the Texture2D for this rank object, or null if one is not available.
        /// </summary>
        /// <returns>Texture2D object if the texture was found; otherwise null.</returns>
        public Microsoft.Xna.Framework.Graphics.Texture2D GetTexture()
        {
            if (String.IsNullOrEmpty(Filename))
            {
                return null;
            }

            try
            {
                Texture2D rankTexture = Client.src.service.ServiceManager.Resources.GetRank(Filename);
                return rankTexture;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Warning: Could not load rank texture: {0}", ex.Message);
            }

            return null;
            
        }
        #endregion
    }
}
