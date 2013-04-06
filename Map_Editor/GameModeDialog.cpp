/*!
    \file   GameModeDialog.cpp
    \brief  Implementation of the game mode dialog box.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#include "Library.hpp"
#include "GameModeDialog.hpp"
#include "vtassert.hpp"

//lint -e1924
BEGIN_EVENT_TABLE(GameModeDialog, wxDialog)
    EVT_BUTTON(wxID_CANCEL,        GameModeDialog::on_quit    )
    EVT_BUTTON(wxID_SAVE,          GameModeDialog::on_save    )
    EVT_BUTTON(wxID_PROPERTIES,    GameModeDialog::on_validate)
    EVT_LISTBOX(wxID_DEFAULT,      GameModeDialog::on_select  )
END_EVENT_TABLE()
//lint +e1924

//! Constructor that creates a new maplist dialog.
/*!
 *  \param map_list A sequence of strings representing the name of maps that is
 *  currently stored on the server.
 */
GameModeDialog::GameModeDialog(Map *c_map)
       : wxDialog(NULL, -1, wxT("Game Modes"), wxDefaultPosition, wxSize(400,255)),
         noncopyable        (),
         current_map        (c_map),
         save_button        (NULL),
         validate_button    (NULL),
         description        (NULL),
         game_mode_box      (NULL)
{
    wxPanel    *const panel1 = new wxPanel(this, -1);
    wxPanel    *const panel2 = new wxPanel(this, -1);
    wxBoxSizer *const v_box1 = new wxBoxSizer(wxVERTICAL);
    wxBoxSizer *const v_box2 = new wxBoxSizer(wxVERTICAL);
    wxBoxSizer *const v_box3 = new wxBoxSizer(wxVERTICAL);
    wxBoxSizer *const h_box1 = new wxBoxSizer(wxHORIZONTAL);
    wxBoxSizer *const h_box2 = new wxBoxSizer(wxHORIZONTAL);

    new wxStaticBox(panel1,
        -1,
        _("Game Modes"),
        wxPoint(0,10),
        wxSize(200, 150));

    game_mode_box =
        new wxCheckListBox(panel1,
        wxID_DEFAULT,
        wxPoint(10, 30),
        wxSize(180, 120),
        get_array_data(),
        wxLB_NEEDED_SB,
        wxDefaultValidator,
        wxListBoxNameStr);

    description =
        new wxStaticText(panel2,
        wxNewId(),
        _(""),
        wxPoint(10, 10),
        wxSize(170, 170),
        wxALIGN_LEFT,
        wxT(""));

    wxButton *const close_button =
        new wxButton(this,
            wxID_CANCEL,
            _("Cancel"),
            wxDefaultPosition,
            wxSize(70, 30));

    save_button =
        new wxButton(this,
            wxID_SAVE,
            _("Save"),
            wxDefaultPosition,
            wxSize(70, 30));

    validate_button =
        new wxButton(this,
            wxID_PROPERTIES,
            _("Validate Map"),
            wxDefaultPosition,
            wxSize(70, 30));

    h_box1->Add(save_button, 1, wxLEFT | wxTOP, 10);
    h_box1->Add(validate_button, 1, wxLEFT | wxTOP, 10);
    h_box1->AddSpacer(155);
    h_box1->Add(close_button, 1, wxRIGHT | wxTOP, 10);
    v_box1->Add(panel1, 1, wxLEFT, 10);
    v_box2->Add(panel2, 1, wxRIGHT, 10);
    h_box2->Add(v_box1, 0, wxLEFT, 0);
    h_box2->Add(v_box2, 0, wxRIGHT, 0);
    v_box3->Add(h_box2, 0, wxTOP, 0);
    v_box3->Add(h_box1, 0, wxBOTTOM, 0);
    SetSizer(v_box3);
    Centre();
    populate_list();
}

void GameModeDialog::populate_list()
{
    std::vector<int> game_modes = current_map->get_supported_game_modes();
    for (std::vector<int>::iterator i = game_modes.begin(); i != game_modes.end(); i++) {
        VTANK_ASSERT(*i >= 0);
        game_mode_box->Check(static_cast<unsigned int>(*i), true);
    }
}
const wxArrayString GameModeDialog::get_array_data() const
{
    wxArrayString temp;
    temp.Add(wxString("Death Match", wxConvUTF8));
    temp.Add(wxString("Team Death Match", wxConvUTF8));
    temp.Add(wxString("Capture the Flag", wxConvUTF8));
    temp.Add(wxString("Capture the Base", wxConvUTF8));
    return temp;
}


void GameModeDialog::on_select(wxCommandEvent& WXUNUSED(event))
{
    if (game_mode_box->GetSelection() >= 0) {
        wxString game = game_mode_box->GetString(static_cast<unsigned>(game_mode_box->GetSelection()));
       if(game == wxString("Death Match", wxConvUTF8)) {
            description->SetLabel(wxString("Deathmatch is a straight\n"
                                            "free-for-all kill-fest between every\n"
                                            "tank and every other tank. The\n"
                                            "player spawns at spawn points\n"
                                            "defined in the event layer.", wxConvUTF8));
        }
       else if(game == wxString("Team Death Match", wxConvUTF8)) {
           description->SetLabel(wxString("Team Deathmatch is a team\n"
                                          "free-for-all kill-fest between two\n"
                                          "or more teams. The objective is\n"
                                          "to have more kills than the other\n"
                                          "teams. Players spawn together in\n"
                                          "a team spawn area defined in the\n"
                                          "event layer.", wxConvUTF8));
       }
       else if(game == wxString("Capture the Flag", wxConvUTF8)) {
           description->SetLabel(wxString("Capture the Flag pits two\n"
                                           "teams against each other with\n"
                                           "the objective of capturing the\n"
                                           "other player's flag and returning\n"
                                           "it to their own base. If a player\n"
                                           "is killed while holding the flag,\n"
                                           "the flag is dropped on the ground.\n"
                                           "The team who owns the flag can\n"
                                           "run over it to return it to the\n"
                                           "base instantly, while the team who\n"
                                           "is trying to capture the flag can\n"
                                           "run over it to pick it up and\n"
                                           "continue running it home.", wxConvUTF8));
       }
       else if(game == wxString("Capture the Base", wxConvUTF8)) {
           description->SetLabel(wxString("Capture the Base is a team mode\n"
                                          "which has teams pushing towards\n"
                                          "the other in order to capture more\n"
                                          "bases. When one team captures all 6\n"
                                          "bases, that team wins a point.\n"
                                          "The team with the highest amount of\n"
                                          "points at the end of the round wins\n"
                                          "the overall game.", wxConvUTF8));
       }
       Refresh();
    }
}

