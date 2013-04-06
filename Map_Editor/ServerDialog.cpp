/*!
    \file   ServermaplistDialog.cpp
    \brief  Implementation of the server maplist dialog box.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#include "Library.hpp"
#include "ServerCommunication.hpp"
#include "ServerDialog.hpp"
#include "VTankObjects.h"
#include "Support.hpp"

//lint -e1924
BEGIN_EVENT_TABLE(ServerDialog, wxDialog)
    EVT_BUTTON(wxID_EXIT,       ServerDialog::on_quit      )
    EVT_BUTTON(wxID_DOWN,       ServerDialog::on_download  )
    EVT_BUTTON(wxID_DELETE,     ServerDialog::on_remove    )
    EVT_BUTTON(wxID_OPEN,       ServerDialog::on_upload    )
    EVT_BUTTON(wxID_OK,         ServerDialog::on_login     )
    EVT_LISTBOX(wxID_DEFAULT,   ServerDialog::on_select    )
END_EVENT_TABLE()
//lint +e1924

//! Constructor that creates a new maplist dialog.
/*!
 *  \param map_list A sequence of strings representing the name of maps that is
 *  currently stored on the server.
 */
ServerDialog::ServerDialog()
       : wxDialog(NULL, -1, wxT("Server Map List"), wxDefaultPosition, wxSize(367,315)),
         noncopyable       (),
         keep_active       (),
         remove_button     (NULL),
         download_button   (NULL),
         login_button      (NULL),
         upload_button     (NULL),
         map_list_box      (NULL),
         username_text_box (NULL),
         password_text_box (NULL)
{
    const long ID_STATICTEXT0 = wxNewId();
    const long ID_STATICTEXT1 = wxNewId();

    wxPanel    *const panel1 = new wxPanel(this, -1);
    wxPanel    *const panel2 = new wxPanel(this, -1);
    wxBoxSizer *const v_box1 = new wxBoxSizer(wxVERTICAL);
    wxBoxSizer *const h_box1 = new wxBoxSizer(wxHORIZONTAL);
    wxBoxSizer *const v_box2 = new wxBoxSizer(wxVERTICAL);
    wxBoxSizer *const h_box2 = new wxBoxSizer(wxHORIZONTAL);
    wxBoxSizer *const v_box3 = new wxBoxSizer(wxVERTICAL);
    wxBoxSizer *const h_box3 = new wxBoxSizer(wxHORIZONTAL);

   new wxStaticBox(panel1,
        -1,
        _("Server Map List"),
        wxPoint(10, 0),
        wxSize(254, 182));

   map_list_box =
        new wxListBox(panel1,
        wxID_DEFAULT,
        wxPoint(23, 19),
        wxSize(229, 149),
        NULL,
        wxLB_NEEDED_SB,
        wxDefaultValidator,
        wxListBoxNameStr);

   new wxStaticText(panel2,
        ID_STATICTEXT0,
        _("Username:"),
        wxPoint(5,5),
        wxDefaultSize,
        0,
        wxT(""));

   username_text_box =
        new wxTextCtrl(panel2,
            wxTE_RICH,
            get_login(),
            wxPoint(5, 20));

    new wxStaticText(panel2,
        ID_STATICTEXT1,
        _("Password:"),
        wxPoint(225,5),
        wxDefaultSize,
        0,
        wxT(""));

   password_text_box =
        new wxTextCtrl(panel2,
            wxID_DOWN,
            get_pass(),
            wxPoint(225,20),
            wxDefaultSize,
            wxTE_PASSWORD,
            wxDefaultValidator,
            wxT(""));

    remove_button =
        new wxButton(this,
            wxID_DELETE,
            _("Remove"),
            wxPoint(10, 10),
            wxSize(70, 30));

    download_button =
        new wxButton(this,
            wxID_DOWN,
            _("Download"),
            wxPoint(10,40),
            wxSize(70, 30));

    wxButton *const close_button =
        new wxButton(this,
            wxID_EXIT,
            _("Logout"),
            wxDefaultPosition,
            wxSize(70, 30));
    login_button =
        new wxButton(this,
            wxID_OK,
            _("Login"),
            wxDefaultPosition,
            wxSize(70, 30));
    upload_button =
        new wxButton(this,
            wxID_OPEN,
            _("Upload"),
            wxDefaultPosition,
            wxSize(70, 30));
    username_text_box->SetFocus();
    login_button->SetDefault();

    if(!ServerCommunication::initialize_communicator()) {
        ServerCommunication::deinitialize_communicator();
        login_button->Disable();
        remove_button->Disable();
        upload_button->Disable();
        download_button->Disable();
    }
    else {
        if (ServerCommunication::cred_login.empty() || ServerCommunication::cred_pass.empty()) {
            remove_button->Disable();
            upload_button->Disable();
            download_button->Disable();
        }
        else {
            if(ServerCommunication::send_login(ServerCommunication::cred_login, ServerCommunication::cred_pass)) {
                keep_active = boost::thread(&ServerCommunication::keep_alive);
                map_list_box->Set(get_array_data(ServerCommunication::get_map_list()));
                remove_button->Disable();
                download_button->Disable();
                login_button ->Disable();
            }
            else {
                remove_button->Disable();
                upload_button->Disable();
                download_button->Disable();
            }
        }
    }

    h_box3->Add(login_button, 1, wxLEFT | wxTOP, 10);
    h_box3->AddSpacer(200);
    h_box3->Add(close_button, 1, wxRIGHT | wxTOP, 10);
    h_box2->Add(panel2, 1, wxLEFT, 10);
    v_box2->Add(download_button, 1, wxBOTTOM, 5);
    v_box2->Add(remove_button, 1, wxBOTTOM, 5);
    v_box2->Add(upload_button, 1, wxBOTTOM, 5);
    v_box3->Add(panel1, 1, wxRIGHT, 10);
    h_box1->Add(v_box2, 0, wxLEFT, 10);
    h_box1->Add(v_box3, 0, wxRIGHT, 10);
    v_box1->Add(h_box1, 0, wxTOP, 10);
    v_box1->Add(h_box2, 0, wxALIGN_CENTER_VERTICAL, 10);
    v_box1->Add(h_box3, 0, wxBOTTOM, 10);
    
    SetSizer(v_box1);
    Centre();
}
const wxString ServerDialog::get_login() const
{
    if (!ServerCommunication::cred_login.empty())
        return wxString(ServerCommunication::cred_login.c_str(), wxConvUTF8);
    return wxString("", wxConvUTF8);
}

