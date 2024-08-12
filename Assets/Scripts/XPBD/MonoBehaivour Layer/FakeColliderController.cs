using UnityEngine;
using XPBD;

namespace FakeXPBDMonoBehaviour
{
	[SelectionBase]
	[RequireComponent(typeof(Collider))]
	public class FakeColliderController : MonoBehaviour
	{
		public BaseFakeCollider Collider => new FakeBoxCollider();

		private void OnEnable()
		{
			var collider = GetComponent<Collider>();
			collider.hasModifiableContacts = true;
		}
	}
}
