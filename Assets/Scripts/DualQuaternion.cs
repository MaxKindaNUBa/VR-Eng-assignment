using UnityEngine;

/// <summary>
/// Unit dual quaternion for a rigid-body pose (R, t) in SE(3).
/// q_hat = q_r + eps * q_d, eps^2 = 0.
/// q_r is the unit rotation quaternion; q_d = 0.5 * t * q_r encodes translation
/// coupled to the current orientation.
///
/// Unity's Quaternion type has no +, scalar *, or magnitude support between
/// two quaternions, so every dual-part operation below is done componentwise.
/// </summary>
[System.Serializable]
public struct DualQuaternion
{
    public Quaternion r; // real part: rotation
    public Quaternion d; // dual part: translation-rotation coupling

    public DualQuaternion(Quaternion real, Quaternion dual)
    {
        r = real;
        d = dual;
    }

    /// <summary>Identity pose: r = (0,0,0,1), d = (0,0,0,0). Note the dual part is
    /// the ZERO quaternion, not Quaternion.identity (which would wrongly give d.w = 1).</summary>
    public static DualQuaternion identity =>
        new DualQuaternion(Quaternion.identity, new Quaternion(0f, 0f, 0f, 0f));

    private static Quaternion Add(Quaternion a, Quaternion b) =>
        new Quaternion(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);

    private static Quaternion Scale(Quaternion q, float s) =>
        new Quaternion(q.x * s, q.y * s, q.z * s, q.w * s);

    /// <summary>Builds q_hat = r + eps*(0.5 * t * r) from a rotation and a translation.</summary>
    public static DualQuaternion FromPose(Quaternion rotation, Vector3 translation)
    {
        rotation = rotation.normalized;

        Quaternion t = new Quaternion(translation.x, translation.y, translation.z, 0f);
        Quaternion dual = Scale(t * rotation, 0.5f);

        return new DualQuaternion(rotation, dual);
    }

    /// <summary>Extracts R = r/|r| and t = 2 * d * conjugate(r).</summary>
    public void ToPose(out Quaternion rotation, out Vector3 translation)
    {
        rotation = r.normalized;

        Quaternion rConj = new Quaternion(-rotation.x, -rotation.y, -rotation.z, rotation.w);
        Quaternion t = Scale(d * rConj, 2f);

        translation = new Vector3(t.x, t.y, t.z);
    }

    /// <summary>Rigid composition: (a_r*b_r) + eps*(a_r*b_d + a_d*b_r). Applies b first, then a.</summary>
    public static DualQuaternion Multiply(DualQuaternion a, DualQuaternion b)
    {
        Quaternion real = a.r * b.r;
        Quaternion dual = Add(a.r * b.d, a.d * b.r);
        return new DualQuaternion(real, dual);
    }

    /// <summary>Dual quaternion conjugate: (r*, d*), componentwise quaternion conjugate.</summary>
    public DualQuaternion Conjugate()
    {
        return new DualQuaternion(
            new Quaternion(-r.x, -r.y, -r.z, r.w),
            new Quaternion(-d.x, -d.y, -d.z, d.w));
    }

    /// <summary>Enforces the unit dual quaternion constraints: |r| = 1 and r . d = 0.</summary>
    public DualQuaternion Normalized()
    {
        float normR = Mathf.Sqrt(r.x * r.x + r.y * r.y + r.z * r.z + r.w * r.w);
        if (normR < 1e-6f)
        {
            return identity;
        }

        float inv = 1f / normR;
        Quaternion rn = Scale(r, inv);

        // Project out the component of d along rn so that rn . dn = 0.
        float dot = rn.x * d.x + rn.y * d.y + rn.z * d.z + rn.w * d.w;
        Quaternion dn = Scale(Add(d, Scale(rn, -dot)), inv);

        return new DualQuaternion(rn, dn);
    }

    /// <summary>
    /// Blends two unit dual quaternions (dual quaternion linear blending / DLB):
    /// shortest-path sign alignment, Quaternion.Slerp on the real part, a matching
    /// componentwise Lerp on the dual part (NOT Quaternion.Lerp, which renormalizes
    /// the dual part to unit length and corrupts the translation), then re-normalize.
    /// </summary>
    public static DualQuaternion Slerp(DualQuaternion a, DualQuaternion b, float u)
    {
        u = Mathf.Clamp01(u);

        if (Quaternion.Dot(a.r, b.r) < 0f)
        {
            b = new DualQuaternion(Scale(b.r, -1f), Scale(b.d, -1f));
        }

        Quaternion real = Quaternion.Slerp(a.r, b.r, u);
        Quaternion dual = new Quaternion(
            Mathf.Lerp(a.d.x, b.d.x, u),
            Mathf.Lerp(a.d.y, b.d.y, u),
            Mathf.Lerp(a.d.z, b.d.z, u),
            Mathf.Lerp(a.d.w, b.d.w, u));

        return new DualQuaternion(real, dual).Normalized();
    }

    /// <summary>Applies this rigid transform to a point: p' = R*p + t.</summary>
    public Vector3 TransformPoint(Vector3 p)
    {
        ToPose(out Quaternion rot, out Vector3 trans);
        return rot * p + trans;
    }
}
