using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectHangingFromRope : MonoBehaviour
{
	private Rigidbody m_rigidbody;

	public void Init()
	{
		m_rigidbody = GetComponent<Rigidbody>();
	}

	public void SetPosition(Vector3 position)
	{
		m_rigidbody.WakeUp();
		m_rigidbody.position = position;
	}

	public void LookAt(Vector3 target)
	{
		//transform.LookAt(target);
	}
}
