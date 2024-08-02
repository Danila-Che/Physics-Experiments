using System;
using Unity.Collections;
using Unity.Mathematics;

namespace RopePhysics
{
	public class Point : IActor, IDisposable
	{
		private readonly NativeArray<Particle> m_Particle;
		private readonly NativeArray<DistanceConstraint> m_EmptyConstraint;

		private int m_Offset;
		private float3 m_Position;

		public Point(Attachment attachment, float3 position)
		{
			m_Particle = new NativeArray<Particle>(1, Allocator.Persistent);
			m_EmptyConstraint = new NativeArray<DistanceConstraint>(0, Allocator.Persistent);

			m_Particle[0] = attachment switch
			{
				Attachment.Dynamic => new Particle(position, mass: 1.0f),
				Attachment.Static => new Particle(position),
				_ => new Particle(position),
			};
			m_Position = position;
		}

		public Point(Attachment attachment, float3 position, float mass)
		{
			m_Particle = new NativeArray<Particle>(1, Allocator.Persistent);
			m_EmptyConstraint = new NativeArray<DistanceConstraint>(0, Allocator.Persistent);

			m_Particle[0] = attachment switch
			{
				Attachment.Dynamic => new Particle(position, mass),
				Attachment.Static => new Particle(position),
				_ => new Particle(position),
			};

			m_Position = position;
		}

		public NativeArray<Particle> Particles => m_Particle;

		public NativeArray<DistanceConstraint> DistanceConstraints => m_EmptyConstraint;

		public int Offset => m_Offset;

		public float3 Position => m_Position;

		public void Dispose()
		{
			if (m_Particle.IsCreated)
			{
				m_Particle.Dispose();
			}

			if (m_EmptyConstraint.IsCreated)
			{
				m_EmptyConstraint.Dispose();
			}
		}

		public void Notify(int startIndexOfParticles, int startIndexOfConstraint)
		{
			m_Offset = startIndexOfParticles;
		}

		public void UpdateWith(NativeList<Particle> particles, NativeList<DistanceConstraint> distanceConstraints)
		{
			m_Position = particles[m_Offset].Position;
		}

		public void SetPosition(Solver solver, float3 position)
		{
			var particles = solver.Particles;
			var particle = particles[m_Offset];

			particle.Position = position;

			particles[m_Offset] = particle;
		}

		public float3 GetPosition(Solver solver)
		{
			return solver.Particles[m_Offset].Position;
		}

		public float3 GetVelocity(Solver solver)
		{
			return solver.Particles[m_Offset].Velocity;
		}
	}
}
