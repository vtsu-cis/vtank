/*!
    \file server.hpp
    \brief Blueprint for the entity which initializes Ice.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef SERVER_HPP
#define SERVER_HPP

#include <player.hpp>
#include <asynctemplate.hpp>
#include <mtgservice.hpp>

namespace Server {
    /*!
        The ServerService class acts as a method for initializing Ice in an Ice-friendly way.
        The inherited Ice::Service class provides a 'main()' function which 
        initializes Ice. The Ice runtime then calls the 'start()' function.
    */
    class ServerService
    {
    private:
        Ice::CommunicatorPtr comm;
        Ice::ObjectAdapterPtr adapter;

    public:
        /*!
            The constructor for ServerService does nothing significant.
        */
	    ServerService();

        /*!
            Since the ServerService class does not dynamically allocate memory, it does
            not free any memory here.
        */
	   ~ServerService();

        /*!
            Access to the object adapter which listens for clients.
        */
        const Ice::ObjectAdapterPtr get_adapter() const { return adapter; }

        const Ice::CommunicatorPtr communicator() const { return comm; }

        /*!
            The main() function is overridden in order to provide better exception handling.
        */
        int main(Ice::StringSeq);

        /*!
            This function is called by Ice when Ice-related components are initialized 
            by the Ice::Service::main() function.
            \param argc Number of elements in the command-line arguments.
            \param argv Arguments to consider, if any.
            \return True if it's alright to proceed, false otherwise.
        */
        bool start();

        /*!
            This function is called by Ice when the server is shutting down.
        */
        void stop();
    };

    extern ServerService server;
    extern MTGService mtg_service;
    extern boost::threadpool::pool pool;

    /*!
        Send a keep alive request to the server. This also works as a "connection check",
        meaning that if the connection failed, the game server will shut down.
    */
    bool keep_alive();
}

#endif
