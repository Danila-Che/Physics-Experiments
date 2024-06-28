using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RopePhysics
{
	internal struct SleepJob : IJob
	{
		public NativeList<Particle> Particles;

		public void Execute()
		{
			for (int i = 0; i < Particles.Length; i++)
			{
				var particle = Particles[i];
				var isSleep = particle.OldPosition == particle.Position;

				particle.IsSleep = math.all(isSleep);

				Particles[i] = particle;
			}
		}
	}
}
