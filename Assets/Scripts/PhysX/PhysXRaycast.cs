using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

using UnityEngine;

namespace PhysX {
    public class PhysXRaycast {
        private static Queue<PhysXRaycastHit> raycastHits = new Queue<PhysXRaycastHit>();
        private static IntPtr defaultPhysXRaycastHit = IntPtr.Zero;

        private static PhysXVec3 physXOrigin = new PhysXVec3(Vector3.zero);
        private static PhysXVec3 physXDirection = new PhysXVec3(Vector3.forward);

        public static PhysXRaycastHit GetRaycastHit() {
            if (raycastHits.Count > 0) {
                return raycastHits.Dequeue();
            }
            else return new PhysXRaycastHit();
        }

        public static void ReleaseRaycastHit(PhysXRaycastHit raycastHit) {
            raycastHits.Enqueue(raycastHit);
        }

        public static bool Fire(Vector3 origin, Vector3 direction, PhysXRaycastHit raycastHit, float distance) {
            physXOrigin.FromVector(origin);
            physXDirection.FromVector(direction);
            bool hit = PhysXSceneManager.FireRaycast(physXOrigin, physXDirection, distance, raycastHit.physXRaycastHit);
            if (hit) raycastHit.PopulateFields();
            return hit;
        }

        public static bool Fire(Vector3 origin, Vector3 direction, PhysXRaycastHit raycastHit) {
            return Fire(origin, direction, raycastHit, float.MaxValue);
        }

        public static bool Fire(Vector3 origin, Vector3 direction, float distance) {
            physXOrigin.FromVector(origin);
            physXDirection.FromVector(direction);
            if (defaultPhysXRaycastHit == IntPtr.Zero) defaultPhysXRaycastHit = PhysXLib.CreateRaycastHit();

            return PhysXSceneManager.FireRaycast(physXOrigin, physXDirection, distance, defaultPhysXRaycastHit);
        }

        public static bool Fire(Vector3 origin, Vector3 direction) {
            return Fire(direction, origin, float.MaxValue);
        }

        public static bool Fire(Vector3 origin, Vector3 direction, PhysXRaycastHit raycastHit, float distance , PhysXCollider.CollisionLayer layers, uint ignoredVehicleId = 0) {
            physXOrigin.FromVector(origin);
            physXDirection.FromVector(direction);
            bool hit = PhysXSceneManager.FireRaycastFiltered(physXOrigin, physXDirection, distance, raycastHit.physXRaycastHit, (uint)layers, 0, 0, ignoredVehicleId);
            if (hit) raycastHit.PopulateFields();
            return hit;
        }

        public static bool Fire(Vector3 origin, Vector3 direction, PhysXRaycastHit raycastHit, PhysXCollider.CollisionLayer layers, uint ignoredVehicleId = 0) {
            return Fire(origin, direction,raycastHit, float.MaxValue, layers, ignoredVehicleId);
        }

        public static bool Fire(Vector3 origin, Vector3 direction, float distance, PhysXCollider.CollisionLayer layers, uint ignoredVehicleId = 0) {
            physXOrigin.FromVector(origin);
            physXDirection.FromVector(direction);
            if (defaultPhysXRaycastHit == IntPtr.Zero) defaultPhysXRaycastHit = PhysXLib.CreateRaycastHit();

            return PhysXSceneManager.FireRaycastFiltered(physXOrigin, physXDirection, distance, defaultPhysXRaycastHit, (uint)layers, 0, 0, ignoredVehicleId);
        }

        public static bool Fire(Vector3 origin, Vector3 direction, PhysXCollider.CollisionLayer layers, uint ignoredVehicleId = 0) {
            return Fire(direction, origin, float.MaxValue, layers, ignoredVehicleId);
        }
    }
}
