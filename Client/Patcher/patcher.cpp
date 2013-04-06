///////////////////////////////////////////////////////////////////////////
// C++ code generated with wxFormBuilder (version Apr 16 2008)
// http://www.wxformbuilder.org/
//
// PLEASE DO "NOT" EDIT THIS FILE!
///////////////////////////////////////////////////////////////////////////

#include "patcher.h"

///////////////////////////////////////////////////////////////////////////

Patcher::Patcher( wxWindow* parent, wxWindowID id, const wxString& title, const wxPoint& pos, const wxSize& size, long style ) : wxFrame( parent, id, title, pos, size, style )
{
	this->SetSizeHints( wxDefaultSize, wxDefaultSize );
	
	wxFlexGridSizer* fgSizer1;
	fgSizer1 = new wxFlexGridSizer( 2, 2, 0, 0 );
	fgSizer1->SetFlexibleDirection( wxBOTH );
	fgSizer1->SetNonFlexibleGrowMode( wxFLEX_GROWMODE_SPECIFIED );
	
	
	fgSizer1->Add( 0, 0, 1, wxEXPAND, 5 );
	
	wxBoxSizer* bSizer1;
	bSizer1 = new wxBoxSizer( wxVERTICAL );
	
	bSizer1->SetMinSize( wxSize( 160,-1 ) ); 
	play_button = new wxBitmapButton( this, wxID_ANY, wxBitmap( wxT("assets/buttons/play.png"), wxBITMAP_TYPE_ANY ), wxDefaultPosition, wxSize( 160,75 ), 0 );
	
	play_button->SetBitmapDisabled( wxBitmap( wxT("assets/buttons/play_disabled.png"), wxBITMAP_TYPE_ANY ) );
	play_button->Enable( false );
	
	play_button->Enable( false );
	
	bSizer1->Add( play_button, 0, wxALL, 5 );
	
	options_button = new wxBitmapButton( this, wxID_ANY, wxBitmap( wxT("assets/buttons/options.png"), wxBITMAP_TYPE_ANY ), wxDefaultPosition, wxSize( 160,75 ), wxBU_AUTODRAW );
	
	options_button->SetBitmapDisabled( wxBitmap( wxT("assets/buttons/options_disabled.png"), wxBITMAP_TYPE_ANY ) );
	options_button->Enable( false );
	
	options_button->Enable( false );
	
	bSizer1->Add( options_button, 0, wxALL, 5 );
	
	about_button = new wxBitmapButton( this, wxID_ANY, wxBitmap( wxT("assets/buttons/about.png"), wxBITMAP_TYPE_ANY ), wxDefaultPosition, wxSize( 160,75 ), wxBU_AUTODRAW );
	bSizer1->Add( about_button, 0, wxALL, 5 );
	
	website_button = new wxBitmapButton( this, wxID_ANY, wxBitmap( wxT("assets/buttons/website.png"), wxBITMAP_TYPE_ANY ), wxDefaultPosition, wxSize( 160,75 ), wxBU_AUTODRAW );
	bSizer1->Add( website_button, 0, wxALL, 5 );
	
	exit_button = new wxBitmapButton( this, wxID_ANY, wxBitmap( wxT("assets/buttons/exit.png"), wxBITMAP_TYPE_ANY ), wxDefaultPosition, wxSize( 160,75 ), wxBU_AUTODRAW );
	bSizer1->Add( exit_button, 0, wxALL, 5 );
	
	fgSizer1->Add( bSizer1, 1, wxALL, 5 );
	
	wxBoxSizer* bSizer2;
	bSizer2 = new wxBoxSizer( wxVERTICAL );
	
	
	bSizer2->Add( 0, 30, 1, wxEXPAND, 5 );
	
	m_staticText1 = new wxStaticText( this, wxID_ANY, wxT(""), wxDefaultPosition, wxDefaultSize, 0 );
	m_staticText1->Wrap( -1 );
	m_staticText1->SetForegroundColour( wxColour( 255, 255, 255 ) );
	m_staticText1->SetBackgroundColour( wxColour( 41, 43, 49 ) );
	
	bSizer2->Add( m_staticText1, 0, wxALL, 5 );
	
	m_gauge2 = new wxGauge( this, wxID_ANY, 100, wxPoint( 900,-1 ), wxSize( 440,15 ), wxGA_HORIZONTAL );
	m_gauge2->SetValue( 0 ); 
	m_gauge2->Hide();
	bSizer2->Add( m_gauge2, 0, wxALIGN_CENTER|wxALL, 5 );
	bSizer2->AddSpacer(450);
	
	progress_label = new wxTextCtrl( this, wxID_ANY, wxT("Checking for updates..."), wxDefaultPosition, wxSize(440, 15), wxTE_READONLY | wxNO_BORDER);
	progress_label->SetForegroundColour( wxColour( 255, 255, 255 ) );
	progress_label->SetBackgroundColour( wxColour( 27, 28, 32 ) );
	progress_label->Hide();
	
	bSizer2->Add( progress_label, 0, wxALL, 5 );
	
	fgSizer1->Add( bSizer2, 1, wxALIGN_BOTTOM, 5 );
	
	this->SetSizer( fgSizer1 );
	this->Layout();
	
	// Connect Events
	this->Connect( wxEVT_CLOSE_WINDOW, wxCloseEventHandler( Patcher::Closed ) );
	this->Connect( wxEVT_ERASE_BACKGROUND, wxEraseEventHandler( Patcher::Erase ) );
	this->Connect( wxEVT_PAINT, wxPaintEventHandler( Patcher::Paint ) );
	this->Connect( wxEVT_UPDATE_UI, wxUpdateUIEventHandler( Patcher::Update ) );
	play_button->Connect( wxEVT_COMMAND_BUTTON_CLICKED, wxCommandEventHandler( Patcher::LaunchGame ), NULL, this );
	options_button->Connect( wxEVT_COMMAND_BUTTON_CLICKED, wxCommandEventHandler( Patcher::OpenOptions ), NULL, this );
	about_button->Connect( wxEVT_COMMAND_BUTTON_CLICKED, wxCommandEventHandler( Patcher::OpenAbout ), NULL, this );
	website_button->Connect( wxEVT_COMMAND_BUTTON_CLICKED, wxCommandEventHandler( Patcher::OpenWebsite ), NULL, this );
	exit_button->Connect( wxEVT_COMMAND_BUTTON_CLICKED, wxCommandEventHandler( Patcher::ClosePatcher ), NULL, this );
}

