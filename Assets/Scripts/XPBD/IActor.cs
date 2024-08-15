using Unity.Mathematics;

namespace XPBD
{
	public interface IActor
	{
		void ApplyAcceleration(float deltaTime, float3 acceleration);
		void ApplyDrag(float deltaTime);
		void BeginStep();
		void Step(float deltaTime);
		void EndStep(float deltaTime);
	}
}
