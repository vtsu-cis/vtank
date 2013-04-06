#ifndef __PatcherPatcher__
#define __PatcherPatcher__

/**
@file
Subclass of Patcher, which is generated by wxFormBuilder.
*/

#include "patcher.h"
#include <boost/thread.hpp>

/** Implementing Patcher */
class PatcherPatcher : public Patcher
{
private:
    boost::thread refresh_thread;
	wxBitmap background;

protected:
	// Handlers for Patcher events.
    void Update( wxUpdateUIEvent& event );
	void Paint( wxPaintEvent& event );
	void Erase( wxEraseEvent& event );
	void LaunchGame( wxCommandEvent& event );
	void OpenOptions( wxCommandEvent& event );
	void OpenAbout( wxCommandEvent& event );
	void OpenWebsite( wxCommandEvent& event );
	void ClosePatcher( wxCommandEvent& event );
    void Closed( wxCloseEvent& event );
	
public:
	/** Constructor */
	PatcherPatcher( wxWindow* parent );

    void update_progress();
};
#endif // __PatcherPatcher__

