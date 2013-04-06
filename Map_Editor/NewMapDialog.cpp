/*!
    \file   NewMapDialog.cpp
    \brief  Implementation for the map editor map startup dialog.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#include "Library.hpp"
#include "NewMapDialog.hpp"
#include "Support.hpp"
#include "ToolboxWindow.hpp"

//lint -save -e1924
BEGIN_EVENT_TABLE(NewMapDialog, wxDialog)
    EVT_BUTTON(wxID_OK,     NewMapDialog::on_ok    )
    EVT_BUTTON(wxID_CANCEL, NewMapDialog::on_cancel)
END_EVENT_TABLE()
//lint -restore

//! Constructor
/**
 * Required for the parent wxDialog. Simply initializes the wxDialog with some default values
 */
NewMapDialog::NewMapDialog(int *width, int *height, wxString *title, int *default_tile)
    : wxDialog(NULL, wxID_ANY,
               wxT("Create a new map"),
               wxDefaultPosition,
               wxSize(250, 300),
               wxDEFAULT_DIALOG_STYLE | wxSTAY_ON_TOP),
      noncopyable  (),
      x_text_box   (NULL),
      y_text_box   (NULL),
      name_text_box(NULL),
      panel        (NULL),
      v_box        (NULL),
      h_box        (NULL),
      static_box   (NULL),
      name_text    (NULL),
      x_text       (NULL),
      y_text       (NULL),
      t_text       (NULL),
      ok_button    (NULL),
      close_button (NULL),
      base_tile    (NULL),
      new_width    (width),
      new_height   (height),
      new_name     (title),
      new_default_tile(default_tile),
      terrain_dictionary(std::map<int, wxBitmap>()),
      object_dictionary (std::map<int, wxBitmap>()),
      event_dictionary  (std::map<int, wxBitmap>())
{
    const long ID_STATICTEXT0 = wxNewId();
    const long ID_STATICTEXT1 = wxNewId();

    *width  = 50;  // Default map values.
    *height = 50;
    *title  = wxT("Untitled");

    //build the GUI parts and add them to the dialog
    panel = new wxPanel(this, -1);

    v_box = new wxBoxSizer(wxVERTICAL); //vertical box sizer
    h_box = new wxBoxSizer(wxHORIZONTAL); //horizontal box sizer

    static_box = new wxStaticBox(panel, -1, wxT("Enter the new map attributes")
                                ,wxPoint(5, 5), wxSize(240, 300));

    name_text = new wxStaticText(panel, -1, _("Name:"), wxPoint(40,35), wxDefaultSize, 0,
                                wxT("ID_STATICTEXT2"));
    name_text_box = new wxTextCtrl(panel, -1, wxT("Untitled"),wxPoint(75, 35));

    x_text = new wxStaticText(panel, ID_STATICTEXT0, _("X:"), wxPoint(60,60), wxDefaultSize, 0,
                                wxT("ID_STATICTEXT0"));
    x_text_box = new wxTextCtrl(panel, -1, wxT("50"),wxPoint(75, 60));

    y_text = new wxStaticText(panel, ID_STATICTEXT1, _("Y:"), wxPoint(60,85), wxDefaultSize, 0,
                                wxT("ID_STATICTEXT1"));
    y_text_box = new wxTextCtrl(panel, -1, wxT("50"),wxPoint(75, 85));

    t_text = new wxStaticText(panel, -1, _("Default Tile:"), wxPoint(40, 115), wxDefaultSize, 0,
                                                            wxT("ID_STATICTEXT2"));

    ok_button = new wxButton(this, wxID_OK, wxT("OK"), wxDefaultPosition, wxSize(70, 30));
    close_button = new wxButton(this, wxID_CANCEL, wxT("Cancel"), wxDefaultPosition,
                                wxSize(70, 30));

    h_box->Add(ok_button, 1);
    h_box->Add(close_button, 1, wxLEFT, 5);

    v_box->Add(panel, 1, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);
    v_box->Add(h_box, 0, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);

    SetSizer(v_box);

    Raise();
}


//! Initialize the default tile selector.
/**
 * Create and initialize the ToolboxWindow that is used to select the default tile in the new
 * map.
 */
