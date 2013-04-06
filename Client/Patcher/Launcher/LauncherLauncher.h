#ifndef LAUNCHERLAUNCHER_H
#define LAUNCHERLAUNCHER_H

#include "Launcher.h"
#include <boost/thread.hpp>

/** Implementing Launcher */
class LauncherLauncher : public Launcher
{
private:
    boost::thread refresh_thread;

protected:
	// Handlers for Launcher events.
    void Update( wxUpdateUIEvent& event );
	void Paint( wxPaintEvent& event );
	void Erase( wxEraseEvent& event );
	void ClosePatcher( wxCommandEvent& event );
    void Closed( wxCloseEvent& event );
	
public:
	/** Constructor */
	LauncherLauncher( wxWindow* parent );
    
    void kill_refresh_thread();
    bool update_progress();
};
#endif
