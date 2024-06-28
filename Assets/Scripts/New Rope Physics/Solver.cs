using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RopePhysics
{
	public class Solver
	{
		private readonly float3 m_Gravity;
		private readonly List<Rope> m_Ropes;
		private readonly int m_DistanceConstraintsIterations;
		private readonly bool m_NeedDistanceConstraint;

		private readonly float m_ParticleColliderRadius;
		private readonly Collider[] m_ColliderHitBuffer;
		private readonly GameObject m_ColliderHolder;
		private readonly SphereCollider m_ParticleCollider;

		public Solver() : this(float3.zero) { }

		public Solver(float3 gravity) : this(gravity, 1, true) { }

		public Solver(float3 gravity, int distanceConstraintsIterations, bool needDistanceConstraint, float particleColliderRadius, int colliderBufferSize)
			:this(gravity, distanceConstraintsIterations, needDistanceConstraint )
		{
			m_ColliderHitBuffer = new Collider[colliderBufferSize];
			m_ColliderHolder = new GameObject
			{
				name = "Particle Collider"
			};
			m_ColliderHolder.SetActive(false);

			m_ParticleCollider = m_ColliderHolder.AddComponent<SphereCollider>();
			m_ParticleCollider.radius = m_ParticleColliderRadius;
			m_ParticleColliderRadius = particleColliderRadius;
		}

		public Solver(float3 gravity, int distanceConstraintsIterations, bool needDistanceConstraint)
		{
			m_Gravity = gravity;
			m_DistanceConstraintsIterations = distanceConstraintsIterations;
			m_Ropes = new List<Rope>();
			m_NeedDistanceConstraint = needDistanceConstraint;
		}

		public List<Rope> Ropes => m_Ropes;

		public void Register(Rope rope)
		{
			m_Ropes.Add(rope);
		}

		public void BeginStep()
		{
			m_Ropes.ForEach(rope => BeginStep(rope));
		}

		public void Step(float substepTimeInSeconds)
		{
			m_Ropes.ForEach(rope => Step(rope, substepTimeInSeconds));
		}

		public void EndStep()
		{
			m_Ropes.ForEach(rope => EndStep(rope));
		}

		private void BeginStep(Rope rope)
		{
			rope.Job.Complete();
			rope.Update();
		}

		private void Step(Rope rope, float substepTimeInSeconds)
		{
			rope.Job = new SimulationJob
			{
				Particles = rope.Particles,
				AccelerationVector = m_Gravity,
				DeltaTime = substepTimeInSeconds,
			}.Schedule(rope.Job);

			if (m_NeedDistanceConstraint)
			{
				rope.Job = new DistanceConstraintJob
				{
					Particles = rope.Particles,
					DistanceConstraints = rope.DistanceConstraints,
					DistanceConstraintsIterations = m_DistanceConstraintsIterations,
					TargetParticleAttachmentConstraint = rope.TargetParticleAttachmentConstraint,
					SourceParticleAttachmentConstraint = rope.SourceParticleAttachmentConstraint,
				}.Schedule(rope.Job);
			}

			AdjustCollisions(rope);
		}

		private void EndStep(Rope rope)
		{
			rope.Job = new SleepJob
			{
				Particles = rope.Particles,
			}.Schedule(rope.Job);
		}

		private void AdjustCollisions(Rope rope)
		{
			if (m_ColliderHitBuffer == null) { return; }
			if (m_ColliderHolder == null) { return; }
			if (m_ParticleCollider == null) { return; }

			rope.Job.Complete();

			var particles = rope.Particles;

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
