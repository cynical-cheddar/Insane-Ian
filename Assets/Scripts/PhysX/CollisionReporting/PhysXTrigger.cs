using System;
using System.Collections.Generic;

using UnityEngine;

namespace PhysX {
    public class PhysXTrigger {
        private static Queue<PhysXTrigger> triggers = new Queue<PhysXTrigger>();

        internal IntPtr self;
        internal IntPtr colliderShape;
        internal bool isEnter;
        internal bool isExit;

        private IntPtr otherActor;

        public PhysXCollider collider { get; private set; }

        internal void FromPhysXInternalTrigger(IntPtr other, IntPtr otherShape, IntPtr self, bool isEnter, bool isExit) {
            this.self = self;
            this.isEnter = isEnter;
            this.isExit = isExit;

            otherActor = other;
            colliderShape = otherShape;
        }

        internal void PopulateWithUnityObjects(Dictionary<IntPtr, PhysXBody> rigidBodies) {
            PhysXBody body = rigidBodies[otherActor];

            collider = body.GetColliderFromShape(colliderShape);
        }

        internal static PhysXTrigger GetTrigger() {
            if (triggers.Count == 0) return new PhysXTrigger();

            return triggers.Dequeue();
        }

        internal static void ReleaseTrigger(PhysXTrigger trigger) {
            triggers.Enqueue(trigger);
        }
    }
}