/*!
    \file mtgservice.cpp
    \brief Implementation of the service which initializes Ice (for the MainToGame session).
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <server.hpp>
#include <mtgservice.hpp>
#include <logger.hpp>
#include <mtgcallback.hpp>
#include <playermanager.hpp>

MTGService::MTGService() : Ice::Service()
{
}

MTGService::~MTGService()
{
}

void exception_handler(const Ice::Exception& ex)
{
    std::ostringstream formatter;
    formatter << "Exception thrown from asynchronous class: " << ex.what();
    Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

    Server::mtg_service.shutdown();
}

bool keep_alive()
{
    try {
        if (Server::server.communicator()->isShutdown()) {
            // Stop looping: The server has shut down.
            Logger::log(Logger::LOG_LEVEL_INFO, "Communicator is shut down, stopping loop.");

            return false;
        }

        Server::mtg_service.get_proxy()->KeepAlive_async(new VoidAsyncCallback<
            MainToGameSession::AMI_MTGSession_KeepAlive>(exception_handler));
    }
    catch (const Ice::Exception& ex) {
        // Server shut down: Shutdown.
        std::ostringstream formatter;
        formatter << "KeepAlive ping threw exception: " << ex.what();

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

        Server::server.stop();
        (void)Server::mtg_service.shutdown();

        return false;
    }

    return true;
}

void run_server_service(int argc, char * argv[])
{
    try {
        Ice::StringSeq seq;
        seq.push_back("--Ice.Config=config.theatre");
        (void)Server::server.main(seq);

        //Server::mtg_service.shutdown();
    }
    catch (const Ice::Exception &e) {
        std::ostringstream formatter;
        formatter << "Exception thrown from ServerService::main: " << e.what();

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
    }
    catch (...) {
        Logger::log(Logger::LOG_LEVEL_WARNING, 
            "ServerService::main() threw an unhandled exception.");
    }
}

int MTGService::main(int argc, char* argv[], const Ice::InitializationData& data)
{
    // To keep all of our eggs in one basket, convert to StringSeq and call other main().
    Ice::StringSeq seq;
    for (Ice::StringSeq::size_type i = 0; 
        i < static_cast<Ice::StringSeq::size_type>(argc); i++) {
        seq[i] = argv[i];
    }
    
    return main(seq, data);
}

int MTGService::main(Ice::StringSeq& seq, const Ice::InitializationData& data) 
{
    try {
        return Ice::Service::main(seq, data);
    }
    catch (const Ice::EndpointParseException& ex) {
        std::ostringstream formatter;
        formatter << "Some endpoint configuration is invalid: " << ex.str;

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
    }
    catch (const Ice::InitializationException& ex) {
        std::ostringstream formatter;
        formatter << "Error occurred during initialization: " << ex.reason;

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
    }
    catch (const Ice::Exception& ex) {
        std::ostringstream formatter;
        formatter << "Ice::Exception occurred during 'main()': " << ex.what();

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
    }

    return 1;
}

int MTGService::run(int argc, char* argv[], const Ice::InitializationData& data)
{
    try {
        return Ice::Service::run(argc, argv, data);
    }
    catch (const Ice::InitializationException& ex) {
        std::ostringstream formatter;
        formatter << "Error occurred during initialization: " << ex.reason;

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
    }
    catch (const Ice::Exception& ex) {
        std::ostringstream formatter;
        formatter << "Ice::Exception occurred during 'run()': " << ex.what();

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
    }

    return 1;
}

bool MTGService::start(int argc, char* argv[], int &)
{
    try {
        // Stored values required for login.
        const std::string server_name = communicator()->getProperties()->
            getProperty("ServerName");
        const std::string secret = communicator()->getProperties()->
            getProperty("Secret");
        const int port = communicator()->getProperties()->
            getPropertyAsInt("Port");
        const bool using_glacier2 = static_cast<bool>(communicator()->getProperties()->
            getPropertyAsInt("UsingGlacier2"));
        const std::string glacier2_host = communicator()->getProperties()->
            getProperty("Glacier2Host");
        const int glacier2_port = communicator()->getProperties()->
            getPropertyAsInt("Glacier2Port");
        const bool connect_glacier2 = static_cast<bool>(communicator()->getProperties()->
            getPropertyAsInt("ConnectThroughGlacier2"));
        
        Main::SessionFactoryPrx login_proxy = NULL;
        MainToGameSession::ClientSessionPrx callback_proxy = NULL;
        Ice::Identity sessionId;
        sessionId.name = "ClientSession";
        sessionId.category = "";

        if (connect_glacier2) {
            Ice::RouterPrx defaultRouter = communicator()->getDefaultRouter();
            router = Glacier2::RouterPrx::uncheckedCast(defaultRouter);

            Glacier2::SessionPrx glacier_session = router->createSession(server_name, "");

            mtg_adapter = communicator()->createObjectAdapterWithRouter(
                "ClientSession", router);
            sessionId.category = router->getCategoryForClient();
            
            login_proxy = Main::SessionFactoryPrx::uncheckedCast(glacier_session);
        }
        else {
            // Create a proxy that points directly to Echelon.
            Ice::ObjectPrx proxy = communicator()->propertyToProxy("MTGSession.Proxy");
            login_proxy = Main::SessionFactoryPrx::uncheckedCast(proxy);

            router = NULL;
            mtg_adapter = communicator()->createObjectAdapterWithEndpoints(
                "ClientSession", "tcp -h 31336");
        }

        VTANK_ASSERT(login_proxy != NULL);

        mtg_adapter->add(new MTGCallback, sessionId);

        // Create a callback proxy which the server will call to push messages to this machine.
        callback_proxy = 
            MainToGameSession::ClientSessionPrx::uncheckedCast(
            mtg_adapter->createProxy(sessionId)->ice_router(NULL));
        
        std::ostringstream formatter;
        formatter << "Attempting to connect with name=" << server_name 
            << " and secret=" << secret << ". Using glacier2: " << connect_glacier2;

        Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());

        mtg_adapter->activate();

        // Attempt to login. If this fails, it raises an exception.
        mtg_proxy = login_proxy->Join(server_name, secret, port, 
            using_glacier2, glacier2_host, glacier2_port, callback_proxy);

        VTANK_ASSERT(mtg_proxy != NULL);

        Logger::log(Logger::LOG_LEVEL_INFO, "Connection and login to Echelon successful!");

        Server::pool.schedule(boost::threadpool::looped_task_func(&keep_alive, 5000));
        service_thread = boost::thread(
            boost::bind<void>(run_server_service, argc, argv));

        mtg_proxy->SetMaxPlayerLimit(Players::player_limit);

        return true;
    }
    catch (const Exceptions::BadInformationException& ex) {
        std::ostringstream formatter;
        formatter << "Some information sent to the server is invalid: " << ex.reason;
        
        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
    }
    catch (const Exceptions::PermissionDeniedException& ex) {
        std::ostringstream formatter;
        formatter << "Permission denied from Echelon: " << ex.reason;

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
    }
    catch (const Glacier2::CannotCreateSessionException& ex) {
        std::ostringstream formatter;
        formatter << "Glacier2 cannot create a session: " << ex.reason;

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
    }
    catch (const Ice::ConnectionRefusedException& ex) {
        std::ostringstream formatter;
        formatter << "Unable to connect to Echelon: " << ex.what() << ": " << ex.error;

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
    }
    catch (const Ice::Exception& ex) {
        std::ostringstream formatter;
        formatter << "Ice exception: " << ex.what();

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
    }
    catch (const std::logic_error& ex) {
        std::ostringstream formatter;
        formatter << "Assertion error from MTGService::start(): " << ex.what();

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
    }
    catch (...) {
        Logger::log(Logger::LOG_LEVEL_ERROR, 
            "Uncaught exception thrown from MTGService::start().");
    }

    return false;
}

bool MTGService::stop()
{
    if (router != NULL) {
        try {
            router->destroySession();
        }
        catch (const Ice::Exception&) {
            // Expected.
        }
    }

    return true;
}
