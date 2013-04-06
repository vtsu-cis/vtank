/*!
    \file   AboutDialog.cpp
    \brief  Implementation for the map editor about dialog.
    \author (C) Copyright 2009 by Vermont Technical College

*/
#include "Library.hpp"
#include "AboutDialog.hpp"

const char * const address = "http://vtank.summerofsoftware.org/";
const char * const raw_license_string =
    "********************************** LICENSE ************************************\n"
    "This program is free software; you can redistribute it and/or modify it under the "
    "terms of the GNU General Public License as published by the Free Software Foundation; "
    "either version 2 of the License, or (at your option) any later version. This program "
    "is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without "
    "even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See "
    "the GNU General Public License for more details. You should have received a copy of "
    "the GNU General Public License along with this program; if not, write to:\n"
    "\n"
    "\tFree Software Foundation, Inc.\n"
    "\t59 Temple Place Suite 330\n"
    "\tBoston, MA 02111-1307\n"
    "\tUSA\n"
    "\n"
    "*******************************************************************************\n"
    "\n"
    "Please send comments and bug reports to:\n"
    "\n"
    "\tSummer of Software Engineering\n"
    "\tVermont Technical College\n"
    "\t201 Lawrence Place\n"
    "\tWilliston, VT 05495\n";

AboutDialog::AboutDialog()
       : wxDialog(NULL, -1, wxT("About- VTank Map Editor"), wxDefaultPosition, wxSize(500, 500)),
         noncopyable()
{
    wxBoxSizer *const v_box = new wxBoxSizer(wxVERTICAL);
    wxBoxSizer *const h_box = new wxBoxSizer(wxHORIZONTAL);

    new wxStaticBox(
        this,
        -1,
        wxT(""),
        wxPoint(5, 5),
        wxSize(499, 800));

    new wxStaticText(
        this,
        -1,
        wxT("Description: A Product of VTC's Summer of Software Engineering Project"),
        wxPoint(15, 15),
        wxDefaultSize,
        0,
        wxT(""));

    new wxStaticText(
        this,
        -1,
        wxT("Version: 0.9"),
        wxPoint(15, 35),
        wxDefaultSize,
        0,
        wxT("ID_STATICTEXT0"));

    new wxStaticText(
        this,
        -1,
        wxT("Copyright 2008 by Vermont Technical College"),
        wxPoint(15, 55),
        wxDefaultSize,
        0,
        wxT("ID_STATICTEXT1"));

    // Set up the panel containing the license and the link to the SoSE home page.
    wxPanel    *const panel = new wxPanel(this, -1);
    wxString    license_string(raw_license_string, wxConvUTF8);
    wxTextCtrl *const license =
        new wxTextCtrl(
            panel,
            -1,
            wxT(""),
            wxPoint(10,110),
            wxSize(480,360),
            wxTE_MULTILINE | wxTE_AUTO_SCROLL | wxTE_READONLY | wxTE_BESTWRAP | wxTE_LEFT | wxTE_RICH,
            wxDefaultValidator,
            wxT(""));
    static_cast<void>(license->SetDefaultStyle(wxTextAttr(wxColour(wxT("FOREST GREEN")))));
    license->AppendText(license_string);
    new wxHyperlinkCtrl(
        panel,
        -1,
        wxT("VTank Home Page"),
        wxString(address,wxConvUTF8),
        wxPoint(15, 75),
        wxDefaultSize,
        wxHL_DEFAULT_STYLE,
        wxT("hyperlink"));

    wxButton *const ok_button =
        new wxButton(this, wxID_OK, wxT("OK"), wxDefaultPosition, wxSize(70, 30));

    // Put together the overall dialog box.
    h_box->Add(ok_button, 0, wxCenter, 1);
    v_box->Add(panel, 0, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);
    v_box->Add(h_box, 0, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);

    SetSizerAndFit(v_box);
    Centre();
}


void AboutDialog::on_ok(wxCommandEvent &WXUNUSED(event))
{
    EndModal(wxID_OK);
}


//lint -e1924
BEGIN_EVENT_TABLE(AboutDialog, wxDialog)
    EVT_BUTTON(wxID_OK, AboutDialog::on_ok)
END_EVENT_TABLE()
//lint +e1924
