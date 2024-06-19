using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Verlet
{
    public class Rope : MonoBehaviour
    {
        [SerializeField] private ObjectHangingFromRope m_ObjectHangingFromRope;
        [Min(1)]
        [SerializeField] private int m_SimulationIterationCount;
        [Min(0)]
        [SerializeField] private int m_ConstraintsIterationCount;
        [SerializeField] private float3 m_Gravity = new(0.0f, -9.81f, 0.0f);

        [Min(0.01f)]
	    [SerializeField] private float m_InitRopeLength = 7.0f;
	    [Min(2)]
	    [SerializeField] private int m_InitRopeParticleCount = 15;
        [Min(0.01f)]
	    [SerializeField] private float m_RopeWidth = 0.2f;

        private NativeArray<Particle> m_Particles;
        private Rigidbody[] m_WorldParticles;
        private float m_RopeSectionLength;
        private JobHandle m_Job;

        private void Start()
        {
            m_Particles = new NativeArray<Particle>(m_InitRopeParticleCount, Allocator.Persistent);
            m_WorldParticles = new Rigidbody[m_InitRopeParticleCount];
            m_RopeSectionLength = m_InitRopeLength / (float)m_InitRopeParticleCount;
            
            m_ObjectHangingFromRope.Init();
            CreateRope();
        }

        private void OnDestroy()
        {
            if (m_Particles.IsCreated)
            {
                m_Particles.Dispose();
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < m_InitRopeParticleCount; i++)
            {
                var particle = m_Particles[i];
                particle.Position = m_WorldParticles[i].position;
                m_Particles[i] = particle;
            }

            m_Job = new SimulationJob
            {
                Particles = m_Particles,
                DeltaTime = Time.fixedDeltaTime,
                SimulationIterationCount = m_SimulationIterationCount,
                ConstraintsIterationCount = m_ConstraintsIterationCount,
                Gravity = m_Gravity,
                RopeSectionLength = m_RopeSectionLength,
                ObjectRopeIsConnectedToPosition = transform.position,
            }.Schedule(m_Job);

            m_Job.Complete();

            for (int i = 0; i < m_InitRopeParticleCount; i++)
            {
                m_WorldParticles[i].position = m_Particles[i].Position;
            }

            m_ObjectHangingFromRope.SetPosition(m_Particles[^1].Position);
        }

        #if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            m_Job.Complete();
            Gizmos.color = Color.green;

            for (int i = 0; i < m_Particles.Length; i++)
            {
                Gizmos.DrawSphere(m_Particles[i].Position, m_RopeWidth);
            }
        }

        #endif

        private void CreateRope()
        {
            var ropeParticlePosition = transform.position;

            for (int i = 0; i < m_InitRopeParticleCount; i++)
            {
                m_Particles[i] = new Particle {
                    PreviousPosition = ropeParticlePosition,
                    Position = ropeParticlePosition,
                };
                m_WorldParticles[i] = CreateParticle(ropeParticlePosition);
                ropeParticlePosition.y -= m_RopeSectionLength;
            }
        }

        private Rigidbody CreateParticle(Vector3 position)
        {
            var particle = new GameObject();
            particle.transform.position = position;
            SceneManager.MoveGameObjectToScene(particle, gameObject.scene);

            var sphereCollider = particle.AddComponent<SphereCollider>();
            sphereCollider.radius = m_RopeWidth;

            var rigidbody = particle.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.drag = 1.0f;

            return rigidbody;
        }
    }
}
