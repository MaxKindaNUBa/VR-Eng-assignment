"""
Numeric replica of Assets/Scripts/DualQuaternion.cs, used only to produce the
Part C table/figure for the report without needing a Unity Editor run.
Quaternions are stored as (x, y, z, w) to match Unity's convention.
"""
import numpy as np
import matplotlib.pyplot as plt

def qmul(a, b):
    ax, ay, az, aw = a
    bx, by, bz, bw = b
    return np.array([
        aw * bx + ax * bw + ay * bz - az * by,
        aw * by - ax * bz + ay * bw + az * bx,
        aw * bz + ax * by - ay * bx + az * bw,
        aw * bw - ax * bx - ay * by - az * bz,
    ])

def qconj(q):
    return np.array([-q[0], -q[1], -q[2], q[3]])

def qnorm(q):
    return np.linalg.norm(q)

def from_pose(rotation, translation):
    r = rotation / qnorm(rotation)
    t = np.array([translation[0], translation[1], translation[2], 0.0])
    d = 0.5 * qmul(t, r)
    return r, d

def to_pose(r, d):
    rn = r / qnorm(r)
    t = 2.0 * qmul(d, qconj(rn))
    return rn, t[:3]

def dq_normalize(r, d):
    normR = qnorm(r)
    rn = r / normR
    dot = np.dot(rn, d)
    dn = (d - dot * rn) / normR
    return rn, dn

def slerp_quat(a, b, u):
    dot = np.dot(a, b)
    dot = np.clip(dot, -1.0, 1.0)
    theta = np.arccos(dot)
    if theta < 1e-6:
        return a * (1 - u) + b * u
    s = np.sin(theta)
    return (np.sin((1 - u) * theta) / s) * a + (np.sin(u * theta) / s) * b

def dq_slerp(rA, dA, rB, dB, u):
    if np.dot(rA, rB) < 0:
        rB, dB = -rB, -dB
    r = slerp_quat(rA, rB, u)
    d = (1 - u) * dA + u * dB
    return dq_normalize(r, d)

def euler_z(deg):
    """Pure rotation about +Z, in Unity (x,y,z,w) form."""
    half = np.radians(deg) / 2.0
    return np.array([0.0, 0.0, np.sin(half), np.cos(half)])

def quat_angle_deg(a, b):
    dot = np.clip(abs(np.dot(a, b) / (qnorm(a) * qnorm(b))), -1.0, 1.0)
    return np.degrees(2.0 * np.arccos(dot))

# ---- Scenario: same style as the worked example, kept 2D for a readable table/plot ----
posA = np.array([-2.0, 0.0, 0.0])
rotA = np.array([0.0, 0.0, 0.0, 1.0])  # identity

posB = np.array([2.0, 0.0, 0.0])
rotB = euler_z(120.0)  # 120 deg about +Z

rA, dA = from_pose(rotA, posA)
rB, dB = from_pose(rotB, posB)

us = [0.0, 0.25, 0.5, 0.75, 1.0]
rows = []
path_dq = []
path_sep = []

for u in np.linspace(0, 1, 101):
    r_dlb, d_dlb = dq_slerp(rA, dA, rB, dB, u)
    _, t_dlb = to_pose(r_dlb, d_dlb)
    path_dq.append(t_dlb[:2])

    t_sep = (1 - u) * posA + u * posB
    path_sep.append(t_sep[:2])

for u in us:
    r_dlb, d_dlb = dq_slerp(rA, dA, rB, dB, u)
    r_out, t_out = to_pose(r_dlb, d_dlb)
    angle_dlb = quat_angle_deg(rotA, r_out)

    r_sep = slerp_quat(rA, rB, u) if np.dot(rA, rB) >= 0 else slerp_quat(-rA, rB, u)
    angle_sep = quat_angle_deg(rotA, r_sep)

    rows.append((u, t_out[0], t_out[1], angle_dlb, angle_sep))

print(f"{'u':>5} | {'DLB x':>8} {'DLB y':>8} | {'DLB angle':>10} | {'Separate angle':>15}")
for u, x, y, a_dlb, a_sep in rows:
    print(f"{u:5.2f} | {x:8.3f} {y:8.3f} | {a_dlb:10.2f} | {a_sep:15.2f}")

path_dq = np.array(path_dq)
path_sep = np.array(path_sep)

plt.figure(figsize=(6, 4))
plt.plot(path_sep[:, 0], path_sep[:, 1], color="#e8348c", label="Separate Lerp + Slerp", linewidth=2)
plt.plot(path_dq[:, 0], path_dq[:, 1], color="#1ab3e6", label="Dual-quaternion Slerp (DLB)", linewidth=2)
plt.scatter([posA[0], posB[0]], [posA[1], posB[1]], color="black", zorder=5)
plt.annotate("PoseA", (posA[0], posA[1]), textcoords="offset points", xytext=(-10, 8))
plt.annotate("PoseB", (posB[0], posB[1]), textcoords="offset points", xytext=(-10, 8))
plt.xlabel("x (m)")
plt.ylabel("y (m)")
plt.title("Part C: position path comparison")
plt.legend()
plt.grid(alpha=0.3)
plt.tight_layout()
plt.savefig("report/figures/trajectory_comparison.png", dpi=160)
print("\nSaved report/figures/trajectory_comparison.png")
