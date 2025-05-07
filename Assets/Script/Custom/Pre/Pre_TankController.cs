using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pre_TankController : MonoBehaviour
{
	//for   AI
	public bool AI; //  AI ON/OFF
	public bool Turret_Turn; //Turn ON/OFF > 포탑을 회전 활성화 시킬지 여부
	public Transform[] targetPointsPos; //(enemy AI) Points for positions > AI 가 이동할 위치 목록 
	private byte sel_ltargetPointPos; //(enemy AI) selected targetPointPos in array > 현재 목표로 하는 위치 인덱스

	public float m_Speed = 8.0f; // How fast the tank moves forward and back. > 이동 속도
	public float m_TurnSpeed = 180f; // How fast the tank turns in degrees per second. > 회전 속도
	public int currentHealth = 50; //The tank's current health point total > 현재 체력

	public float m_PitchRange = 0.2f; // The amount by which the pitch of the engine noises can vary. > 엔진 소리 피치 랜덤 범위
	
	// 트랙 애니메이션용 y 위치 변수 = 현재 사용되지 않음
	private float TrackLeft_y;
	private float TrackRight_y;

	// 탱크 엔진 및 사망 소리
	public AudioClip tank_idle;
	public AudioClip tankDead;

	// 유니티 InputManager 에 설정된 축 이름
	private string m_MovementAxisName; // The name of the input axis for moving forward and back.
	private string m_TurnAxisName; // The name of the input axis for turning.
	
	private Rigidbody m_Rigidbody; // Reference used to move the tank. > 탱크 본체의 물리적용을 위한 Rigidbody
	
	// 현재 프레임의 이동/회전 입력 값 (플레이어용)
	private float m_MovementInputValue; // The current value of the movement input.
	private float m_TurnInputValue; // The current value of the turn input.
	
	// 원래 피치, 엔진 사운드 재생에 사용될 AudioSource
	private float m_OriginalPitch; // The pitch of the audio source at the start of the scene.
	private AudioSource m_MovementAudio; // Reference to the audio source used to play engine sounds.
	
	private Animator animator;
	
	private WaitForSeconds shotDuration = new WaitForSeconds(15f); // WaitForSeconds hide object > 탱크 파괴 후 삭제할 대기 시간
	
	// 폭발 이펙트, 현재 재생 중인 애니메이션 이름
	private ParticleSystem m_ExplosionParticles; //  the particles that will play on explosion.
	string animDo; //the animation used now

	
	
	// 포탑, 포신, 발사 지점 transform
	public Transform Turret;
	public Transform Barrel;
	public Transform GunEnd;

	private Transform TargetForTurn; // 포탑이 조준할 타겟
	private Vector3 TargetForTurnOld; // 타겟의 이전 위치
	private float TargetForTurnTimer; // 타겟이 멈춘 시간 누적
	
	public float EnemyRangeFire = 50; // 사정거리
	public Rigidbody shell; // 발사할 탄환 프리팹

	public float FireRate = 0.5f; // Number in seconds which controls how often the player can fire > 발사 간격
	public float speedShell = 80f; // 포탄 속도
	
	private float nextFire; // 발사 간격
	private Vector3 pos_barrel; // 포신 기본 위치 저장 변수


	// 상태 플래그
	private bool m_dead; // 사망 상태 
	private bool EnemyFire; // 발사 플래그

	private Quaternion target; // 포탑 회전 용 목표
	
	// 이펙트 및 사운드
	public ParticleSystem m_smokeBarrel;
	public ParticleSystem m_smokeDead;
	public ParticleSystem m_tankExplosion;
	public AudioSource m_AudioSource;
	public AudioClip soundFire;





	private void Awake()
	{
		animator = GetComponentInChildren<Animator>();
		m_Rigidbody = GetComponent<Rigidbody>();
		pos_barrel = Barrel.transform.localPosition; // 포신의 로컬 위치 저장 >> 발사 후 반동 애니메이션 처리에 활용

		m_MovementAudio = GetComponent<AudioSource>();

		m_MovementAudio.clip = tank_idle; // 오디오 소스 초기값 엔진소리로 설정




		//	m_ExplosionParticles=transform.FindChild ("Tank_Anim/TankExplosion").GetComponent<ParticleSystem> ().Play; 폭발 이펙트를 불러오는 코드였던 것으로 추정, 현재 사용되지 않음

	}


	private void OnEnable()
	{
		// 이동 및 회전 입력 값 초기화
		m_MovementInputValue = 0f;
		m_TurnInputValue = 0f;
	}


	private void OnDisable()
	{

	}
	
	private void Start()
	{
		TargetForTurn = transform; // 포탑이 조준할 타겟을 우선 자기 자신으로 설정
		if (!AI) // 플레이어일 경우
		{
			TargetForTurn = GameObject.Find("TargetMouse").transform; // TargetMouse 오브젝트를 찾아 타겟으로 삼음 >> 마우스 위치 조준용 대상
		}
		else // AI 일 경우
		{
			if (gameObject.tag == "Enemy") TargetForTurn = GameObject.FindGameObjectWithTag("Player").transform; // 자기 태그가 Enemy 일 경우 플레이어 찾음
			else TargetForTurn = GameObject.FindGameObjectWithTag("Enemy").transform; // 그 이외(플레이어) 라면 적을 찾음
		}

		// 탱크 앞뒤 이동 = 버티컬, 좌우 회전 = 호라이즌탈
		m_MovementAxisName = "Vertical";
		m_TurnAxisName = "Horizontal";

		// 현재 오디오소스의 피치(음높이) 저장
		m_OriginalPitch = m_MovementAudio.pitch;
	}
	
	// 포탄 발사, AI 이동, 플레이어 조작 감지에 따른 소리 및 애니메이션 설정
	private void Update()
	{
		if (m_dead) // 사망시 작동 안함
			return;

		if (!AI && Input.GetButtonDown("Fire1")) EnemyFire = true; // 마우스 왼쪽(Fire1) 클릭시 플래그를 true 로 변경(=발사 준비 완료)  

		if (EnemyFire && nextFire <= 0) // 발사 준비가 되어 있고 발사 쿨타임 중이 아니라면
		{
			EnemyFire = false; // 플래그를 false 하여 중복 발사 막음
			nextFire = FireRate; // 다음 발사까지 대기시간 설정
			fire(); // 발사
			m_smokeBarrel.Play(); // 발사 이펙트 재생
			m_AudioSource.PlayOneShot(soundFire); // 발사 소리 재생
		}

		if (nextFire > 0) // 발사 쿨타임이 도는 중이라면
		{
			nextFire -= 0.01f; // 0.01 초 감소 시킴
			Barrel.transform.localPosition = new Vector3(Barrel.transform.localPosition.x, pos_barrel.y + nextFire / 2,
				Barrel.transform.localPosition.z); // 포신이 살짝 위로 올라갔다 내려오는 애니메이션 연출 (쿨타임에 따라 포신 위치 조정)
		}

		//////////////// for Enemy AI //////////////// begin !! AI 이동관련 코드
		if (AI)
		{
			if (targetPointsPos.Length > 0)
			{
				var heading = transform.position - targetPointsPos[sel_ltargetPointPos].position;


				//move forward
				//heading.y = 0;  // This is the overground heading.
				if (heading.sqrMagnitude > 100)
				{
					//if the target is far move otherwise stand
					if (m_MovementInputValue < 1)
						m_MovementInputValue += 0.01f;
					//turn towards  
					Vector3 targetDir = targetPointsPos[sel_ltargetPointPos].position - transform.position;
					float step = 5.5f * Time.deltaTime;
					Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
					newDir.y = 0;
					transform.rotation = Quaternion.LookRotation(newDir);

				}
				else if (m_MovementInputValue > 0)
					m_MovementInputValue -= 0.01f;
				else
				{
					//The tank got to the target, choose another target position for movement
					m_MovementInputValue = 0;
					if (targetPointsPos.Length > 1)
						if (sel_ltargetPointPos < targetPointsPos.Length - 1)
							sel_ltargetPointPos++;
						else
							sel_ltargetPointPos = 0;


				}
			}
		}
		//////////////// for Enemy AI //////////////// end
		else // 플레이어 조작일 경우
		{
			// 키보드 입력값을 읽어옴
			m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
			m_TurnInputValue = Input.GetAxis(m_TurnAxisName);
		}

		EngineAudio(); // 엔진 소리 처리(움직임에 따라 엔진음 바뀜)

		if (animDo != "Idle" && m_MovementInputValue == 0 && m_TurnInputValue == 0) // Movement 에 아무런 입력이 없고, 현재 애니메이션이 Idle 이 아니라면
		{ 
			animDo = "Idle"; // 현재 애니메이션을 Idle 로 설정
			if (!Turret_Turn) animator.SetBool("Idle1", true); // 포탑 회전 중이 아니라면 Idle1 애니메이션 재생
			else animator.SetBool("Idle" + (int)Random.Range(1, 4), true); // 포탑 회전 중이라면 Idle1~4 중 랜덤 재생
		}

		// 포탑 회전 제어 코드 (타겟이 움직이지 않을 경우 포탑 머리를 제자리로 돌림)
		var dist = TargetForTurnOld - TargetForTurn.position; // 현재 타겟의 이전 위치와 현재 위치를 비교
		if (dist.sqrMagnitude > 0.01) TargetForTurnTimer = 0; // 타겟이 움직였으면 타이머를 초기화
		else TargetForTurnTimer += 1; // 가만히 있으면 타이머를 올림

		TargetForTurnOld = TargetForTurn.position; // 현재 타겟 위치 저장(다음 프레임과 비교를 위해)
	}

	// 엔진 소리 설정
	private void EngineAudio() 
	{
		// If there is no input (the tank is stationary)...
		if (Mathf.Abs(m_MovementInputValue) == 0 && Mathf.Abs(m_TurnInputValue) < 0.1f) // 이동이나 회전 입력이 거의 없다면
		{
			// ... change the clip to idling and play it.
			m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange); // 정지 엔진음이 조금씩 다르게 들리게 함(자연스로운 엔진음 유도)
		}
		else
			m_MovementAudio.pitch = 1 + Mathf.Abs(m_MovementInputValue); // 움직일 경우, 이동 속도에 비례해 엔진 소리 높이 올림
	}
	
	// 이동 및 회전, 사망시 애니메이션
	private void FixedUpdate()
	{
		if (m_dead) // 사망한 상태라면
		{
			transform.position = new Vector3(transform.position.x, transform.position.y - 0.002f, transform.position.z); // 탱크가 조금씩 바닥으로 가라앉는 효과 연출
			return;
		}
		
		// Adjust the rigidbodies position and orientation in FixedUpdate.
		Move();
		Turn();
	}


	// 이동 처리
	private void Move()
	{
		// 이동할 거리 계산
		Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime; // 전방 방향을 기준으로 입력값과 속도, 시간을 곱해 이동할 거리 계산

		// Rigidbody 의 위치를 계산 결과 만큼 이동시킴
		m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
		
		// 회전이 없을 경우
		if (m_TurnInputValue == 0)
			if (m_MovementInputValue > 0) // 앞으로 움직일 때 앞으로 움직이는 애니메이션 세팅
			{
				//Movement of the texture of the tank caterpillar
				if (animDo != "Move")
				{
					animDo = "Move";
					animator.SetBool("MoveForwStart", true);
				}
			}
			else if (m_MovementInputValue < 0) // 뒤로 움직일 때 뒤로 움직이는 애니메이션 세팅
			{
				//Movement of the texture of the tank caterpillar
				if (animDo != "Move")
				{
					animDo = "Move";
					animator.SetBool("MoveBackStart", true);

				}
			}
	}


	// 회전 처리
	private void Turn()
	{
		// 입력값에 따라 회전할 각도 계산
		float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
		
		if (turn != 0)
		{
			// Movement of the texture of the tank caterpillar

		}

		// Y축에만 적용되는 회전값 생성
		Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

		// 계산 값을 이용해 Rigidbody 회전
		m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);

		if (m_TurnInputValue > 0) // 오른쪽 회전일 경우 애니메이션 처리
		{
			//Movement of the texture of the tank caterpillar
			if (animDo != "TurnRight")
			{

				animDo = "TurnRight";
				if (m_MovementInputValue >= 0) animator.SetBool("TurnRight", true);
				else animator.SetBool("TurnLeft", true);
			}
		}
		else if (m_TurnInputValue < 0) // 왼쪽 회전을 경우 애니메이션 처리
		{
			//Movement of the texture of the tank caterpillar
			if (animDo != "TurnLeft")
			{

				animDo = "TurnLeft";
				if (m_MovementInputValue >= 0) animator.SetBool("TurnLeft", true);
				else animator.SetBool("TurnRight", true);
			}
		}
	}

	//Collider col = Physics.OverlapBox(enemyCheck.position, 0.6f, LayerEnemy);
	// 포탄 충돌 처리
	void OnTriggerEnter(Collider col)
	{
		if (m_dead) return; // dead 상태라면 충돌을 무시
		if (col.gameObject.tag == "Shell") // 충돌한 물체가 포탄일 경우
		{
			// 해당 포탄의 설정 데미지 만큼 자기 자신에게 데미지 입힘
			SCT_Shell shell = col.GetComponent<SCT_Shell>();
			Damage(shell.shellDamage);

			// 탱크 체력이 남아있을 경우, 피격 애니메이션 재생
			if (currentHealth > 0)
			{
				var a = (int)Random.Range(1, 5);
				if (a == 1) animator.SetBool("HitLeft", true);
				if (a == 2) animator.SetBool("HitRight", true);
				if (a == 3) animator.SetBool("HitForw", true);
				if (a == 4) animator.SetBool("HitBack", true);
				if (a == 5) animator.SetBool("HitStrong", true);
				animDo = "Hit";
			}
		}
	}

	// 데미지 처리
	public void Damage(int damageAmount)
	{
		//subtract damage amount when Damage function is called
		currentHealth -= damageAmount;
		
		// 현재 체력이 0 이하일 경우
		if (currentHealth <= 0)
		{
			// 사망처리
			m_dead = true;
			animator.SetBool("Dead" + (int)Random.Range(1, 5), true);
			animDo = "Dead";
			m_MovementAudio.loop = false; // 엔진 소리 반복 끄기
			m_MovementAudio.pitch = 1; // 피치 기본으로 맞추기
			m_MovementAudio.clip = tankDead; // 오디오 클립 사망 엔진음으로 교체
			m_MovementAudio.Play(); // 재생
			GetComponent<BoxCollider>().enabled = false; // 사망한 탱크 본체의 collider 비활성화
			transform.gameObject.tag = "Respawn"; // 탱크 오브젝트의 태그를 바꿔 AI 타겟팅에서 제외
			Destroy(GetComponent<Rigidbody>()); // Rigidbody 삭제

			// 사망 펙트 재생
			m_smokeDead.Play(); 
			m_tankExplosion.Play();


			StartCoroutine(hideTnak()); // 일정 시간 이후 죽은 탱크를 숨기는 코루틴 시작
		}
	}

	// 포탑 회전 처리
	void LateUpdate()
	{

		if (m_dead) return;
		//////////////// for Enemy AI //////////////// begin !! AI 포탑 타겟팅 로직
		if (AI)
		{
			if (TargetForTurn.gameObject.tag == "Respawn")
				return;

			var heading = Turret.transform.position - TargetForTurn.position;
			if (heading.sqrMagnitude < EnemyRangeFire && heading.sqrMagnitude > 1)
			{
				//if the enemy tank is far move otherwise stand
				EnemyFire = true;
			}
		}
		//////////////// for Enemy AI //////////////// end
		//turn head for mouse

		if (Turret_Turn) // 포탑이 회전 중이라면
		{
			if (TargetForTurn) // 타겟이 존재 한다면
				if (TargetForTurnTimer < 300) // 타겟이 최근 움직였다면
				{
					// 포탑 방향을 타겟을 향해 돌려줌
					Vector3 targetDir = TargetForTurn.position - Turret.transform.position;
					Vector3 newDir = Vector3.RotateTowards(Turret.transform.forward, targetDir, 1, 0.0F);

					target = Quaternion.LookRotation(newDir);

					Turret.transform.rotation = Quaternion.Euler(-90, target.eulerAngles.y, 0);

				}
				else if (TargetForTurnTimer < 400) // 타겟이 오랫동안 가만히 있을 경우
				{
					// 포탑 방향을 탱크 본체 방향으로 돌림
					Turret.transform.rotation = Quaternion.RotateTowards(Turret.transform.rotation,
						Quaternion.Euler(-90, transform.eulerAngles.y, 0), 4f);
				}
		}
		else // 포탑이 회전 중이 아니라면, 기본 포탑 위치로 세팅
		{
			//	Turret.transform.rotation = transform.rotation;
			Turret.transform.rotation = Quaternion.Euler(transform.eulerAngles.x - 90, transform.eulerAngles.y,
				transform.eulerAngles.z);
		}

	}

	// 발사 처리
	void fire()
	{
		Rigidbody shellInstance = Instantiate(shell, GunEnd.position, Turret.rotation) as Rigidbody; // GunEnd 위치에서 포탄 프리팹 생성
		shellInstance.velocity = speedShell * -Turret.transform.up; // 포탄이 포탑의 아래 방향으로 날아가게 설정(포탑 모델이 아랫 방향을 바라보게 되어 있으므로 -transform.up 사용)
	}

	// 사망한 탱크를 숨김 처리 하는 코루틴
	private IEnumerator hideTnak()
	{
		//Wait for 15 seconds
		yield return shotDuration;
		//hide tank
		gameObject.SetActive(false);
	}
}
