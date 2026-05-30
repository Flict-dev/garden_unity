using System;
using UnityEditor;
using UnityEngine;

public sealed class PolyHavenTextureImporter : AssetPostprocessor
{
    private const string TextureRoot = "Assets/Resources/Textures/PolyHaven/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(TextureRoot, StringComparison.Ordinal))
        {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.mipmapEnabled = true;
        importer.alphaSource = TextureImporterAlphaSource.None;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.maxTextureSize = 1024;

        if (assetPath.IndexOf("_nor_", StringComparison.Ordinal) >= 0)
        {
            importer.textureType = TextureImporterType.NormalMap;
            importer.sRGBTexture = false;
            return;
        }

        if (assetPath.IndexOf("_rough_", StringComparison.Ordinal) >= 0 ||
            assetPath.IndexOf("_disp_", StringComparison.Ordinal) >= 0)
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            return;
        }

        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
    }
}
