using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RopePhysics
{
	public class Solver : IDisposable
	{
		private NativeList<Particle> m_Particles;
		private readonly NativeList<DistanceConstraint> m_Constraints;
		private readonly List<IActor> m_Actors;

		private readonly float3 m_Gravity;
		private readonly List<Rope> m_Ropes;
		private readonly int m_DistanceConstraintsIterations;
		private readonly bool m_NeedDistanceConstraint;
		private readonly bool m_NeedCollisionDetection;

		private readonly float m_ParticleColliderRadius;
		private readonly Collider[] m_ColliderHitBuffer;
		private readonly GameObject m_ColliderHolder;
		private readonly SphereCollider m_ParticleCollider;

		private JobHandle m_JobHandle;

		public Solver() : this(float3.zero) { }

		public Solver(float3 gravity) : this(gravity, 1, true) { }

		public Solver(
			float3 gravity,
			int distanceConstraintsIterations,
			bool needDistanceConstraint,
			bool needCollisionDetection,
			float particleColliderRadius,
			int colliderBufferSize)
			: this(gravity, distanceConstraintsIterations, needDistanceConstraint, needCollisionDetection)
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

		public Solver(
			float3 gravity,
			int distanceConstraintsIterations,
			bool needDistanceConstraint,
			bool needCollisionDetection)
		{
			m_Gravity = gravity;
			m_DistanceConstraintsIterations = distanceConstraintsIterations;
			m_Ropes = new List<Rope>();
			m_NeedDistanceConstraint = needDistanceConstraint;
			m_NeedCollisionDetection = needCollisionDetection;

			m_Particles = new NativeList<Particle>(Allocator.Persistent);
			m_Constraints = new NativeList<DistanceConstraint>(Allocator.Persistent);
			m_Actors = new List<IActor>();
		}

		public Solver(float3 gravity, int distanceConstraintsIterations, bool needDistanceConstraint) { }

		public List<Rope> Ropes => m_Ropes;

		public NativeList<Particle> Particles => m_Particles;

		public NativeList<DistanceConstraint> Constraints => m_Constraints;

		public int NextParticleId => Particles.Length;
		public int NextConstraintId => Constraints.Length;

		public void Dispose()
		{
			if (m_Particles.IsCreated)
			{
				m_Particles.Dispose();
			}

			if (m_Constraints.IsCreated)
			{
				m_Constraints.Dispose();
			}
		}

		public void Register(Rope rope)
		{
			m_Ropes.Add(rope);
		}

		public void Register(IActor actor)
		{
			actor.Notify(m_Particles.Length, m_Constraints.Length);
			m_Actors.Add(actor);

			m_Particles.AddRange(actor.Particles);
			m_Constraints.AddRange(actor.DistanceConstraints);
		}

		public void BeginStep()
		{
			m_JobHandle.Complete();

			//m_Ropes.ForEach(rope => BeginStep(rope));
		}

		public void Step(float substepTimeInSeconds)
		{
			m_JobHandle = new SimulationJob
			{
				Particles = m_Particles,
				AccelerationVector = m_Gravity,
				DeltaTime = substepTimeInSeconds,
			}.Schedule(m_JobHandle);

			//if (m_NeedDistanceConstraint)
			//{
			//	m_JobHandle = new DistanceConstraintJob
			//	{
			//		Particles = m_Particles,
			//		DistanceConstraints = m_Constraints,
			//		DistanceConstraintsIterations = m_DistanceConstraintsIterations,
			//	}.Schedule(m_JobHandle);
			//}

			//m_Ropes.ForEach(rope => Step(rope, substepTimeInSeconds));
		}

		public void Constraint()
		{
			m_JobHandle = new DistanceConstraintJob
			{
				Particles = m_Particles,
				DistanceConstraints = m_Constraints,
				DistanceConstraintsIterations = m_DistanceConstraintsIterations,
			}.Schedule(m_JobHandle);
		}

		public void EndStep()
		{
			m_JobHandle.Complete();
			//m_Actors.ForEach(actor => actor.UpdateWith(m_Particles, m_Constraints));

			//m_Ropes.ForEach(rope => EndStep(rope));
		}

		private void BeginStep(Rope rope)
		{
			rope.Job.Complete();
			rope.ActualisePositions();
			rope.ParitclesCopy.CopyFrom(rope.Particles);
		}

		private void Step(Rope rope, float substepTimeInSeconds)
		{
			//rope.Job = new SimulationJob
			//{
			//	Particles = rope.Particles,
			//	AccelerationVector = m_Gravity,
			//	DeltaTime = substepTimeInSeconds,
			//}.Schedule(rope.Job);

			//rope.Job = new AttachementConstraintJob
			//{
			//	Particles = rope.Particles,
			//	TargetParticleAttachmentConstraint = rope.TargetParticleAttachmentConstraint,
			//	SourceParticleAttachmentConstraint = rope.SourceParticleAttachmentConstraint,
			//}.Schedule(rope.Job);

			//if (m_NeedDistanceConstraint)
			//{
			//	rope.Job = new DistanceConstraintJob
			//	{
			//		Particles = rope.Particles,
			//		DistanceConstraints = rope.DistanceConstraints,
			//		DistanceConstraintsIterations = m_DistanceConstraintsIterations,
			//	}.Schedule(rope.Job);
			//}

			//if (m_NeedCollisionDetection)
			//{
			//	AdjustCollisions(rope);
			//}
		}

		private void EndStep(Rope rope)
		{
			//rope.Job = new SleepJob
			//{
			//	Particles = rope.Particles,
			//}.Schedule(rope.Job);

			//rope.Job.Complete();
			//rope.UpdatePositions();
		}

		public void AdjustCollisions()
		{
			if (m_ColliderHitBuffer == null) { return; }
			if (m_ColliderHolder == null) { return; }
			if (m_ParticleCollider == null) { return; }

			m_JobHandle.Complete();

			for (int i = 0; i < m_Particles.Length; i++)
			{
				var particle = m_Particles[i];
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

					particle.OldPosition = particle.Position;
					particle.Position += (float3)direction * distance;
				}

				m_ColliderHolder.SetActive(false);
				m_Particles[i] = particle;
			}
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
