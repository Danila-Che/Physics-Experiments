using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CustomPhysics
{
    public class Rope : MonoBehaviour, IContainer
    {
        public struct ParticleAlias
        {
            public int Index;
        }

        [Min(0.01f)]
        [SerializeField] private float m_ParticleDistance = 1.0f;

        private ParticleAttachment[] m_ParticleAttachments;

        private JobHandle m_Job;
        private NativeList<Particle> m_Particles;
        private NativeList<DistanceConstraint> m_DistanceConstraints;
        private NativeList<AttachmentConstraint> m_AttachmentConstraints;
        private Dictionary<ParticleAttachment, int> m_ParticleAttachmentMap;

        private AttachmentParticle m_StartRopeFrom = AttachmentParticle.Bottom;
        private ParticleAlias m_TopParticleAlias;
        private ParticleAlias m_BottomParticleAlias;

        public JobHandle Job { get => m_Job; set => m_Job = value; }

        public NativeList<Particle> Particles => m_Particles;

        public NativeList<DistanceConstraint> DistanceConstraints => m_DistanceConstraints;
        
        public NativeList<AttachmentConstraint> AttachmentConstraints => m_AttachmentConstraints;
        
        public ParticleAlias TopParticleAlias => m_TopParticleAlias;
        
        public ParticleAlias BottomParticleAlias => m_BottomParticleAlias;

#if UNITY_INCLUDE_TESTS && UNITY_EDITOR

        public float Test_ParticleDistance { get => m_ParticleDistance; set => m_ParticleDistance = value; }
        public NativeList<DistanceConstraint> Test_DistanceConstraints { get => m_DistanceConstraints; set => m_DistanceConstraints = value; }
        public ParticleAttachment[] Test_ParticleAttachments { get => m_ParticleAttachments; set => m_ParticleAttachments = value; }
        public AttachmentParticle Test_StartRopeFrom { get => m_StartRopeFrom; set => m_StartRopeFrom = value; }

        public void OnStart() => Start();
        public void Destroy() => OnDestroy();

#endif

        private void Start()
        {
            m_ParticleAttachments = GetComponents<ParticleAttachment>();
            m_ParticleAttachmentMap = new Dictionary<ParticleAttachment, int>();
            m_Particles = new NativeList<Particle>(Allocator.Persistent);
            m_DistanceConstraints = new NativeList<DistanceConstraint>(Allocator.Persistent);
            m_AttachmentConstraints = new NativeList<AttachmentConstraint>(Allocator.Persistent);

            if (TryGetComponent<IRopeDirection>(out var direction))
            {
                m_StartRopeFrom = direction.StartRopeFrom;
            }

            CreateRope();
            RegisterInSolver();
            InitAttachmentConstraints();
        }

        private void OnDestroy()
        {
            m_ParticleAttachments = null;
            m_ParticleAttachmentMap = null;

            if (m_Particles.IsCreated)
            {
                m_Particles.Dispose();
            }

            if (m_DistanceConstraints.IsCreated)
            {
                m_DistanceConstraints.Dispose();
            }
            
            if (m_AttachmentConstraints.IsCreated)
            {
                m_AttachmentConstraints.Dispose();
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (m_Particles.IsCreated)
            {
                Gizmos.color = Color.green;

                m_Job.Complete();

                for (int i = 0; i < m_Particles.Length; i++)
                {
                    Gizmos.DrawSphere(m_Particles[i].Position, 0.1f);
                }
            }
        }

#endif

        public void UpdateContainer()
        {
            foreach (var pair in m_ParticleAttachmentMap)
            {
                var constraint = m_AttachmentConstraints[pair.Value];

                constraint.Position = pair.Key.Taget.transform.position;

                m_AttachmentConstraints[pair.Value] = constraint;
            }
        }

        private void InitAttachmentConstraints()
        {
            for (int i = 0; i < m_ParticleAttachments.Length; i++)
            {
                if (m_ParticleAttachments[i].Type is ParticleAttachment.AttachmentType.Static)
                {
                    var index = m_ParticleAttachments[i].AttachmentParticle switch
                    {
                        AttachmentParticle.Top => TopParticleAlias.Index,
                        AttachmentParticle.Bottom => BottomParticleAlias.Index,
                        _ => throw new System.NotImplementedException(),
                    };

                    m_ParticleAttachmentMap.Add(m_ParticleAttachments[i], m_AttachmentConstraints.Length);
                    m_AttachmentConstraints.Add(new AttachmentConstraint(index));
                }
            }
        }

        private void CreateRope()
        {
            if (m_ParticleAttachments.Length != 2) { return; }

            var position0 = m_ParticleAttachments[0].Test_Target.transform.position;
            var position1 = m_ParticleAttachments[1].Test_Target.transform.position;

            if (m_ParticleAttachments[0].AttachmentParticle == m_StartRopeFrom)
            {
                CreateRope(position0, position1);
            }
            else
            {
                CreateRope(position1, position0);
            }

            if (m_StartRopeFrom is AttachmentParticle.Bottom)
            {
                m_BottomParticleAlias.Index = 0;
                m_TopParticleAlias.Index = m_Particles.Length - 1;
            }
            else
            {
                m_TopParticleAlias.Index = 0;
                m_BottomParticleAlias.Index = m_Particles.Length - 1;
            }
        }

        private void CreateRope(Vector3 startParticlePosition, Vector3 finishParticlePosition)
        {
            var vector = finishParticlePosition - startParticlePosition;
            var normal = vector.normalized;
            var particleCount = (vector.magnitude % m_ParticleDistance == 0.0f) switch
            {
                true => 1 + (int)(vector.magnitude / m_ParticleDistance),
                false => 2 + (int)(vector.magnitude / m_ParticleDistance),
            };
            var constraintCount = particleCount - 1;

            m_Particles.SetCapacity(particleCount);
            m_DistanceConstraints.SetCapacity(constraintCount);

            for (int i = 0; i < particleCount - 1; i++)
            {
                var position = startParticlePosition + i * m_ParticleDistance * normal;

                m_Particles.AddNoResize(new Particle(position));
            }

            m_Particles.AddNoResize(new Particle(finishParticlePosition));

            for (int i = 0; i < constraintCount; i++)
            {
                m_DistanceConstraints.AddNoResize(CreateDistanceConstraint(i));
            }
        }

        private void RegisterInSolver()
        {
            var solver = GetComponentInParent<RopeSolver>();

            if (solver != null)
            {
                solver.RegisterContainer(this);
            }
        }

        private DistanceConstraint CreateDistanceConstraint(int particleIndex)
        {
            var distance = math.length(m_Particles[particleIndex].Position - m_Particles[particleIndex + 1].Position);

            return new DistanceConstraint(particleIndex, particleIndex + 1, distance);
        }
    }
}
