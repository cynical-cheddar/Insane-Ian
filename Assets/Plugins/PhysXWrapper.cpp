#include "PhysXWrapper.h"

#define PX_RELEASE(x)    if(x) {x->release(); x = NULL;}

DebugLog dl = NULL;

physx::PxFoundation* gFoundation = NULL;

physx::PxPhysics* gPhysics = NULL;

physx::PxTolerancesScale gToleranceScale;

physx::PxDefaultCpuDispatcher* gDispatcher = NULL;

physx::PxScene* gScene = NULL;

physx::PxCooking* gCooking = NULL;

physx::PxMaterial* gMaterial = NULL;

physx::PxDefaultAllocator gAllocator;

physx::PxBatchQuery* gBatchQuery = NULL;

physx::PxVehicleDrivableSurfaceToTireFrictionPairs* gFrictionPairs = NULL;

physx::PxContactStreamIterator iter(NULL, NULL, NULL, 0, 0);

CollisionCallback collisionCallback = NULL;
TriggerCallback triggerCallback = NULL;

void debugLog(const std::string str) {
    if (dl != NULL) {
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

void trigger(const physx::PxActor* other, const physx::PxShape* otherShape, const physx::PxActor* self, bool isEnter, bool isExit) {
    if (triggerCallback != NULL) {
        triggerCallback(other, otherShape, self, isEnter, isExit);
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
    for (int i = 0; i < count; i++) {
        bool fireBegin = false;
        bool fireEnd = false;

        physx::PxU32 contactTriggerFlags = 0;

        if (!(pairs[i].flags & physx::PxTriggerPairFlag::eREMOVED_SHAPE_TRIGGER)) contactTriggerFlags = pairs[i].triggerShape->getSimulationFilterData().word2;

        if (pairs[i].status & physx::PxPairFlag::eNOTIFY_TOUCH_FOUND) {
            if (contactTriggerFlags & TRIGGER_BEGIN) fireBegin = true;
        }

        if (pairs[i].status & physx::PxPairFlag::eNOTIFY_TOUCH_LOST) {
            if (contactTriggerFlags & TRIGGER_END) fireEnd = true;
        }

        if (fireBegin || fireEnd) {
            trigger(pairs[i].otherActor, pairs[i].otherShape, pairs[i].triggerActor, fireBegin, fireEnd);
        }
    }
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

physx::PxVehicleDrivableSurfaceToTireFrictionPairs* createFrictionPairs(const physx::PxMaterial* defaultMaterial)
{
    physx::PxVehicleDrivableSurfaceType surfaceTypes[1];
    surfaceTypes[0].mType = 0;

    const physx::PxMaterial* surfaceMaterials[1];
    surfaceMaterials[0] = defaultMaterial;

    physx::PxVehicleDrivableSurfaceToTireFrictionPairs* surfaceTirePairs = physx::PxVehicleDrivableSurfaceToTireFrictionPairs::allocate(1, 1);

    surfaceTirePairs->setup(1, 1, surfaceMaterials, surfaceTypes);

    surfaceTirePairs->setTypePairFriction(0, 0, 1);
    return surfaceTirePairs;
}

physx::PxQueryHitType::Enum WheelSceneQueryPreFilterBlocking(physx::PxFilterData filterData0, physx::PxFilterData filterData1, const void* constantBlock, physx::PxU32 constantBlockSize, physx::PxHitFlags& queryFlags) {
    //filterData0 is the vehicle suspension query.
    //filterData1 is the shape potentially hit by the query.

    if (filterData0.word3 == filterData1.word3) return physx::PxQueryHitType::eNONE;
    return physx::PxQueryHitType::eBLOCK;
}

RaycastQueryFilter gQueryFilterCallback;

RaycastQueryFilter::~RaycastQueryFilter() {

}

physx::PxQueryHitType::Enum RaycastQueryFilter::preFilter(const physx::PxFilterData &filterData, const physx::PxShape *shape, const physx::PxRigidActor *actor, physx::PxHitFlags &queryFlags) {
    physx::PxFilterData shapeFilterData = shape->getQueryFilterData();

    //filterData0 is the query.
    //filterData1 is the shape potentially hit by the query.

    if (filterData.word3 != 0 && filterData.word3 == shapeFilterData.word3) return physx::PxQueryHitType::eNONE;

    if (filterData.word1 & shapeFilterData.word1) {
        return physx::PxQueryHitType::eBLOCK;
    }

    return physx::PxQueryHitType::eNONE;
}

physx::PxQueryHitType::Enum RaycastQueryFilter::postFilter(const physx::PxFilterData &filterData, const physx::PxQueryHit &hit) {
    return physx::PxQueryHitType::eBLOCK;
}

RaycastHitHandler::RaycastHitHandler(physx::PxRaycastHit* hitBuffer, physx::PxU32 bufferSize) : physx::PxRaycastCallback(hitBuffer, bufferSize) {

}

physx::PxAgain RaycastHitHandler::processTouches(const physx::PxRaycastHit* hits, physx::PxU32 hitCount) {
    return false;
}

physx::PxVehicleDrive4WRawInputData gVehicleInputData;
physx::PxF32 gVehicleModeTimer = 0.0f;
physx::PxU32 gVehicleOrderProgress = 0;

extern "C" {
    EXPORT_FUNC void RegisterDebugLog(DebugLog debl) {
        dl = debl;
    }

    EXPORT_FUNC void SetupFoundation() {
        gFoundation = PxCreateFoundation(PX_PHYSICS_VERSION, gAllocator, gErrorCallback);

        if (gFoundation == 0) {
            gFoundation = &physx::shdfnd::Foundation::getInstance();
        }

        gCooking = PxCreateCooking(PX_PHYSICS_VERSION, *gFoundation, gToleranceScale);
    }

    EXPORT_FUNC void CreatePhysics(bool trackAllocations) {
        gPhysics = PxCreatePhysics(PX_PHYSICS_VERSION, *gFoundation, gToleranceScale, trackAllocations);
    }

    EXPORT_FUNC void CreateVehicleEnvironment(physx::PxVec3* up, physx::PxVec3* forward) {
        PxInitVehicleSDK(*gPhysics);
        PxVehicleSetBasisVectors(*up, *forward);
        PxVehicleSetUpdateMode(physx::PxVehicleUpdateMode::eVELOCITY_CHANGE);

        gFrictionPairs = createFrictionPairs(gMaterial);
    }

    EXPORT_FUNC physx::PxScene* CreateScene(physx::PxVec3* gravity) {
        physx::PxSceneDesc sceneDesc(gPhysics->getTolerancesScale());
        sceneDesc.gravity = *gravity;
        gDispatcher = physx::PxDefaultCpuDispatcherCreate(0);
        sceneDesc.cpuDispatcher    = gDispatcher;
        sceneDesc.filterShader    = FilterShader;
        sceneDesc.simulationEventCallback = &collisionHandler;
        sceneDesc.bounceThresholdVelocity = 2;
        sceneDesc.solverType = physx::PxSolverType::eTGS;

        physx::PxScene* scene = gPhysics->createScene(sceneDesc);

        scene->setDominanceGroupPair(1, 0, physx::PxDominanceGroupPair(0, 1));

        SceneUserData* sceneUserData = new SceneUserData();
        scene->userData = (void*)sceneUserData;

        return scene;
    }

    EXPORT_FUNC physx::PxMaterial* CreateMaterial(float staticFriction, float dynamicFriction, float restitution) {
        return gPhysics->createMaterial(staticFriction, dynamicFriction, restitution);
    }

    EXPORT_FUNC physx::PxGeometry* CreateBoxGeometry(float halfX, float halfY, float halfZ) {
        return new physx::PxBoxGeometry(halfX, halfY, halfZ);
    }

    EXPORT_FUNC physx::PxGeometry* CreateSphereGeometry(float radius) {
        return new physx::PxSphereGeometry(radius);
    }

    EXPORT_FUNC std::vector<physx::PxVec3>* CreateVectorArray() {
        return new std::vector<physx::PxVec3>();
    }

    EXPORT_FUNC void AddVectorToArray(std::vector<physx::PxVec3>* vectorArray, physx::PxVec3* vector) {
        vectorArray->push_back(*vector);
    }

    EXPORT_FUNC physx::PxGeometry* CreateConvexMeshGeometry(std::vector<physx::PxVec3>* vertexArray) {
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
        physx::PxConvexMeshGeometry* geometry = new physx::PxConvexMeshGeometry(mesh);
        geometry->meshFlags = physx::PxConvexMeshGeometryFlag::eTIGHT_BOUNDS;
        return geometry;
    }

    EXPORT_FUNC physx::PxGeometry* CreateMeshGeometry(std::vector<physx::PxVec3>* vertexArray, physx::PxU32* triIndices, physx::PxU32 triCount) {
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
        if (!status) return NULL;

        physx::PxDefaultMemoryInputData input(buffer.getData(), buffer.getSize());
        
        physx::PxTriangleMesh* mesh = gPhysics->createTriangleMesh(input);
        return new physx::PxTriangleMeshGeometry(mesh);
    }

    EXPORT_FUNC physx::PxTransform* CreateTransform(physx::PxVec3* pos, physx::PxQuat* rot) {
        return new physx::PxTransform(*pos, *rot);
    }

    EXPORT_FUNC physx::PxShape* CreateShape(physx::PxGeometry* geometry, physx::PxMaterial* mat, physx::PxReal contactOffset) {
        physx::PxShape* shape = gPhysics->createShape(*geometry, *mat);
        shape->setContactOffset(contactOffset);
        return shape;
    }

    EXPORT_FUNC void SetShapeLocalTransform(physx::PxShape* shape, physx::PxTransform* transform) {
        shape->setLocalPose(*transform);
    }

    EXPORT_FUNC void SetShapeSimulationFlag(physx::PxShape* shape, bool value) {
        shape->setFlag(physx::PxShapeFlag::eSIMULATION_SHAPE, value);
    }

    EXPORT_FUNC void SetShapeTriggerFlag(physx::PxShape* shape, bool value) {
        shape->setFlag(physx::PxShapeFlag::eTRIGGER_SHAPE, value);
    }

    EXPORT_FUNC void SetShapeSceneQueryFlag(physx::PxShape* shape, bool value) {
        shape->setFlag(physx::PxShapeFlag::eSCENE_QUERY_SHAPE, value);
    }

    EXPORT_FUNC physx::PxRigidDynamic* CreateDynamicRigidBody(physx::PxTransform* pose) {
        physx::PxRigidDynamic* actor = gPhysics->createRigidDynamic(*pose);
        actor->setSolverIterationCounts(20, 10);
        actor->userData = (void *)(new ActorUserData());
        return actor;
    }

    EXPORT_FUNC physx::PxRigidStatic* CreateStaticRigidBody(physx::PxTransform* pose) {
        physx::PxRigidStatic* actor = gPhysics->createRigidStatic(*pose);
        actor->userData = (void *)(new ActorUserData());
        return actor;
    }

    EXPORT_FUNC void SetCollisionFilterData(physx::PxShape* shape, physx::PxU32 w0, physx::PxU32 w1, physx::PxU32 w2, physx::PxU32 w3) {
        debugLog(std::to_string(w3));
        shape->setSimulationFilterData(physx::PxFilterData(w0, w1, w2, w3));
    }

    EXPORT_FUNC void SetQueryFilterData(physx::PxShape* shape, physx::PxU32 w0, physx::PxU32 w1, physx::PxU32 w2, physx::PxU32 w3) {
        debugLog(std::to_string(w3));
        shape->setQueryFilterData(physx::PxFilterData(w0, w1, w2, w3));
    }

    EXPORT_FUNC int AttachShapeToRigidBody(physx::PxShape* shape, physx::PxRigidActor* body) {
        body->attachShape(*shape);
        return body->getNbShapes() - 1;
    }

    EXPORT_FUNC physx::PxVehicleWheelData* CreateWheelData() {
        physx::PxVehicleWheelData* wheel = new physx::PxVehicleWheelData();
        wheel->mMaxBrakeTorque = PX_MAX_F32 - 1;
        wheel->mMaxHandBrakeTorque = PX_MAX_F32 - 1;
        wheel->mMaxSteer = (physx::PxPi / 2) - 0.0001f;
        return wheel;
    }

    EXPORT_FUNC void SetWheelRadius(physx::PxVehicleWheelData* wheel, physx::PxReal radius) {
        wheel->mRadius = radius;
    }

    EXPORT_FUNC void SetWheelWidth(physx::PxVehicleWheelData* wheel, physx::PxReal width) {
        wheel->mWidth = width;
    }

    EXPORT_FUNC void SetWheelMass(physx::PxVehicleWheelData* wheel, physx::PxReal mass) {
        wheel->mMass = mass;
    }

    EXPORT_FUNC void SetWheelMomentOfInertia(physx::PxVehicleWheelData* wheel, physx::PxReal momentOfInertia) {
        wheel->mMOI = momentOfInertia;
    }

    EXPORT_FUNC void SetWheelDampingRate(physx::PxVehicleWheelData* wheel, physx::PxReal dampingRate) {
        wheel->mDampingRate = dampingRate;
    }

    EXPORT_FUNC physx::PxVehicleTireData* CreateTireData() {
        physx::PxVehicleTireData* tire = new physx::PxVehicleTireData();
        tire->mType = 0;
        return tire;
    }

    EXPORT_FUNC void SetTireLateralStiffnessMaxLoad(physx::PxVehicleTireData* tire, physx::PxReal maxLoad) {
        tire->mLatStiffX = maxLoad;
    }

    EXPORT_FUNC void SetTireMaxLateralStiffness(physx::PxVehicleTireData* tire, physx::PxReal maxStiffness) {
        tire->mLatStiffY = maxStiffness;
    }

    EXPORT_FUNC void SetTireLongitudinalStiffnessScale(physx::PxVehicleTireData* tire, physx::PxReal stiffnessScale) {
        tire->mLongitudinalStiffnessPerUnitGravity = stiffnessScale;
    }

    EXPORT_FUNC void SetTireBaseFriction(physx::PxVehicleTireData* tire, physx::PxReal friction) {
        tire->mFrictionVsSlipGraph[0][1] = friction;
    }

    EXPORT_FUNC void SetTireMaxFrictionSlipPoint(physx::PxVehicleTireData* tire, physx::PxReal slipPoint) {
        tire->mFrictionVsSlipGraph[1][0] = slipPoint;
    }

    EXPORT_FUNC void SetTireMaxFriction(physx::PxVehicleTireData* tire, physx::PxReal friction) {
        tire->mFrictionVsSlipGraph[1][1] = friction;
    }

    EXPORT_FUNC void SetTirePlateuxSlipPoint(physx::PxVehicleTireData* tire, physx::PxReal slipPoint) {
        tire->mFrictionVsSlipGraph[2][0] = slipPoint;
    }

    EXPORT_FUNC void SetTirePlateuxFriction(physx::PxVehicleTireData* tire, physx::PxReal friction) {
        tire->mFrictionVsSlipGraph[2][1] = friction;
    }

    EXPORT_FUNC physx::PxVehicleSuspensionData* CreateSuspensionData() {
        return new physx::PxVehicleSuspensionData();
    }

    EXPORT_FUNC void SetSuspensionSpringStrength(physx::PxVehicleSuspensionData* suspension, physx::PxReal strength) {
        suspension->mSpringStrength = strength;
    }

    EXPORT_FUNC void SetSuspensionSpringDamper(physx::PxVehicleSuspensionData* suspension, physx::PxReal damper) {
        suspension->mSpringDamperRate = damper;
    }

    EXPORT_FUNC void SetSuspensionMaxCompression(physx::PxVehicleSuspensionData* suspension, physx::PxReal maxCompression) {
        suspension->mMaxCompression = maxCompression;
    }

    EXPORT_FUNC void SetSuspensionMaxDroop(physx::PxVehicleSuspensionData* suspension, physx::PxReal maxDroop) {
        suspension->mMaxDroop = maxDroop;
    }

    EXPORT_FUNC void SetSuspensionSprungMasses(physx::PxVehicleSuspensionData** suspensions, physx::PxU32 wheelCount, std::vector<physx::PxVec3>* wheelPositions, physx::PxVec3* centreOfMass, physx::PxReal mass) {
        std::vector<physx::PxReal> sprungMasses(wheelCount);
        physx::PxVehicleComputeSprungMasses(wheelCount, wheelPositions->data(), *centreOfMass, mass, 1, sprungMasses.data());
        for (int i = 0; i < wheelCount; i++) {
            suspensions[i]->mSprungMass = sprungMasses[i];
        }
    }

    EXPORT_FUNC physx::PxVehicleWheelsSimData* CreateWheelSimData(physx::PxU32 wheelCount) {
        return physx::PxVehicleWheelsSimData::allocate(wheelCount);
    }

    EXPORT_FUNC void SetWheelSimWheelData(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVehicleWheelData* wheel) {
        wheelSimData->setWheelData(wheelNum, *wheel);
    }

    EXPORT_FUNC void SetWheelSimTireData(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVehicleTireData* tire) {
        wheelSimData->setTireData(wheelNum, *tire);
    }

    EXPORT_FUNC void SetWheelSimSuspensionData(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVehicleSuspensionData* suspension, physx::PxVec3* down) {
        wheelSimData->setSuspensionData(wheelNum, *suspension);
        wheelSimData->setSuspTravelDirection(wheelNum, *down);
    }

    EXPORT_FUNC void SetWheelSimWheelCentre(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVec3* centre) {
        wheelSimData->setWheelCentreOffset(wheelNum, *centre);
    }

    EXPORT_FUNC void SetWheelSimForceAppPoint(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxVec3* point) {
        wheelSimData->setSuspForceAppPointOffset(wheelNum, *point);
        wheelSimData->setTireForceAppPointOffset(wheelNum, *point);
    }

    EXPORT_FUNC void SetWheelSimQueryFilterData(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxU32 w0, physx::PxU32 w1, physx::PxU32 w2, physx::PxU32 w3) {
        wheelSimData->setSceneQueryFilterData(wheelNum, physx::PxFilterData(w0, w1, w2, w3));
    }

    EXPORT_FUNC void SetWheelSimWheelShape(physx::PxVehicleWheelsSimData* wheelSimData, physx::PxU32 wheelNum, physx::PxU32 shapeNum) {
        wheelSimData->setWheelShapeMapping(wheelNum, physx::PxI32(shapeNum));
    }

    EXPORT_FUNC physx::PxVehicleNoDrive* CreateVehicleFromRigidBody(physx::PxRigidDynamic* body, physx::PxVehicleWheelsSimData* wheelSimData) {
        physx::PxVehicleNoDrive* vehicle = physx::PxVehicleNoDrive::allocate(wheelSimData->getNbWheels());
        vehicle->setup(gPhysics, body, *wheelSimData);

        ActorUserData* actorUserData = ((ActorUserData*)body->userData);
        SceneUserData* sceneUserData = ((SceneUserData*)actorUserData->scene->userData);

        sceneUserData->vehicles.push_back(vehicle);
        sceneUserData->wheelCount += wheelSimData->getNbWheels();

        for (int i = 0; i < wheelSimData->getNbWheels(); i++) {
            actorUserData->queryResults.push_back(physx::PxWheelQueryResult());

            sceneUserData->raycastResults.push_back(physx::PxRaycastQueryResult());
            sceneUserData->raycastHits.push_back(physx::PxRaycastHit());
        }

        if (sceneUserData->suspensionBatchQuery != NULL) {
            PX_RELEASE(sceneUserData->suspensionBatchQuery);
        }


        physx::PxBatchQueryDesc batchQueryDesc(sceneUserData->wheelCount, sceneUserData->wheelCount, 1);
        batchQueryDesc.queryMemory.userRaycastResultBuffer = sceneUserData->raycastResults.data();
        batchQueryDesc.queryMemory.userRaycastTouchBuffer = sceneUserData->raycastHits.data();
        batchQueryDesc.queryMemory.raycastTouchBufferSize = sceneUserData->raycastHits.size();

        batchQueryDesc.preFilterShader = WheelSceneQueryPreFilterBlocking;

        sceneUserData->suspensionBatchQuery = actorUserData->scene->createBatchQuery(batchQueryDesc);

        physx::PxVehicleWheelQueryResult queryResult;
        queryResult.wheelQueryResults = actorUserData->queryResults.data();
        queryResult.nbWheelQueryResults = wheelSimData->getNbWheels();

        sceneUserData->queryResults.push_back(queryResult);

        vehicle->setToRestState();

        return vehicle;
    }

    EXPORT_FUNC physx::PxVehicleWheelsSimData* GetWheelSimData(physx::PxVehicleWheels* vehicle) {
        return &vehicle->mWheelsSimData;
    }

    EXPORT_FUNC physx::PxVehicleWheelsDynData* GetWheelDynData(physx::PxVehicleWheels* vehicle) {
        return &vehicle->mWheelsDynData;
    }

    EXPORT_FUNC void SetWheelDynTireData(physx::PxVehicleWheelsDynData* wheelDynData, physx::PxU32 wheelNum, physx::PxVehicleTireData* tire) {
        wheelDynData->setTireForceShaderData(wheelNum, tire);
    }

    EXPORT_FUNC void SetWheelSteer(physx::PxVehicleNoDrive* vehicle, physx::PxU32 wheelNum, physx::PxReal steerAngle) {
        vehicle->setSteerAngle(wheelNum, steerAngle);
    }

    EXPORT_FUNC void SetWheelDrive(physx::PxVehicleNoDrive* vehicle, physx::PxU32 wheelNum, physx::PxReal driveTorque) {
        vehicle->setDriveTorque(wheelNum, driveTorque);
    }

    EXPORT_FUNC void SetWheelBrake(physx::PxVehicleNoDrive* vehicle, physx::PxU32 wheelNum, physx::PxReal brakeTorque) {
        vehicle->setBrakeTorque(wheelNum, brakeTorque);
    }

    EXPORT_FUNC void UpdateVehicleCentreOfMass(physx::PxTransform* oldCentre, physx::PxTransform* newCentre, physx::PxVehicleWheels* vehicle) {
        physx::PxVec3 change = oldCentre->p - newCentre->p;

        for (int i = 0; i < vehicle->mWheelsSimData.getNbWheels(); i++) {
            physx::PxVec3 suspAppPoint = vehicle->mWheelsSimData.getSuspForceAppPointOffset(i);
            vehicle->mWheelsSimData.setSuspForceAppPointOffset(i, suspAppPoint + change);

            physx::PxVec3 tireAppPoint = vehicle->mWheelsSimData.getTireForceAppPointOffset(i);
            vehicle->mWheelsSimData.setTireForceAppPointOffset(i, tireAppPoint + change);

            physx::PxVec3 wheelCentre = vehicle->mWheelsSimData.getWheelCentreOffset(i);
            vehicle->mWheelsSimData.setWheelCentreOffset(i, wheelCentre + change);
        }
    }

    EXPORT_FUNC void RegisterCollisionCallback(CollisionCallback onCollisionCallback) {
        collisionCallback = onCollisionCallback;
    }

    EXPORT_FUNC void RegisterTriggerCallback(TriggerCallback onTriggerCallback) {
        triggerCallback = onTriggerCallback;
    }

    EXPORT_FUNC void SetRigidBodyMassAndInertia(physx::PxRigidBody* body, float mass, const physx::PxVec3* massLocalPose) {
        physx::PxRigidBodyExt::setMassAndUpdateInertia(*body, mass, massLocalPose);
    }

    EXPORT_FUNC void SetRigidBodyMassPose(physx::PxRigidBody* body, physx::PxTransform* pose) {
        body->setCMassLocalPose(*pose);
    }

    EXPORT_FUNC void SetRigidBodyDamping(physx::PxRigidBody* body, float linear, float angular) {
        body->setLinearDamping(linear);
        body->setAngularDamping(angular);
    }

    EXPORT_FUNC void SetRigidBodyFlag(physx::PxRigidBody* body, physx::PxRigidBodyFlag::Enum flag, bool value) {
        body->setRigidBodyFlag(flag, value);
    }

    EXPORT_FUNC void SetRigidBodyDominanceGroup(physx::PxRigidBody* body, physx::PxDominanceGroup group) {
        body->setDominanceGroup(group);
    }

    EXPORT_FUNC void SetRigidBodyMaxDepenetrationVelocity(physx::PxRigidBody* body, physx::PxReal velocity) {
        body->setMaxDepenetrationVelocity(velocity);
    }

    EXPORT_FUNC void AddActorToScene(physx::PxScene* scene, physx::PxActor* actor) {
        scene->addActor(*actor);
        ((ActorUserData*)actor->userData)->scene = scene;
    }

    EXPORT_FUNC void StepPhysics(physx::PxScene* scene, float time) {
        int substeps = 1;
        float substepTime = time / substeps;

        for (int i = 0; i < substeps; i++) {
            SceneUserData* sceneUserData = ((SceneUserData*)scene->userData);

            physx::PxU32 vehicleCount = sceneUserData->vehicles.size();
            physx::PxVehicleWheels** vehicles = sceneUserData->vehicles.data();
            physx::PxU32 wheelCount = sceneUserData->wheelCount;
            physx::PxBatchQuery* batchQuery = sceneUserData->suspensionBatchQuery;
            physx::PxRaycastQueryResult* raycastResults = sceneUserData->raycastResults.data();
            physx::PxVehicleWheelQueryResult* queryResults = sceneUserData->queryResults.data();

            if (vehicleCount > 0) {
                PxVehicleSuspensionRaycasts(batchQuery, vehicleCount, vehicles, wheelCount, raycastResults);

                PxVehicleUpdates(substepTime, scene->getGravity(), *gFrictionPairs, vehicleCount, vehicles, queryResults);
            }

            scene->simulate(substepTime);
            scene->fetchResults(true);
        }
    }

    bool usingA = true;
    physx::PxTransform centreOfMassA;
    physx::PxTransform centreOfMassB;

    EXPORT_FUNC physx::PxTransform* GetCentreOfMass(physx::PxRigidBody* body) {
        if (usingA) {
            usingA = false;

            centreOfMassA = body->getCMassLocalPose();
            return &centreOfMassA;
        }
        else {
            usingA = true;

            centreOfMassB = body->getCMassLocalPose();
            return &centreOfMassB;
        }
    }

    EXPORT_FUNC void GetPosition(physx::PxRigidActor* actor, physx::PxVec3* position) {
        *position = actor->getGlobalPose().p;
    }

    EXPORT_FUNC void GetRotation(physx::PxRigidActor* actor, physx::PxQuat* rotation) {
        *rotation = actor->getGlobalPose().q;
    }

    EXPORT_FUNC void SetPosition(physx::PxRigidActor* actor, physx::PxVec3* position) {
        physx::PxQuat rotation = actor->getGlobalPose().q;
        actor->setGlobalPose(physx::PxTransform(*position, rotation));
    }

    EXPORT_FUNC void SetRotation(physx::PxRigidActor* actor, physx::PxQuat* rotation) {
        physx::PxVec3 position = actor->getGlobalPose().p;
        actor->setGlobalPose(physx::PxTransform(position, *rotation));
    }

    EXPORT_FUNC void GetLinearVelocity(physx::PxRigidBody* rigidBody, physx::PxVec3* velocity) {
        *velocity = rigidBody->getLinearVelocity();
    }

    EXPORT_FUNC void GetAngularVelocity(physx::PxRigidBody* rigidBody, physx::PxVec3* velocity) {
        *velocity = rigidBody->getAngularVelocity();
    }

    EXPORT_FUNC void SetLinearVelocity(physx::PxRigidBody* body, physx::PxVec3* velocity) {
        body->setLinearVelocity(*velocity);
    }

    EXPORT_FUNC void AddForce(physx::PxRigidBody* rigidBody, physx::PxVec3* force, physx::PxForceMode::Enum forceMode) {
        rigidBody->addForce(*force, forceMode);
    }

    EXPORT_FUNC void AddForceAtPosition(physx::PxRigidBody* rigidBody, physx::PxVec3* force, physx::PxVec3* position, physx::PxForceMode::Enum forceMode) {
        physx::PxRigidBodyExt::addForceAtPos(*rigidBody, *force, *position, forceMode);
    }

    EXPORT_FUNC void AddTorque(physx::PxRigidBody* rigidBody, physx::PxVec3* torque, physx::PxForceMode::Enum forceMode) {
        rigidBody->addTorque(*torque, forceMode);
    }

    EXPORT_FUNC physx::PxActor* GetPairHeaderActor(physx::PxContactPairHeader* header, int actorNum) {
        return header->actors[actorNum];
    }

    EXPORT_FUNC physx::PxShape* GetContactPairShape(physx::PxContactPair* pairs, int i, int actor) {
        return pairs[i].shapes[actor];
    }

    EXPORT_FUNC physx::PxContactStreamIterator* GetContactPointIterator(physx::PxContactPair* pairs, int i) {
        iter = physx::PxContactStreamIterator(pairs[i].contactPatches, pairs[i].contactPoints, pairs[i].getInternalFaceIndices(), pairs[i].patchCount, pairs[i].contactCount);

        return &iter;
    }

    EXPORT_FUNC bool NextContactPatch(physx::PxContactStreamIterator* iter) {
        if (iter->hasNextPatch()) {
            iter->nextPatch();
            return true;
        }

        return false;
    }

    EXPORT_FUNC bool NextContactPoint(physx::PxContactStreamIterator* iter) {
        if (iter->hasNextContact()) {
            iter->nextContact();
            return true;
        }

        return false;
    }

    EXPORT_FUNC void GetContactPointData(physx::PxContactStreamIterator* iter, int j, physx::PxContactPair* pairs, int i, physx::PxVec3* point, physx::PxVec3* normal, physx::PxVec3* impulse) {
        *point = iter->getContactPoint();
        *normal = iter->getContactNormal();

        if (pairs[i].flags & physx::PxContactPairFlag::eINTERNAL_HAS_IMPULSES) {
            *impulse = *normal * pairs[i].contactImpulses[j];
        }
    }

    EXPORT_FUNC physx::PxReal GetSuspensionCompression(physx::PxVehicleWheels* vehicle, physx::PxU32 wheelNum) {
        ActorUserData* actorUserData = (ActorUserData*)vehicle->getRigidDynamicActor()->userData;
        return actorUserData->queryResults[wheelNum].suspJounce;
    }

    EXPORT_FUNC void GetWheelTransform(physx::PxVehicleWheels* vehicle, physx::PxU32 wheelNum, physx::PxVec3* position, physx::PxQuat* rotation) {
        ActorUserData* actorUserData = (ActorUserData*)vehicle->getRigidDynamicActor()->userData;
        *position = actorUserData->queryResults[wheelNum].localPose.p;
        *rotation = actorUserData->queryResults[wheelNum].localPose.q;
    }

    EXPORT_FUNC physx::PxReal GetSuspensionSprungMass(physx::PxVehicleSuspensionData* suspension) {
        return suspension->mSprungMass;
    }

    EXPORT_FUNC void GetTransformComponents(physx::PxTransform* transform, physx::PxVec3* position, physx::PxQuat* rotation) {
        *position = transform->p;
        *rotation = transform->q;
    }

    EXPORT_FUNC physx::PxShape* GetGroundHitShape(physx::PxVehicleWheels* vehicle, physx::PxU32 wheelNum) {
        ActorUserData* actorUserData = (ActorUserData*)vehicle->getRigidDynamicActor()->userData;

        return actorUserData->queryResults[wheelNum].tireContactShape;
    }

    EXPORT_FUNC void GetGroundHitPosition(physx::PxVehicleWheels* vehicle, physx::PxU32 wheelNum, physx::PxVec3* position) {
        ActorUserData* actorUserData = (ActorUserData*)vehicle->getRigidDynamicActor()->userData;

        *position = actorUserData->queryResults[wheelNum].tireContactPoint;
    }

    EXPORT_FUNC bool GetGroundHitIsGrounded(physx::PxVehicleWheels* vehicle, physx::PxU32 wheelNum, physx::PxVec3* position) {
        ActorUserData* actorUserData = (ActorUserData*)vehicle->getRigidDynamicActor()->userData;

        return !actorUserData->queryResults[wheelNum].isInAir;
    }

    EXPORT_FUNC void DestroyActor(physx::PxActor* actor) {
        ActorUserData* actorUserData = (ActorUserData*)actor->userData;
        delete actorUserData;

        actor->release();
    }

    EXPORT_FUNC void DestroyVehicle(physx::PxVehicleNoDrive* vehicle) {
        vehicle->free();
    }

    EXPORT_FUNC physx::PxRaycastCallback* CreateRaycastHit() {
        return new RaycastHitHandler(NULL, 0);
    }

    EXPORT_FUNC bool FireRaycast(physx::PxScene* scene, physx::PxVec3* origin, physx::PxVec3* direction, physx::PxReal distance, physx::PxRaycastCallback* raycastHit) {
        return scene->raycast(*origin, *direction, distance, *raycastHit);
    }

    EXPORT_FUNC bool FireRaycastFiltered(physx::PxScene* scene, physx::PxVec3* origin, physx::PxVec3* direction, physx::PxReal distance, physx::PxRaycastCallback* raycastHit, physx::PxU32 w0, physx::PxU32 w1, physx::PxU32 w2, physx::PxU32 w3) {
        physx::PxQueryFlags flags = physx::PxQueryFlag::eSTATIC | physx::PxQueryFlag::eDYNAMIC | physx::PxQueryFlag::ePREFILTER;
        return scene->raycast(*origin, *direction, distance, *raycastHit, physx::PxHitFlag::eDEFAULT, physx::PxQueryFilterData(physx::PxFilterData(w0, w1, w2, w3), flags), &gQueryFilterCallback);
    }

    EXPORT_FUNC void GetRaycastHitNormal(physx::PxRaycastCallback* raycastHit, physx::PxVec3* normal) {
        *normal = raycastHit->block.normal;
    }

    EXPORT_FUNC void GetRaycastHitPoint(physx::PxRaycastCallback* raycastHit, physx::PxVec3* point) {
        *point = raycastHit->block.position;
    }

    EXPORT_FUNC physx::PxShape* GetRaycastHitShape(physx::PxRaycastCallback* raycastHit) {
        return raycastHit->block.shape;
    }

    EXPORT_FUNC physx::PxActor* GetRaycastHitActor(physx::PxRaycastCallback* raycastHit) {
        return raycastHit->block.actor;
    }

    EXPORT_FUNC physx::PxReal GetRaycastHitDistance(physx::PxRaycastCallback* raycastHit) {
        return raycastHit->block.distance;
    }

    EXPORT_FUNC void DestroyRaycastHit(physx::PxRaycastCallback* raycastHit) {
        delete raycastHit;
    }
}

