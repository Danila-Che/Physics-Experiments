using UnityEngine;

namespace FakeXPBDMonoBehaviour
{
	public abstract class BaseFakeController : MonoBehaviour
	{
		private FakeSolverController m_FakeSolverController;

		private void OnEnable()
		{
			m_FakeSolverController = GetComponentInParent<FakeSolverController>();

			InitWith(m_FakeSolverController);
		}

		private void OnDisable()
		{
			FinishWith(m_FakeSolverController);
		}

		protected abstract void InitWith(FakeSolverController fakeSolverController);

		protected abstract void FinishWith(FakeSolverController fakeSolverController);
	}
}
