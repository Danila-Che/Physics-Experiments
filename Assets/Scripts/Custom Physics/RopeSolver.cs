using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CustomPhysics
{
	public class RopeSolver : MonoBehaviour
	{
		[BurstCompile]
		private struct AttachmentConstraintJob : IJob
		{
			public NativeList<Particle> Particles;
			[ReadOnly]
			public NativeArray<AttachmentConstraint> AttachmentConstraints;

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
			public NativeArray<AttachmentConstraint> AttachmentConstraints;
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

						if (constraint.IsIndex0Free)
						{
							particle1.Position += 0.5f * error * vector;
						}
						else
						{
							particle2.Position -= error * vector;
						}

						if (constraint.IsIndex1Free)
						{
							particle2.Position -= 0.5f * error * vector;
						}
						else
						{
							particle1.Position += error * vector;
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

		[SerializeField] private float3 m_GravityVector = new(0.0f, -9.807f, 0.0f);
		[Min(1)]
		[SerializeField] private int m_DistanceConstraintsIterations = 1;
		[Min(0.0f)]
		[SerializeField] private float m_ParticleColliderRadius = 0.1f;
		[Min(0)]
		[SerializeField] private int m_ColliderBufferSize = 1;

		private List<IContainer> m_Containers;
		private Collider[] m_ColliderHitBuffer;
		private GameObject m_ColliderHolder;
		private SphereCollider m_ParticleCollider;

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
			m_ColliderHitBuffer = new Collider[m_ColliderBufferSize];

			m_ColliderHolder = new GameObject
			{
				name = "Particle Collider"
			};
			m_ColliderHolder.SetActive(false);

			m_ParticleCollider = m_ColliderHolder.AddComponent<SphereCollider>();
			m_ParticleCollider.radius = m_ParticleColliderRadius;

			m_Containers.Add(GetComponentInChildren<Rope>());
		}

		private void OnDestroy()
		{
			m_Containers?.Clear();
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

				AdjustCollisions(container);
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

		//public void RegisterContainer(IContainer container)
		//{
		//	m_Containers.Add(container);
		//}

		private void AdjustCollisions(IContainer container)
		{
			container.Job.Complete();

			var particles = container.Particles;

			for (int i = 0; i < particles.Length; i++)
			{
				var particle = particles[i];

				var collidersCount = Physics.OverlapSphereNonAlloc(particle.Position, m_ParticleColliderRadius, m_ColliderHitBuffer);

				m_ColliderHolder.SetActive(true);

				for (int j = 0; j < collidersCount; j++)
				{
					var colliderPosition = m_ColliderHitBuffer[j].transform.position;
					var colliderRotation = m_ColliderHitBuffer[j].transform.rotation;

					Physics.ComputePenetration(
						m_ParticleCollider, particle.Position, Quaternion.identity,
						m_ColliderHitBuffer[j], colliderPosition, colliderRotation,
						out Vector3 direction,
						out float distance);

					particle.Position += (float3)direction * distance;
				}

				m_ColliderHolder.SetActive(false);

				particles[i] = particle;
			}
		}
	}
}
