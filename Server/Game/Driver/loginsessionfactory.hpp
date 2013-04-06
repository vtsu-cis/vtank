/*!
    \file loginsessionfactory.hpp
    \brief Blueprint that defines a session-creator Ice servant.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef LOGINSESSIONFACTORY_HPP
#define LOGINSESSIONFACTORY_HPP

/*!
    The LoginSessionFactory class is an Ice servant. A servant implements an
    interface defined in the Slice specification in the IceCpp project.
    Documentation that delve into the details of the Game::Auth generated
    Ice code is available in it's respective .ice file: Game.ice.
*/
class LoginSessionFactory : public IGame::Auth
{
public:
    /*!
        Simple constructor.
		\param parent Takes the parent 'Server' object as a parameter.
    */
    LoginSessionFactory();

    /*!
        Simple destructor.
    */
    ~LoginSessionFactory();
    
    /* The following methods are overridden from Ice servants.
     * For documentation on these methods, see the Game.ice file.
     */
    virtual GameSession::GameInfoPrx JoinServer(const ::std::string&, 
        const GameSession::ClockSynchronizerPrx&, const GameSession::ClientEventCallbackPrx&,
        const Ice::Current& = Ice::Current());

    virtual Glacier2::SessionPrx create(const std::string&, const Glacier2::SessionControlPrx&, 
        const Ice::Current& = Ice::Current());
};

#endif
