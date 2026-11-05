using System.Collections;
using UnityEngine;

// Lecture 07, part 2: ONE script for every danger.
//   Spikes  : Box Collider (Is Trigger), no Rigidbody, damage 1, restTime = 0
//   Guard   : Capsule Collider (Is Trigger) + kinematic Rigidbody, damage 1, restTime = 0
//   Crusher : Box Collider (Is Trigger), damage 2, restTime > 0, a 'warning' child and a 'slab' child
public class Hazard : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float knockback = 6f;     // Impulse: 6 m/s for a 1 kg player
    [SerializeField] float knockTime = 0.25f;  // seconds the player can't steer

    [Header("Timed hazard (crusher). Leave restTime at 0 for an always-on hazard.")]
    [SerializeField] float restTime = 0f;      // collider OFF, safe
    [SerializeField] float warnTime = 0.7f;    // warning ON, still safe (>= 0.5 s or it's unfair)
    [SerializeField] float strikeTime = 0.4f;  // collider ON, hurts
    [SerializeField] float startOffset = 0f;   // wait this long first, so two crushers can alternate
    [SerializeField] GameObject warning;       // child that lights up during warnTime
    [SerializeField] GameObject slab;          // child shown during the strike (visual only, no collider)

    IEnumerator Start()
    {
        if (restTime <= 0f) yield break;       // static hazard: nothing to schedule

        Collider zone = GetComponent<Collider>();
        if (startOffset > 0f)
        {
            zone.enabled = false;
            yield return new WaitForSeconds(startOffset);
        }

        while (true)
        {
            zone.enabled = false;
            if (slab) slab.SetActive(false);
            if (warning) warning.SetActive(false);
            yield return new WaitForSeconds(restTime);

            if (warning) warning.SetActive(true);
            yield return new WaitForSeconds(warnTime);

            if (warning) warning.SetActive(false);
            zone.enabled = true;
            if (slab) slab.SetActive(true);
            yield return new WaitForSeconds(strikeTime);
        }
    }

    // Every physics step while overlapping. Health's i-frames decide how often it lands.
    void OnTriggerStay(Collider other)
    {
        if (!other.TryGetComponent(out Health health)) return;
        if (!health.TakeDamage(damage)) return;      // no hit, no knockback

        Vector3 away = other.transform.position - transform.position;
        away.y = 0f;                                  // flat: a hit never launches you upward
        if (away.sqrMagnitude < 0.0001f) away = transform.right;
        Vector3 push = away.normalized * knockback;

        if (other.TryGetComponent(out PlayerController player))
            player.Knockback(push, knockTime);
        else if (other.attachedRigidbody != null)
            other.attachedRigidbody.AddForce(push, ForceMode.Impulse);
    }
}
