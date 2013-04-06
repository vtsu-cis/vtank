###########################################################################
# FILE     : Map.py
# SUBJECT  : Represents a player-used map with tile data and collision data.
# AUTHOR   : (C) Copyright 2008 by Vermont Technical College
###########################################################################
from Utils import bytes_to_int, bytes_to_short, int_to_dword, short_to_word;
from hashlib import sha1;
from cStringIO import StringIO;

# Version of the map format currently supported.
FORMAT_VERSION = 0x01;

class Tile:
    """
    Holds data about a single tile.
    """
    tile_id     = 0;
    object_id   = 0;
    event_id    = 0;
    collision   = False;
    height      = 0;
    type        = 0;
    effect      = 0;
    
    def __init__(self, tile_id, object_id, event_id, collision, height, type, effect):
        """
        Create a new tile.
        @param tile_id ID of the tile.
        @param collision Collision (true, false) of the tile.
        """
        self.tile_id    = tile_id;
        self.object_id  = object_id;
        self.event_id   = event_id;
        self.collision  = collision;
        self.height     = height;
        self.type       = type;
        self.effect     = effect;
        
    def get_raw_tile(self):
        """
        Get this tile in raw binary data format.
        @return Raw tile.
        """
        return "%s%s%s%c%c%c%c" % (
            int_to_dword(self.tile_id), short_to_word(self.object_id), 
            short_to_word(self.event_id), chr(not collision), chr(height), chr(type),
            chr(effect));

class Map:
    """
    Holds data about a single map file, including an array-list of tiles belonging to the map.
    """
    title       = "";
    map_width   = 0;
    map_height  = 0;
    tiles       = [];
    reporter    = None;
    sum         = '';
    filesize    = 0;
    raw_data    = '';
    game_modes  = [];
    version     = 0;
    
    def __init__(self, reporter = None):
        """
        Creates a new Map object, but does not set it's data.
        @param reporter Can optionally add a reporter for debugging.
        """
        self.reporter = reporter;
    
    def report(self, message):
        """
        If a reporter was enabled, will print out a message to that reporter.
        @param message Message to print. 
        """
        if self.reporter:
            self.reporter(message);
    
    def create_from_ice_object(self, ice_map):
        """
        Helper function for converting a VTankObject::Map to a natively recognized map.
        @param ice_map VTankObject::Map object.
        """
        out = StringIO();
        out.write(chr(ice_map.version));
        out.write(ice_map.title);
        out.write('\n');
        out.write(int_to_dword(ice_map.width));
        out.write(int_to_dword(ice_map.height));
        out.write("".join([ chr(x) for x in ice_map.supportedGameModes ])); 
        out.write('\n');
        
        for tile in ice_map.tileData:
            out.write(int_to_dword(tile.id));
            out.write(short_to_word(tile.objectId));
            out.write(short_to_word(tile.eventId));
            out.write(chr(tile.passable));
            out.write(chr(tile.height));
            out.write(chr(tile.type));
            out.write(chr(tile.effect));
            
        self.read_map(out.getvalue());
    
    def read_map(self, data):
        """
        Will read binary map data (presumably retrieved from the database) to load an array of
        tiles into this map class.
        @param data Unmodified binary map data to read.
        @return True if the map was read just fine, False if an error occurred and the map is
        corrupted. 
        """
        try:
            if not data: 
                self.report("Data is empty: cannot create map.");
                return False;
            self.tiles = [];
            
            self.sum            = sha1(data).hexdigest();
            self.filesize       = len(data);
            self.raw_data       = data;
            
            self.version        = ord(data[0]);
            if self.version != FORMAT_VERSION:
                raise RuntimeError, "Invalid map version: " + self.version;
            pos = 1;
            for n in xrange(1, len(data)):
                pos += 1;
                if data[n] == '\n':
                    break;
                self.title += data[n];
            
            self.map_width      = bytes_to_int([ data[x] for x in xrange(pos, pos + 4) ]);
            pos += 4;
            self.map_height     = bytes_to_int([ data[x] for x in xrange(pos, pos + 4) ]);
            pos += 4;
            
            self.game_modes     = [];
            
            while True:
                mode = data[pos];
                pos += 1;
                if mode == '\n':
                    break;
                
                self.game_modes.append(int(ord(mode)));
            
            tile_data = data[ pos : ];
            
            self.report("Reading map %s, size: %s bytes." % (self.title, self.filesize));
            
            # Tile format:
            # [4] unsigned - Tile ID
            # [2] short - Object ID
            # [2] short - Event ID
            # [1] byte - Is Passable (False == collision)
            # [1] byte - Height
            # [1] byte - Type
            # [1] byte - Effect
            tile_size = 12; # in bytes
            for data_pos in xrange(0, len(tile_data), tile_size):
                tile_id     = bytes_to_int(tile_data[data_pos : data_pos + 4]);
                object_id   = bytes_to_short(tile_data[data_pos + 4 : data_pos + 6]);
                event_id    = bytes_to_short(tile_data[data_pos + 6 : data_pos + 8]);
                collision   = ord(tile_data[data_pos + 8 : data_pos + 9]) != 0;
                height      = ord(tile_data[data_pos + 9 : data_pos + 10]);
                type        = ord(tile_data[data_pos + 10 : data_pos + 11]);
                effect      = ord(tile_data[data_pos + 11 : data_pos + 12]);
                
                t = Tile(tile_id, object_id, event_id, collision, height, type, effect);
                self.tiles.append(t);
                
        except Exception, e:
            self.report("Exception while reading map data: %s." % (str(e)));
            return False;
        
        return True;
    
    def get_title(self):
        """
        Get the title of the map.
        @return String.
        """
        return self.title;
    
    def get_size(self):
        """
        Returns a tuple containing the (width, height) of the map.
        @return Tuple.  The first value is the width of the map.  The second value is th height.
        """
        return (self.map_width, self.map_height);
    
    def get_size_in_bytes(self):
        """
        Returns the size of the map (and it's raw data) in bytes.
        @return Integer.
        """
        return self.filesize;
    
    def checksum(self):
        """
        Returns the calculated checksum of the map.  This is a cached value, so don't worry
        about calling it repeatedly.
        @return Calculated checksum of the map.  Must have a map loaded.
        """
        return self.sum;
    
    def get_raw_data(self):
        """
        Returns the map as binary data, ready to be saved without modification.
        @return Binary data.
        """
        return self.raw_data;
