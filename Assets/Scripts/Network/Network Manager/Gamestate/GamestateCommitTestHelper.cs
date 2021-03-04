using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamestate {
    public class GamestateCommitTestHelper<T> : IGamestateCommitHandler where T : GamestateEntry
    {
        public List<GamestatePacket> commitedPackets;
        public int actorNumber {
            get { return _actorNumber; }
        }
        private int _actorNumber;

        public GamestateCommitTestHelper(int actorNumber) {
            _actorNumber = actorNumber;
            commitedPackets = new List<GamestatePacket>();
        }

        public GamestateTracker.Table tableType {
            get { return GamestateTracker.Table.Globals; }
        }

        private GamestateTable<T> table = null;
        private T globals = null;

        public GamestateTable<T> TestTable(GamestateTracker.Table tableType) {
            if (table == null && globals == null) {
                table = new GamestateTable<T>(this, tableType);
                return table;
            }
            return null;
        }

        public void Apply(GamestatePacket packet) {
            if (table != null) {
                table.Apply(packet);
            }
            else if (globals != null) {
                globals.Apply(packet);
            }
        }

        public bool AttemptApply(GamestatePacket packet) {
            if (table != null) {
                return table.AttemptApply(packet);
            }
            else if (globals != null) {
                return globals.AttemptApply(packet);
            }
            return false;
        }

        void IGamestateCommitHandler.CommitPacket(GamestatePacket packet) {
            GamestatePacket storedPacket = CopyPacket(packet);

            commitedPackets.Add(storedPacket);
        }

        private GamestatePacket CopyPacket(GamestatePacket packet) {
            GamestatePacket copiedPacket = new GamestatePacket();

            copiedPacket.revisionNumber = packet.revisionNumber;
            copiedPacket.revisionActor = packet.revisionActor;

            copiedPacket.id = packet.id;

            copiedPacket.hasName = packet.hasName;
            copiedPacket.name = packet.name;

            copiedPacket.shortValues = new List<short>(packet.shortValues);
            for (int i = 0; i < GamestatePacket.maxShorts; i++) {
                copiedPacket.hasShortValues[i] = packet.hasShortValues[i];
            }

            for (int i = 0; i < GamestatePacket.maxBools; i++) {
                copiedPacket.boolValues[i] = packet.boolValues[i];
            }

            return copiedPacket;
        }
    }
}
