/**
    @file ClockSync.ice
    @brief Interface for synchronizing clocks between a client and server.
    @author (C) Copyright 2009 by Vermont Technical College
*/

/**
    The ClockSync interface is part of the GameSession module since only Theatre
    cares if the clocks are synchronized.
*/
module GameSession
{
    /**
        In order to accurately calculate and predict client game character movements,
        the client must stamp the current time and send it to the server alongside 
        their action. The timestamp sent by the client is used to shift the client's
        position. Subtracting the current time (on the server) by the timestamp given by 
        the client gives us the approximate time that the client started to move. 
        However, there's a problem with this: Not all clocks are guaranteed to be
        synchronized. Indeed, computers in different time zones will have timestamps
        that differ by hours.
        
        The solution is to create a method for calculating the approximate offset of the 
        client's clock from the server's clock. The server pushes a request to the 
        client requesting a timestamp. The client returns the timestamp. The server
        approximates how long it took to do this, subtracts that value from the 
        timestamp, and then subtracts the server's timestamp from the client's timestamp
        to get the offset. It performs this a few more times to get a good idea of
        average latency and average offset.
        
        The clock synchronization should be performed every once in a while, perhaps
        every time the server shifts to a new map, in order to update the average
        latency value on the server.
    */
    interface ClockSynchronizer
    {
        /**
            Request a timestamp from the client.
            This method should be implemented by the <em>client, NOT</em> by the server.
            @return Timestamp.
        */
        ["ami"] long Request();
    };
};
