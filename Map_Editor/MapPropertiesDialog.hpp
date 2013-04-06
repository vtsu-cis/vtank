/*!
    \file   MapPropertiesDialog.hpp
    \brief  Interface of the map editor map map properties dialog.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef MAPPROPERTIESDIALOG_HPP
#define MAPPROPERTIESDIALOG_HPP

//! A dialog to view and/or edit a map's properties.
/*!
 * The MapPropertiesDialog class creates a dialog that asks the user to input a title, a map
 * width and a map height. These values are stored within the NewMapDialog object and should be
 * used to create a new Map (map.h) before it is destroyed. This class is similar to
 * NewMapDialog, except that it takes current map values.
 */
class MapPropertiesDialog : public wxDialog, private boost::noncopyable {
    //lint -save -e1516
    DECLARE_EVENT_TABLE()
    //lint -restore

public:
    MapPropertiesDialog(const wxString &dialog_title,
                        int      *current_width,
                        int      *current_height,
                        wxString *current_map_title);

    void on_mp_quit(wxCommandEvent& event);
    void on_mp_ok  (wxCommandEvent& event);

private:
    wxTextCtrl *width_text_box;
    wxTextCtrl *height_text_box;
    wxTextCtrl *title_text_box;

    wxPanel      *panel;
    wxBoxSizer   *v_box;
    wxBoxSizer   *h_box;
    wxStaticBox  *static_box;
    wxStaticText *name_text;
    wxStaticText *x_text;
    wxStaticText *y_text;
    wxButton     *ok_button;
    wxButton     *close_button;

    int      *width;
    int      *height;
    wxString *title;
};

#endif
