
#ifndef PATCHCLIENT_H
#define PATCHCLIENT_H

#include <string>

namespace PatchClient {
    //! Initialize the patch client.
    bool initialize();

    //! Start the patch thread.
    bool start();

    //! Ask if the client is done.
    bool is_done();
    
    //! Set whether this client is done.
    void set_done(const bool);

    // Get how far along the patch process is.
    int get_progress();

    //! Set how far along the patch process is.
    void set_progress(const int);

    //! Set a new display message.
    void update_display_message(const std::string &);

    //! Get the message to display on the patcher.
    std::string get_display_message();

    //! Deinitialize the patch client.
    void deinitialize();
};

#endif
