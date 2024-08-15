using Unity.Mathematics;
using UnityEngine;
using XPBD;
using XPBD.SoftBody;
using XPBD.SoftBody.Render;

namespace FakeXPBDMonoBehaviour
{
	[RequireComponent(typeof(IRenderer))]
	[DefaultExecutionOrder((int)ExecutionOrder.Joint)]
	public class FakeRopeController : BaseFakeController
	{
		[SerializeField] private FakeBodyController m_AnchorBody;
		[SerializeField] private float3 m_AnchorLocalAttachement;
		[SerializeField] private FakeBodyController m_TargetBody;
		[SerializeField] private float3 m_TargetLocalAttachement;
		[Min(0.01f)]
		[SerializeField] private float m_SpanDistance = 1.0f;
		[Min(0.0f)]
		[SerializeField] private float m_Drag = 0.0f;

		private FakeRope m_Rope;
		private IRenderer m_Renderer;

		protected override void InitWith(FakeSolverController fakeSolverController)
		{
			var fakeJointContainer = new FakeJointContainer(
				m_AnchorBody.Body,
				m_TargetBody.Body,
				new FakePose(m_AnchorLocalAttachement, quaternion.identity),
				new FakePose(m_TargetLocalAttachement, quaternion.identity));

			m_Rope = new FakeRope(fakeJointContainer, m_SpanDistance, m_Drag);
			m_Rope.CreateFromJoint();

			m_Renderer = GetComponent<IRenderer>();
			m_Renderer.Init();

			fakeSolverController.RegisterActor(m_Rope);
			fakeSolverController.RegisterConstrainable(m_Rope);
		}

		protected override void FinishWith(FakeSolverController fakeSolverController)
		{
			fakeSolverController.UnregisterActor(m_Rope);
			fakeSolverController.UnregisterConstrainable(m_Rope);

			m_Rope.Dispose();
			m_Rope = null;
		}

		private void Update()
		{
			m_Renderer.Draw(m_Rope.Particles);
		}

#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			if (m_AnchorBody == null || m_TargetBody == null)
			{
				return;
			}

			Gizmos.color = Color.green;

			var fakeJointContainer = new FakeJointContainer(
				new FakeBody(m_AnchorBody.transform),
				new FakeBody(m_TargetBody.transform),
				new FakePose(m_AnchorLocalAttachement, quaternion.identity),
				new FakePose(m_TargetLocalAttachement, quaternion.identity));

			fakeJointContainer.RecalculateGlobalPoses();

			Gizmos.DrawSphere(fakeJointContainer.AnchorGlobalPose.Position, 0.1f);
			Gizmos.DrawSphere(fakeJointContainer.TargetGlobalPose.Position, 0.1f);
		}

#endif
	}
}
