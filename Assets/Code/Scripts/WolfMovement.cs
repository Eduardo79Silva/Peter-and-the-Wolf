using UnityEngine;

public class WolfMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float turnSpeed = 10f;
    public Camera mainCamera;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Handle movement input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        // Move the Wolf
        if (moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * turnSpeed);

            rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.deltaTime);
        }

        
    }

    void LateUpdate(){
        // Camera follow logic
        CameraFollow();
    }

    void CameraFollow()
    {
        // Keep the camera following the Wolf smoothly
        if (mainCamera != null)
        {
            Vector3 newCameraPos = transform.position + new Vector3(0f, 10f, -10f);
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, newCameraPos, 0.1f);
            mainCamera.transform.LookAt(transform);
        }
    }
}
