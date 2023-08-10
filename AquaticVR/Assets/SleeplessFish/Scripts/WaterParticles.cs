using UnityEngine;
using System.Collections;

public class WaterParticles : MonoBehaviour {	
	public int maxParticles = 4000; // How many particles
	public float range = 100.0f; // How far away before they are moved back into place
	public float distanceMove = 0.5f;	
	public float minParticleSize = 0.3f;
	public float maxParticleSize = 0.6f;
    private float sizeMultiplier = 1.0f;	
	private float minParticleSpeed = 0.0f;
    private float maxParticleSpeed = 1.0f;
	private float speedMultiplier = 1.0f;	
	public bool fadeParticles = true;
	private float distanceFade = 0.5f;
	
	private float _distanceToMove;
	private float _distanceToFade;
	private Transform _Transform;
	
	void Start ()
    {
		_Transform = transform;
		_distanceToMove = range * distanceMove;
		_distanceToFade = range * distanceFade;
		// Place all new particles in range of camera
		for (int i=0; i < maxParticles; i ++)
        {
			ParticleSystem.Particle _newParticle = new ParticleSystem.Particle();					
			_newParticle.position = _Transform.position + (Random.insideUnitSphere * _distanceToMove);
			_newParticle.remainingLifetime = Mathf.Infinity;
			Vector3 _velocity = new Vector3(Random.Range(minParticleSpeed, maxParticleSpeed) * speedMultiplier,Random.Range(minParticleSpeed, maxParticleSpeed) * speedMultiplier,Random.Range(minParticleSpeed, maxParticleSpeed) * speedMultiplier);
			_newParticle.velocity = _velocity;						
			_newParticle.startSize = Random.Range(minParticleSize, maxParticleSize) * sizeMultiplier;								
			GetComponent<ParticleSystem>().Emit(1);
		}			
	}
	
	void Update ()
    {
		int _numParticles = GetComponent<ParticleSystem>().particleCount;		
		ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[_numParticles];
		GetComponent<ParticleSystem>().GetParticles(_particles);
		for (int i = 0; i < _particles.Length; i++)
        {			
			// Calculate distance to particle from transform
			float _distance = Vector3.Distance(_particles[i].position, _Transform.position);			
			// If distance is greater
			if (_distance > range)
            {						
				// Move particle
				_particles[i].position = Random.onUnitSphere * _distanceToMove + _Transform.position;								
				_distance = Vector3.Distance(_particles[i].position, _Transform.position);
                Vector3 _velocity = new Vector3(Random.Range(minParticleSpeed, maxParticleSpeed) * speedMultiplier, Random.Range(minParticleSpeed, maxParticleSpeed) * speedMultiplier, Random.Range(minParticleSpeed, maxParticleSpeed) * speedMultiplier);
				_particles[i].velocity = _velocity;
				// Set size of particle
				_particles[i].startSize = Random.Range(minParticleSize, maxParticleSize) * sizeMultiplier;						
			}
			
			// If particle fading is enabled...
			if (fadeParticles)
            {
				// Get original color of the particle
				Color _col = _particles[i].startColor;				
				if (_distance > _distanceToFade)
                {		
					// Fade alpha value of particle
                    _particles[i].startColor = new Color(_col.r, _col.g, _col.b, Mathf.Clamp01(1.0f - ((_distance - _distanceToFade) / (_distanceToMove - _distanceToFade))));						
				} else
                {
					// Particle is within range so it is set to 1
                    _particles[i].startColor = new Color(_col.r, _col.g, _col.b, 1.0f);						
				}
			}
		}        
		// Set the particles to changes above and loop
		GetComponent<ParticleSystem>().SetParticles(_particles, _numParticles);    	
	}
}
