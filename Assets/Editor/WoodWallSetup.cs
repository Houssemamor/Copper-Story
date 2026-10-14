using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

// Run: Unity -batchmode -projectPath "..." -executeMethod WoodWallSetup.FixLvl2 -quit
public static class WoodWallSetup
{
    public static void FixLvl2()
    {
        const string scenePath = "Assets/Levels/Lvl2.unity";
        if (!File.Exists(scenePath))
        {
            Debug.LogError("Lvl2.unity not found");
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Debug.Log($"Opened: {scene.name}");

        // Configure the visible plank, not the old invisible placeholder at x=20.
        GameObject wall = GameObject.Find("woodWall");
        if (wall == null)
        {
            wall = GameObject.Find("WoodWall");
        }

        if (wall == null)
        {
            Debug.LogError("Could not find the visible wooden wall in Lvl2.");
            return;
        }

        wall.name = "WoodWall";

        var woodWall = wall.GetComponent<WoodWall>();
        if (woodWall == null)
        {
            woodWall = Undo.AddComponent<WoodWall>(wall);
        }

        var collider = wall.GetComponent<Collider2D>();
        if (collider == null)
        {
            var box = Undo.AddComponent<BoxCollider2D>(wall);
            var sprite = wall.GetComponent<SpriteRenderer>();
            if (sprite != null && sprite.sprite != null)
            {
                Vector2 spriteSize = sprite.sprite.bounds.size;
                box.size = new Vector2(
                    spriteSize.x / Mathf.Max(Mathf.Abs(wall.transform.localScale.x), 0.0001f),
                    spriteSize.y / Mathf.Max(Mathf.Abs(wall.transform.localScale.y), 0.0001f));
            }
            collider = box;
        }
        collider.isTrigger = false;

        var body = wall.GetComponent<Rigidbody2D>();
        if (body == null)
        {
            body = Undo.AddComponent<Rigidbody2D>(wall);
        }
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.useFullKinematicContacts = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Disable any legacy duplicate so it cannot intercept shots or block the route.
        GameObject oldPlaceholder = GameObject.Find("WoodWall_UNUSED");
        if (oldPlaceholder != null)
        {
            Undo.RecordObject(oldPlaceholder, "Disable old wood wall placeholder");
            oldPlaceholder.SetActive(false);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Level 2 wood wall setup complete: projectile hits rotate the visible plank.");
    }
}
