using UnityEngine;
using System.Collections;
//This script executes commands to change character animations
[RequireComponent (typeof (Animator))]
public class SCT_Actions : MonoBehaviour {

 
	private Animator animator;
 
	void Awake () {
		animator = GetComponent<Animator> ();

 
 

 
    }
 
 

	public void HitLeft()
	{
		animator.SetBool ("HitLeft", true);
	}
	public void HitRight()
	{
		animator.SetBool ("HitRight", true);
	}
	public void HitForw()
	{
		animator.SetBool ("HitForw", true);
	}
	public void HitBack()
	{
		animator.SetBool ("HitBack", true);
	}
	public void HitStrong()
	{
		animator.SetBool ("HitStrong", true);
	}
	public void Dead1()
	{
		animator.SetBool ("Dead1", true);
	}
	public void Dead2()
	{
		animator.SetBool ("Dead2", true);
	}
	public void Dead3()
	{
		animator.SetBool ("Dead3", true);
	}
	public void Dead4()
	{
		animator.SetBool ("Dead3", true);
	}


	public void MoveForwStart()
	{
		animator.SetBool ("MoveForwStart", true);
	}
	public void MoveBackStart()
	{
		animator.SetBool ("MoveBackStart", true);
	}

	public void TurnLeft()
	{
		animator.SetBool ("TurnLeft", true);
	}
	public void TurnRight()
	{
		animator.SetBool ("TurnRight", true);
	}
	public void Idle1()
	{
		animator.SetBool ("Idle1", true);
	}
	public void Idle2()
	{
		animator.SetBool ("Idle2", true);
	}
	public void Idle3()
	{
		animator.SetBool ("Idle3", true);
	}
 

}
