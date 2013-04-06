#include "PatcherApp.h"
#include "PatcherPatcher.h"

IMPLEMENT_APP(PatcherApp);

bool PatcherApp::OnInit()
{
    PatcherPatcher * frame = NULL;
    try {
        wxInitAllImageHandlers();

        frame = new PatcherPatcher(NULL);
        frame->SetIcon(wxICON(patcher));
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
