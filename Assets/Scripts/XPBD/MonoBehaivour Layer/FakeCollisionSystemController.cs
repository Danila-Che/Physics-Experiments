using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace FakeXPBDMonoBehaviour
{
	public class FakeCollisionSystemController : IDisposable
	{
		private readonly HashSet<int> m_Bodies;
		private readonly List<Vector3> m_Contacts = new();

		private bool m_IsDisposed;

		public FakeCollisionSystemController()
		{
			m_Bodies = new HashSet<int>();
			m_IsDisposed = false;

			Physics.ContactModifyEvent += PhysicsOnContactModifyEvent;
		}

		public List<Vector3> Contacts => m_Contacts;

		public void Dispose()
		{
			m_Bodies.Clear();

			Physics.ContactModifyEvent -= PhysicsOnContactModifyEvent;

			m_IsDisposed = true;
		}

		public void RegisterModifier(int bodyId)
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			m_Bodies.Add(bodyId);
		}

		public void UnregisterModifier(int bodyId)
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			m_Bodies.Remove(bodyId);
		}

		private void PhysicsOnContactModifyEvent(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
		{
			m_Contacts.Clear();

			for (int i = 0; i < pairs.Length; i++)
			{
				var pair = pairs[i];

				if (m_Bodies.TryGetValue(pair.bodyInstanceID, out _))
				{
					IgnoreContact(pair.bodyInstanceID, pair.otherBodyInstanceID, pair);
				}
				else if (m_Bodies.TryGetValue(pair.otherBodyInstanceID, out _))
				{
					IgnoreContact(pair.otherBodyInstanceID, pair.bodyInstanceID, pair);
				}
			}
		}

		private void IgnoreContact(int body0, int body1, in ModifiableContactPair pair)
		{
			for (int i = 0; i < pair.contactCount; i++)
			{
				//pair.IgnoreContact(i);
				m_Contacts.Add(pair.GetPoint(i));
			}
		}
	}
}
