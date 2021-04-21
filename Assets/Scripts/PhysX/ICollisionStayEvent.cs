
namespace PhysX {
    public interface ICollisionStayEvent {
        bool requiresData { get; }

        void CollisionStay();
        void CollisionStay(PhysXCollision collision);
    }
}