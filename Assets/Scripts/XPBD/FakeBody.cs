using Unity.Mathematics;

namespace XPBD
{
	public class FakeBody
	{
		private const float k_MaxRotationPerSubstep = 0.5f;

		private readonly BaseFakeCollider m_Collider;

		private FakePose m_Pose;
		private FakePose m_PreviousPose;

		private float3 m_Velocity;
		private float3 m_AngularVelocity; // omega

		public FakeBody(in FakePose pose, BaseFakeCollider collider)
		{
			m_Pose = pose;
			m_PreviousPose = pose;
			m_Velocity = float3.zero;
			m_AngularVelocity = float3.zero;

			m_Collider = collider;
		}

		public FakePose Pose => m_Pose;

		public BaseFakeCollider Collider => m_Collider;

		public float3 AngularVelocity => m_AngularVelocity;

		// public void SetBox()
		// {
		// 	var boxCollider = GetComponent<BoxCollider>();

		// 	Assert.IsNotNull(boxCollider);

		// 	m_InverseMass = math.isfinite(m_Mass) switch
		// 	{
		// 		true => 1.0f / m_Mass,
		// 		false => 0.0f,
		// 	};

		// 	var size = boxCollider.size;
		// 	var inertiaTensor = new float3()
		// 	{
		// 		x = (m_Mass / 12.0f) * (size.y * size.y + size.z * size.z),
		// 		y = (m_Mass / 12.0f) * (size.x * size.x + size.z * size.z),
		// 		z = (m_Mass / 12.0f) * (size.x * size.x + size.y * size.y),
		// 	};

		// 	m_InverseInertiaTensor = new float3()
		// 	{
		// 		x = 1.0f / inertiaTensor.x,
		// 		y = 1.0f / inertiaTensor.y,
		// 		z = 1.0f / inertiaTensor.z,
		// 	};
		// }

		public void ApplyAcceleration(float deltaTime, float3 acceleration)
		{
			m_Velocity += acceleration * deltaTime;
		}

		public void BeginStep()
		{
			m_PreviousPose = m_Pose;
		}

		public void Step(float deltaTime)
		{
			m_Pose = m_Pose.Translate(m_Velocity * deltaTime);

			ApplyRotation(m_AngularVelocity, deltaTime);
		}

		public void EndStep(float deltaTime)
		{
			m_Velocity = (m_Pose.Position - m_PreviousPose.Position) / deltaTime;

			var deltaQuaternion = math.mul(m_Pose.Rotation, math.inverse(m_PreviousPose.Rotation));
			m_AngularVelocity = new float3(
				x: 2.0f * deltaQuaternion.value.x / deltaTime,
				y: 2.0f * deltaQuaternion.value.y / deltaTime,
				z: 2.0f * deltaQuaternion.value.z / deltaTime);

			if (deltaQuaternion.value.w < 0.0f)
			{
				m_AngularVelocity = -m_AngularVelocity;
			}
		}

		public float3 GetVelocityAt(float3 position)
		{
			var velocity = math.cross(position - m_Pose.Position, m_AngularVelocity);

			return m_Velocity - velocity;
		}

		public float GetInverseMass(float3 normal, float3? position = null)
		{
			var nVector = position == null
				? normal
				: math.cross(position.Value - m_Pose.Position, normal);

			nVector = m_Pose.InverseRotate(nVector);

			var w =
				nVector.x * nVector.x * m_Collider.InverseInertiaTensor.x +
				nVector.y * nVector.y * m_Collider.InverseInertiaTensor.y +
				nVector.z * nVector.z * m_Collider.InverseInertiaTensor.z;

			if (position != null)
			{
				w += m_Collider.InverseMass;
			}

			return w;
		}

		public void ApplyCorrection(float3 correction, float3? position = null, bool velocityLevel = false)
		{
			float3 deltaQuaternion;
			if (position == null)
			{
				deltaQuaternion = correction;
			}
			else
			{
				if (velocityLevel)
				{
					m_Velocity += correction * m_Collider.InverseMass;
				}
				else
				{
					m_Pose = m_Pose.Translate(correction * m_Collider.InverseMass);
				}

				deltaQuaternion = math.cross(position.Value - m_Pose.Position, correction);
			}

			deltaQuaternion = m_Pose.InverseRotate(deltaQuaternion);
			deltaQuaternion *= m_Collider.InverseInertiaTensor;
			deltaQuaternion = m_Pose.Rotate(deltaQuaternion);

			if (velocityLevel)
			{
				m_AngularVelocity += deltaQuaternion;
			}
			else
			{
				ApplyRotation(deltaQuaternion);
			}
		}

