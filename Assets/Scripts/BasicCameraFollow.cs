using UnityEngine;
using System.Collections;

public class BasicCameraFollow : MonoBehaviour 
{

    public Transform followTarget;
    public float moveSpeed = 5f;

    private Vector3 shakeOffset = Vector3.zero; 
    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        if (followTarget != null)
        {
            Vector3 targetPos = new Vector3(followTarget.position.x, followTarget.position.y, transform.position.z);
            Vector3 finalPos = targetPos + shakeOffset;

            transform.position = Vector3.SmoothDamp(transform.position, finalPos, ref velocity, 0.1f);
        }
    }
    public void SetShakeOffset(Vector3 offset)
    {
        shakeOffset = offset;
    }
}

