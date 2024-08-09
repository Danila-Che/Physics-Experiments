using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace RopePhysics
{
    public class SolfBoxColliderController : MonoBehaviour, IPrimaryActorController
	{
		[SerializeField] private Vector3 m_Size;
		[SerializeField] private List<float3> m_LocalAttachments;
 
		private readonly List<float3> m_Vertices = new();
		private readonly List<(int Vertex0, int Vertex1)> m_Connections = new();

		private BoxCollider m_BoxCollider;

		public void InitWithSolver(Solver solver)
		{
			Debug.Log($"> Yes");

			m_Vertices.Clear();
			GenerateVertices();
			m_Connections.Clear();
			Triangulate();
			AddAttachments();

			m_BoxCollider = new BoxCollider(solver.NextParticleId, m_Vertices, m_Connections);
			solver.Register(m_BoxCollider);
		}

		private void Update()
		{
			var center = m_BoxCollider.GetCenter();
			var rotation = m_BoxCollider.GetRotation(center);

			transform.SetPositionAndRotation(center, rotation);
		}

		public void ActualiaseToSolver(Solver solver) { }

		public void ActualiaseFromSolver(Solver solver)
		{
			m_BoxCollider.UpdateWith(solver.Particles, solver.Constraints);
		}

		public int GetParticleIndex(int innerIndex)
		{
			return m_BoxCollider.StartIndexOfParticles + innerIndex;
		}

		public void Dispose()
		{
			m_BoxCollider?.Dispose();
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if (m_BoxCollider == null)
			{
				Gizmos.color = Color.green;

				m_Vertices.Clear();
				GenerateVertices();
				m_Connections.Clear();
				Triangulate();
				AddAttachments();

				m_Vertices.ForEach(vertex => Gizmos.DrawCube(vertex, 0.1f * Vector3.one));

				m_Connections.ForEach(connection =>
				{
					var point0 = m_Vertices[connection.Vertex0];
					var point1 = m_Vertices[connection.Vertex1];

					Gizmos.DrawLine(point0, point1);
				});

				//Gizmos.color = Color.blue;

				//m_LocalAttachments.ForEach(attachment => Gizmos.DrawCube(attachment, 0.1f * Vector3.one));
			}
			else
			{
				Gizmos.color = Color.green;

				for (int i = 0; i < m_BoxCollider.ParticlesCopy.Length; i++)
				{
					Gizmos.DrawCube(m_BoxCollider.ParticlesCopy[i].Position, 0.1f * Vector3.one);
				}

				for (int i = 0; i < m_BoxCollider.DistanceConstraints.Length; i++)
				{
					var constraint = m_BoxCollider.DistanceConstraints[i];

					Gizmos.DrawLine(
						m_BoxCollider.ParticlesCopy[constraint.Index0 - m_BoxCollider.StartIndexOfParticles].Position,
						m_BoxCollider.ParticlesCopy[constraint.Index1 - m_BoxCollider.StartIndexOfParticles].Position);
				}
			}
		}

#endif

		private void GenerateVertices()
		{
			m_Vertices.Capacity = 8;

			var center = transform.position;
			var size = m_Size;

			var minX = center.x - 0.5f * size.x;
			var maxX = center.x + 0.5f * size.x;
			var minY = center.y - 0.5f * size.y;
			var maxY = center.y + 0.5f * size.y;
			var minZ = center.z - 0.5f * size.z;
			var maxZ = center.z + 0.5f * size.z;

			//https://en.wikipedia.org/wiki/Gray_code
			m_Vertices.Add(new float3(minX, minY, minZ)); //000
			m_Vertices.Add(new float3(maxX, minY, minZ)); //100
			m_Vertices.Add(new float3(maxX, maxY, minZ)); //110
			m_Vertices.Add(new float3(minX, maxY, minZ)); //010
			m_Vertices.Add(new float3(minX, maxY, maxZ)); //011
			m_Vertices.Add(new float3(maxX, maxY, maxZ)); //111
			m_Vertices.Add(new float3(maxX, minY, maxZ)); //101
			m_Vertices.Add(new float3(minX, minY, maxZ)); //001
		}

		private void Triangulate()
		{
			Assert.IsTrue(m_Vertices.Count == 8);

			var indexes = m_Vertices
				.Select((vetex, i) => i)
				.ToArray();

			for (int i = 0; i < indexes.Length; i++)
			{
				var one = i;
				var two = (i + 1) % indexes.Length;
				var three = (i + 5) % indexes.Length;
				//var two = (i + 1) % indexes.Length;
				//var three = (i + 2) % indexes.Length;
				//var four = (i + 3) % indexes.Length;

				m_Connections.Add((indexes[one], indexes[two]));
				//m_Connections.Add((indexes[one], indexes[three]));
				//m_Connections.Add((indexes[one], indexes[four]));
			}

			for (int i = 0, j = indexes.Length - 1; i < j; i++, j--)
			{
				m_Connections.Add((indexes[i], indexes[j]));
			}

			for (int i = 0, j = 1; j < indexes.Length; i += 2, j += 2)
			{
				m_Connections.Add((indexes[i], indexes[j]));
			}

			m_Connections.Add((indexes[0], indexes[3]));
			m_Connections.Add((indexes[1], indexes[2]));
			m_Connections.Add((indexes[4], indexes[7]));
			m_Connections.Add((indexes[5], indexes[6]));

			m_Vertices.Add(transform.position);

			for (int i = 0; i < indexes.Length; i++)
			{
				m_Connections.Add((indexes[i], m_Vertices.Count - 1));
			}

			Debug.Log($"> V: {m_Vertices.Count} C: {m_Connections.Count}");
		}

		private void AddAttachments()
		{
			var start = m_Vertices.Count;

			m_Vertices.AddRange(m_LocalAttachments.Select(attachment => (float3)transform.position + attachment));

			for (int i = 0; i < m_LocalAttachments.Count; i++)
			{
				Debug.Log($"> {m_LocalAttachments[i]} {start + i}");

				m_Connections.Add((start + i, start + (i + 1) % m_LocalAttachments.Count));
				m_Connections.Add((start + i, GetClosestIndex(m_LocalAttachments[i])));
				m_Connections.Add((start + i, 8));
			}
		}

		private int GetClosestIndex(float3 point)
		{
			var index = 0;
			var distance = math.distancesq(point, m_Vertices[0]);

			for (int i = 1; i < 8; i++)
			{
				var tempDistance = math.distancesq(point, m_Vertices[i]);

				if (tempDistance < distance)
				{
					index = i;
					distance = tempDistance;
				}
			}

			return index;
		}

		private void TriangulateSide(Vector3 direction)
		{
			var point = Vector3.Dot(0.5f * m_Size, direction);
			var indexes = m_Vertices
				.Where(vertex => Vector3.Dot(vertex, direction) == point)
				.Select((vetex, i) => i)
				.ToArray();

			for (int i = 0; i < indexes.Length - 1; i++)
			{
				m_Connections.Add((indexes[i], indexes[i + 1]));
			}

			Debug.Log($"> {point} {indexes.Length}");
		}
	}
}
