using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, -10f);

    [Header("Clamp Bounds")]
    public float minX, maxX, minY, maxY;

    [Header("Smoothness")]
    public float followLerpX = 0.2f;
    public float followLerpY = 0.1f;

    [Header("Lookahead")]
    public float lookaheadX = 0.8f;
    public float lookaheadSmooth = 0.2f;
    private float currentLookahead;
    private float prevTargetX;

    void Start()
    {
        if (target != null)
            prevTargetX = target.position.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float vx = (Time.deltaTime > 0) ? (target.position.x - prevTargetX) / Time.deltaTime : 0f;
        if (Mathf.Abs(vx) < 0.01f) vx = 0f;

        float wanted = Mathf.Sign(vx) * lookaheadX;
        float a = 1f - Mathf.Exp(-lookaheadSmooth * Time.deltaTime);
        currentLookahead = Mathf.Lerp(currentLookahead, wanted, a);
        prevTargetX = target.position.x;

        Vector3 desired = target.position + offset + new Vector3(currentLookahead, 0, 0);
        float x = Mathf.Lerp(transform.position.x, desired.x, followLerpX);
        float y = Mathf.Lerp(transform.position.y, desired.y, followLerpY);
        x = Mathf.Clamp(x, minX, maxX);
        y = Mathf.Clamp(y, minY, maxY);

        transform.position = new Vector3(x, y, desired.z);
    }

    public void UpdateTarget(Transform newTarget)
    {
        target = newTarget;
        prevTargetX = target.position.x;
        currentLookahead = 0f;
        transform.position = target.position + offset;
    }
}

