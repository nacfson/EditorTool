using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace RJ_TC
{
    public static class CM
    {
        public static readonly string sCS = "_C";
        public static readonly string sCS_PATH = $"{Application.dataPath}/RJ/Script_Cached/";
        public static readonly string sCM_PATH = $"{Application.dataPath}/RJ/Script_Cached/C.cs"; // File path to check and create

        private static Dictionary<int, ICached> s_cacheUtilDictonary = new();

        public static ICached G_TCI(MonoBehaviour obj)
        {
            if (obj == null) return null;
            int hashCode = obj.GetHashCode();

            if (s_cacheUtilDictonary.ContainsKey(hashCode) == false)
            {
                try
                {
                    string cachedTypeName = $"{obj.GetType().Name}{sCS}";
                    Type type = GetTypeFromAllAssemblies(cachedTypeName);
                    object[] param = new object[] { obj.transform };
                    var instance = Activator.CreateInstance(type, param) as ICached;
                    Debug.Log($"instance: {instance}");
                    s_cacheUtilDictonary.Add(hashCode, instance);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Message: {ex.Message}");
                    return null;
                }
            }
            return s_cacheUtilDictonary[hashCode];
        }

        public static Type GetTypeFromAllAssemblies(string typeName)
        {
            // 현재 도메인에 로드된 모든 어셈블리를 가져옵니다.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // 각 어셈블리에서 타입을 시도하여 가져옵니다.
                Type type = assembly.GetType(typeName);

                if (type != null)
                {
                    return type;  // 타입을 찾으면 반환
                }
            }

            return null;  // 타입을 찾지 못하면 null 반환
        }
        public static string GetGTCS_String(string className, string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                return @$"
                public static {className}{CM.sCS} G_TC({className} obj) => RJ_TC.CM.G_TCI(obj) as {className}{CM.sCS};";
            }
            return @$"
            public static {className}{CM.sCS} G_TC({namespaceName}.{className} obj) => RJ_TC.CM.G_TCI(obj) as {className}{CM.sCS};";
        }
    }
}


