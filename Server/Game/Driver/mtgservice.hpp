/*!
    \file mtgservice.hpp
    \brief Blueprint for the entity which initializes Ice (for the MainToGame session).
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef MTGSERVER_HPP
#define MTGSERVER_HPP

/*!
    Like the ServerService class, this also initializes Ice to communicate with
    other parts of VTank. This particular service is only configured to run towards
    Echelon, though. ZeroC recommends separating the communicators for each job that
    they have to do so that communicators cannot interfere with each other. Since
    Ice::Service objects take care of initializing and cleaning up these communicators,
    it makes sense to create a service dedicated to Echelon.
*/
class MTGService : public Ice::Service
{
private:
    Ice::ObjectAdapterPtr mtg_adapter;
    MainToGameSession::MTGSessionPrx mtg_proxy;
    boost::thread service_thread;
    Glacier2::RouterPrx router;

public:
    MTGService();
    virtual ~MTGService();

    /*!
        Access to the main-to-game session proxy.
    */
    MainToGameSession::MTGSessionPrx get_proxy() const { return mtg_proxy; }
    
    /*!
        The main() function is overridden in order to provide better exception handling.
    */
    virtual int main(int, char *[], const Ice::InitializationData& = Ice::InitializationData());

    /*!
        The main() function is overridden in order to provide better exeception handling.
    */
    virtual int main(Ice::StringSeq&, const Ice::InitializationData& = Ice::InitializationData());

    /*!
        The run() function is overridden in order to provide better exception handling.
    */
    virtual int run(int, char *[], const Ice::InitializationData& = Ice::InitializationData());

    /*!
        This function is called by Ice when Ice-related components are initialized 
        by the Ice::Service::main() function.
        \param argc Number of elements in the command-line arguments.
        \param argv Arguments to consider, if any.
        \return True if it's alright to proceed, false otherwise.
    */
    virtual bool start(int, char*[], int&);

    /*!
        This function is called by Ice when the server is shutting down.
    */
    virtual bool stop();
};

#endif
