/*!
    \file   MapEditorApp.cpp
    \brief  Map Editor main program, wxWidgets style.
    \author (C) Copyright 2009 by Vermont Technical College

LICENSE

This program is free software; you can redistribute it and/or modify it under the terms of the
GNU General Public License as published by the Free Software Foundation; either version 2 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See
the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program; if
not, write to the Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA
02111-1307 USA
*/

#include "Library.hpp"
#include "config.hpp"
#include "MapEditorApp.hpp"
#include "MapEditorFrame.hpp"

#if TARGET == LINTARGET
// Icon data in XPM form.
#include "data/resources/Gardener.xpm"
#endif

//lint -save -e534 -e1717 -e1774 -e1924 -e1929
IMPLEMENT_APP(MapEditorApp);
//lint -restore

bool MapEditorApp::OnInit()
{
    // Set up the configuration information before doing tests in case some configuration items
    // are related to testing. Note that if the Map Editor ever takes the name of its
    // configuration file from the command line, this will have to be reorganized.
    //
    Support::register_parameter("RESOURCE_ROOT", ".");
    Support::read_config_files("Gardener.cfg");

    MapEditorFrame *frame = NULL;
    try {
        // Process the command line here if necessary.
        wxInitAllImageHandlers();
        frame = new MapEditorFrame(0L, wxT("VTank Map Editor"));
        #if TARGET == WINTARGET
        frame->SetIcon(wxICON(gardener));
        #elif TARGET == LINTARGET
        frame->SetIcon(wxIcon(Gardener));
        #endif
        frame->Show();
    }
    catch (...) {
        delete frame;
        return false;
    }

    return true;
}
