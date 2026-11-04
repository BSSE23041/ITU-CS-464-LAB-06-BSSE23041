using UnityEngine;

// A trigger pad. Wins only if the player has collected enough coins (GameManager decides).
public class Goal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance.ReachGoal();
    }
}
