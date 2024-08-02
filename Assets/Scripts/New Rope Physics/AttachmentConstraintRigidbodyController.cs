using Unity.Mathematics;
using UnityEngine;

namespace RopePhysics
{
    public class AttachmentConstraintRigidbodyController : IAttachmentConstraintController
    {
		private readonly Attachment m_Attachment;
		private readonly Rigidbody m_Rigidbody;
		private float3 m_Position;

		public AttachmentConstraintRigidbodyController(Attachment attachment, Rigidbody rigidbody)
		{
			m_Attachment = attachment;
			m_Rigidbody = rigidbody;
			m_Position = rigidbody.position;
		}

		public Attachment Attachment => m_Attachment;

		public float3 Position => m_Position;

		public void ActualisePosition()
		{
			m_Position = m_Rigidbody.position;
		}

		public void Resolve()
		{
			if (m_Attachment is Attachment.Dynamic)
			{
				m_Rigidbody.position = m_Position;
			}
			else if (m_Attachment is Attachment.Static)
			{
				m_Position = m_Rigidbody.position;
			}
		}

		public void Resolve(ref AttachmentConstraint attachmentConstraint)
		{
			if (m_Attachment is Attachment.Dynamic)
			{
				m_Position = attachmentConstraint.Position;
			}
			else if (m_Attachment is Attachment.Static)
			{
				attachmentConstraint.Position = m_Position;
			}
		}
	}
}
