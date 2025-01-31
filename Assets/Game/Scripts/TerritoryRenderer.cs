using UnityEngine;
using System.Collections.Generic;

public class TerritoryRenderer : MonoBehaviour
{
    public Material fillMaterial;
    private RenderTexture renderTexture;

    void Start()
    {
        renderTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        fillMaterial.SetTexture("_MainTex", renderTexture);
    }

    public void UpdateTexture(List<Vector3> points)
    {
        if (points == null || points.Count < 3) return;

        Texture2D texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[1024 * 1024];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        foreach (Vector3 point in points)
        {
            int x = Mathf.Clamp((int)(point.x * 1024), 0, 1023);
            int z = Mathf.Clamp((int)(point.z * 1024), 0, 1023);
            pixels[z * 1024 + x] = Color.red;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        Graphics.Blit(texture, renderTexture);
        Destroy(texture);
    }
}
