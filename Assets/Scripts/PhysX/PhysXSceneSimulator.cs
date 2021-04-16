using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

using UnityEngine;

namespace PhysX {
    public class PhysXSceneSimulator {
        private static List<PhysXSceneManager> scenes = new List<PhysXSceneManager>();

        public static void AddScene(PhysXSceneManager scene) {
            scenes.Add(scene);
        }

        public static void Simulate() {

            foreach (PhysXSceneManager scene in scenes) {
                scene.Simulate();
            }
        }
    }
}