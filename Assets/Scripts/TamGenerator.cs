using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TamGenerator : MonoBehaviour
{
    public Texture2D strokeTexture;
    public Texture2D texture;
    public MeshRenderer[] textureTheseMeshes;

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
        
        Color[] stroke = strokeTexture.GetPixels();

        Color[][] tam = new Color[texture.mipmapCount][];

        int tamWidth = texture.width;
        int tamHeight = texture.height;
        int strokeWidth = strokeTexture.width;
        int strokeHeight = strokeTexture.height;

        for (int m = 0; m < texture.mipmapCount; m++)
        {
            Color[] mip = new Color[tamWidth * tamHeight];
            tam[m] = mip;
            // Color color = Color.Lerp(Color.red, Color.blue, (float)m / (float)texture.mipmapCount);
            // for (int x = 0; x < width; x++)
            // {
            //     for (int y = 0; y < height; y++)
            //     {
            //         mip[y * width + x] = color;
            //     }
            // }

            for (int i = 0; i < mip.Length; i++)
            {
                mip[i] = Color.white;
            }

            int offsetX = tamWidth / 2 - strokeWidth / 2;
            int offsetY = tamHeight / 2 - strokeHeight / 2;
            for (int x = 0; x < strokeWidth; x++)
            {
                for (int y = 0; y < strokeHeight; y++)
                {
                    Color strokePixel = stroke[x + y * strokeWidth];
                    if (strokePixel.a > 0.001f)
                    {
                        int tamX = offsetX + x;
                        int tamY = offsetY + y;
                        if (tamX < 0 || tamX >= tamWidth || tamY < 0 || tamY >= tamHeight)
                        {
                            continue;
                        }

                        mip[tamX + tamY * tamWidth] = strokePixel;
                    }
                }
            }

            tamWidth /= 2;
            tamHeight /= 2;
        }

        for (int m = 0; m < texture.mipmapCount; m++)
        {
            texture.SetPixels(tam[m], m);
        }

        texture.Apply(false);

        texture.filterMode = FilterMode.Trilinear;
        // texture.anisoLevel = 16;
    }
}