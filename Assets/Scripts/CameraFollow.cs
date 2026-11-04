using UnityEngine;

// Simple follow camera. Put it on the Main Camera and drag the player in.
// Skip it if your Lecture 04/05 project already has a camera rig.
public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 12f, -9f);
    [SerializeField] float smooth = 6f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 wanted = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, wanted, smooth * Time.deltaTime);
        transform.LookAt(target.position);
    }
}
