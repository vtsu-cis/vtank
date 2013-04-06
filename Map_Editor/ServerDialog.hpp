/*!
    \file   ServerLoginDialog.hpp
    \brief  Interface to the server login dialog box.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef SERVERDIALOG_HPP
#define SERVERDIALOG_HPP

//! Server map list interface
/*!
 * The ServerMapListDialog class creates a user friendly interface to the VTank server for
 * viewing maps available.
 */
class ServerDialog : public wxDialog, private boost::noncopyable {
    //lint -save -e1516
    DECLARE_EVENT_TABLE()
    //lint -restore

public:
    enum { CANCELED,
           FAILURE};

    explicit ServerDialog();
    const wxArrayString get_array_data(const VTankObject::MapList &map_list) const;
    const wxString get_login() const;
    const wxString get_pass() const;
    void quit_properly(int reason);
    // Event handlers.
    void on_select     (wxCommandEvent &event);
    void on_quit       (wxCommandEvent &event);
    void on_download   (wxCommandEvent &event);
    void on_remove     (wxCommandEvent &event);
    void on_upload     (wxCommandEvent &event);
    void on_login      (wxCommandEvent &event);

private:
    boost::thread keep_active;
    wxButton   *remove_button;
    wxButton   *download_button;
    wxButton   *login_button;
    wxButton   *upload_button;
    wxListBox  *map_list_box;
    wxTextCtrl *username_text_box;
    wxTextCtrl *password_text_box;
};

#endif
