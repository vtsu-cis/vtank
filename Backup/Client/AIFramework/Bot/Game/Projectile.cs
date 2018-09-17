using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Util;

namespace AIFramework.Bot.Game
{
    /// <summary>
    /// Tracks information about a projectile.
    /// </summary>
    public class Projectile
    {
        #region Properties
        /// <summary>
        /// Gets the ID of this projectile.
        /// </summary>
        public int ID
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets information about this projectile.
        /// </summary>
        public int ProjectileTypeID
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the position where this projectile originally spawned.
        /// </summary>
        public VTankObject.Point StartingPosition
        {
            get;
            private set;
        }

        public double Angle
        {
            get;
            private set;
        }

        public ProjectileData Data
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        public Projectile(int ID, int projectileTypeId, 
            VTankObject.Point position, double angle)
        {
            this.ID = ID;
            ProjectileTypeID = projectileTypeId;
            this.Data = WeaponLoader.GetProjectile(ProjectileTypeID);
            StartingPosition = position;
            Angle = angle;
        }
        #endregion
    }
}
