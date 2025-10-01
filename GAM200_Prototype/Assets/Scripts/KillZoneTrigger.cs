using UnityEngine;

public class KillZoneTrigger : MonoBehaviour
{
    private gameController gc;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //cache controller
        gc = FindAnyObjectByType<gameController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Only the player triggers respawn
        if (!other.CompareTag("Player")) return;

        if (gc != null)
        {
            gc.Respawn();
            Debug.Log("Killzone hit: respawning");
        }

    }
}
