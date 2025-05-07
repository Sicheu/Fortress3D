using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCT_CameraControl : MonoBehaviour {
	public Transform target;
	public Transform TargetMouse;
	private Vector3 m_MoveVelocity;                 // Reference velocity for the smooth damping of the position.
	private Vector3 m_DesiredPosition;              // The position the camera is moving towards.
	private Plane plane;
	public Camera cam;
	// Use this for initialization
	void Start () {
		plane= new Plane(Vector3.up, Vector3.zero);	
	 
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			cam.orthographicSize=cam.orthographicSize-5;
		}

		if (Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			cam.orthographicSize=cam.orthographicSize+5;
		}

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float rayDistance;
		if (plane.Raycast(ray, out rayDistance))
			TargetMouse.position = ray.GetPoint(rayDistance);

	 //	transform.position = new Vector3 (target.transform.position.x-2.4f, transform.position.y, target.transform.position.z );
		if (target) {
			Vector3 trg = new Vector3 (target.transform.position.x - 16.4f, transform.position.y, target.transform.position.z - 12.4f);
			transform.position = Vector3.SmoothDamp (transform.position, trg, ref m_MoveVelocity, 0.2f);
		}
	}
}
