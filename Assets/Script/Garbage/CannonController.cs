using UnityEngine;

public class CannonController : MonoBehaviour
{
    public Transform head;      // 동체
    public Transform barrel;    // 포신 (Sphere)
    public Transform firePoint; // 발사 위치 (Lunch)

    public GameObject projectilePrefab;
    public float rotationSpeed = 50f;
    public float minAngle = -30f;
    public float maxAngle = 60f;

    public float maxPower = 1000f;
    public float chargeRate = 500f;
    private float currentPower = 0f;

    void Update()
    {
        // 동체 회전 (Y축)
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        head.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime);

        // 포신 회전 (X축) - 제한된 각도로
        float vertical = Input.GetAxis("Vertical"); // W/S
        Vector3 currentRotation = barrel.localEulerAngles;
        currentRotation.x -= vertical * rotationSpeed * Time.deltaTime;
        currentRotation.x = ClampAngle(currentRotation.x, minAngle, maxAngle);
        barrel.localEulerAngles = currentRotation;

        // 발사 힘 충전
        if (Input.GetKey(KeyCode.Space))
        {
            currentPower += chargeRate * Time.deltaTime;
            currentPower = Mathf.Clamp(currentPower, 0f, maxPower);
        }

        // 포탄 발사
        if (Input.GetKeyUp(KeyCode.Space))
        {
            FireProjectile();
            currentPower = 0f;
        }
    }

    void FireProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.AddForce(firePoint.forward * currentPower); // 발사 방향은 총구의 forward 방향
    }

    float ClampAngle(float angle, float min, float max)
    {
        angle = (angle > 180) ? angle - 360 : angle; // -180~180 기준으로 정리
        return Mathf.Clamp(angle, min, max);
    }
}