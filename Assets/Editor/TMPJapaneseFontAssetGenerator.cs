#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class TMPJapaneseFontAssetGenerator
{
    [MenuItem("Tools/TMP/Create Dynamic Font Asset From Selected Font")]
    public static void CreateFromSelectedFont()
    {
        Object selected = Selection.activeObject;

        if (selected == null)
        {
            EditorUtility.DisplayDialog(
                "TMP Font Asset Generator",
                "Projectウィンドウで日本語フォント（TTF/OTF）を選択してください。",
                "OK"
            );
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(selected);

        if (string.IsNullOrEmpty(sourcePath))
        {
            EditorUtility.DisplayDialog(
                "TMP Font Asset Generator",
                "選択中のアセットパスを取得できませんでした。",
                "OK"
            );
            return;
        }

        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);

        if (sourceFont == null)
        {
            EditorUtility.DisplayDialog(
                "TMP Font Asset Generator",
                "選択したファイルはUnityでFontとして読み込まれていません。\nTTFまたはOTFを選択してください。",
                "OK"
            );
            return;
        }

        string folder = Path.GetDirectoryName(sourcePath)?.Replace("\\", "/");

        if (string.IsNullOrEmpty(folder))
            folder = "Assets";

        string sourceName = Path.GetFileNameWithoutExtension(sourcePath);
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(
            folder + "/" + sourceName + "_TMP_Dynamic.asset"
        );

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);

        if (fontAsset == null)
        {
            EditorUtility.DisplayDialog(
                "TMP Font Asset Generator",
                "TMP Font Assetの生成に失敗しました。",
                "OK"
            );
            return;
        }

        fontAsset.name = sourceName + "_TMP_Dynamic";
        fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;

        AssetDatabase.CreateAsset(fontAsset, assetPath);

        if (fontAsset.material != null)
        {
            fontAsset.material.name = sourceName + "_TMP_Dynamic Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        if (fontAsset.atlasTextures != null)
        {
            foreach (Texture2D atlasTexture in fontAsset.atlasTextures)
            {
                if (atlasTexture == null)
                    continue;

                atlasTexture.name = sourceName + "_TMP_Dynamic Atlas";
                AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
            }
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = fontAsset;
        EditorGUIUtility.PingObject(fontAsset);

        EditorUtility.DisplayDialog(
            "TMP Font Asset Generator",
            "日本語対応のDynamic TMP Font Assetを作成しました。\n\n" +
            assetPath +
            "\n\nResultTextやResultWorldBoardBinderのResult Fontに設定してください。",
            "OK"
        );
    }

    [MenuItem("Tools/TMP/Create Dynamic Font Asset From Selected Font", true)]
    private static bool ValidateCreateFromSelectedFont()
    {
        if (Selection.activeObject == null)
            return false;

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(path))
            return false;

        return AssetDatabase.LoadAssetAtPath<Font>(path) != null;
    }
}
#endif
