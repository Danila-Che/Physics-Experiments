using Unity.Entities;
using UnityEngine;

namespace ECSPhysics
{
	public class Solver : MonoBehaviour
	{
		private EntityManager m_EntityManager;
		private World m_CustomWorld;

		private Entity m_Entity;

		private void Start()
		{
			m_CustomWorld = new World("Custom World");
			m_EntityManager = m_CustomWorld.EntityManager;

			EntityArchetype archetype = m_EntityManager.CreateArchetype(
				typeof(RigidBody),
				typeof(Pose)
			);

			m_Entity = m_EntityManager.CreateEntity(archetype);

			m_EntityManager.SetComponentData(m_Entity, new RigidBody(isKinematic: false));
			m_EntityManager.SetComponentData(m_Entity, Pose.k_Identity);

			var onBeginStepJob = m_CustomWorld.GetOrCreateSystem<OnBeginStepJob>();
			m_CustomWorld.GetOrCreateSystemManaged<SimulationSystemGroup>().AddSystemToUpdateList(onBeginStepJob);
		}

		private void OnDestroy()
		{
			if (m_CustomWorld.IsCreated)
			{
				m_CustomWorld.Dispose();
			}
		}

		private void FixedUpdate()
		{
			m_CustomWorld.Update();

			Debug.Log($"> {m_EntityManager.GetComponentData<Pose>(m_Entity).Position}");
		}
	}
}
