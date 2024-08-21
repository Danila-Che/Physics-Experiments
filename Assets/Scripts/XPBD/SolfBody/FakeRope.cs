using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace XPBD.SoftBody
{
	public class FakeRope : IActor, IConstrainable, IDisposable
	{
		private readonly FakeJointContainer m_FakeJointContainer;
		private readonly float m_SpanDistance;
		private readonly float3 m_Drag;
		private readonly List<FakeParticle> m_Particles;
		private readonly List<FakeDistanceConstraint> m_DistanceConstraints;

		public FakeRope(FakeJointContainer fakeJointContainer, float spanDistance, float3 drag)
			: this(spanDistance, drag)
		{
			m_FakeJointContainer = fakeJointContainer;
		}

		public FakeRope(float spanDistance, float3 drag)
		{
			m_SpanDistance = spanDistance;
			m_Drag = drag;
			m_Particles = new List<FakeParticle>();
			m_DistanceConstraints = new List<FakeDistanceConstraint>();
		}

		public List<FakeParticle> Particles => m_Particles;

		public void Dispose()
		{
			m_Particles.Clear();
		}

		public void BeginStep()
		{
			for (int i = 0; i < m_Particles.Count; i++)
			{
				var particle = m_Particles[i];

				particle.PreviousPosition = particle.Position;

				m_Particles[i] = particle;
			}
		}

		public void ApplyAcceleration(float deltaTime, float3 acceleration)
		{
			for (int i = 0; i < m_Particles.Count; i++)
			{
				var particle = m_Particles[i];

				particle.Velocity += acceleration * deltaTime;

				m_Particles[i] = particle;
			}
		}

		public void ApplyDrag(float deltaTime)
		{
			for (int i = 0; i < m_Particles.Count; i++)
			{
				var particle = m_Particles[i];

				var drag = particle.Velocity * m_Drag;
				particle.Velocity -= particle.InverseMass * deltaTime * drag;

				m_Particles[i] = particle;
			}
		}

		public void Step(float deltaTime)
		{
			for (int i = 0; i < m_Particles.Count; i++)
			{
				var particle = m_Particles[i];

				particle.Position += particle.Velocity * deltaTime;

				m_Particles[i] = particle;
			}
		}

		public void EndStep(float deltaTime)
		{
			for (int i = 0; i < m_Particles.Count; i++)
			{
				var particle = m_Particles[i];

				particle.Velocity = (particle.Position - particle.PreviousPosition) / deltaTime;

				m_Particles[i] = particle;
			}
		}

		public void SolveConstraints(float deltaTime)
		{
			if (m_FakeJointContainer == null)
			{
				SolveInnerConstraints();
			}
			else
			{
				m_FakeJointContainer.RecalculateGlobalPoses();

				SolverOuterConstraint(
					particleIndex: 0,
					m_FakeJointContainer.TargetBody,
					m_FakeJointContainer.TargetGlobalPose.Position,
					deltaTime);

				SolveInnerConstraints();

				SolverOuterConstraint(
					m_Particles.Count - 1,
					m_FakeJointContainer.AnchorBody,
					m_FakeJointContainer.AnchorGlobalPose.Position,
					deltaTime);
			}
		}

		public void CreateFromJoint()
		{
			if (m_FakeJointContainer == null)
			{
				throw new ArgumentNullException(nameof(m_FakeJointContainer));
			}

			m_FakeJointContainer.RecalculateGlobalPoses();

			CreateParticles(m_FakeJointContainer.AnchorGlobalPose.Position, m_FakeJointContainer.TargetGlobalPose.Position);
			CreateConstraints();
		}

		public void Create(float3 sourcePosition, float3 targetPosition)
		{
			CreateParticles(sourcePosition, targetPosition);
			CreateConstraints();
		}

		private void CreateParticles(float3 sourcePosition, float3 targetPosition)
		{
			var vector = sourcePosition - targetPosition;
			var normal = math.normalize(vector);
			var magnitude = math.length(vector);
			var particleCount = 1 + (int)math.ceil(magnitude / m_SpanDistance);

			m_Particles.Capacity = particleCount;

			for (int i = 0; i < particleCount; i++)
			{
				m_Particles.Add(new FakeParticle(targetPosition + m_SpanDistance * i * normal, mass: 1.0f));
			}

			m_Particles.Add(new FakeParticle(sourcePosition, mass: 1.0f));
		}

		private void CreateConstraints()
		{
			m_DistanceConstraints.Capacity = m_Particles.Count - 1;

			for (int i = 0; i < m_Particles.Count - 1; i++)
			{
				var distance = math.distance(m_Particles[i].Position, m_Particles[i + 1].Position);

				m_DistanceConstraints.Add(new FakeDistanceConstraint(i, i + 1, distance));
			}
		}

		private void SolveInnerConstraints()
		{
			for (int i = 0; i < m_DistanceConstraints.Count; i++)
			{
				var constraint = m_DistanceConstraints[i];

				var particle0 = m_Particles[constraint.Index0];
				var particle1 = m_Particles[constraint.Index1];

				var direction = particle1.Position - particle0.Position;
				var length = math.length(direction);

				if (length > 0.0f)
				{
					var error = (length - constraint.Distance) / length;

					var w = particle0.InverseMass + particle1.InverseMass;

					var k0 = particle0.InverseMass / w;
					var k1 = particle1.InverseMass / w;

					particle0.Position += k0 * error * direction;
					particle1.Position -= k1 * error * direction;

					m_Particles[constraint.Index0] = particle0;
					m_Particles[constraint.Index1] = particle1;
				}
			}
		}

		private void SolverOuterConstraint(int particleIndex, FakeBody body, float3 globalPosition, float deltaTime)
		{
			var particle = m_Particles[particleIndex];

			var correction = FakeUtilities.CalculateCorrection(globalPosition, particle.Position);

			//if (math.lengthsq(correction) < math.EPSILON)
			//{
			//	return;
			//}

			if (math.any(correction))
			{
				FakeBody.ApplyBodyPairCorrection(
					body,
					null,
					correction,
					0.0f,
					deltaTime,
					globalPosition,
					particle.Position);

				var bodyInverseMass = body.IsKinematic ? 0.0f : body.GetInverseMass(math.normalize(correction), globalPosition);

				var w = bodyInverseMass + particle.InverseMass;
				var k = particle.InverseMass / w;

				//UnityEngine.Debug.Log($"> {particleIndex} {math.normalize(correction)} {bodyInverseMass} {k} {correction} {k * correction}");

				particle.Position -= k * correction;

				m_Particles[particleIndex] = particle;
			}
		}
	}
}
