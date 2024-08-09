using UnityEngine;

namespace FakeXPBDMonoBehaviour
{
	[SelectionBase]
	[RequireComponent(typeof(Collider))]
	public class FakeColliderController : MonoBehaviour
	{
		private void OnEnable()
		{
			var collider = GetComponent<Collider>();
			collider.hasModifiableContacts = true;
		}
	}
}
