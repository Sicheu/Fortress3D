using UnityEngine;
using System.Collections;
//This script executes commands to change character animations
[RequireComponent (typeof (Animator))]
public class SCAirCrafts_Actions : MonoBehaviour {

 
	private Animator animator;
 
	void Awake () {
		animator = GetComponent<Animator> ();

    }
 
 


	public void Dead1()
	{
		animator.SetBool ("Dead1", true);
	}
	public void Dead2()
	{
		animator.SetBool ("Dead2", true);
	}
	public void TakeOff()
	{
		animator.SetBool ("TakeOff", true);
	}
	public void Idle()
	{
		animator.SetBool ("Idle", true);
	}
	public void Flight()
	{
		animator.SetBool ("Flight", true);
	}


}
