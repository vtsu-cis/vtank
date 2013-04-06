using Client.src.utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Client.src.utils.gameobjects;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Xna.Framework;
using VTankObject;
using Drivers.src.utils;
using Microsoft.Xna.Framework.Graphics;
using Client;

namespace ClientTests.DriverTests
{
    
    
    /// <summary>
    ///This is a test class for ProjectileManagerTest and is intended
    ///to contain all ProjectileManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ProjectileManagerTest
    {


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

        /// <summary>
        ///A test for CreateProjectileObject
        ///</summary>
        //[TestMethod]
        public void CreateProjectileObjectTest()
        {
            Model model = Program.GameObject.Content.Load<Model>(
                VTankGlobal.PROJECTILE_DIR + "missile");

            MissileObject missle = new MissileObject(model, "missile",
                new Vector3(20, 20, 0), 0, 200, 1000, "test");

            //ProjectileManager manager = new ProjectileManager();
            
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
    }
}
