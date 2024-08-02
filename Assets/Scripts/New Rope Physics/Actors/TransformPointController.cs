using System;
using UnityEngine;

namespace RopePhysics
{
	public class TransformPointController : MonoBehaviour, IActorController
	{
		[SerializeField] private Attachment m_Attachment;

		private Point m_Point;

		public void InitWithSolver(Solver solver)
		{
			m_Point = new Point(m_Attachment, transform.position);

			solver.Register(m_Point);
		}

		private void Update()
		{
			transform.position = m_Point.Position;
		}

		public void Dispose()
		{
			((IDisposable)m_Point).Dispose();
		}

		public void ActualiaseToSolver(Solver solver)
		{

		}

		public void ActualiaseFromSolver(Solver solver)
		{

		}
	}
}
