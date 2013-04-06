/*!
    \file   NewMapDialog.hpp
    \brief  Interface of the map editor map startup dialog.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef NEWMAPDIALOG_HPP
#define NEWMAPDIALOG_HPP

#include "ToolboxWindow.hpp"

//! A dialog to get information needed to make a map.
/**
 * The NewMapDialog class creates a dialog that asks the user to input a title, a map width and
 * a map height. These values are stored within the NewMapDialog object and should be used to
 * create a new Map.
 */
class NewMapDialog : public wxDialog, private boost::noncopyable {
    //lint -save -e1516
    DECLARE_EVENT_TABLE()
    //lint -restore

public:
    NewMapDialog(int *width, int *height, wxString *title, int *default_tile);


    void init_default_tile_selector(std::map<int, wxBitmap> ter_dict, std::map<int, wxBitmap> obj_dict, std::map<int, wxBitmap> evt_dict);

    void on_ok(wxCommandEvent &event);
    void on_cancel(wxCommandEvent &event);

private:
    //GUI parts that make up the dialog.
    wxTextCtrl   *x_text_box;
    wxTextCtrl   *y_text_box;
    wxTextCtrl   *name_text_box;
    wxPanel      *panel;
    wxBoxSizer   *v_box;
    wxBoxSizer   *h_box;
    wxStaticBox  *static_box;
    wxStaticText *name_text;
    wxStaticText *x_text;
    wxStaticText *y_text;
    wxStaticText *t_text;
    wxButton     *ok_button;
    wxButton     *close_button;

    ToolboxWindow *base_tile;

    int      *new_width;
    int      *new_height;
    wxString *new_name;
    int      *new_default_tile;

    std::map<int, wxBitmap> terrain_dictionary;
    std::map<int, wxBitmap> object_dictionary;
    std::map<int, wxBitmap> event_dictionary;
};

#endif
