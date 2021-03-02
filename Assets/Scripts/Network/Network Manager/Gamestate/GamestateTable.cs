using System.Collections.Generic;
using System;
using System.Reflection;

namespace Gamestate {
    public class GamestateTable<T> : IGamestateCommitHandler where T : GamestateEntry
    {
        public static GamestateTable<T> CreateTestTable(GamestateCommitTestHelper testHelper) {
            return new GamestateTable<T>(testHelper);
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
            if (Get(id) != null) throw new Exception("An entry with ID " + id + " already exists.");

            object[] args = new object[] {id, this}; 

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            T created = (T)Activator.CreateInstance(typeof(T), flags, null, args, null);

            created.Lock();

            return created;
        }

        public T Get(short id) {
            foreach (T entry in entries) {
                if (entry.id == id) {
                    entry.Lock();
                    return entry;
                }
            }

            return null;
        }

        void IGamestateCommitHandler.CommitPacket(GamestatePacket packet) {
            
        }
    }
}