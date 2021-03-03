namespace Gamestate {
    internal interface IGamestateCommitHandler {
        int actorNumber {get;}

        void CommitPacket(GamestatePacket packet);
    }
}
