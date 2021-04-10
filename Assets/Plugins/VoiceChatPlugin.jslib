var ExampleLibraryPlugin = {
    $VCSharedData: {
        lastPeerId: null,
        peer: null // own peer object
    },

    initialize: function(id) {
        // Create own peer object with connection to shared PeerJS server
        VCSharedData.peer = new Peer(Pointer_stringify(id), {
            host: 'insane-ian-308116.ew.r.appspot.com',
            secure: true,
            port: 443,
            key: 'peerjs',
            debug: 3,
            config: {'iceServers': [
                {url: 'stun:34.105.198.244', username: 'ubuntu', credential: 'admin'},
                {url: 'turn:34.105.198.244', username: 'ubuntu', credential: 'admin'}
            ]}
        });

        VCSharedData.peer.on('open', function(id) {
            // Workaround for peer.reconnect deleting previous id
            if (VCSharedData.peer.id === null) {
                VCSharedData.peer.id = VCSharedData.lastPeerId;
            } else {
                VCSharedData.lastPeerId = VCSharedData.peer.id;
            }
        });

        VCSharedData.peer.on('disconnected', function() {
            // Workaround for peer.reconnect deleting previous id
            VCSharedData.peer.id = VCSharedData.lastPeerId;
            VCSharedData.peer._lastServerId = VCSharedData.lastPeerId;
            VCSharedData.peer.reconnect();
        });

        VCSharedData.peer.on('error', function(err) {
            console.log(err);
            alert('' + err);
        });

        // Handle incoming call
        VCSharedData.peer.on('call', function(call) {
            navigator.mediaDevices.getUserMedia({audio: true, video: false})
                .then(function(stream) {
                    call.answer(stream);
                })
                .catch(function(err) {
                    console.error('Failed to get local stream', err);
                });
                call.on('stream', function(remoteStream) {
                    var audio = document.querySelector('audio');
                    audio.srcObject = remoteStream;
                });
        });
    },

    call: function(recvID) {
        var callerID = Pointer_stringify(recvID);
        navigator.mediaDevices.getUserMedia({audio: true, video: false})
            .then(function(stream) {
                const call = VCSharedData.peer.call(callerID, stream);
                call.on('stream', function(remoteStream) {
                    var audio = document.querySelector('audio');
                    audio.srcObject = remoteStream;
                });
            })
            .catch(function(err) {
                console.error('Failed to get local stream', err);
            });
    }
};

autoAddDeps(ExampleLibraryPlugin, '$VCSharedData');
mergeInto(LibraryManager.library, ExampleLibraryPlugin);