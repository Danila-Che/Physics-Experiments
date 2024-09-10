using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics
{
	public partial class OnBeginStepJob : SystemBase
	{
		[BurstCompile]
		private partial struct PoseJob : IJobEntity
		{
			public readonly void Execute([ReadOnly] ref RigidBody rigidBody, ref Pose pose)
			{
				if (rigidBody.IsKinematic)
				{
					return;
				}

				pose = new Pose(pose.Position + new float3(0f, -1f, 0f), pose.Rotation);
			}
		}

		protected override void OnUpdate()
		{
			Dependency = new PoseJob().ScheduleParallel(Dependency);
		}
	}
}
