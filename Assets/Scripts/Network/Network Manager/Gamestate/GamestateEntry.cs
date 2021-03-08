using System.Collections.Generic;
using System.Threading;
using System;
using System.Timers;
using Timer = System.Timers.Timer;
using UnityEngine;

namespace Gamestate {
    public abstract class GamestateEntry {
        internal delegate void EntryListener(GamestateEntry entry);
        internal delegate void EntryErrorCallback(GamestateEntry entry, bool succeeded);

        private uint _revisionNumber;
        private short _id;
        private string _name;
        [SerializeField]
        protected List<bool> boolValues;

        protected List<short> shortValues;

        private Mutex mutex;
        private IGamestateCommitHandler commitHandler;
        private GamestatePacket packet = null;
        [SerializeField]
        private bool[] setBools;
        private bool firstCommit;

        private List<EntryListener> listeners;
        private List<EntryErrorCallback> errorCallbacks;

        public uint revisionNumber {
            get { return _revisionNumber; }
        }
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
            firstCommit = true;

            listeners = new List<EntryListener>();
            errorCallbacks = new List<EntryErrorCallback>();
        }

        internal GamestateEntry(short id, IGamestateCommitHandler commitHandler) : this(commitHandler) {
            _id = id;
        }

        internal void Lock() {
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

            packet.hasShortValues[field] = true;
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

        private void PreparePacketForCommit() {
            if (packet == null) GetPacket();

            packet.id = id;

            if (firstCommit) packet.packetType = GamestatePacket.PacketType.Create;
            else             packet.packetType = GamestatePacket.PacketType.Update;

            packet.revisionNumber = revisionNumber + 1;
            packet.revisionActor  = (byte)commitHandler.actorNumber;

            for (int i = 0; i < GamestatePacket.maxBools; i++) {
                if (!setBools[i]) {
                    if (i < boolValues.Count) packet.boolValues[i] = boolValues[i];
                    else                      packet.boolValues[i] = false;
                }
            }
        }

        public void Commit() {
            PreparePacketForCommit();

            if (errorCallbacks.Count == 0) {
                errorCallbacks.Add(null);
                commitHandler.CommitPacket(packet);
            }

            GamestatePacketManager.ReleasePacket(packet);
            packet = null;

            Release();
        }

        internal void Commit(EntryErrorCallback callback) {
            PreparePacketForCommit();

            errorCallbacks.Add(callback);
            if (errorCallbacks.Count == 1) {
                commitHandler.CommitPacket(packet);
            }

            GamestatePacketManager.ReleasePacket(packet);
            packet = null;

            Release();
        }

        private void PreparePacketForIncrement() {
            if (packet == null) GetPacket();

            packet.id = id;
            packet.revisionActor = (byte)commitHandler.actorNumber;
            packet.packetType = GamestatePacket.PacketType.Increment;

            int j = 0;
            for (int i = 0; i < shortValues.Count; i++) {
                if (packet.hasShortValues[i]) {
                    packet.shortValues[j] -= shortValues[i];
                    j++;
                }
            }

            for (int i = 0; i < GamestatePacket.maxBools; i++) {
                if (!setBools[i]) {
                    if (i < boolValues.Count) packet.boolValues[i] = boolValues[i];
                    else                      packet.boolValues[i] = false;
                }
            }
        }

        public void Increment() {
            PreparePacketForIncrement();

            if (errorCallbacks.Count == 0) {
                errorCallbacks.Add(null);
            }
            commitHandler.CommitPacket(packet);

            GamestatePacketManager.ReleasePacket(packet);
            packet = null;

            Release();
        }

        internal void Increment(EntryErrorCallback callback) {
            PreparePacketForIncrement();

            errorCallbacks.Add(callback);
            commitHandler.CommitPacket(packet);

            GamestatePacketManager.ReleasePacket(packet);
            packet = null;

            Release();
        }

        public void Delete() {
            if (packet == null) GetPacket();

            packet.id = id;
            packet.packetType = GamestatePacket.PacketType.Delete;

            commitHandler.CommitPacket(packet);

            GamestatePacketManager.ReleasePacket(packet);
            packet = null;

            Release();
        }

        internal bool AttemptApply(GamestatePacket packet) {
            bool succeeded = false;

            Lock();

            if (_revisionNumber + 1 == packet.revisionNumber || packet.packetType == GamestatePacket.PacketType.Increment) {
                succeeded = true;
                Apply(packet);
            }

            Release();

            return succeeded;
        }

        internal void Apply(GamestatePacket packet) {
            Lock();

            if (packet.packetType == GamestatePacket.PacketType.Increment) {
                _revisionNumber++;

                int j = 0;
                for (int i = 0; i < shortValues.Count; i++) {
                    if (packet.hasShortValues[i]) {
                        shortValues[i] += packet.shortValues[j];
                        j++;
                    }
                }
            }
            else {
                _revisionNumber = packet.revisionNumber;
                if (packet.hasName) _name = packet.name;

                int j = 0;
                for (int i = 0; i < shortValues.Count; i++) {
                    if (packet.hasShortValues[i]) {
                        shortValues[i] = packet.shortValues[j];
                        j++;
                    }
                }
            }

            for (int i = 0; i < boolValues.Count; i++) {
                boolValues[i] = packet.boolValues[i];
            }

            if (errorCallbacks.Count > 0) {
                if (errorCallbacks[0] != null) {
                    Lock();
                    errorCallbacks[0](this, packet.revisionActor == commitHandler.actorNumber);
                }

                for (int i = 1; i < errorCallbacks.Count; i++) {
                    Lock();
                    errorCallbacks[i](this, false);
                }

                errorCallbacks.Clear();
            }

            for (int i = 0; i < listeners.Count; i++) {
                if (listeners[i] != null) {
                    Lock();
                    listeners[i](this);
                }
            }

            Release();
        }

        internal int AddEntryListener(EntryListener listener) {
            for (int i = 0; i < listeners.Count; i++) {
                if (listeners[i] == null) {
                    listeners[i] = listener;
                    return i;
                }
            }

            listeners.Add(listener);
            return listeners.Count - 1;
        }

        public void RemoveListener(int listener) {
            listeners[listener] = null;
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

        public delegate void GlobalsListener(GlobalsEntry entry);
        public delegate void GlobalsErrorCallback(GlobalsEntry entry, bool succeeded);

        public int AddListener(GlobalsListener listener) {
            return AddEntryListener((GamestateEntry entry) => {
                listener(entry as GlobalsEntry);
            });
        }

        public void Commit(GlobalsErrorCallback callback) {
            Commit((GamestateEntry entry, bool succeeded) => {
                callback(entry as GlobalsEntry, succeeded);
            });
        }

        public void Increment(GlobalsErrorCallback callback) {
            Increment((GamestateEntry entry, bool succeeded) => {
                callback(entry as GlobalsEntry, succeeded);
            });
        }

        internal GlobalsEntry(IGamestateCommitHandler commitHandler) : base(commitHandler) {
            for (int i = 0; i < Enum.GetValues(typeof(ShortFields)).Length; i++) shortValues.Add(0);
        }
    }
    [Serializable]
    public class PlayerEntry : GamestateEntry {
        //  Max 16 short fields
 
        public enum ShortFields { ActorNumber, Role, Character, TeamId }

        public enum Role { None, Driver, Gunner }

        public short actorNumber {
            get { return shortValues[ (int)ShortFields.ActorNumber ]; }
            set { ChangeShortValue( (int)ShortFields.ActorNumber, value ); }
        }
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

        public delegate void PlayerListener(PlayerEntry entry);
        public delegate void PlayerErrorCallback(PlayerEntry entry, bool succeeded);

        public int AddListener(PlayerListener listener) {
            return AddEntryListener((GamestateEntry entry) => {
                listener(entry as PlayerEntry);
            });
        }

        public void Commit(PlayerErrorCallback callback) {
            Commit((GamestateEntry entry, bool succeeded) => {
                callback(entry as PlayerEntry, succeeded);
            });
        }

        public void Increment(PlayerErrorCallback callback) {
            Increment((GamestateEntry entry, bool succeeded) => {
                callback(entry as PlayerEntry, succeeded);
            });
        }

        internal PlayerEntry(short id, IGamestateCommitHandler commitHandler) : base(id, commitHandler) {
            for (int i = 0; i < Enum.GetValues(typeof(ShortFields)).Length; i++) shortValues.Add(0);
            for (int i = 0; i < Enum.GetValues(typeof(BoolFields)).Length; i++) boolValues.Add(false);
        }
    }
    [Serializable]
    public class TeamEntry : GamestateEntry {
        //  Max 16 short fields
        public enum ShortFields { Kills, Deaths, Assists, Checkpoint, Vehicle }

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
        public short checkpoint {
            get { return shortValues[ (int)ShortFields.Checkpoint ]; }
            set { ChangeShortValue( (int)ShortFields.Checkpoint, value ); }
        }
        public short vehicle {
            get { return shortValues[ (int)ShortFields.Vehicle ]; }
            set { ChangeShortValue( (int)ShortFields.Vehicle, value ); }
        }

        //  Max 15 bool fields
        public enum BoolFields { IsDead }

        public bool isDead {
            get { return boolValues[ (int)BoolFields.IsDead ]; }
            set { ChangeBoolValue( (int)BoolFields.IsDead, value ); }
        }

        public delegate void TeamListener(TeamEntry entry);
        public delegate void TeamErrorCallback(TeamEntry entry, bool succeeded);

        public int AddListener(TeamListener listener) {
            return AddEntryListener((GamestateEntry entry) => {
                listener(entry as TeamEntry);
            });
        }

        public void Commit(TeamErrorCallback callback) {
            Commit((GamestateEntry entry, bool succeeded) => {
                callback(entry as TeamEntry, succeeded);
            });
        }

        public void Increment(TeamErrorCallback callback) {
            Increment((GamestateEntry entry, bool succeeded) => {
                callback(entry as TeamEntry, succeeded);
            });
        }

        internal TeamEntry(short id, IGamestateCommitHandler commitHandler) : base(id, commitHandler) {
            for (int i = 0; i < Enum.GetValues(typeof(ShortFields)).Length; i++) shortValues.Add(0);
            for (int i = 0; i < Enum.GetValues(typeof(BoolFields)).Length; i++) boolValues.Add(false);
        }
    }
}