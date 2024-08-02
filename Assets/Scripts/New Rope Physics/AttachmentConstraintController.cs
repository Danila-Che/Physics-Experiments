using Unity.Mathematics;
using UnityEngine;

namespace RopePhysics
{
	public class AttachmentConstraintController : IAttachmentConstraintController
	{
		private readonly Attachment m_Attachment;
		private readonly Transform m_Transform;
		private float3 m_Position;

		public AttachmentConstraintController(Attachment attachment, Transform transform)
		{
			m_Attachment = attachment;
			m_Transform = transform;
			m_Position = transform.position;
		}

		public Attachment Attachment => m_Attachment;

		public float3 Position => m_Position;

		public void ActualisePosition()
		{
			m_Position = m_Transform.position;
		}

		public void Resolve()
		{
			if (m_Attachment is Attachment.Dynamic)
			{
				m_Transform.position = m_Position;
			}
			else if (m_Attachment is Attachment.Static)
			{
				m_Position = m_Transform.position;
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
