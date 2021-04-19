
namespace PhysX {
    public interface ITriggerExitEvent {
        bool requiresData { get; }

        void TriggerExit();
        void TriggerExit(PhysXCollider collider);
    }
}