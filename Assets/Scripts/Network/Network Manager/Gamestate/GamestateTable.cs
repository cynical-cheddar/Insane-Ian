using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

namespace Gamestate {
    [Serializable]
    public class GamestateTable<T> : IGamestateCommitHandler where T : GamestateEntry
    {
        public int actorNumber {
            get { return gamestateTracker.actorNumber; }
        }

        public GamestateTracker.Table tableType {
            get { return _tableType; }
        }

        public int count {
            get {
                int count;
                lock (entries) {
                    count = entries.Count;
                }
                return count;
            }
        }

        private GamestateTracker.Table _tableType;

        //public delegate void TableCallback(GamestateTable<T> table);
        public delegate bool TableSearcher(T entry);
        [SerializeField] private List<T> entries;
        //private List<TableCallback> tableCallbacks;
        //private List<List<EntryCallback>> entryCallbacks;
        private IGamestateCommitHandler gamestateTracker;

        internal GamestateTable(IGamestateCommitHandler gamestateTracker, GamestateTracker.Table tableType) {
            _tableType = tableType;
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

        public T Create(bool nonZero, bool decrement) {
            short id = 0;

            if (nonZero) {
                if (decrement) id--;
                else id++;
            }

            T created = null;

            lock (entries) {
                while (Contains(id)) {
                    if (decrement) id--;
                    else id++;
                }

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
                        foundEntry = entry;
                        break;
                    }
                }
            }
            foundEntry.Lock();

            return foundEntry;
        }
        
        public T Read(short id) {
            T foundEntry = null;
            
                foreach (T entry in entries) {
                    if (entry.id == id) {
                        foundEntry = entry;
                        break;
                    }
                }

                return foundEntry;
        }
        public T ReadAtIndex(short index) {
            T foundEntry = null;
            
            if (index < entries.Count) {
               foundEntry = entries[index];
            }

            return foundEntry;
        }

        public T GetAtIndex(int index) {
            T foundEntry = null;

            lock (entries) {
                if (index < entries.Count) {
                    foundEntry = entries[index];
                }
            }
            foundEntry.Lock();

            return foundEntry;
        }

        public T Find(TableSearcher searcher) {
            T foundEntry = null;

            lock (entries) {
                foreach (T entry in entries) {
                    if (searcher(entry)) {
                        foundEntry = entry;
                        break;
                    }
                }
            }
            foundEntry.Lock();

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
                if (packet.packetType == GamestatePacket.PacketType.Delete) {
                    for (int i = 0; i < entries.Count; i++) {
                        if (entries[i].id == packet.id) {
                            entries.RemoveAt(i);
                            break;
                        }
                    }
                }
                else {
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
            }
            
            if (foundEntry != null) foundEntry.Apply(packet);
            if (created) foundEntry.Release();
        }

        internal bool AttemptApply(GamestatePacket packet) {
            T foundEntry = null;
            bool created = false;
            bool succeeded = false;

            lock (entries) {
                if (packet.packetType == GamestatePacket.PacketType.Delete) {
                    for (int i = 0; i < entries.Count; i++) {
                        if (entries[i].id == packet.id) {
                            entries.RemoveAt(i);
                            succeeded = true;
                            break;
                        }
                    }
                }
                else {
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
            }

            if (foundEntry != null) {
                foundEntry.Apply(packet);
                succeeded = true;
            }
            if (created) foundEntry.Release();
            return succeeded;
        }

        void IGamestateCommitHandler.CommitPacket(GamestatePacket packet) {
            gamestateTracker.CommitPacket(packet);
        }
    }
}
