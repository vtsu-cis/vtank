/*!
    \file   ScrolledWindow.cpp
    \brief  Utilize wxWidgets to support sub-scrolling windows.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#include "Library.hpp"
#include "config.hpp"
#include "ScrolledWindow.hpp"
#include "Support.hpp"
#include "vtassert.hpp"
#include "MapEditorFrame.hpp"

//lint -e1924
BEGIN_EVENT_TABLE(ScrolledWindow, wxScrolledWindow)
  EVT_PAINT           (ScrolledWindow::OnPaint        )
  EVT_ERASE_BACKGROUND(ScrolledWindow::OnErase        )
  EVT_SIZE            (ScrolledWindow::OnSize         )
  EVT_LEFT_DOWN       (ScrolledWindow::on_left_down   )
  EVT_LEFT_UP         (ScrolledWindow::on_left_up     )
  EVT_RIGHT_DOWN      (ScrolledWindow::on_right_down  )
  EVT_MOTION          (ScrolledWindow::on_mouse_motion)
  EVT_MOUSEWHEEL      (ScrolledWindow::on_mouse_scroll)
END_EVENT_TABLE()
//lint +e1924


//! Create a new scrollable sub-window.
/*!
 *   The scrolled window provides an interface for tile displaying and editing. The best way to
 *   display a tile in a scrolling window is to fix the scroll rate to 64x64 -- that is, it
 *   scrolls vertically 64 pixels, and scrolls horizontally 64 pixels. This will cause each
 *   scroll to view the next column or row of tiles, rather than viewing tiles pixel-by-pixel.
 *   This enhances the editing experience.
 *
 *   \param parent The parent of this window.
 *   \param id ID of the window, generated through wxWidgets (but tracked by the user).
 *   \param pos Position of the window. Uses a wxPoint (x, y).
 *   \param size Size of the window. Uses a wxSize (w, h).
 *   \param tile_selector Gives this class direct access to which tile is selected by the
 *       user.
 */
ScrolledWindow::ScrolledWindow(
    wxPanel       *parent,
    wxWindowID     id,
    const wxPoint &pos,
    const wxSize  &size,
    ToolboxWindow *box,
    wxStaticText    *t_height)
    : wxScrolledWindow(parent, id, pos, size, wxNO_BORDER, wxT("Tile View")),
      noncopyable        (),
      open_map           (NULL),
      current_map        (NULL),
      terrain_dictionary (NULL),
      object_dictionary  (NULL),
      event_dictionary   (NULL),
      buffer             (NULL),
      collision          (new wxBitmap()),
      selection          (new wxBitmap()),
      tile_selector      (box),
      tile_height        (t_height),
      edit_tool          (POINTER),
      start_tile         (new wxPoint(0, 0)),
      cursor             (wxCursor(wxCURSOR_ARROW)),
      need_paint         (new std::queue<std::pair<wxPoint, std::string> >()),
      needs_refresh      (true),
      display_grid       (true),
      display_collision  (true),
      display_height     (false),
      collision_flag     (false),  // What is the appropriate initial value for this field?
      global_tile_height (0),
      last_tile          (),
      current_selection  (std::vector<wxPoint>()),
      tile_selection     (std::vector<Tile>()),
      tile_points        (std::vector<wxPoint>())


{
    const wxSize area  = GetClientSize();
    buffer = new wxBitmap(area.GetWidth(), area.GetHeight());

    wxString resource_root(Support::lookup_parameter("RESOURCE_ROOT")->c_str(), wxConvUTF8);

    if (!collision->LoadFile(
            Support::normalize_path_wx(resource_root + wxT("/data/terrain/collision.png")),
            wxBITMAP_TYPE_PNG)) {
        std::cerr << "Unable to load collision tile." << std::endl;
    }
    if (!selection->LoadFile(
            Support::normalize_path_wx(resource_root + wxT("/data/terrain/selection.png")),
            wxBITMAP_TYPE_PNG)) {
        std::cerr << "Unable to load selection tile." << std::endl;
    }
    SetCursor(cursor);
    SetFocus();
}
//! Destroys graphical resources used by the ScrolledWindow instance.
ScrolledWindow::~ScrolledWindow()
{
    delete start_tile;
    delete need_paint;
    delete buffer;
    delete collision;
    delete selection;
    terrain_dictionary = NULL;
    object_dictionary = NULL;
    event_dictionary = NULL;
    open_map = NULL;
    current_map = NULL;
    tile_selector = NULL;
    tile_height = NULL;
}


