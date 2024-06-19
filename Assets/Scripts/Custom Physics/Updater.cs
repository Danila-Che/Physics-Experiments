using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics
{
    public abstract class Updater : MonoBehaviour
    {
        [SerializeField] private List<RopeSolver> m_Solvers;

        protected void BeginStep()
        {
            m_Solvers.ForEach(solver => solver.BeginStep());
        }

        protected void Substep(float substepDeltaTimeInSeconds)
        {
            m_Solvers.ForEach(solver => solver.Substep(substepDeltaTimeInSeconds));
        }

        protected void EndStep()
        {
            m_Solvers.ForEach(solver => solver.EndStep());
        }
    }
}
