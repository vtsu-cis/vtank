using System;
using System.Collections.Generic;
using System.Text;
using Client.src.util.game;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Client.src.util
{
    /// <summary>
    /// Reads weapons, projectiles, and environment effects from VTank's weapon system XML files and
    /// stores a collection of them for later reference.
    /// </summary>
    public static class WeaponLoader
    {
        #region Members        

        private static readonly string weaponsFile = "Weapons.xml";
        private static readonly string projectilesFile = "Projectiles.xml";
        private static readonly string environmentPropertiesFile = "EnvironmentProperties.xml";

        private static bool weaponsLoaded = false;
        private static bool projectilesLoaded = false;
        private static bool envPropsLoaded = false;
        private static Dictionary<int,Weapon> weapons = new Dictionary<int, Weapon>();
        private static Dictionary<int, ProjectileData> projectiles = new Dictionary<int, ProjectileData>();
        private static Dictionary<int, EnvironmentProperty> environmentProperties = new Dictionary<int, EnvironmentProperty>();
        #endregion

        #region Main Loader Methods

        /// <summary>
        /// Load all the weapon system's XML files into their respective collections.
        /// </summary>
        public static void LoadFiles()
        {
            LoadWeapons();
            LoadEnvironmentProperties();
            LoadProjectiles();            
        }

        /// <summary>
        /// Load the weapons from their XML file.
        /// </summary>
        public static void LoadWeapons()
        {
            weapons = new Dictionary<int, Weapon>();

            XmlDocument doc = new XmlDocument();
            doc.Load(weaponsFile);

            XmlNodeList weaponNodes = doc.GetElementsByTagName("weapon");

            foreach (XmlNode weaponNode in weaponNodes)
            {
                XmlNodeList weaponChildNodes = weaponNode.ChildNodes;

                LoadWeapon(weaponChildNodes);
            }

            weaponsLoaded = true;
        }

        /// <summary>
        /// Load a single weapon entry from the XML
        /// </summary>
        /// <param name="weaponChildNodes"></param>
        private static void LoadWeapon(XmlNodeList weaponChildNodes)
        {
            Dictionary<string, string> rawWeaponValues = new Dictionary<string, string>()
            {
                {"id", ""},
                {"name", ""},
                {"customColor", ""},
                {"weaponFireSound", ""},
                {"customColorValue", ""},
                {"colorMesh", ""},
                {"model", ""},
                {"deadModel", ""},
                {"muzzleEffectName", ""},
                {"projectileId", ""},
                {"cooldown", ""},
                {"launchAngle", ""},
                {"maxChargeTime", ""},
                {"projectilesPerShot", ""},
                {"intervalBetweenEachProjectile", ""},
                {"overheatTime", ""},
                {"overheatAmountPerShot", ""},
                {"overheatRecoverySpeed", ""},
                {"overheatRecoveryStartTime", ""}
            };

            List<Weapon.ParticleEmitter> emitters = null;
            List<Weapon.ModelAnimation> animations = null;

            //Copy collection's values so we can modify the first.
            List<string> rawValuesList = new List<string>();
            foreach (string key in rawWeaponValues.Keys)
            {
                rawValuesList.Add(key);
            }

            foreach (XmlNode weaponNode in weaponChildNodes)
            {
                foreach (string key in rawValuesList)
                {
                    if (weaponNode.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        rawWeaponValues[key] = GetXMLValue(weaponNode, key);
                    }
                    else if(weaponNode.Name.Equals("emitters", StringComparison.OrdinalIgnoreCase))
                    {
                        emitters = GetEmittersFromNode(weaponNode);
                    }
                    else if(weaponNode.Name.Equals("animations", StringComparison.OrdinalIgnoreCase))
                    {
                        animations = GetAnimationsFromNode(weaponNode);
                    }
                }
            }

            Weapon weaponEntry = GetWeaponFromStrings(rawWeaponValues, emitters, animations);
            if (weaponEntry != null)
            {
                weapons.Add(weaponEntry.ID, weaponEntry);
            }
        }

        /// <summary>
        /// Method used by LoadWeapon to get the Weapon object from the raw strings in the XML
        /// </summary>
        /// <param name="rawWeaponValues">The raw values for the weapon</param>
        /// <param name="emitters">The weapon's emitters</param>
        /// <param name="animations">The weapon's animations</param>
        /// <returns></returns>
        private static Weapon GetWeaponFromStrings(Dictionary<string, string> rawWeaponValues, 
            List<Weapon.ParticleEmitter> emitters,
            List<Weapon.ModelAnimation> animations)
        {
            bool valid = true;

            int id = HandleIdVal(rawWeaponValues["id"], out valid);
            string name = HandleNameVal(rawWeaponValues["name"], out valid);

            if (valid == false)
                return null;

            bool customColor = HandleBoolVal(rawWeaponValues["customColor"]);
            string weaponFireSound = rawWeaponValues["weaponFireSound"];
            Color customColorValue = HandleColorVal(rawWeaponValues["customColorValue"]);
            string colorMesh = rawWeaponValues["colorMesh"];
            string model = rawWeaponValues["model"];
            string deadModel = rawWeaponValues["deadModel"];
            string muzzleEffectName = rawWeaponValues["muzzleEffectName"];
            int projectileId = HandleIntVal(rawWeaponValues["projectileId"]);            
            float cooldown = HandleFloatVal(rawWeaponValues["cooldown"]);
            float launchAngle = MathHelper.ToRadians(HandleFloatVal(rawWeaponValues["launchAngle"]));
            float maxChargeTime = HandleFloatVal(rawWeaponValues["maxChargeTime"]);
            int projectilesPerShot = HandleIntVal(rawWeaponValues["projectilesPerShot"]);
            float intervalBetweenEachProjectile = HandleFloatVal(rawWeaponValues["intervalBetweenEachProjectile"]);
            float overheatTime = HandleFloatVal(rawWeaponValues["overheatTime"]);
            float overheatAmountPerShot = HandleFloatVal(rawWeaponValues["overheatAmountPerShot"]);
            float overheatRecoverySpeed = HandleFloatVal(rawWeaponValues["overheatRecoverySpeed"]);
            float overheatRecoveryStartTime = HandleFloatVal(rawWeaponValues["overheatRecoveryStartTime"]);

            return new Weapon(id, name, customColor, customColorValue, weaponFireSound, colorMesh, model, deadModel, muzzleEffectName, emitters,
                animations, projectileId, cooldown, launchAngle, maxChargeTime, projectilesPerShot, intervalBetweenEachProjectile,
                overheatTime, overheatAmountPerShot, overheatRecoverySpeed, overheatRecoveryStartTime);
            
        }

        /// <summary>
        /// Load the projectiles from their XML file
        /// </summary>
        public static void LoadProjectiles()
        {
            projectiles = new Dictionary<int, ProjectileData>();

            XmlDocument doc = new XmlDocument();
            doc.Load(projectilesFile);

            XmlNodeList projectileNodes = doc.GetElementsByTagName("projectile");

            foreach (XmlNode projectileNode in projectileNodes)
            {
                XmlNodeList projectileChildNodes = projectileNode.ChildNodes;

                LoadProjectile(projectileChildNodes);
            }

            projectilesLoaded = true;
        }

        /// <summary>
        /// Load a single projectile from an XML entry.
        /// </summary>
        /// <param name="projectileChildNodes"></param>
        private static void LoadProjectile(XmlNodeList projectileChildNodes)
        {
            Dictionary<string, string> rawProjectileValues = new Dictionary<string, string>()
            {
                {"id", ""},
                {"name", ""},
                {"modelName", ""},
                {"scale", ""},
                {"particleEffectName", ""},
                {"impactParticleName", ""},
                {"impactSoundEffect", ""},
                {"expirationParticleName", ""},
                {"areaOfEffectRadius", ""},
                {"areaOfEffectUsesCone", ""},
                {"areaOfEffectDecay", ""},
                {"coneRadius", ""},
                {"coneOriginWidth", ""},
                {"coneDamagesEntireArea", ""},
                {"minDamage", ""},
                {"maxDamage", ""},
                {"isInstantaneous", ""},
                {"initialVelocity", ""},
                {"terminalVelocity", ""},
                {"acceleration", ""},
                {"range", ""},
                {"rangeVariation", ""},
                {"jumpRange", ""},
                {"jumpDamageDecay", ""},
                {"collisionRadius", ""},
                {"environmentEffectID", ""},
                {"jumpCount", ""}
            };

            List<Weapon.ParticleEmitter> emitters = null;
            List<Weapon.ModelAnimation> animations = null;

            //Copy collection's values so we can modify the first.
            List<string> rawValuesList = new List<string>();
            foreach (string key in rawProjectileValues.Keys)
            {
                rawValuesList.Add(key);
            }

            foreach (XmlNode projectileNode in projectileChildNodes)
            {
                foreach (string key in rawValuesList)
                {
                    if (projectileNode.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        rawProjectileValues[key] = GetXMLValue(projectileNode, key);
                    }
                    else if (projectileNode.Name.Equals("emitters", StringComparison.OrdinalIgnoreCase))
                    {
                        emitters = GetEmittersFromNode(projectileNode);
                    }
                    else if (projectileNode.Name.Equals("animations", StringComparison.OrdinalIgnoreCase))
                    {
                        animations = GetAnimationsFromNode(projectileNode);
                    }
                }
            }

            ProjectileData projectileEntry = GetProjectileFromStrings(rawProjectileValues, emitters, animations);
            if (projectileEntry != null)
            {
                projectiles.Add(projectileEntry.ID, projectileEntry);
            }
        }

        /// <summary>
        /// Get the ProjectileData object from the raw strings from the XML
        /// </summary>
        /// <param name="rawProjectileValues">The string values from the XML</param>
        /// <param name="emitters">The projectile's emitters</param>
        /// <param name="animations">The projectile's animations</param>
        /// <returns>A ProjectileData object.</returns>
        private static ProjectileData GetProjectileFromStrings(Dictionary<string, string> rawProjectileValues,
            List<Weapon.ParticleEmitter> emitters, 
            List<Weapon.ModelAnimation> animations)
        {
            bool valid = true;

            int id = HandleIdVal(rawProjectileValues["id"], out valid);
            string name = HandleNameVal(rawProjectileValues["name"], out valid);

            if (valid == false)
                return null;

            string model = rawProjectileValues["modelName"];
            float projectileScale = HandleFloatVal(rawProjectileValues["scale"]);
            string particleEffectName = rawProjectileValues["particleEffectName"];
            string impactParticleName = rawProjectileValues["impactParticleName"];
            string impactSoundEffect = rawProjectileValues["impactSoundEffect"];
            string expirationParticleName = rawProjectileValues["expirationParticleName"];
            int areaOfEffectRadius = HandleIntVal(rawProjectileValues["areaOfEffectRadius"]);
            bool areaOfEffectUsesCone = HandleBoolVal(rawProjectileValues["areaOfEffectUsesCone"]);
            float areaOfEffectDecay = HandleFloatVal(rawProjectileValues["areaOfEffectDecay"]);
            int coneRadius = HandleIntVal(rawProjectileValues["coneRadius"]);
            int coneOriginWidth = HandleIntVal(rawProjectileValues["coneOriginWidth"]);
            bool coneDamagesEntireArea = HandleBoolVal(rawProjectileValues["coneDamagesEntireArea"]);
            int minDamage = HandleIntVal(rawProjectileValues["minDamage"]);
            int maxDamage = HandleIntVal(rawProjectileValues["maxDamage"]);
            bool isInstantaneous = HandleBoolVal(rawProjectileValues["isInstantaneous"]);
            int initialVelocity = HandleIntVal(rawProjectileValues["initialVelocity"]);
            int terminalVelocity = HandleIntVal(rawProjectileValues["terminalVelocity"]);
            int acceleration = HandleIntVal(rawProjectileValues["acceleration"]);
            int range = HandleIntVal(rawProjectileValues["range"]);
            int rangeVariation = HandleIntVal(rawProjectileValues["rangeVariation"]);
            int jumpRange = HandleIntVal(rawProjectileValues["jumpRange"]);
            float jumpDamageDecay = HandleFloatVal(rawProjectileValues["jumpDamageDecay"]);
            int jumpCount = HandleIntVal(rawProjectileValues["jumpCount"]);
            float collisionRadius = HandleFloatVal(rawProjectileValues["collisionRadius"]);
            int environmentEffectID = HandleIntVal(rawProjectileValues["environmentEffectID"]);

            return new ProjectileData(id, name, model, projectileScale, particleEffectName, impactParticleName, impactSoundEffect, expirationParticleName,
                emitters, animations, areaOfEffectRadius, areaOfEffectUsesCone, areaOfEffectDecay,coneRadius, coneOriginWidth, coneDamagesEntireArea,
                minDamage, maxDamage, isInstantaneous, initialVelocity, terminalVelocity, acceleration, range, rangeVariation, jumpRange,
                jumpDamageDecay, jumpCount, environmentEffectID, collisionRadius, WeaponLoader.GetEnvironmentProperty(environmentEffectID));
        }

        /// <summary>
        /// Load the environment properties from the XML
        /// </summary>
        public static void LoadEnvironmentProperties()
        {
            environmentProperties = new Dictionary<int, EnvironmentProperty>();

            XmlDocument doc = new XmlDocument();
            doc.Load(environmentPropertiesFile);

            XmlNodeList environmentPropertiesNodes = doc.GetElementsByTagName("environmentProperty");

            foreach (XmlNode environmentProperty in environmentPropertiesNodes)
            {
                XmlNodeList environmentPropertyChildNodes = environmentProperty.ChildNodes;

                LoadEnvironmentProperty(environmentPropertyChildNodes);
            }

            envPropsLoaded = true;
        }

        /// <summary>
        /// Load a single environment property from the XML
        /// </summary>
        /// <param name="environmentPropertyChildNodes"></param>
        private static void LoadEnvironmentProperty(XmlNodeList environmentPropertyChildNodes)
        {
            Dictionary<string, string> rawEnvironmentPropertyValues = new Dictionary<string, string>()
            {
                {"id", ""},
                {"name", ""},
                {"triggersUponImpactWithEnvironment", ""},
                {"triggersUponImpactWithPlayer", ""},
                {"triggersUponExpiration", ""},
                {"soundEffect", ""},
                {"particleEffect", ""},
                {"duration", ""},
                {"interval", ""},
                {"areaOfEffectRadius", ""},
                {"areaOfEffectDecay", ""},
                {"minDamage", ""},
                {"maxDamage", ""}
            };

            //Copy collection's values so we can modify the first.
            List<string> rawValuesList = new List<string>();
            foreach (string key in rawEnvironmentPropertyValues.Keys)
            {
                rawValuesList.Add(key);
            }

            foreach (XmlNode environmentPropertyNode in environmentPropertyChildNodes)
            {
                foreach (string key in rawValuesList)
                {
                    if (environmentPropertyNode.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        rawEnvironmentPropertyValues[key] = GetXMLValue(environmentPropertyNode, key);
                    }
                }
            }

            EnvironmentProperty environmentPropertyEntry = GetEnvironmentPropertyFromStrings(rawEnvironmentPropertyValues);
            if (environmentPropertyEntry != null)
            {
                environmentProperties.Add(environmentPropertyEntry.ID, environmentPropertyEntry);
            }
        }

        /// <summary>
        /// Get the environment property object from raw strings.
        /// </summary>
        /// <param name="rawEnvironmentPropertyValues">The raw string values from the XML</param>
        /// <returns>An EnvironmentProperty object</returns>
        private static EnvironmentProperty GetEnvironmentPropertyFromStrings(Dictionary<string, string> rawEnvironmentPropertyValues)
        {
            bool valid = true;

            int id = HandleIdVal(rawEnvironmentPropertyValues["id"], out valid);
            string name = HandleNameVal(rawEnvironmentPropertyValues["name"], out valid);

            if (valid == false)
                return null;

            bool triggersUponImpactWithEnvironment = HandleBoolVal(rawEnvironmentPropertyValues["triggersUponImpactWithEnvironment"]);
            bool triggersUponImpactWithPlayer = HandleBoolVal(rawEnvironmentPropertyValues["triggersUponImpactWithPlayer"]);
            bool triggersUponExpiration = HandleBoolVal(rawEnvironmentPropertyValues["triggersUponExpiration"]);

            string soundEffect = rawEnvironmentPropertyValues["soundEffect"];
            string particleEffect = rawEnvironmentPropertyValues["particleEffect"];
            float duration = HandleFloatVal(rawEnvironmentPropertyValues["duration"]);
            float interval = HandleFloatVal(rawEnvironmentPropertyValues["interval"]);
            float areaOfEffectRadius = HandleFloatVal(rawEnvironmentPropertyValues["areaOfEffectRadius"]);
            float areaOfEffectDecay = HandleFloatVal(rawEnvironmentPropertyValues["areaOfEffectDecay"]);

            int minDamage = HandleIntVal(rawEnvironmentPropertyValues["minDamage"]);
            int maxDamage = HandleIntVal(rawEnvironmentPropertyValues["maxDamage"]);

            return new EnvironmentProperty(id, name, triggersUponImpactWithEnvironment, triggersUponImpactWithPlayer, triggersUponExpiration,
                soundEffect, particleEffect, duration, interval, areaOfEffectRadius, areaOfEffectDecay, minDamage, maxDamage);
        }

        #endregion

        #region Type Handlers
        /********************************************************************************
         * This region has a series of methods that extract data values from raw strings.
         * Critical values, such as the ID or Name of an XML entry have an Out variable
         * determining their validity. (Particularly important for ID, which must be unique.)
         * 
         * Other types, such as float, use default values if the field found in the XML is
         * empty or otherwise invalid. Consequently NO EXCEPTIONS are thrown by these methods,
         * unless explicitly thrown.
         * *****************************************************************************/

        private static int HandleIdVal(string _id, out bool valid)
        {
            int id = -1;
            bool success = int.TryParse(_id, out id);

            if (success == false)
            {
                valid = false;
            }
            else
                valid = true;

            return id;
        }

        private static string HandleNameVal(string name, out bool valid)
        {
            if (String.IsNullOrEmpty(name))
            {
                valid = false;
            }
            else
                valid = true;

            return name;
        }

        private static float HandleFloatVal(string floatData)
        {
            float innerFloatData;

            bool success = float.TryParse(floatData, out innerFloatData);
            if (success == false)
            {
                return 0f;
            }
            else
                return innerFloatData;
        }

        private static int HandleIntVal(string intData)
        {
            int innerIntData;

            bool success = int.TryParse(intData, out innerIntData);
            if (success == false)
            {
                return -1;
            }
            else
                return innerIntData;
        }

        private static bool HandleBoolVal(string boolData)
        {
            bool innerBoolData;

            bool success = bool.TryParse(boolData, out innerBoolData);
            if (success == false)
            {
                return false;
            }
            else
                return innerBoolData;
        }

        /// <summary>
        /// TODO:  Implement Custom Colors.
        /// </summary>
        /// <param name="colorData"></param>
        /// <returns></returns>
        private static Color HandleColorVal(string colorData)
        {
            return Color.White;
        }
        #endregion

        #region XMLNode Getters
        /// <summary>
        /// Special case for extracting particle emitters from their XML node.
        /// </summary>
        /// <param name="node">The node to extract from.</param>
        /// <returns>A list of particle emitters.</returns>
        private static List<Weapon.ParticleEmitter> GetEmittersFromNode(XmlNode node)
        {
            List<Weapon.ParticleEmitter> emitters = new List<Weapon.ParticleEmitter>();

            XmlNodeList emittersChildren = node.ChildNodes;

            foreach (XmlNode emitterNode in emittersChildren)
            {
                string emitterName = null;
                string effectName = null;

                XmlNodeList emitterChildNodes = emitterNode.ChildNodes;
                for (int i = 0; i < emitterChildNodes.Count; i++)
                {
                    switch (emitterChildNodes[i].Name)
                    {
                        case "emitterName":
                            emitterName = emitterNode.InnerText;
                            break;
                        case "effectName":
                            effectName = emitterNode.InnerText;
                            break;
                    }
                }

                if (emitterName == null || effectName == null)
                {
                }
                else
                {
                    Weapon.ParticleEmitter __emitter = new Weapon.ParticleEmitter();
                    __emitter.name = emitterName;
                    __emitter.particleName = effectName;
                    emitters.Add(__emitter);
                }
            }

            return emitters;
        }

        /// <summary>
        /// Special case for extracting animations from their XML node.
        /// </summary>
        /// <param name="node">The node to extract from.</param>
        /// <returns>A list of animations.</returns>
        private static List<Weapon.ModelAnimation> GetAnimationsFromNode(XmlNode node)
        {
            List<Weapon.ModelAnimation> animations = new List<Weapon.ModelAnimation>();

            XmlNodeList animationNodes = node.ChildNodes;

            foreach (XmlNode animationNode in animationNodes)
            {
                string animationName = "";
                int startingFrameNumber = -1;
                int endFrameNumber = -1;
                bool isContinuous = false;
                string handlerClass = "";

                XmlNodeList animationChildNodes = animationNode.ChildNodes;
                for (int i = 0; i < animationChildNodes.Count; i++)
                {
                    switch (animationChildNodes[i].Name)
                    {
                        case "animationName":
                            animationName = animationNode.InnerText;
                            break;
                        case "startingFrameNumber":
                            startingFrameNumber = HandleIntVal(animationNode.InnerText);
                            break;
                        case "endFrameNumber":
                            endFrameNumber = HandleIntVal(animationNode.InnerText);
                            break;
                        case "isContinuous":
                            isContinuous = HandleBoolVal(animationNode.InnerText);
                            break;
                        case "handlerClass":
                            handlerClass = animationNode.InnerText;
                            break;
                    }
                }

                Weapon.ModelAnimation animation = new Weapon.ModelAnimation();
                animation.name = animationName;
                animation.startingFrame = startingFrameNumber;
                animation.endingFrame = endFrameNumber;
                animation.isContinuous = isContinuous;
                animations.Add(animation);
            }

            return animations;
        }

        /// <summary>
        /// Generically retrieve XML data (inner text) for a given XML element.
        /// </summary>
        /// <param name="node">The XMLNode containing the element in question</param>
        /// <param name="searchKey">The item being retrieved.</param>
        /// <returns></returns>
        private static string GetXMLValue(XmlNode node, string searchKey)
        {
            if (String.IsNullOrEmpty(node.InnerText))
            {
                return "";
            }

            return node.InnerText;
        }
        #endregion

        #region Weapon/Projectile Getters
        /// <summary>
        /// Get the entire weapon dictionary.
        /// </summary>
        /// <returns>All weapons as a WeaponID : Weapon dictionary.</returns>
        public static Dictionary<int, Weapon> GetWeapons()
        {
            if (weaponsLoaded == false)
                LoadWeapons();

            return weapons;
        }

        /// <summary>
        /// Gets all weapons as a list.
        /// </summary>
        /// <returns></returns>
        public static List<Weapon> GetWeaponsAsList()
        {
            List<Weapon> weaponsList = new List<Weapon>();

            if (weaponsLoaded == false)
                LoadWeapons();

            foreach (Weapon weapon in weapons.Values)
            {
                weaponsList.Add(weapon);
            }

            return weaponsList;
        }

        /// <summary>
        /// Get the projectile dictionary.
        /// </summary>
        /// <returns>All projectiles as a ProjectileID : ProjectileData dictionary.</returns>
        public static Dictionary<int, ProjectileData> GetProjectiles()
        {
            if (projectilesLoaded == false)
                LoadProjectiles();

            return projectiles;
        }

        /// <summary>
        /// Get the whole environment property dictionary.
        /// </summary>
        /// <returns>All loaded environment properties as an ID : EnvironmentProperty pair.</returns>
        public static Dictionary<int, EnvironmentProperty> GetEnvironmentProperties()
        {
            if (envPropsLoaded == false)
                LoadEnvironmentProperties();

            return environmentProperties;
        }

        /// <summary>
        /// Get a weapon from the dictionary.
        /// </summary>
        /// <param name="weaponID">The weapon's ID.</param>
        /// <returns>The weapon.</returns>
        public static Weapon GetWeapon(int weaponID)
        {
            if (weaponsLoaded == false)
                LoadProjectiles();

            if (weapons.ContainsKey(weaponID))
            {
                return weapons[weaponID];
            }
            else
                return null;
        }

        /// <summary>
        /// Gets a projectile from the dictionary.
        /// </summary>
        /// <param name="projectileID">The projectile's ID</param>
        /// <returns>The pr ojectile.</returns>
        public static ProjectileData GetProjectile(int projectileID)
        {
            if (projectilesLoaded == false)
                LoadProjectiles();

            if (projectiles.ContainsKey(projectileID))
            {
                return projectiles[projectileID];
            }
            else
                return null;
        }

        /// <summary>
        /// Gets an environment property from the dictionary.
        /// </summary>
        /// <param name="propertyID">The environment property's ID.</param>
        /// <returns>The environment property.</returns>
        public static EnvironmentProperty GetEnvironmentProperty(int propertyID)
        {
            if (envPropsLoaded == false)
                LoadEnvironmentProperties();

            if (environmentProperties.ContainsKey(propertyID))
            {
                return environmentProperties[propertyID];
            }
            else
                return null;
        }

        #endregion
    }
}