//! Set the map used for tile displaying and tracking.
/*!
 *  Pass an object pointing to the map focused on by the user. This lets the scrolling window
 *  know how to draw and set the tiles. Be sure to unset the map when the map is closed by
 *  passing set_map(NULL).
 *
 *  \param map Initialized Map to use in the ScrolledWindow. Pass NULL to unset.
 */
void ScrolledWindow::set_map(OpenMap *map)
{
    if(map != NULL) {
        open_map = map;
        current_map = open_map->data;
    }
}


//! Set the tile look-up dictionary.
/*!
 *  The tile dictionary is used to match the map's data with the tile data when displaying
 *  tiles.
 *
 *  \param dictionary Dictionary of tile images. The key to this dictionary is the tile ID. The value
 *  is the tile image itself.
 *
 *  \todo It would probably be better if this was made a constructor argument.
 */
void ScrolledWindow::set_dictionaries(std::map<int, wxBitmap>* ter_dict,
                                      std::map<int, wxBitmap>* obj_dict,
                                      std::map<int, wxBitmap>* evt_dict)
{
    terrain_dictionary = ter_dict;
    object_dictionary = obj_dict;
    event_dictionary = evt_dict;
}


//! Sets the display_grid and display_collision variables
/*!
 *  Setting these to true will draw both the window grid and the collision data
 */
void ScrolledWindow::set_window_display_states(
    const bool do_draw_collisions, const bool do_draw_grid, const bool do_draw_height)
{
    display_grid      = do_draw_grid;
    display_collision = do_draw_collisions;
    display_height    = do_draw_height;
    // Now update that the window needs a refresh.
    needs_refresh = true;
}

void ScrolledWindow::set_edit_tool(const tool new_edit_tool)
{
    if (new_edit_tool == POINTER) {
        if (edit_tool == SELECTGROUP) {
            current_selection.clear();
            needs_refresh = true;
            Refresh();
        }
        cursor = wxCursor(wxCURSOR_ARROW);
        SetCursor(cursor);
    }
    else if (new_edit_tool == FILLGROUP) {
        if (edit_tool == SELECTGROUP) {
            current_selection.clear();
            needs_refresh = true;
            Refresh();
        }
        cursor = wxCursor(wxCURSOR_CROSS);
        SetCursor(cursor);
    }
    else if (new_edit_tool == SELECTGROUP) {
        cursor = wxCursor(wxCURSOR_CROSS);
        SetCursor(cursor);
    }
    else if (new_edit_tool == FILL) {
        if (edit_tool == SELECTGROUP) {
            current_selection.clear();
            needs_refresh = true;
            Refresh();
        }
        cursor = wxCursor(wxCURSOR_PAINT_BRUSH);
        SetCursor(cursor);
    }
    edit_tool = new_edit_tool;
}

//! wx Event call whenever a paint is requested.
/*!
 *  Defines what we should paint to the screen
 */
void ScrolledWindow::OnPaint(wxPaintEvent &WXUNUSED(event))
{
    try {
        wxBufferedPaintDC dc(this);
        wxMemoryDC memory_dc;
        memory_dc.SelectObject(*buffer);

        memory_dc.SetBrush(*wxBLACK_BRUSH);
        memory_dc.SetBackground(*wxBLACK);
        memory_dc.Clear();

        dc.Blit(0, 0, buffer->GetWidth(), buffer->GetHeight(), &memory_dc, 0, 0, wxCOPY);

        draw_tiles(memory_dc);
        draw_grid(memory_dc);

        dc.Blit(0, 0, buffer->GetWidth(), buffer->GetHeight(), &memory_dc, 0, 0, wxCOPY);

        if (needs_refresh) {
            Refresh(true);
            needs_refresh = false;
        }
    }
    CATCH_LOGIC_ERRORS
}


//! wx Event call whenever the window is resized.
/*!
 *  Adjusts the scrollbars when the map size has been adjusted.
 */
