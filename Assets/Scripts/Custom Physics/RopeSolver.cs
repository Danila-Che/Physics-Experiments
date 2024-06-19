using System;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace CustomPhysics
{
    public class RopeSolver : MonoBehaviour
    {
        [BurstCompile]
        private struct SimulationJob :  IJob
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

        [BurstCompile]
        private struct AttachmentConstraintJob : IJob
        {
            public NativeList<Particle> Particles;
            [ReadOnly]
            public NativeList<AttachmentConstraint> AttachmentConstraints;

            public void Execute()
            {
                for (int i = 0; i < AttachmentConstraints.Length; i++)
                {
                    var constraint = AttachmentConstraints[i];
                    var particle = Particles[constraint.Index];
                    
                    particle.OldPosition = constraint.Position;
                    particle.Position = constraint.Position;
                    particle.IsSleep = true;

                    Particles[constraint.Index] = particle;
                }
            }
        }

        [BurstCompile]
        private struct DistanceConstraintJob : IJob
        {
            public NativeList<Particle> Particles;
            [ReadOnly]
            public NativeList<DistanceConstraint> DistanceConstraints;
            [ReadOnly]
            public NativeList<AttachmentConstraint> AttachmentConstraints;
            public int DistanceConstraintsIterations;

            public void Execute()
            {
                for (int j = 0; j < DistanceConstraintsIterations; j++)
                {
                    for (int i = 0; i < DistanceConstraints.Length; i++)
                    {
                        var constraint = DistanceConstraints[i];

                        var particle1 = Particles[constraint.Index0];
                        var particle2 = Particles[constraint.Index1];
                        
                        var vector = particle2.Position - particle1.Position;
                        var magnitude = math.length(vector);
                        var error = (magnitude - constraint.Distance) / magnitude;

                        for (int k = 0; k < AttachmentConstraints.Length; k++)
                        {
                            if (AttachmentConstraints[k].Index != constraint.Index0)
                            {
                                particle1.Position += 0.5f * error * vector;
                            }
                            else if (AttachmentConstraints[k].Index != constraint.Index1)
                            {
                                particle2.Position -= error * vector;
                            }

                            if (AttachmentConstraints[k].Index != constraint.Index1)
                            {
                                particle2.Position -= 0.5f * error * vector;
                            }
                            else if (AttachmentConstraints[k].Index != constraint.Index0)
                            {
                                particle1.Position += error * vector;
                            }
                        }

                        Particles[constraint.Index0] = particle1;
                        Particles[constraint.Index1] = particle2;
                    }
                }
            }
        }


        [BurstCompile]
        private struct EndStepJob : IJob
        {
            public NativeList<Particle> Particles;

            public void Execute()
            {
                for (int i = 0; i < Particles.Length; i++)
                {
                    var particle = Particles[i];

                    var isSleep = particle.OldPosition == particle.Position;
                    particle.IsSleep = isSleep.x && isSleep.y && isSleep.z;

                    Particles[i] = particle;
                }
            }
        }

        private enum Evaluation
        {
            Sequential,
            Parallel,
        }

        [Serializable]
        private struct Constraint
        {
            public Evaluation Evaluation;
            public int Iterations;
        }

        [SerializeField] private float3 m_GravityVector = new(0.0f, -9.807f, 0.0f);
        [Min(1)]
        [SerializeField] private int m_DistanceConstraintsIterations = 1;

        private List<IContainer> m_Containers;

#if UNITY_INCLUDE_TESTS && UNITY_EDITOR

        public float3 GravityVector { get => m_GravityVector; set => m_GravityVector = value; }
        public List<IContainer> Containers { get => m_Containers; set => m_Containers = value; }
        public int DistanceConstraintsIterations { get => m_DistanceConstraintsIterations; set => m_DistanceConstraintsIterations = value; }

        public void OnStart() => Start();
        public void Destoy() => OnDestroy();

#endif

        private void Start()
        {
            m_Containers = new List<IContainer>();
        }

        private void OnDestroy()
        {
            m_Containers.Clear();
        }

        public void BeginStep()
        {
            m_Containers.ForEach(container =>
            {
                container.Job.Complete();
                container.UpdateContainer();
            });
        }

        public void Substep(float substepTimeInSeconds)
        {
            m_Containers.ForEach(container =>
            {
                container.Job = new SimulationJob
                {
                    Particles = container.Particles,
                    AccelerationVector = m_GravityVector,
                    DeltaTime = substepTimeInSeconds,
                }.Schedule(container.Job);

                container.Job = new AttachmentConstraintJob
                {
                    Particles = container.Particles,
                    AttachmentConstraints = container.AttachmentConstraints,
                }.Schedule(container.Job);

                container.Job = new DistanceConstraintJob
                {
                    Particles = container.Particles,
                    DistanceConstraints = container.DistanceConstraints,
                    AttachmentConstraints = container.AttachmentConstraints,
                    DistanceConstraintsIterations = m_DistanceConstraintsIterations,
                }.Schedule(container.Job);
            });
        }

        public void EndStep()
        {
            m_Containers.ForEach(container =>
            {
                container.Job = new EndStepJob
                {
                    Particles = container.Particles,
                }.Schedule(container.Job);
            });
        }

        public void RegisterContainer(IContainer container)
        {
            m_Containers.Add(container);
        }
    }
}
