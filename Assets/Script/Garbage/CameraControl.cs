using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour 
{
	public Transform target; // 카메라가 따라갈 대상
	public Transform TargetMouse; // 마우스 위치를 따라 움직일 오브젝트
	private Vector3 m_MoveVelocity; // 카메라 이동을 부드럽게 만드는 데 필요한 임시 벡터
	private Vector3 m_DesiredPosition; // 카메라가 목표로 하는 위치 (사실상 지금은 안 쓰임)
	private Plane plane; // 마우스 클릭 위치를 잡기 위해 만든 가상의 평면
	public Camera cam; // 카메라 컴포넌트
	// Use this for initialization
	void Start () 
	{
		plane = new Plane(Vector3.up, Vector3.zero); // 마우스 커서 위치를 3D 공간에 매핑할 때 쓸 평면 생성
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		if (Input.GetAxis("Mouse ScrollWheel") > 0) // 스크롤 업 일 경우 카메라 줌 인
		{
			cam.orthographicSize=cam.orthographicSize-5;
		}

		if (Input.GetAxis("Mouse ScrollWheel") < 0) // 스크롤 다운일 경우 스크롤 줌 아웃
		{
			cam.orthographicSize=cam.orthographicSize+5;
		}

		// 마우스 커서 위치 계산
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 화면에 ray 발사
		float rayDistance; // 
		if (plane.Raycast(ray, out rayDistance)) // 카메라에서 쏜 ray 가 Plane 과 어디서 만나는지 계산하여 교차점이 있다면
			TargetMouse.position = ray.GetPoint(rayDistance); // 마우스 커서가 가리키는 지점에 TargetMouse 오브젝트 위치시킴

	    // 카메라가 플레이어를 따라가게 처리
		if (target) 
		{
			Vector3 trg = new Vector3 (target.transform.position.x - 16.4f, transform.position.y, target.transform.position.z - 12.4f);
			transform.position = Vector3.SmoothDamp (transform.position, trg, ref m_MoveVelocity, 0.2f);
		}
	}
}