void ScrolledWindow::OnSize(wxSizeEvent &WXUNUSED(event))
{
    try {
        const wxSize area  = GetClientSize();
        delete buffer;
        buffer = new wxBitmap(area.GetWidth(), area.GetHeight());
        AdjustScrollbars();
        needs_refresh = true;
        Update();
    }
    CATCH_LOGIC_ERRORS
}


//! wx Event call whenever a screen clear is requested.
/*!
 *  Overridden to prevent screen flicker while using a double-buffer draw.
 **/
void ScrolledWindow::OnErase(wxEraseEvent &WXUNUSED(event))
{
}

void ScrolledWindow::on_left_up(wxMouseEvent &event)
{
    if (edit_tool == FILLGROUP) {
        if (current_selection.begin() != current_selection.end()) {
            if (!event.RightIsDown()) {
                bool in_bounds;
                for(std::vector<wxPoint>::iterator i = current_selection.begin(); i != current_selection.end(); i++) {
                    if(validate_point(*i)) {
                        in_bounds = set(*i);
                        VTANK_ASSERT(in_bounds);
                    }
                }
                current_selection.clear();
                needs_refresh = true;
                Refresh();
            }
        }
    }
}
//! Handle action when the left mouse is pressed down.
/*!
 *  This is called by wxWidgets when the user clicks on the sub-scrolling window.
 *
 *  \param event The event passes the (x, y) position of the mouse click.
 */
//lint -e1764
void ScrolledWindow::on_left_down(wxMouseEvent &event)
{
    try {
        if (current_map != NULL) {
            wxCoord x, y;
            event.GetPosition(&x, &y);
            int scroll_x = 0, scroll_y = 0;
            CalcUnscrolledPosition(scroll_x, scroll_y, &scroll_x, &scroll_y);
            const int tile_x = (x + scroll_x) / TILE_SIZE_X;
            const int tile_y = (y + scroll_y) / TILE_SIZE_Y;
            if (edit_tool == FILLGROUP || edit_tool == SELECTGROUP) {
                delete start_tile;
                start_tile = new wxPoint(tile_x, tile_y);
                if (validate_point(*start_tile)) {
                    current_selection = set_tile_selection(*start_tile, *start_tile);
                    needs_refresh = true;
                    Refresh();
                }
            }
            if (edit_tool == FILL) {
                wxPoint start = wxPoint(tile_x, tile_y);
                if(validate_point(start)) {
                    const int replace = get(start);
                    if(replace == tile_selector->get_selected_tile_id())
                        return;
                    (void)set(start);
                    const wxPoint north = wxPoint(start.x, (start.y-1));
                    if(validate_point(north)) {
                        if(get(north) == replace) {
                            need_paint->push(std::pair <wxPoint, std::string>(north,"north"));
                        }
                    }
                    const wxPoint east = wxPoint((start.x+1), start.y);
                    if(validate_point(east)) {
                        if(get(east) == replace) {
                            need_paint->push(std::pair <wxPoint, std::string>(east,"east"));
                        }
                    }
                    const wxPoint south = wxPoint(start.x, (start.y+1));
                    if(validate_point(south)) {
                        if(get(south) == replace) {
                            need_paint->push(std::pair <wxPoint, std::string>(south,"south"));
                        }
                    }
                    const wxPoint west = wxPoint((start.x-1), start.y);
                    if(validate_point(west)) {
                        if(get(west) == replace) {
                            need_paint->push(std::pair <wxPoint, std::string>(west,"west"));
                        }
                    }
                    fill(replace);
                    open_map->is_modified = true;
                    set_editor_status(GetGrandParent(), open_map);
                    needs_refresh = true;
                    Refresh();
                }
            }
            if (edit_tool == POINTER) {
                const bool in_bounds = set(wxPoint(tile_x, tile_y));
                VTANK_ASSERT(in_bounds);
                needs_refresh = true;
                Refresh();
            }
        }
    }
    CATCH_LOGIC_ERRORS
}
//lint +e1764


//! Handle action when the right mouse button is pressed down.
/*!
 *  This is called by wxWidgets when the user right clicks on the sub-scrolling window.
 *
 *  \param event The event passes the (x, y) position of the mouse click.
 */
