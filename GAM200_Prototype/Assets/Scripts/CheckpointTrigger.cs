using Unity.Cinemachine;
using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    private gameController gc;
    private bool claimed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //cache the central controller once
        gc = FindAnyObjectByType<gameController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //only reach to player
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (claimed)
        {
            return;
        }

        if (gc != null)
        {
            //tell controller this is now the active checkpoint
            gc.SetCheckpoint(transform);

            // lock this checkpoint so it cant be triggered again
            claimed = true;
            GetComponent<Collider2D>().enabled = false;
        }

    }
}
