/*!
    \file   ServerCommunication.hpp
    \brief  Iterface for the official VTank Map Editor main form.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef SERVERCOMMUNICATION_HPP
#define SERVERCOMMUNICATION_HPP

#include "Map.hpp"
#include "VTankObjects.h"
#include "MapEditorSession.h"

//! Client to server interface provides a bridge between the map editor and the client.
/*!
 * Takes map information, as well as login data, and attempts a login to the server. On
 * success, the map is pushed to the server.
 */
namespace ServerCommunication {

    extern Ice::CommunicatorPtr comm;
    extern MapEditor::MapEditorSessionPrx me_session_prx;
    extern Glacier2::RouterPrx router;
    extern std::string cred_login;
    extern std::string cred_pass;

    bool initialize_communicator();
	void deinitialize_communicator();
    void deinitialize_session();

    bool send_login(const std::string &pass,const std::string &name);
    bool upload_map(const Map *current_map,  const std::string &map_name);
    Map download_map(const std::string& mapName);
	bool remove_map(const std::string& mapName);
	VTankObject::MapList get_map_list();
	void keep_alive();
}

#endif