//lint -e1764
void ScrolledWindow::on_right_down(wxMouseEvent &event)
{
    try {
        if(open_map != NULL) {
            if (current_map != NULL) {
                int x, y;
                event.GetPosition(&x, &y);
                int scroll_x = 0, scroll_y = 0;
                CalcUnscrolledPosition(scroll_x, scroll_y, &scroll_x, &scroll_y);
                const int tile_x = (x + scroll_x) / TILE_SIZE_X;
                const int tile_y = (y + scroll_y) / TILE_SIZE_Y;
                if (!validate_point(wxPoint(tile_x, tile_y)))
                    return; // Invalid tile
                if(edit_tool == POINTER) {
                    collision_flag = (current_map->get_tile(tile_x, tile_y).passable) ? 0 : 1;
                    if(current_map->get_tile_event(tile_x, tile_y) == 0) {
                        const bool in_bounds = current_map->set_tile_collision(tile_x, tile_y, static_cast<bool>(collision_flag));
                        VTANK_ASSERT(in_bounds);
                        open_map->is_modified = true;
                        set_editor_status(GetGrandParent(), open_map);
                        needs_refresh = true;
                        Refresh();
                    }
                }
                else if (edit_tool == FILLGROUP || edit_tool == SELECTGROUP) {
                    current_selection.clear();
                    needs_refresh = true;
                    Refresh();
                }
            }
        }
    }
    CATCH_LOGIC_ERRORS
}

void ScrolledWindow::on_mouse_scroll(wxMouseEvent &event)
{
    if(event.ShiftDown())
    {
        const int scroll_amount = event.m_wheelRotation / event.GetWheelDelta();
        if((global_tile_height + scroll_amount) >= 0 && (global_tile_height + scroll_amount) <= 20) {
            global_tile_height = global_tile_height + scroll_amount;
            wxString height("Tile Height: ", wxConvUTF8);
            height << global_tile_height;
            tile_height->SetLabel(height);
            needs_refresh = true;
            Refresh();
        }
    }
}

void ScrolledWindow::fill(const int replace)
{
    while(!need_paint->empty()) {
        std::pair<wxPoint, std::string> pair = need_paint->front();
        wxPoint point = pair.first;
        std::string direction = pair.second;
        need_paint->pop();
        const bool in_bounds = set(point);
        VTANK_ASSERT(in_bounds);
        const wxPoint north = wxPoint(point.x, (point.y-1));
        if(validate_point(north)) {
            if(get(north) == replace) {
                if(direction != "east") {
                    need_paint->push(std::pair <wxPoint, std::string>(north,"north"));
                }
            }
        }
        const wxPoint east = wxPoint((point.x+1), point.y);
        if(validate_point(east)) {
            if(get(east) == replace) {
                if(direction != "south") {
                    need_paint->push(std::pair <wxPoint, std::string>(east,"east"));
                }
            }
        }
        const wxPoint south = wxPoint(point.x, (point.y+1));
        if(validate_point(south)) {
            if(get(south) == replace) {
                if(direction != "west") {
                    need_paint->push(std::pair <wxPoint, std::string>(south,"south"));
                }
            }
        }
        const wxPoint west = wxPoint((point.x-1), point.y);
        if(validate_point(west)) {
            if(get(west) == replace) {
                if(direction != "north") {
                    need_paint->push(std::pair <wxPoint, std::string>(west,"west"));
                }
            }
        }
    }
}

std::vector<wxPoint> ScrolledWindow::set_tile_selection(const wxPoint &start, const wxPoint &finish) const
{
    std::vector<wxPoint> temp;
    if(start.x > finish.x) {
        if(start.y > finish.y) {
            for(int y = finish.y; y <= start.y; y++) {
                for(int x = finish.x; x <= start.x; x++) {
                    temp.push_back(wxPoint(x,y));
                }
            }
        }
        else {
            for(int y = start.y; y <= finish.y; y++) {
                for(int x = finish.x; x <= start.x; x++) {
                    temp.push_back(wxPoint(x,y));
                }
            }
        }
    }
    else {
        if(start.y > finish.y) {
            for(int y = finish.y; y <= start.y; y++) {
                for(int x = start.x; x <= finish.x; x++) {
                    temp.push_back(wxPoint(x,y));
                }
            }
        }
        else {
            for(int y = start.y; y <= finish.y; y++) {
                for(int x = start.x; x <= finish.x; x++) {
                    temp.push_back(wxPoint(x,y));
                }
            }
        }
    }
    return temp;
}
//lint +e1764
//! Handle action when the mouse is moved acrossed the window.
/*!
 *  This is called by wxWidgets when the user moves his mouse from any one point to any other
 *  point on the screen.
 *
 *  \param event The event passes the (x, y) position of... [what, exactly?].
 */
