using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click builder for the Part B / Part C demo scene, since a .unity scene
/// authored outside the Unity Editor is not reliably reproducible. Open this
/// project in Unity and run "AM5011 > Build Full Demo Scene" once.
/// </summary>
public static class SceneSetupHelper
{
    [MenuItem("AM5011/Build Full Demo Scene")]
    public static void BuildFullDemoScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[AM5011] Exit Play Mode before building the scene.");
            return;
        }

        GameObject poseA = CreateAnchor("PoseA", new Vector3(-2f, 0f, 0f), Quaternion.identity, new Color(0.9f, 0.2f, 0.2f));
        GameObject poseB = CreateAnchor("PoseB", new Vector3(2f, 1.5f, 2f), Quaternion.Euler(0f, 180f, 90f), new Color(0.2f, 0.8f, 0.2f));

        GameObject interp = CreatePointer("InterpolatedPose", new Color(0.1f, 0.7f, 1.0f)); // cyan: dual quaternion
        GameObject decoupled = CreatePointer("DecoupledPose", new Color(1.0f, 0.15f, 0.6f)); // magenta: lerp+slerp

        DualQuaternionPoseDemo demo = interp.GetComponent<DualQuaternionPoseDemo>();
        if (demo == null) demo = interp.AddComponent<DualQuaternionPoseDemo>();
        demo.poseA = poseA.transform;
        demo.poseB = poseB.transform;
        demo.decoupledComparisonObject = decoupled.transform;
        demo.u = 0.5f;

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        }

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        Slider slider = Object.FindObjectOfType<Slider>();
        if (slider == null)
        {
            GameObject sliderGO = DefaultControls.CreateSlider(new DefaultControls.Resources());
            sliderGO.name = "InterpolationSlider";
            sliderGO.transform.SetParent(canvas.transform, false);

            RectTransform rect = sliderGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.05f);
            rect.anchorMax = new Vector2(0.5f, 0.05f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400f, 35f);

            slider = sliderGO.GetComponent<Slider>();
        }
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;
        demo.uSlider = slider;

        SetupTrail(interp, new Color(0.0f, 0.85f, 1.0f));
        SetupTrail(decoupled, new Color(1.0f, 0.15f, 0.6f));

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 2f, -3.5f);
            cam.transform.LookAt(new Vector3(0f, 0.75f, 1f));
        }

        EditorUtility.SetDirty(demo);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("<color=green><b>[AM5011] Demo scene built.</b></color> PoseA, PoseB, InterpolatedPose (cyan, dual quaternion) and DecoupledPose (magenta, lerp+slerp) are wired to the slider.");
    }

    private static GameObject CreateAnchor(string name, Vector3 pos, Quaternion rot, Color color)
    {
        GameObject go = GameObject.Find(name) ?? new GameObject(name);
        go.transform.SetPositionAndRotation(pos, rot);

        if (go.transform.Find("Visual") == null)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Visual";
            cube.transform.SetParent(go.transform, false);
            cube.transform.localPosition = new Vector3(0f, 0f, 0.25f);
            cube.transform.localScale = new Vector3(0.15f, 0.15f, 0.5f);
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            SetColor(cube, color);
        }
        return go;
    }

    private static GameObject CreatePointer(string name, Color color)
    {
        GameObject go = GameObject.Find(name) ?? new GameObject(name);

        if (go.transform.Find("Visual") == null)
        {
            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.name = "Visual";
            cyl.transform.SetParent(go.transform, false);
            cyl.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
            Object.DestroyImmediate(cyl.GetComponent<Collider>());
            SetColor(cyl, color);
        }
        return go;
    }

    private static void SetupTrail(GameObject go, Color color)
    {
        TrailRenderer trail = go.GetComponent<TrailRenderer>();
        if (trail == null) trail = go.AddComponent<TrailRenderer>();

        trail.time = 4f;
        trail.startWidth = 0.08f;
        trail.endWidth = 0.01f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = color;
        trail.endColor = new Color(color.r, color.g, color.b, 0f);
    }

    private static void SetColor(GameObject go, Color color)
    {
        Renderer rend = go.GetComponent<Renderer>();
        if (rend == null) return;
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        rend.material = mat;
    }
}
