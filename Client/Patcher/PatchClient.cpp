
#include "PatchClient.h"
#include <boost/thread.hpp>
//#include <Ice/Ice.h>
//#include <IcePatch2/ClientUtil.h>
#include "PatchMonitor.h"
#include <direct.h>

namespace PatchClient {
	/*
    Ice::CommunicatorPtr comm = NULL;
    IcePatch2::PatcherFeedbackPtr feedback = NULL;
    IcePatch2::PatcherPtr patcher = NULL;*/
    std::string display_message;
    /*boost::thread thread;
    boost::shared_mutex mutex;*/
    bool done = false;
    int progress = 0;

    void update_display_message(const std::string &message)
    {
        //boost::unique_lock<boost::shared_mutex> guard(mutex);
        display_message = message;
    }

    std::string get_display_message()
    {
        //boost::shared_lock<boost::shared_mutex> guard(mutex);
        return display_message;
    }

    bool is_done()
    {
        //boost::shared_lock<boost::shared_mutex> guard(mutex);
        return done;
    }

    void set_done(const bool done_value)
    {
        //boost::unique_lock<boost::shared_mutex> guard(mutex);
        done = done_value;
    }

    int get_progress()
    {
        //boost::shared_lock<boost::shared_mutex> guard(mutex);
        return progress;
    }

    void set_progress(const int new_progress)
    {
        //boost::unique_lock<boost::shared_mutex> guard(mutex);
        progress = new_progress;
    }

    bool initialize()
    {
        update_display_message("Initializing patcher...");

        return true;
    }

    void patch_thread()
    {
        /*try {
            Ice::StringSeq args;
            args.push_back("--Ice.Config=config.patcher");
            // Debugging:
            args.push_back("--Ice.StdOut=output.log");
            args.push_back("--Ice.StdErr=error.log");

            comm = Ice::initialize(args);

            const std::string dir = comm->getProperties()->getProperty("IcePatch2.Directory");
            if (dir.empty()) {
                throw "Cannot create directory: IcePatch2.Directory property is empty.";
            }
            _mkdir(dir.c_str());

            feedback = new PatchMonitor();
            
            std::cout << "Starting patcher..." << std::endl;
            patcher = new IcePatch2::Patcher(comm, feedback);
            std::cout << "Preparing IcePatch2..." << std::endl;
            if (!patcher->prepare()) {
                update_display_message("Patch was aborted.");

                return;
            }
            
            std::cout << "Patching IcePatch2..." << std::endl;
            if (!patcher->patch(".")) {
                update_display_message("Patch was aborted during the process.");

                return;
            }
            
            std::cout << "Finishing IcePatch2..." << std::endl;
            patcher->finish();

            update_display_message("Update complete.");
        }
        catch (const Ice::Exception &ex) {
            std::string message = "Patcher error: ";
            message += ex.what();
            update_display_message(message);
        }
        catch (const boost::thread_interrupted &) {
            update_display_message("Patcher was interrupted.");
        }
        catch (const std::string &error) {
            const std::string message = "Exception thrown from patcher: " + error;
            update_display_message(message);
        }
        catch (...) {
            update_display_message("Unhandled exception in the patcher.");
        }

		//system("copy /Y Client.exe.config client");

        set_progress(100);
        set_done(true);*/
    }

    bool start()
    {
        /*update_display_message("Connecting to patch server...");

        thread = boost::thread(patch_thread);*/

		update_display_message("Welcome to VTank. Click \"PLAY\" to begin.");
		set_progress(100);
		set_done(true);

        return true;
    }

    void deinitialize()
    {
        //thread.interrupt();

        /*try {
            if (comm != NULL) {
                comm->destroy();
            }
        }
        catch (const IceUtil::Exception &) {
            // This could happen.
        }*/
    }
};
