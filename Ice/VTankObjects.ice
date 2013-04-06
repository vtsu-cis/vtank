/**
	@file	VTankObjects.ice
	@brief	General-purpose objects used in VTank.
	@author (C) Copyright 2009 by Vermont Technical College
*/

#ifndef VTANKOBJECTS_ICE
#define VTANKOBJECTS_ICE

/**
    Module for any objects used in VTank.  This does not restrict itself to objects used
    in game: it can also be generic structs or utility objects used in other components
    of VTank.
*/
module VTankObject
{
    /**
        List of supported game modes.
    */
    enum GameMode
    {
        DEATHMATCH,
        TEAMDEATHMATCH,
        CAPTURETHEFLAG,
        CAPTURETHEBASE
    };
    
    /**
        Structure which holds data about a version -- it's major, minor, build, and
        revision number. A version number looks like this: 0.0.0.0 - where the first
        digit is the major number, the second is the minor number, the third is the
        build number, and the forth is the revision number.
    */
    struct Version
    {
        int major;
        int minor;
        int build;
        int revision;
    };
    
    /**
        Structure which holds information about a server.
    */
    struct ServerInfo
    {
        string host;
        int port;
        string name;
        bool approved;
        bool usingGlacier2;
        int players;
        int playerLimit;
        string currentMap;
        VTankObject::GameMode gameMode;
    };
    
    /** Allows the server to send the client a list of game servers. */
    sequence<VTankObject::ServerInfo> ServerList;

    /**
        Structure that simply holds the RGB values of a color.
    */
    struct VTankColor
    {
        int red;
        int green;
        int blue;
    };
    
    /**
        Store a single (x, y) coordinate.
    */
    struct Point
    {
        double x;
        double y;
    };
    
    /**
        Structure that defines what properties belong to utilities.  A VTank
        utility is a kind of "power-up" or "power-down", depending on the
        effect of the utility.
    */
    struct Utility
    {
        int utilityId;
		float duration;
		float damageFactor;
		float speedFactor;
		float rateFactor;
		int healthIncrease;
		float healthFactor;
        string utilityName;
		string model;
		string description;
    };
	
	/** Array list of utilities. */
	sequence<VTankObject::Utility> UtilityList;
	
	/**
		Describes a relationship between utilities and who is currently using them.
		This is used for Theater to 
	*/
	struct UtilityMap
	{
		int playerId;
		VTankObject::Utility utility;
		VTankObject::Point position;
		float durationRemaining;
	};
    
    /**
        The Tank class holds information about a tank used by a player.  The information
        is important since it affects how the player performs in-game.
    */
    struct TankAttributes
    {
        string name;
        float speedFactor;
        float armorFactor;
        long points;
		int health;
        string model;
		string skin;
        int weaponID;
        VTankObject::VTankColor color;
    };
    
    /**
        Store attributes for a map tile.
    */
    struct Tile
    {
        int id;
        int objectId;
        int eventId;
        bool passable;
        short height;
        short type;
        short effect;
    };
    
    /** Array list of tiles. */
    sequence<VTankObject::Tile> TileList;
	
	/** Array list of game modes supported in a Map. */
    sequence<int> SupportedGameModes;
    
    /**
        A map object is basically a collection of tiles. The information associated with a Map
        follows that handled by the map editor.
    */
    struct Map
    {
        byte version;
        string filename;
        string title;
        int    width;
        int    height;
        VTankObject::SupportedGameModes supportedGameModes;
        VTankObject::TileList tileData;
    };
    
    /** Define a type for an array list of tanks. */
    sequence<VTankObject::TankAttributes> TankList;
    
    /** Array list of strings which holds the names of maps. */
    sequence<string> MapList;
    
    /** Direction enumeration represents rotating and moving forward/backward. */
    enum Direction
    {
        NONE,
        FORWARD,
        REVERSE,
        LEFT,
        RIGHT
    };
    
    /** Statistics relating to the player. */
    struct Statistics
    {
        int kills;
        int assists;                // Number of times this tank helped kill a tank.
        int deaths;                 // Number of times this tank died.
        int objectivesCompleted;    // Number of times this tank completed an objective.
        int objectivesCaptured;     // Number of times this tank captured an objective.
        int calculatedPoints;       // Estimated points to advance a player's rank.
        string tankName;            // Tank who these statistics pertain to.
    };
    
    /** Array list of statistics. */
    sequence<VTankObject::Statistics> StatisticsList;
	
	/** Array list of tank names */
	sequence<string> TankNameList;
	
	/** Array list of ranks */
	sequence<int> TankRankList;
	
	/** Array list of process names (e.g. process.exe). */
	sequence<string> ProcessList;
	
	/**
		Represents a snapshot of a health given by a backup server.
	*/
	struct HealthSnapshot
	{
		float cpuUsage;
		long hdUsedBytes;
		long hdCapacityBytes;
		long memoryUsedBytes;
		long memoryCapacityBytes;
		ProcessList runningProcesses;
		string additionalNotes;
	};
};

#endif
