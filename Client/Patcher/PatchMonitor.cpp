#include "PatchMonitor.h"
#include "PatchClient.h"
/*
PatchMonitor::PatchMonitor()
: done(false), message("Initializing...")
{
}

PatchMonitor::~PatchMonitor()
{
}

void PatchMonitor::update_message(const std::string &new_message)
{
    message = new_message;

    std::cout << message << std::endl;

    PatchClient::update_display_message(message);
}

void PatchMonitor::update_file(const std::string &new_file)
{
    current_file = new_file;
}

std::string PatchMonitor::display_message()
{
    return message;
}

void PatchMonitor::set_done(const bool patch_done)
{
    done = patch_done;
}

bool PatchMonitor::is_done()
{
    return done;
}

bool PatchMonitor::noFileSummary(const std::string& reason)
{
    // Returning true allows the patcher to continue.
    update_message("Performing full patch...");
    return true;
}

bool PatchMonitor::checksumStart()
{
    // Returning true allows a thorough patch.
    update_message("Calculating local file checksums...");
    return true;
}

bool PatchMonitor::checksumProgress(const std::string &path)
{
    std::ostringstream formatter;
    formatter << "Computing checksum for " << path << "...";
    update_message(formatter.str());

    return true;
}

bool PatchMonitor::checksumEnd()
{
    update_message("Checksum calculation done.");

    // Continue.
    return true;
}

bool PatchMonitor::fileListStart()
{
    update_message("Retrieving list of files from the server...");
    return true;
}

bool PatchMonitor::fileListProgress(Ice::Int percent)
{
    std::ostringstream formatter;
    formatter << "Retrieving list of files from the server... " << percent << "% complete.";
    update_message(formatter.str());

    return true;
}

bool PatchMonitor::fileListEnd()
{
    update_message("Retrieving list of files from the server... 100% complete.");
    return true;
}

bool PatchMonitor::patchStart(const std::string &path, 
                              Ice::Long size, Ice::Long updated, Ice::Long total)
{
    update_file(path);
    std::ostringstream formatter;

    progress = static_cast<long>((updated * 100) / total);
    if (progress >= 100) {
        progress = 99;
    }

    formatter << "(" << progress << "%) - Downloading " << path 
        << "... 0 / " << size;

    update_message(formatter.str());
    
    PatchClient::set_progress(static_cast<int>(progress));

    return true;
}

bool PatchMonitor::patchProgress(Ice::Long pos, Ice::Long size, Ice::Long updated, Ice::Long total)
{
    std::ostringstream formatter;
    progress = static_cast<long>((updated * 100) / total);
    if (progress >= 100) {
        progress = 99;
    }

    formatter << "(" << progress << "%) - Downloading " << current_file 
        << "... " << pos << " / " << size;

    update_message(formatter.str());

    PatchClient::set_progress(static_cast<int>(progress));

    return true;
}

bool PatchMonitor::patchEnd()
{
    std::ostringstream formatter;
    formatter << "(" << progress << "%) - Finished " << current_file << ".";
    update_message(formatter.str());
    return true;
}
*/