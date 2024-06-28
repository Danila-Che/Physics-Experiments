using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RopePhysics
{
	/// <summary>
	/// Used algorithm: https://en.wikipedia.org/wiki/Verlet_integration
	/// </summary>
	[BurstCompile]
	internal struct SimulationJob : IJob
	{
		public NativeList<Particle> Particles;
		public float3 AccelerationVector;
		public float DeltaTime;

		public void Execute()
		{
			for (int i = 0; i < Particles.Length; i++)
			{
				if (Particles[i].IsSleep)
				{
					ExecuteFirstStep(i);
				}
				else
				{
					ExecuteSecondStep(i);
				}
			}
		}

		private void ExecuteFirstStep(int index)
		{
			var particle = Particles[index];
			var velocity = 0.5f * DeltaTime * DeltaTime * AccelerationVector;

			particle.OldPosition = particle.Position;
			particle.Position += velocity;
			particle.IsSleep = false;

			Particles[index] = particle;
		}

		private void ExecuteSecondStep(int index)
		{
			var particle = Particles[index];
			var velocity = particle.Position - particle.OldPosition;

			particle.OldPosition = particle.Position;
			particle.Position += velocity + DeltaTime * DeltaTime * AccelerationVector;
			particle.IsSleep = false;

			Particles[index] = particle;
		}
	}
}
