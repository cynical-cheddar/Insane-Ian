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
        private static extern void CreateVehicleEnvironment([In] PhysXVec3 up, [In] PhysXVec3 forward);

        [DllImport(dllName)]
        public static extern IntPtr CreateScene([In] PhysXVec3 gravity);

        [DllImport(dllName)]
        public static extern IntPtr CreateMaterial(float staticFriction, float dynamicFriction, float restitution);

        [DllImport(dllName)]
        public static extern IntPtr CreateBoxGeometry(float halfX, float halfY, float halfZ);

        [DllImport(dllName)]
        public static extern IntPtr CreateSphereGeometry(float radius);

        [DllImport(dllName)]
        public static extern IntPtr CreateVectorArray();

        [DllImport(dllName)]
	    public static extern void AddVectorToArray(IntPtr vectorArray, [In] PhysXVec3 vector);

        [DllImport(dllName)]
	    public static extern IntPtr CreateConvexMeshGeometry(IntPtr vertexArray);

        [DllImport(dllName)]
        public static extern IntPtr CreateMeshGeometry(IntPtr vertexArray, [In] int[] triIndices, int triCount);

        [DllImport(dllName)]
        public static extern IntPtr CreateTransform([In] PhysXVec3 pos, [In] PhysXQuat rot);

        [DllImport(dllName)]
        public static extern IntPtr CreateShape(IntPtr geometry, IntPtr mat);

        [DllImport(dllName)]
        public static extern void SetShapeLocalTransform(IntPtr shape, IntPtr transform);

        [DllImport(dllName)]
        public static extern IntPtr CreateDynamicRigidBody(IntPtr pose);

        [DllImport(dllName)]
        public static extern IntPtr SetCollisionFilterData(IntPtr shape, UInt32 w0, UInt32 w1, UInt32 w2, UInt32 w3);

        [DllImport(dllName)]
        public static extern int AttachShapeToRigidBody(IntPtr shape, IntPtr body);

        [DllImport(dllName)]
        public static extern IntPtr CreateWheelData();

        [DllImport(dllName)]
        public static extern void SetWheelRadius(IntPtr wheel, float radius);

        [DllImport(dllName)]
        public static extern void SetWheelWidth(IntPtr wheel, float width);

        [DllImport(dllName)]
        public static extern void SetWheelMass(IntPtr wheel, float mass);

        [DllImport(dllName)]
        public static extern void SetWheelMomentOfInertia(IntPtr wheel, float momentOfInertia);

        [DllImport(dllName)]
        public static extern void SetWheelDampingRate(IntPtr wheel, float dampingRate);

        [DllImport(dllName)]
        public static extern IntPtr CreateTireData();

        [DllImport(dllName)]
        public static extern void SetTireLateralStiffnessMaxLoad(IntPtr tire, float maxLoad);

        [DllImport(dllName)]
        public static extern void SetTireMaxLateralStiffness(IntPtr tire, float maxStiffness);

        [DllImport(dllName)]
        public static extern void SetTireLongitudinalStiffnessScale(IntPtr tire, float stiffnessScale);

        [DllImport(dllName)]
        public static extern void SetTireBaseFriction(IntPtr tire, float friction);

        [DllImport(dllName)]
        public static extern void SetTireMaxFrictionSlipPoint(IntPtr tire, float slipPoint);

        [DllImport(dllName)]
        public static extern void SetTireMaxFriction(IntPtr tire, float friction);

        [DllImport(dllName)]
        public static extern void SetTirePlateuxSlipPoint(IntPtr tire, float slipPoint);

        [DllImport(dllName)]
        public static extern void SetTirePlateuxFriction(IntPtr tire, float friction);

        [DllImport(dllName)]
        public static extern IntPtr CreateSuspensionData();

        [DllImport(dllName)]
        public static extern void SetSuspensionSpringStrength(IntPtr suspension, float strength);

        [DllImport(dllName)]
        public static extern void SetSuspensionSpringDamper(IntPtr suspension, float damper);

        [DllImport(dllName)]
        public static extern void SetSuspensionMaxCompression(IntPtr suspension, float maxCompression);

        [DllImport(dllName)]
        public static extern void SetSuspensionMaxDroop(IntPtr suspension, float maxDroop);

        [DllImport(dllName)]
        public static extern void SetSuspensionSprungMasses(IntPtr[] suspensions, int wheelCount, IntPtr wheelPositions, [In] PhysXVec3 centreOfMass, float mass);

        [DllImport(dllName)]
        public static extern IntPtr CreateWheelSimData(int wheelCount);

        [DllImport(dllName)]
        public static extern void SetWheelSimWheelData(IntPtr wheelSimData, int wheelNum, IntPtr wheel);

        [DllImport(dllName)]
        public static extern void SetWheelSimTireData(IntPtr wheelSimData, int wheelNum, IntPtr tire);

        [DllImport(dllName)]
        public static extern void SetWheelSimSuspensionData(IntPtr wheelSimData, int wheelNum, IntPtr suspension, [In] PhysXVec3 down);

        [DllImport(dllName)]
        public static extern void SetWheelSimWheelCentre(IntPtr wheelSimData, int wheelNum, [In] PhysXVec3 centre);

        [DllImport(dllName)]
        public static extern void SetWheelSimForceAppPoint(IntPtr wheelSimData, int wheelNum, [In] PhysXVec3 point);

        [DllImport(dllName)]
        public static extern void SetWheelSimQueryFilterData(IntPtr wheelSimData, int wheelNum, int w0, int w1, int w2, int w3);

        [DllImport(dllName)]
        public static extern void SetWheelSimWheelShape(IntPtr wheelSimData, int wheelNum, int shapeNum);

        [DllImport(dllName)]
        public static extern IntPtr CreateVehicleFromRigidBody(IntPtr body, IntPtr wheelSimData);

        [DllImport(dllName)]
	    public static extern IntPtr GetWheelSimData(IntPtr vehicle);

        [DllImport(dllName)]
	    public static extern IntPtr GetWheelDynData(IntPtr vehicle);

        [DllImport(dllName)]
	    public static extern void SetWheelDynTireData(IntPtr wheelDynData, int wheelNum, IntPtr tire);

        [DllImport(dllName)]
        public static extern void RegisterCollisionCallback(CollisionCallback callback);

        [DllImport(dllName)]
        public static extern IntPtr SetRigidBodyMassAndInertia(IntPtr body, float mass, [In] PhysXVec3 massLocalPose = null);

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
            CreateVehicleEnvironment(new PhysXVec3(Vector3.up), new PhysXVec3(Vector3.forward));
            // IntPtr mat = CreateMaterial(0.5f, 0.5f, 0.6f);
            // IntPtr plane = CreateStaticPlane(new PhysXVec3(Vector3.zero), new PhysXVec3(Vector3.up), mat);
            // AddActorToScene(scene, plane);
            // IntPtr boxGeom = CreateBoxGeometry(2, 2, 2);
            // IntPtr box = CreateShape(boxGeom, mat);
        }
    }
}
