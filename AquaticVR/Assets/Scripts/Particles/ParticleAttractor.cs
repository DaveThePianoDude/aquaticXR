using UnityEngine;
using System.Collections;
 
public class ParticleAttractor : MonoBehaviour 
{
	
    public float PullDistance = 200;
    public Transform magnetTransform;
	public ParticleSystem particleSystem;

	void Start()
	{
		if (!magnetTransform){
			magnetTransform = this.transform;
		}
	}
	
	/*Update is called once per frame and is good for simple timers, basing changes off frame rate and recieving input. 
	Note that if one frame takes longer to process than the next, Update will not be called consistently*/
	void Update ()  
	{

		ParticlePull();
	}
 
    void ParticlePull ()
	{
		
		/*
		float sqrPullDistance = PullDistance * PullDistance;
		
		ParticleSystem.Particle[] x = particleSystem.Particle[particleSystem.particleCount+1]; 
		int y = particleSystem.GetParticles(x);

		for (int i = 0; i < y; i++){
		
			Vector3 offset = magnetTransform.localPosition - x[i].position;
			//creates Vector3 variable based on the position of the target magnetTransform (set by user) and the current particle position
			float sqrLen = offset.sqrMagnitude;
			//creats float type integer based on the square magnitude of the offset variable set above (faster than .Magnitude)

			if (sqrLen <= sqrPullDistance){
				x[i].position = Vector3.Lerp(x[i].position, magnetTransform.localPosition, Mathf.SmoothStep(0, 2, (Time.deltaTime / 0.1F)));
				if ((x[i].position-magnetTransform.localPosition).magnitude <= 30){
					x[i].lifetime = 0;
				}
			}
		}

		particleSystem.SetParticles(x, y);
		return;
		*/
		/*
		ParticleSystem.Particle[] ps = particleSystem.Particle[particles.particleCount];
		particles.GetParticles (ps);
		for (int i =0; i<ps.Length; i++){
			ps [i].velocity = Vector3.Lerp (ps [i].velocity, (particlesAttractor.position - ps [i].position).normalized, 0.1f);
			particles.SetParticles (ps, ps.Length);
		}
		*/
    }
 

 }