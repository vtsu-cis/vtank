using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework;

namespace GameForms
{
    public enum GameForm
    {
        LOGIN_FORM,
        TANK_LIST_FORM,
        TANK_CREATION_FORM,
        TANK_EDIT_FORM,
        SERVER_LIST_FORM,
        LOADING_SCREEN_FORM,
        GAMEPLAY_FORM
    }

    public static class Utils
    {
        public static int ConvertFactorToProgressBar(float factor)
        {
            return (int)(factor * 100f - 50f);
        }

        public static float ConvertProgressBarToFactor(int value)
        {
            return ((float)(value + 50f) / 100f);
        }

        public static int ConvertPowerToProgressBar(int dmg)
        {
            return dmg*2;
        }

        public static int ConvertRangeToProgressBar(int range)
        {
            return (int)(range/10000);
        }

        public static int ConvertRateToProgressBar(float cd)
        {
            return (int)(2f / cd) * 5;
        }

        public static List<object> GetTankModels()
        {
            List<object> tanks = new List<object>();

            DirectoryInfo dir = new DirectoryInfo("Content\\models\\tanks\\");

            //Gets all the files with the extention .xnb
            FileInfo[] files = dir.GetFiles("*.xnb");


            string[] tempString = new string[files.Length];

            //Removes the extention from the files and stores the file names into the string
            foreach (FileInfo f in files)
            {
                string tempTank = f.ToString().Substring(0, f.ToString().LastIndexOf('.'));

                if (tempTank.EndsWith("_0") || tempTank.EndsWith("_dead"))
                    continue;
                else
                    tanks.Add(tempTank);
            }

            return tanks;
        }

        public static List<string> GetWeaponModels()
        {
            List<string> weapons = new List<string>();

            DirectoryInfo dir = new DirectoryInfo("Content\\models\\weapons\\");

            //Gets all the files with the extention .xnb
            FileInfo[] files = dir.GetFiles("*.xnb");

            string[] tempString = new string[files.Length];

            //Removes the extention from the files and stores the file names into the string
            foreach (FileInfo f in files)
            {
                string weaponName = f.ToString().Substring(0, f.ToString().LastIndexOf('.'));

                if (weaponName.EndsWith("_0") || weaponName.EndsWith("_dead"))
                    continue;
                else
                    weapons.Add(weaponName);
            }

            return weapons;
        }

        public static List<string> GetProjectileModels()
        {
            List<string> projectiles = new List<string>();

            DirectoryInfo dir = new DirectoryInfo("Content\\models\\projectiles\\");

            //Gets all the files with the extention .xnb
            FileInfo[] files = dir.GetFiles("*.xnb");

            string[] tempString = new string[files.Length];

            //Removes the extention from the files and stores the file names into the string
            foreach (FileInfo f in files)
            {
                string projectileName = f.ToString().Substring(0, f.ToString().LastIndexOf('.'));

                if (projectileName.EndsWith("_0"))
                    continue;
                else
                    projectiles.Add(projectileName);
            }

            return projectiles;
        }

        public static List<string> GetUtilityModels()
        {
            List<string> utilities = new List<string>();

            DirectoryInfo dir = new DirectoryInfo("Content\\models\\powerups\\");

            //Gets all the files with the extention .xnb
            FileInfo[] files = dir.GetFiles("*.xnb");

            //Removes the extention from the files and stores the file names into the string
            foreach (FileInfo f in files)
            {
                string utilityName = f.Name.Replace(".xnb", "");
                
                if (utilityName.EndsWith("_0"))
                    continue;
                
                utilities.Add(utilityName);
            }

            return utilities;
        }

        /// <summary>
        /// Find all resources in the target directory (usually starting with "Content\").
        /// It does not load files ending in _0. Note that if the directory does not exist,
        /// no file names are returned.
        /// </summary>
        /// <param name="directory">Directory to load XNB resources from.</param>
        /// <returns>List of file names under this directory, if any.</returns>
        public static List<string> GetResources(string directory)
        {
            List<string> result = new List<string>();

            DirectoryInfo dir = new DirectoryInfo(directory);
            if (dir.Exists)
            {
                FileInfo[] files = dir.GetFiles("*.xnb");
                foreach (FileInfo file in files)
                {
                    string modelName = file.Name.Replace(".xnb", "");
                    if (modelName.EndsWith("_0"))
                    {
                        // Do not load models ending with _0 (compiled textures).
                        continue;
                    }

                    result.Add(modelName);
                }
            }

            return result;
        }

        public static List<string> GetEffects()
        {
            List<string> effects = new List<string>();
            DirectoryInfo dir = new DirectoryInfo("Content\\effects\\");

            FileInfo[] files = dir.GetFiles("*.xnb");

            foreach (FileInfo inf in files)
            {
                string effectName = inf.ToString().Substring(0, inf.ToString().LastIndexOf('.'));

                if (effectName.EndsWith("_0"))
                    continue;
                else
                {
                    effects.Add(effectName);
                }
            }
            return effects;
         }
    }
}
