using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamestate;

namespace Gamestate {
    public class GamestateCreateTests
    {
        private GameObject gamestateTrackerObject;

        [SetUp]
        public void Setup() {
            gamestateTrackerObject = Instantiate();
        }

        [TearDown]
        public void Teardown() {

        }


        // A Test behaves as an ordinary method
        [Test]
        public void GamestateCreatePlayerTest()
        {
            
        }
    }
}