		public static void ApplyBodyPairCorrection(
			FakeBody body0,
			FakeBody body1,
			float3 correction,
			float compliance,
			float deltaTime,
			float3? position0 = null,
			float3? position1 = null,
			bool velocityLevel = false)
		{
			var correctionLength = math.length(correction);

			if (correctionLength == 0.0f)
			{
				return;
			}

			var normal = correction / correctionLength;

			var w0 = body0 == null ? 0.0f : body0.GetInverseMass(normal, position0);
			var w1 = body1 == null ? 0.0f : body1.GetInverseMass(normal, position1);

			var w = w0 + w1;

			if (w == 0.0f)
			{
				return;
			}

			var lambda = -correctionLength / (w + compliance / (deltaTime * deltaTime));
			normal *= -lambda;

			if (body0 != null)
			{
				body0.ApplyCorrection(normal, position0, velocityLevel);
			}

			if (body1 != null)
			{
				normal = -normal;
				body1.ApplyCorrection(normal, position1, velocityLevel);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="body0"></param>
		/// <param name="body1"></param>
		/// <param name="n">Axis</param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="minAngle"></param>
		/// <param name="maxAngle"></param>
		/// <param name="compliance"></param>
		/// <param name="deltaTime"></param>
		/// <param name="maxCorrection"></param>
		public static void LimitAngle(
			FakeBody body0,
			FakeBody body1,
			float3 n,
			float3 a,
			float3 b,
			float minAngle,
			float maxAngle,
			float compliance,
			float deltaTime,
			float maxCorrection = math.PI)
		{
			var c = math.cross(a, b);
			var phi = math.asin(math.dot(c, n));

			if (math.dot(a, b) < 0.0f)
			{
				phi = math.PI - phi;
			}

			if (phi > math.PI)
			{
				phi -= 2.0f * math.PI;
			}

			if (phi < -math.PI)
			{
				phi += 2.0f * math.PI;
			}

			if (phi < minAngle || phi > maxAngle)
			{
				phi = math.min(math.max(minAngle, phi), maxAngle);
				var q = quaternion.AxisAngle(n, phi);
				var omega = math.cross(math.mul(q, a), b);

				phi = math.length(omega);

				if (phi > maxCorrection)
				{
					omega *= maxCorrection / phi;
				}

				ApplyBodyPairCorrection(body0, body1, omega, compliance, deltaTime);
			}
		}

		public float CalculateFirctionForceLimit(
			float frictionMagnitude,
			float3 contactNormal,
			float3 contactPoint,
            float3 deltaVDirection,
			float deltaVMagnitude)
		{
			var beforePointV = GetVelocityAt(contactPoint);

            // TODO refactor
            var beforeVelocity = m_Velocity;
            var beforeAngularVelocity = m_AngularVelocity;

            var correctionAmount = deltaVDirection * frictionMagnitude;
            ApplyCorrection(correctionAmount, contactPoint, true);

            var afterPointV = GetVelocityAt(contactPoint);
            var actualDeltaV = afterPointV - beforePointV;
            var actualTangDeltaV = actualDeltaV - contactNormal * math.dot(actualDeltaV, contactNormal);
            var actualTDVLenght = math.length(actualTangDeltaV);
            
            m_Velocity = beforeVelocity;
            m_AngularVelocity = beforeAngularVelocity;
            var reduction = actualTDVLenght == 0f ? 0f : math.clamp(deltaVMagnitude / actualTDVLenght, 0, 1);
            return reduction * frictionMagnitude;
		}

		public void ApplyDrag(float deltaTime)
		{
			var drag = m_Velocity * m_Collider.Drag;
			m_Velocity -= deltaTime * m_Collider.InverseMass * drag;
		}

		/// <summary>
		/// </summary>
		/// <param name="rotation">Angular speed</param>
		/// <param name="scale"></param>
		private void ApplyRotation(float3 rotation, float scale = 1.0f)
		{
			// Safety clamping. This happens very rarely if the solver
			// wants to turn the body by more than 30 degrees in the
			// orders of milliseconds

			var phi = math.length(rotation);

			if (phi * scale > k_MaxRotationPerSubstep)
			{
				scale = k_MaxRotationPerSubstep / phi;
			}

			var deltaQuaternion = new quaternion(
				x: rotation.x * scale,
				y: rotation.y * scale,
				z: rotation.z * scale,
				w: 0.0f);
			deltaQuaternion = math.mul(deltaQuaternion, m_Pose.Rotation);

			var quaternion = new quaternion(
				x: m_Pose.Rotation.value.x + 0.5f * deltaQuaternion.value.x,
				y: m_Pose.Rotation.value.y + 0.5f * deltaQuaternion.value.y,
				z: m_Pose.Rotation.value.z + 0.5f * deltaQuaternion.value.z,
				w: m_Pose.Rotation.value.w + 0.5f * deltaQuaternion.value.w);
			quaternion = math.normalize(quaternion);

			m_Pose = m_Pose.SetRotation(quaternion);
		}
	}
}
