//#include <stdio.h>
#include "physx/include/PxPhysicsAPI.h"
#include "physx/source/foundation/include/PsFoundation.h"

#include <iostream>
#include <string>

#define CONTACT_BEGIN   (1)
#define CONTACT_SUSTAIN (1 << 1)
#define CONTACT_END     (1 << 2)

extern "C" {
    typedef void(*DebugLog)(const char* stringPtr, int length);
    typedef void(*CollisionCallback)(const physx::PxActor* actor);

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

    class ActorUserData {
    public:
        ActorUserData();
        ~ActorUserData();

        CollisionCallback onCollisionEnterCallback;
        CollisionCallback onCollisionStayCallback;
        CollisionCallback onCollisionExitCallback;

        int q = 0;
    };

    void RegisterDebugLog(DebugLog dl);

    void SetupFoundation();

    void CreatePhysics(bool trackAllocations);

    physx::PxScene* CreateScene(physx::PxVec3* gravity);

    physx::PxMaterial* CreateMaterial(float staticFriction, float dynamicFriction, float restitution);

    physx::PxRigidStatic* CreateStaticPlane(physx::PxVec3* point, physx::PxVec3* normal, physx::PxMaterial* mat);

    physx::PxGeometry* CreateBoxGeometry(float halfX, float halfY, float halfZ);
    physx::PxGeometry* CreateSphereGeometry(float radius);

    physx::PxShape* CreateShape(physx::PxGeometry* geometry, physx::PxMaterial* mat);

    physx::PxTransform* CreateTransform(physx::PxVec3* pos, physx::PxQuat* rot);

    physx::PxRigidDynamic* CreateDynamicRigidBody(physx::PxTransform* pose);

    void SetCollisionFilterData(physx::PxShape* shape, physx::PxU32 w0, physx::PxU32 w1, physx::PxU32 w2, physx::PxU32 w3);

    void AttachShapeToRigidBody(physx::PxShape* shape, physx::PxRigidActor* body);

    void RegisterCollisionEnterCallback(CollisionCallback collisionEnterCallback, physx::PxActor* actor);
    void RegisterCollisionStayCallback(CollisionCallback collisionStayCallback, physx::PxActor* actor);
    void RegisterCollisionExitCallback(CollisionCallback collisionExitCallback, physx::PxActor* actor);

    void SetRigidBodyMassAndInertia(physx::PxRigidBody* body, float density, const physx::PxVec3* massLocalPose = NULL);
    void SetRigidBodyDamping(physx::PxRigidBody* body, float linear, float angular);

    void SetRigidBodyFlag(physx::PxRigidBody* body, physx::PxRigidBodyFlag::Enum flag, bool value);

    void AddActorToScene(physx::PxScene* scene, physx::PxActor* actor);

    void StepPhysics(physx::PxScene* scene, float time);

    void GetPosition(physx::PxRigidActor* actor, physx::PxVec3* position);
    void GetRotation(physx::PxRigidActor* actor, physx::PxQuat* rotation);

    void GetLinearVelocity(physx::PxRigidBody* rigidBody, physx::PxVec3* velocity);
    void GetAngularVelocity(physx::PxRigidBody* rigidBody, physx::PxVec3* velocity);

	void AddForce(physx::PxRigidBody* rigidBody, physx::PxVec3* force, physx::PxForceMode::Enum forceMode);
	void AddForceAtPosition(physx::PxRigidBody* rigidBody, physx::PxVec3* force, physx::PxVec3* position, physx::PxForceMode::Enum forceMode);
	void AddTorque(physx::PxRigidBody* rigidBody, physx::PxVec3* torque, physx::PxForceMode::Enum forceMode);
}
