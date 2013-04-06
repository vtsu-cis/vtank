/*!
    \file   ServerCommunication.cpp
    \brief  Iterface for the official VTank Map Editor main form.
    \author (C) Copyright 2009 by Vermont Technical College
*/

#include "Library.hpp"
#include "ServerCommunication.hpp"
#include "VTankObjects.h"
#include "MapEditorSession.h"
#include "Main.h"
#include "Exception.h"
#include "Map.hpp"
#include "vtassert.hpp"


namespace ServerCommunication {

    Ice::CommunicatorPtr comm;
    MapEditor::MapEditorSessionPrx me_session_prx;
    Glacier2::RouterPrx router;
    std::string cred_login;
    std::string cred_pass;

    /*! Initialize Ice with Gardener's configurations */
    bool initialize_communicator()
    {
        try {
            Ice::StringSeq args;
            args.push_back("--Ice.Config=config.gardener");
            comm = Ice::initialize(args);
            Ice::ObjectPrx p = comm->stringToProxy(comm->getProperties()->getProperty("VTankRouter"));
            router = Glacier2::RouterPrx::checkedCast(p);
            return true;
        }
        catch (const Ice::InitializationException &ex) {
            (void)wxMessageBox(wxString(ex.reason.c_str(), wxConvUTF8),
                               wxT("Can't initialize Ice."), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::FileException &) {
            (void)wxMessageBox(wxString("Configuration file config.gardener can't be found", wxConvUTF8),
                               wxT("Ice file I/O error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::DNSException&) {
            (void)wxMessageBox(wxT("Server cannot be reached."),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::Exception &) {
            (void)wxMessageBox(wxT("An unknown error has occured during communicator initialization."),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        return false;
    }


    /*! Deinitialize Ice */
    void deinitialize_communicator()
    {
        try {
            deinitialize_session();
            if(router)
                router->destroySession();
            if(comm)
                comm->destroy();
        }
        catch (Ice::ConnectionLostException&) {
            return;
        }
        catch (Glacier2::SessionNotExistException&) {
            return;
        }
        catch (Ice::CommunicatorDestroyedException&) {
            return;
        }
        catch (const Ice::DNSException&) {
            return;
        }
    }
    

    /*! Destroys the Map Editor Session Proxy */
    void deinitialize_session()
    {
        if(me_session_prx) {
            try {
                me_session_prx->destroy();
            }
            catch (Ice::CommunicatorDestroyedException&) {
                return;
            }
            catch (const Ice::DNSException&) {
                return;
            }
        }
    }

     /*!
     * Sends the login to the server and attemps to start a session.
     *
     * \param login The user's server username
     * \param pass The user's server password
     */
    bool send_login(const std::string &login,const std::string &pass)
    {
        try {
            Glacier2::SessionPrx session = router->createSession(login, "");
            Main::SessionFactoryPrx mainprx = Main::SessionFactoryPrx::uncheckedCast(session->ice_router(router));
            me_session_prx = MapEditor::MapEditorSessionPrx::uncheckedCast(
                mainprx->MapEditorLogin(login, pass)->ice_router(router));
            if (cred_login.empty() || cred_pass.empty()) {
                cred_login = login;
                cred_pass = pass;
            }
            return true;
        }
        catch (const Glacier2::PermissionDeniedException&) {
            (void)wxMessageBox(wxString("Permission Denied", wxConvUTF8),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Glacier2::CannotCreateSessionException&) {
            (void)wxMessageBox(wxString("Session with Server can not be created", wxConvUTF8),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Exceptions::VTankException &ex) {
            (void)wxMessageBox(wxString(ex.reason.c_str(), wxConvUTF8),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::TimeoutException &) {
            (void)wxMessageBox(wxString("Connection to server lost", wxConvUTF8),
                               wxT("Error!"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::DNSException&) {
            (void)wxMessageBox(wxT("Server cannot be reached."),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::ConnectionRefusedException&) {
            (void)wxMessageBox(wxT("Connection refused by the target machine."),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::ConnectFailedException&){
            (void)wxMessageBox(wxT("Server unreachable. Please try again later."),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::UnknownException&) {
            (void)wxMessageBox(wxString("Unknown Error", wxConvUTF8),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const IceUtil::Exception &ex) {
            (void)wxMessageBox(wxString(ex.what(), wxConvUTF8),
                               wxT("Unknown Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        deinitialize_session();
        return false;
    }

/*!
     * Uploads a maps to the server.
     *
     * \param current_map The map to be uploaded to the server
     * \param map_name The name of the map to be uploaded
     */
    bool upload_map(const Map *const current_map, const std::string &map_name)
    {
        VTankObject::TileList data;
        VTankObject::Map vtmap;

        if(current_map->get_supported_game_modes().empty()) {
            (void)wxMessageBox(wxString("Map doesn't contain supported game modes\n"
                "Map failed to upload.", wxConvUTF8), wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
            return false;
        }

        const int current_width = current_map->get_width();
        const int current_height = current_map->get_height();

        VTANK_ASSERT(current_width > 0);
        VTANK_ASSERT(current_height > 0);
        //lint -save -e732
        
        data.resize(current_width * current_height);

        vtmap.version   = current_map->get_version();
        vtmap.height    = current_height;
        vtmap.width     = current_width;
        vtmap.title     = current_map->get_title();
        vtmap.filename  = map_name;

        const std::vector<int> modes = current_map->get_supported_game_modes();        for (std::vector<int>::size_type i = 0; i < modes.size(); i++) {            vtmap.supportedGameModes.push_back(modes[i]);        }        for (int y = 0; y < current_height; y++) {
            for (int x = 0; x < current_width; x++) {
                VTankObject::Tile tile;
                const Tile temp_tile = current_map->get_tile(x, y);
                tile.id         = temp_tile.tile_id;
                tile.objectId   = temp_tile.object_id;
                tile.eventId    = temp_tile.event_id;
                tile.passable   = (temp_tile.passable != 0);
                tile.height     = temp_tile.height;
                tile.type       = temp_tile.type;
                tile.effect     = temp_tile.effect;

                data[y * current_width + x] = tile;
            }
        }
        vtmap.tileData = data;
        //lint -restore

        try {
            me_session_prx->UploadMap(vtmap);
            return true;
        }
        catch (const Exceptions::BadInformationException &) {
            (void)wxMessageBox(wxT("Map name already exists."),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::ConnectionLostException &ex) {
            (void)wxMessageBox(wxString(ex.what(), wxConvUTF8),
                               wxT("Error!"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::TimeoutException &) {
            (void)wxMessageBox(wxString("Connection to server lost", wxConvUTF8),
                               wxT("Error!"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::DNSException &) {
            (void)wxMessageBox(wxString("Server can not be reached", wxConvUTF8),
                               wxT("Error!"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::MemoryLimitException &) {
            (void)wxMessageBox(wxT("The map is too big!"),
                               wxT("Error!"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        return false;
    }

    /*!
     * Get a list of maps from the server.
     *
     * \return Sequence of strings, which hold the map names for each map in the database.
     */
    VTankObject::MapList get_map_list()
    {
        return me_session_prx->GetMapList();
    }
    

    /*!
     * Download a map from the server.
     *
     * \param map_name Name of the map.
     * \return Map object which holds the tile data.
     * \throws BadInformationException Thrown if the map does not exist.
     */ 
    Map download_map(const std::string &map_name)
    {
        Map vtmap = Map();
        try {
            VTankObject::Map icemap = me_session_prx->DownloadMap(map_name);         
            if(!vtmap.create(icemap.width, icemap.height, icemap.title)) {
                return Map();
            }
            for (int y = 0; y < icemap.height; y++) {
                for (int x = 0; x < icemap.width; x++) {
                   const VTankObject::Tile temp = icemap.tileData[static_cast<unsigned>(y * icemap.width + x)];
                   //lint -save -e534
                   vtmap.set_tile_id(x, y, temp.id);
                   vtmap.set_tile_object(x, y, temp.objectId);
                   vtmap.set_tile_event(x, y, temp.eventId);
                   vtmap.set_tile_collision(x, y, temp.passable);
                   vtmap.set_tile_height(x, y, temp.height);
                   vtmap.set_tile_type(x, y, temp.type);
                   vtmap.set_tile_effect(x, y, temp.effect);
                   //lint -restore
                }
            }

            for (VTankObject::SupportedGameModes::size_type i = 0;
                 i < icemap.supportedGameModes.size(); i++) {                    
                     vtmap.add_supported_game_mode(icemap.supportedGameModes[i]);            
            }            
            
            return Map(vtmap);
        }
        catch (const Exceptions::BadInformationException &) {
            (void)wxMessageBox(wxT("That map does not exist."),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::TimeoutException &) {
            (void)wxMessageBox(wxString("Connection to server lost", wxConvUTF8),
                               wxT("Error!"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::DNSException&) {
            (void)wxMessageBox(wxT("Server cannot be reached."),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        return Map();
    }

    
    /*!
     * Ask the server to delete a map, identified by its name.
     *
     * \param map_name Name of the map.
     * \return true if the map was removed successfully; false otherwise.
     */
    bool remove_map(const std::string &map_name)
    {
        try {
            me_session_prx->RemoveMap(map_name);
            return true;
        }
        catch (const Exceptions::BadInformationException &) {
            (void)wxMessageBox(wxT("That map does not exist."),
                               wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::TimeoutException &) {
            (void)wxMessageBox(wxString("Connection to server lost", wxConvUTF8),
                               wxT("Error!"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        catch (const Ice::DNSException &) {
            (void)wxMessageBox(wxString("Server can not be reached", wxConvUTF8),
                               wxT("Error!"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        return false;
    }

    
    /*!
     * The client can prevent himself from being disconnected by calling this frequently.
     */
    void keep_alive()
    {   
        try {
            while(true) {
                me_session_prx->KeepAlive();
                boost::this_thread::sleep(boost::posix_time::milliseconds(30000LL));
            }
        }
        catch (const Ice::ConnectionLostException&) {
            (void)wxMessageBox(wxString("Connection to Server Lost", wxConvUTF8),
                               wxT("Error!"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
            deinitialize_session();
        }
        catch (const Ice::Exception &) {
        }
    }
}
