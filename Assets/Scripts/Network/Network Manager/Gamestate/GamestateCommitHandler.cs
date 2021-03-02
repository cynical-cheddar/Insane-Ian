namespace Gamestate {
    internal interface IGamestateCommitHandler {
        void CommitPacket(GamestatePacket packet);
    }
}