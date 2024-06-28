using Unity.Mathematics;
using UnityEngine;

namespace RopePhysics
{
	public enum Attachment
	{
		Static,
		Dynamic,
	}

	public class AttachmentConstraintAlias
	{
		private readonly Attachment m_Attachment;
		private float3 m_Position;

		public AttachmentConstraintAlias() : this(Attachment.Static) { }

		public AttachmentConstraintAlias(Attachment attachment)
		{
			m_Attachment = attachment;
		}

		public Attachment Attachment => m_Attachment;

		public float3 Position => m_Position;

		public void SetPosition(float3 position)
		{
			m_Position = position;
		}

		public void SetPosition(Vector3 position)
		{
			m_Position = (float3)position;
		}
	}
}
