using System.Collections.Generic;
using System.Threading;

namespace Gamestate {
    public abstract class GamestateEntry {
        private string _name;
        private short _id;
        protected List<bool> boolValues;
        protected List<short> shortValues;

        private Mutex mutex;
        private IGamestateCommitHandler commitHandler;
        private GamestatePacket packet = null;
        private bool[] setBools;

        public short id {
            get { return _id; }
        }
        public string name {
            get { return _name; }
            set { ChangeName(value); }
        }

        internal GamestateEntry(IGamestateCommitHandler commitHandler) {
            shortValues = new List<short>();
            boolValues = new List<bool>();

            mutex = new Mutex();
            this.commitHandler = commitHandler;
            setBools = new bool[GamestatePacket.maxBools];
        }
        internal GamestateEntry(short id, IGamestateCommitHandler commitHandler) : this(commitHandler) {
            _id = id;
        }

        public void Lock() {
            mutex.WaitOne();
        }

        public void Release() {
            mutex.ReleaseMutex();
        }

        protected void ChangeShortValue(int field, short value) {
            if (packet == null) GetPacket();

            int index = 0;
            for (int i = 0; i < field; i++) {
                if (index == packet.shortValues.Count) break;
                if (packet.hasShortValues[i]) index++;
            }

            if (packet.hasShortValues[field]) packet.shortValues[index] = value;
            else packet.shortValues.Insert(index, value);
        }

        protected void ChangeBoolValue(int field, bool value) {
            if (packet == null) GetPacket();

            packet.boolValues[field] = value;
            setBools[field] = true;
        }

        protected void ChangeName(string value) {
            if (packet == null) GetPacket();

            packet.hasName = true;
            packet.name = value;
        }

        public void Commit() {
            commitHandler.CommitPacket(packet);

            GamestatePacketManager.ReleasePacket(packet);
            packet = null;
        }

        private void GetPacket() {
            packet = GamestatePacketManager.GetPacket();

            for (int i = 0; i < GamestatePacket.maxBools; i++) {
                setBools[i] = false;
            }
        }
    }

    public class GlobalsEntry : GamestateEntry {
        //  Max 16 short fields
        public enum ShortFields { TimeLimit }

        public short timeLimit {
            get { return shortValues[ (int)ShortFields.TimeLimit ]; }
            set { ChangeShortValue( (int)ShortFields.TimeLimit, value ); }
        }

        internal GlobalsEntry(IGamestateCommitHandler commitHandler) : base(commitHandler) {}
    }

    public class PlayerEntry : GamestateEntry {
        //  Max 16 short fields
        public enum ShortFields { ActorNumber, Role, Character, TeamId }

        public short role {
            get { return shortValues[ (int)ShortFields.Role ]; }
            set { ChangeShortValue( (int)ShortFields.Role, value ); }
        }
        public short character {
            get { return shortValues[ (int)ShortFields.Character ]; }
            set { ChangeShortValue( (int)ShortFields.Character, value ); }
        }
        public short teamId {
            get { return shortValues[ (int)ShortFields.TeamId ]; }
            set { ChangeShortValue( (int)ShortFields.TeamId, value ); }
        }

        //  Max 15 bool fields
        public enum BoolFields { IsBot, Ready }

        public bool isBot {
            get { return boolValues[ (int)BoolFields.IsBot ]; }
            set { ChangeBoolValue( (int)BoolFields.IsBot, value ); }
        }
        public bool ready {
            get { return boolValues[ (int)BoolFields.Ready ]; }
            set { ChangeBoolValue( (int)BoolFields.Ready, value ); }
        }

        internal PlayerEntry(short id, IGamestateCommitHandler commitHandler) : base(id, commitHandler) {}
    }

    public class TeamEntry : GamestateEntry {
        //  Max 16 short fields
        public enum ShortFields { Kills, Deaths, Assists, Checkpoint }

        public short kills {
            get { return shortValues[ (int)ShortFields.Kills ]; }
            set { ChangeShortValue( (int)ShortFields.Kills, value ); }
        }
        public short deaths {
            get { return shortValues[ (int)ShortFields.Deaths ]; }
            set { ChangeShortValue( (int)ShortFields.Deaths, value ); }
        }
        public short assists {
            get { return shortValues[ (int)ShortFields.Assists ]; }
            set { ChangeShortValue( (int)ShortFields.Assists, value ); }
        }
        public short Checkpoint {
            get { return shortValues[ (int)ShortFields.Checkpoint ]; }
            set { ChangeShortValue( (int)ShortFields.Checkpoint, value ); }
        }

        //  Max 15 bool fields
        public enum BoolFields { IsDead }

        public bool isDead {
            get { return boolValues[ (int)BoolFields.IsDead ]; }
            set { ChangeBoolValue( (int)BoolFields.IsDead, value ); }
        }

        internal TeamEntry(short id, IGamestateCommitHandler commitHandler) : base(id, commitHandler) {}
    }
}