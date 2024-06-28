using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

namespace CustomPhysics
{
	[BurstCompile]
    internal struct SimulationJob :  IJob
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
                    Setup(i);
                }
                else
                {
                    Simulate(i);
                }
            }
        }

        private void Setup(int index)
        {
            var particle = Particles[index];

            particle.OldPosition = particle.Position;
            particle.Position += 0.5f*DeltaTime*DeltaTime*AccelerationVector;
            particle.IsSleep = false;

            Particles[index] = particle;
        }

        private void Simulate(int index)
        {
            var particle = Particles[index];
            var velocity = particle.Position - particle.OldPosition;

            particle.OldPosition = particle.Position;
            particle.Position += velocity + DeltaTime*DeltaTime*AccelerationVector;

            Particles[index] = particle;
        }
    }
}
