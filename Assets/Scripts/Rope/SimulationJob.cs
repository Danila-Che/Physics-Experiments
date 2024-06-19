using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Verlet
{
    [BurstCompile]
    public struct SimulationJob : IJob
    {
        public NativeArray<Particle> Particles;
        public float3 Gravity;
        public float DeltaTime;
        public int SimulationIterationCount;
        public int ConstraintsIterationCount;
        public float RopeSectionLength;
        public float3 ObjectRopeIsConnectedToPosition;

        public void Execute()
        {
            SimulationIterationCount = math.max(SimulationIterationCount, 1);
            ConstraintsIterationCount = math.max(ConstraintsIterationCount, 0);

            DeltaTime /= SimulationIterationCount;

            for (int iteration = 0; iteration < SimulationIterationCount; iteration++)
            {
                var topParticle = Particles[0];
                topParticle.Position = ObjectRopeIsConnectedToPosition;
                Particles[0] = topParticle;

                for (int i = 1; i < Particles.Length; i++)
                {
                    var particle = Particles[i];
                    var position = particle.Position;

                    particle.Position = 2*position - particle.PreviousPosition + Gravity*DeltaTime*DeltaTime;
                    particle.PreviousPosition = position;

                    Particles[i] = particle;
                }

                ApplyConstraints();
            }
        }

        [BurstCompile]
        private void ApplyConstraints()
        {
            for (int iteration = 0; iteration < ConstraintsIterationCount; iteration++)
            {
                for (int i = 0; i < Particles.Length - 1; i++)
                {
                    var topSection = Particles[i];
                    var bottomSection = Particles[i + 1];

                    var distance = math.distance(topSection.Position, bottomSection.Position);
                    var distanceError = math.abs(distance - RopeSectionLength);

                    float3 changeDirection;

                    if (distance > RopeSectionLength)
                    {
                        //Compress
                        changeDirection = math.normalize(topSection.Position - bottomSection.Position);
                    }
                    else if (distance < RopeSectionLength)
                    {
                        //Extend
                        changeDirection = math.normalize(bottomSection.Position - topSection.Position);
                    }
                    else
                    {
                        continue;
                    }

                    var change = changeDirection * distanceError;

                    if (i != 0)
                    {
                        bottomSection.Position += change * 0.5f;
                        Particles[i + 1] = bottomSection;

                        topSection.Position -= change * 0.5f;
                        Particles[i] = topSection;
                    }
                    //Because the rope is connected to something
                    else
                    {
                        bottomSection.Position += change;
                        Particles[i + 1] = bottomSection;
                    }
                }
            }
        }
    }
}
