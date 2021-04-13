NAME := PhysXWrapper

PHYSX_SOURCE := physx/source
PHYSX_BUILD := ../../PhysXBuild

IMPORT_FLAGS :=

IMPORT_FLAGS += -I../../PhysXStripped/sys/include
IMPORT_FLAGS += -I../../PhysXStripped/sys/include/libcxx
IMPORT_FLAGS += -I../../PhysXStripped/sys/include/libc
IMPORT_FLAGS += -I../../PhysXStripped/sys/lib/libc/musl/arch/emscripten

IMPORT_FLAGS += -I../../PhysXStripped/include
IMPORT_FLAGS += -I../../PhysXStripped/source/common/src
IMPORT_FLAGS += -I../../PhysXStripped/source/foundation/include
IMPORT_FLAGS += -I../../PhysXStripped/source/foundation/include/unix
IMPORT_FLAGS += -I../../PhysXStripped/include/common
IMPORT_FLAGS += -I../../PhysXStripped/source/lowlevel/api/include
IMPORT_FLAGS += -I../../PhysXStripped/source/lowlevel/software/include
IMPORT_FLAGS += -I../../PhysXStripped/source/lowlevel/common/include/pipeline
IMPORT_FLAGS += -I../../PhysXStripped/source/lowlevel/common/include/utils
IMPORT_FLAGS += -I../../PhysXStripped/source/lowlevel/common/include/collision
IMPORT_FLAGS += -I../../PhysXStripped/source/lowlevelaabb/src
IMPORT_FLAGS += -I../../PhysXStripped/source/lowlevelaabb/include
IMPORT_FLAGS += -I../../PhysXStripped/source/lowleveldynamics/src
IMPORT_FLAGS += -I../../PhysXStripped/source/lowleveldynamics/include
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/src
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/src/mesh
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/src/hf
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/src/convex
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/src/intersection
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/src/distance
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/src/gjk
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/src/contact
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/src/common
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/src/pcm
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/src/sweep
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/src/ccd
IMPORT_FLAGS += -I../../PhysXStripped/source/geomutils/include
IMPORT_FLAGS += -I../../PhysXStripped/source/physx/src
IMPORT_FLAGS += -I../../PhysXStripped/source/physx/src/buffering
IMPORT_FLAGS += -I../../PhysXStripped/source/physx/src/device
IMPORT_FLAGS += -I../../PhysXStripped/source/scenequery/include
IMPORT_FLAGS += -I../../PhysXStripped/source/simulationcontroller/src
IMPORT_FLAGS += -I../../PhysXStripped/source/simulationcontroller/include
IMPORT_FLAGS += -I../../PhysXStripped/source/pvd/src
IMPORT_FLAGS += -I../../PhysXStripped/source/pvd/include
IMPORT_FLAGS += -I../../PhysXStripped/source/scenequery/src
#IMPORT_FLAGS += -I../../PhysXStripped/source/physxcharacterkinematic/src
IMPORT_FLAGS += -I../../PhysXStripped/source/physxmetadata/core/include
IMPORT_FLAGS += -I../../PhysXStripped/source/physxmetadata/extensions/include
IMPORT_FLAGS += -I../../PhysXStripped/source/filebuf/include
IMPORT_FLAGS += -I../../PhysXStripped/source/physxextensions/src
#IMPORT_FLAGS += -I../../PhysXStripped/source/physxextensions/src/serialization
#IMPORT_FLAGS += -I../../PhysXStripped/source/physxextensions/src/serialization/Binary
#IMPORT_FLAGS += -I../../PhysXStripped/source/physxextensions/src/serialization/Xml
#IMPORT_FLAGS += -I../../PhysXStripped/source/physxextensions/src/serialization/File
#IMPORT_FLAGS += -I../../PhysXStripped/source/fastxml/include
IMPORT_FLAGS += -I../../PhysXStripped/source/physxgpu/include

