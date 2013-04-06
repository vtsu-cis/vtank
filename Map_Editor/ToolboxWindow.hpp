/*!
    \file   ToolboxWindow.hpp
    \brief  Interface to toolbox scrolling windows.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef TOOLBOXWINDOW_HPP
#define TOOLBOXWINDOW_HPP

const int TILE_SIZE_X = 64;
const int TILE_SIZE_Y = 64;
const int COLUMNS     = 3;


//! This class implements a scrolling sub-window.
/*!
 *  This window provides the user with a scrollable list of all available tiles for use during
 *  the editing of a map. In the future additional tools may also be displayed here. [Perhaps
 *  it would be better to have a separate window type for just tile display since there might
 *  be situations where we would want to show a list of all tiles without the other editing
 *  tools.]
 */
class ToolboxWindow : public wxScrolledWindow, private boost::noncopyable {
    //lint -save -e1516
    DECLARE_EVENT_TABLE()
    //lint -restore

public:
    enum layer{ TERRAIN = 0,
                OBJECT     ,
                EVENT      };

    ToolboxWindow(
        wxPanel       *parent,
        wxWindowID     id,
        const wxPoint &pos,
        const wxSize  &size,
        std::map<int, wxBitmap> *terrain_dictionary,
        std::map<int, wxBitmap> *object_dictionary,
        std::map<int, wxBitmap> *event_dictionary);

   ~ToolboxWindow();

    int  get_selected_tile_id() const { return selected_id; }
    void set_edit_mode(const layer edit_layer);
    layer  get_edit_mode() const {return edit_mode;}
    void OnPaint     (wxPaintEvent &event);
    void OnSize      (wxSizeEvent  &event);
    void OnErase     (wxEraseEvent &event);
    void on_left_down(wxMouseEvent &event);

private:
    void draw_tiles(wxDC& dc);

    std::map<int, wxBitmap> *terrain_dictionary;
    std::map<int, wxBitmap> *object_dictionary;
    std::map<int, wxBitmap> *event_dictionary;
    wxBitmap *buffer;
    wxBitmap *border;
    layer     edit_mode;
    int       selected_id;
    bool      needs_refresh;
};

#endif
