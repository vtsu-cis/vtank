/**
	@file MapEditorSession.ice
	@brief Allow the user to modify maps in the database via session methods.
	@author (C) Copyright 2009 by Vermont Technical College
*/

#ifndef MAPEDITORSESSION_ICE
#define MAPEDITORSESSION_ICE

#include <Glacier2/Session.ice>
#include <Exception.ice>
#include <VTankObjects.ice>

/**
    Define interfaces and methods that allow a user to log in from the map editor and
    upload or download maps.
*/
module MapEditor
{
    /**
        The map editor can log in through the main server and call functions from the
        MapEditorSession Ice object to perform server-side operations on maps in the
        database.
    */
    interface MapEditorSession extends Glacier2::Session
    {
        /**
            The client can prevent himself from being disconnected by calling this
            frequently.
        */
        ["ami"] void KeepAlive();
    
        /**
            Get a list of maps from the server.
            @return Sequence of strings, which hold the map names for each map in the
            database.
        */
        VTankObject::MapList GetMapList();
        
        /**
            Download a map from the server.
            @param mapName Name of the map.
            @return Map object which holds the tile data.
            @throws BadInformationException Thrown if the map does not exist.
        */
        VTankObject::Map DownloadMap(string mapName) 
            throws Exceptions::BadInformationException;
        
        /**
            Upload a map to the server. If the map name already exists in the server,
            that entry is overwritten with the uploaded one.
            @param map Map to upload.
        */
        void UploadMap(VTankObject::Map map)
            throws Exceptions::BadInformationException;
        
        /**
            Ask the server to delete a map, identified by it's name.
            @param mapName Name of the map.
            @throws BadInformationException THrown if the map does not exist.
        */
        void RemoveMap(string mapName) throws Exceptions::BadInformationException;
    };
};

#endif
