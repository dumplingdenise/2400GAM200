using UnityEngine;
using static GameController;

public class ShadowFollower : MonoBehaviour
{
    [HideInInspector] public Transform target;
    public float followSpeed = 8f;

    void Update()
    {
        if (GameController.IsPaused) return;

        if (target == null) return;

        transform.position = Vector3.Lerp(
            transform.position,
            target.position,
            Time.deltaTime * followSpeed
        );
    }
}