//lint -e1764
void ScrolledWindow::on_mouse_motion(wxMouseEvent &event)
{
    try {
        if (open_map != NULL) {
            if (current_map != NULL) {
                int x, y;
                event.GetPosition(&x, &y);
                int scroll_x = 0, scroll_y = 0;
                CalcUnscrolledPosition(scroll_x, scroll_y, &scroll_x, &scroll_y);
                const int tile_x = (x + scroll_x) / TILE_SIZE_X;
                const int tile_y = (y + scroll_y) / TILE_SIZE_Y;
                wxPoint current_point = wxPoint(tile_x, tile_y);
                if (!validate_point(current_point))
                    return; // Invalid tile.
                if (event.ButtonIsDown(wxMOUSE_BTN_LEFT)) {
                    if(edit_tool == POINTER) {
                        const bool in_bounds = set(current_point);
                        VTANK_ASSERT(in_bounds);
                        needs_refresh = true;
                        Refresh();
                    }
                    if(edit_tool == FILLGROUP || edit_tool == SELECTGROUP) {
                        const wxPoint start = *start_tile;
                        const wxPoint finish = current_point;
                        current_selection = set_tile_selection(start, finish);
                        needs_refresh = true;
                        Refresh();
                    }
                }
                if (event.ButtonIsDown(wxMOUSE_BTN_RIGHT) &&
                    current_point != last_tile &&
                    current_map->get_tile_event(tile_x, tile_y) == 0) {
                        if(edit_tool == POINTER) {
                            const bool in_bounds = current_map->set_tile_collision(tile_x, tile_y, static_cast<bool>(collision_flag));
                            VTANK_ASSERT(in_bounds);
                            open_map->is_modified = true;
                            set_editor_status(GetGrandParent(), open_map);
                            needs_refresh = true;
                            Refresh();
                        }
                }
                last_tile = current_point;
            }
        }
    }
    CATCH_LOGIC_ERRORS
}
//lint +e1764

void ScrolledWindow::cut() {
    if(open_map != NULL) {
        if(current_map != NULL) {
            if (current_selection.begin() != current_selection.end()) {
                tile_selection.clear();
                tile_points.clear();
                tile_points = current_selection;
                for(std::vector<wxPoint>::iterator i = tile_points.begin(); i != tile_points.end(); i++) {
                    const wxPoint point = *i;
                    tile_selection.push_back(current_map->get_tile(point.x, point.y));
                    const Tile tile = Tile();
                    const bool tile_set= current_map->set_tile(point.x, point.y, current_map->get_default_tile(),
                        tile.passable, tile.object_id, tile.event_id, tile.height, tile.type, tile.effect);
                    VTANK_ASSERT(tile_set);
                }
                open_map->is_modified = true;
                set_editor_status(GetGrandParent(), open_map);
                current_selection.clear();
                needs_refresh = true;
                Refresh();
            }
        }
    }
}

void ScrolledWindow::copy() {
    if (current_map != NULL) {
        if (current_selection.begin() != current_selection.end()) {
            tile_selection.clear();
            tile_points.clear();
            tile_points = current_selection;
            for(std::vector<wxPoint>::iterator i = tile_points.begin(); i != tile_points.end(); i++) {
                tile_selection.push_back(current_map->get_tile(i->x, i->y));
            }
            current_selection.clear();
            needs_refresh = true;
            Refresh();
        }
    }
}

