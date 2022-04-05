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
    private const float desiredTone = 0.75f;

    public struct TextureColors
    {
        public int width;
        public int height;
        public Vector2Int size => new Vector2Int(width, height);
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

        public Color32 SampleLinear(float x, float y)
        {
            int yPoint = Mathf.RoundToInt(y);
            Color32 col1 = GetPixel(Mod(Mathf.FloorToInt(x), width), yPoint);
            Color32 col2 = GetPixel(Mod(Mathf.CeilToInt(x), height), yPoint);
            return Color32.Lerp(col1, col2, Frac(x));
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
            // int pixelWidth = Mathf.FloorToInt(0.5f * mips[m].width);
            // BlitWrapped(new Vector2(0.5f, 0.5f), new Vector2Int(pixelWidth, stroke.height), mips[m], stroke);

            for (int failsafe = 0; failsafe < 256; failsafe++)
            {
                float s = Random.value;
                float t = Random.value;
                float length = Random.Range(0.3f, 1.0f);

                TextureColors tmp = new TextureColors(texture, m);
                Array.Copy(mips[m].colors, tmp.colors, Mathf.Min(tmp.colors.Length, mips[m].colors.Length));
                int pixelWidth = Mathf.RoundToInt(length * tmp.width);
                if (pixelWidth == 0)
                {
                    if (desiredTone >= 0.5f)
                    {
                        break;
                    }
                    else
                    {
                        pixelWidth = 1;
                    }
                }
                BlitWrapped(new Vector2(s, t), new Vector2Int(pixelWidth, stroke.height), tmp, stroke);

                float newTone = CalculateTone(tmp);
                
                if (newTone < desiredTone)
                {
                    Debug.Log("Reached tone for " + m);
                    break;
                }

                for (int mm = m; mm >= 0; mm--)
                {
                    pixelWidth = Mathf.Max(1, Mathf.RoundToInt(length * mips[mm].width));
                    BlitWrapped(new Vector2(s, t), new Vector2Int(pixelWidth, stroke.height), mips[mm], stroke);
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
        texture.mipMapBias = -0.5f;

        CreateOrReplaceAsset(texture, outPath);

        double timeSpent = Time.realtimeSinceStartupAsDouble - t0;
        Debug.Log($"Generated texture {outPath} in {timeSpent:F3}s");
    }

    static void BlitWrapped(Vector2 uv, Vector2Int size, TextureColors target, TextureColors image)
    {
        Vector2Int pixelCoords = new Vector2Int(Mathf.RoundToInt(uv.x * target.width), Mathf.RoundToInt(uv.y * target.height));
        int offsetX = pixelCoords.x - size.x / 2;
        int offsetY = pixelCoords.y - size.y / 2;

        for (int imgx = 0; imgx < size.x; imgx++)
        {
            for (int imgy = 0; imgy < size.y; imgy++)
            {
                int targetX = Mod(offsetX + imgx, target.width);
                int targetY = Mod(offsetY + imgy, target.height);
                float sourceX = ((float)imgx / size.x) * image.width;
                float sourceY = ((float)imgy / size.y) * image.height;
                Color32 srcPixel = image.SampleLinear(sourceX, sourceY);
                Color32 dstPixel = target.GetPixel(targetX, targetY);
                Color32 newColor = Color32.Lerp(dstPixel, srcPixel, srcPixel.a / 255.0f);
                target.SetPixel(targetX, targetY, newColor);
            }
        }
    }

    static float Frac(float x)
    {
        return Mathf.Repeat(x, 1.0f);
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