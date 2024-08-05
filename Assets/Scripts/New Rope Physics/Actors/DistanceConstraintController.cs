using UnityEngine;

namespace RopePhysics
{
	public class DistanceConstraintController : MonoBehaviour, ISecondaryActorController
	{
		[SerializeField] private RigidbodyPointController m_FirstPoint;
		[SerializeField] private RigidbodyPointController m_SecondPoint;

		private Constraint m_Constraint;

		public void InitWithSolver(Solver solver)
		{
			var distance = Vector3.Distance(
				m_FirstPoint.transform.position,
				m_SecondPoint.transform.position);

			m_Constraint = new Constraint(distance, m_FirstPoint.Id, m_SecondPoint.Id);
			solver.Register(m_Constraint);
		}

		public void ActualiaseFromSolver(Solver solver) { }

		public void ActualiaseToSolver(Solver solver) { }

		public void Dispose()
		{
			m_Constraint?.Dispose();
		}
	}
}
