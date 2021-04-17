using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysX {
    public class PhysXContactPoint {
        private static Queue<PhysXContactPoint> contactPoints = new Queue<PhysXContactPoint>();

        internal IntPtr colliderShape;

        public Vector3 impulse { get; internal set; }

        //  USED
        public Vector3 normal { get; internal set; }

        //  USED
        public Vector3 point { get; internal set; }

        //  USED
        public PhysXCollider collider { get; internal set; }

        internal static PhysXContactPoint GetContactPoint() {
            if (contactPoints.Count == 0) return new PhysXContactPoint();

            return contactPoints.Dequeue();
        }

        internal static void ReleaseContactPoint(PhysXContactPoint contactPoint) {
            contactPoints.Enqueue(contactPoint);
        }
    }
}