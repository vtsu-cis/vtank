/**
	@file CaptainVTank.ice
	@brief Session for the administrative client for VTank.
	@author (C) Copyright 2009 by Vermont Technical College
*/

#ifndef CAPTAINVTANK_ICE
#define CAPTAINVTANK_ICE

#include <Glacier2/Session.ice>
#include <Exception.ice>

/**
    Interact with the server in ways normal users of any other type can't. This includes
    managing connected users, user accounts, server configurations, connected game
    servers, and everything else.
*/
module Admin
{
    /**
        Admin-specific struct that stores information about an online client.
    */
    struct OnlineUser
    {
        string username;
        string clientType;  // "VTank", "Map Editor", etc.
        int userlevel;
        bool playingGame;
        bool online;
    };
    
    /**
        Admin-specific struct that stores information about an account.
    */
    struct Account
    {
        string accountName;
        string email;
    	string creationDate;
    	string lastLoggedIn;
        int rankLevel;
        int userLevel;
		long points;
    };
    
    /** Array of [OnlineUser] structs. */
    sequence<OnlineUser> UserList;
    
    /** Array of [Account] structs. */
    sequence<Account> AccountList;

    /**
        The administrator session through which the client communicates.
    */
    interface AdminSession extends Glacier2::Session
    {
        /**
            The client can prevent himself from being disconnected by calling this
            frequently.
        */
        ["ami"] void KeepAlive();
        
        /**
            Get the number of online users.
            \return Integer indicating how many users are online.
        */
        int GetUserCount();
        
        /**
            Get a list of online clients.
            \return [UserList] of players.
        */
        Admin::UserList GetUserList();
        
        /**
            Get a 'full' userlist -- that is, a list of users who are either online or
            offline.
            \return Sequence of users.
        */
        Admin::UserList GetFullUserList();
        
        /**
            Gets a list of all [Account] entities in the database.
            \return Sequence of [Account] structs.
        */
        Admin::AccountList GetAccountList();
        
        /**
            Get information about an account.
            \param accountName Name of the account you wish to view.
            \return [Account] information.
            \throws BadInformationException Thrown if the account does not exist.
        */
        Admin::Account GetAccountByName(string accountName)
            throws Exceptions::BadInformationException;
        
        /**
            Create a new account.
            \param username New username to create.
            \param password Password identifying the user.
            \param email E-mail address of the user.
            \param userlevel Privilege level of the user.
            \throws BadInformationException Thrown if the account exists or if some of
            the data is invalid.
        */
        void CreateAccount(string username, string password, string email, 
            int userlevel) throws Exceptions::BadInformationException;
        
        /**
            Edit information about an account.
            \param accountName (Old or current) account name of the person to edit.
            \param newInformation New information to update about an account.
            \throws BadInformationException Thrown if the account does not exist or if
            the new information is valid.
        */
        void EditAccountByName(string accountName, Admin::Account newInformation)
            throws Exceptions::BadInformationException;
        
        /**
            Set a new password for the account.
            \param accountName Name of the account.
            \param newPassword New password for the account.
            \throws BadInformationException Thrown when the account does not exist or
            if the password is invalid.
        */
        void SetAccountPassword(string accountName, string newPassword)
            throws Exceptions::BadInformationException;
        
        /**
            Delete an account given the account's name. This is undoable! This deletes
            all associated information, including the account's tanks.
            \param accountName Name of the account you wish to delete.
            \throws BadInformationException Thrown if the account does not exist.
        */
        void DeleteAccountByName(string accountName) 
            throws Exceptions::BadInformationException;
            
        /**
            Kick a user off. If they are playing a game, they are removed from that 
            game.
            \param accountName Person to kick off.
            \param reason Reason why this person is being kicked.
            \throws BadInformationException Thrown if the user is not logged in.
        */
        void KickUserByAccountName(string accountName, string reason)
            throws Exceptions::BadInformationException;
            
        /**
            Ban a user from the game. This kicks the user off and then disallows them 
            from logging in again. Be careful with this!
            \param accountName Person to kick off.
            \param reason Reason why this person is being banned.
            \throws BadInformationException Thrown if the user does not exist.
        */
        void BanUserByAccountName(string accountName, string reason)
            throws Exceptions::BadInformationException;
        
        /**
            Unban a user from the game. This allows the user to once again login.
            \param accountName Person to unban.
            \throws BadInformationException Thrown if the user does not exist, or if 
            the user is not banned.
        */
        void UnbanUserByAccountName(string accountName)
            throws Exceptions::BadInformationException;
    };
};

#endif
