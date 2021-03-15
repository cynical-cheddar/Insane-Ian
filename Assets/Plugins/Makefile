NAME := CppTest

SOURCE := $(NAME).cpp 
OBJECT := $(NAME).o

ifeq ($(OS), Windows_NT)
	LIB := $(NAME).dll
else
	LIB := $(NAME).dylib
endif

default: buildLibrary

buildLibrary:
	clang++ -c -o $(OBJECT) $(SOURCE)
	clang++ -shared -v -o $(LIB) $(OBJECT)