//! Event handler for the cancel button.
/*!
 * Quit event hides the dialog so it can be destroyed.
 */
void GameModeDialog::on_quit(wxCommandEvent& WXUNUSED(event))
{
      EndModal(CANCELED);
}

//! Event handler for the remove button.
/*!
 * Deletes the selected map from the server.
 */
//TODO: Error messages more clear
void GameModeDialog::on_save(wxCommandEvent& WXUNUSED(event))
{
    for (unsigned int x = 0; x < game_mode_box->GetCount(); x++) {
        wxString game = game_mode_box->GetString(x);
        if (game == wxString("Death Match", wxConvUTF8)) {
            if(game_mode_box->IsChecked(x)) {
                if(current_map->validate_death_match()) {
                    current_map->add_supported_game_mode(DEATH_MATCH);
                }
                else {
                    current_map->remove_supported_game_mode(DEATH_MATCH);
                    (void)wxMessageBox(wxString("Map won't support Death Match in its current form,\n \n"
                        "At least one spawn point is needed", wxConvUTF8),
                               wxT("Supported Game Mode Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
                }
            }
            else {
                current_map->remove_supported_game_mode(DEATH_MATCH);
            }
        }
        else if(game == wxString("Team Death Match", wxConvUTF8)) {
            if(game_mode_box->IsChecked(x)) {
                if(current_map->validate_team_death_match()) {
                    current_map->add_supported_game_mode(TEAM_DEATH_MATCH);
                }
                else {
                    current_map->remove_supported_game_mode(TEAM_DEATH_MATCH);
                    (void)wxMessageBox(wxString("Map won't support Team Death Match in its current form,\n \n"
                        "At least one Red spawn area is needed\n"
                        "At least one Blue spawn area is needed", wxConvUTF8),
                               wxT("Supported Game Mode Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
                }
            }
            else {
                current_map->remove_supported_game_mode(TEAM_DEATH_MATCH);
            }
        }
        else if(game == wxString("Capture the Flag", wxConvUTF8)) {
            if(game_mode_box->IsChecked(x)) {
                if(current_map->validate_capture_the_flag()) {
                    current_map->add_supported_game_mode(CAPTURE_THE_FLAG);
                }
                else {
                    current_map->remove_supported_game_mode(CAPTURE_THE_FLAG);
                    (void)wxMessageBox(wxString("Map won't support Capture the Flag in its current form,\n \n"
                        "At least one Red spawn area is needed\n"
                        "At least one Blue spawn area is needed\n"
                        "A Blue flag is needed\n"
                        "A Red flag is needed", wxConvUTF8),
                               wxT("Supported Game Mode Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
                }
            }
            else {
                current_map->remove_supported_game_mode(CAPTURE_THE_FLAG);
            }
        }
        else if(game == wxString("Capture the Base", wxConvUTF8)) {
            if(game_mode_box->IsChecked(x)) {
                if(current_map->validate_capture_the_base()) {
                    current_map->add_supported_game_mode(CAPTURE_THE_BASE);
                }
                else {
                    current_map->remove_supported_game_mode(CAPTURE_THE_BASE);
					(void)wxMessageBox(wxString("Map won't support Capture the Base in its current form:\n"
                        "One and only one spawn point must exist for\n"
                        "each of the six bases.", wxConvUTF8),
                               wxT("Supported Game Mode Error"), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
                }
            }
            else {
                current_map->remove_supported_game_mode(CAPTURE_THE_BASE);
            }
        }
    }
    EndModal(APPLIED);
}

void GameModeDialog::on_validate(wxCommandEvent& WXUNUSED(event))
{
    for(unsigned int x = 0; x < game_mode_box->GetCount();x++) {
        wxString game = game_mode_box->GetString(x);
        if(game == wxString("Death Match", wxConvUTF8)) {
           if(current_map->validate_death_match()) {
                game_mode_box->Check(x,true);
            }
            else {
                game_mode_box->Check(x,false);
            }
        }
        else if(game == wxString("Team Death Match", wxConvUTF8)) {
            if(current_map->validate_team_death_match()) {
                game_mode_box->Check(x,true);
            }
            else {
                game_mode_box->Check(x,false);
            }
        }
        else if(game == wxString("Capture the Flag", wxConvUTF8)) {
            if(current_map->validate_capture_the_flag()) {
                game_mode_box->Check(x,true);
            }
            else {
                game_mode_box->Check(x,false);
            }
        }
        else if(game == wxString("Capture the Base", wxConvUTF8)) {
            if(current_map->validate_capture_the_base()) {
                game_mode_box->Check(x,true);
            }
            else {
                game_mode_box->Check(x,false);
            }
        }
    }
}
