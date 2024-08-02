using UnityEngine;

namespace CustomCollisionDetection
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
	public class CustomBoxCollider : MonoBehaviour
    {
        private BoxCollider m_BoxCollider;
        private Rigidbody m_Rigidbody;
		private Vector3 m_Velocity;

        public void Init()
        {
            m_BoxCollider = GetComponent<BoxCollider>();
            m_Rigidbody = GetComponent<Rigidbody>();

            m_Rigidbody.useGravity = false;
            m_Rigidbody.isKinematic = true;

			m_Velocity = Vector3.zero;
		}

        public void ResolveAsseleration(Vector3 acceleration, float deltaTime)
        {
            m_Velocity += acceleration * deltaTime;
            transform.position += m_Velocity * deltaTime;
		}

        public void ResolveCollision(int maxCollisionCount)
        {
            var colliderHitBuffer = new Collider[maxCollisionCount];
            var position = transform.position;
            var rotation = transform.rotation;

            var collidersCount = Physics.OverlapBoxNonAlloc(
				position,
                m_BoxCollider.size / 2,
				colliderHitBuffer,
				rotation);

            var impulse = Vector3.zero;

            if (collidersCount > 1)
            {
                m_Velocity = Vector3.zero;
            }

            for (int i = 0; i < collidersCount; i++)
            {
                if (colliderHitBuffer[i] == m_BoxCollider)
                {
                    continue;
                }

				var colliderPosition = colliderHitBuffer[i].transform.position;
				var colliderRotation = colliderHitBuffer[i].transform.rotation;

				Physics.ComputePenetration(
					m_BoxCollider,
					position,
					rotation,
					colliderHitBuffer[i],
                    colliderPosition,
                    colliderRotation,
					out Vector3 direction,
					out float distance);

                Debug.DrawRay(position, direction, Color.green);

                impulse += distance * direction;
				transform.position += distance * direction;
			}

            impulse /= collidersCount;

			//transform.position += impulse;

			//if (impulse != Vector3.zero)
			//{
			//	transform.rotation = Quaternion.LookRotation(impulse.normalized);
			//}
		}
    }
}
