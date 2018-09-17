using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Util;

namespace AIFramework.Bot.Game
{
    /// <summary>
    /// Information about an in-game player.
    /// </summary>
    public class Player : ITarget
    {
        #region Members
        public static readonly int DEFAULT_MAX_HEALTH = 200;
        public const double RadiusRange = 1300;
        private GameSession.Tank tank;
        private double angle = 0;
        private double distance = 0;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the player's ID.
        /// </summary>
        public int ID
        {
            get
            {
                return tank.id;
            }
        }

        /// <summary>
        /// Get the tank's (player's) name.
        /// </summary>
        public string Name
        {
            get
            {
                return tank.attributes.name;
            }
        }

        /// <summary>
        /// Gets the tank's team.
        /// </summary>
        public GameSession.Alliance Team
        {
            get
            {
                return tank.team;
            }
        }

        /// <summary>
        /// Gets the tank's weapon.
        /// </summary>
        public Weapon Weapon
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the player's position.
        /// </summary>
        public VTankObject.Point Position
        {
            get
            {
                return tank.position;
            }

            set
            {
                tank.position = value;
            }
        }

        /// <summary>
        /// Gets the armor factor for this player.
        /// </summary>
        public float ArmorFactor
        {
            get
            {
                return tank.attributes.armorFactor;
            }
        }

        /// <summary>
        /// Gets the speed factor for this player.
        /// </summary>
        public float SpeedFactor
        {
            get
            {
                return tank.attributes.speedFactor;
            }
        }

        /// <summary>
        /// Get the player's health.
        /// </summary>
        public int Health
        {
            get
            {
                return tank.attributes.health;
            }

            private set
            {
                tank.attributes.health = value;
            }
        }

        /// <summary>
        /// Gets or sets the angle (radians) which the tank is facing.
        /// </summary>
        public double Angle
        {
            get
            {
                return angle;
            }

            set
            {
                angle = value % 6.28;
            }
        }

        /// <summary>
        /// Gets whether or not the tank is alive.
        /// </summary>
        public bool Alive
        {
            get
            {
                return tank.attributes.health > 0;
            }
        }

        /// <summary>
        /// Gets or sets the direction in which the player is moving.
        /// Valid options are: NONE, FORWARD, REVERSE.
        /// </summary>
        public VTankObject.Direction MovementDirection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the direction in which the player is rotating.
        /// Valid options are: NONE, LEFT, RIGHT.
        /// </summary>
        public VTankObject.Direction RotationDirection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the distance from the local player. Returns 0 if the local player is us.
        /// </summary>
        public double DistanceFromLocalPlayer
        {
            get
            {
                return distance;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new Player object.
        /// </summary>
        /// <param name="_tank">Tank identifying this player.</param>
        public Player(GameSession.Tank _tank)
        {
            tank = _tank;
            MovementDirection = VTankObject.Direction.NONE;
            RotationDirection = VTankObject.Direction.NONE;
            Angle = _tank.angle;
            Weapon = WeaponLoader.GetWeapon(tank.attributes.weaponID);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Set a new position (in 3D space units) for this player.
        /// </summary>
        /// <param name="x">New 'x' position for the player.</param>
        /// <param name="y">New 'y' position for the player.</param>
        public void SetPosition(double x, double y)
        {
            tank.position.x = x;
            tank.position.y = y;
        }

        /// <summary>
        /// Set a new position (in 3D space units) for this player.
        /// </summary>
        /// <param name="position">Position to set.</param>
        public void SetPosition(VTankObject.Point position)
        {
            SetPosition(position.x, position.y);
        }

        /// <summary>
        /// Inflicts damage to this player. Note that a negative value will
        /// restore the tank's health.
        /// </summary>
        /// <param name="value">Value to deduct from the player's health.</param>
        /// <param name="killingBlow">True if the blow killed the player; false otherwise.</param>
        /// <returns>True if the player is still alive; false otherwise.</returns>
        public bool InflictDamage(int value, bool killingBlow)
        {
            Health -= value;

            // TODO: Work-around for lack of buff implementation.
            if (!Alive && !killingBlow)
            {
                Health = 1;
            }
            else if (Alive && killingBlow)
            {
                Health = 0;
            }

            return Alive;
        }

        /// <summary>
        /// Have the player respawn at a certain position.
        /// </summary>
        /// <param name="x">'x' position of the tank.</param>
        /// <param name="y">'y' position of the tank.</param>
        public void Respawn(double x, double y)
        {
            Health = DEFAULT_MAX_HEALTH;
            SetPosition(x, y);
        }

        /// <summary>
        /// Have the player respawn at a certain position.
        /// </summary>
        /// <param name="position">Position of the tank.</param>
        public void Respawn(VTankObject.Point position)
        {
            Respawn(position.x, position.y);
        }

        /// <summary>
        /// Check if this player is in range of another player. The range is an
        /// arbitrary radius around the tank.
        /// </summary>
        /// <param name="other">Other player.</param>
        /// <returns>True if this player is in range of another player.</returns>
        public bool IsInRangeOf(Player other)
        {
            double x1 = Position.x;
            double x2 = other.Position.x;
            double y1 = Position.y;
            double y2 = other.Position.y;

            distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            if (distance > Weapon.Projectile.Range)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region ITarget Members

        /// <summary>
        /// Check if this player is alive.
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            return Alive;
        }

        /// <summary>
        /// Check how much health the player has.
        /// </summary>
        /// <returns></returns>
        public int GetHealth()
        {
            return Health;
        }

        /// <summary>
        /// Check the player's position.
        /// </summary>
        /// <returns></returns>
        public VTankObject.Point GetPosition()
        {
            return Position;
        }

        /// <summary>
        /// Check the player's team.
        /// </summary>
        /// <returns></returns>
        public GameSession.Alliance GetTeam()
        {
            return Team;
        }
        #endregion
    }
}
