using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

/**
 * A simple copy of example code from the manual, modified to orient particles
 * based on the center of the system.
 */

[ExecuteInEditMode]
public class OrientParticles : MonoBehaviour
{

  ParticleSystem m_System;
  ParticleSystem.Particle[] m_Particles;
  public float angleOffset = 0.0f;
	public float speed = 1.0f;
  private void LateUpdate()
  {
      InitializeIfNeeded();

      // GetParticles is allocation free because we reuse the m_Particles buffer between updates
      int numParticlesAlive = m_System.GetParticles(m_Particles);

      // Change only the particles that are alive
      for (int i = 0; i < numParticlesAlive; i++)
      {
		  /*
        // use atan2 to calc angle based on position, then convert to degrees.
        float angle = Mathf.Atan2(m_Particles[i].position.x, m_Particles[i].position.y) * Mathf.Rad2Deg;
        // add the offset (in case the artwork is rotated, etc.)
        m_Particles[i].rotation = angle + angleOffset;
		*/
		
		Vector3 rot = m_Particles[i].rotation3D;
		Vector3 angVel = m_Particles[i].angularVelocity3D;
		Vector3 vel = m_Particles[i].velocity;
		Vector3 normVel = vel.normalized;
		//Vector3 newDir = Vector3.RotateTowards(Vector3.zero, normVel, speed*Time.deltaTime, 0.0f);
		Vector3 newDir = (Quaternion.LookRotation(normVel)).eulerAngles;
		
		if (i < 10){
			//Debug.Log("angVel: " + angVel.ToString());
			//Debug.Log("vel: " + vel.ToString());
			//Debug.Log("Normvel: " + normVel.ToString());
			//Debug.Log("3dRot: " + newDir.ToString());
			//Debug.Log("angVel: " + angVel.ToString() + " : New Dir : " + newDir.ToString());
		}
		//m_Particles[i].rotation3D = m_Particles[i].angularVelocity3D;
		m_Particles[i].rotation3D = newDir;
      }

      // Apply the particle changes to the particle system
      m_System.SetParticles(m_Particles, numParticlesAlive);
  }

  void InitializeIfNeeded()
  {
      if (m_System == null)
          m_System = GetComponent<ParticleSystem>();

      if (m_Particles == null || m_Particles.Length < m_System.maxParticles)
          m_Particles = new ParticleSystem.Particle[m_System.maxParticles]; 
  }

}