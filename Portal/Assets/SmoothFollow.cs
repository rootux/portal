using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;
 
    public float smoothSpeed = 0.5f;
 
    public Vector3 offset;
 
    private void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
 
        transform.LookAt(target);
 
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position* Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, smoothSpeed* Time.deltaTime);
    }
}