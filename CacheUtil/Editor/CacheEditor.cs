#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using System;
using UnityEngine;
using Unity.VisualScripting;
using PlasticPipe.PlasticProtocol.Messages;
using System.Collections.Generic;

namespace RJ_TC
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class CacheEditor : Editor
    {
        private static Dictionary<int, CacheUtil> _cacheUtilDict = new Dictionary<int, CacheUtil>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (target is ICaching caching && target is MonoBehaviour mono)
            {
                int instanceID = target.GetInstanceID();
                if (_cacheUtilDict.ContainsKey(instanceID) == false)
                {
                    _cacheUtilDict.Add(instanceID, new CacheUtil(mono));
                }

                var cacheUtil = _cacheUtilDict[instanceID];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                DrawCachingButton(cacheUtil);
                EditorGUILayout.Space(5);

                GUI.enabled = cacheUtil.IsCached;
                DrawGenerateScriptButton(cacheUtil);
                EditorGUILayout.Space(5);
                GUI.enabled = true;

                GUI.enabled = cacheUtil.IsCreatedScript;
                DrawRemoveScriptButton(cacheUtil);
                GUI.enabled = true;

                EditorGUILayout.EndVertical();
            }
        }
        private void OnEnable()
        {
            // 윈도우가 활성화될 때 초기화
            ClearDictionaries();
        }

        private void OnDisable()
        {
            // 윈도우가 비활성화될 때 정리
            ClearDictionaries();
        }

        private void ClearDictionaries()
        {
            _cacheUtilDict.Clear();
        }


        private void DrawCachingButton(CacheUtil caching)
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
                Type type = target.Root.GetType();
                string className = type.Name;
                string namespaceName = type.Namespace;
                target.CreateScript(className);
                CacheUtil.GenerateCScript(className, namespaceName);
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
                Type type = target.Root.GetType();
                string className = type.Name;
                string namespaceName = type.Namespace;
                RemoveCScript(className, namespaceName);
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
        public static void RemoveCachedScript(CacheUtil targetObject)
        {
            //string className = targetObject.name;
            string className = targetObject.Root.GetType().Name;
            string cachedScriptName = $"{className}_C";

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
        public static void RemoveCScript(string className, string namespaceName)
        {
            // CM 스크립트 경로 (예시로 설정)
            string cmScriptPath = CM.sCM_PATH;  // CM 스크립트 파일 경로를 정확히 설정해야 합니다.

            // CM 스크립트 내용 읽기
            try
            {
                string cmScriptContent = File.ReadAllText(cmScriptPath);
                string scriptContent = CM.GetGTCS_String(className, namespaceName);

                if (cmScriptContent.Contains(scriptContent))
                {
                    cmScriptContent = cmScriptContent.Replace(scriptContent, "");

                    File.WriteAllText(cmScriptPath, cmScriptContent);
                    Debug.Log("Script has been removed from C.cs");
                }
                else
                {
                    Debug.Log("The script does not exist in C.cs");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error modifying CM script: " + ex.Message);
            }
        }
    }

}

#endif
