using UnityEngine;
using XPBD;

namespace FakeXPBDMonoBehaviour
{
	[SelectionBase]
	[RequireComponent(typeof(Rigidbody))]
	public class FakeBodyController : MonoBehaviour
	{
		private int m_BodyInstanceId;
		private FakeSolverController m_FakeSolverController;

		private FakeBody m_Body;

		public int BodyInstanceId => m_BodyInstanceId;

		private void OnEnable()
		{
			m_BodyInstanceId = GetComponent<Rigidbody>().GetInstanceID();
			m_FakeSolverController = GetComponentInParent<FakeSolverController>();

			var colliderController = GetComponent<FakeColliderController>();

			m_Body = new FakeBody(new FakePose(transform), colliderController.Collider);

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

		private void Update()
		{
			var pose = m_Body.Pose;
			
			transform.SetPositionAndRotation(pose.Position, pose.Rotation);
        }
	}
}
