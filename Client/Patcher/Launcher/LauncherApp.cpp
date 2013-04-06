#include "LauncherApp.h"
#include "LauncherLauncher.h"

IMPLEMENT_APP(LauncherApp);

bool LauncherApp::OnInit()
{
    LauncherLauncher * frame = NULL;
    try {
        wxInitAllImageHandlers();
        
        frame = new LauncherLauncher(NULL);
        frame->Show();
    }
    catch (...) {
        if (frame != NULL) {
            delete frame;
        }
        return false;
    }

    return true;
}
