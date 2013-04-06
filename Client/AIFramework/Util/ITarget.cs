using System;
using System.Collections.Generic;
using System.Text;

namespace AIFramework.Util
{
    /// <summary>
    /// Interface which potential targets (e.g. players, bases, etc.) must inherit.
    /// </summary>
    public interface ITarget
    {
        bool IsAlive();
        int GetHealth();
        bool InflictDamage(int health, bool killingBlow);
        void SetPosition(double x, double y);
        VTankObject.Point GetPosition();
        GameSession.Alliance GetTeam();
    }
}
