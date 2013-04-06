/*!
    \file   TileParser.cs
    \brief  Parses the tile_dictionary.txt file and stores the tiles into a dictionary for caching
    \author (C) Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;
using GameForms.Controls;


namespace Client.src.util.game
{
    /// <summary>
    /// The tile parser reads in tiles from the given tile dictionary and loads the
    /// linked textures. The list of tiles is static because tile information needs
    /// to be loaded only once.
    /// </summary>
    public static class TileList
    {
        #region Members
        private static string tileFile;
        private static bool init = false;
        private static Dictionary<int, Texture2D> tileDictionary = 
            new Dictionary<int, Texture2D>();
        private static Dictionary<int, Model> objectDictionary =
            new Dictionary<int, Model>();
        private static Dictionary<int, Model> eventDictionary =
            new Dictionary<int, Model>();
        #endregion
        
        #region Properties
        /// <summary>
        /// Get whether or not the program has been initialized (read).
        /// </summary>
        public static bool Initialized
        {
            get { return init; }
            private set { init = value; }
        }
        
        /// <summary>
        /// Gets or sets the tile dictionary file. This is the file where tile 
        /// IDs and tile textures are associated to each other.
        /// </summary>
        public static string TileDictionary
        {
            get
            {
                return tileFile;
            }

            set
            {
                tileFile = value;
            }
        }

        /// <summary>
        /// Gets or sets the object dictionary file.
        /// </summary>
        public static string ObjectDictionary
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event dictionary file.
        /// </summary>
        public static string EventDictionary
        {
            get;
            set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Parses the tile document and loads the files into the dictionary
        /// </summary>
        public static bool Read()
        {
            if (TileDictionary == null)
                TileDictionary = "tile_dictionary.txt";
            if (ObjectDictionary == null)
                ObjectDictionary = "object_dictionary.txt";
            if (EventDictionary == null)
                EventDictionary = "event_dictionary.txt";

            bool result = false;
            try
            {
                using (StreamReader streamReader = new StreamReader(tileFile))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        //Consider all lines that are not comments.
                        if (!line.StartsWith("#") && !String.IsNullOrEmpty(line))
                        {
                            // Remove any trailing comments.
                            if (line.Contains("#"))
                            {
                                line = line.Substring(0, line.IndexOf("#"));
                            }

                            string tile = line.Substring(line.IndexOf(':') + 1);
                            tile = tile.Substring(0, tile.Length - 4);
                            int tileID = int.Parse(line.Substring(0, line.IndexOf(':')));

                            Texture2D tileTexture = ServiceManager.Resources.GetTileTexture(tile);
                            tileDictionary.Add(tileID, tileTexture);
                        }
                    }
                }

                using (StreamReader streamReader = new StreamReader(ObjectDictionary))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        //Consider all lines that are not comments.
                        if (!line.StartsWith("#"))
                        {
                            // Remove any trailing comments.
                            if (line.Contains("#"))
                            {
                                line = line.Substring(0, line.IndexOf("#"));
                            }

                            string obj = line.Substring(line.IndexOf(':') + 1);
                            obj = obj.Substring(0, obj.Length - 4);
                            int objectID = int.Parse(line.Substring(0, line.IndexOf(':')));

                            Model objectModel = ServiceManager.Resources.GetModel(
                                String.Format("objects\\{0}", obj));
                            objectDictionary.Add(objectID, objectModel);
                        }
                    }
                }

                result = true;
                Initialized = true;
            }
            catch (FileNotFoundException err)
            {
                // TODO: The user should probably know about this.
                ServiceManager.Game.Console.DebugPrint(
                    "File could not be found: " + err);
            }
            catch (IOException e)
            {
                ServiceManager.Game.Console.DebugPrint(
                    "I/O error reading the tile dictionary: " + e);
                MessageBox alert = new MessageBox(ServiceManager.Game.Manager,
                    MessageBox.MessageBoxType.ERROR,
                    string.Format("An error occured while trying to read {0}: {1}", 
                        tileFile, e.Message),
                    "Error reading tile dictionary file.");
            }
            catch (Exception e)
            {
                ServiceManager.Game.Console.DebugPrint(
                    "The tile dictionary {0} is corrupted: {1}", tileFile, e);
                MessageBox alert = new MessageBox(ServiceManager.Game.Manager,
                    MessageBox.MessageBoxType.ERROR,
                    string.Format("The file {0} is corrupted.\nTry running the patcher again." +
                    "If that doesn't work, please visit the website for help.", tileFile),
                    "Error reading tile dictionary file.");
            }

            return result;
        }

        /// <summary>
        /// Get a tile at a given index.
        /// </summary>
        /// <param name="id">ID of the tile.</param>
        /// <returns>The texture matching the ID.</returns>
        public static Texture2D GetTile(int id)
        {
            if (!tileDictionary.ContainsKey(id))
            {
                throw new KeyNotFoundException(
                    "The following tile ID was not found: " + id);
            }

            return tileDictionary[id];
        }

        /// <summary>
        /// Get an object at the given index.
        /// </summary>
        /// <param name="id">ID of the object.</param>
        /// <returns>Model object matching the ID.</returns>
        public static Model GetObject(int id)
        {
            if (!objectDictionary.ContainsKey(id))
            {
                // Returns the placeholder model.
                return ServiceManager.Resources.Load<Model>(null);
            }

            return objectDictionary[id];
        }
        #endregion Methods
    }
}
