#include "PatcherPatcher.h"
#include "PatcherAbout.h"
#include "PatchClient.h"
#include <wx/msgdlg.h>
#include <wx/dcbuffer.h>
#include <wx/log.h>
#include <fstream>

//! Revision header.
#define REVISION_STRING " - Release "
#define VERSION_FILE "version.ini"

static std::string get_version()
{
	std::fstream file(VERSION_FILE, std::ios_base::in);
	if (!file.is_open()) {
		// Can't read the file. Return default.
		return "N/A";
	}
	
	std::string header;
	std::string version_line;
	std::string version;

	std::getline(file, header);
	std::getline(file, version_line);

	file.close();
	
	version = version_line.substr(version_line.find("=") + 1);
	return version;
}

void perform_refresh(PatcherPatcher * patcher)
{
    /*try {
        while (true) {
            boost::this_thread::sleep(boost::posix_time::milliseconds(100));
            patcher->DoUpdateWindowUI(wxUpdateUIEvent());
            patcher->update_progress();
        }
    }
    catch (const boost::thread_interrupted &) {

    }*/
}

PatcherPatcher::PatcherPatcher( wxWindow* parent )
:
Patcher( parent )
{	
    this->SetDoubleBuffered(true);

    if (!PatchClient::initialize()) {
        (void)wxMessageBox(
            wxString("Initialization of the patcher (IcePatch2) failed. "
			"Please report this issue immediately.", wxConvUTF8),
            wxT("Initialization failed."), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        Close();
    }

    if (!PatchClient::start()) {
        (void)wxMessageBox(
            wxString("Could not spawn thread to start the patching process. "
			"Please report this issue immediately.", wxConvUTF8),
            wxT("Cannot start patcher."), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        Close();
    }

	//refresh_thread = boost::thread(boost::bind<void>(perform_refresh, this));

	background = wxBitmap(wxT("assets/logo/background.png"), wxBITMAP_TYPE_ANY);
	
	wxString title_suffix;
	const std::string version = get_version();
	title_suffix = wxT(REVISION_STRING) + wxString(version.c_str(), wxConvUTF8);

    SetTitle(GetTitle() + title_suffix);
}

void PatcherPatcher::Closed( wxCloseEvent& event )
{
    //refresh_thread.interrupt();

    PatchClient::deinitialize();

    Destroy();
}

void PatcherPatcher::Update( wxUpdateUIEvent& event )
{
	update_progress();
}

void PatcherPatcher::Paint( wxPaintEvent& event )
{
	wxBufferedPaintDC dc(this);
	dc.DrawBitmap(background, 0, 0, false);
}

void PatcherPatcher::Erase( wxEraseEvent& event )
{

}

void PatcherPatcher::LaunchGame( wxCommandEvent& event )
{
	const int return_code = system("start Client.exe");
    if (return_code != EXIT_SUCCESS) {
		// Do nothing in this case. If the file doesn't exist, the 'start' process will complain.
        (void)wxMessageBox(
            wxString("Unable to execute the file \"Client.exe\".\n"
            "Confirm that the file exists and try again.", wxConvUTF8),
            wxT("Couldn't run Client.exe."), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
    }
    else {
		(void)Hide();
		//boost::this_thread::sleep(boost::posix_time::milliseconds(1000));
        Close();
    }
}

void PatcherPatcher::OpenOptions( wxCommandEvent& event )
{
	const int exit_code = system("options");
	if (exit_code != 0) {
        // The 'start' process will complain if 'options' can't be launched.
		/*(void)wxMessageBox(wxString("Problem running the file \"options.exe\".\n"
            "Confirm that the file exists and try again.\n"
			"If it crashed, please report the issue immediately.", wxConvUTF8),
            wxT("Error running options.exe."), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);*/
	}
}

void PatcherPatcher::OpenAbout( wxCommandEvent& event )
{
	PatcherAbout about(this);
    (void)about.ShowModal();
}

void PatcherPatcher::OpenWebsite( wxCommandEvent &event )
{
	wxString m_url(wxT("http://vtank.cis.vtc.edu"));
	wxHyperlinkEvent linkEvent(this, GetId(), m_url);
    if (!GetEventHandler()->ProcessEvent(linkEvent))     // was the event skipped ?
        if (!wxLaunchDefaultBrowser(m_url))
            wxLogWarning(wxT("Could not launch the default browser with url '%s' !"), m_url.c_str());
}

void PatcherPatcher::ClosePatcher( wxCommandEvent& event )
{
	//refresh_thread.interrupt();
	Close();
}

void PatcherPatcher::update_progress()
{
    progress_label->SetLabel(wxString(PatchClient::get_display_message().c_str(), wxConvUTF8));
    m_gauge2->SetValue(PatchClient::get_progress());
    if (!play_button->IsEnabled() && PatchClient::is_done()) {
		play_button->Enable();
		options_button->Enable();

		//refresh_thread.interrupt();
    }
}