using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace RJ_TC
{
    public class CacheEditorWindow : EditorWindow
    {
        private Dictionary<GameObject, bool> _toggleStates = new Dictionary<GameObject, bool>();
        private List<GameObject> _prefabList = new List<GameObject>();
        private Type _parentClassType;
        private string _debugString = string.Empty;

        private string[] _cachingClassNames;
        private int _selectedClassIndex;
        private bool _selectAll = false;

        // 윈도우를 여는 버튼
        [MenuItem("Caching/CacheEditor")]
        public static void ShowWindow()
        {
            // 윈도우를 열기
            CacheEditorWindow window = GetWindow<CacheEditorWindow>("Cache Editor");
            window.Show();
        }

        // 윈도우의 내용을 그리는 부분
        private void OnGUI()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.normal.textColor = Color.red; // 글자 색을 빨간색으로 변경
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label("This is for a caching window", titleStyle);
            GUILayout.Label($"Debug: {_debugString}");
            GUILayout.Space(40);

            if (DisplaySelectingParentType() == false) return;
            DisplayTogglePrefabs();
            DisplayCachingButton();
        }

        private bool DisplaySelectingParentType()
        {
            if (_cachingClassNames == null)
            {
                _cachingClassNames = GetCachingClassNames();
            }

            // 선택할 수 있는 클래스 팝업 메뉴 생성
            try
            {
                _selectedClassIndex = EditorGUILayout.Popup("Parent Class", _selectedClassIndex, _cachingClassNames);
                _parentClassType = FindTypeByFullName(_cachingClassNames[_selectedClassIndex]);
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}");
                _selectedClassIndex = 0;
                _parentClassType = null;
            }

            if (_parentClassType == null)
            {
                EditorGUILayout.HelpBox("Please select a valid class.", MessageType.Warning);
                return false;
            }
            return true;
        }

        private void DisplayTogglePrefabs()
        {
            GUILayout.Space(20);
            try
            {
                if (_toggleStates.Count < 1) return;

                GUILayout.BeginHorizontal();
                //GUILayout.FlexibleSpace();
                if (GUILayout.Button(_selectAll ? "Deselect All" : "Select All"))
                {
                    // 전체 선택 또는 해제
                    _selectAll = !_selectAll;
                    SetAllToggleStates(_selectAll);
                }
                if (GUILayout.Button($"Clear"))
                {
                    _toggleStates.Clear();
                    _prefabList.Clear();
                    _selectAll = false;
                    _debugString = string.Empty;
                }
                //GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(20);

                GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.normal.textColor = Color.green; // 글자 색을 빨간색으로 변경
                titleStyle.fontSize = 15;
                GUILayout.Label("Founded prefab names", titleStyle);

                // Horizontal layout을 사용하여 가로로 배치
                GUILayout.BeginHorizontal();
                float buttonWidth = 150f; // 버튼의 가로 크기 설정
                int itemsPerRow = Mathf.FloorToInt(position.width / buttonWidth); // 한 행에 배치할 버튼 개수

                // Prefab 목록을 가져오고 각 항목을 토글
                int count = 0;
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.normal.textColor = Color.yellow; // 글자 색을 빨간색으로 변경

                foreach (var prefab in _toggleStates.Keys.ToList())
                {
                    string className = prefab.name;
                    if (prefab.TryGetComponent(out CacheUtil cacheUtil))
                    {
                        className = cacheUtil.GetType().Name;
                    }
                    string cacheClassName = $"{(className)}{CM.sCS}";
                    bool findedCachedClass = FindCachClass(cacheClassName);


                    bool newState = GUILayout.Toggle(_toggleStates[prefab], prefab.name, GUILayout.Width(buttonWidth));
                    if (newState != _toggleStates[prefab])
                    {
                        // 개별 토글이 변경되면 _selectAll 상태 갱신
                        _toggleStates[prefab] = newState;
                        UpdateSelectAllState();
                    }

                    if (findedCachedClass)
                    {
                        // 라벨 추가
                        GUILayout.Label("already cached", labelStyle); // 토글 옆에 라벨을 추가
                    }

                    count++;

                    // 한 행에 배치된 버튼이 itemsPerRow 만큼 되면 줄 바꿈
                    if (count >= itemsPerRow)
                    {
                        GUILayout.EndHorizontal(); // 현재 행을 끝냄
                        GUILayout.BeginHorizontal(); // 새로운 행을 시작
                        count = 0; // 카운트를 초기화
                    }
                }

                // 마지막 행 끝을 위해 엔딩
                GUILayout.EndHorizontal();
            }
            catch
            {
                _toggleStates.Clear();
                _prefabList.Clear();
            }
        }

        private bool FindCachClass(string v)
        {
            return File.Exists($"{CM.sCS_PATH}{v}.cs");
        }

        private void DisplayCachingButton()
        {
            var parentType = _parentClassType;

            GUILayout.Space(20);

            // 버튼을 수평으로 배치
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // 초록색 버튼
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Finding Objects", GUILayout.Width(200)))
            {
                if (parentType == null) return;
                FindCachingObjects(parentType);
            }

            // 노란색 버튼
            GUI.backgroundColor = Color.yellow;
            GUI.enabled = _toggleStates.Count > 0;
            if (GUILayout.Button("Caching & Setting Scripts", GUILayout.Width(200)))
            {
                if (parentType == null) return;
                SettingScriptObjects(parentType);
            }

            // 빨간색 버튼
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button($"Remove Scripts", GUILayout.Width(200)))
            {
                if (parentType == null) return;
                RemoveScripts();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // 색상 초기화 (다음 UI에 영향을 미치지 않도록)
            GUI.backgroundColor = Color.white;

        }

        private void SetAllToggleStates(bool state)
        {
            foreach (var prefab in _toggleStates.Keys.ToList())
                _toggleStates[prefab] = state;
        }

        private void UpdateSelectAllState()
        {
            // 전체 선택/해제 상태 갱신
            if (_toggleStates.Values.All(state => state)) // 모든 항목이 선택됨
                _selectAll = true;
            else if (_toggleStates.Values.All(state => !state)) // 모든 항목이 해제됨
                _selectAll = false;
            else // 일부 항목이 선택됨
                _selectAll = false; // "부분 선택" 상태를 지원할 수 있지만, 이 경우 "전체 선택" 버튼을 비활성화로 할 수 있음
        }
        private void SettingScriptObjects(Type parentType)
        {
            if (_toggleStates.Count > 0)
            {
                foreach (var state in _toggleStates)
                {
                    if (state.Value)
                    {
                        // 여기서 필요한 캐시 작업을 진행하세요.
                        if (state.Key.TryGetComponent(out CacheUtil mono))
                        {
                            string className = mono.GetType().Name;
                            mono.Caching();
                            mono.CreateScript(className);
                            CacheUtil.GenerateCgetScript(className);
                        }
                    }
                }
            }
        }
        private void RemoveScripts()
        {
            if (_toggleStates.Count > 0)
            {
                Debug.Log($"ToggleStatesCount: {_toggleStates.Count}");
                foreach (var state in _toggleStates)
                {
                    if (state.Value)
                    {
                        var prefab = state.Key;
                        string className = prefab.name;
                        if (prefab.TryGetComponent(out CacheUtil cacheUtil))
                        {
                            className = cacheUtil.GetType().Name;
                        }
                        //string cachedScriptName = CacheUtil.SanitizeVariableName($"{className}{CM.sCS}");
                        string cachedScriptName = $"{className}{CM.sCS}";

                        Debug.Log($"CachedScriptName: {cachedScriptName}");
                        CacheEditor.RemoveCMScript(className);

                        // Unity 프로젝트 내에서 해당 Cached 클래스 파일 찾기
                        string[] guids = AssetDatabase.FindAssets(cachedScriptName);
                        foreach (string guid in guids)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(guid);
                            if (Path.GetFileNameWithoutExtension(path) == cachedScriptName)
                            {
                                if (AssetDatabase.DeleteAsset(path))
                                {
                                    string debugContent = $"Cached script removed: {path}";
                                    _debugString = debugContent;
                                    Debug.Log(debugContent);
                                    AssetDatabase.Refresh();
                                }
                                else
                                {
                                    string debugContent = $"Failed to remove cached script: {path}";
                                    _debugString = debugContent;
                                    Debug.LogError(debugContent);
                                }
                            }
                        }
                    }
                }
            }
        }
        private void FindCachingObjects(Type parentType)
        {
            _toggleStates.Clear();
            _prefabList.Clear();

            // 프로젝트에서 해당 타입을 가진 프리팹들을 찾는 방법
            var guids = AssetDatabase.FindAssets("t:Prefab");
            List<GameObject> prefabList = new List<GameObject>();

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null && prefab.GetComponent(parentType) != null)
                {
                    prefabList.Add(prefab);
                }
            }
            _prefabList = prefabList;

            // 찾은 프리팹 리스트에서 토글 UI를 생성
            foreach (var prefab in prefabList)
            {
                if (!_toggleStates.ContainsKey(prefab))
                {
                    _toggleStates[prefab] = false; // Default to false (not selected)
                }
            }
            string debugContent = $"Founded CachedObj Count: {_toggleStates.Count}";
            _debugString = debugContent;
            Debug.Log(debugContent);
            _selectAll = false;
        }
        private string[] GetCachingClassNames()
        {
            var cachingClassTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(CacheUtil).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .ToList();

            // Ÿ���� ���ڿ� �迭�� ��ȯ�Ͽ� ��ȯ
            return cachingClassTypes.Select(type => type.FullName).ToArray();
        }

        private Type FindTypeByFullName(string fullName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }
    }

}

