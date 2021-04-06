using System;
using System.Runtime.InteropServices;
using AOT;

using UnityEngine;

namespace PhysX {
    public class PhysXLib {
        private delegate void PhysXDebugLog(IntPtr stringPtr, int length);
        public delegate void CollisionCallback(IntPtr pairHeader, IntPtr pairs, int pairCount, IntPtr self, bool isEnter, bool isStay, bool isExit);

        public enum PhysXRigidBodyFlag {
            eKINEMATIC = (1<<0),
            eUSE_KINEMATIC_TARGET_FOR_SCENE_QUERIES = (1<<1),
            eENABLE_CCD = (1<<2),
            eENABLE_CCD_FRICTION = (1<<3),
            eENABLE_POSE_INTEGRATION_PREVIEW = (1 << 4),
            eENABLE_SPECULATIVE_CCD = (1 << 5),
            eENABLE_CCD_MAX_CONTACT_IMPULSE = (1 << 6),
            eRETAIN_ACCELERATIONS = (1<<7)
        }

        #if UNITY_WEBGL
            #if UNITY_EDITOR
                private const string dllName = "PhysXWrapper";
            #else
                private const string dllName = "__Internal";
            #endif
        #else
            private const string dllName = "PhysXWrapper";
        #endif

        [DllImport(dllName)]
        private static extern void RegisterDebugLog(PhysXDebugLog dl);

        [DllImport(dllName)]
        private static extern void SetupFoundation();

        [DllImport(dllName)]
        private static extern void CreatePhysics(bool trackAllocations);

        [DllImport(dllName)]
        public static extern IntPtr CreateScene([In] PhysXVec3 gravity);

        [DllImport(dllName)]
        public static extern IntPtr CreateMaterial(float staticFriction, float dynamicFriction, float restitution);

        [DllImport(dllName)]
        private static extern IntPtr CreateStaticPlane([In] PhysXVec3 point, [In] PhysXVec3 normal, IntPtr mat);

        [DllImport(dllName)]
        public static extern IntPtr CreateBoxGeometry(float halfX, float halfY, float halfZ);

        [DllImport(dllName)]
        public static extern IntPtr CreateSphereGeometry(float radius);

        [DllImport(dllName)]
        public static extern IntPtr CreateShape(IntPtr geometry, IntPtr mat);

        [DllImport(dllName)]
        public static extern IntPtr CreateTransform([In] PhysXVec3 pos, [In] PhysXQuat rot);

        [DllImport(dllName)]
        public static extern IntPtr CreateDynamicRigidBody(IntPtr pose);

        [DllImport(dllName)]
        public static extern IntPtr SetCollisionFilterData(IntPtr shape, UInt32 w0, UInt32 w1, UInt32 w2, UInt32 w3);

        [DllImport(dllName)]
        public static extern IntPtr AttachShapeToRigidBody(IntPtr shape, IntPtr body);

        [DllImport(dllName)]
        public static extern void RegisterCollisionCallback(CollisionCallback callback);

        [DllImport(dllName)]
        public static extern IntPtr SetRigidBodyMassAndInertia(IntPtr body, float density, [In] PhysXVec3 massLocalPose = null);

        [DllImport(dllName)]
        public static extern IntPtr SetRigidBodyDamping(IntPtr body, float linear, float angular);

        [DllImport(dllName)]
        public static extern IntPtr SetRigidBodyFlag(IntPtr body, PhysXRigidBodyFlag flag, bool value);

        [DllImport(dllName)]
        public static extern void AddActorToScene(IntPtr scene, IntPtr actor);

        [DllImport(dllName)]
        public static extern void StepPhysics(IntPtr scene, float time);

        [DllImport(dllName)]
        public static extern void GetPosition(IntPtr actor, [Out] PhysXVec3 position);

        [DllImport(dllName)]
        public static extern void GetRotation(IntPtr actor, [Out] PhysXQuat rotation);

        [DllImport(dllName)]
        public static extern void GetLinearVelocity(IntPtr rigidBody, [Out] PhysXVec3 velocity);

        [DllImport(dllName)]
        public static extern void GetAngularVelocity(IntPtr rigidBody, [Out] PhysXVec3 velocity);

        [DllImport(dllName)]
        public static extern void AddForce(IntPtr rigidBody, [In] PhysXVec3 force, int forceMode);

        [DllImport(dllName)]
        public static extern void AddForceAtPosition(IntPtr rigidBody, [In] PhysXVec3 force, [In] PhysXVec3 position, int forceMode);

        [DllImport(dllName)]
        public static extern void AddTorque(IntPtr rigidBody, [In] PhysXVec3 torque, int forceMode);

        [DllImport(dllName)]
        public static extern IntPtr GetPairHeaderActor(IntPtr header, int actorNum);

        [DllImport(dllName)]
        public static extern IntPtr GetContactPairShape(IntPtr pairs, int i, int actor);

        [DllImport(dllName)]
        public static extern IntPtr GetContactPointIterator(IntPtr pairs, int i);

        [DllImport(dllName)]
        public static extern bool NextContactPatch(IntPtr iter);

        [DllImport(dllName)]
        public static extern bool NextContactPoint(IntPtr iter);

        [DllImport(dllName)]
        public static extern void GetContactPointData(IntPtr iter, int j, IntPtr pairs, int i, [Out] PhysXVec3 point, [Out] PhysXVec3 normal, [Out] PhysXVec3 impulse);

        [MonoPInvokeCallback(typeof(PhysXDebugLog))]
        private static void HandleDebugLog(IntPtr stringPtr, int length) {
            string str = Marshal.PtrToStringAnsi(stringPtr, length);
            Debug.Log(str);
        }

        public static void SetupPhysX() {
            RegisterDebugLog(HandleDebugLog);
            SetupFoundation();
            CreatePhysics(true);
            // IntPtr mat = CreateMaterial(0.5f, 0.5f, 0.6f);
            // IntPtr plane = CreateStaticPlane(new PhysXVec3(Vector3.zero), new PhysXVec3(Vector3.up), mat);
            // AddActorToScene(scene, plane);
            // IntPtr boxGeom = CreateBoxGeometry(2, 2, 2);
            // IntPtr box = CreateShape(boxGeom, mat);
        }
    }
}