void ScrolledWindow::paste() {
    if (open_map != NULL) {
        if(current_map != NULL) {
            if (tile_selection.begin() != tile_selection.end()) {
                if (tile_points.begin() != tile_points.end()) {
                    if (current_selection.begin() != current_selection.end()) {
                        const int x_offset = current_selection.begin()->x - tile_points.begin()->x;
                        const int y_offset = current_selection.begin()->y - tile_points.begin()->y;
                        std::vector<Tile>::iterator t = tile_selection.begin();
                        for (std::vector<wxPoint>::iterator p = tile_points.begin(); p != tile_points.end(); p++) {
                            const int x = p->x + x_offset;
                            const int y = p->y + y_offset;
                            if (x < current_map->get_width() || y < current_map->get_height()) {
                                const bool tile_set = current_map->set_tile(x, y, t->tile_id, t->passable, t->object_id, t->event_id,
                                    t->height, t->type, t->effect);
                                VTANK_ASSERT(tile_set);
                            }
                            t++;
                        }
                        open_map->is_modified = true;
                        set_editor_status(GetGrandParent(), open_map);
                        current_selection.clear();
                        needs_refresh = true;
                        Refresh();
                    }
                }
            }
        }
    }
}

void ScrolledWindow::delete_selected()
{
    if (open_map != NULL) {
        if(current_map != NULL) {
            if (current_selection.begin() != current_selection.end()) {
                 std::vector<wxPoint> sel= current_selection;
                for(std::vector<wxPoint>::iterator i = sel.begin(); i != sel.end(); i++) {
                    const Tile tile = Tile();
                    const bool tile_set= current_map->set_tile(i->x, i->y, current_map->get_default_tile(),
                        tile.passable, tile.object_id, tile.event_id, tile.height, tile.type, tile.effect);
                    VTANK_ASSERT(tile_set);
                }
                open_map->is_modified = true;
                set_editor_status(GetGrandParent(), open_map);
                current_selection.clear();
                needs_refresh = true;
                Refresh();
            }
        }
    }
}
void ScrolledWindow::select_all()
{
    if (current_map != NULL) {
        current_selection = set_tile_selection(wxPoint(0, 0),
            wxPoint(current_map->get_width()-1, current_map->get_height()-1));
        needs_refresh = true;
        Refresh();
    }
}

int ScrolledWindow::get(const wxPoint &point) const
{
    int id = 0;
    if(current_map != NULL) {
        if (tile_selector->get_edit_mode() == tile_selector->TERRAIN)
            id = current_map->get_tile(point.x, point.y).tile_id;
        else if (tile_selector->get_edit_mode() == tile_selector->OBJECT)
            id = current_map->get_tile_object(point.x, point.y);
        else {
            if(current_map->get_tile_collision(point.x, point.y))
                id = current_map->get_tile_event(point.x, point.y);
            else
                return id; //can't be an event if theres collision
        }
    }
    return id;
}


bool ScrolledWindow::set(const wxPoint &point)
{
    bool in_bounds = false;
    if(open_map != NULL) {
        if(current_map != NULL) {
            if(tile_selector->get_edit_mode() == tile_selector->TERRAIN) {
                const bool one = current_map->set_tile_id(point.x, point.y, tile_selector->get_selected_tile_id());
                const bool two = current_map->set_tile_height(point.x, point.y, global_tile_height);
                in_bounds = one && two;
            }
            else if(tile_selector->get_edit_mode() == tile_selector->OBJECT) {
                in_bounds = current_map->set_tile_object(point.x, point.y, tile_selector->get_selected_tile_id());
            }
            else {
                if(current_map->get_tile_collision(point.x, point.y)) {
                    in_bounds = current_map->set_tile_event(point.x, point.y, tile_selector->get_selected_tile_id());
                }
                else
                    return in_bounds;
            }
            open_map->is_modified = true;
            set_editor_status(GetGrandParent(), open_map);
        }
    }
    return in_bounds;
}

bool ScrolledWindow::validate_point(const wxPoint &point) const
{
    if(current_map != NULL) {
        if (point.x < current_map->get_width() && point.x >= 0 && point.y < current_map->get_height() && point.y >= 0)
            return true;
        else
            return false;
    }
    return false;
}


//! Draw the grid onto the buffer.
/*!
 *  Draws a grid to outline where tiles will be placed.
 *
 *  \param dc Device context onto which drawing is to take place. Usually a memory DC.
 */
