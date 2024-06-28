using Unity.Mathematics;

namespace RopePhysics
{
	public struct AttachmentConstraint
	{
		public readonly bool IsExists;
		public readonly Attachment Attachment;
		public float3 Position;

		public AttachmentConstraint(bool isExists)
		{
			IsExists = isExists;
			Attachment = Attachment.Static;
			Position = float3.zero;
		}

		public AttachmentConstraint(Attachment attachment, bool isExists)
		{
			IsExists = isExists;
			Attachment = attachment;
			Position = float3.zero;
		}

		public static AttachmentConstraint Empty => new(isExists: false);

		public void SetPosition(float3 position)
		{
			Position = position;
		}
	}
}
