/***************************************************************************
FILE     : Map.cs
SUBJECT  : Track tiles for VTank game maps.
AUTHOR   : (C) Copyright 2008 by Vermont Technical College

LICENSE

This program is free software; you can redistribute it and/or modify it
under the terms of the GNU General Public License as published by the
Free Software Foundation; either version 2 of the License, or (at your
option) any later version.

This program is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANT-
ABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public
License for more details.

You should have received a copy of the GNU General Public License along
with this program; if not, write to the Free Software Foundation, Inc.,
59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

TODO

  + ...

Please send comments and bug reports to

     Summer of Software Engineering
     Vermont Technical College
     201 Lawrence Place
     Williston, VT 05495
     sosebugs@summerofsoftware.org (http://www.summerofsoftware.org)
***************************************************************************/
using System;
using Microsoft.Win32;

namespace Client.src.util
{
    using System.IO;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A Map in VTank is a set of tiles which are drawn to the screen to create
    /// an environment for the player to move around in.  Along with the terrain,
    /// it also keeps track of which tiles the player is allowed to move on.
    /// </summary>
    public sealed class Map
    {
        public static readonly byte FORMAT_VERSION = 1;

        private byte version;
        private string filename;
        private string title;
        private uint width;
        private uint height;
        private Tile[] tile_data;
        private List<int> supportedGameModes = new List<int>();
        private string sha1sum;

        /// <summary>
        /// Access the map's filename.
        /// </summary>
        public string FileName
        {
            get { return filename; }
            set { filename = value; }
        }
        
        /// <summary>
        /// Access the map's title.
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        /// <summary>
        /// Access the map's width. Changing the width will call Resize.
        /// </summary>
        /// <see>Client.src.gamemap.Map.Resize</see>
        public uint Width
        {
            get { return width; }
            set { width = value; /* TODO Call Resize */}
        }

        /// <summary>
        /// Access the map's height. Changing the height will call Resize.
        /// </summary>
        /// <see>Client.src.gamemap.Map.Resize</see>
        public uint Height
        {
            get { return height; }
            set { height = value; /* TODO Call Resize */}
        }

        /// <summary>
        /// Set what game modes are allowed.
        /// </summary>
        /// <param name="gameModes"></param>
        public void SetGameModes(List<int> gameModes)
        {
            supportedGameModes = gameModes;
        }

