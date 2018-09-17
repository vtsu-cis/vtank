/**
	@file HealthMonitor.ice
	@brief Communication structure between a backup daemon and the main server.
	@author (C) Copyright 2010 by Vermont Technical College
*/

#ifndef HEALTHMONITOR_ICE
#define HEALTHMONITOR_ICE

#include <VTankObjects.ice>

/**
	Namespace encapsulating health monitor interfaces.
*/
module Monitor
{
	/**
		Interface which a backup daemon script must implement in order to receive
		health update requests.
	*/
	interface HealthMonitor
	{
		/**
			Get a snapshot of this server's health.
		*/
		["ami"] VTankObject::HealthSnapshot GetHealth();
	};
	
	/**
		Implemented by the main server.
	*/
	interface HealthMonitorEndpoint
	{
		["ami"] void KeepAlive();
	};
};

#endif