void ScrolledWindow::draw_grid(wxDC& dc) const
{
    //if the display_grid is flagged true, draw the grid
    if(display_grid) {
        wxPen pen(wxColour(255, 255, 255));
        dc.SetPen(pen);
        const wxSize window_size = GetClientSize();

        int scroll_x = 0, scroll_y = 0;
        CalcUnscrolledPosition(scroll_x, scroll_y, &scroll_x, &scroll_y);

        const int num_tiles_x = (window_size.GetWidth() / TILE_SIZE_X) + 1;
        for (int x = 0; x < num_tiles_x; x++) {
            dc.DrawLine(x * TILE_SIZE_X, 0, x * TILE_SIZE_X, window_size.GetHeight());
        }

        const int num_tiles_y = (window_size.GetHeight() / TILE_SIZE_Y) + 1;
        for (int y = 0; y < num_tiles_y; y++) {
            dc.DrawLine(0, y * TILE_SIZE_Y, window_size.GetWidth(), y * TILE_SIZE_Y);
        }
    }
}


//! Draw the tiles onto the buffer.
/*!
 *  Draws tiles onto the screen according to the given tile dictionary. If no map was set this
 *  function does nothing.
 *
 *  \param dc Device context onto which drawing is to take place.  Usuaully a memory DC.
 */
void ScrolledWindow::draw_tiles(wxDC& dc) const
{
    if (current_map == NULL) {
        return;
    }
    // Probably the constructor should require a non-null tile_dictionary as an argument.
    VTANK_ASSERT(terrain_dictionary != NULL);
    VTANK_ASSERT(object_dictionary != NULL);
    VTANK_ASSERT(event_dictionary != NULL);

    const wxSize window_size = GetClientSize();

    int scroll_x = 0;
    int scroll_y = 0;
    CalcUnscrolledPosition(scroll_x, scroll_y, &scroll_x, &scroll_y);

    const int map_width  = current_map->get_width();
    const int map_height = current_map->get_height();

    const int num_tiles_x = (window_size.GetWidth()  / TILE_SIZE_X) + 1;
    const int num_tiles_y = (window_size.GetHeight() / TILE_SIZE_Y) + 1;
    for (int y = 0; y < num_tiles_y; y++) {
        for (int x = 0; x < num_tiles_x; x++) {
            const int tile_x = x + (scroll_x / TILE_SIZE_X);
            const int tile_y = y + (scroll_y / TILE_SIZE_Y);
            if (tile_x >= map_width || tile_y >= map_height) {
                continue;
            }
            const int tile_id = current_map->get_tile(tile_x, tile_y).tile_id;
            std::map<int, wxBitmap>::iterator i = terrain_dictionary->find(tile_id);
            if (i != terrain_dictionary->end()) {
                wxBitmap temp(i->second);
                dc.DrawBitmap(temp, x * TILE_SIZE_X, y * TILE_SIZE_Y, false);
            }
            const unsigned short object_id = current_map->get_tile(tile_x, tile_y).object_id;
            i = object_dictionary->find(object_id);
            if (object_id != 0 && i != object_dictionary->end()) {
                wxBitmap temp(i->second);
                dc.DrawBitmap(temp, x * TILE_SIZE_X, y * TILE_SIZE_Y, false);
            }
            const unsigned short event_id = current_map->get_tile(tile_x, tile_y).event_id;
            i = event_dictionary->find(event_id);
            if (event_id != 0 && i != event_dictionary->end()) {
                wxBitmap temp(i->second);
                dc.DrawBitmap(temp, x * TILE_SIZE_X, y * TILE_SIZE_Y, false);
            }
            if (display_collision && !current_map->get_tile(tile_x, tile_y).passable) {
                dc.DrawBitmap(*collision, x * TILE_SIZE_X, y * TILE_SIZE_Y, true);
            }
            if (display_height) {
                wxString text;
                const int height = current_map->get_tile_height(tile_x, tile_y);
                text << height;
                dc.SetTextForeground(*wxRED);
                dc.DrawText(text, ((x * TILE_SIZE_X)+2), (y * TILE_SIZE_Y));
            }
            if(current_selection.begin() != current_selection.end()) {
                std::vector<wxPoint> sel = current_selection;
                for(std::vector<wxPoint>::iterator p = sel.begin(); p != sel.end(); p++) {
                    if (*p == wxPoint(tile_x, tile_y)) {
                        dc.DrawBitmap(*selection, x * TILE_SIZE_X, y * TILE_SIZE_Y, true);
                        break;
                    }
                }
            }
        }
    }
}
