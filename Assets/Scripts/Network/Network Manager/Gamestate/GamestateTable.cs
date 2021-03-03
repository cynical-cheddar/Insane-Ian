using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

namespace Gamestate {
    public class GamestateTable<T> : IGamestateCommitHandler where T : GamestateEntry
    {
        public int actorNumber {
            get { return gamestateTracker.actorNumber; }
        }

        //public delegate void TableCallback(GamestateTable<T> table);
        //public delegate void EntryCallback(T entry);
        private List<T> entries;
        //private List<TableCallback> tableCallbacks;
        //private List<List<EntryCallback>> entryCallbacks;
        private IGamestateCommitHandler gamestateTracker;

        internal GamestateTable(IGamestateCommitHandler gamestateTracker) {
            this.gamestateTracker = gamestateTracker;
            entries = new List<T>();
        }

        public T Create(short id) {
            T created = null;

            lock (entries) {
                if (Contains(id)) throw new Exception("An entry with ID " + id + " already exists.");

                object[] args = new object[] {id, this}; 

                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                created = (T)Activator.CreateInstance(typeof(T), flags, null, args, null);

                created.Lock();
                entries.Add(created);
            }

            return created;
        }

        public T Get(short id) {
            T foundEntry = null;

            lock (entries) {
                foreach (T entry in entries) {
                    if (entry.id == id) {
                        entry.Lock();
                        foundEntry = entry;
                        break;
                    }
                }
            }

            return foundEntry;
        }

        public bool Contains(short id) {
            bool found = false;

            lock (entries) {
                foreach (T entry in entries) {
                    if (entry.id == id) {
                        found = true;
                        break;
                    }
                }
            }

            return found;
        }

        internal void Apply(GamestatePacket packet) {
            T foundEntry = null;
            bool created = false;

            lock (entries) {
                foreach (T entry in entries) {
                    if (entry.id == packet.id) {
                        foundEntry = entry;
                        break;
                    }
                }

                if (foundEntry == null && packet.packetType == GamestatePacket.PacketType.Create) {
                    foundEntry = Create(packet.id);
                    created = true;
                }
            }

            if (foundEntry != null) foundEntry.Apply(packet);
            if (created) foundEntry.Release();
        }

        internal bool AttemptApply(GamestatePacket packet) {
            T foundEntry = null;
            bool created = false;
            bool succeeded = false;

            lock (entries) {
                foreach (T entry in entries) {
                    if (entry.id == packet.id) {
                        foundEntry = entry;
                        break;
                    }
                }

                if (foundEntry == null && packet.packetType == GamestatePacket.PacketType.Create) {
                    foundEntry = Create(packet.id);
                    created = true;
                }
            }

            if (foundEntry != null) succeeded = foundEntry.AttemptApply(packet);
            if (created) foundEntry.Release();
            return succeeded;
        }

        void IGamestateCommitHandler.CommitPacket(GamestatePacket packet) {
            gamestateTracker.CommitPacket(packet);
        }
    }
}
