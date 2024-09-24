using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class GolfBallController : MonoBehaviour
{
    [SerializeField] private Transform holeTransform;

    public Transform HoleTransform => holeTransform;

    [SerializeField] private float forceMultiplier;
    [SerializeField] private float extraFriction;

    private Rigidbody rb;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // rb.AddForce(-rb.velocity * extraFriction);
    }

    public void FireBall(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        Vector3 targetDirection = (targetPosition - transform.position).normalized;
        float initVelocityMag = Mathf.Sqrt(2 * extraFriction * rb.mass * 9.8f * distance);
        rb.velocity = Vector3.zero;
        rb.velocity = initVelocityMag * targetDirection;
        StartCoroutine(Logger(6.5f));
    }

    private IEnumerator Logger(float time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log("In Hole?");
    }

    public void ResetBall()
    {
        transform.position = startPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}
