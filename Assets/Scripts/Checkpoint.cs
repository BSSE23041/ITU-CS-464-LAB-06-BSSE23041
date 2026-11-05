using UnityEngine;

// Optional (Assignment 03, part C). A trigger you pass through; respawn here from now on.
public class Checkpoint : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance.SetCheckpoint(transform);
    }
}
