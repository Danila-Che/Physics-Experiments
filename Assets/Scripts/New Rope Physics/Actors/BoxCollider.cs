using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace RopePhysics
{
	public class BoxCollider : IActor, IDisposable
	{
		private static readonly int[,] s_NeighborIndices = new int[,]
		{
			{0, 1}, {0, 2}, {0, 4},
			{1, 3}, {1, 5},
			{2, 3}, {2, 6},
			{3, 7},
			{4, 5}, {4, 6},
			{5, 7},
			{6, 7}
		};

		private readonly NativeArray<Particle> m_Particles;
		private readonly NativeArray<DistanceConstraint> m_Constraints;
		private NativeArray<Particle> m_ParticlesCopy;

		private readonly int m_StartIndexOfParticles;

		public BoxCollider(int startIndexOfParticles, float3 center, float3 size)
		{
			m_Particles = new NativeArray<Particle>(8, Allocator.Persistent);
			m_ParticlesCopy = new NativeArray<Particle>(m_Particles.Length, Allocator.Persistent);
			m_Constraints = new NativeArray<DistanceConstraint>(12 + 4, Allocator.Persistent);

			var minX = center.x - 0.5f * size.x;
			var maxX = center.x + 0.5f * size.x;
			var minY = center.y - 0.5f * size.y;
			var maxY = center.y + 0.5f * size.y;
			var minZ = center.z - 0.5f * size.z;
			var maxZ = center.z + 0.5f * size.z;

			m_Particles[0] = new Particle(new float3(minX, minY, minZ), mass: 1.0f);
			m_Particles[1] = new Particle(new float3(maxX, minY, minZ), mass: 1.0f);
			m_Particles[2] = new Particle(new float3(minX, maxY, minZ), mass: 1.0f);
			m_Particles[3] = new Particle(new float3(maxX, maxY, minZ), mass: 1.0f);
			m_Particles[4] = new Particle(new float3(minX, minY, maxZ), mass: 1.0f);
			m_Particles[5] = new Particle(new float3(maxX, minY, maxZ), mass: 1.0f);
			m_Particles[6] = new Particle(new float3(minX, maxY, maxZ), mass: 1.0f);
			m_Particles[7] = new Particle(new float3(maxX, maxY, maxZ), mass: 1.0f);

			m_ParticlesCopy.CopyFrom(m_Particles);

			m_StartIndexOfParticles = startIndexOfParticles;

			int index = 0;
			for (int i = 0; i < s_NeighborIndices.GetLength(0); i++)
			{
				int index0 = s_NeighborIndices[i, 0];
				int index1 = s_NeighborIndices[i, 1];
				var distance = math.distance(m_Particles[index0].Position, m_Particles[index1].Position);

				m_Constraints[index++] = new DistanceConstraint(distance, m_StartIndexOfParticles + index0, m_StartIndexOfParticles + index1);
			}

			for (int i = 0; i < 4; i++)
			{
				var distance = math.distance(m_Particles[i].Position, m_Particles[7 - i].Position);

				m_Constraints[index++] = new DistanceConstraint(distance, m_StartIndexOfParticles + i, m_StartIndexOfParticles + 7 - i);
			}
		}

		public BoxCollider(int startIndexOfParticles, List<float3> vertices, List<(int vertex0, int vertex1)> connection)
		{
			m_StartIndexOfParticles = startIndexOfParticles;

			m_Particles = new NativeArray<Particle>(vertices.Count, Allocator.Persistent);
			m_ParticlesCopy = new NativeArray<Particle>(m_Particles.Length, Allocator.Persistent);
			m_Constraints = new NativeArray<DistanceConstraint>(connection.Count, Allocator.Persistent);

			for (int i = 0; i < vertices.Count; i++)
			{
				m_Particles[i] = new Particle(vertices[i], mass: 1.0f);
			}

			m_ParticlesCopy.CopyFrom(m_Particles);

			for (int i = 0; i < connection.Count; i++)
			{
				int index0 = connection[i].vertex0;
				int index1 = connection[i].vertex1;

				var distance = math.distance(m_Particles[index0].Position, m_Particles[index1].Position);

				m_Constraints[i] = new DistanceConstraint(
					distance,
					m_StartIndexOfParticles + index0,
					m_StartIndexOfParticles + index1);
			}
		}

		public NativeArray<Particle> Particles => m_Particles;

		public NativeArray<DistanceConstraint> DistanceConstraints => m_Constraints;

		public NativeArray<Particle> ParticlesCopy => m_ParticlesCopy;

		public int StartIndexOfParticles => m_StartIndexOfParticles;

		public void Dispose()
		{
			if (m_Particles.IsCreated)
			{
				m_Particles.Dispose();
			}

			if (m_ParticlesCopy.IsCreated)
			{
				m_ParticlesCopy.Dispose();
			}

			if (m_Constraints.IsCreated)
			{
				m_Constraints.Dispose();
			}
		}

		public void Notify(int startIndexOfParticles, int startIndexOfConstraint) { }

		public void UpdateWith(NativeList<Particle> particles, NativeList<DistanceConstraint> distanceConstraints)
		{
			for (int i = 0; i < m_ParticlesCopy.Length; i++)
			{
				m_ParticlesCopy[i] = particles[m_StartIndexOfParticles + i];
			}
		}

		public float3 GetCenter()
		{
			//var center = float3.zero;

			//for (int i = 0; i < 8; i++)
			//{
			//	center += m_ParticlesCopy[i].Position;
			//}

			//center /= m_ParticlesCopy.Length;

			//return center;

			return m_ParticlesCopy[8].Position;
		}

		public quaternion GetRotation(float3 center)
		{
			var upPoint =
				  m_ParticlesCopy[2].Position
				+ m_ParticlesCopy[3].Position
				+ m_ParticlesCopy[4].Position
				+ m_ParticlesCopy[5].Position;
			upPoint /= 4;
			var forwardPoint =
				  m_ParticlesCopy[0].Position
				+ m_ParticlesCopy[1].Position
				+ m_ParticlesCopy[2].Position
				+ m_ParticlesCopy[3].Position;
			forwardPoint /= 4;

			var upVector = upPoint - center;
			var forwardVector = forwardPoint - center;

			return quaternion.LookRotationSafe(forwardVector, upVector);
		}
	}
}
