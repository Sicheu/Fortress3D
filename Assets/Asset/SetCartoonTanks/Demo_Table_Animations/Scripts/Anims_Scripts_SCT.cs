using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anims_Scripts_SCT : MonoBehaviour {
	public Transform Shell;
	public Transform Gun_EndR;
	public Transform Gun_EndL;
  
	public ParticleSystem[] p_jet;       
	private bool restartRes=true;  
	private float shellSpeed=500;
	private Vector3 pos_side;

	public ParticleSystem p_hit, p_dead, p_smoke, p_fireL, p_fireSmokeL, p_fireR, p_fireSmokeR;    //Particle effect  
	private AudioSource m_AudioSource;   
	public AudioClip s_Fire, s_hit, s_dead,s_signal,s_say;   	//Sound effect 
	// Use this for initialization
	void Start () {
		m_AudioSource = GetComponent<AudioSource> ();		
	}

	void  f_hit() //hit
	{
		p_hit.Play();
		m_AudioSource.PlayOneShot(s_hit);
 
	}
 
	void  f_afterFire()  
	{
		p_fireSmokeL.Play();
		p_fireSmokeR.Play();
	}

	void  f_start()  
	{
		if (!restartRes) {
			restartRes = true;
			m_AudioSource.loop=true;
			m_AudioSource.Play();

			for (int i = 0; i < p_jet.Length ; i++){ 
				p_jet[i].Play();
			}
		} 
	}
	void  f_prevDead()  
	{
 
		m_AudioSource.PlayOneShot(s_signal);
	}
 
	void  f_dead() //dead
	{
		for (int i = 0; i < p_jet.Length ; i++){ 
				p_jet[i].Stop();
		}

		p_dead.Play();
		p_smoke.Play();
		m_AudioSource.PlayOneShot (s_dead);
		m_AudioSource.loop=false;
		restartRes = false;


	}
 
	void  f_fire(int side) //shot 
	{
		if (side == 1) {
			p_fireR.Play();
			pos_side = Gun_EndR.transform.position;
		} else {
			p_fireL.Play();
			pos_side = Gun_EndL.transform.position;
		}
		var gameOb = (Transform)Instantiate (Shell, pos_side, Quaternion.LookRotation(-transform.forward)); 
		 
		Vector3 dir =-transform.right*shellSpeed;
		gameOb.GetComponent<Rigidbody>().AddForce( dir);

		m_AudioSource.PlayOneShot (s_Fire); 
 
	}
 
}
 