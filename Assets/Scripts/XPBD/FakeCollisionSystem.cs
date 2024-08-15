using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace XPBD
{
	public class FakeCollisionSystem : IDisposable
	{
		private readonly List<FakeContactPair> m_Contacts = new();

		private bool m_IsDisposed;

		public FakeCollisionSystem()
		{
			m_Contacts = new List<FakeContactPair>();

			m_IsDisposed = false;

			Physics.ContactEvent += Physics_ContactEvent;
		}

		public IReadOnlyList<FakeContactPair> Contacts => m_Contacts;

		public void Dispose()
		{
			m_Contacts.Clear();
			m_IsDisposed = true;

			Physics.ContactEvent -= Physics_ContactEvent;
		}

		public void ClearContacts()
		{
			m_Contacts.Clear();
		}

		public void SolveCollision(IReadOnlyList<FakeBody> bodies, float deltaTime)
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			Debug.Log($"> {bodies.Count} {deltaTime}");

			for (int i = 0; i < bodies.Count - 1; i++)
			{
				var body0 = bodies[i];

				//Debug.Log($"> {body0.IsKinematic}");

				//if (body0.IsKinematic)
				//{
				//	continue;
				//}

				for (int j = i + 1; j < bodies.Count; j++)
				{
					var body1 = bodies[j];

					if (body0.IsKinematic && body1.IsKinematic)
					{
						continue;
					}

					if (CheckContact(body0, body1, out var contact))
					{
						AddContact(body0, body1, in contact);
						ResolveContact(body0, body1, in contact, deltaTime);

						Debug.DrawRay(contact.Point, contact.Normal, Color.red);
						Debug.Log($"> {contact.Point}");
					}
				}
			}
		}

		private void Physics_ContactEvent(PhysicsScene scene, NativeArray<ContactPairHeader>.ReadOnly pairHeaders)
		{
			//Debug.Log($"> {pairHeaders.Length}");

			//for (int i = 0; i < pairHeaders.Length; i++)
			//{
			//	var pairHeader = pairHeaders[i];

			//	for (int j = 0; j < pairHeader.pairCount; j++)
			//	{
			//		var pair = pairHeader.GetContactPair(j);

			//		for (int k = 0; k < pair.contactCount; k++)
			//		{
			//			var contact = pair.GetContactPoint(k);

			//			Debug.DrawRay(contact.position, contact.normal, Color.cyan);
			//			Debug.Log($"> P {contact.position} N {contact.normal} D {contact.separation}");
			//		}
			//	}
			//}
		}

		private bool CheckContact(FakeBody body0, FakeBody body1, out FakeContact contact)
		{
			return body0.Collider.Intersects(body0.Pose, body1.Collider, body1.Pose, out contact);
		}

		private void ResolveContact(FakeBody body0, FakeBody body1, in FakeContact contact, float deltaTime)
		{
			FakeBody.ApplyBodyPairCorrection(
				body0.IsKinematic ? null : body0,
				body1.IsKinematic ? null : body1,
				contact.Normal * contact.Separation,
				0.0f,
				deltaTime,
				contact.Point);
		}

		private void AddContact(FakeBody body0, FakeBody body1, in FakeContact contact)
		{
			body0 = body0.IsKinematic ? null : body0;
			body1 = body1.IsKinematic ? null : body1;

			AddContact(body0, body1, contact.Point, contact.Normal, contact.Separation);
		}

		private void AddContact(FakeBody body0, FakeBody body1, float3 point, float3 normal, float depth)
		{
			var pointVelocity0 = body0?.GetVelocityAt(point) ?? float3.zero;
			var pointVelocity1 = body1?.GetVelocityAt(point) ?? float3.zero;
			var projection0 = math.dot(pointVelocity0, normal);
			var projection1 = math.dot(pointVelocity1, normal);

			// simplified (pointVel0 - proj0 * normal) - (pointVel1 - proj1 * normal)
			// delta of velocities' parts tangential to normal
			var deltaV = pointVelocity0 - pointVelocity1 - (projection0 - projection1) * normal;
			var deltaVLength = math.length(deltaV);
			var deltaVDirection = float3.zero;

			if (deltaVLength != 0.0f)
			{
				deltaVDirection = deltaV / deltaVLength;
			}

			var friction = -100.0f;
			m_Contacts.Add(new FakeContactPair(
				body0,
				body1,
				point,
				normal,
				math.max(0.0f, depth),
				deltaVDirection,
				deltaVLength,
				friction));
		}
	}
}
