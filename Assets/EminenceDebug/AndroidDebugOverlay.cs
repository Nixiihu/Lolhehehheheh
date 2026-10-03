using UnityEngine;

// Lightweight, in-game development visualization for objects marked DebugTarget.
// This is not an external Android overlay and does not inspect other apps.
public sealed class AndroidDebugOverlay : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera gameCamera;

    [Header("Menu")]
    [SerializeField] private bool startExpanded = true;
    [SerializeField] private bool showESP = true;
    [SerializeField] private bool showBoxes = true;
    [SerializeField] private bool showHealth = true;
    [SerializeField] private bool showDistance = true;
    [SerializeField] private bool showNames;
    [SerializeField] private bool showColliderBounds;

    [Header("Performance")]
    [SerializeField, Min(1)] private int drawEveryNFrames = 1;
    [SerializeField, Min(0f)] private float maxDistance = 250f;

    private bool expanded;
    private GUIStyle labelStyle;
    private const float RefWidth = 720f;
    private const float RefHeight = 1280f;
    private const float Padding = 16f;

    private void Awake()
    {
        expanded = startExpanded;
        if (gameCamera == null)
            gameCamera = Camera.main;
    }

    private void OnGUI()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        return;
#endif
        // Keep debug GUI from rendering in release builds.
        float scale = Mathf.Min(Screen.width / RefWidth, Screen.height / RefHeight);
        if (scale <= 0f) return;

        float offsetX = (Screen.width - RefWidth * scale) * 0.5f;
        float offsetY = (Screen.height - RefHeight * scale) * 0.5f;
        Matrix4x4 previousMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(
            new Vector3(offsetX, offsetY, 0f),
            Quaternion.identity,
            new Vector3(scale, scale, 1f));

        EnsureStyles();

        // Touch-friendly in-game launcher. It is part of the Unity scene UI.
        if (GUI.Button(new Rect(Padding, Padding, 176f, 58f),
            expanded ? "Eminence  −" : "Eminence  +"))
            expanded = !expanded;

        if (expanded)
        {
            GUILayout.BeginArea(new Rect(Padding, 88f, 320f, 360f), GUI.skin.box);
            GUILayout.Label("EMINENCE • DEV VISUALS");
            showESP = GUILayout.Toggle(showESP, "Enable visualization");
            showBoxes = GUILayout.Toggle(showBoxes, "Collider boxes");
            showHealth = GUILayout.Toggle(showHealth, "Health");
            showDistance = GUILayout.Toggle(showDistance, "Distance");
            showNames = GUILayout.Toggle(showNames, "Target names");
            showColliderBounds = GUILayout.Toggle(showColliderBounds, "Collider bounds");
            GUILayout.Space(8f);
            GUILayout.Label("Development build only");
            GUILayout.EndArea();
        }

        if (showESP && gameCamera != null &&
            (drawEveryNFrames <= 1 || Time.frameCount % drawEveryNFrames == 0))
        {
            foreach (DebugTarget target in DebugTarget.GetActiveTargets())
            {
                if (target == null || !target.isActiveAndEnabled) continue;

                Vector3 origin = target.transform.position;
                float distance = Vector3.Distance(gameCamera.transform.position, origin);
                if (maxDistance > 0f && distance > maxDistance) continue;

                // Use the assigned gameplay collider's world bounds for a tighter
                // approximation than a fixed-height rectangle.
                Bounds bounds;
                if (target.targetCollider != null && target.targetCollider.enabled)
                    bounds = target.targetCollider.bounds;
                else
                    bounds = new Bounds(origin + Vector3.up * 0.9f,
                        new Vector3(0.6f, 1.8f, 0.6f));

                if (!TryGetScreenRect(bounds, out Rect screenRect))
                    continue;

                if (showBoxes || showColliderBounds)
                    DrawOutline(screenRect, Color.green, 2f);

                if (showNames)
                    GUI.Label(new Rect(screenRect.x, screenRect.y - 25f,
                        Mathf.Max(120f, screenRect.width + 80f), 24f),
                        target.targetName, labelStyle);

                if (showHealth)
                {
                    float fraction = Mathf.Clamp01(target.health / 100f);
                    Rect barBg = new Rect(screenRect.x, screenRect.y - 7f,
                        Mathf.Max(8f, screenRect.width), 5f);
                    GUI.color = Color.gray;
                    GUI.DrawTexture(barBg, Texture2D.whiteTexture);
                    GUI.color = Color.Lerp(Color.red, Color.green, fraction);
                    GUI.DrawTexture(new Rect(barBg.x, barBg.y,
                        barBg.width * fraction, barBg.height), Texture2D.whiteTexture);
                    GUI.color = Color.white;
                }

                if (showDistance)
                    GUI.Label(new Rect(screenRect.x, screenRect.yMax + 2f,
                        140f, 24f), distance.ToString("F1") + " m", labelStyle);
            }
        }

        GUI.color = Color.white;
        GUI.matrix = previousMatrix;
    }

    private bool TryGetScreenRect(Bounds b, out Rect rect)
    {
        rect = default;
        Vector3 c = b.center;
        Vector3 e = b.extents;
        Vector3[] corners =
        {
            c + new Vector3(-e.x,-e.y,-e.z), c + new Vector3(-e.x,-e.y,e.z),
            c + new Vector3(-e.x,e.y,-e.z),  c + new Vector3(-e.x,e.y,e.z),
            c + new Vector3(e.x,-e.y,-e.z),  c + new Vector3(e.x,-e.y,e.z),
            c + new Vector3(e.x,e.y,-e.z),   c + new Vector3(e.x,e.y,e.z)
        };

        float minX = float.PositiveInfinity, minY = float.PositiveInfinity;
        float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
        bool anyInFront = false;

        foreach (Vector3 corner in corners)
        {
            Vector3 p = gameCamera.WorldToScreenPoint(corner);
            if (p.z <= gameCamera.nearClipPlane) continue;
            anyInFront = true;
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }

        if (!anyInFront) return false;

        // Convert physical screen pixels to the centered virtual GUI layout.
        float scale = Mathf.Min(Screen.width / RefWidth, Screen.height / RefHeight);
        float ox = (Screen.width - RefWidth * scale) * 0.5f;
        float oy = (Screen.height - RefHeight * scale) * 0.5f;
        float x1 = (minX - ox) / scale;
        float x2 = (maxX - ox) / scale;
        float y1 = (Screen.height - maxY - oy) / scale;
        float y2 = (Screen.height - minY - oy) / scale;

        rect = Rect.MinMaxRect(x1, y1, x2, y2);
        return rect.width > 1f && rect.height > 1f;
    }

    private void EnsureStyles()
    {
        if (labelStyle != null) return;
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
    }

    private static void DrawOutline(Rect r, Color color, float thickness)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(r.x, r.y, r.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x, r.yMax - thickness, r.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x, r.y, thickness, r.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.xMax - thickness, r.y, thickness, r.height), Texture2D.whiteTexture);
        GUI.color = old;
    }
}
