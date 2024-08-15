using UnityEngine;
using XPBD;

namespace FakeXPBDMonoBehaviour
{
	[SelectionBase]
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(FakeColliderController))]
	[DefaultExecutionOrder((int)ExecutionOrder.Body)]
	public class FakeBodyController : MonoBehaviour
	{
		private int m_BodyInstanceId;
		private Rigidbody m_Rigidbody;
		private FakeSolverController m_FakeSolverController;

		private FakeBody m_Body;

		public FakeBody Body => m_Body;

		private void OnEnable()
		{
			m_Rigidbody = GetComponent<Rigidbody>();
			m_BodyInstanceId = m_Rigidbody.GetInstanceID();
			m_FakeSolverController = GetComponentInParent<FakeSolverController>();

			var colliderController = GetComponent<FakeColliderController>();
			colliderController.Initialize();

			m_Body = new FakeBody(m_Rigidbody, colliderController.Collider);

			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.RegisterBody(m_Body);
				m_FakeSolverController.RegisterModifier(m_BodyInstanceId, m_Body);
			}

			m_Rigidbody.detectCollisions = true;
		}

		private void OnDestroy()
		{
			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.UnregisterBody(m_Body);
				m_FakeSolverController.UnregisterModifier(m_BodyInstanceId);
			}
		}

		private void Update()
		{
			if (m_Body.IsKinematic)
			{
				m_Body.UpdateWith(m_Rigidbody);
			}
			else
			{
				var pose = m_Body.Pose;

				m_Rigidbody.position = pose.Position;
				m_Rigidbody.rotation = pose.Rotation;
			}
		}
	}
}
