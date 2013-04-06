/*!
    \file server.cpp
    \brief Implementation of the Server blueprint.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <server.hpp>
#include <loginsessionfactory.hpp>
#include <mtgcallback.hpp>
#include <vtassert.hpp>
#include <asynctemplate.hpp>
#include <logger.hpp>
#include <mapmanager.hpp>
#include <playermanager.hpp>
#include <gamemanager.hpp>

namespace Server {
    // TODO: "Singleton"
    ServerService server;
    MTGService mtg_service;
    boost::threadpool::pool pool(1);

    ServerService::ServerService()
    {
    }

    ServerService::~ServerService()
    {
    }

    int ServerService::main(Ice::StringSeq seq)
    {
        comm = Ice::initialize(seq);
        if (!start()) {
            return 1;
        }

        communicator()->waitForShutdown();

        return 0;
    }

    bool ServerService::start()
    {
        try {
            // Create an object adapter which Echelon can connect to.
            //mtg_adapter = communicator()->createObjectAdapter("ClientSession");
            adapter = communicator()->createObjectAdapter("GameSessionFactory");

            // The callback object receives messages from the server.
            // Ice will provide garbage collection for the allocated memory.
            //adapter->add(new MTGCallback, communicator()->stringToIdentity("ClientSession"));

            communicator()->setDefaultRouter(NULL);

            // Convert the proxy to a one-way connection.
            // Note: I disabled this so that Theatre can detect bad connections.
            //mtg_proxy = MainToGameSession::MTGSessionPrx::uncheckedCast(mtg_proxy->ice_oneway());

            // Create an object adapter which clients can connect to.
            adapter->add(new LoginSessionFactory, communicator()->stringToIdentity("GameSessionFactory"));
            adapter->activate();

			MapManager::start();
			MapManager::rotate();
            Players::start_game();
            Players::task_pool.schedule(boost::threadpool::looped_task_func(&Players::manage_players, 1000));

            Logger::log(Logger::LOG_LEVEL_INFO, "The ServerService finished initializing.");
        }
        catch (const Exceptions::BadInformationException& ex) {
            std::ostringstream formatter;
            formatter << "Some information sent to the server is invalid: " << ex.reason;
            
            Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

            return false;
        }
        catch (const Exceptions::PermissionDeniedException& ex) {
            std::ostringstream formatter;
            formatter << "Permission denied from Echelon: " << ex.reason;

            Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

            return false;
        }
        catch (const Glacier2::CannotCreateSessionException& ex) {
            std::ostringstream formatter;
            formatter << "Glacier2 cannot create a session: " << ex.reason;

            Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

            return false;
        }
        catch (const Ice::ConnectionRefusedException& ex) {
            std::ostringstream formatter;
            formatter << "Unable to connect to Echelon: " << ex.what() << ": " << ex.error;

            Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

            return false;
        }
        catch (const Ice::Exception& ex) {
            std::ostringstream formatter;
            formatter << "Ice exception: " << ex.what();

            Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

            return false;
        }
        catch (const std::logic_error& ex) {
            std::ostringstream formatter;
            formatter << "Assertion error from start(): " << ex.what();

            Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

            return false;
        }
		catch (const std::exception &ex) {
			std::ostringstream formatter;
			formatter << "std::exception: " << ex.what();

            Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

            return false;
		}
        catch (...) {
            Logger::log(Logger::LOG_LEVEL_ERROR, "Uncaught exception thrown from start().");

            return false;
        }

        return true;
    }

    void ServerService::stop()
    {
        communicator()->shutdown();
        MapManager::shutdown();
    }
}
