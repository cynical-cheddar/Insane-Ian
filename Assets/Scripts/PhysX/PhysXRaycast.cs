using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

using UnityEngine;

namespace PhysX {
    public class PhysXRaycast {
        private static Queue<PhysXRaycastHit> raycastHits = new Queue<PhysXRaycastHit>();
        private static IntPtr defaultPhysXRaycastHit = IntPtr.Zero;

        public static PhysXRaycastHit GetRaycastHit() {
            if (raycastHits.Count > 0) {
                return raycastHits.Dequeue();
            }
            else return new PhysXRaycastHit();
        }

        public static void ReleaseRaycastHit(PhysXRaycastHit raycastHit) {
            raycastHits.Enqueue(raycastHit);
        }

        public static bool Fire(Vector3 origin, Vector3 direction, float distance, PhysXRaycastHit raycastHit) {
            bool hit = PhysXSceneManager.FireRaycast(new PhysXVec3(origin), new PhysXVec3(direction), distance, raycastHit.physXRaycastHit);
            if (hit) raycastHit.PopulateFields();
            return hit;
        }

        public static bool Fire(Vector3 origin, Vector3 direction, PhysXRaycastHit raycastHit) {
            return Fire(origin, direction, float.MaxValue, raycastHit);
        }

        public static bool Fire(Vector3 origin, Vector3 direction, float distance) {
            if (defaultPhysXRaycastHit == IntPtr.Zero) defaultPhysXRaycastHit = PhysXLib.CreateRaycastHit();

            return PhysXSceneManager.FireRaycast(new PhysXVec3(origin), new PhysXVec3(direction), distance, defaultPhysXRaycastHit);
        }

        public static bool Fire(Vector3 origin, Vector3 direction) {
            return Fire(direction, origin, float.MaxValue);
        }
    }
}
