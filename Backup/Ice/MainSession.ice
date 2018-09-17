/**
	@file MainSession.ice
	@brief Communication structure between any authenticated client and the main server.
	@author (C) Copyright 2009 by Vermont Technical College
*/

#ifndef MAINSESSION_ICE
#define MAINSESSION_ICE

#include <VTankObjects.ice>
#include <Exception.ice>
#include <Glacier2/Session.ice>

/**
    Continuation of Main server interfaces.
*/
module Main
{
    /**
        The main session holds methods that only users who have logged into the server
        can access.
    */
    interface MainSession extends Glacier2::Session
    {
        /**
            The client can prevent himself from being disconnected by calling this
            frequently.
        */
        ["ami"] void KeepAlive();
    
        /**
            Notify the server that the client intends to leave.
        */
        ["ami"] void Disconnect();
    
        /**
            Allow the user access to his or her tank list.
            @return Array list of tanks that belong to the user.  May be empty.
        */
        ["ami"] VTankObject::TankList GetTankList();
        
        /**
            The user will attempt to create a tank.
            @param tank Tank to create.
            @return True if the creation had no problems.  False if it was not allowed.
        */
        ["ami"] bool CreateTank(VTankObject::TankAttributes tank) 
            throws Exceptions::BadInformationException;
        
        /**
            Update information for an existing tank.
            @param oldTankName Identifier for the old tank, so the server knows which
            one to replace.
            @param newTank New tank object to use in place of the old one.
            @return True if the creation had no problems.  False if it was not allowed.
        */
        ["ami"] bool UpdateTank(string oldTankName, VTankObject::TankAttributes newTank)
            throws Exceptions::BadInformationException;
        
        /**
            Delete an existing tank.
            @param tankName Name of the tank to delete.
            @return True if the tank was found and deleted, false if the tank was not
            deleted for some reason.
            @throws BadInformationException Thrown when the tank does not exist.
        */
        ["ami"] bool DeleteTank(string tankName) throws Exceptions::BadInformationException;
        
        /**
            Allows the client to select a tank to use on the server.
            @param tankName Name of the tank to select.
            @return True if the tank was selected without issues.
            @throws BadInformationException Thrown when the tank does not exist.
        */
        ["ami"] bool SelectTank(string tankName) throws Exceptions::BadInformationException;
        
        /**
            Grabs a list of game servers that the main server knows about.
            @return Sequence of servers' information.
        */
        ["ami"] VTankObject::ServerList GetGameServerList();
        
       
        /**
            Ask the main server if the client can join a game server.
            @param server Server the client wishes to join.
            @return If the main server will let the client join a target game server,
            the main server will generate a unique key and send it back to the client.
            The client must use this key to create a game session with the game server.
            @throws VTankException Thrown if any error occurs -- either the server
            isn't available, or it's full.
        */
        ["ami"] string RequestJoinGameServer(VTankObject::ServerInfo server)
            throws Exceptions::VTankException;
        
        /**
            Download a map from the server.
            @param mapName Name of the map you wish to download.
            @return The entire map file.
        */
        ["ami"] VTankObject::Map DownloadMap(string mapName);
        
        /**
            Download a list of maps from the server.
            @return Array list of maps.
        */
        ["ami"] VTankObject::MapList GetMapList();
        
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
            An interface for pushing feedback to the server. The feedback can be 
            suggestions or bug reports. The resulting feedback is sent to the administrator.j
            @param topic Topic of the feedback.
            @param message Message to send.
        */
        ["ami"] void SendFeedback(string topic, string message);
		 
		/**
			Get the ranks of a set of tanks.
			@param tanks A sequence of tank names
			@return A sequence of integers representing their ranks.
		*/
		["ami"] VTankObject::TankRankList GetRanksOfTanks(VTankObject::TankNameList tanks);
		
		/**
			Get the current rank of a certain account.
			@return The rank as an integer.
		*/
		["ami"] int GetRank();
		
		/**
			Get the number of points required for a certain rank.
			@param rankNumber The number corresponding the rank in question.
			@return The number of points needed for that rank.
		*/
		["ami"] long GetPointsForRank(int rankNumber);
		
		/**
			Get the total points an account has.
			@return The total number of points a tank has.
		*/
		["ami"] long GetAccountPoints();
		
		/**
		    Get the most recent version of the VTank client.
			@return A string describing the most recent client version.
		*/
		["ami"] string CheckClientVersion();
    };
};

#endif
