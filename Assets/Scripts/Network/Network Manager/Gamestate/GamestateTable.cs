using System.Collections.Generic;

namespace Gamestate {
    public class GamestateTable<T> where T : GamestateEntry {
        public delegate void TableCallback(GamestateTable<T> table);
        public delegate void EntryCallback(T entry);
        public List<T> entries;
        private List<TableCallback> tableCallbacks;
        private List<List<EntryCallback>> entryCallbacks;
    }
}