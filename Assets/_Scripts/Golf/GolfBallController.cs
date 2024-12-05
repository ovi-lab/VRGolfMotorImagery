using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GolfBallController : MonoBehaviour
{
    public UnityEvent OnBallStop;

    [SerializeField] private Transform holeTransform;

    private Vector3 targetPosition;
    private Rigidbody rb;
    private Vector3 startPosition;
    private bool isMoving;
    private float maxDistance;
    private float maxSpeed;
    private Phaser phaser;

    public Transform HoleTransform => holeTransform;
    public Phaser Phaser => phaser;

    private void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        phaser = GetComponent<Phaser>();
    }

    private void OnTriggerEnter(Collider other)
    {
        rb.velocity = Vector3.zero;
        isMoving = false;
        phaser.PhaseOut();
        StartCoroutine(EndAnimTime(phaser.AnimTime));
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);
        if(distance > 0.01f && (rb.velocity.magnitude > 0.005f || distance > 1f))
        {
            float easingFactor = 1 - Mathf.Exp(-distance * 0.22f);
            rb.velocity = (direction * (easingFactor * maxSpeed)).XZPlane(-1.2f); //hack to make falling look realistic
            // Debug.Log($"==={distance}==={rb.velocity.magnitude}===");
        }
        else
        {
            rb.velocity = Vector3.zero;
            isMoving = false;
            phaser.PhaseOut();
            StartCoroutine(EndAnimTime(phaser.AnimTime));
        }
    }

    public void FireBall(Vector3 position)
    {
        phaser.PhaseIn();
        StartCoroutine(AnimTime(phaser.AnimTime, position));
    }

    private IEnumerator AnimTime(float time, Vector3 position)
    {
        yield return new WaitForSeconds(time);
        ActuallyFireBall(position);
    }

    private IEnumerator EndAnimTime(float time)
    {
        yield return new WaitForSeconds(time);
        OnBallStop?.Invoke();
        ResetBall();
    }

    private void ActuallyFireBall(Vector3 position)
    {
        targetPosition = position;
        maxDistance = Vector3.Distance(transform.position, targetPosition);
        maxSpeed = maxDistance;
        isMoving = true;
    }

    private void OnDrawGizmos()
    {
        if (!isMoving) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(targetPosition, 0.03f);
    }

    public void ResetBall()
    {
        transform.position = startPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        isMoving = false;
        Debug.ClearDeveloperConsole();
    }
}
