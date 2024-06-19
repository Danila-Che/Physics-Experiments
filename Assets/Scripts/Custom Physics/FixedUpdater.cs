using UnityEngine;

namespace CustomPhysics
{
    public sealed class FixedUpdater : Updater
    {
        [SerializeField] private bool m_SubstepUnityPhysics = false;
        [Min(1)]
        [SerializeField] private int m_Substeps = 1;
        
        private void OnDisable()
        {
            Physics.simulationMode = SimulationMode.FixedUpdate;
        }

        private void FixedUpdate()
        {
            if (m_SubstepUnityPhysics)
            {
                Physics.simulationMode = SimulationMode.Script;
            }

            var substepDelta = Time.fixedDeltaTime / (float)m_Substeps;

            BeginStep();

            for (int i = 0; i < m_Substeps; i++)
            {
                Substep(substepDelta);

                if (m_SubstepUnityPhysics)
                {
                    Physics.Simulate(substepDelta);
                }
            }

            EndStep();
        }
    }
}
