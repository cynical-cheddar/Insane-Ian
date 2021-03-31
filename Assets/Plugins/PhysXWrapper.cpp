#include "PhysXWrapper.h"

#define PX_RELEASE(x)	if(x) {x->release(); x = NULL;}

DebugLog dl = nullptr;

physx::PxFoundation* gFoundation = NULL;

physx::PxPhysics* gPhysics = NULL;

physx::PxDefaultCpuDispatcher* gDispatcher = NULL;

physx::PxScene* gScene = NULL;

physx::PxMaterial* gMaterial = NULL;

physx::PxDefaultAllocator	  gAllocator;

physx::PxReal stackZ = 10.0f;

void debugLog(const std::string str) {
	if (dl != nullptr) {
		const char* stringPtr = str.c_str();
		int length = strlen(stringPtr);

		dl(stringPtr, length);
	}
}


MyErrorCallback::MyErrorCallback()
{
}

MyErrorCallback::~MyErrorCallback()
{
}

void MyErrorCallback::reportError(physx::PxErrorCode::Enum e, const char* message, const char* file, int line)
{
	const char* errorCode = NULL;

	switch (e)
	{
	case physx::PxErrorCode::eNO_ERROR:
		errorCode = "no error";
		break;
	case physx::PxErrorCode::eINVALID_PARAMETER:
		errorCode = "invalid parameter";
		break;
	case physx::PxErrorCode::eINVALID_OPERATION:
		errorCode = "invalid operation";
		break;
	case physx::PxErrorCode::eOUT_OF_MEMORY:
		errorCode = "out of memory";
		break;
	case physx::PxErrorCode::eDEBUG_INFO:
		errorCode = "info";
		break;
	case physx::PxErrorCode::eDEBUG_WARNING:
		errorCode = "warning";
		break;
	case physx::PxErrorCode::ePERF_WARNING:
		errorCode = "performance warning";
		break;
	case physx::PxErrorCode::eABORT:
		errorCode = "abort";
		break;
	case physx::PxErrorCode::eINTERNAL_ERROR:
		errorCode = "internal error";
		break;
	case physx::PxErrorCode::eMASK_ALL:
		errorCode = "unknown error";
		break;
	}

	PX_ASSERT(errorCode);
	if(errorCode)
	{
		char buffer[1024];
		sprintf(buffer, "%s (%d) : %s : %s\n", file, line, errorCode, message);

		debugLog(buffer);

		// in debug builds halt execution for abort codes
		PX_ASSERT(e != physx::PxErrorCode::eABORT);
	}	
}

MyErrorCallback gErrorCallback;

physx::PxRigidDynamic* createDynamic(const physx::PxTransform& t, const physx::PxGeometry& geometry, const physx::PxVec3& velocity = physx::PxVec3(0))
{
	physx::PxRigidDynamic* dynamic = physx::PxCreateDynamic(*gPhysics, t, geometry, *gMaterial, 10.0f);
	dynamic->setAngularDamping(0.5f);
	dynamic->setLinearVelocity(velocity);
	gScene->addActor(*dynamic);
	return dynamic;
}

void createStack(const physx::PxTransform& t, physx::PxU32 size, physx::PxReal halfExtent)
{
	physx::PxShape* shape = gPhysics->createShape(physx::PxBoxGeometry(halfExtent, halfExtent, halfExtent), *gMaterial);

	for(physx::PxU32 i = 0; i < size; i++)
	{
		for(physx::PxU32 j = 0; j < size - i; j++)
		{
			physx::PxTransform localTm(physx::PxVec3(physx::PxReal(j*2) - physx::PxReal(size-i), physx::PxReal(i*2+1), 0) * halfExtent);
			physx::PxRigidDynamic* body = gPhysics->createRigidDynamic(t.transform(localTm));
			body->attachShape(*shape);
			physx::PxRigidBodyExt::updateMassAndInertia(*body, 10.0f);
			gScene->addActor(*body);
		}
	}
	shape->release();
}

void initPhysics(bool interactive)
{
	

	//gMaterial = gPhysics->createMaterial(0.5f, 0.5f, 0.6f);

	//physx::PxRigidStatic* groundPlane = PxCreatePlane(*gPhysics, physx::PxPlane(0,1,0,0), *gMaterial);
	//gScene->addActor(*groundPlane);

	for(physx::PxU32 i=0; i<5; i++) {
		createStack(physx::PxTransform(physx::PxVec3(0,0,stackZ-=10.0f)), 10, 2.0f);
    }

	if (!interactive) {
		createDynamic(physx::PxTransform(physx::PxVec3(0,40,100)), physx::PxSphereGeometry(10), physx::PxVec3(0,-50,-100));
    }
}
	
void cleanupPhysics()
{
	PX_RELEASE(gScene);
	PX_RELEASE(gDispatcher);
	PX_RELEASE(gPhysics);
	//PX_RELEASE(gFoundation);
}

