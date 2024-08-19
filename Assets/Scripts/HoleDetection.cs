using UnityEngine;
using System.Collections;

public class HoleDetection : MonoBehaviour
{
    [SerializeField] private float timeToDrop = 0.1f;
    
    private void OnTriggerEnter(Collider other)
    {
        GolfBallController golfBall = other.GetComponent<GolfBallController>();
        Debug.Log(golfBall.holeDetectionTrigger);
        if (golfBall != null && golfBall.holeDetectionTrigger)
        {
            StartCoroutine(WaitForBallToStop(golfBall));
        }
    }
    private IEnumerator WaitForBallToStop(GolfBallController ball)
    {
        yield return new WaitForSeconds(timeToDrop);
        ball.Fall(true);
    }
}