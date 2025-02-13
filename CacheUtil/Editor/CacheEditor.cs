using System.IO;
using UnityEditor;
using System;
using UnityEngine;
using Unity.VisualScripting;
using PlasticPipe.PlasticProtocol.Messages;

namespace RJ_TC
{
[CustomEditor(typeof(MonoBehaviour), true)]
public class CacheEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (target is ICaching caching)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawCachingButton(caching);
            EditorGUILayout.Space(5);

            GUI.enabled = caching.IsCached;
            DrawGenerateScriptButton(target as CacheUtil);
            EditorGUILayout.Space(5);
            GUI.enabled = true;

            GUI.enabled = caching.IsCreatedScript;
            DrawRemoveScriptButton(target as CacheUtil);
            GUI.enabled = true;

            EditorGUILayout.EndVertical();
        }
    }

    private void DrawCachingButton(ICaching caching)
    {
        var buttonStyle = CreateExpandingButtonStyle(Color.blue);
        var prevColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);

        if (GUILayout.Button("Cache Object", buttonStyle))
        {
            caching.Caching();
        }
        GUI.backgroundColor = prevColor;
    }

    private void DrawGenerateScriptButton(CacheUtil target)
    {
        var buttonStyle = CreateExpandingButtonStyle(Color.green);
        var prevColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.3f, 0.9f, 0.3f);

        if (GUILayout.Button("Generate Script", buttonStyle))
        {
            string className = target.GetType().Name;
            target.CreateScript(className);
            CacheUtil.GenerateCMScript(className);
        }
        GUI.backgroundColor = prevColor;
    }

    private void DrawRemoveScriptButton(CacheUtil target)
    {
        var buttonStyle = CreateExpandingButtonStyle(Color.red);
        var prevColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);

        if (GUILayout.Button("Remove Cached Script", buttonStyle))
        {
            string className = target.GetType().Name;
            RemoveCMScript(className);
            RemoveCachedScript(target);
        }
        GUI.backgroundColor = prevColor;
    }

    private GUIStyle CreateExpandingButtonStyle(Color hoverColor)
    {
        var style = new GUIStyle(GUI.skin.button);
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;
        style.fixedHeight = 30;
        style.hover.textColor = Color.yellow;
        style.hover.scaledBackgrounds = new Texture2D[] { MakeColorTexture(hoverColor * 0.8f) };

        // ���콺 ������ ���� ũ�� Ȯ��
        style.margin = new RectOffset(5, 5, 0, 0);
        style.padding = new RectOffset(10, 10, 5, 5);
        style.stretchWidth = true;
        style.hover.textColor = Color.white;
        style.onHover.textColor = Color.white;
        style.onHover.scaledBackgrounds = style.hover.scaledBackgrounds;

        return style;
    }
    // Create texture for hover background
    private Texture2D MakeColorTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
    public static void RemoveCachedScript(MonoBehaviour targetObject)
    {
        if (targetObject == null)
        {
            Debug.LogError("it is not valid objects");
            return;
        }

        //string className = targetObject.name;
        string className = targetObject.GetType().Name;
        string cachedScriptName = $"{className}_C";

        // Unity ������Ʈ ������ �ش� Cached Ŭ���� ���� ã��
        string[] guids = AssetDatabase.FindAssets(cachedScriptName);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == cachedScriptName)
            {
                if (AssetDatabase.DeleteAsset(path))
                {
                    Debug.Log($"Removed cached script {path}");
                    AssetDatabase.Refresh();
                    return;
                }
                else
                {
                    Debug.LogError("Failed to remove cached script");
                }
            }
        }
        Debug.LogWarning("Cannot find cached script");
    }
    public static void RemoveCMScript(string className)
    {
        // CM 스크립트 경로 (예시로 설정)
        string cmScriptPath = CM.sCM_PATH;  // CM 스크립트 파일 경로를 정확히 설정해야 합니다.

        // CM 스크립트 내용 읽기
        try
        {
            // CM.cs 파일을 읽어서 내용을 가져옵니다.
            string cmScriptContent = File.ReadAllText(cmScriptPath);

            // 제거할 코드 내용
            string scriptContent = $@"
    public static {className}{CM.sCS} G_TC({className} obj) => G_TCI(obj) as {className}{CM.sCS};
";

            // 코드가 존재하는지 확인하고, 존재하면 제거
            if (cmScriptContent.Contains(scriptContent))
            {
                // CM 스크립트에서 해당 코드 제거
                cmScriptContent = cmScriptContent.Replace(scriptContent, "");

                // 수정된 내용을 파일에 다시 저장
                File.WriteAllText(cmScriptPath, cmScriptContent);

                Debug.Log("Script has been removed from CM.cs");
            }
            else
            {
                Debug.Log("The script does not exist in CM.cs");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error modifying CM script: " + ex.Message);
        }
    }
}

}

