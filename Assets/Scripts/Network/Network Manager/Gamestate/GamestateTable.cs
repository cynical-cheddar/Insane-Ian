using System.Collections.Generic;
using System;

namespace Gamestate {
    public class GamestateTable<T> where T : GamestateEntry {
        public delegate void TableCallback(GamestateTable<T> table);
        public delegate void EntryCallback(T entry);
        private List<T> entries;
        private List<TableCallback> tableCallbacks;
        private List<List<EntryCallback>> entryCallbacks;

        public T Create(short id) {
            if (Read(id) != null) throw new Exception("An entry with ID " + id + " already exists.");

            object[] args = new object[] {id}; 
            T created = (T)Activator.CreateInstance(typeof(T), args);

            entries.Add(created);

            return created;
        }

        public T Read(short id) {
            foreach (T entry in entries) {
                if (entry.shortValues[0] == id) return entry;
            }

            return null;
        }
    }
}