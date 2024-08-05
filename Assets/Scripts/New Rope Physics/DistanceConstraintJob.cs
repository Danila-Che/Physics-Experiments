using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RopePhysics
{
	/// <summary>
	/// Used algorithm of constraint Projection
	/// https://www.cs.toronto.edu/~jacobson/seminar/mueller-et-al-2007.pdf
	/// https://en.wikipedia.org/wiki/Verlet_integration
	/// From target to source
	/// </summary>
	[BurstCompile]
	public struct DistanceConstraintJob : IJob
	{
		public NativeList<Particle> Particles;
		[ReadOnly]
		public NativeList<DistanceConstraint> DistanceConstraints;
		public int DistanceConstraintsIterations;

		public void Execute()
		{
			for (int iteration = 0; iteration < DistanceConstraintsIterations; iteration++)
			{
				for (int i = 0; i < DistanceConstraints.Length; i++)
				{
					var constraint = DistanceConstraints[i];

					var particle0 = Particles[constraint.Index0];
					var particle1 = Particles[constraint.Index1];

					var direction = particle1.Position - particle0.Position;
					var length = math.length(direction);

					if (length > 0.0f)
					{
						var error = (length - constraint.Distance) / length;

						var w = particle0.InverseMass + particle1.InverseMass;

						var k0 = particle0.InverseMass / w;
						var k1 = particle1.InverseMass / w;

						particle0.Position += k0 * error * direction;
						particle1.Position -= k1 * error * direction;

						Particles[constraint.Index0] = particle0;
						Particles[constraint.Index1] = particle1;
					}
				}
			}
		}
	}
}
