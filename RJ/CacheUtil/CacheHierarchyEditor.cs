using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
[InitializeOnLoad]
public class CacheHierarchyEditor
{
    private static readonly Color searchBlockerColor = new Color(1f, 0.7f, 0.7f, 0.3f); // 연한 빨간색
    private static readonly Color cachingColor = new Color(0.7f, 1f, 0.7f, 0.3f); // 연한 초록색
    private static readonly Color cachingMainColor = new Color(0.7f, 0.7f, 1f, 0.3f); // 연한 파란색

    // 이 정적 생성자는 에디터 시작 시 자동으로 실행됩니다.
    static CacheHierarchyEditor()
    {
        // Hierarchy 창에 아이템을 그릴 때마다 호출되는 이벤트를 등록합니다.
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject != null)
        {
            Transform current = gameObject.transform;
            bool hasSearchBlocker = false;
            bool hasCaching = false;
            bool isMainCaching = gameObject.GetComponent<ICaching>() != null;

            while (current != null)
            {
                if (current.GetComponent<SearchBlocker>() != null)
                {
                    hasSearchBlocker = true;
                }
                if (current.GetComponent<ICaching>() != null && !isMainCaching)
                {
                    hasCaching = true;
                }
                current = current.parent;
            }

            // 기존 배경색 저장
            Color oldColor = GUI.backgroundColor;

            if (hasSearchBlocker)
            {
                // SearchBlocker가 있는 경우 빨간색 하이라이트
                GUI.backgroundColor = searchBlockerColor;
                EditorGUI.DrawRect(selectionRect, searchBlockerColor);
            }
            else if (isMainCaching)
            {
                // ICaching을 직접 상속받은 오브젝트는 파란색 하이라이트
                GUI.backgroundColor = cachingMainColor;
                EditorGUI.DrawRect(selectionRect, cachingMainColor);
            }
            else if (hasCaching)
            {
                // ICaching이 있는 경우 초록색 하이라이트
                GUI.backgroundColor = cachingColor;
                EditorGUI.DrawRect(selectionRect, cachingColor);
            }

            // 기존 배경색 복구
            GUI.backgroundColor = oldColor;
        }
    }
}
#endif
