using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class CacheUtil : MonoBehaviour, ICaching
{
    private Dictionary<string, string> _transformNameList = new Dictionary<string, string>();
    private Dictionary<int, string> _transformList = new Dictionary<int, string>();
    private List<Transform> _transforms = new List<Transform>();

    public bool IsCached { get => _transformNameList.Count > 0; }
    public bool IsCreatedScript
    {
        get
        {
            //, $"{this.GetType().Name}{CM.sCS}"
            string path = $"{Application.dataPath}/RJ/Script_Cached/{this.GetType().Name}{CM.sCS}.cs";
            bool isExist = File.Exists(path);
            return isExist;
        }
    }
    public virtual bool Caching()
    {
        _transformNameList = new Dictionary<string, string>();
        _transformList = new Dictionary<int, string>();
        _transforms.Clear();

        GetTransformsExceptSearchBlocker(transform);

        foreach (Transform t in _transforms)
        {
            if (t == transform) continue;

            StringBuilder sb = new StringBuilder();
            sb.Append(t.name);

            Transform parent = t.parent;
            int callcnt = 0;

            while (parent != null && parent != this.transform)
            {
                if (_transformList.TryGetValue(parent.GetHashCode(), out string name))
                {
                    sb.Insert(0, $"{name}/");
                    break;
                }
                else
                {
                    sb.Insert(0, $"{parent.name}/");
                }

                parent = parent.parent;
                callcnt++;

                if (callcnt >= 10000)
                {
                    Debug.LogError($"CallCnt is over 10000");
                    return false;
                }
            }

            _transformList.Add(t.GetHashCode(), sb.ToString());
        }
        foreach (var a in _transformList)
        {
            string fullPath = a.Value;
            string[] parts = fullPath.Split('/');
            string variableName = SanitizeVariableName(parts.Last());
            variableName = GetNameOfResult(variableName);
            _transformNameList.Add(variableName, fullPath);
        }

        return true;
    }

    public void MakingScript()
    {
        string cacheClassName = this.GetType().Name;
        CreateScript(cacheClassName);
    }
    public virtual void CreateScript(string cacheClassName)
    {
        string className = $"{cacheClassName}{CM.sCS}";
        className = SanitizeVariableName(className);

        // Create directory path
        string directoryPath = Path.Combine(Application.dataPath, "RJ", "Script_Cached");

        // Create directories if they don't exist
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string path = Path.Combine(directoryPath, $"{className}.cs");

        StringBuilder contentBuilder = new StringBuilder();
        contentBuilder.Append($@"using UnityEngine;
using System.Collections.Generic;

public class {className} : ICached
{{
    private Dictionary<string, Transform> _cachedTrmDictionary = new();
    public Transform Root {{get; set;}}

    public {className}(Transform root)
    {{
        Root = root;
    }}


    public Transform GetCachedTrm(string key)
    {{
        if (_cachedTrmDictionary.ContainsKey(key))
        {{
            return _cachedTrmDictionary[key];
        }}
        
        if (_namePathDictionary != null && _namePathDictionary.TryGetValue(key, out string path))
        {{
            Transform findTrm = Root.Find(path);
            if (findTrm == null)
            {{
                Debug.LogError($""Cannot cache transform!"");
                return null;
            }}
            _cachedTrmDictionary.Add(key, findTrm);
            return findTrm;
        }}
        
        return null;
    }}
");

        contentBuilder.Append("\nprivate Dictionary<string, string> _namePathDictionary= new Dictionary<string,string>()\n{");
        foreach (var kvp in _transformNameList)
        {
            contentBuilder.Append($"{{\"{kvp.Key}\",\"{kvp.Value}\"}},\n");
        }
        contentBuilder.Append("};\n");
        foreach (var kvp in _transformNameList)
        {
            contentBuilder.Append($"    public Transform {kvp.Key} => GetCachedTrm(\"{kvp.Key}\");\n");
        }

        // 🔹 클래스 닫는 중괄호 추가
        contentBuilder.Append("}");

        File.WriteAllText(path, contentBuilder.ToString());
#if UNITY_EDITOR
        AssetDatabase.Refresh(); // 에디터에서 새 파일 인식하도록 갱신
#endif
    }

    public static void GenerateCMScript(string className)
    {
        string scriptContent = $@"
    public static {className}{CM.sCS} G_TC({className} obj) => G_TCI(obj) as {className}{CM.sCS};
";  // 삽입할 코드 내용

        string filePath = CM.sCM_PATH; // 수정할 파일 경로

        // 파일을 읽어들입니다.
        string fileContent = File.ReadAllText(filePath);

        if(fileContent.Contains(scriptContent))
        {
            Debug.Log($"이미 중복된 코드가 추가되어있는 상태입니다.");
            return;
        }

        // 클래스 내부에 삽입할 코드 위치를 찾습니다.
        string insertPositionPattern = "//cachedGetMethod";
        int insertPosition = fileContent.IndexOf(insertPositionPattern);

        if (insertPosition == -1)
        {
            Debug.LogError($"패턴 '{insertPositionPattern}'을 찾을 수 없습니다.");
            return;
        }

        // 해당 패턴의 위치 다음 줄에 삽입하도록 수정
        insertPosition += insertPositionPattern.Length; // 패턴 끝 위치
        if (insertPosition < fileContent.Length && fileContent[insertPosition] == '\n')
        {
            insertPosition++; // 만약 바로 다음이 줄바꿈 문자라면 그 위치를 넘김
        }

        // 새 코드 내용을 삽입합니다.
        string updatedContent = fileContent.Substring(0, insertPosition) + scriptContent + fileContent.Substring(insertPosition);

        // 수정된 내용을 파일에 씁니다.
        File.WriteAllText(filePath, updatedContent);

        Debug.LogError("코드가 성공적으로 추가되었습니다.");
    }

    private string GetNameOfResult(string result, int callCnt = 0)
    {
        string callCntResult = callCnt < 1 ? result : $"{result}_{callCnt - 1}";
        if (_transformNameList.ContainsKey(callCntResult))
        {
            return GetNameOfResult(result, callCnt + 1);
        }
        return callCntResult;
    }

    public static string SanitizeVariableName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "_"; // 빈 문자열이면 기본값 반환

        // 공백을 제거한 후, 특수 문자는 _로 바꾸는 정규식
        string sanitized = Regex.Replace(input, @"[^a-zA-Z0-9_]", "_");

        // 공백도 제거 (공백을 아예 없애기)
        sanitized = sanitized.Replace(" ", "");

        // 첫 글자가 숫자라면 앞에 _ 추가
        if (char.IsDigit(sanitized[0]))
            sanitized = "_" + sanitized;

        return sanitized;
    }

    private void GetTransformsExceptSearchBlocker(Transform parent)
    {
        var searchBlocker = parent.GetComponent<SearchBlocker>();
        if (searchBlocker != null)
        {
            if (!searchBlocker.EnableSearchChildren)
                return;
            else
            {
                foreach (Transform child in parent)
                {
                    GetTransformsExceptSearchBlocker(child);
                }
                return;
            }
        }
        else
        {
            _transforms.Add(parent);
        }

        foreach (Transform child in parent)
        {
            GetTransformsExceptSearchBlocker(child);
        }
    }
}
