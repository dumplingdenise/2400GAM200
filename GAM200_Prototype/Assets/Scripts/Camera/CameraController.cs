using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform mainDoll;
    [SerializeField] Transform shadowDoll;
    [SerializeField] gameController controller;
    [SerializeField] Vector3 offset = new Vector3(0, 1.5f, -10f);
   // [SerializeField] float followLerp = 0.15f;

    [SerializeField] float minX;
    [SerializeField] float maxX;
    [SerializeField] float minY;
    [SerializeField] float maxY;

    [SerializeField] float followLerpX = 0.20f;
    [SerializeField] float followLerpY = 0.10f;

    [SerializeField] float lookaheadX = 0.8f;
    [SerializeField] float lookaheadSmooth = 0.2f;
    [SerializeField] float currentLookahead;
    [SerializeField] float prevTargetX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var t = (controller.currentMode == gameController.WorldState.Real) ? mainDoll : shadowDoll;
        prevTargetX = t.position.x;
        currentLookahead = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        var target = (controller.currentMode == gameController.WorldState.Real) ? mainDoll : shadowDoll;
        
        var targetPos = target.position;

        float vx = (Time.deltaTime > 0f) ? (targetPos.x - prevTargetX) / Time.deltaTime : 0f;

        if (Mathf.Abs(vx) < 0.01f)   // dead zone for tiny velocities
            vx = 0f;

        float wanted = Mathf.Sign(vx) * lookaheadX;
        // smoother that respects Time.deltaTime:
        float a = 1f - Mathf.Exp(-lookaheadSmooth * Time.deltaTime);
        currentLookahead = Mathf.Lerp(currentLookahead, wanted, a);
        prevTargetX = targetPos.x;

        var desired = targetPos + offset + new Vector3(currentLookahead, 0f, 0f);
        float x = Mathf.Lerp(transform.position.x, desired.x, followLerpX);
        float y = Mathf.Lerp(transform.position.y, desired.y, followLerpY);

        x = Mathf.Clamp(x, minX, maxX);
        y = Mathf.Clamp(y, minY, maxY);
        transform.position = new Vector3(x, y, desired.z);  // z comes from offset (-10)


    }

    public void SnapToTarget()
    {
        var t = (controller.currentMode == gameController.WorldState.Real) ? mainDoll: shadowDoll;

        transform.position = t.position + offset;

        prevTargetX = t.position.x;   // reset velocity baseline
        currentLookahead = 0f;        // avoid one-frame lookahead jump

    }
}
