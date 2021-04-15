//#include <stdio.h>
#include "physx/include/PxPhysicsAPI.h"
#include "physx/source/foundation/include/PsFoundation.h"

#include <iostream>
#include <string>
#include <vector>

#ifdef ON_MAC
#define EXPORT_FUNC
#else
#define EXPORT_FUNC __declspec(dllexport)
#endif

#define CONTACT_BEGIN   (1)
#define CONTACT_SUSTAIN (1 << 1)
#define CONTACT_END     (1 << 2)
#define TRIGGER_BEGIN   (1 << 3)
#define TRIGGER_SUSTAIN (1 << 4)
#define TRIGGER_END     (1 << 5)

#define WHEEL_LAYER (1 << 1)

extern "C" {
    typedef void(*DebugLog)(const char* stringPtr, int length);
    typedef void(*CollisionCallback)(const physx::PxContactPairHeader* pairHeader, const physx::PxContactPair* pairs, physx::PxU32 nbPairs, const physx::PxActor* self, bool isEnter, bool isStay, bool isExit);
    typedef void(*TriggerCallback)(const physx::PxActor* other, const physx::PxShape* otherShape, const physx::PxActor* self, bool isEnter, bool isExit);

    class MyErrorCallback : public physx::PxErrorCallback {
    public:
        MyErrorCallback();
        ~MyErrorCallback();

        virtual void reportError(physx::PxErrorCode::Enum code, const char* message, const char* file, int line);
    };

    class CollisionHandler : public physx::PxSimulationEventCallback {
    public:
        CollisionHandler();
        ~CollisionHandler();

        void onConstraintBreak(physx::PxConstraintInfo *constraints, physx::PxU32 count);
        void onWake(physx::PxActor **actors, physx::PxU32 count);
        void onSleep(physx::PxActor **actors, physx::PxU32 count);
        void onContact(const physx::PxContactPairHeader &pairHeader, const physx::PxContactPair *pairs, physx::PxU32 nbPairs);
        void onTrigger(physx::PxTriggerPair *pairs, physx::PxU32 count);
        void onAdvance(const physx::PxRigidBody *const *bodyBuffer, const physx::PxTransform *poseBuffer, const physx::PxU32 count);
    };

    physx::PxQueryHitType::Enum WheelSceneQueryPreFilterBlocking(physx::PxFilterData filterData0, physx::PxFilterData filterData1,
                                                                 const void* constantBlock, physx::PxU32 constantBlockSize,
                                                                 physx::PxHitFlags& queryFlags);

    class RaycastHitHandler : public physx::PxRaycastCallback {
    public:
        RaycastHitHandler(physx::PxRaycastHit* hitBuffer, physx::PxU32 bufferSize);

        physx::PxAgain processTouches(const physx::PxRaycastHit* hits, physx::PxU32 hitCount);
    };

    struct SceneUserData
    {
        SceneUserData() : wheelCount(0),
                          suspensionBatchQuery(NULL) {

        }

        std::vector<physx::PxVehicleWheels*> vehicles;
        physx::PxU32 wheelCount;
        physx::PxBatchQuery* suspensionBatchQuery;
        std::vector<physx::PxVehicleWheelQueryResult> queryResults;
        std::vector<physx::PxRaycastQueryResult> raycastResults;
        std::vector<physx::PxRaycastHit> raycastHits;
    };

    struct ActorUserData
    {
        ActorUserData() : scene(NULL) {
            
        }

        physx::PxScene* scene;
        std::vector<physx::PxWheelQueryResult> queryResults;
    };

    struct ShapeUserData
    {
        ShapeUserData()
            : isWheel(false),
            wheelId(0xffffffff)
        {
        }

        bool isWheel;
        physx::PxU32 wheelId;
    };

    EXPORT_FUNC void RegisterDebugLog(DebugLog dl);

    EXPORT_FUNC void SetupFoundation();

    EXPORT_FUNC void CreatePhysics(bool trackAllocations);

    EXPORT_FUNC void CreateVehicleEnvironment(physx::PxVec3* up, physx::PxVec3* forward);

    EXPORT_FUNC physx::PxScene* CreateScene(physx::PxVec3* gravity);

    EXPORT_FUNC physx::PxMaterial* CreateMaterial(float staticFriction, float dynamicFriction, float restitution);

    EXPORT_FUNC physx::PxGeometry* CreateBoxGeometry(float halfX, float halfY, float halfZ);
    EXPORT_FUNC physx::PxGeometry* CreateSphereGeometry(float radius);
    EXPORT_FUNC std::vector<physx::PxVec3>* CreateVectorArray();
    EXPORT_FUNC void AddVectorToArray(std::vector<physx::PxVec3>* vectorArray, physx::PxVec3* vector);
    EXPORT_FUNC physx::PxGeometry* CreateConvexMeshGeometry(std::vector<physx::PxVec3>* vertexArray);
    EXPORT_FUNC physx::PxGeometry* CreateMeshGeometry(std::vector<physx::PxVec3>* vertexArray, physx::PxU32* triIndices, physx::PxU32 triCount);

    EXPORT_FUNC physx::PxTransform* CreateTransform(physx::PxVec3* pos, physx::PxQuat* rot);

    EXPORT_FUNC physx::PxShape* CreateShape(physx::PxGeometry* geometry, physx::PxMaterial* mat, physx::PxReal contactOffset);
    EXPORT_FUNC void SetShapeLocalTransform(physx::PxShape* shape, physx::PxTransform* transform);
    EXPORT_FUNC void SetShapeSimulationFlag(physx::PxShape* shape, bool value);
    EXPORT_FUNC void SetShapeTriggerFlag(physx::PxShape* shape, bool value);
    EXPORT_FUNC void SetShapeSceneQueryFlag(physx::PxShape* shape, bool value);

    EXPORT_FUNC physx::PxRigidDynamic* CreateDynamicRigidBody(physx::PxTransform* pose);
    EXPORT_FUNC physx::PxRigidStatic* CreateStaticRigidBody(physx::PxTransform* pose);

    EXPORT_FUNC void SetCollisionFilterData(physx::PxShape* shape, physx::PxU32 w0, physx::PxU32 w1, physx::PxU32 w2, physx::PxU32 w3);

    EXPORT_FUNC int AttachShapeToRigidBody(physx::PxShape* shape, physx::PxRigidActor* body);

    EXPORT_FUNC physx::PxVehicleWheelData* CreateWheelData();
    EXPORT_FUNC void SetWheelRadius(physx::PxVehicleWheelData* wheel, physx::PxReal radius);
    EXPORT_FUNC void SetWheelWidth(physx::PxVehicleWheelData* wheel, physx::PxReal width);
    EXPORT_FUNC void SetWheelMass(physx::PxVehicleWheelData* wheel, physx::PxReal mass);
    EXPORT_FUNC void SetWheelMomentOfInertia(physx::PxVehicleWheelData* wheel, physx::PxReal momentOfInertia);
    EXPORT_FUNC void SetWheelDampingRate(physx::PxVehicleWheelData* wheel, physx::PxReal dampingRate);

    EXPORT_FUNC physx::PxVehicleTireData* CreateTireData();
    EXPORT_FUNC void SetTireLateralStiffnessMaxLoad(physx::PxVehicleTireData* tire, physx::PxReal maxLoad);
    EXPORT_FUNC void SetTireMaxLateralStiffness(physx::PxVehicleTireData* tire, physx::PxReal maxStiffness);
    EXPORT_FUNC void SetTireLongitudinalStiffnessScale(physx::PxVehicleTireData* tire, physx::PxReal stiffnessScale);
    EXPORT_FUNC void SetTireBaseFriction(physx::PxVehicleTireData* tire, physx::PxReal friction);
    EXPORT_FUNC void SetTireMaxFrictionSlipPoint(physx::PxVehicleTireData* tire, physx::PxReal slipPoint);
    EXPORT_FUNC void SetTireMaxFriction(physx::PxVehicleTireData* tire, physx::PxReal friction);
    EXPORT_FUNC void SetTirePlateuxSlipPoint(physx::PxVehicleTireData* tire, physx::PxReal slipPoint);
    EXPORT_FUNC void SetTirePlateuxFriction(physx::PxVehicleTireData* tire, physx::PxReal friction);

    EXPORT_FUNC physx::PxVehicleSuspensionData* CreateSuspensionData();
    EXPORT_FUNC void SetSuspensionSpringStrength(physx::PxVehicleSuspensionData* suspension, physx::PxReal strength);
    EXPORT_FUNC void SetSuspensionSpringDamper(physx::PxVehicleSuspensionData* suspension, physx::PxReal damper);
    EXPORT_FUNC void SetSuspensionMaxCompression(physx::PxVehicleSuspensionData* suspension, physx::PxReal maxCompression);
    EXPORT_FUNC void SetSuspensionMaxDroop(physx::PxVehicleSuspensionData* suspension, physx::PxReal maxDroop);
    EXPORT_FUNC void SetSuspensionSprungMasses(physx::PxVehicleSuspensionData** suspensions, physx::PxU32 wheelCount, std::vector<physx::PxVec3>* wheelPositions, physx::PxVec3* centreOfMass, physx::PxReal mass);

    EXPORT_FUNC physx::PxVehicleWheelsSimData* CreateWheelSimData(physx::PxU32 wheelCount);
    EXPORT_FUNC void SetWheelSimWheelData(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVehicleWheelData* wheel);
    EXPORT_FUNC void SetWheelSimTireData(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVehicleTireData* tire);
    EXPORT_FUNC void SetWheelSimSuspensionData(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVehicleSuspensionData* suspension, physx::PxVec3* down);
    EXPORT_FUNC void SetWheelSimWheelCentre(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVec3* centre);
    EXPORT_FUNC void SetWheelSimForceAppPoint(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVec3* point);
    EXPORT_FUNC void SetWheelSimQueryFilterData(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxU32 w0, physx::PxU32 w1, physx::PxU32 w2, physx::PxU32 w3);
    EXPORT_FUNC void SetWheelSimWheelShape(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxU32 shapeNum);

    EXPORT_FUNC physx::PxVehicleNoDrive* CreateVehicleFromRigidBody(physx::PxRigidDynamic* body, physx::PxVehicleWheelsSimData* wheelSimData);

    EXPORT_FUNC physx::PxVehicleWheelsSimData* GetWheelSimData(physx::PxVehicleWheels* vehicle);
    EXPORT_FUNC physx::PxVehicleWheelsDynData* GetWheelDynData(physx::PxVehicleWheels* vehicle);
    EXPORT_FUNC void SetWheelDynTireData(physx::PxVehicleWheelsDynData* wheelDynData, physx::PxU32 wheelNum, physx::PxVehicleTireData* tire);

    EXPORT_FUNC void SetWheelSteer(physx::PxVehicleNoDrive* vehicle, physx::PxU32 wheelNum, physx::PxReal steerAngle);
    EXPORT_FUNC void SetWheelDrive(physx::PxVehicleNoDrive* vehicle, physx::PxU32 wheelNum, physx::PxReal driveTorque);
    EXPORT_FUNC void SetWheelBrake(physx::PxVehicleNoDrive* vehicle, physx::PxU32 wheelNum, physx::PxReal brakeTorque);

    EXPORT_FUNC void RegisterCollisionCallback(CollisionCallback onCollisionCallback);
    EXPORT_FUNC void RegisterTriggerCallback(TriggerCallback onTriggerCallback);

    EXPORT_FUNC void SetRigidBodyMassAndInertia(physx::PxRigidBody* body, float density, const physx::PxVec3* massLocalPose = NULL);
    EXPORT_FUNC void SetRigidBodyMassPose(physx::PxRigidBody* body, physx::PxTransform* pose);
    EXPORT_FUNC void SetRigidBodyDamping(physx::PxRigidBody* body, float linear, float angular);

    EXPORT_FUNC void UpdateVehicleCentreOfMass(physx::PxTransform* oldCentre, physx::PxTransform* newCentre, physx::PxVehicleWheels* vehicle);

    EXPORT_FUNC void SetRigidBodyFlag(physx::PxRigidBody* body, physx::PxRigidBodyFlag::Enum flag, bool value);
    EXPORT_FUNC void SetRigidBodyDominanceGroup(physx::PxRigidBody* body, physx::PxDominanceGroup group);
    EXPORT_FUNC void SetRigidBodyMaxDepenetrationVelocity(physx::PxRigidBody* body, physx::PxReal velocity);

    EXPORT_FUNC void AddActorToScene(physx::PxScene* scene, physx::PxActor* actor);

    EXPORT_FUNC void StepPhysics(physx::PxScene* scene, float time);

    EXPORT_FUNC physx::PxTransform* GetCentreOfMass(physx::PxRigidBody* body);

    EXPORT_FUNC void GetPosition(physx::PxRigidActor* actor, physx::PxVec3* position);
    EXPORT_FUNC void GetRotation(physx::PxRigidActor* actor, physx::PxQuat* rotation);

    EXPORT_FUNC void GetLinearVelocity(physx::PxRigidBody* rigidBody, physx::PxVec3* velocity);
    EXPORT_FUNC void GetAngularVelocity(physx::PxRigidBody* rigidBody, physx::PxVec3* velocity);

    EXPORT_FUNC void AddForce(physx::PxRigidBody* rigidBody, physx::PxVec3* force, physx::PxForceMode::Enum forceMode);
    EXPORT_FUNC void AddForceAtPosition(physx::PxRigidBody* rigidBody, physx::PxVec3* force, physx::PxVec3* position, physx::PxForceMode::Enum forceMode);
    EXPORT_FUNC void AddTorque(physx::PxRigidBody* rigidBody, physx::PxVec3* torque, physx::PxForceMode::Enum forceMode);

    EXPORT_FUNC physx::PxActor* GetPairHeaderActor(physx::PxContactPairHeader* header, int actorNum);
    EXPORT_FUNC physx::PxShape* GetContactPairShape(physx::PxContactPair* pairs, int i, int actor);
    EXPORT_FUNC physx::PxContactStreamIterator* GetContactPointIterator(physx::PxContactPair* pairs, int i);
    EXPORT_FUNC bool NextContactPatch(physx::PxContactStreamIterator* iter);
    EXPORT_FUNC bool NextContactPoint(physx::PxContactStreamIterator* iter);
    EXPORT_FUNC void GetContactPointData(physx::PxContactStreamIterator* iter, int j, physx::PxContactPair* pairs, int i, physx::PxVec3* point, physx::PxVec3* normal, physx::PxVec3* impulse);

    EXPORT_FUNC physx::PxReal GetSuspensionCompression(physx::PxVehicleWheels* vehicle, physx::PxU32 wheelNum);
    EXPORT_FUNC void GetWheelTransform(physx::PxVehicleWheels* vehicle, physx::PxU32 wheelNum, physx::PxVec3* position, physx::PxQuat* rotation);

    EXPORT_FUNC void GetTransformComponents(physx::PxTransform* transform, physx::PxVec3* position, physx::PxQuat* rotation);

    EXPORT_FUNC physx::PxRaycastCallback* CreateRaycastHit();
    EXPORT_FUNC bool FireRaycast(physx::PxScene* scene, physx::PxVec3* origin, physx::PxVec3* direction, physx::PxReal distance, physx::PxRaycastCallback* raycastHit);
    EXPORT_FUNC void GetRaycastHitNormal(physx::PxRaycastCallback* raycastHit, physx::PxVec3* normal);
    EXPORT_FUNC void GetRaycastHitPoint(physx::PxRaycastCallback* raycastHit, physx::PxVec3* point);
    EXPORT_FUNC physx::PxShape* GetRaycastHitShape(physx::PxRaycastCallback* raycastHit);
    EXPORT_FUNC physx::PxActor* GetRaycastHitActor(physx::PxRaycastCallback* raycastHit);
    EXPORT_FUNC physx::PxReal GetRaycastHitDistance(physx::PxRaycastCallback* raycastHit);
    EXPORT_FUNC void DestroyRaycastHit(physx::PxRaycastCallback* raycastHit);
}
