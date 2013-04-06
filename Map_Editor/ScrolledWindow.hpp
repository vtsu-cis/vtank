/*!
    \file   ScrolledWindow.hpp
    \brief  Interface for the sub-window scrolling frame.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef SCROLLEDWINDOW_HPP
#define SCROLLEDWINDOW_HPP

#include "Map.hpp"
#include "ToolboxWindow.hpp"
#include "OpenMap.hpp"

//! Scrolling sub-window that inherits wxScrolledWindow.
/*!
 *  The ScrolledWindow class, despite it's generic name, is designed to support the Map class by
 *  displaying it's tiles according to it's ID number and giving the user a front end for
 *  editing tiles.
 */
class ScrolledWindow : public wxScrolledWindow, private boost::noncopyable {
    //lint -save -e1516
    DECLARE_EVENT_TABLE()
    //lint -restore

public:
    enum tool{ POINTER = 0,
               SELECTGROUP,
               FILL       ,
               FILLGROUP };

    ScrolledWindow(wxPanel        *parent,
                    wxWindowID     id,
                    const wxPoint &pos,
                    const wxSize  &size,
                    ToolboxWindow *tile_selector,
                    wxStaticText  *tile_height);
    ~ScrolledWindow();

    void set_map(OpenMap *);
    void set_dictionaries(std::map<int, wxBitmap>* ter_dict, std::map<int, wxBitmap>* obj_dict, std::map<int, wxBitmap>* evt_dict);
    void set_window_display_states(bool do_draw_collisions, bool do_draw_grid, bool do_draw_height);
    std::vector<wxPoint> set_tile_selection(const wxPoint &start, const wxPoint &finish) const;
    void set_edit_tool(const tool edit_tool);
    tool get_edit_tool() const {return edit_tool;}
    void cut();
    void copy();
    void paste();
    void delete_selected();
    void select_all();
    void fill(const int replace);

    // Event handlers.
    void OnPaint        (wxPaintEvent &event);
    void OnSize         (wxSizeEvent  &event);
    void OnErase        (wxEraseEvent &event);
    void on_left_down   (wxMouseEvent &event);
    void on_left_up     (wxMouseEvent &event);
    void on_right_down  (wxMouseEvent &event);
    void on_mouse_motion(wxMouseEvent &event);
    void on_mouse_scroll(wxMouseEvent &event);

private:
    // Drawing helper methods.
    void draw_grid (wxDC& dc) const;
    void draw_tiles(wxDC& dc) const;
    int  get(const wxPoint &point)   const;
    bool set(const wxPoint &point);
    bool validate_point(const wxPoint &point) const;

    OpenMap                 *open_map;
    Map                     *current_map;
    std::map<int, wxBitmap> *terrain_dictionary;
    std::map<int, wxBitmap> *object_dictionary;
    std::map<int, wxBitmap> *event_dictionary;
    wxBitmap                *buffer;
    wxBitmap                *collision;
    wxBitmap                *selection;
    ToolboxWindow           *tile_selector;
    wxStaticText            *tile_height;
    tool                    edit_tool;
    wxPoint                 *start_tile;
    wxCursor                cursor;
    std::queue<std::pair<wxPoint, std::string> >     *need_paint;

    bool needs_refresh;
    bool display_grid;
    bool display_collision;
    bool display_height;

    // Handle collision motion changes.
    int                  collision_flag;
    int                  global_tile_height;
    wxPoint              last_tile;
    std::vector<wxPoint> current_selection;
    std::vector<Tile>    tile_selection;
    std::vector<wxPoint> tile_points;
};

#endif
