using UnityEngine;

// Lecture 07, slide 30. One wide Box Collider BELOW the level, Is Trigger ticked.
// Falling is a mistake, not damage: skip the hearts and cost a life at once.
public class KillZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance.PlayerDied();
    }
}
