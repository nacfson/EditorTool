using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace RJ_TC
{
    public static class CM
    {
        public static readonly string sCS = "_C";
        public static readonly string sCS_PATH = $"Packages/com.rj.unitytools/Script_Cached/";
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
                    Type type = Type.GetType(cachedTypeName);
                    object[] param = new object[] { obj.transform };
                    var instance = Activator.CreateInstance(type, param) as ICached;
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
    }
}


