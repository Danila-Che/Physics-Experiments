using UnityEngine;

namespace RopePhysics
{
	public class RopeController : MonoBehaviour, ISecondaryActorController
	{
		[Min(0.0f)]
		[SerializeField] private float m_SpanDistance = 1.0f;
		[SerializeField] private RigidbodyPointController m_TopPoint;
		[SerializeField] private RigidbodyPointController m_BottomPoint;

		private Rope m_Rope;
		private IRenderer m_Renderer;

		public Rope Rope => m_Rope;

		public void InitWithSolver(Solver solver)
		{
			m_Renderer = GetComponent<IRenderer>();
			m_Renderer?.Init();

			m_Rope = new Rope(m_SpanDistance);

			m_Rope.CreateRope(solver, m_BottomPoint.Id, m_TopPoint.Id);
			solver.Register((IActor)m_Rope);
		}

		public void Dispose()
		{
			m_Rope?.Dispose();
		}

		private void Update()
		{
			m_Renderer?.Draw(m_Rope.ParitclesCopy.AsReadOnly());
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if (m_Rope == null) { return; }

			Gizmos.color = Color.green;

			for (int i = 0; i < m_Rope.ParitclesCopy.Length; i++)
			{
				Gizmos.DrawSphere(m_Rope.ParitclesCopy[i].Position, 0.2f);
			}
		}

#endif

		public void ActualiaseToSolver(Solver solver)
		{
		}

		public void ActualiaseFromSolver(Solver solver)
		{
			m_Rope.UpdateWith(solver);
		}
	}
}
