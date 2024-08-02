using Unity.Collections;

namespace RopePhysics
{
    public interface IActor
    {
        NativeArray<Particle> Particles { get; }
        NativeArray<DistanceConstraint> DistanceConstraints { get; }

        void Notify(int startIndexOfParticles, int startIndexOfConstraint);

        void UpdateWith(NativeList<Particle> particles, NativeList<DistanceConstraint> distanceConstraints);
	}
}
