using System.Collections.Generic;

namespace Gamestate {
    public abstract class GamestateEntry {
        public string name;
        public short boolValues;
        public List<short> shortValues;

        public GamestateEntry() {}
        public GamestateEntry(short id) {
            shortValues = new List<short>();
            shortValues.Add(id);
        }
    }

    public class GlobalsEntry : GamestateEntry {
        public enum ShortFields { TimeLimit }

        public short playerId {
            get { return shortValues[ (int)ShortFields.TimeLimit ]; }
        }

        public GlobalsEntry() : base() {}
    }

    public class PlayerEntry : GamestateEntry {
        public enum ShortFields { PlayerId, ActorNumber, Role, Character, TeamId }

        public short playerId {
            get { return shortValues[ (int)ShortFields.PlayerId ]; }
        }
        public short role {
            get { return shortValues[ (int)ShortFields.Role ]; }
        }
        public short character {
            get { return shortValues[ (int)ShortFields.Character ]; }
        }
        public short teamId {
            get { return shortValues[ (int)ShortFields.TeamId ]; }
        }

        public enum BoolFields { IsBot, Ready }

        public bool isBot {
            get { return ((boolValues >> (int)BoolFields.IsBot) & 1) == 1; }
        }
        public bool ready {
            get { return ((boolValues >> (int)BoolFields.Ready) & 1) == 1; }
        }

        public PlayerEntry(short id) : base(id) {}
    }

    public class TeamEntry : GamestateEntry {
        public enum ShortFields { TeamId, Kills, Deaths, Assists, Checkpoint }

        public short teamId {
            get { return shortValues[ (int)ShortFields.TeamId ]; }
        }
        public short kills {
            get { return shortValues[ (int)ShortFields.Kills ]; }
        }
        public short deaths {
            get { return shortValues[ (int)ShortFields.Deaths ]; }
        }
        public short assists {
            get { return shortValues[ (int)ShortFields.Assists ]; }
        }
        public short Checkpoint {
            get { return shortValues[ (int)ShortFields.Checkpoint ]; }
        }

        public enum BoolFields { IsDead }

        public bool isDead {
            get { return ((boolValues >> (int)BoolFields.IsDead) & 1) == 1; }
        }

        public TeamEntry(short id) : base(id) {}
    }
}