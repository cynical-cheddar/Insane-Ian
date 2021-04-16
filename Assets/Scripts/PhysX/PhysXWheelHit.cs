using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

using UnityEngine;

namespace PhysX {
    public class PhysXWheelHit {
        private static Queue<PhysXWheelHit> wheelHits = new Queue<PhysXWheelHit>();

        public static PhysXWheelHit GetWheelHit() {
            if (wheelHits.Count > 0) {
                return wheelHits.Dequeue();
            }

            return new PhysXWheelHit();
        }

        public static void ReleaseWheelHit(PhysXWheelHit wheelHit) {
            wheelHits.Enqueue(wheelHit);
        }


        // public Vector3 normal {
        //     get {
        //         return _normal;
        //     }
        // }
        // private Vector3 _normal;
        // private PhysXVec3 _pxnormal = new PhysXVec3(Vector3.zero);

        public Vector3 point {
            get {
                return _point;
            }
        }
        private Vector3 _point;
        private PhysXVec3 _pxpoint = new PhysXVec3(Vector3.zero);

        public PhysXCollider collider { get; private set; }

        // public float distance { get; private set; }

        // public Transform transform { get; private set; }

        internal void PopulateFields(IntPtr vehicle, int wheelNum) {
            IntPtr hitActor = PhysXLib.GetGroundHitActor(vehicle, wheelNum);
            PhysXBody body = PhysXSceneManager.GetBodyFromPointer(hitActor);

            IntPtr shape = PhysXLib.GetGroundHitShape(vehicle, wheelNum);
            collider = body.GetColliderFromShape(shape);

            PhysXLib.GetGroundHitPosition(vehicle, wheelNum, _pxpoint);
            _pxpoint.ToVector(ref _point);
        }
    }
}