void NewMapDialog::init_default_tile_selector(std::map<int, wxBitmap> ter_dict, std::map<int, wxBitmap> obj_dict, std::map<int, wxBitmap> evt_dict)
{
    terrain_dictionary = ter_dict;
    object_dictionary = obj_dict;
    event_dictionary = evt_dict;
    base_tile =
        new ToolboxWindow(panel, wxID_ANY, wxPoint(static_cast<int>((250-((COLUMNS*64)+15))/2), 135),wxSize(((COLUMNS*64)+15), 64), &terrain_dictionary, &object_dictionary, &event_dictionary);
	base_tile->set_edit_mode(base_tile->TERRAIN);
    base_tile->EnableScrolling(true, false);

    const int complete_row_count = static_cast<int>(terrain_dictionary.size() / COLUMNS);
    const int toolbox_row_count = complete_row_count + ((terrain_dictionary.size() % COLUMNS) == 0 ? 0 : 1);
    base_tile->SetScrollbars(0, TILE_SIZE_Y, 0, toolbox_row_count);
    base_tile->Update();
    base_tile->Refresh();
}


//! Get the input.
/*!
 * This event handler checks the validity of the input and stores it if it is correct.
 *
 * \param event is required for event funcionality, but is unused.
 */
void NewMapDialog::on_ok(wxCommandEvent& event)
{
    try {
        if (event.GetEventObject() != ok_button) {
            event.Skip();
        }
        else {
            wxString str_x = x_text_box->GetValue();
            wxString str_y = y_text_box->GetValue();
            wxString str_name = name_text_box->GetValue();
            str_name.Trim(false); // Get rid of any leading white spaces.

            double temp_x_double;
            double temp_y_double;
            bool valid_data = true;

            // Check for a null string in the name field.
            if (str_name.IsNull()) {
                (void)wxMessageBox(wxT("You didn't enter a map title, please try again"),
                                   wxT("Invalid Map Title"),
                                   wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
                name_text_box->SetFocus();
                return;
            }

            // Make sure name has valid characters. If it doesn't, warn and retry.
            for (wxString::size_type i = 0; i < str_name.size(); i++) {
                if (str_name[i] == '\\' ||
                    str_name[i] == '/'  ||
                    str_name[i] == ':'  ||
                    str_name[i] == '*'  ||
                    str_name[i] == '?'  ||
                    str_name[i] == '"'  ||
                    str_name[1] == '\'' ||
                    str_name[i] == '<'  ||
                    str_name[i] == '>'  ||
                    str_name[i] == '|') {
                    valid_data = false;

                    (void)wxMessageBox(wxT("You entered an invalid character: \\/:*?\"\'<>|"),
                                       wxT("Invalid Characters"),
                                       wxOK | wxICON_ERROR);
                    name_text_box->SetFocus();
                    break;
                }
            }

            *new_name = str_name;

            if (str_x.ToDouble(&temp_x_double) && str_y.ToDouble(&temp_y_double)) {
                *new_width = static_cast<int>(temp_x_double);
                *new_height = static_cast<int>(temp_y_double);

                if (*new_width < 1 || *new_height < 1) {
                    valid_data = false;
                    (void)wxMessageBox(wxT("The map width and height must be at least 1"),
                                       wxT("Invalid Map Size"),
                                       wxOK | wxICON_ERROR);
                    x_text_box->SetFocus();
                }
            }
            else {  // Invalid input warn and retry.
                valid_data = false;
                (void)wxMessageBox(wxT("The map width and height must be numbers"),
                                   wxT("Invalid Width and Height"),
                                   wxOK | wxICON_ERROR);
                x_text_box->SetFocus();
            }
            if(base_tile != NULL) {
                *new_default_tile = base_tile->get_selected_tile_id();
            }
            if (valid_data) {

                EndModal(wxOK);
            }
        }
    }
    CATCH_LOGIC_ERRORS
}


//! Cancel the dialog
/*!
 * This event handler ends the modal state with the wxCANCEL flag and sets the member
 * variable cancled to true.
 *
 * \param event is required for event functionality, but is unused.
 */
void NewMapDialog::on_cancel(wxCommandEvent &WXUNUSED(event))
{
    EndModal(wxCANCEL);
}
