using UnityEngine;
using XPBD;

namespace FakeXPBDMonoBehaviour
{
	[SelectionBase]
	[RequireComponent(typeof(Collider))]
	[DefaultExecutionOrder((int)ExecutionOrder.Collider)]
	public class FakeColliderController : MonoBehaviour
	{
		private IFakeCollider m_Collider;

		public IFakeCollider Collider => m_Collider;

		public void Initialize()
		{
			var collider = GetComponent<Collider>();
			collider.hasModifiableContacts = true;
			collider.providesContacts = true;

			if (collider is BoxCollider boxCollider)
			{
				m_Collider = new FakeBoxCollider(boxCollider);
			}
			else
			{
				m_Collider = null;
			}
		}
	}
}
