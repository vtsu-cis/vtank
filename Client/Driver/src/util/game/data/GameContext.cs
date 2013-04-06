using System;
using System.Collections.Generic;
using System.Text;
using VTankObject;

namespace Client.src.util.game.data
{
    /// <summary>
    /// Contains information pertaining to a game
    /// </summary>
    public class GameContext
    {
        GameMode currentMode;
        Weapon currentWeapon;
        PlayerTank currentPlayer;

        /// <summary>
        /// Constructor for the GameContext
        /// </summary>
        /// <param name="currentMode">The current game mode being played</param>
        /// <param name="currentWeapon">The current weapon being used</param>
        public GameContext(GameMode currentMode, PlayerTank currentPlayer)
        {
            this.currentMode = currentMode;
            this.currentWeapon = currentPlayer.Weapon;
            this.currentPlayer = currentPlayer;
        }

        public bool PlayerIsDead()
        {
            if (currentPlayer.Alive == false)
                return true;

            return false;
        }

        /// <summary>
        /// Find out if the given keyword is relevant to this GameContext
        /// </summary>
        /// <param name="keyWord">Keyword to evaluate</param>
        /// <returns>True if relevant, False otherwise</returns>
        public bool Contains(string keyWord)
        {
            bool found = false;

            switch (keyWord.ToLower())
            {
                case "laser":
                    found = (currentWeapon.Name == "Laser Cannon");
                    break;
                case "mortar":
                    found = (currentWeapon.Name == "Mortar");
                    break;
                case "shotgun":
                    found = (currentWeapon.Name == "Shotgun");
                    break;
                case "grenade launcher":
                    found = (currentWeapon.Name == "Grenade Launcher");
                    break;
                case "heavy cannon":
                    found = (currentWeapon.Name == "Heavy Cannon");
                    break;
                case "flamethrower":
                    found = (currentWeapon.Name == "Flamethrower");
                    break;
                case "minigun":
                    found = (currentWeapon.Name == "Minigun");
                    break;
                case "rocket launcher":
                    found = (currentWeapon.Name == "Rocket Launcher");
                    break;
                case "capture the base": case "ctb":
                    found = (currentMode == GameMode.CAPTURETHEBASE);
                    break;
                case "capture the flag": case "ctf":
                    found = (currentMode == GameMode.CAPTURETHEFLAG);
                    break;
                case "deathmatch": case "dm":
                    found = (currentMode == GameMode.DEATHMATCH);
                    break;
                case "team deathmatch": case "tdm":
                    found = (currentMode == GameMode.TEAMDEATHMATCH);
                    break;
            }

            return found;
        }
    }
}
