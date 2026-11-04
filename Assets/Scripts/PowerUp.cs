using UnityEngine;

public enum PowerUpType { Speed, Shield }

// Lecture 07, part 3. The pickup is a trigger that reports and disappears.
// The boost AND its timer belong to the player (PlayerController.BoostSpeed):
// a coroutine started here would die with Destroy(gameObject), and the boost would never end.
public class PowerUp : MonoBehaviour
{
    [SerializeField] PowerUpType type = PowerUpType.Speed;
    [SerializeField] float multiplier = 1.6f;   // Speed only
    [SerializeField] float duration = 5f;       // Speed only

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (type == PowerUpType.Speed)
        {
            if (other.TryGetComponent(out PlayerController player))
                player.BoostSpeed(multiplier, duration);
        }
        else if (other.TryGetComponent(out Health health))
        {
            health.shielded = true;
            GameManager.Instance.UpdateUI();
        }
        Destroy(gameObject);
    }
}
