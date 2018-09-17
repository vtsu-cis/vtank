/**
	@file Exception.ice
	@brief Exceptions specific to VTank.
	@author (C) Copyright 2009 by Vermont Technical College
*/

#ifndef EXCEPTION_ICE
#define EXCEPTION_ICE    

/**
    Holds only exceptions pertaining to VTank.
*/
module Exceptions
{
    /**
        Base exception for all VTank exceptions.
    */
    exception VTankException
    {
        string reason;
    };

    /**
        Exception thrown if the user attempts to do something that he cannot do.
        This will be most commonly thrown by the Auth::Login method.
    */
    exception PermissionDeniedException extends Exceptions::VTankException {};
    
    /**
        Exception thrown if the user attempts to login with a bad version ID.  Only
        thrown from authentication methods.
    */
    exception BadVersionException extends Exceptions::VTankException {};
    
    /**
        Exception thrown if the user sends data that makes no sense to the server.
        It can also be used in situations where the information is invalid for some
        reason -- for example, the user attempts to create an account with an empty
        or taken username.
    */
    exception BadInformationException extends Exceptions::VTankException {};
};

#endif
