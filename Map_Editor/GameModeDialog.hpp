/*!
    \file   GameModeDialog.hpp
    \brief  Interface to the game mode dialog box.
    \author (C) Copyright 2009 by Vermont Technical College

*/
#include "Map.hpp"

#ifndef GAMEMODEDIALOG_HPP
#define GAMEMODEDIALOG_HPP

class GameModeDialog : public wxDialog, private boost::noncopyable {
    //lint -save -e1516
    DECLARE_EVENT_TABLE()
    //lint -restore

public:
    enum { CANCELED,
           APPLIED};

    explicit GameModeDialog(Map *c_map);
    void populate_list();
    const wxArrayString get_array_data() const;
    
    // Event handlers.
    void on_select     (wxCommandEvent &event);
    void on_quit       (wxCommandEvent &event);
    void on_validate   (wxCommandEvent &event);
    void on_save       (wxCommandEvent &event);

private:
    Map             *current_map;

    wxButton        *save_button;
    wxButton        *validate_button;
    wxStaticText    *description;
    wxCheckListBox  *game_mode_box;
};

#endif
