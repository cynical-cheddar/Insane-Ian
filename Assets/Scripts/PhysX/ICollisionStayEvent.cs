
namespace PhysX {
    public interface ICollisionStayEvent {
        bool requiresData { get; }

        void OnCollisionStay();
        void OnCollisionStay(PhysXCollision collision);
    }
}