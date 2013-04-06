/*!
    \file   OpenMap.hpp
    \brief  Definition of a map open in gardener.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef OPENMAP_HPP
#define OPENMAP_HPP

struct OpenMap {
    Map  *data;           //!< The map object
    bool is_modified;     //!< =true if modified since last change.
    std::string filename; //!< The name of the file where the map is stored.
};

//! Sets whether there have been changes to the map
/*!
 * \param is_modified True if the map has had unsaved changes made to it, otherwise it is
 * false.
 */
static void set_editor_status(wxWindow *win, const OpenMap *const current_map)
{
    if(current_map->data != NULL) {
        std::string window_title = std::string(win->GetLabel().mb_str());
        if (current_map->is_modified) {
            if (window_title.substr((window_title.size() - 3), 3) != "***") {
                window_title.append("***");
            }
        }
        else {
            if (window_title.substr((window_title.size() - 3), 3) == "***") {
                window_title = window_title.substr(0, (window_title.size() - 3));
            }
        }
        win->SetLabel(wxString(window_title.c_str(), wxConvUTF8));
    }
}
#endif
