
#ifndef PATCHPATCHER_H
#define PATCHPATCHER_H

#include <string>
#include <wx/window.h>

namespace PatchPatcher {
    //! Initialize the patch patcher.
    bool initialize(wxWindow *window);

    //! Start the patch thread.
    bool start();

    //! Ask if the patcher is done.
    bool is_done();
    
    //! Set whether this patcher is done.
    void set_done(const bool);

    // Get how far along the patch process is.
    int get_progress();

    //! Set how far along the patch process is.
    void set_progress(const int);

    //! Set a new display message.
    void update_display_message(const std::string &);

    //! Get the message to display on the launcher.
    std::string get_display_message();

    //! Deinitialize the patch patcher.
    void deinitialize();
};

#endif
