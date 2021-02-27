using System.Collections.Generic;

namespace Gamestate {
    public class GamestateTable<T> where T : GamestateEntry {
        public delegate void TableCallback(GamestateTable<T> table);
        public delegate void EntryCallback(T entry);
        private List<T> entries;
        private List<TableCallback> tableCallbacks;
        private List<List<EntryCallback>> entryCallbacks;

        public void UpdateEntry(T newEntry) {
            for (int i = 0; i < entries.Count; i++) {
                if (entries[i].shortValues[0] == newEntry.shortValues[0]) {
                    for (int j = 1; j < newEntry.shortValues.Count; j++) {
                        entries[i].shortValues[j] = newEntry.shortValues[j];
                    }

                    foreach (EntryCallback callback in entryCallbacks[i]) {
                        callback(entries[i]);
                    }

                    break;
                }
            }

            foreach (TableCallback callback in tableCallbacks) {
                callback(this);
            }
        }
    }
}