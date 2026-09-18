using UnityEngine;

/// <summary>
/// Sanity checks for DualQuaternion.cs. Attach to any GameObject and press Play;
/// results are written to the Console. Not a substitute for a real test framework,
/// but enough to confirm Part B's math before building the demo on top of it.
/// </summary>
public class DualQuaternionMathTests : MonoBehaviour
{
    void Start()
    {
        bool allPassed = true;

        Vector3 posA = new Vector3(1.75f, -0.85f, 3.42f);
        Quaternion rotA = Quaternion.Euler(35f, 62f, -110f);
        DualQuaternion dqA = DualQuaternion.FromPose(rotA, posA);

        // 1. Roundtrip: FromPose -> ToPose recovers the original pose.
        dqA.ToPose(out Quaternion recRotA, out Vector3 recPosA);
        float posErr = Vector3.Distance(posA, recPosA);
        float rotErr = Quaternion.Angle(rotA, recRotA);
        if (posErr > 1e-3f || rotErr > 1e-2f)
        {
            Debug.LogError($"[FAIL] Roundtrip: posErr={posErr}, rotErr={rotErr}");
            allPassed = false;
        }

        Vector3 posB = new Vector3(-2.5f, 1.2f, 0.4f);
        Quaternion rotB = Quaternion.Euler(-20f, 140f, 45f);
        DualQuaternion dqB = DualQuaternion.FromPose(rotB, posB);

        // 2. Slerp endpoints: u=0 -> A, u=1 -> B exactly.
        DualQuaternion.Slerp(dqA, dqB, 0f).ToPose(out Quaternion r0, out Vector3 p0);
        DualQuaternion.Slerp(dqA, dqB, 1f).ToPose(out Quaternion r1, out Vector3 p1);
        float err0 = Vector3.Distance(posA, p0) + Quaternion.Angle(rotA, r0);
        float err1 = Vector3.Distance(posB, p1) + Quaternion.Angle(rotB, r1);
        if (err0 > 1e-2f || err1 > 1e-2f)
        {
            Debug.LogError($"[FAIL] Slerp endpoints: err0={err0}, err1={err1}");
            allPassed = false;
        }

        // 3. Midpoint stays a valid unit dual quaternion: |r|=1, r.d=0.
        DualQuaternion mid = DualQuaternion.Slerp(dqA, dqB, 0.5f);
        float normR = Mathf.Sqrt(mid.r.x * mid.r.x + mid.r.y * mid.r.y + mid.r.z * mid.r.z + mid.r.w * mid.r.w);
        float orth = Mathf.Abs(mid.r.x * mid.d.x + mid.r.y * mid.d.y + mid.r.z * mid.d.z + mid.r.w * mid.d.w);
        if (Mathf.Abs(normR - 1f) > 1e-3f || orth > 1e-3f)
        {
            Debug.LogError($"[FAIL] Unit constraint at midpoint: |r|={normR}, r.d={orth}");
            allPassed = false;
        }

        // 4. Multiply composes rigid transforms the same way Unity composes them.
        DualQuaternion combined = DualQuaternion.Multiply(dqB, dqA); // apply A, then B
        Vector3 testPt = new Vector3(1f, 2f, 3f);
        Vector3 viaDualQuat = combined.TransformPoint(testPt);
        Vector3 viaUnity = rotB * (rotA * testPt + posA) + posB;
        float compErr = Vector3.Distance(viaDualQuat, viaUnity);
        if (compErr > 1e-2f)
        {
            Debug.LogError($"[FAIL] Multiply composition: err={compErr}");
            allPassed = false;
        }

        if (allPassed)
        {
            Debug.Log("<color=green><b>[PASS]</b></color> Roundtrip, Slerp endpoints, unit constraints and Multiply composition all check out.");
        }
    }
}
