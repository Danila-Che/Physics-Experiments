using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace CustomCollisionDetection
{
	public class CustomCollisionSolver : MonoBehaviour
	{
		private struct Intersection
		{
			public CustomBoxCollider Collider;
			public CustomBoxCollider OtherCollider;
		}

		private struct Contact
		{
			public float3 Point;
		}

		private CustomBoxCollider[] m_Colliders;

		private readonly List<Intersection> m_Intersections = new();
		private readonly List<Contact> m_Contacts = new();

		private void OnEnable()
		{
			m_Colliders = GetComponentsInChildren<CustomBoxCollider>();

			Array.ForEach(m_Colliders, collider => collider.Init());
		}

		private void FixedUpdate()
		{
			m_Intersections.Clear();
			m_Contacts.Clear();

			Array.ForEach(m_Colliders, collider => collider.OnBeginStep());

			InitNextStep();
			StepBroabPhase();
			StepNarrowPhase();
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if (m_Colliders == null) { return; }

			foreach (var collider in m_Colliders)
			{
				Gizmos.color = collider.IsIntersects switch
				{
					true => Color.green,
					false => Color.red,
				};

				Gizmos.DrawWireCube(collider.Bounds.center, collider.Bounds.size);

				Gizmos.color = collider.IsCollide switch
				{
					true => Color.green,
					false => Color.red,
				};

				var points = collider.WorldVertices;

				for (int i = 0; i < points.Length; i++)
				{
					Gizmos.DrawSphere(points[i], 0.05f);
				}
			}
		}

#endif

		private void InitNextStep()
		{
			for (int i = 0; i < m_Colliders.Length; i++)
			{
				m_Colliders[i].IsIntersects = false;
				m_Colliders[i].IsCollide = false;
			}
		}

		private void StepBroabPhase()
		{
			for (int i = 0; i < m_Colliders.Length - 1; i++)
			{
				for (int j = i + 1; j < m_Colliders.Length; j++)
				{
					if (m_Colliders[i].Bounds.Intersects(m_Colliders[j].Bounds))
					{
						m_Colliders[i].IsIntersects = true;
						m_Colliders[j].IsIntersects = true;

						m_Intersections.Add(new Intersection
						{
							Collider = m_Colliders[i],
							OtherCollider = m_Colliders[j],
						});
					}
				}
			}
		}

		private void StepNarrowPhase()
		{
			for (int i = 0; i < m_Intersections.Count; i++)
			{
				var collider = m_Intersections[i].Collider;
				var otherCollider = m_Intersections[i].OtherCollider;

				var isCollide = IsCollide(collider, otherCollider);

				collider.IsCollide |= isCollide;
				otherCollider.IsCollide |= isCollide;
			}
		}

		// GJK
		// https://winter.dev/articles/physics-engine
		// https://winter.dev/articles/gjk-algorithm
		// https://winter.dev/articles/epa-algorithm
		private bool IsCollide(CustomBoxCollider collider, CustomBoxCollider otherCollider)
		{
			var supportPoint = GetSupportPoint(collider, otherCollider, math.right());
			var simplex = Simplex.Default;
			var direction = -supportPoint;

			simplex.PushFront(supportPoint);

			while (true)
			{
				supportPoint = GetSupportPoint(collider, otherCollider, direction);

				if (math.dot(supportPoint, direction) <= 0.0f)
				{
					return false;
				}

				simplex.PushFront(supportPoint);

				if (HasNextSimplex(ref simplex, ref direction))
				{
					return true;
				}
			}
		}

		private float3 GetSupportPoint(CustomBoxCollider collider, CustomBoxCollider otherCollider, float3 direction)
		{
			var pointA = collider.FindFurthestPoint(direction);
			var pointB = otherCollider.FindFurthestPoint(-direction);

			return pointA - pointB;
		}

		private bool HasNextSimplex(ref Simplex simplex, ref float3 direction)
		{
			return simplex.Size switch
			{
				2 => CollisionUtilities.CheckLine(ref simplex, ref direction),
				3 => CollisionUtilities.CheckTriangle(ref simplex, ref direction),
				4 => CollisionUtilities.CheckTetrahedron(ref simplex, ref direction),
				_ => false,
			};
		}

		private static bool ContainsOrigin(int length, float3[] simplex, ref float3 direction)
		{
			// Если симплекс состоит из одной точки, то он не может содержать начало координат
			if (length == 1)
			{
				return false;
			}

			// Если симплекс состоит из двух точек (отрезок)
			if (length == 2)
			{
				float3 A = simplex[1];
				float3 B = simplex[0];
				float3 AB = B - A;
				float3 AO = -A;

				// Направление перпендикулярно отрезку AB в сторону начала координат
				direction = math.cross(math.cross(AB, AO), AB);
				return false;
			}

			// Если симплекс состоит из трех точек (треугольник)
			if (length == 3)
			{
				float3 A = simplex[2];
				float3 B = simplex[1];
				float3 C = simplex[0];
				float3 AB = B - A;
				float3 AC = C - A;
				float3 AO = -A;

				float3 ABC = math.cross(AB, AC);

				// Проверка, находится ли начало координат в одной из сторон треугольника
				if (math.dot(math.cross(ABC, AC), AO) > 0)
				{
					simplex[1] = simplex[0];
					simplex[0] = A;
					direction = math.cross(math.cross(AC, AO), AC);
				}
				else if (math.dot(math.cross(AB, ABC), AO) > 0)
				{
					simplex[0] = A;
					direction = math.cross(math.cross(AB, AO), AB);
				}
				else
				{
					direction = ABC;
				}
				return false;
			}

			// Если симплекс состоит из четырех точек (тетраэдр)
			if (length == 4)
			{
				float3 A = simplex[3];
				float3 B = simplex[2];
				float3 C = simplex[1];
				float3 D = simplex[0];
				float3 AB = B - A;
				float3 AC = C - A;
				float3 AD = D - A;
				float3 AO = -A;

				float3 ABC = math.cross(AB, AC);
				float3 ACD = math.cross(AC, AD);
				float3 ADB = math.cross(AD, AB);

				// Проверка, находится ли начало координат в одной из сторон тетраэдра
				if (math.dot(ABC, AO) > 0)
				{
					simplex[0] = simplex[1];
					simplex[1] = simplex[2];
					simplex[2] = simplex[3];
					//simplex = simplex.Take(3).ToArray();
					direction = ABC;
				}
				else if (math.dot(ACD, AO) > 0)
				{
					simplex[0] = simplex[1];
					simplex[1] = simplex[3];
					//simplex = simplex.Take(3).ToArray();
					direction = ACD;
				}
				else if (math.dot(ADB, AO) > 0)
				{
					simplex[1] = simplex[2];
					simplex[2] = simplex[3];
					//simplex = simplex.Take(3).ToArray();
					direction = ADB;
				}
				else
				{
					return true;
				}
			}

			return false;
		}
	}
}
