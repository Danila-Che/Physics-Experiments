using Unity.Mathematics;

namespace RopePhysics
{
    public interface IAttachmentConstraintController
    {
		Attachment Attachment { get; }

		float3 Position { get; }

		/// <summary>
		/// Call before simulation step
		/// </summary>
		void ActualisePosition();
		
		/// <summary>
		/// Call from MonoBehaviour to update a position for represent a object
		/// </summary>
		void Resolve();

		/// <summary>
		/// Call after simulation step to synchronise attachement and object
		/// </summary>
		/// <param name="attachmentConstraint">Attachement from solver</param>
		void Resolve(ref AttachmentConstraint attachmentConstraint);
	}
}