Patcher::~Patcher()
{
	// Disconnect Events
	this->Disconnect( wxEVT_CLOSE_WINDOW, wxCloseEventHandler( Patcher::Closed ) );
	this->Disconnect( wxEVT_ERASE_BACKGROUND, wxEraseEventHandler( Patcher::Erase ) );
	this->Disconnect( wxEVT_PAINT, wxPaintEventHandler( Patcher::Paint ) );
	this->Disconnect( wxEVT_UPDATE_UI, wxUpdateUIEventHandler( Patcher::Update ) );
	play_button->Disconnect( wxEVT_COMMAND_BUTTON_CLICKED, wxCommandEventHandler( Patcher::LaunchGame ), NULL, this );
	options_button->Disconnect( wxEVT_COMMAND_BUTTON_CLICKED, wxCommandEventHandler( Patcher::OpenOptions ), NULL, this );
	about_button->Disconnect( wxEVT_COMMAND_BUTTON_CLICKED, wxCommandEventHandler( Patcher::OpenAbout ), NULL, this );
	website_button->Disconnect( wxEVT_COMMAND_BUTTON_CLICKED, wxCommandEventHandler( Patcher::OpenWebsite ), NULL, this );
	exit_button->Disconnect( wxEVT_COMMAND_BUTTON_CLICKED, wxCommandEventHandler( Patcher::ClosePatcher ), NULL, this );
}

About::About( wxWindow* parent, wxWindowID id, const wxString& title, const wxPoint& pos, const wxSize& size, long style ) : wxDialog( parent, id, title, pos, size, style )
{
	this->SetSizeHints( wxDefaultSize, wxDefaultSize );
	
	wxBoxSizer* bSizer3;
	bSizer3 = new wxBoxSizer( wxVERTICAL );
	
	minilogo = new wxStaticBitmap( this, wxID_ANY, wxBitmap( wxT("assets/logo/vtanklogogreen_about.png"), wxBITMAP_TYPE_ANY ), wxDefaultPosition, wxSize( 200,145 ), 0 );
	bSizer3->Add( minilogo, 0, wxALIGN_CENTER|wxALL, 5 );
	
	m_staticText3 = new wxStaticText( this, wxID_ANY, wxT("VTank Patcher\nCreated by Summer of Software Engineering"), wxPoint( -1,-1 ), wxDefaultSize, 0 );
	m_staticText3->Wrap( -1 );
	bSizer3->Add( m_staticText3, 0, wxALL, 5 );
	
	m_hyperlink1 = new wxHyperlinkCtrl( this, wxID_ANY, wxT("VTank Website"), wxT("http://vtank.cis.vtc.edu"), wxDefaultPosition, wxDefaultSize, wxHL_DEFAULT_STYLE );
	bSizer3->Add( m_hyperlink1, 0, wxALL, 5 );
	
	this->SetSizer( bSizer3 );
	this->Layout();
}

About::~About()
{
}