const wxString ServerDialog::get_pass() const
{
    if (!ServerCommunication::cred_pass.empty())
        return wxString(ServerCommunication::cred_pass.c_str(), wxConvUTF8);
    return wxString("", wxConvUTF8);
}

const wxArrayString ServerDialog::get_array_data(const VTankObject::MapList &map_list) const
{
    wxArrayString temp;
    for (VTankObject::MapList::size_type i = 0; i < map_list.size(); i++) {
        temp.Insert(wxString(map_list[i].c_str(), wxConvUTF8), i);
    }
    temp.Sort(false);
    return temp;
}

void ServerDialog::quit_properly(int reason)
{
    keep_active.interrupt();
    ServerCommunication::deinitialize_communicator();
    EndModal(reason);
}

void ServerDialog::on_select(wxCommandEvent& WXUNUSED(event))
{
    if (map_list_box->GetSelection() >= 0) {
        remove_button->Enable();
        download_button->Enable();
    }
}

//! Event handler for the cancel button.
/*!
 * Quit event hides the dialog so it can be destroyed.
 */
void ServerDialog::on_quit(wxCommandEvent& WXUNUSED(event))
{
    quit_properly(CANCELED);
}

//! Event handler for the remove button.
/*!
 * Deletes the selected map from the server.
 */
void ServerDialog::on_remove(wxCommandEvent& WXUNUSED(event))
{
    wxString file_name = map_list_box->GetString(static_cast<unsigned>(map_list_box->GetSelection()));
    std::string name = std::string(file_name.mb_str());
    if (ServerCommunication::remove_map(name)) {
        (void)wxMessageBox(wxT("The map was successfully removed."),
                           wxT("Removal completed successfully."), 
                           wxOK | wxICON_INFORMATION | wxSTAY_ON_TOP);
        map_list_box->Set(get_array_data(ServerCommunication::get_map_list()));
        if (map_list_box->GetSelection() < 0) {
            remove_button->Disable();
            download_button->Disable();
        }
    }
    else {
        quit_properly(FAILURE);
    }
}

