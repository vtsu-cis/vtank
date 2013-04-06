///////////////////////////////////////////////////////////////////////////
// C++ code generated with wxFormBuilder (version Apr 16 2008)
// http://www.wxformbuilder.org/
//
// PLEASE DO "NOT" EDIT THIS FILE!
///////////////////////////////////////////////////////////////////////////

#ifndef __patcher__
#define __patcher__

#include <wx/bitmap.h>
#include <wx/image.h>
#include <wx/icon.h>
#include <wx/bmpbuttn.h>
#include <wx/gdicmn.h>
#include <wx/font.h>
#include <wx/colour.h>
#include <wx/settings.h>
#include <wx/string.h>
#include <wx/button.h>
#include <wx/sizer.h>
#include <wx/stattext.h>
#include <wx/gauge.h>
#include <wx/frame.h>
#include <wx/statbmp.h>
#include <wx/hyperlink.h>
#include <wx/dialog.h>
#include <wx/textctrl.h>

///////////////////////////////////////////////////////////////////////////


///////////////////////////////////////////////////////////////////////////////
/// Class Patcher
///////////////////////////////////////////////////////////////////////////////
class Patcher : public wxFrame 
{
	private:
	
	protected:
		
		wxBitmapButton* play_button;
		wxBitmapButton* options_button;
		wxBitmapButton* about_button;
		wxBitmapButton* website_button;
		wxBitmapButton* exit_button;
		
		wxStaticText* m_staticText1;
		wxGauge* m_gauge2;
		wxTextCtrl* progress_label;
		
		// Virtual event handlers, overide them in your derived class
		virtual void Closed( wxCloseEvent& event ){ event.Skip(); }
		virtual void Erase( wxEraseEvent& event ){ event.Skip(); }
		virtual void Paint( wxPaintEvent& event ){ event.Skip(); }
		virtual void Update( wxUpdateUIEvent& event ){ event.Skip(); }
		virtual void LaunchGame( wxCommandEvent& event ){ event.Skip(); }
		virtual void OpenOptions( wxCommandEvent& event ){ event.Skip(); }
		virtual void OpenAbout( wxCommandEvent& event ){ event.Skip(); }
		virtual void OpenWebsite( wxCommandEvent& event ){ event.Skip(); }
		virtual void ClosePatcher( wxCommandEvent& event ){ event.Skip(); }
		
	
	public:
		Patcher( wxWindow* parent, wxWindowID id = wxID_ANY, const wxString& title = wxT("VTank Launcher"), const wxPoint& pos = wxDefaultPosition, const wxSize& size = wxSize( 640,480 ), long style = wxDEFAULT_FRAME_STYLE|wxCLIP_CHILDREN|wxTAB_TRAVERSAL );
		~Patcher();
	
};

///////////////////////////////////////////////////////////////////////////////
/// Class About
///////////////////////////////////////////////////////////////////////////////
class About : public wxDialog 
{
	private:
	
	protected:
		wxStaticBitmap* minilogo;
		wxStaticText* m_staticText3;
		wxHyperlinkCtrl* m_hyperlink1;
	
	public:
		About( wxWindow* parent, wxWindowID id = wxID_ANY, const wxString& title = wxT("About"), const wxPoint& pos = wxDefaultPosition, const wxSize& size = wxSize( 320,250 ), long style = wxDEFAULT_DIALOG_STYLE );
		~About();
	
};

#endif //__patcher__
