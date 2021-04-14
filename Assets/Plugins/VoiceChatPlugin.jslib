var ExampleLibraryPlugin = {
    
    initializeA: function(id) {
        initialize(Pointer_stringify(id));
    },

    callA: function(recvID) {
        call(Pointer_stringify(recvID));
    }

};
mergeInto(LibraryManager.library, ExampleLibraryPlugin);