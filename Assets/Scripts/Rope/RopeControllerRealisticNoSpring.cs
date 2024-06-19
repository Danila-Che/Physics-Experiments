using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LineRenderer))]
public class RopeControllerRealisticNoSpring : MonoBehaviour
{
	public struct Particle
	{
		private readonly Rigidbody m_rigidbody;
		private Vector3 m_previousPosition;

		public Particle(Rigidbody rigidbody)
		{
			m_rigidbody = rigidbody;
			m_previousPosition = m_rigidbody.position;
		}

		public readonly Vector3 Position
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => m_rigidbody.position;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => m_rigidbody.position = value;
		}

		public readonly Vector3 PreviousPosition
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => m_previousPosition;
		}

		public readonly void WakeUp()
		{
			m_rigidbody.WakeUp();
		}

		public void RememberPosition()
		{
			m_previousPosition = m_rigidbody.position;
		}
	}

	[SerializeField] private Transform m_objectRopeIsConnectedTo;
	[SerializeField] private ObjectHangingFromRope m_objectHangingFromRope;
	[SerializeField] private Vector3 m_gravity = new(0f, -9.81f, 0f);
	[Min(0.01f)]
	[SerializeField] private float m_ropeLength = 7.0f;
	[Min(2)]
	[SerializeField] private int m_ropeSectionCount = 15;
	[Min(0.01f)]
	[SerializeField] private float m_ropeWidth = 0.2f;
	[Range(0, 50)]
	[SerializeField] private int m_iterations;

	private LineRenderer m_lineRenderer;
	private Particle[] m_particles;
	private float m_ropeSectionLength;

	private void Start()
	{
		m_objectHangingFromRope.Init();
		m_lineRenderer = GetComponent<LineRenderer>();
		m_ropeSectionLength = m_ropeLength / (float)m_ropeSectionCount;
		var ropeSectionPosition = m_objectRopeIsConnectedTo.position;

		m_particles = new Particle[m_ropeSectionCount];

		for (int i = 0; i < m_ropeSectionCount; i++)
		{
			m_particles[i] = CreateParticle(ropeSectionPosition);
			ropeSectionPosition.y -= m_ropeSectionLength;
		}
	}

	private void Update()
	{
		DisplayRope();

		m_objectHangingFromRope.SetPosition(m_particles[^1].Position);
		m_objectHangingFromRope.LookAt(m_particles[^2].Position);
	}

	private void FixedUpdate()
	{
		UpdateRopeSimulation(Time.fixedDeltaTime);

		for (int i = 0; i < m_iterations; i++)
		{
			ImplementMaximumStretch();
		}
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		if (m_particles == null) { return; }

		Gizmos.color = Color.green;

		Array.ForEach(m_particles, particle =>
		{
			Gizmos.DrawSphere(particle.Position, m_ropeWidth);
		});
	}

#endif

	public void ChangeLength(float change)
	{
		m_ropeLength += change;
		m_ropeSectionLength = m_ropeLength / (float)m_ropeSectionCount;
	}

	private Particle CreateParticle(Vector3 position)
	{
		var particle = new GameObject();
		particle.transform.position = position;
		SceneManager.MoveGameObjectToScene(particle, gameObject.scene);

		var sphereCollider = particle.AddComponent<SphereCollider>();
		sphereCollider.radius = m_ropeWidth;

		var rigidbody = particle.AddComponent<Rigidbody>();
		rigidbody.useGravity = false;
		rigidbody.drag = 1.0f;

		return new Particle(rigidbody);
	}

	//Verlet integration
	//From top to bottom
	private void UpdateRopeSimulation(float deltaTime)
	{
		m_particles[0].Position = m_objectRopeIsConnectedTo.position;

		for (int i = 1; i < m_particles.Length; i++)
		{
			UpdateParticleSimulation(ref m_particles[i], deltaTime);
		}
	}

	private void UpdateParticleSimulation(ref Particle particle, float deltaTime)
	{
		var velocity = particle.Position - particle.PreviousPosition;

		particle.WakeUp();
		particle.RememberPosition();
		particle.Position += velocity;
		particle.Position += m_gravity * deltaTime;
	}

	//Make sure the rope sections have the correct lengths
	private void ImplementMaximumStretch()
	{
		for (int i = 0; i < m_particles.Length - 1; i++)
		{
			var topSection = m_particles[i];
			var bottomSection = m_particles[i + 1];

			var distance = Vector3.Distance(topSection.Position, bottomSection.Position);
			var distanceError = Mathf.Abs(distance - m_ropeSectionLength);

			Vector3 changeDirection;

			if (distance > m_ropeSectionLength)
			{
				//Compress
				changeDirection = (topSection.Position - bottomSection.Position).normalized;
			}
			else if (distance < m_ropeSectionLength)
			{
				//Extend
				changeDirection = (bottomSection.Position - topSection.Position).normalized;
			}
			else
			{
				continue;
			}

			var change = changeDirection * distanceError;

			if (i != 0)
			{
				bottomSection.Position += change * 0.5f;
				m_particles[i + 1] = bottomSection;

				topSection.Position -= change * 0.5f;
				m_particles[i] = topSection;
			}
			//Because the rope is connected to something
			else
			{
				bottomSection.Position += change;
				m_particles[i + 1] = bottomSection;
			}
		}
	}

	private void DisplayRope()
	{
		m_lineRenderer.startWidth = m_ropeWidth;
		m_lineRenderer.endWidth = m_ropeWidth;

		var positions = new Vector3[m_particles.Length];

		for (int i = 0; i < m_particles.Length; i++)
		{
			positions[i] = m_particles[i].Position;
		}

		m_lineRenderer.positionCount = positions.Length;
		m_lineRenderer.SetPositions(positions);
	}
}