void ServerDialog::on_download(wxCommandEvent& WXUNUSED(event))
{
    wxString file_name = map_list_box->GetString(static_cast<unsigned>(map_list_box->GetSelection()));
    std::string name = std::string(file_name.mb_str());
    Map vtmap = ServerCommunication::download_map(name);
    if(vtmap.get_width() != 0 && vtmap.get_height() != 0){
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
            wxString the_file = f_s_dialog.GetPath();
            if (vtmap.save(std::string(the_file.ToAscii()))) {
                (void)wxMessageBox(wxT("The map was successfully downloaded and saved."),
                    wxT("Download completed successfully."), 
                    wxOK | wxICON_INFORMATION | wxSTAY_ON_TOP);
            }
            else {  //failed to download
                wxBell();
                (void)wxMessageBox(wxT("Unable to download the map from the server."),
                                   wxT("Error!"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
                return;
            }
        }
    }
    else {
        quit_properly(FAILURE);
    }
}

void ServerDialog::on_upload(wxCommandEvent& WXUNUSED(event))
{
    try {
        wxString new_file;
        // Create an open file dialog...
        wxFileDialog map_file_dialog(NULL,
            wxT("Choose a map to upload."),
            wxT(""),
            wxT(""),
            wxT("VTank Map Files (*.vtmap)|*.vtmap|All Files (*.*)|*.*"),
            wxFD_OPEN | wxFD_FILE_MUST_EXIST);

        // ... and let the user interact with it.
        if (map_file_dialog.ShowModal() == wxID_OK) {
            new_file = map_file_dialog.GetPath();
        }
        if (new_file.IsNull()) {
            return;
        }

        Map map_to_send;
        std::string filestring(new_file.mb_str(wxConvUTF8));
        if (!map_to_send.load(filestring)) {
            wxBell();
            (void)wxMessageBox(
                wxT("Unable to load the map file."),
                wxT("Corrupted map file."), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
            return;
        }
        //Parse the Map title from file name
        filestring = filestring.substr((filestring.find_last_of('\\'))+1);
        filestring = filestring.substr((filestring.find_last_of('/'))+1);

        if (ServerCommunication::upload_map(&map_to_send, filestring)) {
            map_list_box->Set(get_array_data(ServerCommunication::get_map_list()));
            (void)wxMessageBox(wxT("The map was uploaded successfully."),
                               wxT("Upload completed successfully."), 
                               wxOK | wxICON_INFORMATION | wxSTAY_ON_TOP);
        }
        else {
            quit_properly(FAILURE);
        }
    }
    CATCH_LOGIC_ERRORS
}

void ServerDialog::on_login(wxCommandEvent& WXUNUSED(event))
{
    try {
        wxString user_txt = username_text_box->GetValue();
        wxString pass_txt = password_text_box->GetValue();
        user_txt.Trim(false); // Get rid of any leading white spaces.
        pass_txt.Trim(false);
        user_txt.Trim(true);  // Get rid of any trailing white spaces.
        pass_txt.Trim(true);

        // Check for a null string in the text boxes.
        if (user_txt.IsNull() || pass_txt.IsNull()) {
            wxBell();
            (void)wxMessageBox(wxT("You must enter a username and password."),
                               wxT("Login error."), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
            username_text_box->SetFocus();
            return;
        }
        // Maximum length username is 12 characters.
        if (user_txt.Len() > 12) {
            wxBell();
            (void)wxMessageBox(wxT("Username must be under 12 characters."),
                               wxT("Login error."), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
            username_text_box->SetFocus();
            return;
        }

        std::string user_login(std::string(user_txt.mb_str(wxConvUTF8)));
        std::string pass_login(std::string(pass_txt.mb_str(wxConvUTF8)));

        if (ServerCommunication::send_login(user_login, pass_login)) {
            keep_active = boost::thread(&ServerCommunication::keep_alive);
            upload_button->Enable();
            login_button->Disable();
            map_list_box->Set(get_array_data(ServerCommunication::get_map_list()));
        }
        else {  //failed to send
            username_text_box->SetFocus();
            return;
        }
    }
    CATCH_LOGIC_ERRORS
}
