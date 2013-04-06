/**
	@file IGame.ice
	@brief Communication structure between the VTank game client and the server.
	@author (C) Copyright 2009 by Vermont Technical College
*/

#ifndef IGAME_ICE
#define IGAME_ICE

#include <Exception.ice>
#include <GameSession.ice>
#include <ClockSync.ice>
#include <Glacier2/Session.ice>

/**
    The Game module holds methods that the client calls from the server to interact with
    it.  Likewise, the client holds callback methods that lets the 
*/
module IGame
{
    /**
        The Auth interface is the only interface on the game server that's open to the 
        world.  The client will communicate with this interface initially to create
        a session.
    */
    interface Auth extends Glacier2::SessionManager
    {
        /**
            Join the game server, enabling access to session-only methods.  Before
            the client calls this, the client should call 
            MainSession::RequestJoinGameServer.
            @param key Key retrieved from the MainSession::RequestJoinGameServer
            method.
            @param callback Callback object that the server calls to notify the client
            of actions.
            @return Pointer to a GameSession object, enabling access to in-game-only
            methods.
            @throws PermissionDeniedException Thrown if the client passed in an
            invalid, unrecognized, or expired key.
        */
        GameSession::GameInfo * JoinServer(string key, 
            GameSession::ClockSynchronizer * clock,
            GameSession::ClientEventCallback * callback) 
            throws Exceptions::PermissionDeniedException;
    };
};

#endif
