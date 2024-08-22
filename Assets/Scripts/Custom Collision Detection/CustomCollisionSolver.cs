using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace CustomCollisionDetection
{
	internal class CustomCollisionSolver : MonoBehaviour
	{
		private struct Intersection
		{
			public CustomBoxCollider Collider;
			public CustomBoxCollider OtherCollider;
		}

		private struct RawContact
		{
			public float3 Point;
			public float angle;
		}

		private CustomBoxCollider[] m_Colliders;

		private readonly List<Intersection> m_Intersections = new();
		private readonly List<CollisionPoints> m_Contacts = new();

		private readonly CollisionSystem m_CollisionSystem = new();

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

			Gizmos.color = Color.blue;

			foreach (var contact in m_Contacts)
			{
				foreach (var point in contact.Points)
				{
					Gizmos.DrawSphere(point, 0.1f);
					Gizmos.DrawRay(point, contact.Normal);
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

				if (TryGetCollisionPoints(collider, otherCollider, out var collisionPoints))
				{
					m_Contacts.Add(collisionPoints);

					collider.IsCollide = true;
					otherCollider.IsCollide = true;
				}
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

		private bool TryGetCollisionPoints(CustomBoxCollider collider, CustomBoxCollider otherCollider, out CollisionPoints collisionPoints)
		{
			if (collider.TryCalculatePenetration(otherCollider, out collisionPoints))
			{
				m_CollisionSystem.CalculateContactPoints(collider, otherCollider, ref collisionPoints);

				return true;
			}

			return false;
		}

		private void CalculateContactPoints(CustomBoxCollider collider, CustomBoxCollider otherCollider, ref CollisionPoints collisionPoints)
		{
			var contactPlane = new Plane(collisionPoints.Normal, collider.FindFurthestPoint(collisionPoints.Normal));

			// World point and angle
			var contactPointsA = new List<(float3, float)>(capacity: 4);
			var contactPointsB = new List<(float3, float)>(capacity: 4);

			FindContactPoints(collider, contactPlane, contactPointsA);
			FindContactPoints(otherCollider, contactPlane, contactPointsB);

			if (contactPointsA.Count == 0)
			{
				for (int i = 0; i < contactPointsB.Count; i++)
				{
					collisionPoints.Points.Add(contactPointsB[i].Item1);
				}

				return;
			}

			if (contactPointsB.Count == 0)
			{
				for (int i = 0; i < contactPointsA.Count; i++)
				{
					collisionPoints.Points.Add(contactPointsA[i].Item1);
				}

				return;
			}

			//Vertex to face contact
			if (contactPointsA.Count == 1)
			{
				collisionPoints.Points.Add(contactPointsA[0].Item1);
				return;
			}
			else if (contactPointsB.Count == 1)
			{
				collisionPoints.Points.Add(contactPointsB[0].Item1);
				return;
			}

			CalculateAngles(contactPointsA, collisionPoints.Normal);
			CalculateAngles(contactPointsB, collisionPoints.Normal);

			contactPointsA.Sort(CompareAngle);
			contactPointsB.Sort(CompareAngle);

			var realContactPoints = new List<float3>();
			CheckIntersection(contactPointsA, contactPointsB, collisionPoints.Normal, realContactPoints);

			if (realContactPoints.Count == 0)
			{
				CheckIntersection(contactPointsB, contactPointsA, collisionPoints.Normal, realContactPoints);
			}

			collisionPoints.Points.AddRange(realContactPoints);
		}

		private void FindContactPoints(CustomBoxCollider collider, Plane plane, List<(float3, float)> contactBuffer)
		{
			for (int i = 0; i < collider.WorldVertices.Length; i++)
			{
				var worldPoint = collider.WorldVertices[i];
				var distance = plane.GetDistanceToPoint(worldPoint);

				if (distance > 0.005f) { continue; }

				contactBuffer.Add((worldPoint, 0.0f));
			}
		}

		private void CalculateAngles(List<(float3, float)> contactBuffer, float3 normal)
		{
			var origin = FindOrigin(contactBuffer);
			var refVector = contactBuffer[0].Item1 - origin;

			for (int i = 0; i < contactBuffer.Count; i++)
			{
				var originToPoint = contactBuffer[i].Item1 - origin;
				var u = math.dot(normal, math.cross(refVector, originToPoint));
				var angle = CollisionUtilities.AngleInDegrees(refVector, originToPoint);

				var contact = contactBuffer[i];

				if (u <= 0.001f)
				{
					contact.Item2 = angle;
				}
				else
				{
					contact.Item2 = angle + 180.0f;
				}

				contactBuffer[i] = contact;
			}
		}

		private float3 FindOrigin(List<(float3, float)> contactBuffer)
		{
			var origin = float3.zero;

			for (int i = 0; i < contactBuffer.Count; i++)
			{
				origin += contactBuffer[i].Item1;
			}

			origin /= contactBuffer.Count;
			return origin;
		}

		private int CompareAngle((float3, float) contactA, (float3, float) contactB)
		{
			if (contactA.Item2 < contactB.Item2)
			{
				return -1;
			}

			return 1;
		}

		private void CheckIntersection(
			List<(float3, float)> contactPointsA,
			List<(float3, float)> contactPointsB,
			float3 normal,
			List<float3> contactPointsBuffer)
		{
			for (int it = 0; it < contactPointsB.Count; it++)
			{
				var itSecondP = it + 1;
				if (itSecondP == contactPointsB.Count)
				{
					itSecondP = 0;
				}

				//We assume that the first point is inside
				var passedCount = 1;

				//Check if first point inside shape
				for (int jt = 0; jt < contactPointsA.Count; jt++)
				{
					var jtSecondP = jt + 1;
					if (jtSecondP == contactPointsA.Count)
					{
						jtSecondP = 0;
					}

					var vct = contactPointsB[it].Item1 - contactPointsA[jt].Item1;
					var edgeVector = contactPointsA[jtSecondP].Item1 - contactPointsA[jt].Item1;

					var norm = math.cross(math.normalize(edgeVector), normal);

					if (math.dot(norm, vct) < 0)
					{
						passedCount--;
						break;
					}
				}
				if (passedCount == 1)
				{
					contactPointsBuffer.Add(contactPointsB[it].Item1);
				}


				//Check if second point inside shape
				//We assume that the second point is inside
				passedCount++;
				for (int jt = 0; jt < contactPointsA.Count; jt++)
				{
					int jtSecondP = jt + 1;
					if (jtSecondP == contactPointsA.Count)
					{
						jtSecondP = 0;
					}

					var vct = contactPointsB[itSecondP].Item1 - contactPointsA[jt].Item1;
					var edgeVector = contactPointsA[jtSecondP].Item1 - contactPointsA[jt].Item1;

					var norm = math.normalize(math.cross(math.normalize(edgeVector), normal));

					if (math.dot(norm, vct) < 0)
					{
						passedCount--;
						break;
					}
				}

				//all points of current line is inside shape
				if (passedCount == 2)
				{
					continue;
				}

				////all points of current line is outside shape
				//if (passedCount == 0 && !(contactPointsA.size() == 2 && contactPointsB.size() == 2))
				//    continue;

				//Check if line separate shape
				for (int jt = 0; jt < contactPointsA.Count; jt++)
				{
					int jtSecondP = jt + 1;
					if (jtSecondP == contactPointsA.Count)
					{
						jtSecondP = 0;
					}

					bool notParallel = CollisionUtilities.ClosetPointBetweenAxis(
						(contactPointsB[it].Item1, contactPointsB[itSecondP].Item1),
						(contactPointsA[jt].Item1, contactPointsA[jtSecondP].Item1),
						out float3 projectPoint);

					if (!notParallel)
					{
						continue;
					}

					float vct1 = math.lengthsq(contactPointsB[it].Item1 - projectPoint);
					float vct2 = math.lengthsq(contactPointsB[itSecondP].Item1 - projectPoint);

					float lengthAxisB = math.lengthsq(contactPointsB[itSecondP].Item1 - contactPointsB[it].Item1);
					float lengthAxisA = math.lengthsq(contactPointsA[jtSecondP].Item1 - contactPointsA[jt].Item1);

					if (vct1 > lengthAxisA || vct2 > lengthAxisA)
					{
						continue;
					}

					if (vct1 > lengthAxisB || vct2 > lengthAxisB)
					{
						continue;
					}

					contactPointsBuffer.Add(projectPoint);
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
	}
}
