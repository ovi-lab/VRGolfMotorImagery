using System.Collections;
using UnityEngine;

public class GolfBallController : MonoBehaviour
{
    [SerializeField] private Transform holeTransform;
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
        rb.AddForce(Vector3.down * 9.8f);
        rb.AddForce(-rb.velocity * extraFriction);
    }

    public void FireBall()
    {
        float distance = Vector3.Distance(transform.position, holeTransform.position);
        float velocityMagnitude = forceMultiplier * distance;
        Vector3 direction = (holeTransform.position - transform.position).normalized;
        rb.AddForce(velocityMagnitude * direction, ForceMode.Force);
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
