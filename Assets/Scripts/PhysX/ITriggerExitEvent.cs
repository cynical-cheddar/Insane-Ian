
namespace PhysX {
    public interface ITriggerExitEvent {
        bool requiresData { get; }

        void OnTriggerExit();
        void OnTriggerExit(PhysXCollider collider);
    }
}