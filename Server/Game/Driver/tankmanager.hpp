/*!
    \file   tankmanager.hpp
    \brief  Blueprint for a class which manages Tank instances.
    \author Copyright 
*/
#ifndef TANKMANAGER_HPP
#define TANKMANAGER_HPP

#include <tank.hpp>

/*!
    The TankNotExistException is used to notify interested parties that the player they
    wish to modify is no longer available.
*/
class TankNotExistException : public std::exception
{
private:
    int which;

public:
    /*!
        Create a TankNotExistException object.
        \param id ID of the tank which was not found.
    */
    TankNotExistException(const int id)
        : which(id)
    {   
    }

    /*!
        Get the ID of the tank that was saught.
        \return Identification number.
    */
    const int get_id() const
    {
        return which;
    }
};

/*!
    The TankManager must keep track of which tanks are still part of the game and which aren't.
    It's ultimately up to the manager which players get processed.
*/
class TankManager
{
private:
    std::map<int, tank_ptr> tanks;
    boost::shared_mutex mutex;
    
    /*!
        Performs the actual list get.
    */
    const tank_array inner_get_tank_list();

public:
    /*!
        Construct the TankManager class. The constructor only performs basic 
        initializations.
    */
    TankManager();

    /*!
        The destructor does nothing significant.
    */
   ~TankManager();

    /*!
        Add a tank to the manager.
        \param tank Tank to add.
    */
    void add(const tank_ptr);

    /*!
        Get a tank by it's ID number.
        \param id ID to look for.
        \return Pointer to a tank.
        \throws NoSuchTankException Thrown if the tank does not exist.
    */
    const tank_ptr get(const int);

    /*!
        Remove a tank identified by it's ID number.
        \param id ID to look for.
    */
    bool remove(const int);

    /*!
        Compile a list of tanks that this manager owns.
        \return Array of tanks that this manager manages.
    */
    const tank_array get_tank_list();

    /*!
        Get the number of elements in the tank manager.
        \return Integer indicating the size of the tank list.
    */
    const int size();

    /*!
        Go through each player and assign them to a team. This may mean 
        setting all players to no team, or setting people up into evenly
        distributed teams.
    */
    void organize_teams();

    /*!
        Assign a team for a new tank.
        \return Next team assignment.
    */
    GameSession::Alliance get_next_team_assignment();
};

#endif
