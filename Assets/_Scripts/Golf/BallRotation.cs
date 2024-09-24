using UnityEngine;

public class BallRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (rb.velocity.magnitude > 0.001f)
        {
            Vector3 movementDirection = rb.velocity.normalized;
            float speed = rb.velocity.magnitude;
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, movementDirection);
            transform.Rotate(rotationAxis, speed * rotationSpeed * Time.deltaTime);
        }
    }
}