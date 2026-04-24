using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to automatically set up NetPhysics on all Net GameObjects in the scene.
/// 
/// Usage: In Unity, go to Tools > Setup Net Physics
/// This will find all GameObjects named "Net" and add the NetPhysics component.
/// The NetPhysics script handles everything else at runtime (trigger zone, cloth, etc.)
/// </summary>
public class NetPhysicsSetup : EditorWindow
{
    [MenuItem("Tools/Setup Net Physics")]
    static void SetupNetPhysics()
    {
        // Find all GameObjects in the scene
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Net")
            {
                // Check if it already has NetPhysics
                NetPhysics existing = obj.GetComponent<NetPhysics>();
                if (existing != null)
                {
                    Debug.Log("[NetPhysicsSetup] '" + obj.name + "' already has NetPhysics. Skipping.");
                    continue;
                }

                // Add NetPhysics component
                NetPhysics netPhysics = obj.AddComponent<NetPhysics>();

                // Configure default values
                netPhysics.activeTime = 2.5f;
                netPhysics.forceMultiplier = 0.3f;
                netPhysics.triggerRadius = 0.3f;
                netPhysics.triggerOffset = new Vector3(0f, 0.3f, 0f);

                // Mark the object as dirty so changes persist
                EditorUtility.SetDirty(obj);

                count++;
                Debug.Log("[NetPhysicsSetup] Added NetPhysics to: " + GetFullPath(obj));
            }
        }

        if (count > 0)
        {
            // Save the scene
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
            Debug.Log("[NetPhysicsSetup] Done! Added NetPhysics to " + count + " Net object(s). Save your scene!");
            EditorUtility.DisplayDialog("Net Physics Setup",
                "Successfully added NetPhysics to " + count + " Net object(s).\n\n" +
                "The script will automatically create trigger zones at runtime.\n\n" +
                "Don't forget to save the scene!",
                "OK");
        }
        else
        {
            Debug.LogWarning("[NetPhysicsSetup] No 'Net' GameObjects found, or all already have NetPhysics.");
            EditorUtility.DisplayDialog("Net Physics Setup",
                "No Net GameObjects found without NetPhysics.\n\n" +
                "Make sure your scene is open and contains GameObjects named 'Net'.",
                "OK");
        }
    }

    static string GetFullPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
