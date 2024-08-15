using Unity.Mathematics;

namespace XPBD
{
	public class FakeJoint
	{
		private readonly FakeBody m_Body0;
		private readonly FakeBody m_Body1;
		private readonly FakePose m_LocalPose0;
		private readonly FakePose m_LocalPose1;
		private FakePose m_GlobalPose0;
		private FakePose m_GlobalPose1;

		private readonly FakeJointParams m_Params;

		public FakeJoint(FakeBody body0, FakeBody body1, FakeJointParams parameters)
		{
			m_Body0 = body0;
			m_Body1 = body1;
			m_Params = parameters;

			m_LocalPose0 = new FakePose(m_Params.AnchorLocalPosition, m_Params.AnchorLocalRotation);
			m_LocalPose1 = new FakePose(m_Params.TargetLocalPosition, m_Params.TargetLocalRotation);

			m_GlobalPose0 = m_LocalPose0;
			m_GlobalPose1 = m_LocalPose1;
		}

		public FakePose GlobalPose0 => m_GlobalPose0;
		public FakePose GlobalPose1 => m_GlobalPose1;

		private void UpdateGlobalPoses()
		{
			m_GlobalPose0 = m_LocalPose0;

			if (m_Body0 != null)
			{
				m_GlobalPose0 = m_Body0.Pose.Transform(m_GlobalPose0);
			}

			m_GlobalPose1 = m_LocalPose1;

			if (m_Body1 != null)
			{
				m_GlobalPose1 = m_Body1.Pose.Transform(m_GlobalPose1);
			}
		}

		public void SolvePosition(float deltaTime)
		{
			UpdateGlobalPoses();

			// orientation
			if (m_Params.Type == FakeJointType.Fixed)
			{
				var quaternion = math.mul(m_GlobalPose1.Rotation, math.inverse(m_GlobalPose0.Rotation));
				var omega = new float3(
					x: 2.0f * quaternion.value.x,
					y: 2.0f * quaternion.value.y,
					z: 2.0f * quaternion.value.z);

				if (quaternion.value.w < 0.0f)
				{
					omega = -omega;
				}

				FakeBody.ApplyBodyPairCorrection(m_Body0, m_Body1, omega, m_Params.Compliance, deltaTime);
			}

			if (m_Params.Type == FakeJointType.Hinge)
			{
				// align axes
				var a0 = FakeUtility.GetAxis0(m_GlobalPose0.Rotation);
				var a1 = FakeUtility.GetAxis0(m_GlobalPose1.Rotation);
				FakeBody.ApplyBodyPairCorrection(m_Body0, m_Body1, math.cross(a0, a1), 0f, deltaTime);

				// limits
				if (m_Params.HasSwingLimits)
				{
					UpdateGlobalPoses();
					var n = FakeUtility.GetAxis0(m_GlobalPose0.Rotation);
					var b0 = FakeUtility.GetAxis1(m_GlobalPose0.Rotation);
					var b1 = FakeUtility.GetAxis1(m_GlobalPose1.Rotation);
					FakeBody.LimitAngle(
						m_Body0,
						m_Body1,
						n,
						b0,
						b1,
						m_Params.MinSwingAngle,
						m_Params.MaxSwingAngle,
						m_Params.SwingLimitsCompliance,
						deltaTime);
				}
			}

			if (m_Params.Type == FakeJointType.Spherical)
			{
				// swing limits
				if (m_Params.HasSwingLimits)
				{
					UpdateGlobalPoses();
					var a0 = FakeUtility.GetAxis0(m_GlobalPose0.Rotation);
					var a1 = FakeUtility.GetAxis0(m_GlobalPose1.Rotation);
					var n = math.normalize(math.cross(a0, a1));
					FakeBody.LimitAngle(
						m_Body0,
						m_Body1,
						n,
						a0,
						a1,
						m_Params.MinSwingAngle,
						m_Params.MaxSwingAngle,
						m_Params.SwingLimitsCompliance,
						deltaTime);
				}

				// twist limits
				if (m_Params.HasTwistLimits)
				{
					UpdateGlobalPoses();
					var n0 = FakeUtility.GetAxis0(m_GlobalPose0.Rotation);
					var n1 = FakeUtility.GetAxis0(m_GlobalPose1.Rotation);

					var n = math.normalize(n0 + n1);
					var a0 = FakeUtility.GetAxis1(m_GlobalPose0.Rotation);
					a0 = math.normalize(a0 - n * math.dot(n, a0));

					var a1 = FakeUtility.GetAxis1(m_GlobalPose1.Rotation);
					a1 = math.normalize(a1 - n * math.dot(n, a1));

					// handling gimbal lock problem
					var maxCorrection = math.dot(n0, n1) > -0.5f ? 2f * math.PI : deltaTime;

					FakeBody.LimitAngle(
						m_Body0,
						m_Body1,
						n,
						a0,
						a1,
						m_Params.MinTwistAngle,
						m_Params.MaxTwistAngle,
						m_Params.TwistLimitCompliance,
						deltaTime,
						maxCorrection);
				}
			}

			// position
			// simple attachment

			UpdateGlobalPoses();
			var correction = float3.zero;
			if (m_Params.Type == FakeJointType.Distance)
			{
				var dp = m_GlobalPose1.Position - m_GlobalPose0.Position;
				var distance = math.length(dp);

				if (distance > m_Params.Distance)
				{
					correction = dp / distance * (distance - m_Params.Distance);
				}
			}
			else
			{
				correction = m_GlobalPose1.Position - m_GlobalPose0.Position;
			}

			FakeBody.ApplyBodyPairCorrection(
				m_Body0,
				m_Body1,
				correction,
				m_Params.Compliance,
				deltaTime,
				m_GlobalPose0.Position,
				m_GlobalPose1.Position);
		}

		public void SolveVelocity(float deltaTime)
		{
			// Gauss-Seidel vars us make damping unconditionally stable in a 
			// very simple way. We clamp the correction for each constraint
			// to the magnitude of the current velocity making sure that
			// we never subtract more than there actually is.

			if (m_Params.RotationDamping > 0.0f)
			{
				var omega = float3.zero;

				if (m_Body0 != null)
				{
					omega -= m_Body0.AngularVelocity;
				}

				if (m_Body1 != null)
				{
					omega += m_Body1.AngularVelocity;
				}

				omega *= math.min(1.0f, m_Params.RotationDamping * deltaTime);
				FakeBody.ApplyBodyPairCorrection(
					m_Body0,
					m_Body1,
					omega,
					0.0f,
					deltaTime,
					null,
					null,
					true);
			}

			if (m_Params.PositionDamping > 0.0f)
			{
				UpdateGlobalPoses();

				var velocity = float3.zero;

				if (m_Body0 != null)
				{
					velocity -= m_Body0.GetVelocityAt(m_GlobalPose0.Position);
				}

				if (m_Body1 != null)
				{
					velocity += m_Body1.GetVelocityAt(m_GlobalPose1.Position);
				}

				velocity *= math.min(1.0f, m_Params.PositionDamping * deltaTime);
				FakeBody.ApplyBodyPairCorrection(
					m_Body0,
					m_Body1,
					velocity,
					0.0f,
					deltaTime,
					m_GlobalPose0.Position,
					m_GlobalPose1.Position,
					true);
			}
		}
	}
}
