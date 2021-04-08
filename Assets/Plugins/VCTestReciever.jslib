var ExampleLibraryPlugin = {
    $SharedDataVCTestReciever: {
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
    initializeVCTestReciever: function() {
        // Create own peer object with connection to shared PeerJS server
        SharedDataVCTestReciever.peer = new Peer('unityreciever', {
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

        SharedDataVCTestReciever.peer.on('open', function(id) {
            // Workaround for peer.reconnect deleting previous id
            if (SharedDataVCTestReciever.peer.id === null) {
                console.log('Received null id from peer open');
                SharedDataVCTestReciever.peer.id = SharedDataVCTestReciever.lastPeerId;
            } else {
                SharedDataVCTestReciever.lastPeerId = SharedDataVCTestReciever.peer.id;
            }

            console.log('ID: ' + SharedDataVCTestReciever.peer.id);
        });
        SharedDataVCTestReciever.peer.on('connection', function (c) {
            // Allow only a single connection
            if (SharedDataVCTestReciever.conn && SharedDataVCTestReciever.conn.open) {
                c.on('open', function() {
                    c.send("Already connected to another client");
                    setTimeout(function() { c.close(); }, 500);
                });
                return;
            }

            SharedDataVCTestReciever.conn = c;
            console.log("Connected to: " + SharedDataVCTestReciever.conn.peer);
            
            /**
            * Triggered once a connection has been achieved.
            * Defines callbacks to handle incoming data and connection events.
            */
            SharedDataVCTestReciever.conn.on('data', function (data) {
                console.log("Data recieved");
                switch (data) {
                    case 'Go':
                        window.unityInstance.SendMessage('VoiceChatTestRecieverGO', 'RecieveSignal', 1);
                        break;
                    case 'Fade':
                        window.unityInstance.SendMessage('VoiceChatTestRecieverGO', 'RecieveSignal', 2);
                        break;
                    case 'Off':
                        window.unityInstance.SendMessage('VoiceChatTestRecieverGO', 'RecieveSignal', 3);
                        break;
                    case 'Reset':
                        window.unityInstance.SendMessage('VoiceChatTestRecieverGO', 'RecieveSignal', 0);
                        break;
                    default:
                        window.unityInstance.SendMessage('VoiceChatTestRecieverGO', 'RecieveMessage', data);
                        break;
                };
            });
            SharedDataVCTestReciever.conn.on('close', function () {
                SharedDataVCTestReciever.conn = null;
                console.log("Connection reset, awaiting connection...");
            });

        });
        SharedDataVCTestReciever.peer.on('disconnected', function() {
            console.log('Connection lost. Please reconnect');

            // Workaround for peer.reconnect deleting previous id
            SharedDataVCTestReciever.peer.id = SharedDataVCTestReciever.lastPeerId;
            SharedDataVCTestReciever.peer._lastServerId = SharedDataVCTestReciever.lastPeerId;
            SharedDataVCTestReciever.peer.reconnect();
        });
        SharedDataVCTestReciever.peer.on('close', function() {
            SharedDataVCTestReciever.conn = null;
            console.log('Connection destroyed');
        });
        SharedDataVCTestReciever.peer.on('error', function(err) {
            console.log(err);
            alert('' + err);
        });
    },

    /**
     * Send a message via the peer connection and add it to the log.
     * This will only occur if the connection is still alive.
     */
    sendMessageVCTestReciever: function(msg) {
        if (SharedDataVCTestReciever.conn && SharedDataVCTestReciever.conn.open) {
            SharedDataVCTestReciever.conn.send(Pointer_stringify(msg));
            console.log("Sent: " + Pointer_stringify(msg));
        } else {
            console.log('Connection is closed');
        }
    }
};

autoAddDeps(ExampleLibraryPlugin, '$SharedDataVCTestReciever');
mergeInto(LibraryManager.library, ExampleLibraryPlugin);