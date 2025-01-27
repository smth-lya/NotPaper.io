using System.Collections.Generic;
using UnityEngine;

public class TerritoryRenderer : MonoBehaviour
{
    [SerializeField] private Material _fillMaterial;

    private RenderTexture _renderTexture;

    private void Awake()
    {
        _renderTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        _fillMaterial.SetTexture("_MainTex", _renderTexture);
    }

    public void UpdateTexture(List<Vector3> points)
    {
        if (points == null || points.Count < 3) return;

        Texture2D texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[1024 * 1024];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        foreach (Vector2 point in points)
        {
            int x = Mathf.Clamp((int)(point.x * 1024), 0, 1023);
            int y = Mathf.Clamp((int)(point.y * 1024), 0, 1023);
            pixels[y * 1024 + x] = Color.red;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        Graphics.Blit(texture, _renderTexture);
        Destroy(texture);
    }
}