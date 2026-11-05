using UnityEngine;

// Lecture 06's collectible. Make it a prefab: Sphere/cylinder mesh, collider with Is Trigger ticked.
public class Coin : MonoBehaviour
{
    [SerializeField] int value = 1;
    [SerializeField] float spinSpeed = 120f;   // degrees per second, so coins read as pickups

    void Update() => transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;     // built-in "Player" tag, case-sensitive
        GameManager.Instance.AddScore(value);
        Destroy(gameObject);
    }
}
