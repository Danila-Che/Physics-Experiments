using System;
using Unity.Collections;

namespace RopePhysics
{
	public class Constraint : IActor, IDisposable
	{
		private readonly NativeArray<Particle> m_EmptyParticles;
		private readonly NativeArray<DistanceConstraint> m_Constraint;

		public Constraint(float distance, int particle0Index, int particle1Index)
		{
			m_EmptyParticles = new NativeArray<Particle>(0, Allocator.Persistent);
			m_Constraint = new NativeArray<DistanceConstraint>(1, Allocator.Persistent);

			m_Constraint[0] = new DistanceConstraint(distance, particle0Index, particle1Index);
		}

		public NativeArray<Particle> Particles => m_EmptyParticles;

		public NativeArray<DistanceConstraint> DistanceConstraints => m_Constraint;

		public void Dispose()
		{
			if (m_EmptyParticles.IsCreated)
			{
				m_EmptyParticles.Dispose();
			}

			if (m_Constraint.IsCreated)
			{
				m_Constraint.Dispose();
			}
		}

		public void Notify(int startIndexOfParticles, int startIndexOfConstraint) { }

		public void UpdateWith(NativeList<Particle> particles, NativeList<DistanceConstraint> distanceConstraints) { }
	}
}
