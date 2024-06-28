using System;
using UnityEngine;

namespace RopePhysics
{
	public class RopeController : MonoBehaviour
	{
		public event EventHandler<Rope> RopeWasInitialized;

		[Serializable]
		private struct AttachmentConstraintController
		{
			[SerializeField] private Transform m_AttachedObject;
			[SerializeField] private Attachment m_Attachment;
			private AttachmentConstraintAlias m_AttachmentConstraint;

			public AttachmentConstraintAlias Init()
			{
				m_AttachmentConstraint = new AttachmentConstraintAlias(m_Attachment);
				UpdatePosition();

				return m_AttachmentConstraint;
			}

			public readonly void Update()
			{
				UpdatePosition();
			}

			private readonly void UpdatePosition()
			{
				m_AttachmentConstraint.SetPosition(m_AttachedObject.position);
			}
		}

		[Min(0.0f)]
		[SerializeField] private float m_SpanDistance = 1.0f;
		[SerializeField] private AttachmentConstraintController m_TargetAttachment;
		[SerializeField] private AttachmentConstraintController m_SourceAttachment;

		private Rope m_Rope;

		public Rope Rope => m_Rope;

		private void OnEnable()
		{
			m_Rope = new Rope(
				m_SpanDistance,
				m_TargetAttachment.Init(),
				m_SourceAttachment.Init());

			m_Rope.CreateRope();

			RopeWasInitialized?.Invoke(this, m_Rope);
		}

		private void OnDisable()
		{
			m_Rope?.Dispose();
		}

		private void Update()
		{
			m_TargetAttachment.Update();
			m_SourceAttachment.Update();
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if (m_Rope == null) { return; }

			Gizmos.color = Color.green;

			m_Rope.Job.Complete();

			for (int i = 0; i < m_Rope.Particles.Length; i++)
			{
				Gizmos.DrawSphere(m_Rope.Particles[i].Position, 0.2f);
			}
		}

#endif
	}
}
