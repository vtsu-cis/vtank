using System;
using System.Collections.Generic;
using System.Text;

namespace VTankBotRunner.SampleBot
{
    /// <summary>
    /// States that represent a bot's movement/action pattern.
    /// </summary>
    internal enum BotState
    {
        SEEK_AND_DESTROY,
        COMPLETE_OBJECTIVE,
        PROTECT_OBJECTIVE,
        FOLLOW_ALLY,
        CHASE_ENEMY
    }
    
    /// <summary>
    /// States that represent a bot's specific movement action.
    /// </summary>
    internal enum MicroBotState
    {
        STILL,
        ROTATE,
        MOVE_FORWARD,
        MOVE_REVERSE,
    }
}
