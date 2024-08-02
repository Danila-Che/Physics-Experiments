using System;

namespace RopePhysics
{
	public interface IActorController : IDisposable
	{
		void InitWithSolver(Solver solver);
		void ActualiaseToSolver(Solver solver);
		void ActualiaseFromSolver(Solver solver);
	}
	
	public interface IPrimaryActorController : IActorController { }

	public interface ISecondaryActorController : IActorController { }
}
