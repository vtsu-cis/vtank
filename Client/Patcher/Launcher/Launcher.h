#ifndef LAUNCHER_H
#define LAUNCHER_H

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

class Launcher : public wxFrame 
{
	private:
	
	protected:
		
		wxButton* exit_button;
        wxStaticText* m_staticText1;
		wxGauge* m_gauge2;
		wxTextCtrl* progress_label;
		
		// Virtual event handlers, overide them in your derived class
		virtual void Closed( wxCloseEvent& event ){ event.Skip(); }
		virtual void Erase( wxEraseEvent& event ){ event.Skip(); }
		virtual void Paint( wxPaintEvent& event ){ event.Skip(); }
		virtual void Update( wxUpdateUIEvent& event ){ event.Skip(); }
		virtual void ClosePatcher( wxCommandEvent& event ){ event.Skip(); }
		
	
	public:
		Launcher( wxWindow* parent, wxWindowID id = wxID_ANY, const wxString& title = wxT("Patching ..."), const wxPoint& pos = wxDefaultPosition, const wxSize& size = wxSize( 500,130 ), long style = wxDEFAULT_FRAME_STYLE|wxCLIP_CHILDREN|wxTAB_TRAVERSAL );
		~Launcher();
	
};
#endif
