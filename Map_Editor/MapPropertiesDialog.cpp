/*!
    \file   MapPropertiesDialog.cpp
    \brief  Implementation for the map editor map properties dialog.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#include "Library.hpp"
#include "MapPropertiesDialog.hpp"
#include "Support.hpp"


//lint -save -e1924
BEGIN_EVENT_TABLE(MapPropertiesDialog, wxDialog)
    EVT_BUTTON(wxID_EXIT,   MapPropertiesDialog::on_mp_quit)
    EVT_BUTTON(wxID_CANCEL, MapPropertiesDialog::on_mp_ok)
END_EVENT_TABLE()
//lint -restore


//! Constructor
/*!
 * Required for the parent wxDialog. Simply initializes the wxDialog with some default values
 * and the text boxes as follows.
 */
MapPropertiesDialog::MapPropertiesDialog(const wxString &dialog_title,
                                         int *current_width,
                                         int *current_height,
                                         wxString *current_map_title)
    : wxDialog(NULL, -1, dialog_title, wxDefaultPosition, wxSize(250, 230)),
      noncopyable(),
      width_text_box (NULL),
      height_text_box(NULL),
      title_text_box (NULL),
      panel          (new wxPanel(this, -1)),
      v_box          (new wxBoxSizer(wxVERTICAL)),
      h_box          (new wxBoxSizer(wxHORIZONTAL)),
      static_box     (NULL),
      name_text      (NULL),
      x_text         (NULL),
      y_text         (NULL),
      ok_button      (NULL),
      close_button   (NULL),
      width          (current_width),
      height         (current_height),
      title          (current_map_title)
{
    static_box = new wxStaticBox
        (panel, 
         1,
         wxT("Enter the map name and the size in tiles"),
         wxPoint(5, 5),
         wxSize(240, 150));

    name_text = new wxStaticText
        (panel,
         -1,
         wxT("Name:"),
         wxPoint(40,57),
         wxDefaultSize,
         0,
         wxT("ID_STATICTEXT2"));

    title_text_box = new wxTextCtrl
        (panel,
         -1,
         *current_map_title,
         wxPoint(75, 55));

    wxString str;
    str << *current_width;

    x_text = new wxStaticText
        (panel,
         -1,
         wxT("Width:"),
         wxPoint(40,82),
         wxDefaultSize,
         0,
         wxT("ID_STATICTEXT0"));

    width_text_box = new wxTextCtrl(panel, -1, str, wxPoint(75, 80));

    str.Clear();
    str << *current_height;

    y_text = new wxStaticText
        (panel,
         -1,
         wxT("Height:"),
         wxPoint(38,107),
         wxDefaultSize,
         0,
         wxT("ID_STATICTEXT1"));

    height_text_box = new wxTextCtrl(panel, -1, str, wxPoint(75, 105));

    ok_button = new wxButton
        (this,
         wxID_CANCEL,
         wxT("OK"),
         wxDefaultPosition,
         wxSize(70, 30));

    close_button = new wxButton
        (this,
         wxID_EXIT,
         wxT("Cancel"),
         wxDefaultPosition,
         wxSize(70, 30));

    h_box->Add(ok_button, 1);
    h_box->Add(close_button, 1, wxLEFT, 5);
    v_box->Add(panel, 1, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);
    v_box->Add(h_box, 0, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);

    SetSizer(v_box);
    Centre();
}


//! Close the dialog.
/*!
 * This event handler handles closing the dialog when the cancel button is pressed.
 */
void MapPropertiesDialog::on_mp_quit(wxCommandEvent& WXUNUSED(event))
{
    EndModal(wxCANCEL);
}


//! Get the input.
/*!
 *  Checks the validity of the input and stores it if it is correct. 
 *
 * \param event is required for event funcionality, but is unused.
 */
void MapPropertiesDialog::on_mp_ok(wxCommandEvent& WXUNUSED(event))
{
    try {
        wxString str_x = width_text_box->GetValue();
        wxString str_y = height_text_box->GetValue();
        wxString str_name = title_text_box->GetValue();
        str_name.Trim(false); // Get rid of any leading white spaces

        double temp_x_double;
        double temp_y_double;

        // Check for a null string in the name field
        if (str_name.IsNull()) {
            (void)wxMessageBox(wxT("You didn't set a map title"),
                               wxT("No map title."),
                               wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
            title_text_box->SetFocus();
            return;
        }

        // Make sure name has valid chars.  If it does warn and retry
        for (wxString::size_type i = 0; i < str_name.size(); i++) {
            if (str_name[i] == '\\' ||
                str_name[i] == '/'  ||
                str_name[i] == ':'  ||
                str_name[i] == '*'  ||
                str_name[i] == '*'  ||
                str_name[i] == '?'  ||
                str_name[i] == '"'  ||
                str_name[i] == '\'' ||
                str_name[i] == '<'  ||
                str_name[i] == '>'  ||
                str_name[i] == '|') {

                (void)wxMessageBox(wxT("Map Title contains illegal characters: \\/:*?\"\'<>|"),
                                   wxT("Invalid Map Title"),
                                   wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
                title_text_box->SetFocus();
                return;
            }
        }

        if (str_x.ToDouble(&temp_x_double) && str_y.ToDouble(&temp_y_double)) {
            *width  = static_cast<int>(temp_x_double);
            *height = static_cast<int>(temp_y_double);

            if (*width < 1 || *height < 1) {
                (void)wxMessageBox(wxT("Map must be atleast 1 x 1"),
                                   wxT("Invalid Map Size"),
                                   wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
                width_text_box->SetFocus();
                return;
            }
        }
        else {
            (void)wxMessageBox(wxT("X and Y must be valid numbers"),
                               wxT("Invalid Map Size"),
                               wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
            width_text_box->SetFocus();
            return;
        }

        *title = str_name;
        EndModal(wxOK);
    }
    CATCH_LOGIC_ERRORS
}
