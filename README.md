# AM5011 — Assignment 2: Dual Quaternions for VR Poses and Blending

GitHub repository: https://github.com/MaxKindaNUBa/VR-Eng-assignment

## Contents

```
VR Eng assignment/
├── Assets/
│   ├── Scripts/
│   │   ├── DualQuaternion.cs            # Part B core struct
│   │   ├── DualQuaternionPoseDemo.cs    # Part B/C demo + comparison driver
│   │   ├── DualQuaternionMathTests.cs   # Play-mode sanity checks
│   │   └── Editor/SceneSetupHelper.cs   # One-click demo scene builder
│   └── Scenes/                          # (created by the scene builder, see below)
├── Packages/manifest.json               # Unity 2022.3 LTS package set
├── ProjectSettings/ProjectVersion.txt    # Pinned to 2022.3.52f1
├── requirements.txt                      # Python deps for tools/dualquat_sim.py
├── tools/dualquat_sim.py                 # Numeric replica of the math, for Part C figure/table
├── report/main.tex                       # Report write-up (LaTeX)
├── report/main.pdf                       # Compiled report
└── report/figures/                       # trajectory_comparison.png, image.png
```

## Running the Unity project (2022.3.52f1)

1. Open **Unity Hub → Open → Add project from disk**, and select this
   `VR Eng assignment` folder. Unity Hub reads `ProjectSettings/ProjectVersion.txt`
   and should offer to open it directly with 2022.3.52f1; if that exact
   editor isn't installed, install it from Hub first (Installs → Add).
2. On first open, Unity will generate the missing `Library/` and remaining
   `ProjectSettings/` files automatically — this is normal and can take a
   minute.
3. In the menu bar, run **AM5011 → Build Full Demo Scene**. This creates and
   wires up everything in a new scene: `PoseA` (red), `PoseB` (green),
   `InterpolatedPose` (cyan, dual-quaternion blend), `DecoupledPose` (magenta,
   plain Lerp+Slerp), a UI slider, and a `TrailRenderer` on each pointer.
4. Save the scene (**File → Save As...** into `Assets/Scenes/`), then press
   **Play** and drag the slider from 0 to 1. Compare the cyan trail (curved,
   coupled screw motion) against the magenta trail (straight chord).
5. To run the math sanity checks: add the `DualQuaternionMathTests` component
   to any GameObject in the scene and press Play — results print to the
   Console.

## Regenerating the Part C figure/table

`tools/dualquat_sim.py` is a line-for-line numeric replica of
`DualQuaternion.cs`'s math (not a Unity script) — it exists purely to compute
exact numbers and the trajectory plot used in the report without needing a
full Editor run.

Python dependencies are listed in `requirements.txt` (`numpy`, `matplotlib`).
Install them with:

```bash
pip install -r requirements.txt
```

then run the script from the `VR Eng assignment` folder:

```bash
python tools/dualquat_sim.py
```

This prints the $u$/position/angle table and (re)writes
`report/figures/trajectory_comparison.png`.

## Building the report

`report/main.tex` compiles with any standard TeX distribution (MiKTeX, TeX
Live, or a self-contained engine like Tectonic) using only common packages —
`amsmath`, `graphicx`, `booktabs`, `hyperref`, `enumitem`, `microtype`,
`parskip`, `float`. A compiled copy is already checked in at
`report/main.pdf`; to rebuild it after editing `main.tex`:

- **Overleaf:** upload the `report/` folder (including `figures/`) as a new
  project and compile `main.tex`, or
- **Local install:** install MiKTeX or TeX Live, then run
  `pdflatex main.tex` (or `tectonic main.tex`) from inside `report/`.
