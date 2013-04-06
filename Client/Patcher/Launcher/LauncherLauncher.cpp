#include "LauncherLauncher.h"
#include "PatchPatcher.h"
#include <wx/msgdlg.h>
#include <wx/dcbuffer.h>
#include <wx/log.h>

//refresh screen with progress and new message
void perform_refresh(LauncherLauncher * patcher)
{
    try {
        bool running = true;
        while (running) {
            boost::this_thread::sleep(boost::posix_time::milliseconds(100));
            patcher->DoUpdateWindowUI(wxUpdateUIEvent());
            running = patcher->update_progress();
        }
    }
    catch (const boost::thread_interrupted &) {
        
    }

    /*system("start Patcher.exe");
    exit(0);*/
}

LauncherLauncher::LauncherLauncher( wxWindow* parent )
:
Launcher( parent )
{	
    this->SetDoubleBuffered(true);
    if (!PatchPatcher::initialize(this)) {
        (void)wxMessageBox(
            wxString("Initialization of the patcher (IcePatch2) failed. "
			"Please report this issue immediately.", wxConvUTF8),
            wxT("Initialization failed."), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        Close();
    }

    if (!PatchPatcher::start()) {
        (void)wxMessageBox(
            wxString("Could not spawn thread to start the patching process. "
			"Please report this issue immediately.", wxConvUTF8),
            wxT("Cannot start patcher."), wxOK | wxICON_ERROR | wxSTAY_ON_TOP);
        Close();
    }

    //refresh_thread = boost::thread(boost::bind<void>(perform_refresh, this));
}

void LauncherLauncher::kill_refresh_thread()
{
    refresh_thread.interrupt();
}

void LauncherLauncher::Closed( wxCloseEvent& event )
{
    Show(false);

    PatchPatcher::deinitialize();

    kill_refresh_thread();

    Destroy();
}

void LauncherLauncher::Update( wxUpdateUIEvent& event )
{
	update_progress();
}

void LauncherLauncher::Paint( wxPaintEvent& event )
{
	//wxBufferedPaintDC dc(this);
}

void LauncherLauncher::Erase( wxEraseEvent& event )
{

}

void LauncherLauncher::ClosePatcher( wxCommandEvent& event )
{
    Closed(wxCloseEvent());
}
//update the progress bar and message, truncate message with ... if too long
bool LauncherLauncher::update_progress()
{
    std::string mess = PatchPatcher::get_display_message();
    if(mess.size() > 84)
        mess = mess.substr(0, 81) + std::string("...");
    progress_label->SetLabel(wxString(mess.c_str(), wxConvUTF8));
    m_gauge2->SetValue(PatchPatcher::get_progress());
    if (PatchPatcher::is_done()) 
    {
        ClosePatcher(wxCommandEvent());
        return false;
    }

    return true;
}
