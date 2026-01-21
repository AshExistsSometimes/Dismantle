using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteAlways]
public class TrashScatterTool : MonoBehaviour
{
    [Header("Placement Area")]
    public float radius = 5f;

    [Header("Spawn Settings")]
    public int objectCount = 25;

    [Header("Prefab Pool")]
    public List<GameObject> trashPrefabs = new List<GameObject>();

    [Header("Layer Filters")]
    public LayerMask placementLayers;

    [Header("Surface Filtering")]
    [Tooltip("Maximum allowed slope angle in degrees. 0 = flat only, 90 = vertical allowed.")]
    public float maxSlopeAngle = 45f;

    [Header("Collision Check")]
    public float overlapCheckRadius = 0.25f;

    public void PopulateArea()
    {
        if (trashPrefabs.Count == 0)
        {
            Debug.LogWarning("TrashScatterTool: No prefabs assigned.");
            return;
        }

        GameObject parentRoot = GetOrCreateParent();

        for (int i = 0; i < objectCount; i++)
        {
            TrySpawnSingle(parentRoot);
        }
    }
    private void TrySpawnSingle(GameObject parentRoot)
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * radius;
        randomPoint.y += radius;

        if (!Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, radius * 2f, placementLayers))
            return;

        // Reject steep surfaces
        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        if (slopeAngle > maxSlopeAngle)
            return;

        // Prevent overlapping placement
        if (Physics.CheckSphere(hit.point, overlapCheckRadius))
            return;

        GameObject prefab = trashPrefabs[Random.Range(0, trashPrefabs.Count)];
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        instance.transform.position = hit.point;

        AlignToSurface(instance.transform, hit.normal);
        ApplyRandomSpin(instance.transform);

        instance.transform.SetParent(parentRoot.transform);

        Undo.RegisterCreatedObjectUndo(instance, "Spawn Trash Object");
    }

    private void AlignToSurface(Transform objTransform, Vector3 surfaceNormal)
    {
        Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
        objTransform.rotation = surfaceRotation;
    }

    private void ApplyRandomSpin(Transform objTransform)
    {
        float randomY = Random.Range(0f, 360f);
        objTransform.Rotate(Vector3.up, randomY, Space.Self);
    }

    private GameObject GetOrCreateParent()
    {
        GameObject parent = GameObject.Find("TrashDecorations");

        if (parent == null)
        {
            parent = new GameObject("TrashDecorations");
            Undo.RegisterCreatedObjectUndo(parent, "Create TrashDecorations Root");
        }

        return parent;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(TrashScatterTool))]
public class TrashScatterToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TrashScatterTool tool = (TrashScatterTool)target;

        GUILayout.Space(10f);

        if (GUILayout.Button("Populate Area With Prefabs"))
        {
            Undo.RecordObject(tool, "Populate Trash Area");
            tool.PopulateArea();
            EditorUtility.SetDirty(tool);
        }
    }
}
#endif
