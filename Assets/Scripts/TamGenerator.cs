using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TamGenerator : MonoBehaviour
{
    public Texture2D texture { get; set; }
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

        Color[][] tam = new Color[texture.mipmapCount][];

        int width = texture.width;
        int height = texture.height;
        for (int m = 0; m < texture.mipmapCount; m++)
        {
            Color[] mip = new Color[width * height]; 
            tam[m] = mip;
            Color color = Color.Lerp(Color.red, Color.blue, (float)m / (float)texture.mipmapCount);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    mip[y * width + x] = color;
                }
            }

            width /= 2;
            height /= 2;
        }

        for (int m = 0; m < texture.mipmapCount; m++)
        {
            texture.SetPixels(tam[m], m);
        }
        
        texture.Apply(false);
    }
}
