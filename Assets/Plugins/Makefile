NAME := PhysXWrapper

PHYSX_DEFS := -D NO_AUTO_DEFS -D _DEBUG -D __STDC_LIMIT_MACROS -D ON_MAC

SHARED_FLAG := -shared

LIBS := macLibs/libPhysXExtensions_static_64.a
LIBS += macLibs/libPhysXFoundation_static_64.a
LIBS += macLibs/libPhysXCooking_static_64.a
LIBS += macLibs/libPhysXCharacterKinematic_static_64.a
LIBS += macLibs/libPhysX_static_64.a
LIBS += macLibs/libPhysXPvdSDK_static_64.a
LIBS += macLibs/libPhysXVehicle_static_64.a
LIBS += macLibs/libPhysXCommon_static_64.a

default: buildLibrary

buildLibrary:
	$(CXX) $(PHYSX_DEFS) -ggdb -g3 -c -o $(NAME).o $(NAME).cpp
	$(CXX) $(SHARED_FLAG) -ggdb -g3 -o $(NAME).dylib $(NAME).o $(LIBS)
	rm $(NAME).o
