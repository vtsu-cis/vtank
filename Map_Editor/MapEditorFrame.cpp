/*!
    \file   MapEditorFrame.cpp
    \brief  Implementation of the main Map Editor frame class.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#include "Library.hpp"
#include "AboutDialog.hpp"
#include "config.hpp"
#include "MapEditorFrame.hpp"
#include "MapPropertiesDialog.hpp"
#include "NewMapDialog.hpp"
#include "ServerDialog.hpp"
#include "GameModeDialog.hpp"
#include "Support.hpp"
#include "vtassert.hpp"

using namespace std;

//lint -save -e1924
BEGIN_EVENT_TABLE(MapEditorFrame, wxFrame)
    EVT_CLOSE(MapEditorFrame::OnClose)
    EVT_SIZE(MapEditorFrame::OnSize)
END_EVENT_TABLE()
//lint -restore


//! Main constructor
/*!
 * Sets up the main, scrolled, and toolbox windows.
 *
 * \param frame Pointer to the main frame (window) of the application.
 * \param title Title of the window.
 */
MapEditorFrame::MapEditorFrame(wxFrame *frame, const wxString &title)
    : wxFrame(frame,
              -1,
              title,
              wxDefaultPosition,
              wxSize(835,608),
              wxMINIMIZE_BOX | wxSYSTEM_MENU | wxCAPTION | wxCLOSE_BOX | wxMAXIMIZE_BOX | wxRESIZE_BORDER),
      terrain_dictionary  (),
      object_dictionary   (),
      event_dictionary    (),
      view_show_grid      (true),
      view_show_collision (true),
      view_show_height    (false),
      change_size         (false),
      toolbox_window      (NULL),
      scrolled_window     (NULL),
      current_map         (NULL),
      canvas              (NULL),
      help                (),
      tool_bar            (NULL)
{
    load_dictionary(TERRAIN_DICTIONARY);
    load_dictionary(OBJECT_DICTIONARY);
    load_dictionary(EVENT_DICTIONARY);
    
    help.UseConfig(wxConfig::Get());
    bool ret;
    help.SetTempDir(wxT("Gardener Help"));
    ret = help.AddBook(wxFileName(wxT("Gardener Help/gardener.hhp"), wxPATH_UNIX));
    if (! ret)
        (void)wxMessageBox(wxT("Failed adding book GardenerHelp/gardener.hhp"));

    wxMenuBar  *menu_bar;
    wxMenu     *file_menu;
    wxMenuItem *new_menu_item;
    wxMenuItem *open_menu_item;
    wxMenuItem *save_menu_item;
    wxMenuItem *save_as_menu_item;
    wxMenuItem *close_menu_item;
    wxMenuItem *quit_menu_item;
    wxMenu     *edit_menu;
    wxMenuItem *cut_menu_item;
    wxMenuItem *copy_menu_item;
    wxMenuItem *paste_menu_item;
    wxMenuItem *delete_menu_item;
    wxMenuItem *select_all_menu_item;
    wxMenu     *view_menu;
    wxMenuItem *display_grid_menu_item;
    wxMenuItem *display_collision_menu_item;
    wxMenuItem *display_height_menu_item;
    wxMenuItem *full_screen_menu_item;
    wxMenu     *layer_menu;
    wxMenuItem *terrain_menu_item;
    wxMenuItem *object_menu_item;
    wxMenuItem *event_menu_item;
    wxMenu     *options_menu;
    wxMenuItem *map_properties_menu_item;
    wxMenuItem *map_game_modes_menu_item;
    wxMenuItem *map_list_menu_item;
    wxMenu     *tool_menu;
    wxMenuItem *pointer_menu_item;
    wxMenuItem *fill_menu_item;
    wxMenuItem *select_menu_item;
    wxMenuItem *rectangle_menu_item;
    wxMenu     *help_menu;
    wxMenuItem *about_menu_item;
    wxMenuItem *help_menu_item;

    using Support::normalize_path_wx;

    const string *const root_path = Support::lookup_parameter("RESOURCE_ROOT");
    VTANK_ASSERT(root_path != NULL);
    wxString resource_root(root_path->c_str(), wxConvUTF8);

    #if wxUSE_MENUS
    // create a menu bar
    menu_bar        = new wxMenuBar();
    file_menu       = new wxMenu();
    edit_menu       = new wxMenu();
    view_menu       = new wxMenu();
    layer_menu      = new wxMenu();
    options_menu    = new wxMenu(); 
    tool_menu       = new wxMenu();
    help_menu       = new wxMenu();
    

    // Add File->New
    new_menu_item = new wxMenuItem(
        file_menu, idMenuNew, wxT("New\tCtrl-N"), wxT("Create a new map"), wxITEM_NORMAL);
    new_menu_item->SetBitmap(
        wxArtProvider::GetBitmap(wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_NEW")), wxART_OTHER));
    file_menu->Append(new_menu_item);

    file_menu->AppendSeparator();

    // Add File->Open
    open_menu_item = new wxMenuItem(
        file_menu, idMenuOpen, wxT("Open\tCtrl-O"), wxT("Open an existing map"), wxITEM_NORMAL);
    open_menu_item->SetBitmap(
        wxArtProvider::GetBitmap(wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_FILE_OPEN")), wxART_OTHER));
    file_menu->Append(open_menu_item);

    // Add File->Save
    save_menu_item = new wxMenuItem(
        file_menu, idMenuSave, wxT("Save\tCtrl-S"), wxT("Save the current map"), wxITEM_NORMAL);
    save_menu_item->SetBitmap(
        wxArtProvider::GetBitmap(wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_FILE_SAVE")), wxART_MENU));
    file_menu->Append(save_menu_item);

    // Add File->Save As
    save_as_menu_item = new wxMenuItem(file_menu, idMenuSaveAs, wxT("Save As"),
        wxT("Save the current map as a new map"), wxITEM_NORMAL);
    save_as_menu_item->SetBitmap(wxArtProvider::GetBitmap(
        wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_FILE_SAVE_AS")), wxART_OTHER));
    file_menu->Append(save_as_menu_item);

    // Add File->Close
    close_menu_item = new wxMenuItem(
        file_menu, idMenuClose, wxT("Close"), wxT("Close the current map"), wxITEM_NORMAL);
    close_menu_item->SetBitmap(wxArtProvider::GetBitmap(
        wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_QUIT")),wxART_OTHER));
    file_menu->Append(close_menu_item);

    file_menu->AppendSeparator();

    // Add File->Quit
    quit_menu_item = new wxMenuItem(file_menu, idMenuQuit, wxT("Quit\tAlt-F4"),
        wxT("Quit the application"), wxITEM_NORMAL);
    quit_menu_item->SetBitmap(wxArtProvider::GetBitmap(
        wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_CROSS_MARK")),wxART_OTHER));
    file_menu->Append(quit_menu_item);

    // Add menu bar->File
    if (!menu_bar->Append(file_menu, wxT("&File"))) {
        (void)wxMessageBox(wxT("Unable to configure the file menu!"));
    }

    //Add Edit ->Cut
    cut_menu_item = new wxMenuItem(edit_menu, idMenuCut,
        wxT("Cut\tCtrl-X"), wxT("Cut selected tiles"), wxITEM_NORMAL);
    cut_menu_item->SetBitmap(wxArtProvider::GetBitmap(
        wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_CUT")), wxART_OTHER));
    edit_menu->Append(cut_menu_item);

    //Add Edit ->Copy
    copy_menu_item = new wxMenuItem(edit_menu, idMenuCopy,
        wxT("Copy\tCtrl-C"), wxT("Copy selected tiles"), wxITEM_NORMAL);
    copy_menu_item->SetBitmap(wxArtProvider::GetBitmap(
        wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_COPY")), wxART_OTHER));
    edit_menu->Append(copy_menu_item);

    //Add Edit ->Paste
    paste_menu_item = new wxMenuItem(edit_menu, idMenuPaste,
        wxT("Paste\tCtrl-V"), wxT("Paste copied or cut tiles"), wxITEM_NORMAL);
    paste_menu_item->SetBitmap(wxArtProvider::GetBitmap(
        wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_PASTE")), wxART_OTHER));
    edit_menu->Append(paste_menu_item);

    //Add Edit ->Delete
    delete_menu_item = new wxMenuItem(edit_menu, idMenuDelete,
        wxT("Delete\tDel"), wxT("Delete selected tiles"), wxITEM_NORMAL);
    delete_menu_item->SetBitmap(wxArtProvider::GetBitmap(
        wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_DELETE")), wxART_OTHER));
    edit_menu->Append(delete_menu_item);

    //Add Edit ->SelectAll
    select_all_menu_item = new wxMenuItem(edit_menu, idMenuSelectAll,
        wxT("Select All\tCtrl-A"), wxT("Select the entire map"), wxITEM_NORMAL);
    edit_menu->Append(select_all_menu_item);

    // Add menu bar->Edit
    if (!menu_bar->Append(edit_menu, wxT("Edit"))) {
        (void)wxMessageBox(wxT("Unable to configure the edit menu!"));
    }

    // Add View->Display Grid
    display_grid_menu_item = new wxMenuItem(view_menu, idMenuViewGrid, wxT("Display Grid"),
        wxT("Show/hide the grid"), wxITEM_NORMAL);
    display_grid_menu_item->SetBitmap(wxBitmap(wxImage(
        normalize_path_wx(resource_root + wxT("/data/resources/grid_icon.png")))));
    view_menu->Append(display_grid_menu_item);

    // Add View->Display Collision
    display_collision_menu_item = new wxMenuItem(view_menu, idMenuViewDisp, wxT("Display Collision"),
        wxT("Show/hide the collision data"), wxITEM_NORMAL);
    display_collision_menu_item->SetBitmap(wxBitmap(wxImage(
        normalize_path_wx(resource_root + wxT("/data/resources/collision_icon.png")))));
    view_menu->Append(display_collision_menu_item);

    // Add View->Display Height
    display_height_menu_item = new wxMenuItem(view_menu, idMenuViewHeight, wxT("Display Height"),
        wxT("Show/hide tile heights"), wxITEM_NORMAL);
    display_height_menu_item->SetBitmap(wxBitmap(wxImage(
        normalize_path_wx(resource_root + wxT("/data/resources/height.png")))));
    view_menu->Append(display_height_menu_item);

    view_menu->AppendSeparator();

    // Add View->FullScreen
    full_screen_menu_item = new wxMenuItem(view_menu, idMenuFullScreen, wxT("Full screen\tF11"),
        wxT("Make the window full screen"), wxITEM_NORMAL);
    view_menu->Append(full_screen_menu_item);

    // Add menu bar->View
    if (!menu_bar->Append(view_menu, wxT("View"))) {
        (void)wxMessageBox(wxT("Unable to configure the view menu!"));
    }

    // Add Layer->Terrain
    terrain_menu_item = new wxMenuItem(layer_menu, idMenuTerrain, wxT("Terrain Layer\tT"),
        wxT("Edit Terrain Layer"), wxITEM_NORMAL);
    terrain_menu_item->SetBitmap(wxBitmap(wxImage(
        normalize_path_wx(resource_root + wxT("/data/resources/terrain.png")))));
    layer_menu->Append(terrain_menu_item);

    // Add Layer->Object
    object_menu_item = new wxMenuItem(layer_menu, idMenuObject, wxT("Object Layer\tO"),
        wxT("Edit Object Layer"), wxITEM_NORMAL);
    object_menu_item->SetBitmap(wxBitmap(wxImage(
        normalize_path_wx(resource_root + wxT("/data/resources/object.png")))));
    layer_menu->Append(object_menu_item);

    // Add Layer->Event
    event_menu_item = new wxMenuItem(layer_menu, idMenuEvent, wxT("Event Layer\tE"),
        wxT("Edit Event Layer"), wxITEM_NORMAL);
    event_menu_item->SetBitmap(wxBitmap(wxImage(
        normalize_path_wx(resource_root + wxT("/data/resources/event.png")))));
    layer_menu->Append(event_menu_item);

    // Add menu bar->Layer
    if (!menu_bar->Append(layer_menu, wxT("Layer"))) {
        (void)wxMessageBox(wxT("Unable to configure the layer menu!"));
    }

    // Add Options->Map Properties
    map_properties_menu_item = new wxMenuItem(options_menu, idMenuProp,
        wxT("Map Properties\tCtrl-M"), wxT("Configure map properties"), wxITEM_NORMAL);
    wxImage properties(normalize_path_wx(resource_root + wxT("/data/resources/properties.png")));
    map_properties_menu_item->SetBitmap(wxBitmap(properties.Rescale(16, 16)));
    options_menu->Append(map_properties_menu_item);

    //Add Options->Game Modes
    map_game_modes_menu_item = new wxMenuItem(options_menu, idMenuGameModes,
        wxT("Game Modes\tCtrl-G"), wxT("Set the maps game modes"), wxITEM_NORMAL);
    wxImage game_modes(normalize_path_wx(resource_root + wxT("/data/resources/game_mode.png")));
    map_game_modes_menu_item->SetBitmap(wxBitmap(game_modes.Rescale(16, 16)));
    options_menu->Append(map_game_modes_menu_item);

    //Add Options ->Map List From Server
    map_list_menu_item = new wxMenuItem(options_menu, idMenuMapList,
        wxT("Server\tCtrl-L"), wxT("Interact with the map server"), wxITEM_NORMAL);
    wxImage server(normalize_path_wx(resource_root + wxT("/data/resources/server.png")));
    map_list_menu_item->SetBitmap(wxBitmap(server.Rescale(16, 16)));
    options_menu->Append(map_list_menu_item);

    // Add menu bar->Options
    if (!menu_bar->Append(options_menu, wxT("Options"))) {
        (void)wxMessageBox(wxT("Unable to configure the options menu!"));
    }

    //Add Tool ->Pointer
    pointer_menu_item = new wxMenuItem(tool_menu, idMenuPointer,
        wxT("Pointer\t1"), wxT("Pointer Tool"), wxITEM_NORMAL);
    pointer_menu_item->SetBitmap(wxBitmap(wxImage(
        normalize_path_wx(resource_root + wxT("/data/resources/pointer.png")))));
    tool_menu->Append(pointer_menu_item);

    //Add Tool ->Fill
    fill_menu_item = new wxMenuItem(tool_menu, idMenuFill,
        wxT("Fill\t2"), wxT("Fill Tool"), wxITEM_NORMAL);
    fill_menu_item->SetBitmap(wxBitmap(wxImage(
        normalize_path_wx(resource_root + wxT("/data/resources/fill.png")))));
    tool_menu->Append(fill_menu_item);

    //Add Tool ->Select
    select_menu_item = new wxMenuItem(tool_menu, idMenuSelect,
        wxT("Select\t3"), wxT("Select Tool"), wxITEM_NORMAL);
    select_menu_item->SetBitmap(wxBitmap(wxImage(
        normalize_path_wx(resource_root + wxT("/data/resources/selectgroup.png")))));
    tool_menu->Append(select_menu_item);

    //Add Tool ->Rectangle
    rectangle_menu_item = new wxMenuItem(tool_menu, idMenuRectangle,
        wxT("Rectangle\t4"), wxT("Rectangle Tool"), wxITEM_NORMAL);
    rectangle_menu_item->SetBitmap(wxBitmap(wxImage(
        normalize_path_wx(resource_root + wxT("/data/resources/fillgroup.png")))));
    tool_menu->Append(rectangle_menu_item);

    // Add menu bar->Tool
    if (!menu_bar->Append(tool_menu, wxT("Tools"))) {
        (void)wxMessageBox(wxT("Unable to configure the tool menu!"));
    }

    // Add Help->About
    about_menu_item = new wxMenuItem(help_menu, idMenuAbout, wxT("About"),
        wxT("Show info about this application"), wxITEM_NORMAL);
    about_menu_item->SetBitmap(wxArtProvider::GetBitmap(
		wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_QUESTION")),wxART_OTHER, wxSize(16,16)));
    help_menu->Append(about_menu_item);

    help_menu->AppendSeparator();

    // Add Help->Help
    help_menu_item = new wxMenuItem(help_menu, idMenuHelp, wxT("Help\tF1"),
        wxT("Help with the application"), wxITEM_NORMAL);
    help_menu_item->SetBitmap(wxArtProvider::GetBitmap(
		wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_HELP")),wxART_OTHER, wxSize(16,16)));
    help_menu->Append(help_menu_item);

    // Add menu bar->Help
    if (!menu_bar->Append(help_menu, wxT("Help"))) {
        (void)wxMessageBox(wxT("Unable to configure the help menu!"));
    }

    // Set up event handlers.

    // EVENT: File->New selected.
    //lint -save -e1924
    Connect(idMenuNew, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::OnMenuNew);

    // EVENT: File->Open selected.
    Connect(idMenuOpen, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::OnOpenMap);

    // EVENT: File->Save selected.
    Connect(idMenuSave, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::OnSaveMap);

    // EVENT: File->Save As selected.
    Connect(idMenuSaveAs, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::OnSaveAs);

    // EVENT: File->Close selected.
    Connect(idMenuClose, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::OnCloseMap);

    // EVENT: File->Quit selected.
    Connect(idMenuQuit, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::OnQuit);

    // EVENT: Edit->Cut selected.
    Connect(idMenuCut, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_cut);

    // EVENT: Edit->Copy selected.
    Connect(idMenuCopy, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_copy);

    // EVENT: Edit->Paste selected.
    Connect(idMenuPaste, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_paste);

    // EVENT: Edit->Delete selected.
    Connect(idMenuDelete, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_delete);

    // EVENT: Edit->Select All selected.
    Connect(idMenuSelectAll, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_select_all);

    // EVENT: View collision toggled
    Connect(idMenuViewDisp, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_view_collision_toggle);

    // EVENT: View grid toggled
    Connect(idMenuViewGrid, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_view_grid_toggle);

    // EVENT: View height toggled
    Connect(idMenuViewHeight, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_view_height_toggle);

    // EVENT: View->FullScreen
    Connect(idMenuFullScreen, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_full_screen);
    
    // EVENT: Layer->Terrain
    Connect(idMenuTerrain, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_terrain);

    // EVENT: Layer->Object
    Connect(idMenuObject, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_object);

    // EVENT: Layer->Event
    Connect(idMenuEvent, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_event);

    // EVENT: Options->Map Properties selected.
    Connect(idMenuProp, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::OnOpenMapProperties);

    // EVENT: Options->Map Game Modes selected.
    Connect(idMenuGameModes, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_game_mode);

    // EVENT: Options->Server selected.
    Connect(idMenuMapList, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_server_map_list);

    // EVENT: Tools->Pointer
    Connect(idMenuPointer, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_pointer);

    // EVENT: Tools->Fill
    Connect(idMenuFill, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_fill);

    // EVENT: Tools->Select
    Connect(idMenuSelect, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_select_group);

    // EVENT: Tools->Rectangle
    Connect(idMenuRectangle, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_fill_group);

    // EVENT: Help->About selected.
    Connect(idMenuAbout, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::OnAbout);

    // EVENT: Help->Help selected.
    Connect(idMenuHelp, wxEVT_COMMAND_MENU_SELECTED,
        (wxObjectEventFunction)&MapEditorFrame::on_help);

    // EVENT: Map editor closing.
    Connect(wxID_ANY, wxEVT_CLOSE_WINDOW, (wxObjectEventFunction)&MapEditorFrame::OnClose);
    //lint -restore

    SetMenuBar(menu_bar);
    #endif  //wxUSE_MENUS
    //Setup the Toolbar
    #if wxUSE_TOOLBAR
    tool_bar = CreateToolBar(wxNO_BORDER | wxTB_HORIZONTAL, wxID_ANY, wxT("tools"));
    tool_bar->AddSeparator();
    //FileTool->New
    tool_bar->AddTool(idToolNew,
        wxString("New", wxConvUTF8),
        wxArtProvider::GetBitmap(wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_NEW")), wxART_MENU),
        wxNullBitmap,
        wxITEM_NORMAL, 
        wxString("New", wxConvUTF8), 
        wxString("Create new map", wxConvUTF8),
        0);
    //FileTool->Open
    tool_bar->AddTool(idToolOpen,
        wxString("Open", wxConvUTF8),
        wxArtProvider::GetBitmap(wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_FILE_OPEN")), wxART_MENU),
        wxNullBitmap,
        wxITEM_NORMAL, 
        wxString("Open", wxConvUTF8), 
        wxString("Open an exsisting map", wxConvUTF8),
        0);
    //FileTool->Save As
    tool_bar->AddTool(idToolSaveAs,
        wxString("Save As", wxConvUTF8),
        wxArtProvider::GetBitmap(wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_FILE_SAVE_AS")), wxART_MENU),
        wxNullBitmap,
        wxITEM_NORMAL, 
        wxString("Save As", wxConvUTF8), 
        wxString("Save the current map under a new name.", wxConvUTF8),
        0);
    //FileTool->Save
    tool_bar->AddTool(idToolSave,
        wxString("Save", wxConvUTF8),
        wxArtProvider::GetBitmap(wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_FILE_SAVE")), wxART_MENU),
        wxNullBitmap,
        wxITEM_NORMAL, 
        wxString("Save", wxConvUTF8), 
        wxString("Save the current map", wxConvUTF8),
        0);
    tool_bar->AddSeparator();
    //MapTool->Server
    tool_bar->AddTool(idToolServer,
        wxString("Server", wxConvUTF8),
        wxBitmap(server.Rescale(16,16)),
        wxNullBitmap,
        wxITEM_NORMAL, 
        wxString("Server", wxConvUTF8), 
        wxString("Launch Server Dialog", wxConvUTF8),
        0);
    //MapTool->Properties
    tool_bar->AddTool(idToolProp,
        wxString("Properties", wxConvUTF8),
        wxBitmap(properties.Rescale(16,16)),
        wxNullBitmap,
        wxITEM_NORMAL, 
        wxString("Properties", wxConvUTF8), 
        wxString("Properties of the current map", wxConvUTF8),
        0);
    //MapTool->GameModes
    tool_bar->AddTool(idToolGame,
        wxString("Game Modes", wxConvUTF8),
        wxBitmap(game_modes.Rescale(16,16)),
        wxNullBitmap,
        wxITEM_NORMAL, 
        wxString("Game Modes", wxConvUTF8), 
        wxString("Set the game modes of the current map", wxConvUTF8),
        0);
    tool_bar->AddSeparator();
    //LayerTool->Terrain
    tool_bar->AddTool(idToolTer,
        wxString("Terrain", wxConvUTF8),
        wxBitmap(wxImage(normalize_path_wx(resource_root + wxT("/data/resources/terrain.png")))),
        wxNullBitmap,
        wxITEM_RADIO, 
        wxString("Terrain", wxConvUTF8), 
        wxString("Edit the terrain layer", wxConvUTF8),
        0);
    //LayerTool->Object
    tool_bar->AddTool(idToolObj,
        wxString("Object", wxConvUTF8),
        wxBitmap(wxImage(normalize_path_wx(resource_root + wxT("/data/resources/object.png")))),
        wxNullBitmap,
        wxITEM_RADIO, 
        wxString("Object", wxConvUTF8), 
        wxString("Edit the object layer", wxConvUTF8),
        0);
    //LayerTool->Event
    tool_bar->AddTool(idToolEvent,
        wxString("Event", wxConvUTF8),
        wxBitmap(wxImage(normalize_path_wx(resource_root + wxT("/data/resources/event.png")))),
        wxNullBitmap,
        wxITEM_RADIO, 
        wxString("Event", wxConvUTF8), 
        wxString("Edit the event layer", wxConvUTF8),
        0);
    tool_bar->AddSeparator();
    //EditTool->Cut
    tool_bar->AddTool(idToolCut,
        wxString("Cut", wxConvUTF8),
        wxArtProvider::GetBitmap(wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_CUT")), wxART_MENU),
        wxNullBitmap,
        wxITEM_NORMAL, 
        wxString("Cut", wxConvUTF8), 
        wxString("Cut selected tiles", wxConvUTF8),
        0);
    //EditTool->Copy
    tool_bar->AddTool(idToolCopy,
        wxString("Copy", wxConvUTF8),
        wxArtProvider::GetBitmap(wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_COPY")), wxART_MENU),
        wxNullBitmap,
        wxITEM_NORMAL, 
        wxString("Copy", wxConvUTF8), 
        wxString("Copy selected tiles", wxConvUTF8),
        0);
    //EditTool->Paste
    tool_bar->AddTool(idToolPaste,
        wxString("Paste", wxConvUTF8),
        wxArtProvider::GetBitmap(wxART_MAKE_ART_ID_FROM_STR(wxT("wxART_PASTE")), wxART_MENU),
        wxNullBitmap,
        wxITEM_NORMAL, 
        wxString("Paste", wxConvUTF8), 
        wxString("Paste copied or cut tiles", wxConvUTF8),
        0);
    tool_bar->AddSeparator();
	//EditTool->pointer
    tool_bar->AddTool(idToolPointer,
        wxString("Pointer", wxConvUTF8),
        wxBitmap(wxImage(normalize_path_wx(resource_root + wxT("/data/resources/pointer.png")))),
        wxNullBitmap,
        wxITEM_RADIO, 
        wxString("Pointer", wxConvUTF8), 
        wxString("General pointer tool", wxConvUTF8),
        0);
    //EditTool->fill
    tool_bar->AddTool(idToolfill,
        wxString("Fill", wxConvUTF8),
        wxBitmap(wxImage(normalize_path_wx(resource_root + wxT("/data/resources/fill.png")))),
        wxNullBitmap,
        wxITEM_RADIO, 
        wxString("Fill", wxConvUTF8), 
        wxString("Fills a title", wxConvUTF8),
        0);
    //EditTool->selectgroup
    tool_bar->AddTool(idToolselgrp,
        wxString("Select Group", wxConvUTF8),
        wxBitmap(wxImage(normalize_path_wx(resource_root + wxT("/data/resources/selectgroup.png")))),
        wxNullBitmap,
        wxITEM_RADIO, 
        wxString("Select Group", wxConvUTF8), 
        wxString("Select a group of tiles", wxConvUTF8),
        0);
    //EditTool->fillgroup
    tool_bar->AddTool(idToolfillgrp,
        wxString("Fill Group", wxConvUTF8),
        wxBitmap(wxImage(normalize_path_wx(resource_root + wxT("/data/resources/fillgroup.png")))),
        wxNullBitmap,
        wxITEM_RADIO, 
        wxString("Fill Group", wxConvUTF8), 
        wxString("Fill a group of tiles", wxConvUTF8),
        0);
    tool_bar->AddSeparator();
    //ViewTool->Grid
    wxImage grid(normalize_path_wx(resource_root + wxT("/data/resources/grid_icon.png")));
    tool_bar->AddTool(idToolgrid,
        wxString("Toggle grid", wxConvUTF8),
        wxBitmap(grid.Rescale(16,16)),
        wxNullBitmap,
        wxITEM_CHECK, 
        wxString("Toggle grid", wxConvUTF8), 
        wxString("Toggle grid on or off", wxConvUTF8),
        0);
    tool_bar->ToggleTool(idToolgrid, true);
    //ViewTool->Collision
    wxImage collision(normalize_path_wx(resource_root + wxT("/data/resources/collision_icon.png")));
    tool_bar->AddTool(idToolcol,
        wxString("Toggle collision", wxConvUTF8),
        wxBitmap(collision.Rescale(16,16)),
        wxNullBitmap,
        wxITEM_CHECK, 
        wxString("Toggle collision", wxConvUTF8), 
        wxString("Toggle collision on or off", wxConvUTF8),
        0);
    tool_bar->ToggleTool(idToolcol, true);
    //ViewTool->Height
    wxImage height(normalize_path_wx(resource_root + wxT("/data/resources/height.png")));
    tool_bar->AddTool(idToolheight,
        wxString("Toggle height", wxConvUTF8),
        wxBitmap(height.Rescale(16,16)),
        wxNullBitmap,
        wxITEM_CHECK, 
        wxString("Toggle Height", wxConvUTF8), 
        wxString("Toggle height on or off", wxConvUTF8),
        0);
    tool_bar->AddSeparator();
    wxStaticText * const tile_height = new wxStaticText(tool_bar,
        idHeightTxt,
        wxT("Tile Height: 0"),
        wxDefaultPosition,
        wxDefaultSize); 
    tool_bar->AddControl(tile_height);
    if(!tool_bar->Realize()) {
        (void)wxMessageBox(wxT("Unable to populate toolbar."));
    }


    // Set up event handlers.
    //lint -save -e1924
    // EVENT: FileTool->New selected.
    Connect(idToolNew, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::OnMenuNew);
    // EVENT: FileTool->Open selected.
    Connect(idToolOpen, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::OnOpenMap);
    // EVENT: FileTool->SaveAs selected.
    Connect(idToolSaveAs, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::OnSaveAs);
    // EVENT: FileTool->Save selected.
    Connect(idToolSave, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::OnSaveMap);
    // EVENT: MapTool->Server selected.
    Connect(idToolServer, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_server_map_list);
    // EVENT: MapTool->Map Properties selected.
    Connect(idToolProp, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::OnOpenMapProperties);
    // EVENT: MapTool->Game Modes selected.
    Connect(idToolGame, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_game_mode);
    // EVENT: LayerTool->Terrain selected.
    Connect(idToolTer, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_terrain);
    // EVENT: LayerTool->Object selected.
    Connect(idToolObj, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_object);
    // EVENT: LayerTool->Event selected.
    Connect(idToolEvent, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_event);
    // EVENT: LayerTool->Cut selected.
    Connect(idToolCut, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_cut);
    // EVENT: LayerTool->Copy selected.
    Connect(idToolCopy, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_copy);
    // EVENT: LayerTool->Paste selected.
    Connect(idToolPaste, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_paste);
    // EVENT: EditTool->Pointer
    Connect (idToolPointer, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_pointer);
    // EVENT: EditTool->Fill
    Connect (idToolfill, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_fill);
    // EVENT: EditTool->Select Group
    Connect (idToolselgrp, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_select_group);
    // EVENT: EditTool->Fill Group
    Connect(idToolfillgrp, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_fill_group);
    // EVENT: ViewTool->Grid selected.
    Connect(idToolgrid, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_view_grid_toggle);
    // EVENT: ViewTool->Collision selected.
    Connect(idToolcol, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_view_collision_toggle);
    // EVENT: ViewTool->Height selected
    Connect(idToolheight, wxEVT_COMMAND_TOOL_CLICKED,
        (wxObjectEventFunction)&MapEditorFrame::on_view_height_toggle);
    SetToolBar(tool_bar);
    //lint -restore
    #endif //wxUSE_TOOLBAR
    


    if (CreateStatusBar(1, wxSB_FLAT) == NULL) {
        (void)wxMessageBox(wxT("Unable to create the status bar on the main window!"));
    }

    SetMinSize(wxSize(835, 608));
    Center();

    canvas = new wxPanel(this,
                         wxID_ANY,
                         wxPoint(0, 0),
                         GetClientSize(),
                         wxTAB_TRAVERSAL | wxFULL_REPAINT_ON_RESIZE,
                         wxT("ID PANEL1"));

    toolbox_window =
        new ToolboxWindow(canvas, wxID_ANY, wxPoint((canvas->GetClientSize().GetWidth()-200),0),
        wxSize(200,canvas->GetClientSize().GetHeight()), &terrain_dictionary, &object_dictionary, &event_dictionary);
    toolbox_window->set_edit_mode(toolbox_window->TERRAIN);
    scrolled_window =
        new ScrolledWindow(canvas, wxID_ANY, wxDefaultPosition,
        wxSize(canvas->GetClientSize().GetWidth()-200,canvas->GetClientSize().GetHeight()),
        toolbox_window, tile_height);
    editor_startup();
    change_size = true;

}


MapEditorFrame::~MapEditorFrame()
{
    if (current_map != NULL) {
        delete current_map->data;
        delete current_map;
    }
    (void)help.Quit();
    delete tool_bar;
    delete canvas;
    toolbox_window = NULL;
    scrolled_window = NULL;

}

void MapEditorFrame::OnSize(wxSizeEvent &WXUNUSED(event))
{
    if(change_size)
    {
        canvas->SetSize(GetClientSize());
        scrolled_window->SetSize(wxSize(canvas->GetClientSize().GetWidth()-200,canvas->GetClientSize().GetHeight()));
        toolbox_window->SetPosition(wxPoint((canvas->GetClientSize().GetWidth()-200),0));
        toolbox_window->SetSize(wxSize(200,canvas->GetClientSize().GetHeight()));
    }
}

//! Sets the window title
/*!
 * \param m_title The title of the current map that is open
 *
 */
void MapEditorFrame::set_editor_title(const wxString &m_title)
{
    wxString window_title = wxT("VTank Map Editor - ");
    window_title << m_title;
    SetLabel(window_title);
}

//! Editor Startup initializes important non-window specific parts
/*!
 * Sets the map editor to show the grid and collisions. Sets the scrollbars for the
 * ToolboxWindow and ScrolledWindow. Sets the window title and status.
 */
void MapEditorFrame::editor_startup()
{
    scrolled_window->EnableScrolling(true, true);
    scrolled_window->SetScrollbars(TILE_SIZE_X, TILE_SIZE_Y, 0, 0);
    scrolled_window->Update();

    toolbox_window->EnableScrolling(false, true);
    toolbox_window->SetScrollbars(0, TILE_SIZE_Y, 0, static_cast<int>(terrain_dictionary.size()/COLUMNS));
    toolbox_window->Update();

    set_editor_title(wxT(""));

    // Let the scrolled window have access to the tile dictionary.
    // Should only have to do this once.
    scrolled_window->set_dictionaries(&terrain_dictionary, &object_dictionary, &event_dictionary);
    scrolled_window->Refresh();
    toolbox_window->Refresh();
}


//TODO: make dictionary a std::string
//! Load tiles into the dictionary from DICTIONARY
void MapEditorFrame::load_dictionary(const std::string dictionary)
{
    std::string input;
    std::vector<LoadedTile> result;

    std::ifstream in_file;
    in_file.open(dictionary.c_str());
    if (!in_file.is_open()) {
        wxString message(wxT("Check that the data file exists: "));
        message += wxString(dictionary.c_str(), wxConvUTF8);
        (void)wxMessageBox(message, wxT("Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        throw std::runtime_error("Initialization Failure");
    }

    while (getline(in_file, input)) {
        // Remote extraneous CR at the end (when Linux version reads a Windows text file).
        if (input.size() > 0 && input[input.size() - 1] == '\r') input.erase(input.size() - 1);

        // TODO: Remove comments that appear at the tail of a line. Strip trailing whitespace.
        // Skip blank lines. The approach used here is fragile because it requires the '#' to
        // appear at the start of a line. It also feeds blank lines to the logic below.
        if (input.substr(0, 1) == "#") {
            // Skip comments.
            continue;
        }

        const std::string::size_type space_position = input.find_first_of(":");
        if (space_position == std::string::npos) {
            // The line is invalid.
            continue;
        }

        // Expected input: [id] [filename]
        // TODO: Note that only a single space is allowed between the id and the file name.
        std::string id          = input.substr(0, space_position);
        std::string filename    = input.substr(space_position + 1);
        wxString converted_file_name(filename.c_str(), wxConvUTF8);

        // Convert the ID to an integer.
        const int converted_id = atoi(id.c_str());

        // Push the tile onto the vector.
        result.push_back(LoadedTile(converted_id, converted_file_name));
    }

    in_file.close();

    const std::string *const root_path = Support::lookup_parameter("RESOURCE_ROOT");
    VTANK_ASSERT(root_path != NULL);
    wxString prefix(root_path->c_str(), wxConvUTF8);
    if (dictionary == "tile_dictionary.txt") {
        prefix += wxT("/data/terrain/");
    }
    else if (dictionary == "object_dictionary.txt") {
        prefix += wxT("/data/object/");
    }
    else {
        prefix += wxT("/data/event/");
    }
    prefix = Support::normalize_path_wx(prefix);

    // Now load each image into memory.
    for (std::vector<LoadedTile>::size_type i = 0; i < result.size(); i++) {
        wxString file_name = (prefix + result[i].file_name);

        wxBitmap bitmap;
        if (bitmap.LoadFile(file_name, wxBITMAP_TYPE_PNG)) {
            if(dictionary == "tile_dictionary.txt") {
                terrain_dictionary[result[i].tile_id] = bitmap;
            }
            else if(dictionary == "object_dictionary.txt") {
                object_dictionary[result[i].tile_id] = bitmap;
            }
            else {
                event_dictionary[result[i].tile_id] = bitmap;
            }
        }
        
    }
}

//! Refreshs application
/*!
 * Event handler for updating / refreshing the ScrolledWindow and ToolboxWindow when changes
 * have been made and need to be displayed.
 */
void MapEditorFrame::on_paint(wxPaintEvent &WXUNUSED(event))
{
    try {
        scrolled_window->Update();
        toolbox_window->Update();
    }
    CATCH_LOGIC_ERRORS
}


//! Closes the application
/*!
 * Event handler for quitting the application when the quit menu item is clicked.
 */
void MapEditorFrame::OnQuit(wxCommandEvent &WXUNUSED(event))
{
    try {
        if (current_map != NULL && current_map->is_modified) {
            if (wxMessageBox(wxT("Would you like to save your changes?"),
                wxT("Unsaved Changes"), wxYES_NO | wxICON_WARNING | wxSTAY_ON_TOP) == wxYES) {
                   save_map(false);
            }
        }
        const bool destroy = Destroy();
        VTANK_ASSERT(destroy);
    }
    CATCH_LOGIC_ERRORS
}


//! Displays the help dialog
/*!
 * Event handler for displaying the help dialog when the about menu item is clicked.
 */
void MapEditorFrame::OnAbout(wxCommandEvent &WXUNUSED(event))
{
    try {
        AboutDialog about_dialog;
        (void)about_dialog.ShowModal();
    }
    CATCH_LOGIC_ERRORS
}


//! Closes the application
/*!
 * Event handler for quitting the application when the quit x button is clicked.
 */
void MapEditorFrame::OnClose(wxCloseEvent &WXUNUSED(event))
{
    try {
        if (current_map != NULL && current_map->is_modified) {
            if (wxMessageBox(wxT("Would you like to save your changes?"),
                wxT("Unsaved Changes"),wxYES_NO | wxICON_WARNING | wxSTAY_ON_TOP) == wxYES) {
                if(current_map->filename == "./Untitled.vtmap")
                    save_map(true);
            }
        }
        const bool destroy = Destroy();
        VTANK_ASSERT(destroy);
    }
    CATCH_LOGIC_ERRORS
}


//! Creates a new map.
/*!
 * Event handler for bringing up the NewMapDialog and creating the new map. when the New menu
 * item is clicked.
 */
void MapEditorFrame::OnMenuNew(wxCommandEvent &WXUNUSED(event))
{
    try {
        if (current_map != NULL && current_map->is_modified) {
            if (wxMessageBox(wxT("Would you like to save your changes?"),
                             wxT("Unsaved Changes"),
                             wxYES_NO | wxICON_WARNING | wxSTAY_ON_TOP) == wxID_YES) {
                if (!current_map->data->save(current_map->filename)) {
                    // If saving fails, abort the new operation. Is this really the right thing to do?
                    (void)wxMessageBox(wxString(current_map->data->get_last_error().c_str(), wxConvUTF8));
                    return;
                }
            }
        }

        int width = 0, height = 0, default_tile = 0;
        wxString temp_title;

        NewMapDialog new_map_dialog(&width, &height, &temp_title, &default_tile);
        new_map_dialog.init_default_tile_selector(terrain_dictionary, object_dictionary, event_dictionary);

        if (new_map_dialog.ShowModal() == wxOK) {
            if (current_map != NULL) { // Currently have an open map.
                delete current_map->data;
                delete current_map;
                current_map = NULL;
            }

            current_map = new OpenMap();
            current_map->data = new Map();

            const std::string title(temp_title.ToAscii());
            
            current_map->data->set_default_tile(default_tile);
            // Create a new Map from the Map class set to the inputted values.
            if (!current_map->data->create(width, height, title)) {

                // If the create fails, abort. This leaves the frame with a blank map. How many
                // of the steps below this need to execute anyway? That is, are we returning
                // prematurely?
                //
                (void)wxMessageBox(wxString(current_map->data->get_last_error().c_str(), wxConvUTF8));
                return;
            }

            // Mark that a map was changed and can be saved.
            current_map->is_modified = true;

            // Set the save path of this file.
            current_map->filename = ("./" + current_map->data->get_title() + ".vtmap");

            wxString the_title(current_map->data->get_title().c_str(), wxConvUTF8);
            set_editor_title(the_title);
            set_editor_status(static_cast<wxWindow*>(this), current_map);

            // Set up the scroll bar so that it will cover all of the new tiles.
            scrolled_window->SetScrollbars(TILE_SIZE_X, TILE_SIZE_Y, width, height);

            scrolled_window->set_map(current_map);

            // Commented out for now.
            // maps_vector.push_back(new_map);
            scrolled_window->Refresh(true);
        }
    }
    CATCH_LOGIC_ERRORS
}


//! Opens a map.
/*!
 * Event handler for bringing up a file dialog to open a map and opening the map when the Open
 * menu item is clicked.
 */
void MapEditorFrame::OnOpenMap(wxCommandEvent &WXUNUSED(event))
{
    try {
        if (current_map != NULL && current_map->is_modified) {
            if(wxMessageBox(wxT("Would you like to save your changes?"),
                            wxT("Unsaved Changes"),
                            wxYES_NO | wxICON_WARNING | wxSTAY_ON_TOP) == wxID_YES) {
                if (!current_map->data->save(current_map->filename)) {
                    // If the save fails, abort the open. Is this really the right thing to do?
                    (void)wxMessageBox(wxString(current_map->data->get_last_error().c_str(), wxConvUTF8));
                    return;
                }
            }
        }

        // Create an open file dialog object.
        wxFileDialog f_o_dialog(this,
            wxT("Choose a file to open"),
            wxT(""),
            wxT(""),
            wxT("*.vtmap"),
            wxFD_OPEN | wxFD_FILE_MUST_EXIST,
            wxDefaultPosition,
            wxDefaultSize,
            wxT("filedlg"));

        const int exited = f_o_dialog.ShowModal(); // Show the file dialog so it stays in focus.

        if (exited == wxID_OK) {  // Want to open a map.
            // Delete and re-create the map.
            if (current_map != NULL) {
                delete current_map->data;
                delete current_map;
            }

            current_map = new OpenMap();
            current_map->data = new Map();

            wxString the_file = f_o_dialog.GetPath();
            if (!current_map->data->load(std::string(the_file.ToAscii()))) {
                // If the load fails, abort the open here. Is this premature?
                (void)wxMessageBox(wxString(current_map->data->get_last_error().c_str(), wxConvUTF8));
                return;
            }
            current_map->filename = the_file.ToAscii();
            current_map->is_modified = false;

            // Set up the scroll bar so that it will cover all of the new tiles.
            scrolled_window->SetScrollbars(TILE_SIZE_X, TILE_SIZE_Y,
                current_map->data->get_width(),
                current_map->data->get_height());

            scrolled_window->set_map(current_map);
            wxString the_title(current_map->data->get_title().c_str(), wxConvUTF8);
            set_editor_title(the_title);
            set_editor_status(static_cast<wxWindow*>(this), current_map);
        }
        Show(true);
    }
    CATCH_LOGIC_ERRORS
}

//! Closes the current map.
/*!
 * Event handler for closing the current map when the Close menu item is clicked.
 */
void MapEditorFrame::OnCloseMap(wxCommandEvent &WXUNUSED(event))
{
    try {
        if (current_map == NULL) {
            (void)wxMessageBox(wxT("There is currently no map open"),
                               wxT("No Map Open"),
                               wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
        else {
            if (current_map->is_modified) {
                if (wxMessageBox(wxT("Would you like to save your changes?"),
                                 wxT("Unsaved Changes"),
                                 wxYES | wxNO | wxICON_WARNING | wxSTAY_ON_TOP) == wxID_YES) {
                    if (!current_map->data->save(current_map->filename)) {
                        // If the save fails, abort the close operation. Is this really the right thing to do?
                        (void)wxMessageBox(wxString(current_map->data->get_last_error().c_str(), wxConvUTF8));
                        return;
                    }
                }
            }

            delete current_map->data;
            delete current_map;
            current_map = NULL;

            scrolled_window->set_map(NULL);
            scrolled_window->Refresh(true);
            set_editor_title(wxT("")); // Default state.
        }
    }
    CATCH_LOGIC_ERRORS
}


//! View/Changes the current map's properties.
/*!
 * Event handler for bringing up the map properties dialog when the Map Properties menu item is
 * clicked.
 */
void MapEditorFrame::OnOpenMapProperties(wxCommandEvent &WXUNUSED(event))
{
    try {
        if (current_map == NULL) {
            (void)wxMessageBox(wxT("There is currently no map open."),
                               wxT("No Map Open"),
                               wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
            return;
        }

        int      width (current_map->data->get_width());
        wxString title (current_map->data->get_title().c_str(), wxConvUTF8);
        int      height(current_map->data->get_height());

        wxString properties_title(title.c_str(), wxConvUTF8);
        wxString window_title(title);
        properties_title += wxT(" Map Properties");

        // Prompt the user to modify their map's title, width, and height.
        MapPropertiesDialog property_dialog(properties_title, &width, &height, &window_title);
        const int user_action = property_dialog.ShowModal();

        if (user_action == wxOK) {
            const int preferred_width  = width;
            const int preferred_height = height;
            const std::string preferred_title(wxConvCurrent->cWX2MB(window_title));

            current_map->is_modified = true;
            current_map->data->set_title(preferred_title);

            if (current_map->data->resize(preferred_width, preferred_height)) {
                scrolled_window->SetScrollbars(
                    TILE_SIZE_X, TILE_SIZE_Y, preferred_width, preferred_height);
               set_editor_status(static_cast<wxWindow*>(this), current_map);
            }
            else {
                wxString error_message(wxT("Unable to resize map: "));
                error_message += wxString(
                    current_map->data->get_last_error().c_str(), wxConvUTF8);

                (void)wxMessageBox(error_message, wxT("Error!"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
                wxString the_title(current_map->data->get_title().c_str(), wxConvUTF8);
                set_editor_title(the_title);
                set_editor_status(static_cast<wxWindow*>(this), current_map);
            }
        }
    }
    CATCH_LOGIC_ERRORS
}

void MapEditorFrame::save_map(bool save_as)
{
    try {
        wxString the_file;
        if (current_map != NULL) {
            current_map->data->validate_supported_game_modes();
            if(save_as == false && current_map->filename == "./Untitled.vtmap")
                        save_as = true;
            if(save_as) {
                wxFileDialog f_s_dialog(this,
                    wxT("Save File"),
                    wxT(""),
                    wxT(""),
                    wxT("*.vtmap"),
                    wxFD_SAVE|wxFD_OVERWRITE_PROMPT,
                    wxDefaultPosition,
                    wxDefaultSize,
                    wxT("filedlgs"));

                if (f_s_dialog.ShowModal() == wxID_OK) {       // Want to save a map.
                    the_file = f_s_dialog.GetPath();
                }
                else {
                    return;
                }
            }
            else
            {
                the_file = wxString(current_map->filename.c_str(), wxConvUTF8);
            }
            //save the file
            if (!current_map->data->save(std::string(the_file.ToAscii()))) {
                // If the save fails, abort the operation here.
                (void)wxMessageBox(wxString(current_map->data->get_last_error().c_str(), wxConvUTF8));
                return;
            }
            current_map->filename = the_file.mb_str();
            current_map->is_modified = false;
            set_editor_status(static_cast<wxWindow*>(this), current_map);
        }
        else {
            (void)wxMessageBox(wxT("You don't have a map open"),
                               wxT("No Map Open"),
                               wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        }
    }
    CATCH_LOGIC_ERRORS
}
//! Saves the current map.
/*!
 * Event handler for saving a map using the current map's filename when the Save menu item is
 * clicked.
 */
void MapEditorFrame::OnSaveMap(wxCommandEvent &WXUNUSED(event))
{
    save_map(false);
}

//! Saves the current map to a different file
/*!
 * Event handler for bringing up a save file dialog when the Save As menu item is clicked.
 */
void MapEditorFrame::OnSaveAs(wxCommandEvent &WXUNUSED(event))
{
    save_map(true);
}

void MapEditorFrame::on_server_map_list(wxCommandEvent &WXUNUSED(event)) const
{
    ServerDialog maplist_dialog;
    (void)maplist_dialog.ShowModal();
}

void MapEditorFrame::on_game_mode(wxCommandEvent &WXUNUSED(event))
{
    if(current_map != NULL) {
        if(current_map->data != NULL) {
            GameModeDialog game_mode_dialog(current_map->data);
            const int user_action = game_mode_dialog.ShowModal();
            if(user_action == GameModeDialog::APPLIED) {
                current_map->is_modified = true;
                set_editor_status(static_cast<wxWindow*>(this), current_map);
            }
        }
    }
    else {
        (void)wxMessageBox(wxT("There is currently no map open."),
                               wxT("No Map Open"),
                               wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
    }
}

void MapEditorFrame::on_terrain(wxCommandEvent const &event)
{
    if(event.GetId() == idMenuTerrain)
    {
            tool_bar->ToggleTool(idToolTer, true);
    }
    toolbox_window->set_edit_mode(toolbox_window->TERRAIN);
    toolbox_window->SetScrollbars(0, TILE_SIZE_Y, 0, static_cast<int>(terrain_dictionary.size()/COLUMNS));
    toolbox_window->Refresh();
}

void MapEditorFrame::on_object(wxCommandEvent const &event)
{
    if(event.GetId() == idMenuObject)
    {
            tool_bar->ToggleTool(idToolObj, true);
    }
    toolbox_window->set_edit_mode(toolbox_window->OBJECT);
    toolbox_window->SetScrollbars(0, TILE_SIZE_Y, 0, static_cast<int>(object_dictionary.size()/COLUMNS));
    toolbox_window->Refresh();
}
void MapEditorFrame::on_event(wxCommandEvent const &event)
{
    if(event.GetId() == idMenuEvent)
    {
            tool_bar->ToggleTool(idToolEvent, true);
    }
    toolbox_window->set_edit_mode(toolbox_window->EVENT);
    toolbox_window->SetScrollbars(0, TILE_SIZE_Y, 0, static_cast<int>(event_dictionary.size()/COLUMNS));
    toolbox_window->Refresh();
}

void MapEditorFrame::on_cut(wxCommandEvent &WXUNUSED(event))
{
    scrolled_window->cut();
}

void MapEditorFrame::on_copy(wxCommandEvent &WXUNUSED(event))
{
    scrolled_window->copy();
}

void MapEditorFrame::on_paste(wxCommandEvent &WXUNUSED(event))
{
    scrolled_window->paste();
}

void MapEditorFrame::on_delete(wxCommandEvent &WXUNUSED(event))
{
    scrolled_window->delete_selected();
}

void MapEditorFrame::on_select_all(wxCommandEvent &WXUNUSED(event))
{
    scrolled_window->select_all();
}

void MapEditorFrame::on_pointer(wxCommandEvent const &event)
{
    if(event.GetId() == idMenuPointer)
    {
            tool_bar->ToggleTool(idToolPointer, true);
    }
    scrolled_window->set_edit_tool(ScrolledWindow::POINTER);
}

void MapEditorFrame::on_fill(wxCommandEvent const &event)
{
    if(event.GetId() == idMenuFill)
    {
            tool_bar->ToggleTool(idToolfill, true);
    }
    scrolled_window->set_edit_tool(ScrolledWindow::FILL);
}

void MapEditorFrame::on_select_group(wxCommandEvent const &event)
{
    if(event.GetId() == idMenuSelect)
    {
            tool_bar->ToggleTool(idToolselgrp, true);
    }
    scrolled_window->set_edit_tool(ScrolledWindow::SELECTGROUP);
}

void MapEditorFrame::on_fill_group(wxCommandEvent const &event)
{
    if(event.GetId() == idMenuRectangle)
    {
            tool_bar->ToggleTool(idToolfillgrp, true);
    }
    scrolled_window->set_edit_tool(ScrolledWindow::FILLGROUP);
}

void MapEditorFrame::on_full_screen(wxCommandEvent &WXUNUSED(event))
{
    Maximize();
}
void MapEditorFrame::on_help(wxCommandEvent &WXUNUSED(event))
{
    if(!help.DisplayContents()) {
        (void)wxMessageBox(wxT("Unable to open help"),
                               wxT("Help Error"),
                               wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
    }

}
//! Turns the scrolled window grid on and off
/*!
 * Event handler to toggle the scrolled window's grid on and off when the Display Grid menu item
 * is clicked.
 */
void MapEditorFrame::on_view_grid_toggle(wxMenuEvent const &event)
{
    if(event.GetId() == idMenuViewGrid)
    {
        if(tool_bar->GetToolState(idToolgrid))
            tool_bar->ToggleTool(idToolgrid, false);
        else
            tool_bar->ToggleTool(idToolgrid, true);
    }
    try {
        view_show_grid = !view_show_grid;
        // set the window display states
        scrolled_window->set_window_display_states(view_show_collision, view_show_grid, view_show_height);
        scrolled_window->Refresh(false);
    }
    CATCH_LOGIC_ERRORS
}


//! Turns the scrolled window collision data on and off
/*!
 * Event handler to toggle the scrolled window's collision data on and off when the Display
 * Collisions menu item is clicked.
 */
void MapEditorFrame::on_view_collision_toggle(wxMenuEvent const &event)
{
    if(event.GetId() == idMenuViewDisp)
    {
        if(tool_bar->GetToolState(idToolcol))
            tool_bar->ToggleTool(idToolcol, false);
        else
            tool_bar->ToggleTool(idToolcol, true);
    }
    try {
        view_show_collision = !view_show_collision;

        // set the window display states
        scrolled_window->set_window_display_states(view_show_collision, view_show_grid, view_show_height);
        scrolled_window->Refresh(false);
    }
    CATCH_LOGIC_ERRORS
}

void  MapEditorFrame::on_view_height_toggle(wxMenuEvent const &event)
{
    if(event.GetId() == idMenuViewHeight)
    {
        if(tool_bar->GetToolState(idToolheight))
            tool_bar->ToggleTool(idToolheight, false);
        else
            tool_bar->ToggleTool(idToolheight, true);
    }
    try {
        view_show_height = !view_show_height;

        // set the window display states
        scrolled_window->set_window_display_states(view_show_collision, view_show_grid, view_show_height);
        scrolled_window->Refresh(false);
    }
    CATCH_LOGIC_ERRORS
}
