using System.Collections.Generic;

namespace Gamestate {
    public class GamestateEntry {
        public List<short> shortValues;
    }

    public class PlayerEntry : GamestateEntry {
        public enum ShortEntries { PlayerId, Role, Character, TeamId }

        public short playerId {
            get { return shortValues[ (int)ShortEntries.PlayerId ]; }
        }
        public short role {
            get { return shortValues[ (int)ShortEntries.Role ]; }
        }
        public short character {
            get { return shortValues[ (int)ShortEntries.Character ]; }
        }
        public short teamId {
            get { return shortValues[ (int)ShortEntries.TeamId ]; }
        }
    }
}