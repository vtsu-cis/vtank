
#ifndef PATCHMONITOR_H
#define PATCHMONITOR_H

#include <IcePatch2/ClientUtil.h>
#include <boost/thread.hpp>
#include <boost/thread/mutex.hpp>
#include <wx/window.h>

class PatchMonitor : public IcePatch2::PatcherFeedback
{
private:
    bool done;
    std::string message; // Message to display.
    std::string current_file; // File being patched.
    long progress; // Percentage progress.
    wxWindow *window;

    void set_done(const bool);

    void update_message(const std::string &);
    void update_file(const std::string &);
public:
    PatchMonitor(wxWindow *frame);
    virtual ~PatchMonitor();

    bool is_done();
    std::string display_message();

    virtual bool noFileSummary(const std::string& reason);

    virtual bool checksumStart();
    virtual bool checksumProgress(const std::string&);
    virtual bool checksumEnd();

    virtual bool fileListStart();
    virtual bool fileListProgress(Ice::Int);
    virtual bool fileListEnd();

    virtual bool patchStart(const std::string&, Ice::Long, Ice::Long, Ice::Long);
    virtual bool patchProgress(Ice::Long, Ice::Long, Ice::Long, Ice::Long);
    virtual bool patchEnd();
};

#endif
