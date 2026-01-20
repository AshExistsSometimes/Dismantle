using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ScaleRotRandomizer : MonoBehaviour
{
    [Header("Scale Variation")]
    [Tooltip("Scale variance applied to the original scale. Example: (-0.33, 0.1)")]
    public Vector2 sizeVariance = new Vector2(-0.33f, 0.1f);

    [Header("Rotation Limits")]
    [Tooltip("Maximum absolute rotation allowed on X and Z axes")]
    public float maxTiltAngle = 45f;

    private Vector3 originalScale;
    private bool scaleSaved;
    private void SaveOriginalScale()
    {
        if (scaleSaved)
            return;

        originalScale = transform.localScale;
        scaleSaved = true;
    }
    public void Randomise()
    {
        SaveOriginalScale();
        RandomiseRotation();
        RandomiseScale();
    }

    private void RandomiseRotation()
    {
        float x = Random.Range(-maxTiltAngle, maxTiltAngle);
        float y = Random.Range(0f, 360f);
        float z = Random.Range(-maxTiltAngle, maxTiltAngle);

        transform.localRotation = Quaternion.Euler(x, y, z);
    }

    private void RandomiseScale()
    {
        float scaleOffset = Random.Range(sizeVariance.x, sizeVariance.y);
        float finalScaleMultiplier = 1f + scaleOffset;

        transform.localScale = originalScale * finalScaleMultiplier;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ScaleRotRandomizer))]
public class TrashBagRandomiserEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ScaleRotRandomizer randomiser = (ScaleRotRandomizer)target;

        GUILayout.Space(10f);

        if (GUILayout.Button("Randomise Rotation & Scale"))
        {
            Undo.RecordObject(randomiser.transform, "Randomise Trash Bag Transform");
            randomiser.Randomise();
            EditorUtility.SetDirty(randomiser);
        }
    }
}
#endif

