using Unity.Mathematics;
using UnityEngine;
using XPBD;

namespace FakeXPBDMonoBehaviour
{
	[DefaultExecutionOrder((int)ExecutionOrder.Joint)]
	public sealed class FakeJointController : BaseFakeController
	{
		[SerializeField] private FakeBodyController m_AnchorBody;
		[SerializeField] private FakeBodyController m_TargetBody;
		[SerializeField] private FakeJointParams m_Params;

		private FakeJoint m_Joint;

		public FakeBodyController TargetBody
		{
			get => m_TargetBody;
			set => m_TargetBody = value;
		}

		public float3 AnchorGlobalPosition => m_Joint.GlobalPose0.Position;
		public float3 TargetGlobalPosition => m_Joint.GlobalPose1.Position;

		public FakeJointParams Parameters => m_Params;

		protected override void InitWith(FakeSolverController fakeSolverController)
		{
			var body0 = m_AnchorBody == null ? null : m_AnchorBody.Body;
			var body1 = m_TargetBody == null ? null : m_TargetBody.Body;
			m_Joint = new FakeJoint(body0, body1, m_Params);

			fakeSolverController.RegisterJoint(m_Joint);
		}

		protected override void FinishWith(FakeSolverController fakeSolverController)
		{
			fakeSolverController.UnregisterJoint(m_Joint);
		}
	}
}