IMPORT_FLAGS :=


META_HEADERS :=  $(shell find $(PHYSX_SOURCE) -type f -name '*.h.meta')

SOURCE :=  $(shell find $(PHYSX_SOURCE) -type f -name '*.cpp')
WINDOWS_SOURCE :=  $(shell find $(PHYSX_SOURCE) -type f -path '*/windows/*' -name '*.cpp')
UNIX_SOURCE :=  $(shell find $(PHYSX_SOURCE) -type f -path '*/unix/*' -name '*.cpp')
LINUX_SOURCE :=  $(shell find $(PHYSX_SOURCE) -type f -path '*/linux/*' -name '*.cpp')
PVD_SOURCE :=  $(shell find $(PHYSX_SOURCE) -type f -path '*/pvd/*' -name '*.cpp')
EXT_SOURCE :=  $(shell find $(PHYSX_SOURCE) -type f -path '*/serialization/*' -name '*.cpp')
EXT_SOURCE +=  $(shell find $(PHYSX_SOURCE) -type f -path '*/serialization' -name '*.cpp')
EXT_SOURCE +=  $(shell find $(PHYSX_SOURCE) -type f -path '*/fastxml/*' -name '*.cpp')

EXT_SOURCE :=  $(shell find $(PHYSX_SOURCE) -type f -path '*/physxextensions/*' -name '*.cpp')
EXT_SOURCE +=  $(shell find $(PHYSX_SOURCE) -type f -path '*/fastxml/*' -name '*.cpp')
USED_EXT_SOURCE :=  $(shell find $(PHYSX_SOURCE) -type f -path '*/physxextensions/*' -name 'ExtDefaultErrorCallback.cpp')
EXT_SOURCE := $(filter-out $(USED_EXT_SOURCE), $(EXT_SOURCE))
#SOURCE := $(filter-out $(EXT_SOURCE), $(SOURCE))

CHAR_SOURCE +=  $(shell find $(PHYSX_SOURCE) -type f -path '*/physxcharacterkinematic/*' -name '*.cpp')
SOURCE := $(filter-out $(CHAR_SOURCE), $(SOURCE))

PHYSX_DEFS := -D NO_AUTO_DEFS -D _DEBUG -D __STDC_LIMIT_MACROS
#PHYSX_DEFS := -D _DEBUG

ifeq ($(OS), Windows_NT)
SOURCE := $(filter-out $(UNIX_SOURCE), $(SOURCE))
SOURCE := $(filter-out $(LINUX_SOURCE), $(SOURCE))
SHARED_FLAG := -shared
LIBEXT := .dll
LIBPREF :=
else
SOURCE := $(filter-out $(WINDOWS_SOURCE), $(SOURCE))
SOURCE := $(filter-out $(LINUX_SOURCE), $(SOURCE))
SHARED_FLAG := -shared
LIBEXT := .dylib
LIBPREF := 
endif


OBJECT := $(patsubst $(PHYSX_SOURCE)/%.cpp, $(PHYSX_BUILD)/%.o, $(SOURCE))

default: buildLibrary

buildLibrary: $(OBJECT)
	$(CXX) $(PHYSX_DEFS) -ggdb -g3 -c -o $(NAME).o $(NAME).cpp $(IMPORT_FLAGS)
	$(CXX) $(SHARED_FLAG) -ggdb -g3 -o $(LIBPREF)$(NAME)$(LIBEXT) $(NAME).o $(OBJECT)
	rm $(NAME).o

clean:
	rm $(OBJECT)

rmExtensions:
	rm $(EXT_SOURCE)

rmHeaderMeta:
	rm $(META_HEADERS)

$(PHYSX_BUILD)/%.o: $(PHYSX_SOURCE)/%.cpp
	@mkdir -p $(@D)
	$(CXX) -c $(PHYSX_DEFS) -ggdb -g3 -o $@ $^ $(IMPORT_FLAGS)
