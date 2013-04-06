/*!
    \file   ToolboxWindow.cpp
    \brief  Implementation of toolbox scrolling windows.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#include "Library.hpp"
#include "config.hpp"
#include "ToolboxWindow.hpp"
#include "Support.hpp"

//lint -e1924
BEGIN_EVENT_TABLE(ToolboxWindow, wxScrolledWindow)
  EVT_PAINT           (ToolboxWindow::OnPaint     )
  EVT_ERASE_BACKGROUND(ToolboxWindow::OnErase     )
  EVT_SIZE            (ToolboxWindow::OnSize      )
  EVT_LEFT_DOWN       (ToolboxWindow::on_left_down)
END_EVENT_TABLE()
//lint +e1924


//! Construct a ToolboxWindow.
/*!
 *  The constructor sets up the window and initializes it with a dictionary of tiles to be
 *  displayed. In the future additional tool initialization may also be done as appropriate.
 *
 *  \param parent     The parent window of this ToolboxWindow.
 *  \param id         ID of the window generated through wxWidgets (but tracked by the user).
 *  \param pos        Position of the window.
 *  \param size       Size of the window.
 *  
 */
ToolboxWindow::ToolboxWindow(
    wxPanel       *parent,
    wxWindowID     id,
    const wxPoint &pos,
    const wxSize  &size,
    std::map<int, wxBitmap> *ter_dictionary,
    std::map<int, wxBitmap> *obj_dictionary,
    std::map<int, wxBitmap> *evt_dictionary)

    : wxScrolledWindow(parent, id, pos, size,  wxNO_BORDER,  wxT("Tile View")),
      terrain_dictionary(ter_dictionary),
      object_dictionary(obj_dictionary),
      event_dictionary(evt_dictionary),
      noncopyable(),
      buffer(NULL),
      border(NULL),
      edit_mode(TERRAIN),
      selected_id(),
      needs_refresh(true)
{
    const wxSize area = GetClientSize();
    buffer      = new wxBitmap(area.GetWidth(), area.GetHeight());
    border      = new wxBitmap();
    selected_id = 0U;

    wxString resource_root(Support::lookup_parameter("RESOURCE_ROOT")->c_str(), wxConvUTF8);

    if (!border->LoadFile(
            Support::normalize_path_wx(resource_root + wxT("/data/terrain/green_border.png")),
            wxBITMAP_TYPE_PNG)) {
        (void)wxMessageBox(wxT("Unable to load border tile"));
    }
}


//! Destructor.
ToolboxWindow::~ToolboxWindow()
{
    delete buffer;
    delete border;
    terrain_dictionary = NULL;
    object_dictionary = NULL;
    event_dictionary = NULL;
}

void ToolboxWindow::set_edit_mode(const layer edit_layer)
{
    edit_mode = edit_layer;
}

//! wx Event call whenever a paint is requested.
void ToolboxWindow::OnPaint(wxPaintEvent &WXUNUSED(event))
{
    try {
        wxBufferedPaintDC dc(this);
        wxMemoryDC memory_dc;
        memory_dc.SelectObject(*buffer);

        memory_dc.SetBrush(*wxBLACK_BRUSH);
        memory_dc.SetBackground(*wxBLACK);
        memory_dc.Clear();
        if (!dc.Blit(0, 0, buffer->GetWidth(), buffer->GetHeight(), &memory_dc, 0, 0)) {
            (void)wxMessageBox(wxString("Toolbox window may be corrupt", wxConvUTF8));
        }
        draw_tiles(memory_dc);
        if (!dc.Blit(0, 0, buffer->GetWidth(), buffer->GetHeight(), &memory_dc, 0, 0)) {
            (void)wxMessageBox(wxString("Toolbox window may be corrupt", wxConvUTF8));
        }

        if (needs_refresh) {
            Refresh(true);
            needs_refresh = false;
        }
    }
    CATCH_LOGIC_ERRORS
}


//! wxWidgets event called whenever the window is resized.
/*!
 *  Adjusts the scrollbars when the map size has been adjusted.
 */
void ToolboxWindow::OnSize(wxSizeEvent &WXUNUSED(event))
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


//! wxWidgets event called whenever a screen clear is requested.
/*!
 *  Overridden to prevent screen flicker while using a double-buffer draw.
 */
void ToolboxWindow::OnErase(wxEraseEvent &WXUNUSED(event))
{
}


//! Handle action when the left mouse is pressed down.
/*!
 * This is called by wxWidgets when the user clicks on the sub-scrolling window.
 *
 * \param event The event passes the (x, y) position in the map.
 */
//lint -e1764
void ToolboxWindow::on_left_down(wxMouseEvent &event)
{
    try {
        wxCoord x;
        wxCoord y;
        event.GetPosition(&x, &y);

        int scroll_x = 0;
        int scroll_y = 0;
        CalcUnscrolledPosition(scroll_x, scroll_y, &scroll_x, &scroll_y);

        const int tile_x = (x + scroll_x) / TILE_SIZE_X;
        const int tile_y = (y + scroll_y) / TILE_SIZE_Y;
        
        std::map<int, wxBitmap> *dictionary;
        std::map<int, wxBitmap>::const_iterator p;
        if(edit_mode == TERRAIN) {
            dictionary = terrain_dictionary;
            p = dictionary->find(tile_y * COLUMNS + tile_x);
        }
        else if(edit_mode == OBJECT) {
            dictionary = object_dictionary;
            p = dictionary->find(tile_y * COLUMNS + (tile_x));
        }
        else {
            dictionary = event_dictionary;
            p = dictionary->find(tile_y * COLUMNS + (tile_x));
        }

        if (p != dictionary->end()) {
            selected_id = p->first;
        }
        Refresh(false);
    }
    CATCH_LOGIC_ERRORS
}
//lint +e1764


//! Draw the tiles into a device context.
/*!
 *  Draws tiles into a device context according to the current tile dictionary. If no map was
 *  set this function does nothing.
 *
 * \param dc Device context to be used for drawing. Usually a memory context.
 */
void ToolboxWindow::draw_tiles(wxDC &dc)
{
    std::map<int, wxBitmap> *dictionary;
    if(edit_mode == TERRAIN) {
        dictionary = terrain_dictionary;
    }
    else if(edit_mode == OBJECT) {
        dictionary = object_dictionary;
    }
    else {
        dictionary = event_dictionary;
    }
    wxCoord scroll_x = 0;
    wxCoord scroll_y = 0;
    CalcUnscrolledPosition(scroll_x, scroll_y, &scroll_x, &scroll_y);

    int x = 0;
    int y = 0;
    for (std::map<int, wxBitmap>::const_iterator it = dictionary->begin();
         it != dictionary->end();
         ++it) {
        
        if (x == COLUMNS) {
            x = 0;
            y++;
        }

        wxBitmap temp(it->second);
        dc.DrawBitmap(temp, x * TILE_SIZE_X, y * TILE_SIZE_Y - scroll_y, false);
        if (it->first == selected_id) {
            dc.DrawBitmap(*border, x * TILE_SIZE_X, y * TILE_SIZE_Y - scroll_y, false);
        }
        x++;
    }
}
