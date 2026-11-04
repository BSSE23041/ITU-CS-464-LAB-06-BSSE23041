using System.Collections;
using UnityEngine;

// Lecture 04's Rigidbody controller + Lecture 07's BoostSpeed (slide 41/50)
// + Assignment 03's Knockback (page 3).
// On the player: Rigidbody, Sphere Collider, tag "Player", Health.
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 6f;            // top run speed in m/s: guards are tuned against this
    [SerializeField] float jumpSpeed = 7f;        // ~2.5 m jump (v^2 / 2g)
    [SerializeField] float groundCheckDistance = 0.6f; // sphere radius 0.5 + a little
    [SerializeField] LayerMask groundMask = ~0;

    Rigidbody rb;
    Vector3 input;
    bool jumpQueued;

    // --- power-up state (the timer lives HERE, not on the pickup) ---
    float speedMultiplier = 1f;
    Coroutine boost;
    float boostEnd;

    // --- knockback state ---
    float noControlUntil;

    public float BoostRemaining => boost != null ? Mathf.Max(0f, boostEnd - Time.time) : 0f;

    void Awake() => rb = GetComponent<Rigidbody>();

    void Update()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        if (Input.GetButtonDown("Jump")) jumpQueued = true;
    }

    void FixedUpdate()
    {
        Vector3 v = rb.linearVelocity;

        // Steering is switched off for knockTime so the push isn't wiped next step.
        if (Time.time >= noControlUntil)
        {
            Vector3 direction = Vector3.ClampMagnitude(input, 1f);

            Transform cam = Camera.main.transform;

            Vector3 forward = cam.forward;
            Vector3 right = cam.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 move = (right * direction.x + forward * direction.z)
                           * speed * speedMultiplier;
            v = new Vector3(move.x, v.y, move.z);
        }

        if (jumpQueued)
        {
            if (Grounded()) v.y = jumpSpeed;
            jumpQueued = false;
        }

        rb.linearVelocity = v;
    }

    bool Grounded() =>
        Physics.Raycast(transform.position, Vector3.down, groundCheckDistance,
                        groundMask, QueryTriggerInteraction.Ignore);

    // ---------- Assignment 03, part B: knockback the controller keeps ----------
    public void Knockback(Vector3 push, float seconds)
    {
        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(0f, v.y, 0f);       // same throw standing or running
        rb.AddForce(push, ForceMode.Impulse);                // one kick, divided by mass
        noControlUntil = Time.time + seconds;
    }

    // ---------- Lecture 07, part 3: the boost ----------
    // RULE FOR A SECOND PICKUP: REFRESH. The newest pickup wins and the clock restarts:
    // stop the running coroutine, then start a new one. Two 5 s pickups 3 s apart end at 8 s.
    public void BoostSpeed(float multiplier, float seconds)
    {
        if (boost != null) StopCoroutine(boost);
        boost = StartCoroutine(Boost(multiplier, seconds));
    }

    IEnumerator Boost(float multiplier, float seconds)
    {
        speedMultiplier = multiplier;
        boostEnd = Time.time + seconds;
        yield return new WaitForSeconds(seconds);
        speedMultiplier = 1f;
        boost = null;
    }

    // Called by the GameManager on a respawn: the new life starts still, and un-boosted.
    public void ResetMotion()
    {
        if (boost != null) { StopCoroutine(boost); boost = null; }
        speedMultiplier = 1f;
        noControlUntil = 0f;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
