#include "Launcher.h"


Launcher::Launcher( wxWindow* parent, wxWindowID id, const wxString& title, const wxPoint& pos, const wxSize& size, long style ) : wxFrame( parent, id, title, pos, size, style )
{
	this->SetSizeHints( wxDefaultSize, wxDefaultSize );
    wxBoxSizer *vbox1 = new wxBoxSizer(wxVERTICAL);

	m_staticText1 = new wxStaticText( this, wxID_ANY, wxT("Patch Progress"), wxDefaultPosition, wxDefaultSize, 0 );
    m_staticText1->SetForegroundColour(wxColor(255, 255, 255));
    m_staticText1->SetBackgroundColour(wxColor(0, 0, 0));
    vbox1->Add( m_staticText1, 0, wxALL, 5 );
	
	m_gauge2 = new wxGauge( this, wxID_ANY, 100, wxPoint( -1,-1 ), wxSize( 440,15 ), wxGA_HORIZONTAL );
	m_gauge2->SetValue( 0 ); 
	vbox1->Add( m_gauge2, 0, wxALIGN_CENTER, 0 );
	
	progress_label = new wxTextCtrl( this, wxID_ANY, wxT("Checking for updates..."), wxDefaultPosition, wxSize(440, 15), wxTE_READONLY | wxNO_BORDER);
    progress_label->SetForegroundColour(wxColor(255, 255, 255));
    progress_label->SetBackgroundColour(wxColor(0, 0, 0));
    vbox1->Add(progress_label, 0, wxALIGN_CENTER | wxTOP | wxBOTTOM, 5);

    exit_button = new wxButton(this, wxID_ANY, wxT("Cancel"), wxDefaultPosition, wxDefaultSize, 0 );
	vbox1->Add( exit_button, 0, wxALIGN_RIGHT | wxRIGHT, 5 );
	
    this->SetSizer(vbox1);
	this->Layout();

    SetIcon(wxICON(VTankLogo));
    Center();
	
	// Connect Events
	this->Connect( wxEVT_CLOSE_WINDOW, wxCloseEventHandler( Launcher::Closed ) );
	this->Connect( wxEVT_ERASE_BACKGROUND, wxEraseEventHandler( Launcher::Erase ) );
	this->Connect( wxEVT_PAINT, wxPaintEventHandler( Launcher::Paint ) );
	this->Connect( wxEVT_UPDATE_UI, wxUpdateUIEventHandler( Launcher::Update ) );
	exit_button->Connect( wxEVT_COMMAND_BUTTON_CLICKED, wxCommandEventHandler( Launcher::ClosePatcher ), NULL, this );
}

Launcher::~Launcher()
{
	// Disconnect Events
	this->Disconnect( wxEVT_CLOSE_WINDOW, wxCloseEventHandler( Launcher::Closed ) );
	this->Disconnect( wxEVT_ERASE_BACKGROUND, wxEraseEventHandler( Launcher::Erase ) );
	this->Disconnect( wxEVT_PAINT, wxPaintEventHandler( Launcher::Paint ) );
	this->Disconnect( wxEVT_UPDATE_UI, wxUpdateUIEventHandler( Launcher::Update ) );
	exit_button->Disconnect( wxEVT_COMMAND_BUTTON_CLICKED, wxCommandEventHandler( Launcher::ClosePatcher ), NULL, this );
}