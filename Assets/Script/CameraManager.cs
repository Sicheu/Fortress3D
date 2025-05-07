using System.Collections;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [Header("Camera References")]
    public Camera thirdPersonCamera;
    public Camera worldViewCamera;

    [Header("Targets")]
    private Transform player;
    private Transform currentTarget;

    [Header("World View Settings")]
    public float dampingTime = 0.2f;
    private Vector3 moveVelocity;
    private Vector3 desiredPosition;

    public float zoomStep = 5f;
    public float minOrthographicSize = 10f;
    public float maxOrthographicSize = 60f;

    private Coroutine followProjectileRoutine;
    private bool isThirdPerson = true;

    private bool isFollowingProjectile = false; // 포탄 추적중임을 알리는 변수
    private float originalSize; // 포탄 발사 이후 돌릴 기존의 카메라 사이즈 저장용 변수
    
    private bool isFollowingTarget = true;
    private Vector3 lastMousePosition;

    void Start()
    {
        ActivateWorldViewCamera();
    }

    void Update()
    {
        // 카메라 전환 탭
        if (Input.GetKeyDown(KeyCode.Tab) && !isFollowingProjectile)
        {
            if (isThirdPerson)
                ActivateWorldViewCamera();
            else
                ActivateThirdPersonCamera();
        }

        // 월드뷰 카메라 줌 (정사영 줌)
        if (worldViewCamera.enabled)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
            {
                worldViewCamera.orthographicSize = Mathf.Max(minOrthographicSize, worldViewCamera.orthographicSize - zoomStep);
            }
            else if (scroll < 0f)
            {
                worldViewCamera.orthographicSize = Mathf.Min(maxOrthographicSize, worldViewCamera.orthographicSize + zoomStep);
            }
        }
        
        MoveCamera();
    }

    private void MoveCamera()
    {
        // 우클릭 드래그로 카메라 수동 이동
        if (worldViewCamera.enabled && Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (worldViewCamera.enabled && Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            delta *= 0.1f; // 감도 조절

            // 카메라 기준의 오른쪽, 앞 방향으로 이동
            Vector3 move = (worldViewCamera.transform.right * -delta.x) +
                           (worldViewCamera.transform.forward * -delta.y);

            // 평면 이동만 하도록 y 제거
            move.y = 0;

            worldViewCamera.transform.position += move;
            lastMousePosition = Input.mousePosition;
            isFollowingTarget = false;
        }
        // else if (worldViewCamera.enabled && Input.GetMouseButton(1))
        // {
        //     Vector3 delta = Input.mousePosition - lastMousePosition;
        //
        //     // 마우스 드래그 방향대로 이동 (스크린 → 월드 변환)
        //     Vector3 worldDelta =
        //         worldViewCamera.ScreenToWorldPoint(new Vector3(0, 0, worldViewCamera.transform.position.y)) -
        //         worldViewCamera.ScreenToWorldPoint(new Vector3(delta.x, delta.y, worldViewCamera.transform.position.y));
        //     
        //     // y 고정
        //     Vector3 newPosition = worldViewCamera.transform.position + worldDelta;
        //     newPosition.y = currentTarget != null ? currentTarget.position.y + 15f : worldViewCamera.transform.position.y;
        //
        //     worldViewCamera.transform.position = newPosition;
        //
        //     lastMousePosition = Input.mousePosition;
        //     isFollowingTarget = false;
        // }
        
        // WASD, 스페이스 입력 시 다시 타겟 추적
        if (worldViewCamera.enabled && !isFollowingTarget)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.Space))
            {
                isFollowingTarget = true;
                currentTarget = player;
            }
        }
    }

    void FixedUpdate()
    {
        // if (worldViewCamera.enabled && currentTarget != null)
        // {
        //     // 기존 월드 뷰 카메라 위치 계산 방식 적용
        //     Vector3 targetPos = new Vector3(currentTarget.position.x - 16.4f, worldViewCamera.transform.position.y, currentTarget.position.z - 12.4f);
        //     worldViewCamera.transform.position = Vector3.SmoothDamp(worldViewCamera.transform.position, targetPos, ref moveVelocity, dampingTime);
        // }
        
        if (worldViewCamera.enabled && isFollowingTarget && currentTarget != null)
        {
            float fixedY = currentTarget.position.y + 15f;

            Vector3 targetPos = new Vector3(
                currentTarget.position.x - 16.4f,
                fixedY,
                currentTarget.position.z - 12.4f
            );

            worldViewCamera.transform.position = Vector3.SmoothDamp(
                worldViewCamera.transform.position,
                targetPos,
                ref moveVelocity,
                dampingTime
            );
        }
    }

    public void ActivateThirdPersonCamera()
    {
        thirdPersonCamera.enabled = true;
        worldViewCamera.enabled = false;
        isThirdPerson = true;
    }

    public void ActivateWorldViewCamera()
    {
        thirdPersonCamera.enabled = false;
        worldViewCamera.enabled = true;
        currentTarget = player;
        isThirdPerson = false;
    }
    
    
    public void FollowProjectile(Transform projectile, float delayAfterExplosion = 2f)
    {
        if (followProjectileRoutine != null)
            StopCoroutine(followProjectileRoutine);

        followProjectileRoutine = StartCoroutine(FollowProjectileWhenExplodes(projectile, delayAfterExplosion));
    }
    
    private IEnumerator FollowProjectileWhenExplodes(Transform projectile, float delay)
    {
        isFollowingProjectile = true;

        ActivateWorldViewCamera();
        currentTarget = projectile;

        // 현재 줌 값을 저장하고 고정값으로 설정
        originalSize = worldViewCamera.orthographicSize;
        worldViewCamera.orthographicSize = 25f;

        if (projectile.TryGetComponent(out Custom_Shell shell))
        {
            bool hasExploded = false;
            shell.onExplosion = () => hasExploded = true;
            yield return new WaitUntil(() => hasExploded);
            yield return new WaitForSeconds(delay);
        }
        else
        {
            yield return new WaitForSeconds(delay);
        }

        // 시야 크기 복원
        worldViewCamera.orthographicSize = originalSize;

        currentTarget = player;
        isFollowingProjectile = false;
    }

    public void TurnChange(Camera cam, Transform target)
    {
        // 기존 플레이어의 3인칭 카메라 비활성화
        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.enabled = false;
        }
        
        // 다음 플레이어 3인칭 카메라 등록
        thirdPersonCamera = cam;
        
        // 월드뷰 카메라 타겟 변경
        player = target;
        
        ActivateWorldViewCamera();
    }
}
