using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

using UnityEngine;

namespace PhysX {
    public class PhysXRaycastHit {
        internal IntPtr physXRaycastHit = IntPtr.Zero;

        internal PhysXRaycastHit() {
            physXRaycastHit = PhysXLib.CreateRaycastHit();
        }

        ~PhysXRaycastHit() {
            PhysXLib.DestroyRaycastHit(physXRaycastHit);
        }

        public Vector3 normal {
            get {
                return _normal;
            }
        }
        private Vector3 _normal;
        private PhysXVec3 _pxnormal = new PhysXVec3(Vector3.zero);

        public Vector3 point {
            get {
                return _point;
            }
        }
        private Vector3 _point;
        private PhysXVec3 _pxpoint = new PhysXVec3(Vector3.zero);

        public PhysXCollider collider { get; private set; }

        public float distance { get; private set; }

        public Transform transform { get; private set; }

        internal void PopulateFields() {
            IntPtr hitActor = PhysXLib.GetRaycastHitActor(physXRaycastHit);
            PhysXBody body = PhysXSceneManager.GetBodyFromPointer(hitActor);

            transform = body.transform;

            IntPtr shape = PhysXLib.GetRaycastHitShape(physXRaycastHit);
            collider = body.GetColliderFromShape(shape);

            distance = PhysXLib.GetRaycastHitDistance(physXRaycastHit);

            PhysXLib.GetRaycastHitNormal(physXRaycastHit, _pxnormal);
            _pxnormal.ToVector(ref _normal);

            PhysXLib.GetRaycastHitNormal(physXRaycastHit, _pxpoint);
            _pxpoint.ToVector(ref _point);
        }
    }
}
