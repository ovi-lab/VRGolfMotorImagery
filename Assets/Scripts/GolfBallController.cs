using UnityEngine;
using UnityEngine.Serialization;


public class GolfBallController : MonoBehaviour
{
    [SerializeField] private float speedFactor = 1.0f;
    [SerializeField] private float gravity = 9.8f;
    [FormerlySerializedAs("hole")] [SerializeField] private FinalListGenerator finalListGenerator; 
    [SerializeField] private DataSaver dataSaver;
    [SerializeField] private Renderer ballRenderer;
    [SerializeField, Tooltip("Set this to the y value of the golf ball and hole")] 
    private float floorHeight;
    
    
    [Header("Feedback Groups")]
    [SerializeField] public int feedbackGroup; // 1 = Perfect, 2 = Random, 3 = Adaptive
    
    [Header("Parameters")]   
    private new Rigidbody rigidbody;
    private bool hasFired;
    private bool hasSelectedRandomTarget;
    private Vector3 targetDirectionNormalized;
    private new Collider collider;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private int currentIndex;
    public float actualSpeed { get; private set; }
    [SerializeField]private bool hasFallen;
    public bool holeDetectionTrigger;
    
    void Start()
    {
        startPosition = transform.position;
        targetDirectionNormalized = Vector3.zero;
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        targetPosition = GetTargetPosition();
    }

    void Update()
    {
        if (!hasFired)  
        {
            hasFired = true;
        }   
        MoveBall(); 
    }

    public void Reset()
    {
        hasFired = false;
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
        rigidbody.velocity = Vector3.zero; 
        hasFallen = false;
        collider.isTrigger = false; 
        targetPosition = GetTargetPosition();
    }

    private void PrepareNextTrial()
    {
        
        Vector2 currCoordinate = finalListGenerator.finalList[currentIndex];
        targetPosition = new Vector3(currCoordinate.x, floorHeight, currCoordinate.y);
        currentIndex++;
        if (Mathf.Approximately(targetPosition.x, finalListGenerator.transform.position.x) && Mathf.Approximately(targetPosition.z, finalListGenerator.transform.position.z))
        {
            holeDetectionTrigger = true;
        }
    }

    public void Fall(bool state)
    {
        collider.isTrigger = state;
        rigidbody.velocity += Vector3.down * gravity;
        hasFallen = true;
    }

    private void MoveBall()
    {
        if (!hasFired || hasFallen) return;
        
        Vector3 targetDirectionVector = (targetPosition - transform.position);
        float targetDirectionVectorMagnitude = targetDirectionVector.magnitude;
        targetDirectionNormalized = targetDirectionVector.normalized;
        SetActualSpeed(targetDirectionNormalized, targetDirectionVectorMagnitude, speedFactor);
    }

    private Vector3 GetTargetPosition()
    {
        PrepareNextTrial();
        return targetPosition;
    }
    
    private void SetActualSpeed(Vector3 targetDirectionNormalized, float directionVectorMagnitude, float speedFactor)
    {
        actualSpeed = speedFactor * directionVectorMagnitude;
        rigidbody.velocity = targetDirectionNormalized * actualSpeed;
    }
}
