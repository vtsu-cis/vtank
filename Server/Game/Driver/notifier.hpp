/*!
    \file   notifier.hpp
    \brief  Declares functions that help notify players of events.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef NOTIFIER_HPP
#define NOTIFIER_HPP

#include <tank.hpp>

/*!
    The Notifier namespace contains functions which assist in delivering event
    messages to players. These functions are meant to be delivered as a threadpool
    task. The functions implemented in this namespace will also take care of removing
    players from the game who throw connection-related exceptions.
*/
namespace Notifier {
    
    /*!
        Perform a blanket notify that a player has respawned.
        \param id ID of the player who respawned.
        \param position New position of the player.
    */
    void blanket_notify_player_respawn(const int, const VTankObject::Point &);
    
    /*!
        Perform a blanket notify of a projectile hit.
        \param owner_id Player who got shot.
        \param projectile_id ID of the projectile that hit the player.
        \param fired_by_id ID of the player who fired the projectile.
        \param damage_taken Amount of damage taken.
		\param killing_blow True if the blow was a killing blow; false otherwise.
    */
    void blanket_notify_player_damaged(const int, const int, const int, const int,
		const bool);

    /*!
        Perform a blanket notify that a player left.
        \param id ID of the player who left.
    */
    void blanket_notify_player_left(const int);

    /*!
        Perform a blanket notify that a player joined.
        \param tank Tank that joined.
    */
    void blanket_notify_player_joined(const tank_ptr);

    /*!
        Perform a blanket notification that the map is rotating.
    */
    void blanket_notify_rotate_map();

	/*!
		Notify everyone of a new chat message.
		\param message Chat message to send.
		\param color Color of the message.
	*/
	void blanket_notify_chat_message(const std::string &, const VTankObject::VTankColor &);

	/*!
		Notify everyone of a new projectile being created.
		\param owner_id ID of the person firing the projectile.
		\param projectile_id ID of the projectile.
		\param projectile_type_id Type of the projectile being fired.
		\param end_point Targeted position.
	*/
	void blanket_notify_create_projectile(const int owner_id, const int projectile_id,
			const int projectile_type_id, const VTankObject::Point &end_point);
	
	/*!
		Notify only a certain group of a new chat message.
		\param tanks Tank list to distribute the message to.
		\param message Chat message to send.
		\param color Color of the message.
	*/
	void notify_chat_message(const tank_array &, const std::string &, 
		const VTankObject::VTankColor &);

	/*!
		Notify everyone of a utility spawning.
		\param tanks Tank list to distribute the message to.
		\param utilityID Utility number for tracking purposes.
		\param util Utility to spawn.
		\param pos Position of the utility.
	*/
	void blanket_notify_utility_spawn(const tank_array &, int,
		const VTankObject::Utility &, const VTankObject::Point &);

	/*!
		Notify everyone that a player has picked up a utility.
		\param tanks Tank list to distribute the messag eto.
		\param tankID ID of the player who received a utility.
		\param utilityID Utility number for tracking purposes.
		\param util Utility effect to apply.
	*/
	void blanket_notify_apply_utility(const tank_array &, int, int,
		const VTankObject::Utility &);
	
	/*!
		Notify one player that a utility has spawned.
		\param tank Tank to notify.
		\param utilityID ID of the utility.
		\param util Type of utility.
		\param pos Position of the utility.
	*/
	void notify_utility_spawn(const tank_ptr &, int,
		const VTankObject::Utility &, const VTankObject::Point &);

	/*!
		Notify one player that a flag has spawned.
	*/
	void notify_flag_spawned(const tank_ptr &, const VTankObject::Point &,
		const GameSession::Alliance &);

	/*!

	*/
	void notify_flag_picked_up(const tank_ptr &, int,
		const GameSession::Alliance &);
	
	/*!
		
	*/
	void blanket_notify_flag_dropped(const tank_array &, int,
		const VTankObject::Point &, const GameSession::Alliance &);

	/*!
		
	*/
	void blanket_notify_flag_returned(const tank_array &, int,
		const GameSession::Alliance &);

	/*!
		
	*/
	void blanket_notify_flag_picked_up(const tank_array &, int,
		const GameSession::Alliance &);

	/*!
		
	*/
	void blanket_notify_flag_captured(const tank_array &, int,
		const GameSession::Alliance &);

	/*!
		
	*/
	void blanket_notify_flag_spawned(const tank_array &, 
		const VTankObject::Point &, const GameSession::Alliance &);

	/*!
		
	*/
	void blanket_notify_flag_despawned(const tank_array &,
		const GameSession::Alliance &);
	
	/*!
		\param tank_list List of tanks to send the notification to.
		\param old_base_color Color (team) of the old base.
		\param new_base_color Color (team) of the new base.
		\param base_id ID number of the base in question.
		\param capturer_id ID number of the person who captured the base.
	*/
	void blanket_notify_base_captured(const tank_array &,
		const GameSession::Alliance &, const GameSession::Alliance &,
		int, int);

	/*!
		\param tank_list
		\param base_color Color (team) of the base.
		\param base_id ID of the base.
		\param health Health of the base.
	*/
	void blanket_notify_set_base_status(const tank_array &,
		const GameSession::Alliance &, const int, const int);
	
	/*!

	*/
	void notify_set_base_status(const tank_ptr &,
		const GameSession::Alliance &, const int, const int);
	
	/*!
		\param tank_list
		\param base_color Color (team) of the base.
		\param base_id ID of the base.
		\param damage Damage dealt to the base.
		\param projectile_id ID of the projectile which dealt the damage.
		\param player_id ID of the player who dealt the damage.
		\param is_destroyed True if the base was destroyed; false otherwise.
	*/
	void blanket_notify_damage_base(const tank_array &,
		const GameSession::Alliance &, int, int, int, int, bool);

	void notify_reset_position(const tank_ptr &player, const VTankObject::Point &pos);

	void blanket_notify_player_moved(const int who_moved, const VTankObject::Point &pos, 
		const VTankObject::Direction &direction);

	void blanket_notify_end_round(const GameSession::Alliance &winner);

	void blanket_notify_spawn_env_effect(int env_id, int type_id, int owner_id,
		const VTankObject::Point &position);

	void blanket_notify_create_projectiles(const GameSession::ProjectileDamageList &list);

	void blanket_notify_damage_base_by_env(const GameSession::Alliance &team,
		const int base_id, const int env_id, const int damage, const bool killing_blow);

	void blanket_notify_damage_player_by_env(const int victim_id,
		const int env_id, const int damage, const bool killing_blow);
}

#endif
