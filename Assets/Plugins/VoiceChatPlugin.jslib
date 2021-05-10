var ExampleLibraryPlugin = {
    
    initializeA: function(id) {
        initialize(Pointer_stringify(id));
    },

    callA: function(recvID) {
        call(Pointer_stringify(recvID));
    },

    mute: function(teammateID) {
        muteAllButOne(Pointer_stringify(teammateID));
    },

    unmute: function() {
        unmuteEveryone();
    },

    muteAll: function() {
        muteEveryone();
    }

};
mergeInto(LibraryManager.library, ExampleLibraryPlugin);