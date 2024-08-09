using UnityEngine;

namespace FakeXPBDMonoBehaviour
{
	[SelectionBase]
	[RequireComponent(typeof(Rigidbody))]
	public class FakeBodyController : MonoBehaviour
	{
		private int m_BodyInstanceId;
		private FakeSolverController m_FakeSolverController;

		public int BodyInstanceId => m_BodyInstanceId;

		private void OnEnable()
		{
			m_BodyInstanceId = GetComponent<Rigidbody>().GetInstanceID();

			m_FakeSolverController = GetComponentInParent<FakeSolverController>();

			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.RegisterModifier(m_BodyInstanceId);
			}
		}

		private void OnDestroy()
		{
			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.UnregisterModifier(m_BodyInstanceId);
			}
		}
	}
}
