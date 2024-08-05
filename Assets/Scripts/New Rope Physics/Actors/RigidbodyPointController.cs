using System;
using UnityEngine;

namespace RopePhysics
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(Collider))]
    public class RigidbodyPointController : MonoBehaviour, IPrimaryActorController
	{
		[SerializeField] private Attachment m_Attachment;
		[Min(0.0f)]
		[SerializeField] private float m_Mass;

		private Point m_Point;
		private Rigidbody m_Rigidbody;

		public int Id => m_Point.Offset;

		public void InitWithSolver(Solver solver)
		{
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Point = new Point(m_Attachment, m_Rigidbody.position, m_Mass);

			solver.Register(m_Point);
		}

		private void Update()
		{
			m_Rigidbody.position = m_Point.Position;
		}

		public void Dispose()
		{
			m_Point?.Dispose();
		}

		public void ActualiaseFromSolver(Solver solver)
		{
			m_Rigidbody.position = m_Point.GetPosition(solver);
			m_Rigidbody.velocity = Vector3.zero;
		}

		public void ActualiaseToSolver(Solver solver)
		{
			m_Point.SetPosition(solver, m_Rigidbody.position);
			m_Rigidbody.velocity = Vector3.zero;
		}
	}
}
