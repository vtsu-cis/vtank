using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using AIFramework.Bot.Game;


namespace AIFramework.Util
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
        private static Dictionary<int, Weapon> weapons = new Dictionary<int, Weapon>();
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
                    else if (weaponNode.Name.Equals("emitters", StringComparison.OrdinalIgnoreCase))
                    {
                        emitters = GetEmittersFromNode(weaponNode);
                    }
                    else if (weaponNode.Name.Equals("animations", StringComparison.OrdinalIgnoreCase))
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
            string colorMesh = rawWeaponValues["colorMesh"];
            string model = rawWeaponValues["model"];
            string deadModel = rawWeaponValues["deadModel"];
            string muzzleEffectName = rawWeaponValues["muzzleEffectName"];
            int projectileId = HandleIntVal(rawWeaponValues["projectileId"]);
            float cooldown = HandleFloatVal(rawWeaponValues["cooldown"]);
            float launchAngle = HandleFloatVal(rawWeaponValues["launchAngle"]);
            float maxChargeTime = HandleFloatVal(rawWeaponValues["maxChargeTime"]);
            int projectilesPerShot = HandleIntVal(rawWeaponValues["projectilesPerShot"]);
            float intervalBetweenEachProjectile = HandleFloatVal(rawWeaponValues["intervalBetweenEachProjectile"]);
            float overheatTime = HandleFloatVal(rawWeaponValues["overheatTime"]);
            float overheatAmountPerShot = HandleFloatVal(rawWeaponValues["overheatAmountPerShot"]);
            float overheatRecoverySpeed = HandleFloatVal(rawWeaponValues["overheatRecoverySpeed"]);
            float overheatRecoveryStartTime = HandleFloatVal(rawWeaponValues["overheatRecoveryStartTime"]);

            return new Weapon(id, name, customColor, null, weaponFireSound, colorMesh, model, deadModel, muzzleEffectName, emitters,
                animations, projectileId, cooldown, launchAngle, maxChargeTime, projectilesPerShot, intervalBetweenEachProjectile,
                overheatTime, overheatAmountPerShot, overheatRecoverySpeed, overheatRecoveryStartTime);

        }

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
                {"expirationParticleName", ""},
                {"areaOfEffectRadius", ""},
                {"areaOfEffectUsesCone", ""},
                {"areaOfEffectDecay", ""},
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
            string expirationParticleName = rawProjectileValues["expirationParticleName"];
            int areaOfEffectRadius = HandleIntVal(rawProjectileValues["areaOfEffectRadius"]);
            bool areaOfEffectUsesCone = HandleBoolVal(rawProjectileValues["areaOfEffectUsesCone"]);
            float areaOfEffectDecay = HandleFloatVal(rawProjectileValues["areaOfEffectDecay"]);
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
            int environmentEffectID = HandleIntVal(rawProjectileValues["environmentEffectID"]);

            return new ProjectileData(id, name, model, projectileScale, particleEffectName, impactParticleName, expirationParticleName,
                emitters, animations, areaOfEffectRadius, areaOfEffectUsesCone, areaOfEffectDecay, coneOriginWidth, coneDamagesEntireArea,
                minDamage, maxDamage, isInstantaneous, initialVelocity, terminalVelocity, acceleration, range, rangeVariation, jumpRange,
                jumpDamageDecay, jumpCount, environmentEffectID, WeaponLoader.GetEnvironmentProperty(environmentEffectID));
        }

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

        private static void LoadEnvironmentProperty(XmlNodeList environmentPropertyChildNodes)
        {
            Dictionary<string, string> rawEnvironmentPropertyValues = new Dictionary<string, string>()
            {
                {"id", ""},
                {"name", ""},
                {"triggersUponImpactWithEnvironment", ""},
                {"triggersUponImpactWithPlayer", ""},
                {"triggersUponExpiration", ""},
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

            string particleEffect = rawEnvironmentPropertyValues["particleEffect"];
            float duration = HandleFloatVal(rawEnvironmentPropertyValues["duration"]);
            float interval = HandleFloatVal(rawEnvironmentPropertyValues["interval"]);
            float areaOfEffectRadius = HandleFloatVal(rawEnvironmentPropertyValues["areaOfEffectRadius"]);
            float areaOfEffectDecay = HandleFloatVal(rawEnvironmentPropertyValues["areaOfEffectDecay"]);

            int minDamage = HandleIntVal(rawEnvironmentPropertyValues["minDamage"]);
            int maxDamage = HandleIntVal(rawEnvironmentPropertyValues["maxDamage"]);

            return new EnvironmentProperty(id, name, triggersUponImpactWithEnvironment, triggersUponImpactWithPlayer, triggersUponExpiration,
                particleEffect, duration, interval, areaOfEffectRadius, areaOfEffectDecay, minDamage, maxDamage);
        }

        #endregion

        #region Type Handlers
        private static int HandleIdVal(string _id, out bool valid)
        {
            int id = -1;
            bool success = int.TryParse(_id, out id);

            if (success == false)
            {
                Console.WriteLine("Warning:  Invalid value for ID XML weapon files, ignoring entry.");
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
                Console.WriteLine("Warning:  Invalid value for name in XML weapon files, ignoring entry.");
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

        private static string HandleColorVal(string colorData)
        {
            Console.WriteLine("Warning:  Custom color value detected when no function exists to handle it, defaulting to white.");
            return null;
        }
        #endregion

        #region XMLNode Getters
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
                    Console.WriteLine("Warning:  Found null values in an emitter entry, ignoring entry");
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
                Console.Error.WriteLine("Warning: Empty field found for " + searchKey);
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
