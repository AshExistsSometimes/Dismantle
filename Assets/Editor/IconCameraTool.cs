using UnityEngine;
using UnityEditor;
using System.IO;

public class IconCameraTool : EditorWindow
{
    private Camera captureCamera;
    private string iconName = "NewIcon";
    private int resolution = 512;

    [MenuItem("Tools/Weapon Icon Camera")]
    public static void ShowWindow()
    {
        GetWindow<IconCameraTool>("Weapon Icon Camera");
    }

    private void OnGUI()
    {
        GUILayout.Label("Weapon Icon Capture Tool", EditorStyles.boldLabel);

        captureCamera = (Camera)EditorGUILayout.ObjectField("Camera", captureCamera, typeof(Camera), true);
        iconName = EditorGUILayout.TextField("Icon Name", iconName);
        resolution = EditorGUILayout.IntField("Resolution", resolution);

        if (GUILayout.Button("Capture Icon PNG"))
        {
            if (captureCamera == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a camera!", "OK");
                return;
            }

            CaptureIconPNG();
        }
    }

    private void CaptureIconPNG()
    {
        // Ensure folder exists
        string folderPath = "Assets/IconPhotos";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // Create a temporary RenderTexture with alpha
        RenderTexture rt = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 8; // optional for smoother edges
        captureCamera.targetTexture = rt;

        // Set background to transparent
        Color originalBackground = captureCamera.backgroundColor;
        CameraClearFlags originalClearFlags = captureCamera.clearFlags;

        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.backgroundColor = new Color(0, 0, 0, 0); // transparent

        captureCamera.Render();

        // Read the pixels into a Texture2D
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        tex.Apply();

        // Encode as PNG
        byte[] pngData = tex.EncodeToPNG();
        if (pngData != null)
        {
            string path = Path.Combine(folderPath, iconName + ".png");
            File.WriteAllBytes(path, pngData);
            Debug.Log($"Weapon icon saved to {path}");
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("Failed to encode PNG.");
        }

        // Restore camera
        captureCamera.clearFlags = originalClearFlags;
        captureCamera.backgroundColor = originalBackground;

        // Clean up
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        DestroyImmediate(tex);
    }
}
