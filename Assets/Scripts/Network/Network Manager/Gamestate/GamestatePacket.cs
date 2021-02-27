using System.Collections.Generic;

namespace Gamestate {
    public struct GamestatePacket {
        public short shortPresenceFlags;
        public List<short> prevShortValues;
        public List<short> newShortValues;

        public short boolPresenceFlags;
        public short prevBoolValueFlags;
        public short newBoolValueFlags;
    }
}