//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//  * Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
//  * Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//  * Neither the name of NVIDIA CORPORATION nor the names of its
//    contributors may be used to endorse or promote products derived
//    from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ``AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
// OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// Copyright (c) 2008-2019 NVIDIA Corporation. All rights reserved.
// Copyright (c) 2004-2008 AGEIA Technologies, Inc. All rights reserved.
// Copyright (c) 2001-2004 NovodeX AG. All rights reserved.  

#include "../../../include/foundation/PxTransform.h"
#include "SqBounds.h"
#include "../../common/src/CmTransformUtils.h"
#include "../include/SqPruner.h"
#include "../../physx/src/buffering/ScbShape.h"
#include "../../physx/src/buffering/ScbActor.h"
#include "../../physx/src/buffering/ScbRigidStatic.h"
#include "../../physx/src/buffering/ScbBody.h"
#include "../../foundation/include/PsAllocator.h"
#include "../../geomutils/src/GuBounds.h"

using namespace physx;
using namespace Sq;

void Sq::computeStaticWorldAABB(PxBounds3& bounds, const Scb::Shape& scbShape, const Scb::Actor& scbActor)
{
	const PxTransform& shape2Actor = scbShape.getShape2Actor();

	PX_ALIGN(16, PxTransform) globalPose;

	Cm::getStaticGlobalPoseAligned(static_cast<const Scb::RigidStatic&>(scbActor).getActor2World(), shape2Actor, globalPose);
	Gu::computeBounds(bounds, scbShape.getGeometry(), globalPose, 0.0f, NULL, SQ_PRUNER_INFLATION);
}

void Sq::computeDynamicWorldAABB(PxBounds3& bounds, const Scb::Shape& scbShape, const Scb::Actor& scbActor)
{
	const PxTransform& shape2Actor = scbShape.getShape2Actor();

	PX_ALIGN(16, PxTransform) globalPose;
	{
		const Scb::Body& body = static_cast<const Scb::Body&>(scbActor);
		PX_ALIGN(16, PxTransform) kinematicTarget;
		const PxU16 sqktFlags = PxRigidBodyFlag::eKINEMATIC | PxRigidBodyFlag::eUSE_KINEMATIC_TARGET_FOR_SCENE_QUERIES;
		const bool useTarget = (PxU16(body.getFlags()) & sqktFlags) == sqktFlags;
		const PxTransform& body2World = (useTarget && body.getKinematicTarget(kinematicTarget)) ? kinematicTarget : body.getBody2World();
		Cm::getDynamicGlobalPoseAligned(body2World, shape2Actor, body.getBody2Actor(), globalPose);
	}

	Gu::computeBounds(bounds, scbShape.getGeometry(), globalPose, 0.0f, NULL, SQ_PRUNER_INFLATION);
}

const ComputeBoundsFunc Sq::gComputeBoundsTable[2] = 
{ 
	computeStaticWorldAABB, 
	computeDynamicWorldAABB
};
