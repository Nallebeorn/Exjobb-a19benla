using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class TamGenerator : MonoBehaviour
{
    private const string strokeTexturePath = "BadStroke";
    private const string outPath = "Assets/Textures/Tam.gen.asset";
    private const float desiredTone = 0.9f;

    public struct TextureColors
    {
        public int width;
        public int height;
        public Color32[] colors;

        public TextureColors(Texture2D texture, int mipLevel = 0)
        {
            width = texture.width >> mipLevel;
            height = texture.height >> mipLevel;
            colors = texture.GetPixels32(mipLevel);
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

        public void WriteToTexture(Texture2D texture, int mipLevel)
        {
            texture.SetPixels32(colors, mipLevel);
        }
    }
    
    [MenuItem("Line Art/Generate TAM texture %#t")]
    private static void GenerateTextureMap()
    {
        double t0 = Time.realtimeSinceStartupAsDouble;

        Texture2D texture = new Texture2D(2048, 2048, TextureFormat.RGBA32, true, true);
        Texture2D strokeTexture = Resources.Load<Texture2D>(strokeTexturePath);

        TextureColors stroke = new TextureColors(strokeTexture);
        TextureColors[] mips = new TextureColors[texture.mipmapCount];
        float[] averageTonesBefore = new float[texture.mipmapCount];
        float[] averageTonesAfter = new float[texture.mipmapCount];

        for (int m = 0; m < texture.mipmapCount; m++)
        {
            mips[m] = new TextureColors(texture, m);
            mips[m].Fill(Color.white);
            averageTonesBefore[m] = 1.0f;
        }

        for (int m = texture.mipmapCount - 1; m >= 0; m--)
        {
            for (int failsafe = 0; failsafe < 4096; failsafe++)
            {
                if (CalculateTone(mips[m]) <= desiredTone)
                {
                    Debug.Log("Reached tone for " + m);
                    break;
                }

                float s = Random.value;
                float t = Random.value;

                for (int mm = m; mm >= 0; mm--)
                {
                    BlitWrapped(new Vector2(s, t), mips[mm], stroke);
                }

                //
                // for (int mm = m; mm >= 0; mm--)
                // {
                //     averageTonesAfter[mm] = CalculateTone(mips[mm]);
                // }
            }
        }

        for (int m = 0; m < texture.mipmapCount; m++)
        {
            mips[m].WriteToTexture(texture, m);
            Debug.Log($"Resulting tone of mip {m}: {CalculateTone(mips[m])}");
        }

        texture.Apply(false);

        texture.filterMode = FilterMode.Trilinear;
        texture.anisoLevel = 16;

        CreateOrReplaceAsset(texture, outPath);
        
        double timeSpent = Time.realtimeSinceStartupAsDouble - t0;
        Debug.Log($"Generated texture {outPath} in {timeSpent:F3}s");
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

    static float CalculateTone(TextureColors tex)
    {
        int sum = 0;
        for (int i = 0; i < tex.colors.Length; i++)
        {
            sum += tex.colors[i].r;
        }

        return ((float)sum / tex.colors.Length) / 255;
    }

    static int Mod(int x, int modulo)
    {
        return (x % modulo + modulo) % modulo;
    }

    static T CreateOrReplaceAsset<T>(T asset, string path) where T : UnityEngine.Object
    {
        T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (existingAsset == null)
        {
            AssetDatabase.CreateAsset(asset, path);
            existingAsset = asset;
        }
        else
        {
            EditorUtility.CopySerialized(asset, existingAsset);
            AssetDatabase.SaveAssets();
        }

        return existingAsset;
    }
}