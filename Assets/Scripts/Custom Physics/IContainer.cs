using Unity.Collections;
using Unity.Jobs;

namespace CustomPhysics
{
    public interface IContainer
    {
        JobHandle Job { get; set; }
        NativeList<Particle> Particles { get; }
        NativeList<DistanceConstraint> DistanceConstraints { get; }
        NativeList<AttachmentConstraint> AttachmentConstraints { get; }

        void UpdateContainer();
    }
}
