using System;
using System.Collections.Generic;
using System.Text;
using Client.src.util.game.data;

namespace Client.src.util.helpers
{
    public static class TipLoader
    {
        private static List<Tip> tips;

        /// <summary>
        /// Statically load all the tips
        /// </summary>
        static TipLoader()
        {
            tips = new List<Tip>();
            //Weapon Tips
            tips.Add(new Tip("Laser", "Lasers can be charged by holding down fire"));
            tips.Add(new Tip("Laser", "Lasers can hit at extreme distances"));
            tips.Add(new Tip("Laser", "A charged laser is much more effective than an uncharged one"));

            tips.Add(new Tip("Mortar", "Mortars can fire over walls"));
            tips.Add(new Tip("Mortar", "Mortars are slow, but do huge damage"));
            tips.Add(new Tip("Mortar", "Mortar shells leave a brief, damaging fire in the area of impact"));
            tips.Add(new Tip("Mortar", "Mortar shells deal splash damage upon impact"));

            tips.Add(new Tip("Grenade Launcher", "Grenade launchers can fire over walls"));
            tips.Add(new Tip("Grenade Launcher", "Grenade launchers fire much faster than Mortars"));

            tips.Add(new Tip("Flamethrower", "Flamethrowers will eventually overheat if fired continuously"));
            tips.Add(new Tip("Flamethrower", "Flamethrowers have the highest burst damage in the game"));

            tips.Add(new Tip("Minigun", "Minigun damage is greatly affected by armor"));
            tips.Add(new Tip("Minigun", "The Minigun has the highest rate of fire in the game"));
            tips.Add(new Tip("Minigun", "A Minigun with rapid fire can be extremely deadly"));

            tips.Add(new Tip("Rocket Launcher", "The Rocket Launcher offers a nice balance of range and damage"));

            tips.Add(new Tip("Heavy Cannon", "Heavy Cannons deal splash damage"));
            tips.Add(new Tip("Heavy Cannon", "Heavy Cannons are among VTank's longest range weapons"));

            tips.Add(new Tip("Shotgun", "Shotguns have short range but high damage"));
            tips.Add(new Tip("Shotgun", "Shotguns fire many pellets, which can hit different targets"));

            //Game Mode Tips
            tips.Add(new Tip("ctb", "Destroy enemy bases by shooting at their towers"));
            tips.Add(new Tip("ctb", "Capture an enemy base by rolling over it after destroying it"));
            tips.Add(new Tip("ctb", "Make sure your forward tower is safe before attacking"));
            tips.Add(new Tip("ctb", "If you capture all enemy towers, you win the round"));
            tips.Add(new Tip("ctb", "Slow, armored tanks should consider defending in CTB mode"));
            tips.Add(new Tip("ctb", "Capturing a base causes the health on all bases to reset"));
            tips.Add(new Tip("ctb", "Only bases that are in conflict can be captured"));
            tips.Add(new Tip("ctb", "A bases in conflict are indicated by crossed swords (enemy) or a shield (friendly)"));

            tips.Add(new Tip("ctf", "Pick up an enemy flag by rolling over it"));
            tips.Add(new Tip("ctf", "Capture an enemy flag by returning it to your own base"));
            tips.Add(new Tip("ctf", "Destroying a tank carrying a flag causes them to drop it"));
            tips.Add(new Tip("ctf", "Rolling over a dropped enemy flag picks it up"));
            tips.Add(new Tip("ctf", "Rolling over a dropped friendly flag instantly returns it"));
            tips.Add(new Tip("ctf", "Fast tanks are excellent flag runners"));

            /*tips.Add(new Tip("You're fat!", (x) => {
                return true;
            }));*/
        }

        /// <summary>
        /// Get context sensitive tips
        /// </summary>
        /// <param name="context">The game's context</param>
        /// <returns>Queue of tips matching matching the given context</returns>
        public static Queue<string> GetTips(GameContext context)
        {
            Queue<string> tipQueue = new Queue<string>();

            foreach (Tip tip in tips)
            {
                if (context.Contains(tip.Context))
                    tipQueue.Enqueue(tip.Message);
            }                

            return tipQueue;
        }

        /// <summary>
        /// Get a queue of all tips
        /// </summary>
        /// <returns>Every tip in the main tip collection</returns>
        public static Queue<string> GetTips()
        {
            Queue<string> tipQueue = new Queue<string>();

            foreach (Tip tip in tips)
                tipQueue.Enqueue(tip.Message);

            return tipQueue;
        }
    }
}