extern "C" {
	int AddNumberses(int x, int y) {
		static const physx::PxU32 frameCount = 100;
		initPhysics(false);

		for(physx::PxU32 i=0; i<frameCount; i++) {
			//StepPhysics();
		}

		cleanupPhysics();
		
		return x + y + 1;
	}

	void RegisterDebugLog(DebugLog debl) {
		dl = debl;
	}

	void SetupFoundation() {
		gFoundation = PxCreateFoundation(PX_PHYSICS_VERSION, gAllocator, gErrorCallback);

		if (gFoundation == 0) {
			gFoundation = &physx::shdfnd::Foundation::getInstance();
		}
	}

	void CreatePhysics(bool trackAllocations) {
		gPhysics = PxCreatePhysics(PX_PHYSICS_VERSION, *gFoundation, physx::PxTolerancesScale(), trackAllocations);
	}

	physx::PxScene* CreateScene(physx::PxVec3* gravity) {
		physx::PxSceneDesc sceneDesc(gPhysics->getTolerancesScale());
		sceneDesc.gravity = *gravity;
		gDispatcher = physx::PxDefaultCpuDispatcherCreate(0);
		sceneDesc.cpuDispatcher	= gDispatcher;
		sceneDesc.filterShader	= physx::PxDefaultSimulationFilterShader;

		return gPhysics->createScene(sceneDesc);
	}

	physx::PxMaterial* CreateMaterial(float staticFriction, float dynamicFriction, float restitution) {
		return gPhysics->createMaterial(staticFriction, dynamicFriction, restitution);
	}

	physx::PxRigidStatic* CreateStaticPlane(physx::PxVec3* point, physx::PxVec3* normal, physx::PxMaterial* mat) {
		return PxCreatePlane(*gPhysics, physx::PxPlane(*point, *normal), *mat);
	}

	physx::PxGeometry* CreateBoxGeometry(float halfX, float halfY, float halfZ) {
		return new physx::PxBoxGeometry(halfX, halfY, halfZ);
	}

	physx::PxGeometry* CreateSphereGeometry(float radius) {
		return new physx::PxSphereGeometry(radius);
	}

	physx::PxShape* CreateShape(physx::PxGeometry* geometry, physx::PxMaterial* mat) {
		return gPhysics->createShape(*geometry, *mat);
	}

	physx::PxTransform* CreateTransform(physx::PxVec3* pos, physx::PxQuat* rot) {
		return new physx::PxTransform(*pos, *rot);
	}

	physx::PxRigidDynamic* CreateDynamicRigidBody(physx::PxTransform* pose) {
		return gPhysics->createRigidDynamic(*pose);
	}

	void AttachShapeToRigidBody(physx::PxShape* shape, physx::PxRigidActor* body) {
		body->attachShape(*shape);
	}

	void SetRigidBodyMassAndInertia(physx::PxRigidBody* body, float density, const physx::PxVec3* massLocalPose) {
		physx::PxRigidBodyExt::setMassAndUpdateInertia(*body, density, massLocalPose);
	}

	void SetRigidBodyDamping(physx::PxRigidBody* body, float linear, float angular) {
		body->setLinearDamping(linear);
		body->setAngularDamping(angular);
	}

	void SetRigidBodyFlag(physx::PxRigidBody* body, physx::PxRigidBodyFlag::Enum flag, bool value) {
		body->setRigidBodyFlag(flag, value);
	}

	void AddActorToScene(physx::PxScene* scene, physx::PxActor* actor) {
		scene->addActor(*actor);
	}

	void StepPhysics(physx::PxScene* scene, float time) {
		scene->simulate(time);
		scene->fetchResults(true);
	}

	void GetPosition(physx::PxRigidActor* actor, physx::PxVec3* position) {
		*position = actor->getGlobalPose().p;
	}

	void GetRotation(physx::PxRigidActor* actor, physx::PxQuat* rotation) {
		*rotation = actor->getGlobalPose().q;
	}

	void GetLinearVelocity(physx::PxRigidBody* rigidBody, physx::PxVec3* velocity) {
		*velocity = rigidBody->getLinearVelocity();
	}

	void GetAngularVelocity(physx::PxRigidBody* rigidBody, physx::PxVec3* velocity) {
		*velocity = rigidBody->getAngularVelocity();
	}

	void AddForce(physx::PxRigidBody* rigidBody, physx::PxVec3* force, physx::PxForceMode::Enum forceMode) {
		rigidBody->addForce(*force, forceMode);
	}

	void AddForceAtPosition(physx::PxRigidBody* rigidBody, physx::PxVec3* force, physx::PxVec3* position, physx::PxForceMode::Enum forceMode) {
		physx::PxRigidBodyExt::addForceAtPos(*rigidBody, *force, *position, forceMode);
	}

	void AddTorque(physx::PxRigidBody* rigidBody, physx::PxVec3* torque, physx::PxForceMode::Enum forceMode) {
		rigidBody->addTorque(*torque, forceMode);
	}
}

