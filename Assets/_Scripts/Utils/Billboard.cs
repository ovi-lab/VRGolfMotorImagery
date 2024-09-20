using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private bool isEnabled = true;
    [SerializeField] private Transform cam;

    private void Awake()
    {
        if (cam != null) return;
        if (Camera.main == null) return;
        cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (!isEnabled) return;
        if(cam == null) Debug.LogWarning("Camera transform not assigned!!");
        transform.LookAt(cam);
        transform.Rotate(0, 180, 0);
    }
}