var ExampleLibraryPlugin = {
    $SharedData: {
        lastPeerId: null,
        peer: null, // own peer object
        conn: null
    },

    /**
     * Create the Peer object for our end of the connection.
     *
     * Sets up callbacks that handle any events related to our
     * peer object.
     */
    initialize: function() {
        // Create own peer object with connection to shared PeerJS server
        SharedData.peer = new Peer('unitysender', {
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

        SharedData.peer.on('open', function(id) {
            // Workaround for peer.reconnect deleting previous id
            if (SharedData.peer.id === null) {
                console.log('Received null id from peer open');
                SharedData.peer.id = SharedData.lastPeerId;
            } else {
                SharedData.lastPeerId = SharedData.peer.id;
            }

            console.log('ID: ' + SharedData.peer.id);
        });
        SharedData.peer.on('connection', function(c) {
            // Disallow incoming connections
            c.on('open', function() {
                c.send("Sender does not accept incoming connections");
                setTimeout(function() {
                    c.close();
                }, 500);
            });
        });
        SharedData.peer.on('disconnected', function() {
            console.log('Connection lost. Please reconnect');

            // Workaround for peer.reconnect deleting previous id
            SharedData.peer.id = SharedData.lastPeerId;
            SharedData.peer._lastServerId = SharedData.lastPeerId;
            SharedData.peer.reconnect();
        });
        SharedData.peer.on('close', function() {
            SharedData.conn = null;
            console.log('Connection destroyed');
        });
        SharedData.peer.on('error', function(err) {
            console.log(err);
            alert('' + err);
        });
    },

    /**
     * Create the connection between the two Peers.
     *
     * Sets up callbacks that handle any events related to the
     * connection and data received on it.
     */
    join: function(recvID) {
        // Close old connection
        if (SharedData.conn) {
            SharedData.conn.close();
        }

        // Create connection to destination peer specified in the input field
        SharedData.conn = SharedData.peer.connect(Pointer_stringify(recvID), {
            reliable: true
        });

        console.log("Connection attempt");

        SharedData.conn.on('open', function() {
            console.log("Connected to: " + SharedData.conn.peer);

            // Check URL params for comamnds that should be sent immediately
            var name = "command";
            name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
            var regexS = "[\\?&]" + name + "=([^&#]*)";
            var regex = new RegExp(regexS);
            var results = regex.exec(window.location.href);
            if (results != null)
            SharedData.conn.send(results[1]);
                
        });

        // Handle incoming data (messages only since this is the signal sender)
        SharedData.conn.on('data', function (data) {
            window.unityInstance.SendMessage('VoiceChatGameObject', 'RecieveMessage', data);
            console.log("This is where we would send a message to Unity, if that worked like the docs said it would...");
        });

        console.log("Join finished");
    },

    /**
     * Send a signal via the peer connection and add it to the log.
     * This will only occur if the connection is still alive.
     */
    signal: function(sigName) {
        if (SharedData.conn && SharedData.conn.open) {
            SharedData.conn.send(Pointer_stringify(sigName));
            console.log(Pointer_stringify(sigName) + " signal sent");
        } else {
            console.log('Connection is closed');
        }
    },

    /**
     * Send a message via the peer connection and add it to the log.
     * This will only occur if the connection is still alive.
     */
    sendMessage: function(msg) {
        if (SharedData.conn && SharedData.conn.open) {
            SharedData.conn.send(Pointer_stringify(msg));
            console.log("Sent: " + Pointer_stringify(msg));
        } else {
            console.log('Connection is closed');
        }
    }
};

autoAddDeps(ExampleLibraryPlugin, '$SharedData');
mergeInto(LibraryManager.library, ExampleLibraryPlugin);