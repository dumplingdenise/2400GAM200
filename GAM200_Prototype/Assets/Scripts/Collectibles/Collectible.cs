using UnityEngine;

public class Collectible : MonoBehaviour
{
    /*[Header("Collectible")]
    public AudioClip pickupSound;       // Optional sound
    public GameObject pickupEffect;     // Optional visual effect*/

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Add 1 collectible to the global count
        if (CollectibleManager.instance != null)
            CollectibleManager.instance.AddCollectible(1);

       /* // Play optional sound and visual effects
        if (pickupSound)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        if (pickupEffect)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);*/

        // Destroy this collectible after pickup
        Destroy(gameObject);
    }
}