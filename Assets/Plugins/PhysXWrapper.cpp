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

CollisionHandler::CollisionHandler() {

}

CollisionHandler::~CollisionHandler() {

}

void CollisionHandler::onConstraintBreak(physx::PxConstraintInfo *constraints, physx::PxU32 count) {

}

void CollisionHandler::onWake(physx::PxActor **actors, physx::PxU32 count) {

}

void CollisionHandler::onSleep(physx::PxActor **actors, physx::PxU32 count) {

}

void CollisionHandler::onContact(const physx::PxContactPairHeader &pairHeader, const physx::PxContactPair *pairs, physx::PxU32 nbPairs) {
	bool fireBeginA = false;
	bool fireSustainA = false;
	bool fireEndA = false;

	bool fireBeginB = false;
	bool fireSustainB = false;
	bool fireEndB = false;

	for (int i = 0; i < nbPairs; i++) {
		physx::PxU32 contactTriggerFlagsA = 0;
		physx::PxU32 contactTriggerFlagsB = 0;

		if (!(pairs[i].flags & physx::PxContactPairFlag::eREMOVED_SHAPE_0)) contactTriggerFlagsA = pairs[i].shapes[0]->getSimulationFilterData().word2;
		if (!(pairs[i].flags & physx::PxContactPairFlag::eREMOVED_SHAPE_1)) contactTriggerFlagsB = pairs[i].shapes[1]->getSimulationFilterData().word2;

		if (pairs[i].flags & physx::PxContactPairFlag::eACTOR_PAIR_HAS_FIRST_TOUCH) {
			if (contactTriggerFlagsA & CONTACT_BEGIN) fireBeginA = true;
			if (contactTriggerFlagsB & CONTACT_BEGIN) fireBeginB = true;
		}

		if (contactTriggerFlagsA & CONTACT_SUSTAIN) fireSustainA = true;
		if (contactTriggerFlagsB & CONTACT_SUSTAIN) fireSustainB = true;

		if (pairs[i].flags & physx::PxContactPairFlag::eACTOR_PAIR_LOST_TOUCH) {
			if (contactTriggerFlagsA & CONTACT_END) fireEndA = true;
			if (contactTriggerFlagsB & CONTACT_END) fireEndB = true;
		}
	}

	ActorUserData* userDataA = (ActorUserData *)(pairHeader.actors[0]->userData);
	ActorUserData* userDataB = (ActorUserData *)(pairHeader.actors[1]->userData);

	if (fireBeginA) {
		debugLog(std::to_string(userDataA->q));
		userDataA->onCollisionEnterCallback(pairHeader.actors[0]);
	}
	if (fireSustainA) {
		userDataA->q++;
		userDataA->onCollisionStayCallback(pairHeader.actors[0]);
	}
	if (fireEndA) {
		debugLog(std::to_string(userDataA->q));
		userDataA->onCollisionExitCallback(pairHeader.actors[0]);
	}

	if (fireBeginB) {
		debugLog(std::to_string(userDataB->q));
		userDataB->onCollisionEnterCallback(pairHeader.actors[1]);
	}
	if (fireSustainB) {
		userDataB->q++;
		userDataB->onCollisionStayCallback(pairHeader.actors[1]);
	}
	if (fireEndB) {
		debugLog(std::to_string(userDataB->q));
		userDataB->onCollisionExitCallback(pairHeader.actors[1]);
	}
}

void CollisionHandler::onTrigger(physx::PxTriggerPair *pairs, physx::PxU32 count) {

}

void CollisionHandler::onAdvance(const physx::PxRigidBody *const *bodyBuffer, const physx::PxTransform *poseBuffer, const physx::PxU32 count) {

}

ActorUserData::ActorUserData() {

}

ActorUserData::~ActorUserData() {

}

CollisionHandler collisionHandler;

physx::PxFilterFlags FilterShader(physx::PxFilterObjectAttributes attributesA, physx::PxFilterData dataA,
								  physx::PxFilterObjectAttributes attributesB, physx::PxFilterData dataB,
								  physx::PxPairFlags& pairFlags, const void* filterShaderData, physx::PxU32 shaderDataSize) {
							
	if (physx::PxFilterObjectIsTrigger(attributesA) || physx::PxFilterObjectIsTrigger(attributesB)) {
		pairFlags = physx::PxPairFlag::eTRIGGER_DEFAULT;
		return physx::PxFilterFlag::eDEFAULT;
	}

	if ((dataA.word0 & dataB.word1) && (dataB.word0 & dataA.word1)) {
		pairFlags = physx::PxPairFlag::eCONTACT_DEFAULT;
	}

	if ((dataA.word2 & CONTACT_BEGIN) || (dataB.word2 & CONTACT_BEGIN)) {
		pairFlags |= physx::PxPairFlag::eNOTIFY_TOUCH_FOUND;
	}

	if ((dataA.word2 & CONTACT_SUSTAIN) || (dataB.word2 & CONTACT_SUSTAIN)) {
		pairFlags |= physx::PxPairFlag::eNOTIFY_TOUCH_PERSISTS;
	}

	if ((dataA.word2 & CONTACT_END) || (dataB.word2 & CONTACT_END)) {
		pairFlags |= physx::PxPairFlag::eNOTIFY_TOUCH_LOST;
	}

	return physx::PxFilterFlag::eDEFAULT;
}


extern "C" {
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
		sceneDesc.filterShader	= FilterShader;
		sceneDesc.simulationEventCallback = &collisionHandler;

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
		physx::PxRigidDynamic* actor = gPhysics->createRigidDynamic(*pose);
		actor->userData = (void *)(new ActorUserData());
		return actor;
	}

	void SetCollisionFilterData(physx::PxShape* shape, physx::PxU32 w0, physx::PxU32 w1, physx::PxU32 w2, physx::PxU32 w3) {
		shape->setSimulationFilterData(physx::PxFilterData(w0, w1, w2, w3));
	}

	void AttachShapeToRigidBody(physx::PxShape* shape, physx::PxRigidActor* body) {
		body->attachShape(*shape);
	}

    void RegisterCollisionEnterCallback(CollisionCallback collisionEnterCallback, physx::PxActor* actor) {
		debugLog("a");
		((ActorUserData *)(actor->userData))->onCollisionEnterCallback = collisionEnterCallback;
	}

    void RegisterCollisionStayCallback(CollisionCallback collisionStayCallback, physx::PxActor* actor) {
		debugLog("b");
		((ActorUserData *)(actor->userData))->onCollisionStayCallback = collisionStayCallback;
	}

    void RegisterCollisionExitCallback(CollisionCallback collisionExitCallback, physx::PxActor* actor) {
		debugLog("c");
		((ActorUserData *)(actor->userData))->onCollisionExitCallback = collisionExitCallback;
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

