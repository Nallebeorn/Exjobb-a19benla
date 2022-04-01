using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TamGenerator : MonoBehaviour
{
    public Texture2D strokeTexture;
    public Texture2D texture;
    public MeshRenderer[] textureTheseMeshes;

    public struct TextureColors
    {
        public int width;
        public int height;
        public Color32[] colors;
        public readonly int mip;

        public TextureColors(Texture2D texture, int mipLevel = 0)
        {
            width = texture.width >> mipLevel;
            height = texture.height >> mipLevel;
            colors = texture.GetPixels32(mipLevel);
            mip = mipLevel;
        }

        public void SetPixel(int x, int y, Color32 color)
        {
            colors[x + y * width] = color;
        }

        public Color32 GetPixel(int x, int y)
        {
            return colors[x + y * width];
        }

        public void Fill(Color32 color)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    colors[x + y * width] = color;
                }
            }
        }

        public void WriteToTexture(Texture2D texture)
        {
            texture.SetPixels32(colors, mip);
        }
    }

    private void Start()
    {
        GenerateTextureMap();

        foreach (MeshRenderer mesh in textureTheseMeshes)
        {
            mesh.material = new Material(mesh.sharedMaterial.shader);
            mesh.material.SetTexture("_BaseMap", texture);
            mesh.material.SetTexture("_MainTex", texture);
        }
    }

    private void GenerateTextureMap()
    {
        texture = new Texture2D(2048, 2048, TextureFormat.RGBA32, true, true);

        TextureColors stroke = new TextureColors(strokeTexture);
        TextureColors[] mips = new TextureColors[texture.mipmapCount];

        for (int m = 0; m < texture.mipmapCount; m++)
        {
            mips[m] = new TextureColors(texture, m);
            mips[m].Fill(Color.white);
        }

        for (int m = texture.mipmapCount - 1; m >= 0; m--)
        {
            for (int i = 0; i < 5; i++)
            {
                float s = Random.value;
                float t = Random.value;
                for (int mm = m; mm >= 0; mm--)
                {
                    BlitWrapped(new Vector2(s, t), mips[mm], stroke);
                }
            }
        }

        for (int m = 0; m < texture.mipmapCount; m++)
        {
            mips[m].WriteToTexture(texture);
        }
        
        texture.Apply(false);

        texture.filterMode = FilterMode.Trilinear;
        // texture.anisoLevel = 16;
    }

    static void BlitWrapped(Vector2 uv, TextureColors target, TextureColors image)
    {
        Vector2Int pixelCoords = new Vector2Int(Mathf.RoundToInt(uv.x * target.width), Mathf.RoundToInt(uv.y * target.height));
        int offsetX = pixelCoords.x - image.width / 2;
        int offsetY = pixelCoords.y - image.height / 2;

        for (int imgx = 0; imgx < image.width; imgx++)
        {
            for (int imgy = 0; imgy < image.height; imgy++)
            {
                int targetX = Mod(offsetX + imgx, target.width);
                int targetY = Mod(offsetY + imgy, target.height);
                Color32 srcPixel = image.GetPixel(imgx, imgy);
                Color32 dstPixel = target.GetPixel(targetX, targetY);
                Color32 newColor = Color32.Lerp(dstPixel, srcPixel, srcPixel.a / 255.0f);
                target.SetPixel(targetX, targetY, newColor);
            }
        }
    }

    static int Mod(int x, int modulo)
    {
        return (x % modulo + modulo) % modulo;
    }
}