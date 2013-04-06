/*!
    \file   MapEditorFrame.hpp
    \brief  Definition of the Map Editor frame class.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef MAPEDITORFRAME_HPP
#define MAPEDITORFRAME_HPP

#include "Map.hpp"
#include "OpenMap.hpp"
#include "ScrolledWindow.hpp"
#include "ToolboxWindow.hpp"
#include "ServerCommunication.hpp"

const char *const TERRAIN_DICTIONARY = "tile_dictionary.txt";
const char *const OBJECT_DICTIONARY = "object_dictionary.txt";
const char *const EVENT_DICTIONARY = "event_dictionary.txt";

struct LoadedTile {
    int tile_id;
    wxString file_name;
    LoadedTile() : tile_id(0), file_name(wxT("")) {}
    LoadedTile(const int id, wxString name) : tile_id(id), file_name(name) {}
};
//! Main frame for the map editor program.
class MapEditorFrame: public wxFrame {
    //lint -save -e1516
    DECLARE_EVENT_TABLE()
    //lint -restore

public:
    MapEditorFrame(wxFrame *frame, const wxString &title);
   ~MapEditorFrame();

    void set_editor_title(const wxString &m_title);

private:
    enum {
        idMenuNew = 1000   ,
        idMenuOpen         ,
        idMenuSave         ,
        idMenuSaveAs       ,
        idMenuClose        ,
        idMenuQuit         ,
        idMenuCut          ,
        idMenuCopy         ,
        idMenuPaste        ,
        idMenuDelete       ,
        idMenuSelectAll    ,
        idMenuViewGrid     ,
        idMenuViewDisp     ,
        idMenuViewHeight   ,
        idMenuFullScreen   ,
        idMenuTerrain      ,
        idMenuObject       ,
        idMenuEvent        ,
        idMenuProp         ,
        idMenuGameModes    ,
        idMenuMapList      ,
        idMenuPointer      ,
        idMenuFill         ,
        idMenuSelect       ,
        idMenuRectangle    ,
        idMenuAbout        ,
        idMenuHelp         ,
        idToolNew          ,
        idToolOpen         ,
        idToolSaveAs       ,
        idToolSave         ,
        idToolServer       ,
        idToolProp         ,
        idToolGame         ,
        idToolTer          ,
        idToolObj          ,
        idToolEvent        ,
        idToolCut          ,
        idToolCopy         ,
        idToolPaste        ,
		idToolPointer      ,
        idToolfill         ,
        idToolselgrp       ,
        idToolfillgrp      ,
        idToolgrid         ,
        idToolcol          ,
        idToolheight       ,
        idHeightTxt
    };
    std::map<int, wxBitmap> terrain_dictionary;
    std::map<int, wxBitmap> object_dictionary;
    std::map<int, wxBitmap> event_dictionary;

    void editor_startup();
    void save_map(bool save_as);
    void load_dictionary(const std::string DICTIONARY);

    // Event handlers.
    void OnSize             (wxSizeEvent    &event);
    void on_paint           (wxPaintEvent   &event);
    void OnQuit             (wxCommandEvent &event);
    void OnAbout            (wxCommandEvent &event);
    void OnClose            (wxCloseEvent   &event);
    void OnMenuNew          (wxCommandEvent &event);
    void OnOpenMap          (wxCommandEvent &event);
    void OnSaveMap          (wxCommandEvent &event);
    void OnCloseMap         (wxCommandEvent &event);
    void OnOpenMapProperties(wxCommandEvent &event);
    void OnSaveAs           (wxCommandEvent &event);
    void on_terrain         (wxCommandEvent const &event);
    void on_object          (wxCommandEvent const &event);
    void on_event           (wxCommandEvent const &event);
    void on_server_map_list (wxCommandEvent &event) const;
    void on_game_mode       (wxCommandEvent &event);
    void on_cut             (wxCommandEvent &event);
    void on_copy            (wxCommandEvent &event);
    void on_paste           (wxCommandEvent &event);
    void on_pointer         (wxCommandEvent const &event);
    void on_fill            (wxCommandEvent const &event);
    void on_select_group    (wxCommandEvent const &event);
    void on_fill_group      (wxCommandEvent const &event);
    void on_select_all      (wxCommandEvent &event);
    void on_delete          (wxCommandEvent &event);
    void on_view_grid_toggle(wxMenuEvent    const &event);
    void on_view_collision_toggle(wxMenuEvent const &event);
    void on_view_height_toggle(wxMenuEvent  const &event);
    void on_full_screen     (wxCommandEvent &event);
    void on_help            (wxCommandEvent &event);

    bool view_show_grid;
    bool view_show_collision;
    bool view_show_height;
    bool change_size;

    ToolboxWindow  *toolbox_window;
    ScrolledWindow *scrolled_window;
    OpenMap *current_map;

    wxPanel    *canvas;
    wxHtmlHelpController help;
    wxToolBar *tool_bar;
};


#endif
