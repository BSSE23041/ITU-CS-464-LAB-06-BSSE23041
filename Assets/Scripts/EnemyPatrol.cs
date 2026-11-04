using UnityEngine;

public enum PatrolMode { Loop, PingPong, Random }

// Lecture 07, part 1: patrol (slides 7-12) grown into slide 20's three states.
//   PATROL: walk the waypoints.        Sees player inside chaseRange  -> CHASE
//   CHASE : MoveTowards the player.    Player beyond giveUpRange      -> RETURN
//   RETURN: walk back to the route.    Arrived (by distance)          -> PATROL
//
// On the guard: kinematic Rigidbody, a trigger collider, Hazard.cs, this script.
// The waypoints are empty GameObjects at the guard's height. Do NOT make them
// children of the guard, or they walk away with it.
//
// chaseRange = 0 gives a guard that only patrols (never chases).
public class EnemyPatrol : MonoBehaviour
{
    enum State { Patrol, Chase, Return }

    [SerializeField] Transform[] points;
    [SerializeField] PatrolMode mode = PatrolMode.Loop;
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float chaseSpeed = 3f;        // keep it BELOW the player's run speed
    [SerializeField] float chaseRange = 5f;
    [SerializeField] float giveUpRange = 8f;       // longer than chaseRange, so it can't flicker at one edge
    [SerializeField] float arriveDistance = 0.15f; // bigger than one step (speed x deltaTime)
    [SerializeField] float turnSpeed = 360f;
    [SerializeField] float waitTime = 0f;          // pause at each waypoint (Random mode looks best with ~0.6)

    State state = State.Patrol;
    Transform player;
    int index, step = 1;
    float waitUntil;

    void Start()
    {
        // found by type, so one prefab fits every level
        PlayerController p = FindAnyObjectByType<PlayerController>();
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (points == null || points.Length == 0) return;

        switch (state)
        {
            case State.Patrol:
                if (Time.time >= waitUntil)
                {
                    WalkTo(points[index].position, patrolSpeed);
                    if (Arrived(points[index].position))
                    {
                        index = NextIndex();
                        waitUntil = Time.time + waitTime;
                    }
                }
                if (InRange(chaseRange)) state = State.Chase;
                break;

            case State.Chase:
                WalkTo(player.position, chaseSpeed);
                if (!InRange(giveUpRange)) state = State.Return;
                break;

            case State.Return:
                WalkTo(points[index].position, patrolSpeed);
                if (InRange(chaseRange)) state = State.Chase;       // spotted again on the way back
                else if (Arrived(points[index].position)) state = State.Patrol;
                break;
        }
    }

    // ---- movement: MoveTowards on the ground plane, then face where we walk ----
    void WalkTo(Vector3 target, float speed)
    {
        target.y = transform.position.y;                  // stay at the guard's height
        Vector3 toTarget = target - transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position, target, speed * Time.deltaTime);

        if (toTarget.sqrMagnitude > 0.0001f)              // LookRotation(zero) logs a warning
        {
            Quaternion look = Quaternion.LookRotation(toTarget);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, look, turnSpeed * Time.deltaTime);
        }
    }

    // ---- arrival and sight, both by distance (flat, ignoring height) ----
    bool Arrived(Vector3 target) => Flat(transform.position, target) < arriveDistance;

    bool InRange(float range) =>
        player != null && range > 0f && Flat(transform.position, player.position) < range;

    static float Flat(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    // ---- the order the route is walked (slides 8-10) ----
    int NextIndex()
    {
        if (points.Length < 2) return 0;
        if (mode == PatrolMode.Loop)
            return (index + 1) % points.Length;

        if (mode == PatrolMode.PingPong)
        {
            int ahead = index + step;                     // turn round at each end
            if (ahead < 0 || ahead >= points.Length) step = -step;
            return index + step;
        }

        int next = Random.Range(0, points.Length);        // Random: any point but this one
        return next == index ? (next + 1) % points.Length : next;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = new Color(1f, 0.5f, 0f); Gizmos.DrawWireSphere(transform.position, giveUpRange);
    }
}
