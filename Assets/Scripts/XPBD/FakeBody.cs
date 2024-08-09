using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace XPBD
{
	[RequireComponent(typeof(Collider))]
	public class FakeBody : MonoBehaviour
	{
		private const float k_MaxRotationPerSubstep = 0.5f;

		[Min(0.01f)]
		[SerializeField] private float m_Mass = 1.0f;

		private FakePose m_Pose;
		private FakePose m_PreviousPose;
		private FakePose m_OriginPose;

		private float3 m_Velocity;
		/// <summary>
		/// Omega
		/// </summary>
		private float3 m_AngularVelocity;

		private float m_InverseMass;
		private float3 m_InverseInertiaTensor; // may just inverse inertia

		public void Init()
		{
			m_Pose = new FakePose(transform.position, transform.rotation);
			m_PreviousPose = new FakePose(m_Pose);
			m_OriginPose = new FakePose(m_Pose);
			m_Velocity = float3.zero;
			m_AngularVelocity = float3.zero;

			m_InverseMass = 1.0f;
			m_InverseInertiaTensor = new float3(1.0f);
		}

		public void SetBox()
		{
			var boxCollider = GetComponent<BoxCollider>();

			Assert.IsNotNull(boxCollider);

			m_InverseMass = math.isfinite(m_Mass) switch
			{
				true => 1.0f / m_Mass,
				false => 0.0f,
			};

			var size = boxCollider.size;
			var inertiaTensor = new float3()
			{
				x = (m_Mass / 12.0f) * (size.y * size.y + size.z * size.z),
				y = (m_Mass / 12.0f) * (size.x * size.x + size.z * size.z),
				z = (m_Mass / 12.0f) * (size.x * size.x + size.y * size.y),
			};

			m_InverseInertiaTensor = new float3()
			{
				x = 1.0f / inertiaTensor.x,
				y = 1.0f / inertiaTensor.y,
				z = 1.0f / inertiaTensor.z,
			};
		}

		public void Step(float deltaTime, float3 acceleration)
		{
			m_PreviousPose = m_Pose;
			m_Velocity += acceleration * deltaTime;
			m_Pose = m_Pose.Translate(m_Velocity * deltaTime);

			ApplyRotation(m_AngularVelocity, deltaTime);
		}

		public void EndStep(float deltaTime)
		{
			m_Velocity = (m_Pose.Position - m_PreviousPose.Position) / deltaTime;

			var deltaQuaternion = math.mul(m_Pose.Quaternion, math.conjugate(m_PreviousPose.Quaternion));
			m_AngularVelocity = new float3(
				x: 2.0f * deltaQuaternion.value.x / deltaTime,
				y: 2.0f * deltaQuaternion.value.y / deltaTime,
				z: 2.0f * deltaQuaternion.value.z / deltaTime);

			if (deltaQuaternion.value.w < 0.0f)
			{
				m_AngularVelocity = -m_AngularVelocity;
			}
		}

		/// <summary>
		/// Update position and rotation of body in Unity scene
		/// </summary>
		public void UpdatePresentation()
		{
			transform.SetPositionAndRotation(m_Pose.Position, m_Pose.Quaternion);
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
				nVector.x * nVector.x * m_InverseInertiaTensor.x +
				nVector.y * nVector.y * m_InverseInertiaTensor.y +
				nVector.z * nVector.z * m_InverseInertiaTensor.z;

			if (position != null)
			{
				w += m_InverseMass;
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
					m_Velocity += correction * m_InverseMass;
				}
				else
				{
					m_Pose = m_Pose.Translate(correction * m_InverseMass);
				}

				deltaQuaternion = math.cross(position.Value - m_Pose.Position, correction);
			}

			deltaQuaternion = m_Pose.InverseRotate(deltaQuaternion);
			deltaQuaternion *= m_InverseInertiaTensor;
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
			deltaQuaternion = math.mul(deltaQuaternion, m_Pose.Quaternion);

			var quaternion = new quaternion(
				x: m_Pose.Quaternion.value.x + 0.5f * deltaQuaternion.value.x,
				y: m_Pose.Quaternion.value.y + 0.5f * deltaQuaternion.value.y,
				z: m_Pose.Quaternion.value.z + 0.5f * deltaQuaternion.value.z,
				w: m_Pose.Quaternion.value.w + 0.5f * deltaQuaternion.value.w);
			quaternion = math.normalize(quaternion);

			m_Pose = m_Pose.SetRotation(quaternion);
		}
	}
}
