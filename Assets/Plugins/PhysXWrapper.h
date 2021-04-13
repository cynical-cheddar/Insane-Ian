//#include <stdio.h>
#include "physx/include/PxPhysicsAPI.h"
#include "physx/source/foundation/include/PsFoundation.h"

#include <iostream>
#include <string>
#include <vector>

#define CONTACT_BEGIN   (1)
#define CONTACT_SUSTAIN (1 << 1)
#define CONTACT_END     (1 << 2)
#define TRIGGER_BEGIN   (1 << 3)
#define TRIGGER_SUSTAIN (1 << 4)
#define TRIGGER_END     (1 << 5)

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

    void RegisterDebugLog(DebugLog dl);

    void SetupFoundation();

    void CreatePhysics(bool trackAllocations);

    void CreateVehicleEnvironment(physx::PxVec3* up, physx::PxVec3* forward);

    physx::PxScene* CreateScene(physx::PxVec3* gravity);

    physx::PxMaterial* CreateMaterial(float staticFriction, float dynamicFriction, float restitution);

    physx::PxGeometry* CreateBoxGeometry(float halfX, float halfY, float halfZ);
    physx::PxGeometry* CreateSphereGeometry(float radius);
    std::vector<physx::PxVec3>* CreateVectorArray();
	void AddVectorToArray(std::vector<physx::PxVec3>* vectorArray, physx::PxVec3* vector);
	physx::PxGeometry* CreateConvexMeshGeometry(std::vector<physx::PxVec3>* vertexArray);
    physx::PxGeometry* CreateMeshGeometry(std::vector<physx::PxVec3>* vertexArray, physx::PxU32* triIndices, physx::PxU32 triCount);

    physx::PxTransform* CreateTransform(physx::PxVec3* pos, physx::PxQuat* rot);

    physx::PxShape* CreateShape(physx::PxGeometry* geometry, physx::PxMaterial* mat, physx::PxReal contactOffset);
    void SetShapeLocalTransform(physx::PxShape* shape, physx::PxTransform* transform);
	void SetShapeSimulationFlag(physx::PxShape* shape, bool value);
	void SetShapeTriggerFlag(physx::PxShape* shape, bool value);
	void SetShapeSceneQueryFlag(physx::PxShape* shape, bool value);

    physx::PxRigidDynamic* CreateDynamicRigidBody(physx::PxTransform* pose);
    physx::PxRigidStatic* CreateStaticRigidBody(physx::PxTransform* pose);

    void SetCollisionFilterData(physx::PxShape* shape, physx::PxU32 w0, physx::PxU32 w1, physx::PxU32 w2, physx::PxU32 w3);

    int AttachShapeToRigidBody(physx::PxShape* shape, physx::PxRigidActor* body);

    physx::PxVehicleWheelData* CreateWheelData();
	void SetWheelRadius(physx::PxVehicleWheelData* wheel, physx::PxReal radius);
	void SetWheelWidth(physx::PxVehicleWheelData* wheel, physx::PxReal width);
	void SetWheelMass(physx::PxVehicleWheelData* wheel, physx::PxReal mass);
	void SetWheelMomentOfInertia(physx::PxVehicleWheelData* wheel, physx::PxReal momentOfInertia);
	void SetWheelDampingRate(physx::PxVehicleWheelData* wheel, physx::PxReal dampingRate);

	physx::PxVehicleTireData* CreateTireData();
	void SetTireLateralStiffnessMaxLoad(physx::PxVehicleTireData* tire, physx::PxReal maxLoad);
	void SetTireMaxLateralStiffness(physx::PxVehicleTireData* tire, physx::PxReal maxStiffness);
	void SetTireLongitudinalStiffnessScale(physx::PxVehicleTireData* tire, physx::PxReal stiffnessScale);
	void SetTireBaseFriction(physx::PxVehicleTireData* tire, physx::PxReal friction);
	void SetTireMaxFrictionSlipPoint(physx::PxVehicleTireData* tire, physx::PxReal slipPoint);
	void SetTireMaxFriction(physx::PxVehicleTireData* tire, physx::PxReal friction);
	void SetTirePlateuxSlipPoint(physx::PxVehicleTireData* tire, physx::PxReal slipPoint);
	void SetTirePlateuxFriction(physx::PxVehicleTireData* tire, physx::PxReal friction);

	physx::PxVehicleSuspensionData* CreateSuspensionData();
	void SetSuspensionSpringStrength(physx::PxVehicleSuspensionData* suspension, physx::PxReal strength);
	void SetSuspensionSpringDamper(physx::PxVehicleSuspensionData* suspension, physx::PxReal damper);
	void SetSuspensionMaxCompression(physx::PxVehicleSuspensionData* suspension, physx::PxReal maxCompression);
	void SetSuspensionMaxDroop(physx::PxVehicleSuspensionData* suspension, physx::PxReal maxDroop);
	void SetSuspensionSprungMasses(physx::PxVehicleSuspensionData** suspensions, physx::PxU32 wheelCount, std::vector<physx::PxVec3>* wheelPositions, physx::PxVec3* centreOfMass, physx::PxReal mass);

	physx::PxVehicleWheelsSimData* CreateWheelSimData(physx::PxU32 wheelCount);
	void SetWheelSimWheelData(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVehicleWheelData* wheel);
	void SetWheelSimTireData(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVehicleTireData* tire);
	void SetWheelSimSuspensionData(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVehicleSuspensionData* suspension, physx::PxVec3* down);
	void SetWheelSimWheelCentre(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVec3* centre);
	void SetWheelSimForceAppPoint(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVec3* point);
	void SetWheelSimQueryFilterData(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxU32 w0, physx::PxU32 w1, physx::PxU32 w2, physx::PxU32 w3);
	void SetWheelSimWheelShape(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxU32 shapeNum);

	physx::PxVehicleNoDrive* CreateVehicleFromRigidBody(physx::PxRigidDynamic* body, physx::PxVehicleWheelsSimData* wheelSimData);

	physx::PxVehicleWheelsSimData* GetWheelSimData(physx::PxVehicleWheels* vehicle);
	physx::PxVehicleWheelsDynData* GetWheelDynData(physx::PxVehicleWheels* vehicle);
	void SetWheelDynTireData(physx::PxVehicleWheelsDynData* wheelDynData, physx::PxU32 wheelNum, physx::PxVehicleTireData* tire);

	void SetWheelSteer(physx::PxVehicleNoDrive* vehicle, physx::PxU32 wheelNum, physx::PxReal steerAngle);
	void SetWheelDrive(physx::PxVehicleNoDrive* vehicle, physx::PxU32 wheelNum, physx::PxReal driveTorque);
	void SetWheelBrake(physx::PxVehicleNoDrive* vehicle, physx::PxU32 wheelNum, physx::PxReal brakeTorque);

    void RegisterCollisionCallback(CollisionCallback onCollisionCallback);
    void RegisterTriggerCallback(TriggerCallback onTriggerCallback);

    void SetRigidBodyMassAndInertia(physx::PxRigidBody* body, float density, const physx::PxVec3* massLocalPose = NULL);
    void SetRigidBodyMassPose(physx::PxRigidBody* body, physx::PxTransform* pose);
    void SetRigidBodyDamping(physx::PxRigidBody* body, float linear, float angular);

	void UpdateVehicleCentreOfMass(physx::PxTransform* oldCentre, physx::PxTransform* newCentre, physx::PxVehicleWheels* vehicle);

    void SetRigidBodyFlag(physx::PxRigidBody* body, physx::PxRigidBodyFlag::Enum flag, bool value);
    void SetRigidBodyDominanceGroup(physx::PxRigidBody* body, physx::PxDominanceGroup group);
    void SetRigidBodyMaxDepenetrationVelocity(physx::PxRigidBody* body, physx::PxReal velocity);

    void AddActorToScene(physx::PxScene* scene, physx::PxActor* actor);

    void StepPhysics(physx::PxScene* scene, float time);

    physx::PxTransform* GetCentreOfMass(physx::PxRigidBody* body);

    void GetPosition(physx::PxRigidActor* actor, physx::PxVec3* position);
    void GetRotation(physx::PxRigidActor* actor, physx::PxQuat* rotation);

    void GetLinearVelocity(physx::PxRigidBody* rigidBody, physx::PxVec3* velocity);
    void GetAngularVelocity(physx::PxRigidBody* rigidBody, physx::PxVec3* velocity);

	void AddForce(physx::PxRigidBody* rigidBody, physx::PxVec3* force, physx::PxForceMode::Enum forceMode);
	void AddForceAtPosition(physx::PxRigidBody* rigidBody, physx::PxVec3* force, physx::PxVec3* position, physx::PxForceMode::Enum forceMode);
	void AddTorque(physx::PxRigidBody* rigidBody, physx::PxVec3* torque, physx::PxForceMode::Enum forceMode);

    physx::PxActor* GetPairHeaderActor(physx::PxContactPairHeader* header, int actorNum);
    physx::PxShape* GetContactPairShape(physx::PxContactPair* pairs, int i, int actor);
    physx::PxContactStreamIterator* GetContactPointIterator(physx::PxContactPair* pairs, int i);
    bool NextContactPatch(physx::PxContactStreamIterator* iter);
    bool NextContactPoint(physx::PxContactStreamIterator* iter);
    void GetContactPointData(physx::PxContactStreamIterator* iter, int j, physx::PxContactPair* pairs, int i, physx::PxVec3* point, physx::PxVec3* normal, physx::PxVec3* impulse);

    physx::PxReal GetSuspensionCompression(physx::PxVehicleWheels* vehicle, physx::PxU32 wheelNum);
    void GetWheelTransform(physx::PxVehicleWheels* vehicle, physx::PxU32 wheelNum, physx::PxVec3* position, physx::PxQuat* rotation);

    void GetTransformComponents(physx::PxTransform* transform, physx::PxVec3* position, physx::PxQuat* rotation);

    physx::PxRaycastCallback* CreateRaycastHit();
	bool FireRaycast(physx::PxScene* scene, physx::PxVec3* origin, physx::PxVec3* direction, physx::PxReal distance, physx::PxRaycastCallback* raycastHit);
	void GetRaycastHitNormal(physx::PxRaycastCallback* raycastHit, physx::PxVec3* normal);
	void GetRaycastHitPoint(physx::PxRaycastCallback* raycastHit, physx::PxVec3* point);
	physx::PxShape* GetRaycastHitShape(physx::PxRaycastCallback* raycastHit);
	physx::PxActor* GetRaycastHitActor(physx::PxRaycastCallback* raycastHit);
	physx::PxReal GetRaycastHitDistance(physx::PxRaycastCallback* raycastHit);
	void DestroyRaycastHit(physx::PxRaycastCallback* raycastHit);
}
