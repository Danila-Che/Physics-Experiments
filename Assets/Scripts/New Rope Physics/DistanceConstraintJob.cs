using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RopePhysics
{
	/// <summary>
	/// Used algorithm Forwards and Backwards Reaching Inverse Kinematics (FABRIC)
	/// https://en.wikipedia.org/wiki/Inverse_kinematics
	/// https://www.sciencedirect.com/science/article/pii/S1524070311000178?via%3Dihub
	/// From target to source
	/// </summary>
	[BurstCompile]
	internal struct DistanceConstraintJob : IJob
	{
		public NativeList<Particle> Particles;
		[ReadOnly]
		public NativeList<DistanceConstraint> DistanceConstraints;
		[ReadOnly]
		public AttachmentConstraint TargetParticleAttachmentConstraint;
		[ReadOnly]
		public AttachmentConstraint SourceParticleAttachmentConstraint;
		public int DistanceConstraintsIterations;

		public void Execute()
		{
			for (int j = 0; j < DistanceConstraintsIterations; j++)
			{
				var particle = Particles[0];
				particle.Position = TargetParticleAttachmentConstraint.Position;
				Particles[0] = particle;

				for (int i = 1; i < Particles.Length; i++)
				{
					ConstraintByDistance(i, i - 1, DistanceConstraints[i - 1].Distance);
				}

				particle = Particles[^1];
				particle.Position = SourceParticleAttachmentConstraint.Position;
				Particles[^1] = particle;

				for (int i = Particles.Length - 1; i > 0; i--)
				{
					ConstraintByDistance(i - 1, i, DistanceConstraints[i - 1].Distance);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ConstraintByDistance(int particleIndex, int anchorIndex, float distance)
		{
			var particle = Particles[particleIndex];
			var anchor = Particles[anchorIndex];

			particle.Position = CalculateConstraintDistance(particle.Position, anchor.Position, distance);

			Particles[particleIndex] = particle;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float3 CalculateConstraintDistance(float3 particle, float3 anchor, float distance)
		{
			return anchor + distance * math.normalize(particle - anchor);
		}
	}
}
