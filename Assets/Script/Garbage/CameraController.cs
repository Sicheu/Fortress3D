using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraMode
{
    WorldView,
    PlayerFollow,
    ProjectileFollow
}

public class CameraController : MonoBehaviour
{
    // public CameraMode currentMode;
    // public Transform target;
    // public Vector3 playerOffset = new Vector3(0, 5, -10);
    // public Vector3 projectileOffset = new Vector3(0, 2, -5);
    //
    // public float moveSpeed = 10f;
    // public float zoomSpeed = 500f;
    // public float minZoom = 20f;
    // public float maxZoom = 120f;
    //
    // // 추가: 월드뷰 설정
    // public float edgeScrollSpeed = 10f;
    // public float edgeSize = 30f;
    // public Vector2 xBounds = new Vector2(-100, 100);
    // public Vector2 zBounds = new Vector2(-100, 100);
    //
    // // 카메라 효과
    // public float transitionSpeed = 2f; // 부드러운 이동 속도
    // private Vector3 targetPosition;
    // private bool isTransitioning = false;
    // private bool isShaking = false;
    
    public CameraMode currentMode;
    public Transform target;

    [Header("Offsets")]
    public Vector3 playerOffset = new Vector3(0, 5, -10);
    public Vector3 projectileOffset = new Vector3(0, 2, -5);

    [Header("World View Movement")]
    public float moveSpeed = 10f;
    public float edgeScrollSpeed = 10f;
    public float edgeSize = 30f;
    public Vector2 xBounds = new Vector2(-100, 100);
    public Vector2 zBounds = new Vector2(-100, 100);
    private Vector3 smoothVelocity;

    [Header("World View Zoom")]
    public float zoomSpeed = 100f;
    public float minZoom = 15f; // 최소 높이 (Y값)
    public float maxZoom = 60f; // 최대 높이 (Y값)

    [Header("Camera Transition")]
    public float transitionSpeed = 2f;
    private Vector3 targetPosition;
    private bool isTransitioning = false;

    [Header("Camera Shake")]
    private bool isShaking = false;

    private void LateUpdate()
    {
        if (isTransitioning)
        {
            // 부드럽게 이동
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                isTransitioning = false;
        }
        else
        {
            switch (currentMode)
            {
                case CameraMode.WorldView:
                    ControlWorldView();
                    break;
                case CameraMode.PlayerFollow:
                    ControlPlayerFollow();
                    break;
                case CameraMode.ProjectileFollow:
                    ControlProjectileFollow();
                    break;
            }
        }
    }

    public void ChangeCameraMode(CameraMode mode, Transform newTarget = null)
    {
        currentMode = mode;
        if (newTarget != null)
            target = newTarget;

        UpdateTargetPosition(); // 추가
    }

    private void ControlWorldView()
    {
        if (target == null)
            return;
        
        if (IsPlayerMoving())
        {
            FollowPlayerInWorldView();
        }
        else
        {
            EdgeScroll();
        }

        // // 줌은 항상 적용
        // float scroll = Input.GetAxis("Mouse ScrollWheel");
        // Camera.main.fieldOfView -= scroll * zoomSpeed * Time.deltaTime;
        // Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, minZoom, maxZoom);
        
        // 줌: 카메라 높이 조절 (Y축 이동)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 pos = transform.position;
        pos.y -= scroll * zoomSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom);
        transform.position = pos;
        
        // 카메라 각도: RTS 스타일로 고정 (위에서 아래 사선 시점)
        transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }

    private bool IsPlayerMoving()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
               Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
    }

    private void FollowPlayerInWorldView()
    {
        if (target == null) return;
        
        Vector3 offsetPos = new Vector3(target.position.x - 16.4f, transform.position.y, target.position.z - 12.4f);
        transform.position = Vector3.SmoothDamp(transform.position, offsetPos, ref smoothVelocity, 0.2f);
    }

    private void EdgeScroll()
    {
        Vector3 move = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;

        if (mousePos.x >= Screen.width - edgeSize)
            move += Vector3.right;
        if (mousePos.x <= edgeSize)
            move += Vector3.left;
        if (mousePos.y >= Screen.height - edgeSize)
            move += Vector3.forward;
        if (mousePos.y <= edgeSize)
            move += Vector3.back;

        move.Normalize();

        Vector3 targetPos = transform.position + move * edgeScrollSpeed * Time.deltaTime;
        targetPos.x = Mathf.Clamp(targetPos.x, xBounds.x, xBounds.y);
        targetPos.z = Mathf.Clamp(targetPos.z, zBounds.x, zBounds.y);

        transform.position = targetPos;
    }

    private void ControlPlayerFollow()
    {
        if (target == null) return;
        Vector3 targetPos = target.position + playerOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
        transform.LookAt(target);
    }

    private void ControlProjectileFollow()
    {
        if (target == null) return;
        transform.position = target.position + projectileOffset;
        transform.LookAt(target);
    }
    
    private void UpdateTargetPosition()
    {
        switch (currentMode)
        {
            case CameraMode.PlayerFollow:
                if (target != null)
                    targetPosition = target.position + playerOffset;
                break;
            case CameraMode.ProjectileFollow:
                if (target != null)
                    targetPosition = target.position + projectileOffset;
                break;
            case CameraMode.WorldView:
                if (target != null)
                    targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
                break;
        }

        isTransitioning = true;
    }
    
    public void ShakeCamera(float duration, float magnitude)
    {
        if (!isShaking)
            StartCoroutine(Shake(duration, magnitude));
    }
    
    private IEnumerator Shake(float duration, float magnitude)
    {
        isShaking = true;

        Vector3 originalPos = transform.position;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.position = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
        isShaking = false;
    }
}