        /// <summary>
        /// Check if a certain game mode is supported.
        /// </summary>
        /// <param name="gameMode"></param>
        /// <returns></returns>
        public bool GameModeSupported(int gameMode)
        {
            for (int i = 0; i < supportedGameModes.Count; i++)
            {
                if (supportedGameModes[i] == gameMode)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Obtain a SHA1 hash of the map file. This is useful for integrity
        /// checks. This value is cached, so it can be called repeatedly.
        /// </summary>
        public string SHA1Hash
        {
            get { return sha1sum; }
        }

        /// <summary>
        /// Gets the version byte for this map.
        /// </summary>
        public byte Version
        {
            get { return version; }
        }

        /// <summary>
        /// Get a tile at a specific location on a 2D map plane.
        /// </summary>
        /// <param name="x">X-coordinate (in tiles) of the tile.</param>
        /// <param name="y">Y-coordinate (in tiles) of the tile.</param>
        /// <returns>Tile at the given (x, y) location.</returns>
        public Tile GetTile(uint x, uint y)
        {
            return tile_data[y * Width + x];
        }

        /// <summary>
        /// Get the full array of tile data.
        /// </summary>
        /// <returns>Tile data.</returns>
        public Tile[] GetTileData()
        {
            return tile_data;
        }

        /// <summary>
        /// Set new data for a tile at a specific location on a 2D map plane.
        /// </summary>
        /// <param name="x">X-coordinate (in tiles) of the tile.</param>
        /// <param name="y">Y-coordinate (in tiles) of the tile.</param>
        /// <param name="tile">Tile to set to the given (x, y) location.</param>
        public void SetTile(uint x, uint y, Tile tile)
        {
            tile_data[y * Width + x] = tile;
        }

        /// <summary>
        /// Inistantiate the map object by loading it from a file. Be warned, this method
        /// uses the LoadMap method, which can throw a System.IO.FileNotFoundException.
        /// </summary>
        /// <param name="mapFileName">Map file to load.</param>
        /// <see>Client.src.gamemap.Map.LoadMap</see>
        public Map(string mapFileName)
        {
            LoadMap(mapFileName);
        }

        ~Map()
        {
            Console.WriteLine("Destructing map {0}.", Title);
        }

        /// <summary>
        /// Inistantiate the map object by creating a brand new map.
        /// This constructor calls CreateMap.
        /// </summary>
        /// <param name="mapTitle">Title of the map.</param>
        /// <param name="mapFileName">Filename of the map.</param>
        /// <param name="mapWidth">Width (in tiles) of the map.</param>
        /// <param name="mapHeight">Height (in tiles) of the map.</param>
        /// <see>Client.src.gamemap.Map.CreateMap</see>
        public Map(string mapTitle, string mapFileName, uint mapWidth, uint mapHeight)
        {
            CreateMap(mapTitle, mapFileName, mapWidth, mapHeight);
        }

        /// <summary>
        /// Load a map that exists from disk using the stored filename.
        /// If the map can't be found, this method throws a FileNotFoundException. If otherwise
        /// an IO error occurs, a more general-purpose IOException is thrown.
        /// </summary>
        /// <see>System.IO.IOException</see>
        public void LoadMap()
        {
            using (BinaryReader reader = new BinaryReader(File.Open(FileName, FileMode.Open, FileAccess.Read)))
            {
                byte version_byte = (byte)reader.Read();
                if (version_byte != FORMAT_VERSION)
                {
                    throw new IOException("Version mismatch: The map " + FileName + 
                        " uses an invalid format version.");
                }

                char c;
                StringBuilder stringBuffer = new StringBuilder();
                while ((c = reader.ReadChar()) != '\n')
                {
                    stringBuffer.Append(c);
                }

                title = stringBuffer.ToString();

                byte[] buf = reader.ReadBytes(4);
                int width = ToInt32(buf[0], buf[1], buf[2], buf[3]);

                buf = reader.ReadBytes(4);
                int height = ToInt32(buf[0], buf[1], buf[2], buf[3]);

                string gameModes = "";
                byte b;
                while ((b = reader.ReadByte()) != '\n')
                {
                     gameModes += b;
                }

                if (width <= 0 || height <= 0)
                {
                    throw new IOException("Map size cannot have a width or height less than or equal to 0");
                }

                tile_data = new Tile[width * height];
                version   = version_byte;
                Title     = title;
                Width     = (uint)width;
                Height    = (uint)height;
                
                for (int i = 0; i < gameModes.Length; i++) {
                    supportedGameModes.Add(gameModes[i]);
                }
                
                // Now read file contents into the tile data.
                // The cast is justified if the map is smaller than 2 GiB (on 32 bit systems)
                int byte_count = Tile.BYTES_PER_TILE * width * height;
                byte[] buffer = new byte[byte_count];
                reader.Read(buffer, 0, byte_count);
                
                for (int i = 0, j = 0; i < byte_count; i += Tile.BYTES_PER_TILE, j++)
                {
                    int tileId = (buffer[i]) | ((buffer[i + 1] << 8)) |
                        ((buffer[i + 2] << 16)) | ((buffer[i + 3] << 24));

                    int objectId = (buffer[i + 4]) + ((buffer[i + 5] << 8));
                    int eventId = (buffer[i + 6]) + ((buffer[i + 7] << 8));
                    bool passable = (buffer[i + 8] == 0) ? false : true;
                    int tile_height = (buffer[i + 9]);
                    int type = (buffer[i + 10]);
                    int effect = (buffer[i + 11]);

                    tile_data[j] = new Tile(
                        (uint)tileId, (ushort)objectId, (ushort)eventId, passable,
                        (ushort)tile_height, (ushort)type, (ushort)effect);
                }
            }

            sha1sum = Hash.CalculateSHA1OfFile(FileName);
        }

        /// <summary>
        /// Load a map that exists from disk.
        /// If the map can't be found, this method throws a FileNotFoundException. If otherwise
        /// an IO error occurs, a more general-purpose IOException is thrown.
        /// </summary>
        /// <param name="fileName">Name of the file to be loaded.</param>
        /// <see>System.IO.IOException</see>
        public void LoadMap(string fileName)
        {
            FileName = fileName;

            LoadMap();
        }

        /// <summary>
        /// Create a new map from scratch. Initializes all tiles to ID '0'.
        /// It will attempt to write the map to disk with the given filename. If an error occurs,
        /// a System.IO.IOException will be thrown.
        /// </summary>
        /// <param name="mapTitle">Title of the map.</param>
        /// <param name="mapFileName">The filename of the map.</param>
        /// <param name="mapWidth">Width (in tiles) of the map.</param>
        /// <param name="mapHeight">Height (in tiles) of the map.</param>
        /// <see>System.IO.IOException</see>
        public void CreateMap(string mapTitle, string mapFileName, uint mapWidth, uint mapHeight)
        {
            Title       = mapTitle;
            FileName    = mapFileName;
            Width       = mapWidth;
            Height      = mapHeight;
            tile_data   = new Tile[Width * Height];
            version     = FORMAT_VERSION;

            uint mapSize = Width * Height;
            for (uint i = 0; i < mapSize; i++)
            {
                tile_data[i] = new Tile(0, 0, 0, true, 0, 0, 0);
            }

            SaveMap();
        }

        /// <summary>
        /// Save the map to disk under the stored filename.
        /// If it has a problem writing to disk, a System.IO.IOExceptin is thrown.
        /// This function is a convenience function which calls SaveMap(path). The
        /// default path points to [current]/maps.
        /// </summary>
        /// <see>System.IO.IOException</see>
        public void SaveMap()
        {
            SaveMap("maps");
        }

        /// <summary>
        /// Save the map to disk under the stored filename.
        /// If it has a problem writing to disk, a System.IO.IOException is thrown.
        /// </summary>
        /// <param name="path">Path to the maps folder -- not including the file name itself.</param>
        /// <see>System.IO.IOException</see>
        public void SaveMap(string path)
        {
            // Used to convert a string to ASCII bytes.
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            // Convert each message into it's byte-equivalent.
            byte[] versionData = new byte[] { FORMAT_VERSION };
            byte[] titleData    = encoding.GetBytes(Title + '\n');
            byte[] widthData    = FromInt32(Width);
            byte[] heightData   = FromInt32(Height);
            byte[] gameModes    = new byte[supportedGameModes.Count + 1];
            for (int i = 0; i < supportedGameModes.Count; i++)
            {
                gameModes[i] = (byte)supportedGameModes[i];
            }

            gameModes[gameModes.Length - 1] = 0x0A;

            byte[] tileData     = new byte[Width * Height * Tile.BYTES_PER_TILE];

            // Loop through each block of bytes, assigning it to the appropriate tile data.
            for (uint i = 0, j = 0; i < tileData.Length; i += (uint)Tile.BYTES_PER_TILE, j++)
            {
                byte[] data = tile_data[j].ByteData();
                for (short k = 0; k < Tile.BYTES_PER_TILE; k++)
                {
                    tileData[i + k] = data[k];
                }
            }

            //Checks to see if the path exists. Creates it if it doesnt
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fullPath = String.Format("{0}{1}{2}",
                path, (path.EndsWith(Path.DirectorySeparatorChar.ToString())) ? "" : "\\", FileName);
            // Now write the data to disk.
            using (FileStream fileStream = File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fileStream.Write(versionData, 0, versionData.Length);
                fileStream.Write(titleData,  0, titleData.Length);
                fileStream.Write(widthData,  0, widthData.Length);
                fileStream.Write(heightData, 0, heightData.Length);
                fileStream.Write(gameModes,  0, gameModes.Length);
                fileStream.Write(tileData,   0, tileData.Length);
            }
            
            sha1sum = Hash.CalculateSHA1OfFile(fullPath);
        }

        /// <summary>
        /// Utility function that helps convert binary data to a 32-bit integer.
        /// This method expects a little-endian buffer.
        /// </summary>
        /// <param name="c1">First byte.</param>
        /// <param name="c2">Second byte.</param>
        /// <param name="c3">Third byte.</param>
        /// <param name="c4">Forth byte.</param>
        /// <returns>Converted integer.</returns>
        private static int ToInt32(char c1, char c2, char c3, char c4)
        {
            return ((c1) +
                ((c2 << 8)) +
                ((c3 << 16)) +
                ((c4 << 24)));
        }

        /// <summary>
        /// Utility function that helps convert binary data to a 32-bit integer.
        /// This method expects a little-endian buffer.
        /// </summary>
        /// <param name="c1">First byte.</param>
        /// <param name="c2">Second byte.</param>
        /// <param name="c3">Third byte.</param>
        /// <param name="c4">Forth byte.</param>
        /// <returns>Converted integer.</returns>
        private static int ToInt32(byte b1, byte b2, byte b3, byte b4)
        {
            return b1 + (b2 << 8) + (b3 << 16) + (b4 << 24);
        }

        /// <summary>
        /// Convert an integer to it's byte form.
        /// </summary>
        /// <param name="i">Integer to convert.</param>
        /// <returns>Result byte array.</returns>
        private static byte[] FromInt32(Int32 i)
        {
            return new byte[] {
                (byte)(i & 0xFF),
                (byte)((i >> 8) & 0xFF),
                (byte)((i >> 16) & 0xFF),
                (byte)((i >> 24) & 0xFF)
            };
        }

        /// <summary>
        /// Convert an integer to it's byte form.
        /// </summary>
        /// <param name="i">Integer to convert.</param>
        /// <returns>Result byte array.</returns>
        private static byte[] FromInt32(UInt32 i)
        {
            return new byte[] {
                (byte)(i & 0xFF),
                (byte)((i >> 8) & 0xFF),
                (byte)((i >> 16) & 0xFF),
                (byte)((i >> 24) & 0xFF)
            };
        }
    }
}
