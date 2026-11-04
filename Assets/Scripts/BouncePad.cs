using UnityEngine;

// Assignment 03, part A: one force, on purpose.
// ForceMode.VelocityChange = one kick, mass ignored, so the pad launches a heavy and a
// light player to the same height. Fired ONCE from OnTriggerEnter (a kick, not a push).
// Height ~ v^2 / 2g:  10 m/s -> ~5 m,  8 m/s -> ~3.3 m (gravity 9.81).
// Box Collider (Is Trigger), no Rigidbody on the pad: the player's is enough.
public class BouncePad : MonoBehaviour
{
    [SerializeField] float launchSpeed = 10f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Rigidbody body = other.attachedRigidbody;
        if (body == null) return;

        Vector3 v = body.linearVelocity;
        body.linearVelocity = new Vector3(v.x, 0f, v.z);        // same height however you land
        body.AddForce(Vector3.up * launchSpeed, ForceMode.VelocityChange);
    }
}
