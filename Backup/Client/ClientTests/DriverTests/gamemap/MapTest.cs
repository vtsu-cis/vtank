
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using Client.src.util;

namespace ClientTests
{
    /// <summary>
    ///This is a test class for MapTest and is intended
    ///to contain all MapTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MapTest
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


        /// <summary>
        ///A test for CreateMap
        ///</summary>
        [TestMethod()]
        public void CreateMapTest()
        {
            string mapFileName = "testMap";
            string mapTitle = "Test";
            uint mapWidth = 2; 
            uint mapHeight = 2;

            Map target = new Map(mapTitle, mapFileName, mapWidth, mapHeight);

            Assert.AreEqual("testMap", target.FileName);
           
        }

        /// <summary>
        ///A test for SetGameModes
        ///</summary>
        [TestMethod()]
        public void SetGameModesTest()
        {
            string mapFileName = "test";
            string mapTitle = "Test";
            uint mapWidth = 2;
            uint mapHeight = 2;

            Map target = new Map(mapTitle, mapFileName, mapWidth, mapHeight);
            List<int> gameModes = new List<int>();


            gameModes.Add(1);
            gameModes.Add(2);

            target.SetGameModes(gameModes);


            Assert.IsTrue(target.GameModeSupported(1));
            Assert.IsFalse(target.GameModeSupported(3));

        }

        /// <summary>
        ///A test for SetTile
        ///</summary>
        [TestMethod()]
        public void SetTileTest()
        {
            string mapFileName = "testMap";
            string mapTitle = "Test";
            uint mapWidth = 2;
            uint mapHeight = 2;

            Map target = new Map(mapTitle, mapFileName, mapWidth, mapHeight);
            uint x = 1; 
            uint y = 1;

            Tile tile = new Tile();
            target.SetTile(x, y, tile);

            Assert.IsFalse(target.GetTile(x, y).IsPassable);
           
        }


        /// <summary>
        ///A test for GetTileData
        ///</summary>
        [TestMethod()]
        public void GetTileDataTest()
        {

            string mapFileName = "testMap";
            string mapTitle = "Test";
            uint mapWidth = 1;
            uint mapHeight = 1;

            Map target = new Map(mapTitle, mapFileName, mapWidth, mapHeight);

            Tile testTile = new Tile();
            testTile.IsPassable = true;


            Tile[] expected = { testTile };
            Tile[] actual = target.GetTileData();

            for (int i = 0; i < actual.Length; i++)
            {
                if (expected.Length != actual.Length)
                    Assert.Fail("Expected size does not equal actual size");
                Assert.AreEqual(expected[i].Effect, actual[i].Effect);
                Assert.AreEqual(expected[i].EventID, actual[i].EventID);
                Assert.AreEqual(expected[i].Height, actual[i].Height);
                Assert.AreEqual(expected[i].ID, actual[i].ID);
                Assert.AreEqual(expected[i].IsPassable, actual[i].IsPassable);
                Assert.AreEqual(expected[i].ObjectID, actual[i].ObjectID);
                Assert.AreEqual(expected[i].Type, actual[i].Type);
            }

        }

        /// <summary>
        ///A test for GetTile
        ///</summary>
        [TestMethod()]
        public void GetTileTest()
        {
            string mapFileName = "testMap";
            string mapTitle = "Test";
            uint mapWidth = 1;
            uint mapHeight = 1;

            Map target = new Map(mapTitle, mapFileName, mapWidth, mapHeight);
            
            uint x = 0;
            uint y = 0; 
            Tile expected = new Tile();
            expected.IsPassable = true;

            Tile actual;
            actual = target.GetTile(x, y);
            Assert.AreEqual(expected, actual);

        }
    }
}
