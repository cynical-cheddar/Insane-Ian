namespace Gamestate {
    internal interface IGamestateCommitHandler {
        int actorNumber {get;}

        GamestateTracker.Table tableType {get;}

        void CommitPacket(GamestatePacket packet);
    }
}
