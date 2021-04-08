var ExampleLibraryPlugin = {
    $VCSharedData: {
        lastPeerId: null,
        peer: null, // own peer object
        conns: []
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
                console.log('Received null id from peer open');
                VCSharedData.peer.id = VCSharedData.lastPeerId;
            } else {
                VCSharedData.lastPeerId = VCSharedData.peer.id;
            }
            console.log('ID: ' + VCSharedData.peer.id);
        });

        VCSharedData.peer.on('connection', function(conn) {
            // Connect to the other peer
            VCSharedData.conns.push(conn);
            console.log("Connected to: " + conn.peer)
        });

        VCSharedData.peer.on('disconnected', function() {
            console.log('Connection lost. Please reconnect');

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
            call.answer(navigator.mediaDevices.getUserMedia({audio: true, video: false}))

            call.on('stream', function(stream) {
                // `stream` is the MediaStream of the remote peer.
                // Here you'd add it to an HTML video/canvas element.
            });
        });
    },

    join: function(recvID) {
        // Create connection to destination peer specified in the input field
        var conn = VCSharedData.peer.connect(Pointer_stringify(recvID), {
            reliable: true
        });
        VCSharedData.conns.push(conn);

        conn.on('open', function() {
            console.log("Connected to: " + conn.peer);  
        });
    },

    call: function(recvID) {
        var call = VCSharedData.peer.call(recvID, navigator.mediaDevices.getUserMedia({audio: true, video: false}));

        call.on('stream', function(stream) {
            // `stream` is the MediaStream of the remote peer.
            // Here you'd add it to an HTML video/canvas element.
        });
    },

    sendData: function(recvID, data) {
        for (i = 0; i < VCSharedData.conns.length; i++) {
            if (VCSharedData.conns[i].peer == recvID) {
                if (VCSharedData.conns[i] && VCSharedData.conns[i].open) {
                    VCSharedData.conns[i].send(Pointer_stringify(data));
                    console.log("Sent: " + Pointer_stringify(data));
                } else {
                    console.log('Connection is closed');
                }
            }
        }
        
    }
};

autoAddDeps(ExampleLibraryPlugin, '$VCSharedData');
mergeInto(LibraryManager.library, ExampleLibraryPlugin);