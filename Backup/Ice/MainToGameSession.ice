/**
	@file MainToGameSession.ice
	@brief Communication between the main and game server.
	@author (C) Copyright 2009 by Vermont Technical College
*/

#ifndef MAINTOGAMESESSION_ICE
#define MAINTOGAMESESSION_ICE

#include <Glacier2/Session.ice>
#include <VTankObjects.ice>

/**
    Holds interfaces that allow the game server to communicate with the main server
    (Echelon) and vice versa.
*/
module MainToGameSession
{
    /**
        Session interface for the client (the game server).  The game server can
        configure certain options to let the main server know certain things, such as
        the maximum number of players allowed by the game server.
        
        If the server wishes, it can override some options.
    */
    interface MTGSession extends Glacier2::Session
    {
        /**
            The client can prevent himself from being disconnected by calling this
            frequently.
        */
        ["ami"] void KeepAlive();
        
        /**
            Tell the main server what it's preferred limit is.
            @param limit The limit of players.
        */
        ["ami"] void SetMaxPlayerLimit(int limit);
        
        /**
            Notify the server that a player has left the game.
            @param accountName Name of the player who left.
        */
        ["ami"] void PlayerLeft(string accountName);
        
        /**
            Send player statistics to the server. The rank of each tank is put into
            consideration given these statistics. As such, only statistics received
            by approved game servers will be considered.
            Game servers are responsible for calculating how many points a player should
            earn. Since approved game servers are trusted, the server will likely 
            accept these values point blank.
            @param statistics List of statistics.
        */
        ["ami"] void SendStatistics(VTankObject::StatisticsList statistics);
        
        /**
            Set which map is being played on.
            @param mapName Name of the map being played.
        */
        ["ami"] void SetCurrentMap(string mapName);
        
        /**
            Set which mode is being played.
            @param mode Mode in progress.
        */
        ["ami"] void SetCurrentGameMode(VTankObject::GameMode mode);
		
		/**
            Download a map from the server.
            @param mapName Name of the map you wish to download.
            @return The entire map file.
        */
        ["ami"] VTankObject::Map DownloadMap(string mapName);
        
        /**
            The client can check if a local calculated hash for a map is valid. If it isn't, 
            the map should be re-downloaded, because the map file has been corrupted or
            is outdated.
            @param mapFileName Name of the map.
            @param hash Local calculated SHA-1 hash.
            @return True if the integrity check passed; false otherwise.
        */
        ["ami"] bool HashIsValid(string mapFileName, string hash);
		
		/**
			Get the current rank of a player's tank.
			@param tankName Name of the tank.
			@return His rank as an integer.
		*/
		["ami"] int GetRank(string tankName);

    };
    
    /**
        The ClientSession has the client (the game server) implementing methods that the
        main server will call.
    */
    interface ClientSession extends Glacier2::Session
    {
        /**
            The client can prevent himself from being disconnected by calling this
            frequently.
        */
        ["ami"] void KeepAlive();
        
        /**
            Update the game server's list of maps.
            @param mapList String list of maps.
        */
        ["ami"] void UpdateMapList(Ice::StringSeq mapList);
		
		/**
			Update the game server's list of available utilities.
			@param Utility list.
		*/
		["ami"] void UpdateUtilities(VTankObject::UtilityList utilities);
        
        /**
            Add a player to the game server.
            @param key Unique hash key that the client must authenticate with.
            @param username Client's username.
            @param userlevel Userlevel of the user. This may allow users to execute certain
                commands on the server.
            @param attr Client's tank attributes.
        */
        ["ami"] void AddPlayer(string key, string username, int userlevel, 
            VTankObject::TankAttributes attr);
        
        /**
            Remove a player from the server.
            @param username User to kick.
        */
        ["ami"] void RemovePlayer(string username);
        
        /**
            Force the game server to handle a certain amount of players.
            @param limit Number of players to handle.
        */
        ["ami"] void ForceMaxPlayerLimit(int limit);
        
        /**
            Ask the game server how many players it's willing to accept.
            @return Limit according to the game server.
        */
        ["ami"] int GetPlayerLimit();
    };
};

#endif
