using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Part B demo driver: blends PoseA -> PoseB with DualQuaternion.Slerp and writes
/// the result onto this transform every frame. Also drives the Part C comparison
/// object (decoupledComparisonObject) with plain Vector3.Lerp + Quaternion.Slerp so
/// the two trajectories can be visually compared (e.g. via TrailRenderer).
/// </summary>
public class DualQuaternionPoseDemo : MonoBehaviour
{
    [Header("Pose Anchors")]
    public Transform poseA;
    public Transform poseB;

    [Header("Interpolation Parameter")]
    [Range(0f, 1f)]
    public float u = 0.5f;

    [Header("UI")]
    public Slider uSlider;

    [Header("Auto Sweep (for Part C trail capture)")]
    public bool autoAnimate = false;
    public float animationSpeed = 0.5f;

    [Header("Part C: Decoupled Comparison")]
    public Transform decoupledComparisonObject;

    void Start()
    {
        if (uSlider != null)
        {
            uSlider.minValue = 0f;
            uSlider.maxValue = 1f;
            uSlider.value = u;
            uSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    void Update()
    {
        if (poseA == null || poseB == null) return;

        if (autoAnimate)
        {
            u = Mathf.PingPong(Time.time * animationSpeed, 1f);
            if (uSlider != null) uSlider.SetValueWithoutNotify(u);
        }

        DualQuaternion dqA = DualQuaternion.FromPose(poseA.rotation, poseA.position);
        DualQuaternion dqB = DualQuaternion.FromPose(poseB.rotation, poseB.position);

        DualQuaternion dqInterp = DualQuaternion.Slerp(dqA, dqB, u);
        dqInterp.ToPose(out Quaternion rot, out Vector3 pos);

        transform.SetPositionAndRotation(pos, rot);

        if (decoupledComparisonObject != null)
        {
            Vector3 decPos = Vector3.Lerp(poseA.position, poseB.position, u);
            Quaternion decRot = Quaternion.Slerp(poseA.rotation, poseB.rotation, u);
            decoupledComparisonObject.SetPositionAndRotation(decPos, decRot);
        }
    }

    public void OnSliderValueChanged(float value)
    {
        u = value;
    }
}
