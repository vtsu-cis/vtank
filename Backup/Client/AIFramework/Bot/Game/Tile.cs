/***************************************************************************
FILE     : Tile.cs
SUBJECT  : Store data concerning map tiles.
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

namespace AIFramework.Bot.Game
{
    /// <summary>
    /// A Tile contains information about a single block of a 2D Map.
    /// </summary>
    public class Tile
    {
        public static int BYTES_PER_TILE = 12;
        public static int TILE_SIZE_IN_PIXELS = 64;

        private uint tileId;
        private ushort objectId;
        private ushort eventId;
        private bool   passable;
        private ushort height;
        private ushort type;
        private ushort effect;
        
        /// <summary>
        /// Each tile carries an ID number.  This number is used to
        /// find a certain tile by it's ID and draw it onto the screen.
        /// </summary>
        public uint ID
        {
            get { return tileId; }
            set { tileId = value; }
        }

        /// <summary>
        /// Each tile is allowed to carry an object within it. If the object is set
        /// to zero, the tile has no object.
        /// </summary>
        public ushort ObjectID
        {
            get { return objectId; }
            set { objectId = value; }
        }

        /// <summary>
        /// Each tile is allowed to carry an event within it. If the event is set to
        /// zero, the tile has no event.
        /// </summary>
        public ushort EventID
        {
            get { return eventId; }
            set { eventId = value; }
        }
        
        /// <summary>
        /// Control whether or not the tile can be passed by a tank.
        /// </summary>
        public bool IsPassable
        {
            get { return passable; }
            set { passable = value; }
        }

        /// <summary>
        /// Get or set the height of the tile.
        /// </summary>
        public ushort Height
        {
            get { return height; }
            set { height = value; }
        }

        /// <summary>
        /// Get or set the type of the tile.
        /// </summary>
        public ushort Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Get or set the effect of the tile.
        /// </summary>
        public ushort Effect
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Construct a Tile.
        /// </summary>
        /// <param name="_tileId">Unique ID number of the tile, for graphical look-up purposes.</param>
        /// <param name="_passable">True if the tile can be passed by tanks, otherwise false.</param>
        public Tile(uint _tileId, ushort _objectId, ushort _eventId, bool _passable,
            ushort _height, ushort _type, ushort _effect)
        {
            tileId      = _tileId;
            objectId    = _objectId;
            eventId     = _eventId;
            passable    = _passable;
            height      = _height;
            type        = _type;
            effect      = _effect;
        }

        /// <summary>
        /// Convert the tile to an array of bytes. Handy for saving each tile to disk.
        /// Note that each ByteData call will allocate new memory, so it should only be called once.
        /// </summary>
        /// <returns>A 12-length array of the tile's byte data in little-endian order.</returns>
        public byte[] ByteData()
        {
            return new byte[] { 
                (byte)(tileId & 0xFF), 
                (byte)((tileId >> 8) & 0xFF), 
                (byte)((tileId >> 16) & 0xFF),
                (byte)((tileId >> 24) & 0xFF),
                (byte)(objectId & 0xFF),
                (byte)((objectId >> 8) & 0xFF),
                (byte)(eventId & 0xFF),
                (byte)((eventId >> 8) & 0xFF),
                (byte)(passable ? 0x01 : 0x00),
                (byte)(height & 0xFF), 
                (byte)(type & 0xFF), 
                (byte)(effect & 0xFF)
            };
        }
    }
}
