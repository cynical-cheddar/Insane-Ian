using System;
using System.Collections.Generic;

using UnityEngine;

namespace PhysX {
    public class PhysXCollision {
        private static Queue<PhysXCollision> collisions = new Queue<PhysXCollision>();

        internal IntPtr self;
        internal bool isEnter;
        internal bool isStay;
        internal bool isExit;

        private IntPtr otherActor;

        private List<PhysXContactPoint> contactPoints = new List<PhysXContactPoint>();

        public PhysXCollider collider {
            get {
                return contactPoints[0].collider;
            }
        }

        public int contactCount {
            get {
                return contactPoints.Count;
            }
        }

        public GameObject gameObject { get; private set; }

        public Vector3 impulse { get; private set; }

        public Vector3 relativeVelocity {
            get {
                throw new Exception("I couldnt be bothered");
            }
        }

        public PhysXRigidBody rigidBody { get; private set; }

        public Transform transform { get; private set; }

        public PhysXContactPoint GetContact(int index) {
            return contactPoints[index];
        }

        public PhysXContactPoint[] GetContacts() {
            return contactPoints.ToArray();
        }

        private PhysXVec3 pointPos = new PhysXVec3(Vector3.zero);
        private PhysXVec3 pointNormal = new PhysXVec3(Vector3.zero);
        private PhysXVec3 pointImpulse = new PhysXVec3(Vector3.zero);
        internal void FromPhysXInternalCollision(IntPtr pairHeader, IntPtr pairs, int pairCount, IntPtr self, bool isEnter, bool isStay, bool isExit) {
            this.self = self;
            this.isEnter = isEnter;
            this.isStay = isStay;
            this.isExit = isExit;

            otherActor = PhysXLib.GetPairHeaderActor(pairHeader, 0);

            int otherNum = 0;
            if (otherActor == self) {
                otherNum = 1;
                otherActor = PhysXLib.GetPairHeaderActor(pairHeader, otherNum);
            }

            impulse = Vector3.zero;

            for (int i = 0; i < pairCount; i++) {
                IntPtr colliderShape = PhysXLib.GetContactPairShape(pairs, i, otherNum);

                IntPtr iter = PhysXLib.GetContactPointIterator(pairs, i);
                int j = 0;

                while (PhysXLib.NextContactPatch(iter)) {
                    while (PhysXLib.NextContactPoint(iter)) {
                        PhysXLib.GetContactPointData(iter, j, pairs, i, pointPos, pointNormal, pointImpulse);

                        PhysXContactPoint contactPoint = PhysXContactPoint.GetContactPoint();
                        contactPoint.colliderShape = colliderShape;
                        contactPoint.point = pointPos.ToVector();
                        contactPoint.normal = pointNormal.ToVector();
                        contactPoint.impulse = pointImpulse.ToVector();
                        contactPoints.Add(contactPoint);

                        impulse += contactPoint.impulse;

                        j++;
                    }
                }
            }

            impulse /= contactCount;

            Debug.Log(contactPoints.Count);
        }

        internal void PopulateWithUnityObjects(Dictionary<IntPtr, PhysXBody> bodies) {
            PhysXBody body = bodies[otherActor];
            rigidBody = body as PhysXRigidBody;
            gameObject = body.gameObject;
            transform = body.transform;

            IntPtr currentShape = IntPtr.Zero;
            PhysXCollider currentCollider = null;

            foreach (PhysXContactPoint contactPoint in contactPoints) {
                if (contactPoint.colliderShape != currentShape) {
                    currentShape = contactPoint.colliderShape;
                    currentCollider = body.GetColliderFromShape(currentShape);
                }

                contactPoint.collider = currentCollider;
            }
        }

        internal static PhysXCollision GetCollision() {
            if (collisions.Count == 0) return new PhysXCollision();

            return collisions.Dequeue();
        }

        internal static void ReleaseCollision(PhysXCollision collision) {
            foreach (PhysXContactPoint contactPoint in collision.contactPoints) {
                PhysXContactPoint.ReleaseContactPoint(contactPoint);
            }
            collision.contactPoints.Clear();
            collisions.Enqueue(collision);
        }
    }
}