using System.Collections.Generic;
using Unity.Collections;

namespace XPBD.SoftBody.Render
{
	public interface IRenderer
	{
		public void Init();

		public void Draw(NativeArray<FakeParticle>.ReadOnly particles);

		public void Draw(FakeParticle[] particles);

		public void Draw(List<FakeParticle> particles);
	}
}
