using Client.src.utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Client.src.utils.gameobjects;
using System.Collections.Generic;
using System.Collections;
using GameSession;
using Microsoft.Xna.Framework;

namespace ClientTests.DriverTests
{
    
    
    /// <summary>
    ///This is a test class for PlayerManagerTest and is intended
    ///to contain all PlayerManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PlayerManagerTest
    {

        PlayerManager manager;
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            
        }
        
        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            
        }
        //
        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            manager = new PlayerManager(5, new Camera(new Vector3(0, 0, 0), new Vector3(0, 0, 0), Vector3.Up));
        }
        
        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            manager = null;
        }
        //

        //[TestMethod]
        public void GetLocalPlayerTest()
        {
            Assert.AreEqual(5, manager.GetLocalPlayer().ClientID);
        }

        //[TestMethod]
        public void CreateTankObjectTest()
        {
            GameSession.Tank testTank = new Tank(1, 1, true, new VTankObject.Point(1, 1), 
                new VTankObject.TankAttributes("test", .5f, .5f, 1, 100, "testModel", 
                    new VTankObject.Weapon(1, 1, "testWeapon", "testWeaponModel", "testSoundEffect", 
                        new VTankObject.Projectile(1, 1, 1, 1, "testProjectileModel")), 
                        new VTankObject.VTankColor(1, 1, 1)), Alliance.NONE);
            TankObject resultTank = manager.CreateTankObject(testTank);

            Assert.AreEqual(resultTank.ClientID, testTank.id);
            Assert.AreEqual(resultTank.Alive, testTank.alive);
            Assert.AreEqual(resultTank.Angle, testTank.angle);
            Assert.AreEqual(resultTank.Turret.TurretModel, testTank.attributes.weapon.model);
            Assert.AreEqual(resultTank.Health, testTank.attributes.health);
        }

        #endregion
    }
}
