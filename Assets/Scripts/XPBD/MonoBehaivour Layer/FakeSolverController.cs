using UnityEngine;

namespace FakeXPBDMonoBehaviour
{
	[DefaultExecutionOrder(-999)]
    public class FakeSolverController : MonoBehaviour
    {
		private FakeCollisionSystemController m_FakeCollisionSystemController;

		private void OnEnable()
		{
			m_FakeCollisionSystemController = new FakeCollisionSystemController();
		}

		private void OnDisable()
		{
			m_FakeCollisionSystemController?.Dispose();
			m_FakeCollisionSystemController = null;
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;

			m_FakeCollisionSystemController?.Contacts.ForEach(contact => Gizmos.DrawSphere(contact, 0.1f));
		}

#endif

		public void RegisterModifier(int bodyId)
		{
			m_FakeCollisionSystemController?.RegisterModifier(bodyId);
		}

		public void UnregisterModifier(int bodyId)
		{
			m_FakeCollisionSystemController?.UnregisterModifier(bodyId);
		}
	}
}
