/**
	@file Main.ice
	@brief Communication structure between any client and the main server.
	@author (C) Copyright 2009 by Vermont Technical College
*/

#ifndef MAIN_ICE
#define MAIN_ICE

#include <MainSession.ice>
#include <VTankObjects.ice>
#include <Exception.ice>
#include <MapEditorSession.ice>
#include <CaptainVTank.ice>
#include <MainToGameSession.ice>
#include <HealthMonitor.ice>

/**
    The Main module controls how clients can talk to the main server.  
*/
module Main
{
    /**
        Define methods which allow clients to perform any actions within the server.
        Before the server can allow the client do anything, the client must identify
        itself by it's username and password via the SessionFactory.
        This interface allows a client to create a new account.
    */
    interface Auth
    {   
        /**
            Create an account on the server.
            @param username Account name the user wishes to create.
            @param password Password the user uses to identify the account.
            @param email E-mail address of the client.
            @return True if the account creation was accepted.
            @throws BadInformationException Thrown when one of the fields is invalid.
            This can mean either the information is used already (non-unique username, 
            for example), or one of the fields is bad.  Usually the client checks for
            "invalid" fields on it's own, so it could be considered suspicious if one
            of the fields is empty or invalid.
        */
        bool CreateAccount(string username, string password, string email)
            throws Exceptions::BadInformationException;
        
        /**
            Provides a way for any unauthorized user to check the current version of the
            game client.
            @return Verison object.
        */
        VTankObject::Version CheckCurrentVersion();
        
        /**
            Ask the server how large of a file it will accept.  This is useful to the
            map editor since it can call this after a map is made to make sure it will
            fit inside of the database.
            @return Maximum size of the map (in bytes), or -1 if no max is set.
        */
        int GetMaxMapSize();


        /**
            Grabs a list of game servers that the main server knows about.
            @return Sequence of servers' information.
        */
        VTankObject::ServerList GetGameServerList();
        
        /**
            Get the number of players, both playing games and not.
            @return Number of players online.
        */
        int GetPlayersOnline();
    };
    
    /**
        The session factory provides session creators for every client type in VTank.
    */
    interface SessionFactory extends Glacier2::SessionManager
    {
	
		/**
			Send the server a short description and a stack trace for a VTank client
			crash.
			@param username The user who encountered the error.
			@param details A short description of what they were doing when it happened.
			@param stackTrace The program's stack trace from the crash.
		*/
        void SendErrorMessage(string username, string details, string stackTrace);
		
        /**
            Login to the main server and create a session for the client.
            @param username Account to use in the session.
            @param password Password identifying the account.
            @param version Game version of the client.
            @return Pointer to a newly created session object on the server.  The client
            will use the object to call new logged-in-only methods on the server.
            @throws PermissionDeniedException Thrown when the client fails the login
            test.
            @throws BadInformationException Thrown if the information handed to the 
            server is invalid.
            @throws BadVersionException Thrown if the version is out of date.
        */
        ["ami"] Main::MainSession * VTankLogin(string username, string password,
                                  VTankObject::Version version) 
            throws Exceptions::PermissionDeniedException, 
                   Exceptions::BadInformationException,
                   Exceptions::BadVersionException;                
        
        /**
            Request to log in to the map server with a session that allows the client
            to manipulate rows in the map table of the database. Only privelledged users
            may use the MapEditorSession.
            @param username Account to use in the session.
            @param password Password identifying the account.
            @return Pointer to a MapEditorSession, which allows the client to interact
            with a different set of methods related to map editing.
            @throws PermissionDeniedException Thrown when the client fails the login
            test or lacks the permissions.
            @throws BadInformationException Thrown if the information handed to the 
            server is invalid. 
        */
        ["ami"] MapEditor::MapEditorSession * MapEditorLogin(string username, 
                                                             string password)
            throws Exceptions::PermissionDeniedException,
                   Exceptions::BadInformationException;
        
        /**
            Login as an administrator. Only the highest-level users are able to login
            through this method.
            @param username Account name.
            @param password Password identifying the account.
            @param version Version of Captain VTank.
            @return Pointer to an admin session object, which allows the client to call
            administrative commands.
            @throws PermissionDeniedException Thrown if the credentials didn't match
            or if the user does not have permissions to use this method.
            @throws BadInformationException Thrown if the given information is invalid.
            @throws BadVersionException Thrown if the admin client is out of date.
        */
        ["ami"] Admin::AdminSession * AdminLogin(string username, string password, 
                                                 VTankObject::Version version)
            throws Exceptions::PermissionDeniedException, 
                   Exceptions::BadInformationException,
                   Exceptions::BadVersionException;
        
        /**
            The Join method is called by the game server when it wishes to join the
            main server.
            @param servername Name of the server joining.
            @param secret Code that lets the main server know that you're a game server.
            @param port Port that this client listens on.
            @param usesGlacier2 True if clients must use a Glacier2 server to connect.
            @param glacier2Host Host name of the Glacier2 server, if applicable.
            @param glacier2Port Port number of the Glacier2 server, if applicable.
            @param client Pointer to a client-created object.
            @return Pointer to a session object, which lets the game server configure
            some options with the main server.
            @throws PermissionDeniedException Thrown if the game server is not allowed
            to join.
            @throws BadInformationException Thrown if given information is invalid.
        */
        ["ami"] MainToGameSession::MTGSession * Join(string servername, string secret,
                                             int port, bool usingGlacier2, 
                                             string glacier2Host, int glacier2Port,
                                             MainToGameSession::ClientSession * client)
            throws Exceptions::PermissionDeniedException,
                   Exceptions::BadInformationException;
		
		/**
			Log in as a health monitor.
			@param username Account name.
			@param password Password of the user logging in.
			@throws PermissionDeniedException Thrown if the user is banned or his credentials do not match.
            @throws BadInformationException Thrown if given information is invalid.
		*/
		["ami"] Monitor::HealthMonitor * HealthMonitorLogin(string username, string password)
			throws Exceptions::PermissionDeniedException,
                   Exceptions::BadInformationException;
    };
};

#endif
