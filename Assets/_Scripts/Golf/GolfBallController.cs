using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GolfBallController : MonoBehaviour
{
    [SerializeField] private Transform holeTransform;
    [SerializeField] private TextMeshProUGUI tmp;


    public Transform HoleTransform => holeTransform;


    private Vector3 targetPosition;
    private Rigidbody rb;
    private Vector3 startPosition;
    private bool isMoving;
    private float maxDistance;
    private float maxSpeed;

    private float startTime;

    private void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();

        List<float> values = RandomValueGenerator.GenerateValues(27, 0.775f, 0.01f, 1);
        Debug.Log(values.Average());
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);
        if(distance > 0.05f)
        {
            float easingFactor = 1 - Mathf.Exp(-distance * 0.3f);
            rb.velocity = (direction * (easingFactor * maxSpeed)).XZPlane(-1.2f); //hack to make falling look realistic
        }
        else
        {
            rb.velocity = Vector3.zero;
            isMoving = false;
        }

    }

    public void FireBall(Vector3 position)
    {
        targetPosition = position;
        maxDistance = Vector3.Distance(transform.position, targetPosition);
        tmp.text = "[DEBUG]\nPerfect Trial";
        maxSpeed = maxDistance / 2f;
        isMoving = true;
        startTime = Time.time;
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
