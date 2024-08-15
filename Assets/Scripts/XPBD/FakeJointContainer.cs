namespace XPBD
{
	public class FakeJointContainer
	{
		private readonly FakeBody m_AnchorBody;
		private readonly FakeBody m_TargetBody;
		private readonly FakePose m_AnchorLocalPose;
		private readonly FakePose m_TargetLocalPose;
		private FakePose m_AnchorGlobalPose;
		private FakePose m_TargetGlobalPose;

		public FakeJointContainer(
			FakeBody anchorBody,
			FakeBody targetBody,
			FakePose anchorLocalPose,
			FakePose targetLocalPose)
		{
			m_AnchorBody = anchorBody;
			m_TargetBody = targetBody;
			m_AnchorLocalPose = anchorLocalPose;
			m_TargetLocalPose = targetLocalPose;

			m_AnchorGlobalPose = m_AnchorLocalPose;
			m_TargetGlobalPose = m_TargetLocalPose;
		}

		public FakeBody AnchorBody => m_AnchorBody;
		public FakeBody TargetBody => m_TargetBody;

		public FakePose AnchorGlobalPose => m_AnchorGlobalPose;
		public FakePose TargetGlobalPose => m_TargetGlobalPose;

		public void RecalculateGlobalPoses()
		{
			m_AnchorGlobalPose = m_AnchorBody?.Pose.Transform(in m_AnchorLocalPose) ?? m_AnchorLocalPose;
			m_TargetGlobalPose = m_TargetBody?.Pose.Transform(in m_TargetLocalPose) ?? m_TargetLocalPose;
		}
	}
}