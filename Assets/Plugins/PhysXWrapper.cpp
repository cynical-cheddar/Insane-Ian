#include "PhysXWrapper.h"

#define PX_RELEASE(x)	if(x) {x->release(); x = NULL;}

DebugLog dl = nullptr;

physx::PxFoundation* gFoundation = NULL;

physx::PxPhysics* gPhysics = NULL;

VehicleSceneQueryData*	gVehicleSceneQueryData = NULL;

physx::PxDefaultCpuDispatcher* gDispatcher = NULL;

physx::PxScene* gScene = NULL;

physx::PxCooking* gCooking = NULL;

physx::PxMaterial* gMaterial = NULL;

physx::PxDefaultAllocator gAllocator;

physx::PxBatchQuery* gBatchQuery = NULL;

physx::PxVehicleDrivableSurfaceToTireFrictionPairs* gFrictionPairs = NULL;

physx::PxContactStreamIterator iter(NULL, NULL, NULL, 0, 0);

CollisionCallback collisionCallback = NULL;

void debugLog(const std::string str) {
	if (dl != nullptr) {
		const char* stringPtr = str.c_str();
		int length = strlen(stringPtr);

		dl(stringPtr, length);
	}
}

void collision(const physx::PxContactPairHeader* pairHeader, const physx::PxContactPair* pairs, physx::PxU32 nbPairs, const physx::PxActor* self, bool isEnter, bool isStay, bool isExit) {
	if (collisionCallback != NULL) {
		collisionCallback(pairHeader, pairs, nbPairs, self, isEnter, isStay, isExit);
	}
	else {
		debugLog("collision enter callback not set");
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

	if (fireBeginA || fireSustainA || fireEndA) {
		collision(&pairHeader, pairs, nbPairs, pairHeader.actors[0], fireBeginA, fireSustainA, fireEndA);
	}

	if (fireBeginB || fireSustainB || fireEndB) {
		collision(&pairHeader, pairs, nbPairs, pairHeader.actors[1], fireBeginB, fireSustainB, fireEndB);
	}
}

void CollisionHandler::onTrigger(physx::PxTriggerPair *pairs, physx::PxU32 count) {

}

void CollisionHandler::onAdvance(const physx::PxRigidBody *const *bodyBuffer, const physx::PxTransform *poseBuffer, const physx::PxU32 count) {

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
		pairFlags |= physx::PxPairFlag::eNOTIFY_CONTACT_POINTS;
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

VehicleSceneQueryData::VehicleSceneQueryData() : 
	mNumQueriesPerBatch(0),
   	mNumHitResultsPerQuery(0),
   	mRaycastResults(NULL),
   	mRaycastHitBuffer(NULL),
   	mPreFilterShader(NULL),
   	mPostFilterShader(NULL) {

}

VehicleSceneQueryData::~VehicleSceneQueryData() {

}

VehicleSceneQueryData* VehicleSceneQueryData::allocate(const physx::PxU32 maxNumVehicles, const physx::PxU32 maxNumWheelsPerVehicle, const physx::PxU32 maxNumHitPointsPerWheel, const physx::PxU32 numVehiclesInBatch, 
 													   physx::PxBatchQueryPreFilterShader preFilterShader, physx::PxBatchQueryPostFilterShader postFilterShader, physx::PxAllocatorCallback& allocator) {

	const physx::PxU32 sqDataSize = ((sizeof(VehicleSceneQueryData) + 15) & ~15);

	const physx::PxU32 maxNumWheels = maxNumVehicles*maxNumWheelsPerVehicle;
	const physx::PxU32 raycastResultSize = ((sizeof(physx::PxRaycastQueryResult) * maxNumWheels + 15) & ~15);
	const physx::PxU32 sweepResultSize = ((sizeof(physx::PxSweepQueryResult) * maxNumWheels + 15) & ~15);

	const physx::PxU32 maxNumHitPoints = maxNumWheels*maxNumHitPointsPerWheel;
	const physx::PxU32 raycastHitSize = ((sizeof(physx::PxRaycastHit) * maxNumHitPoints + 15) & ~15);
	const physx::PxU32 sweepHitSize = ((sizeof(physx::PxSweepHit) * maxNumHitPoints + 15) & ~15);

	const physx::PxU32 size = sqDataSize + raycastResultSize + raycastHitSize + sweepResultSize + sweepHitSize;
	physx::PxU8* buffer = static_cast<physx::PxU8*>(allocator.allocate(size, NULL, NULL, 0));
	
	VehicleSceneQueryData* sqData = new(buffer) VehicleSceneQueryData();
	sqData->mNumQueriesPerBatch = numVehiclesInBatch*maxNumWheelsPerVehicle;
	sqData->mNumHitResultsPerQuery = maxNumHitPointsPerWheel;
	buffer += sqDataSize;
	
	sqData->mRaycastResults = reinterpret_cast<physx::PxRaycastQueryResult*>(buffer);
	buffer += raycastResultSize;

	sqData->mRaycastHitBuffer = reinterpret_cast<physx::PxRaycastHit*>(buffer);
	buffer += raycastHitSize;

	sqData->mSweepResults = reinterpret_cast<physx::PxSweepQueryResult*>(buffer);
	buffer += sweepResultSize;

	sqData->mSweepHitBuffer = reinterpret_cast<physx::PxSweepHit*>(buffer);
	buffer += sweepHitSize;

	for (physx::PxU32 i = 0; i < maxNumWheels; i++)
	{
		new(sqData->mRaycastResults + i) physx::PxRaycastQueryResult();
		new(sqData->mSweepResults + i) physx::PxSweepQueryResult();
	}

	for (physx::PxU32 i = 0; i < maxNumHitPoints; i++)
	{
		new(sqData->mRaycastHitBuffer + i) physx::PxRaycastHit();
		new(sqData->mSweepHitBuffer + i) physx::PxSweepHit();
	}

	sqData->mPreFilterShader = preFilterShader;
	sqData->mPostFilterShader = postFilterShader;

	return sqData;
}

void VehicleSceneQueryData::free(physx::PxAllocatorCallback& allocator)
{
	allocator.deallocate(this);
}

physx::PxBatchQuery* VehicleSceneQueryData::setUpBatchedSceneQuery(const physx::PxU32 batchId, const VehicleSceneQueryData& vehicleSceneQueryData, physx::PxScene* scene)
{
	const physx::PxU32 maxNumQueriesInBatch =  vehicleSceneQueryData.mNumQueriesPerBatch;
	const physx::PxU32 maxNumHitResultsInBatch = vehicleSceneQueryData.mNumQueriesPerBatch*vehicleSceneQueryData.mNumHitResultsPerQuery;

	physx::PxBatchQueryDesc sqDesc(maxNumQueriesInBatch, maxNumQueriesInBatch, 0);

	sqDesc.queryMemory.userRaycastResultBuffer = vehicleSceneQueryData.mRaycastResults + batchId * maxNumQueriesInBatch;
	sqDesc.queryMemory.userRaycastTouchBuffer = vehicleSceneQueryData.mRaycastHitBuffer + batchId * maxNumHitResultsInBatch;
	sqDesc.queryMemory.raycastTouchBufferSize = maxNumHitResultsInBatch;

	sqDesc.queryMemory.userSweepResultBuffer = vehicleSceneQueryData.mSweepResults + batchId * maxNumQueriesInBatch;
	sqDesc.queryMemory.userSweepTouchBuffer = vehicleSceneQueryData.mSweepHitBuffer + batchId * maxNumHitResultsInBatch;
	sqDesc.queryMemory.sweepTouchBufferSize = maxNumHitResultsInBatch;

	sqDesc.preFilterShader = vehicleSceneQueryData.mPreFilterShader;

	sqDesc.postFilterShader = vehicleSceneQueryData.mPostFilterShader;

	return scene->createBatchQuery(sqDesc);
}

physx::PxRaycastQueryResult* VehicleSceneQueryData::getRaycastQueryResultBuffer(const physx::PxU32 batchId) 
{
	return (mRaycastResults + batchId * mNumQueriesPerBatch);
}

physx::PxSweepQueryResult* VehicleSceneQueryData::getSweepQueryResultBuffer(const physx::PxU32 batchId)
{
	return (mSweepResults + batchId * mNumQueriesPerBatch);
}


physx::PxU32 VehicleSceneQueryData::getQueryResultBufferSize() const 
{
	return mNumQueriesPerBatch;
}

//	might just be able to say 1 tyre type, 1 mat type, friction is 1
physx::PxVehicleDrivableSurfaceToTireFrictionPairs* createFrictionPairs(const physx::PxMaterial* defaultMaterial)
{
	physx::PxVehicleDrivableSurfaceType surfaceTypes[1];
	surfaceTypes[0].mType = 0;//SURFACE_TYPE_TARMAC;

	const physx::PxMaterial* surfaceMaterials[1];
	surfaceMaterials[0] = defaultMaterial;

	physx::PxVehicleDrivableSurfaceToTireFrictionPairs* surfaceTirePairs = physx::PxVehicleDrivableSurfaceToTireFrictionPairs::allocate(1, 1);//MAX_NUM_TIRE_TYPES,MAX_NUM_SURFACE_TYPES);

	surfaceTirePairs->setup(1, 1, surfaceMaterials, surfaceTypes);
	//surfaceTirePairs->setup(MAX_NUM_TIRE_TYPES, MAX_NUM_SURFACE_TYPES, surfaceMaterials, surfaceTypes);

	//for(int i = 0; i < MAX_NUM_SURFACE_TYPES; i++)
	for(int i = 0; i < 1; i++)
	{
		//for(int j = 0; j < MAX_NUM_TIRE_TYPES; j++)
		for(int j = 0; j < 1; j++)
		{
			//surfaceTirePairs->setTypePairFriction(i,j,gTireFrictionMultipliers[i][j]);
			surfaceTirePairs->setTypePairFriction(i, j, 0.8f);
		}
	}
	return surfaceTirePairs;
}

physx::PxQueryHitType::Enum WheelSceneQueryPreFilterBlocking(physx::PxFilterData filterData0, physx::PxFilterData filterData1, const void* constantBlock, physx::PxU32 constantBlockSize, physx::PxHitFlags& queryFlags) {
	//filterData0 is the vehicle suspension query.
	//filterData1 is the shape potentially hit by the query.
	PX_UNUSED(filterData0);
	PX_UNUSED(constantBlock);
	PX_UNUSED(constantBlockSize);
	PX_UNUSED(queryFlags);
	//return ((0 == (filterData1.word3 & DRIVABLE_SURFACE)) ? physx::PxQueryHitType::eNONE : physx::PxQueryHitType::eBLOCK);
	return ((0 == (filterData1.word3 & 1)) ? physx::PxQueryHitType::eNONE : physx::PxQueryHitType::eBLOCK);
}

VehicleDesc initVehicleDesc()
{
	//Set up the chassis mass, dimensions, moment of inertia, and center of mass offset.
	//The moment of inertia is just the moment of inertia of a cuboid but modified for easier steering.
	//Center of mass offset is 0.65m above the base of the chassis and 0.25m towards the front.
	const physx::PxF32 chassisMass = 1500.0f;
	const physx::PxVec3 chassisDims(2.5f,2.0f,5.0f);
	const physx::PxVec3 chassisMOI((chassisDims.y * chassisDims.y + chassisDims.z * chassisDims.z) * 		chassisMass / 12.0f,
		 						   (chassisDims.x * chassisDims.x + chassisDims.z * chassisDims.z) * 0.8f * chassisMass / 12.0f,
		 						   (chassisDims.x * chassisDims.x + chassisDims.y * chassisDims.y) * 		chassisMass / 12.0f);
	const physx::PxVec3 chassisCMOffset(0.0f, -chassisDims.y*0.5f + 0.65f, 0.25f);

	//Set up the wheel mass, radius, width, moment of inertia, and number of wheels.
	//Moment of inertia is just the moment of inertia of a cylinder.
	const physx::PxF32 wheelMass = 20.0f;
	const physx::PxF32 wheelRadius = 0.5f;
	const physx::PxF32 wheelWidth = 0.4f;
	const physx::PxF32 wheelMOI = 0.5f*wheelMass*wheelRadius*wheelRadius;
	const physx::PxU32 nbWheels = 6;

	VehicleDesc vehicleDesc;

	vehicleDesc.chassisMass = chassisMass;
	vehicleDesc.chassisDims = chassisDims;
	vehicleDesc.chassisMOI = chassisMOI;
	vehicleDesc.chassisCMOffset = chassisCMOffset;
	vehicleDesc.chassisMaterial = gMaterial;
	//vehicleDesc.chassisSimFilterData = PxFilterData(COLLISION_FLAG_CHASSIS, COLLISION_FLAG_CHASSIS_AGAINST, 0, 0);
	vehicleDesc.chassisSimFilterData = physx::PxFilterData(1, 1, 0, 0);

	vehicleDesc.wheelMass = wheelMass;
	vehicleDesc.wheelRadius = wheelRadius;
	vehicleDesc.wheelWidth = wheelWidth;
	vehicleDesc.wheelMOI = wheelMOI;
	vehicleDesc.numWheels = nbWheels;
	vehicleDesc.wheelMaterial = gMaterial;
	//vehicleDesc.chassisSimFilterData = PxFilterData(COLLISION_FLAG_WHEEL, COLLISION_FLAG_WHEEL_AGAINST, 0, 0);
	vehicleDesc.chassisSimFilterData = physx::PxFilterData(1, 1, 0, 0);

	return vehicleDesc;
}

static physx::PxConvexMesh* createConvexMesh(const physx::PxVec3* verts, const physx::PxU32 numVerts, physx::PxPhysics& physics, physx::PxCooking& cooking)
{
	// Create descriptor for convex mesh
	physx::PxConvexMeshDesc convexDesc;
	convexDesc.points.count			= numVerts;
	convexDesc.points.stride		= sizeof(physx::PxVec3);
	convexDesc.points.data			= verts;
	convexDesc.flags				= physx::PxConvexFlag::eCOMPUTE_CONVEX;

	physx::PxConvexMesh* convexMesh = NULL;
	physx::PxDefaultMemoryOutputStream buf;
	if(cooking.cookConvexMesh(convexDesc, buf))
	{
		physx::PxDefaultMemoryInputData id(buf.getData(), buf.getSize());
		convexMesh = physics.createConvexMesh(id);
	}

	return convexMesh;
}

physx::PxConvexMesh* createChassisMesh(const physx::PxVec3 dims, physx::PxPhysics& physics, physx::PxCooking& cooking)
{
	const physx::PxF32 x = dims.x*0.5f;
	const physx::PxF32 y = dims.y*0.5f;
	const physx::PxF32 z = dims.z*0.5f;
	physx::PxVec3 verts[8] =
	{
		physx::PxVec3(x,y,-z), 
		physx::PxVec3(x,y,z),
		physx::PxVec3(x,-y,z),
		physx::PxVec3(x,-y,-z),
		physx::PxVec3(-x,y,-z), 
		physx::PxVec3(-x,y,z),
		physx::PxVec3(-x,-y,z),
		physx::PxVec3(-x,-y,-z)
	};

	return createConvexMesh(verts,8,physics,cooking);
}

physx::PxConvexMesh* createWheelMesh(const physx::PxF32 width, const physx::PxF32 radius, physx::PxPhysics& physics, physx::PxCooking& cooking)
{
	physx::PxVec3 points[2*16];
	for(physx::PxU32 i = 0; i < 16; i++)
	{
		const physx::PxF32 cosTheta = physx::PxCos(i*physx::PxPi*2.0f/16.0f);
		const physx::PxF32 sinTheta = physx::PxSin(i*physx::PxPi*2.0f/16.0f);
		const physx::PxF32 y = radius*cosTheta;
		const physx::PxF32 z = radius*sinTheta;
		points[2*i+0] = physx::PxVec3(-width/2.0f, y, z);
		points[2*i+1] = physx::PxVec3(+width/2.0f, y, z);
	}

	return createConvexMesh(points,32,physics,cooking);
}

physx::PxRigidDynamic* createVehicleActor(const physx::PxVehicleChassisData& chassisData, physx::PxMaterial** wheelMaterials, physx::PxConvexMesh** wheelConvexMeshes,
										  const physx::PxU32 numWheels, const physx::PxFilterData& wheelSimFilterData, physx::PxMaterial** chassisMaterials,
										  physx::PxConvexMesh** chassisConvexMeshes, const physx::PxU32 numChassisMeshes, const physx::PxFilterData& chassisSimFilterData,
										  physx::PxPhysics& physics) {

	//We need a rigid body actor for the vehicle.
	//Don't forget to add the actor to the scene after setting up the associated vehicle.
	physx::PxRigidDynamic* vehActor = physics.createRigidDynamic(physx::PxTransform(physx::PxIdentity));

	//Wheel and chassis query filter data.
	//Optional: cars don't drive on other cars.
	physx::PxFilterData wheelQryFilterData;
	//setupNonDrivableSurface(wheelQryFilterData);
	physx::PxFilterData chassisQryFilterData;
	//setupNonDrivableSurface(chassisQryFilterData);

	//Add all the wheel shapes to the actor.
	for(physx::PxU32 i = 0; i < numWheels; i++)
	{
		physx::PxConvexMeshGeometry geom(wheelConvexMeshes[i]);
		physx::PxShape* wheelShape = physx::PxRigidActorExt::createExclusiveShape(*vehActor, geom, *wheelMaterials[i]);
		wheelShape->setQueryFilterData(wheelQryFilterData);
		wheelShape->setSimulationFilterData(wheelSimFilterData);
		wheelShape->setLocalPose(physx::PxTransform(physx::PxIdentity));
	}

	//Add the chassis shapes to the actor.
	for(physx::PxU32 i = 0; i < numChassisMeshes; i++)
	{
		physx::PxShape* chassisShape = physx::PxRigidActorExt::createExclusiveShape(*vehActor, physx::PxConvexMeshGeometry(chassisConvexMeshes[i]), *chassisMaterials[i]);
		chassisShape->setQueryFilterData(chassisQryFilterData);
		chassisShape->setSimulationFilterData(chassisSimFilterData);
		chassisShape->setLocalPose(physx::PxTransform(physx::PxIdentity));
	}

	vehActor->setMass(chassisData.mMass);
	vehActor->setMassSpaceInertiaTensor(chassisData.mMOI);
	vehActor->setCMassLocalPose(physx::PxTransform(chassisData.mCMOffset, physx::PxQuat(physx::PxIdentity)));

	return vehActor;
}

void computeWheelCenterActorOffsets4W(const physx::PxF32 wheelFrontZ, const physx::PxF32 wheelRearZ, const physx::PxVec3& chassisDims, const physx::PxF32 wheelWidth,
									  const physx::PxF32 wheelRadius, const physx::PxU32 numWheels, physx::PxVec3* wheelCentreOffsets) {
	//chassisDims.z is the distance from the rear of the chassis to the front of the chassis.
	//The front has z = 0.5*chassisDims.z and the rear has z = -0.5*chassisDims.z.
	//Compute a position for the front wheel and the rear wheel along the z-axis.
	//Compute the separation between each wheel along the z-axis.
	const physx::PxF32 numLeftWheels = numWheels/2.0f;
	const physx::PxF32 deltaZ = (wheelFrontZ - wheelRearZ)/(numLeftWheels-1.0f);
	//Set the outside of the left and right wheels to be flush with the chassis.
	//Set the top of the wheel to be just touching the underside of the chassis.
	//Begin by setting the rear-left/rear-right/front-left,front-right wheels.
	wheelCentreOffsets[physx::PxVehicleDrive4WWheelOrder::eREAR_LEFT] = physx::PxVec3((-chassisDims.x + wheelWidth)*0.5f, -(chassisDims.y/2 + wheelRadius), wheelRearZ + 0*deltaZ*0.5f);
	wheelCentreOffsets[physx::PxVehicleDrive4WWheelOrder::eREAR_RIGHT] = physx::PxVec3((+chassisDims.x - wheelWidth)*0.5f, -(chassisDims.y/2 + wheelRadius), wheelRearZ + 0*deltaZ*0.5f);
	wheelCentreOffsets[physx::PxVehicleDrive4WWheelOrder::eFRONT_LEFT] = physx::PxVec3((-chassisDims.x + wheelWidth)*0.5f, -(chassisDims.y/2 + wheelRadius), wheelRearZ + (numLeftWheels-1)*deltaZ);
	wheelCentreOffsets[physx::PxVehicleDrive4WWheelOrder::eFRONT_RIGHT] = physx::PxVec3((+chassisDims.x - wheelWidth)*0.5f, -(chassisDims.y/2 + wheelRadius), wheelRearZ + (numLeftWheels-1)*deltaZ);
	//Set the remaining wheels.
	for(physx::PxU32 i = 2, wheelCount = 4; i < numWheels-2; i+=2, wheelCount+=2)
	{
		wheelCentreOffsets[wheelCount + 0] = physx::PxVec3((-chassisDims.x + wheelWidth)*0.5f, -(chassisDims.y/2 + wheelRadius), wheelRearZ + i*deltaZ*0.5f);
		wheelCentreOffsets[wheelCount + 1] = physx::PxVec3((+chassisDims.x - wheelWidth)*0.5f, -(chassisDims.y/2 + wheelRadius), wheelRearZ + i*deltaZ*0.5f);
	}
}

void setupWheelsSimulationData(const physx::PxF32 wheelMass, const physx::PxF32 wheelMOI, const physx::PxF32 wheelRadius, const physx::PxF32 wheelWidth, const physx::PxU32 numWheels,
							   const physx::PxVec3* wheelCenterActorOffsets, const physx::PxVec3& chassisCMOffset, const physx::PxF32 chassisMass, physx::PxVehicleWheelsSimData* wheelsSimData) {

	//Set up the wheels.
	physx::PxVehicleWheelData wheels[PX_MAX_NB_WHEELS];
	{
		//Set up the wheel data structures with mass, moi, radius, width.
		for (physx::PxU32 i = 0; i < numWheels; i++) {
			wheels[i].mMass = wheelMass;
			wheels[i].mMOI = wheelMOI;
			wheels[i].mRadius = wheelRadius;
			wheels[i].mWidth = wheelWidth;
		}

		//Enable the handbrake for the rear wheels only.
		wheels[physx::PxVehicleDrive4WWheelOrder::eREAR_LEFT].mMaxHandBrakeTorque=4000.0f;
		wheels[physx::PxVehicleDrive4WWheelOrder::eREAR_RIGHT].mMaxHandBrakeTorque=4000.0f;
		//Enable steering for the front wheels only.
		wheels[physx::PxVehicleDrive4WWheelOrder::eFRONT_LEFT].mMaxSteer = physx::PxPi*0.3333f;
		wheels[physx::PxVehicleDrive4WWheelOrder::eFRONT_RIGHT].mMaxSteer = physx::PxPi*0.3333f;
	}

	//Set up the tires.
	physx::PxVehicleTireData tires[PX_MAX_NB_WHEELS];
	{
		//Set up the tires.
		for(physx::PxU32 i = 0; i < numWheels; i++)
		{
			//tires[i].mType = TIRE_TYPE_NORMAL;
			tires[i].mType = 0;
		}
	}

	//Set up the suspensions
	physx::PxVehicleSuspensionData suspensions[PX_MAX_NB_WHEELS];
	{
		//Compute the mass supported by each suspension spring.
		physx::PxF32 suspSprungMasses[PX_MAX_NB_WHEELS];
		physx::PxVehicleComputeSprungMasses(numWheels, wheelCenterActorOffsets, chassisCMOffset, chassisMass, 1, suspSprungMasses);

		//Set the suspension data.
		for(physx::PxU32 i = 0; i < numWheels; i++)
		{
			suspensions[i].mMaxCompression = 0.3f;
			suspensions[i].mMaxDroop = 0.1f;
			suspensions[i].mSpringStrength = 35000.0f;	
			suspensions[i].mSpringDamperRate = 4500.0f;
			suspensions[i].mSprungMass = suspSprungMasses[i];
		}

		//Set the camber angles.
		const physx::PxF32 camberAngleAtRest=0.0;
		const physx::PxF32 camberAngleAtMaxDroop=0.01f;
		const physx::PxF32 camberAngleAtMaxCompression=-0.01f;
		for(physx::PxU32 i = 0; i < numWheels; i+=2)
		{
			suspensions[i + 0].mCamberAtRest =  camberAngleAtRest;
			suspensions[i + 1].mCamberAtRest =  -camberAngleAtRest;
			suspensions[i + 0].mCamberAtMaxDroop = camberAngleAtMaxDroop;
			suspensions[i + 1].mCamberAtMaxDroop = -camberAngleAtMaxDroop;
			suspensions[i + 0].mCamberAtMaxCompression = camberAngleAtMaxCompression;
			suspensions[i + 1].mCamberAtMaxCompression = -camberAngleAtMaxCompression;
		}
	}

	//Set up the wheel geometry.
	physx::PxVec3 suspTravelDirections[PX_MAX_NB_WHEELS];
	physx::PxVec3 wheelCentreCMOffsets[PX_MAX_NB_WHEELS];
	physx::PxVec3 suspForceAppCMOffsets[PX_MAX_NB_WHEELS];
	physx::PxVec3 tireForceAppCMOffsets[PX_MAX_NB_WHEELS];
	{
		//Set the geometry data.
		for (physx::PxU32 i = 0; i < numWheels; i++)
		{
			//Vertical suspension travel.
			suspTravelDirections[i] = physx::PxVec3(0,-1,0);

			//Wheel center offset is offset from rigid body center of mass.
			wheelCentreCMOffsets[i] = wheelCenterActorOffsets[i] - chassisCMOffset;

			//Suspension force application point 0.3 metres below 
			//rigid body center of mass.
			suspForceAppCMOffsets[i] = physx::PxVec3(wheelCentreCMOffsets[i].x,-0.3f,wheelCentreCMOffsets[i].z);

			//Tire force application point 0.3 metres below 
			//rigid body center of mass.
			tireForceAppCMOffsets[i] = physx::PxVec3(wheelCentreCMOffsets[i].x,-0.3f,wheelCentreCMOffsets[i].z);
		}
	}

	//Set up the filter data of the raycast that will be issued by each suspension.
	physx::PxFilterData qryFilterData;
	//setupNonDrivableSurface(qryFilterData);

	//Set the wheel, tire and suspension data.
	//Set the geometry data.
	//Set the query filter data
	for(physx::PxU32 i = 0; i < numWheels; i++)
	{
		wheelsSimData->setWheelData(i, wheels[i]);
		wheelsSimData->setTireData(i, tires[i]);
		wheelsSimData->setSuspensionData(i, suspensions[i]);
		wheelsSimData->setSuspTravelDirection(i, suspTravelDirections[i]);
		wheelsSimData->setWheelCentreOffset(i, wheelCentreCMOffsets[i]);
		wheelsSimData->setSuspForceAppPointOffset(i, suspForceAppCMOffsets[i]);
		wheelsSimData->setTireForceAppPointOffset(i, tireForceAppCMOffsets[i]);
		wheelsSimData->setSceneQueryFilterData(i, qryFilterData);
		wheelsSimData->setWheelShapeMapping(i, physx::PxI32(i)); 
	}

	//Add a front and rear anti-roll bar
	physx::PxVehicleAntiRollBarData barFront;
	barFront.mWheel0 = physx::PxVehicleDrive4WWheelOrder::eFRONT_LEFT;
	barFront.mWheel1 = physx::PxVehicleDrive4WWheelOrder::eFRONT_RIGHT;
	barFront.mStiffness = 10000.0f;
	wheelsSimData->addAntiRollBarData(barFront);
	physx::PxVehicleAntiRollBarData barRear;
	barRear.mWheel0 = physx::PxVehicleDrive4WWheelOrder::eREAR_LEFT;
	barRear.mWheel1 = physx::PxVehicleDrive4WWheelOrder::eREAR_RIGHT;
	barRear.mStiffness = 10000.0f;
	wheelsSimData->addAntiRollBarData(barRear);
}

void configureUserData(physx::PxVehicleWheels* vehicle, ActorUserData* actorUserData, ShapeUserData* shapeUserDatas)
{
	if(actorUserData)
	{
		vehicle->getRigidDynamicActor()->userData = actorUserData;
		actorUserData->vehicle = vehicle;
	}

	if(shapeUserDatas)
	{
		physx::PxShape* shapes[PX_MAX_NB_WHEELS + 1];
		vehicle->getRigidDynamicActor()->getShapes(shapes, PX_MAX_NB_WHEELS + 1);

		for (physx::PxU32 i = 0; i < vehicle->mWheelsSimData.getNbWheels(); i++) {
			const physx::PxI32 shapeId = vehicle->mWheelsSimData.getWheelShapeMapping(i);
			shapes[shapeId]->userData = &shapeUserDatas[i];
			shapeUserDatas[i].isWheel = true;
			shapeUserDatas[i].wheelId = i;
		}
	}
}

physx::PxVehicleDrive4W* createVehicle4W(const VehicleDesc& vehicle4WDesc, physx::PxPhysics* physics, physx::PxCooking* cooking)
{
	const physx::PxVec3 chassisDims = vehicle4WDesc.chassisDims;
	const physx::PxF32 wheelWidth = vehicle4WDesc.wheelWidth;
	const physx::PxF32 wheelRadius = vehicle4WDesc.wheelRadius;
	const physx::PxU32 numWheels = vehicle4WDesc.numWheels;

	const physx::PxFilterData& chassisSimFilterData = vehicle4WDesc.chassisSimFilterData;
	const physx::PxFilterData& wheelSimFilterData = vehicle4WDesc.wheelSimFilterData;

	//Construct a physx actor with shapes for the chassis and wheels.
	//Set the rigid body mass, moment of inertia, and center of mass offset.
	physx::PxRigidDynamic* veh4WActor = NULL;
	{
		//Construct a convex mesh for a cylindrical wheel.
		physx::PxConvexMesh* wheelMesh = createWheelMesh(wheelWidth, wheelRadius, *physics, *cooking);
		//Assume all wheels are identical for simplicity.
		physx::PxConvexMesh* wheelConvexMeshes[PX_MAX_NB_WHEELS];
		physx::PxMaterial* wheelMaterials[PX_MAX_NB_WHEELS];

		//Set the meshes and materials for the driven wheels.
		for(physx::PxU32 i = physx::PxVehicleDrive4WWheelOrder::eFRONT_LEFT; i <= physx::PxVehicleDrive4WWheelOrder::eREAR_RIGHT; i++)
		{
			wheelConvexMeshes[i] = wheelMesh;
			wheelMaterials[i] = vehicle4WDesc.wheelMaterial;
		}
		//Set the meshes and materials for the non-driven wheels
		for(physx::PxU32 i = physx::PxVehicleDrive4WWheelOrder::eREAR_RIGHT + 1; i < numWheels; i++)
		{
			wheelConvexMeshes[i] = wheelMesh;
			wheelMaterials[i] = vehicle4WDesc.wheelMaterial;
		}

		//Chassis just has a single convex shape for simplicity.
		physx::PxConvexMesh* chassisConvexMesh = createChassisMesh(chassisDims, *physics, *cooking);
		physx::PxConvexMesh* chassisConvexMeshes[1] = {chassisConvexMesh};
		physx::PxMaterial* chassisMaterials[1] = {vehicle4WDesc.chassisMaterial};

		//Rigid body data.
		physx::PxVehicleChassisData rigidBodyData;
		rigidBodyData.mMOI = vehicle4WDesc.chassisMOI;
		rigidBodyData.mMass = vehicle4WDesc.chassisMass;
		rigidBodyData.mCMOffset = vehicle4WDesc.chassisCMOffset;

		veh4WActor = createVehicleActor(rigidBodyData, wheelMaterials, wheelConvexMeshes, numWheels, wheelSimFilterData,
										chassisMaterials, chassisConvexMeshes, 1, chassisSimFilterData, *physics);
	}

	//Set up the sim data for the wheels.
	physx::PxVehicleWheelsSimData* wheelsSimData = physx::PxVehicleWheelsSimData::allocate(numWheels);
	{
		//Compute the wheel center offsets from the origin.
		physx::PxVec3 wheelCenterActorOffsets[PX_MAX_NB_WHEELS];
		const physx::PxF32 frontZ = chassisDims.z*0.3f;
		const physx::PxF32 rearZ = -chassisDims.z*0.3f;
		computeWheelCenterActorOffsets4W(frontZ, rearZ, chassisDims, wheelWidth, wheelRadius, numWheels, wheelCenterActorOffsets);

		//Set up the simulation data for all wheels.
		setupWheelsSimulationData(vehicle4WDesc.wheelMass, vehicle4WDesc.wheelMOI, wheelRadius, wheelWidth, numWheels, wheelCenterActorOffsets,
			 					  vehicle4WDesc.chassisCMOffset, vehicle4WDesc.chassisMass, wheelsSimData);
	}

	//Set up the sim data for the vehicle drive model.
	physx::PxVehicleDriveSimData4W driveSimData;
	{
		//Diff
		physx::PxVehicleDifferential4WData diff;
		diff.mType = physx::PxVehicleDifferential4WData::eDIFF_TYPE_LS_4WD;
		driveSimData.setDiffData(diff);

		//Engine
		physx::PxVehicleEngineData engine;
		engine.mPeakTorque=500.0f;
		engine.mMaxOmega=600.0f;//approx 6000 rpm
		driveSimData.setEngineData(engine);

		//Gears
		physx::PxVehicleGearsData gears;
		gears.mSwitchTime=0.5f;
		driveSimData.setGearsData(gears);

		//Clutch
		physx::PxVehicleClutchData clutch;
		clutch.mStrength=10.0f;
		driveSimData.setClutchData(clutch);

		//Ackermann steer accuracy
		physx::PxVehicleAckermannGeometryData ackermann;
		ackermann.mAccuracy=1.0f;
		ackermann.mAxleSeparation=
			wheelsSimData->getWheelCentreOffset(physx::PxVehicleDrive4WWheelOrder::eFRONT_LEFT).z-
			wheelsSimData->getWheelCentreOffset(physx::PxVehicleDrive4WWheelOrder::eREAR_LEFT).z;
		ackermann.mFrontWidth=
			wheelsSimData->getWheelCentreOffset(physx::PxVehicleDrive4WWheelOrder::eFRONT_RIGHT).x-
			wheelsSimData->getWheelCentreOffset(physx::PxVehicleDrive4WWheelOrder::eFRONT_LEFT).x;
		ackermann.mRearWidth=
			wheelsSimData->getWheelCentreOffset(physx::PxVehicleDrive4WWheelOrder::eREAR_RIGHT).x -
			wheelsSimData->getWheelCentreOffset(physx::PxVehicleDrive4WWheelOrder::eREAR_LEFT).x;
		driveSimData.setAckermannGeometryData(ackermann);
	}

	//Create a vehicle from the wheels and drive sim data.
	physx::PxVehicleDrive4W* vehDrive4W = physx::PxVehicleDrive4W::allocate(numWheels);
	vehDrive4W->setup(physics, veh4WActor, *wheelsSimData, driveSimData, numWheels - 4);

	//Configure the userdata
	configureUserData(vehDrive4W, vehicle4WDesc.actorUserData, vehicle4WDesc.shapeUserDatas);

	//Free the sim data because we don't need that any more.
	wheelsSimData->free();

	return vehDrive4W;
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

		gCooking = PxCreateCooking(PX_PHYSICS_VERSION, *gFoundation, physx::PxTolerancesScale());
	}

	void CreatePhysics(bool trackAllocations) {
		gPhysics = PxCreatePhysics(PX_PHYSICS_VERSION, *gFoundation, physx::PxTolerancesScale(), trackAllocations);
	}

	physx::PxVehicleDrive4WRawInputData gVehicleInputData;
	physx::PxF32 gVehicleModeTimer = 0.0f;
	physx::PxU32 gVehicleOrderProgress = 0;

	void CreateVehicleEnvironment() {
		//	INITIALISATION
		PxInitVehicleSDK(*gPhysics);
		PxVehicleSetBasisVectors(physx::PxVec3(0,1,0), physx::PxVec3(0,0,1));
		PxVehicleSetUpdateMode(physx::PxVehicleUpdateMode::eVELOCITY_CHANGE);

		//Create the batched scene queries for the suspension raycasts.
		gVehicleSceneQueryData = VehicleSceneQueryData::allocate(1, PX_MAX_NB_WHEELS, 1, 1, WheelSceneQueryPreFilterBlocking, NULL, gAllocator);
		gBatchQuery = VehicleSceneQueryData::setUpBatchedSceneQuery(0, *gVehicleSceneQueryData, gScene);

		//Create the friction table for each combination of tire and surface type.
		gFrictionPairs = createFrictionPairs(gMaterial);
		
		//Create a plane to drive on.
		//physx::PxFilterData groundPlaneSimFilterData(COLLISION_FLAG_GROUND, COLLISION_FLAG_GROUND_AGAINST, 0, 0);
		//gGroundPlane = createDrivablePlane(groundPlaneSimFilterData, gMaterial, gPhysics);
		//gScene->addActor(*gGroundPlane);

		//Create a vehicle that will drive on the plane.
		VehicleDesc vehicleDesc = initVehicleDesc();
		physx::PxVehicleDrive4W* gVehicle4W = createVehicle4W(vehicleDesc, gPhysics, gCooking);
		physx::PxTransform startTransform(physx::PxVec3(0, (vehicleDesc.chassisDims.y*0.5f + vehicleDesc.wheelRadius + 1.0f), 0), physx::PxQuat(physx::PxIdentity));
		gVehicle4W->getRigidDynamicActor()->setGlobalPose(startTransform);
		gScene->addActor(*gVehicle4W->getRigidDynamicActor());

		//Set the vehicle to rest in first gear.
		//Set the vehicle to use auto-gears.
		gVehicle4W->setToRestState();
		gVehicle4W->mDriveDynData.forceGearChange(physx::PxVehicleGearsData::eFIRST);
		gVehicle4W->mDriveDynData.setUseAutoGears(true);

		gVehicleModeTimer = 0.0f;
		gVehicleOrderProgress = 0;
		gVehicleInputData.setDigitalBrake(true);
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

	physx::PxGeometry* CreateBoxGeometry(float halfX, float halfY, float halfZ) {
		return new physx::PxBoxGeometry(halfX, halfY, halfZ);
	}

	physx::PxGeometry* CreateSphereGeometry(float radius) {
		return new physx::PxSphereGeometry(radius);
	}

	std::vector<physx::PxVec3>* CreateMeshVertexArray() {
		return new std::vector<physx::PxVec3>();
	}

	void AddVertexToArray(std::vector<physx::PxVec3>* vertexArray, physx::PxVec3* vertex) {
		vertexArray->push_back(*vertex);
	}

	physx::PxGeometry* CreateConvexMeshGeometry(std::vector<physx::PxVec3>* vertexArray) {
		physx::PxConvexMeshDesc convexDesc;
		convexDesc.points.count = vertexArray->size();
		convexDesc.points.stride = sizeof(physx::PxVec3);
		convexDesc.points.data = vertexArray->data();
		convexDesc.flags = physx::PxConvexFlag::eCOMPUTE_CONVEX;

		physx::PxDefaultMemoryOutputStream buffer;
		physx::PxConvexMeshCookingResult::Enum result;

		if(!gCooking->cookConvexMesh(convexDesc, buffer, &result)) {
			debugLog("Mesh cooking failed.");
    		return NULL;
		}

		physx::PxDefaultMemoryInputData input(buffer.getData(), buffer.getSize());
		
		physx::PxConvexMesh* mesh = gPhysics->createConvexMesh(input);
		return new physx::PxConvexMeshGeometry(mesh);
	}

	physx::PxGeometry* CreateMeshGeometry(std::vector<physx::PxVec3>* vertexArray, physx::PxU32* triIndices, physx::PxU32 triCount) {
		physx::PxTriangleMeshDesc meshDesc;
		meshDesc.points.count = vertexArray->size();
		meshDesc.points.stride = sizeof(physx::PxVec3);
		meshDesc.points.data = vertexArray->data();

		meshDesc.triangles.count = triCount;
		meshDesc.triangles.stride = 3 * sizeof(physx::PxU32);
		meshDesc.triangles.data = triIndices;

		physx::PxDefaultMemoryOutputStream buffer;
		physx::PxTriangleMeshCookingResult::Enum result;
		bool status = gCooking->cookTriangleMesh(meshDesc, buffer, &result);
		if(!status)
			return NULL;

		physx::PxDefaultMemoryInputData input(buffer.getData(), buffer.getSize());
		
		physx::PxTriangleMesh* mesh = gPhysics->createTriangleMesh(input);
		return new physx::PxTriangleMeshGeometry(mesh);
	}

	physx::PxTransform* CreateTransform(physx::PxVec3* pos, physx::PxQuat* rot) {
		return new physx::PxTransform(*pos, *rot);
	}

	physx::PxShape* CreateShape(physx::PxGeometry* geometry, physx::PxMaterial* mat) {
		return gPhysics->createShape(*geometry, *mat);
	}

	void SetShapeLocalTransform(physx::PxShape* shape, physx::PxTransform* transform) {
		shape->setLocalPose(*transform);
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

    void RegisterCollisionCallback(CollisionCallback collisionEnterCallback) {
		collisionCallback = collisionEnterCallback;
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

	physx::PxActor* GetPairHeaderActor(physx::PxContactPairHeader* header, int actorNum) {
		return header->actors[actorNum];
	}

	physx::PxShape* GetContactPairShape(physx::PxContactPair* pairs, int i, int actor) {
		return pairs[i].shapes[actor];
	}

	physx::PxContactStreamIterator* GetContactPointIterator(physx::PxContactPair* pairs, int i) {
		iter = physx::PxContactStreamIterator(pairs[i].contactPatches, pairs[i].contactPoints, pairs[i].getInternalFaceIndices(), pairs[i].patchCount, pairs[i].contactCount);

		return &iter;
	}

	bool NextContactPatch(physx::PxContactStreamIterator* iter) {
		if (iter->hasNextPatch()) {
			iter->nextPatch();
			return true;
		}

		return false;
	}

	bool NextContactPoint(physx::PxContactStreamIterator* iter) {
		if (iter->hasNextContact()) {
			iter->nextContact();
			return true;
		}

		return false;
	}

	void GetContactPointData(physx::PxContactStreamIterator* iter, int j, physx::PxContactPair* pairs, int i, physx::PxVec3* point, physx::PxVec3* normal, physx::PxVec3* impulse) {
		*point = iter->getContactPoint();
		*normal = iter->getContactNormal();

		if (pairs[i].flags & physx::PxContactPairFlag::eINTERNAL_HAS_IMPULSES) {
			*impulse = *normal * pairs[i].contactImpulses[j];
		}
	}
}

