using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TAM Settings", menuName = "TAM Settings")]
public class TamSettings : ScriptableObject
{
    public bool regenerate;
    [Space]
    public string strokeTexturePath = "BadStroke";
    public string outPath = "Assets/Textures/Tam.gen.asset";
    public int maximumStrokes = 128;
    public Vector2Int size = new Vector2Int(256, 256);
    [Range(0, 1)] public float strokeMinLength = 0.1f;
    [Range(0, 1)] public float strokeMaxLength = 0.6f;

    public int numSnapshots = 4;
    [Range(0, 1)] public float startCrossHatching = 0.5f;
    [Range(0, 1)] public float startDoubleHatching = 0.75f;

    private void OnValidate()
    {
        if (regenerate)
        {
            regenerate = false;
            TamGenerator.GenerateTextureMap();
        }
    }
}
