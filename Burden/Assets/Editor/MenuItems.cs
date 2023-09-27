
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MenuItems
{
    [MenuItem("Tools/Remove Empty Mesh Colliders")]
    private static void RemoveEmptyMeshColliders()
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        var length = prefabGuids.Length;
        for (var i = 0; i < length; ++i)
        {
            var guid = prefabGuids[i];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var colliders = prefab.GetComponentsInChildren<MeshCollider>();
            foreach (var collider in colliders)
            {
                if (collider.sharedMesh != null) continue;
                Object.DestroyImmediate(collider, true);
                Debug.Log("Destroyed collider on prefab " + prefab.name);
            }
        }
        var gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
        foreach (var gameObject in gameObjects)
        {
            var colliders = gameObject.GetComponentsInChildren<MeshCollider>();
            foreach (var collider in colliders)
            {
                if (collider.sharedMesh != null) continue;
                Object.DestroyImmediate(collider, true);
                Debug.Log("Destroyed collider on gameObject " + gameObject.name);
            }
        }
    }
    [MenuItem("Tools/Remove Duplicate Mesh Colliders")]
    private static void RemoveDuplicateMeshColliders()
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        var length = prefabGuids.Length;
        for (var i = 0; i < length; ++i)
        {
            var guid = prefabGuids[i];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (Transform item in prefab.transform)
            {
                Collider[] collidersList = item.GetComponents<MeshCollider>();
                for (int j = 1; j < collidersList.Length; j++)
                {
                    Object.DestroyImmediate(collidersList[j], true);
                    Debug.Log("Destroyed duplicate collider on prefab " + prefab.name);
                }
            }
        }
        var gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
        foreach (var gameObject in gameObjects)
        {
            foreach (Transform item in gameObject.transform)
            {
                Collider[] collidersList = item.GetComponents<MeshCollider>();
                for (int j = 1; j < collidersList.Length; j++)
                {
                    Object.DestroyImmediate(collidersList[j], true);
                    Debug.Log("Destroyed duplicate collider on prefab " + gameObject.name);
                }
            }
        }
    }
}