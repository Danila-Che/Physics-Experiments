using System.Collections.Generic;
using Unity.Collections;

namespace RopePhysics
{
	public interface IRenderer
	{
		public void Init();

		public void Draw(NativeArray<Particle>.ReadOnly particles);

		public void Draw(Particle[] particles);

		public void Draw(List<Particle> particles);
	}
}
