using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {

    // Fisher-Yates shuffle algorithm
    public static List<T> Shuffle<T>(this IList<T> list) {
        List<T> shuffledList = new(list); // Create a new list to hold the shuffled elements
        int n = shuffledList.Count;
        for (int i = n - 1; i > 0; i--) {
            int j = UnityEngine.Random.Range(0, i + 1); // Use Unity's Random.Range to get a random index
            (shuffledList[j], shuffledList[i]) = (shuffledList[i], shuffledList[j]);
        }
        return shuffledList;
    }

    // Better invoke
    public static void Invoke(this MonoBehaviour mb, Action f, float delay) {
        mb.StartCoroutine(InvokeRoutine(f, delay));
    }

    private static IEnumerator InvokeRoutine(System.Action f, float delay) {
        yield return new WaitForSeconds(delay);
        f();
    }

    // Performs a DFS to go through entire transform tree for the tag
    public static IEnumerable<GameObject> FindChildrenWithTag(GameObject parent, string tag) {
        Stack<Transform> searchStack = new();

        searchStack.Push(parent.transform);
        while (searchStack.Count > 0) {
            var curr = searchStack.Pop();
            foreach (Transform t in curr) {
                searchStack.Push(t);
                if (t.CompareTag(tag)) yield return t.gameObject;
            }
        }
    }

    /// <summary>
    /// Extracts the texture for a given sprite, accounting for atlas packing.
    /// </summary>
    /// <param name="sprite">The sprite to extract the texture from.</param>
    /// <returns>A new Texture2D containing the sprite's texture.</returns>
    public static Texture2D ExtractSpriteTexture(Sprite sprite) {
        if (sprite == null) {
            Debug.LogError("Sprite is null. Cannot extract texture.");
            return null;
        }

        // Get the full atlas texture
        Texture2D atlasTexture = sprite.texture;

        // Get the area of the sprite in the atlas
        Rect textureRect = sprite.textureRect;

        // Create a new texture to hold the extracted sprite texture
        Texture2D newTexture = new Texture2D((int)textureRect.width, (int)textureRect.height);

        // Copy the pixels from the atlas to the new texture
        Color[] pixels = atlasTexture.GetPixels(
            (int)textureRect.x,
            (int)textureRect.y,
            (int)textureRect.width,
            (int)textureRect.height
        );
        newTexture.SetPixels(pixels);
        newTexture.Apply();

        return newTexture;
    }
}