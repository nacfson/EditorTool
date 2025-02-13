using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
[InitializeOnLoad]
public class CacheHierarchyEditor
{
    private static readonly Color searchBlockerColor = new Color(1f, 0.7f, 0.7f, 0.3f); // ���� ������
    private static readonly Color cachingColor = new Color(0.7f, 1f, 0.7f, 0.3f); // ���� �ʷϻ�
    private static readonly Color cachingMainColor = new Color(0.7f, 0.7f, 1f, 0.3f); // ���� �Ķ���

    // �� ���� �����ڴ� ������ ���� �� �ڵ����� ����˴ϴ�.
    static CacheHierarchyEditor()
    {
        // Hierarchy â�� �������� �׸� ������ ȣ��Ǵ� �̺�Ʈ�� ����մϴ�.
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

            // ���� ���� ����
            Color oldColor = GUI.backgroundColor;

            if (hasSearchBlocker)
            {
                // SearchBlocker�� �ִ� ��� ������ ���̶���Ʈ
                GUI.backgroundColor = searchBlockerColor;
                EditorGUI.DrawRect(selectionRect, searchBlockerColor);
            }
            else if (isMainCaching)
            {
                // ICaching�� ���� ��ӹ��� ������Ʈ�� �Ķ��� ���̶���Ʈ
                GUI.backgroundColor = cachingMainColor;
                EditorGUI.DrawRect(selectionRect, cachingMainColor);
            }
            else if (hasCaching)
            {
                // ICaching�� �ִ� ��� �ʷϻ� ���̶���Ʈ
                GUI.backgroundColor = cachingColor;
                EditorGUI.DrawRect(selectionRect, cachingColor);
            }

            // ���� ���� ����
            GUI.backgroundColor = oldColor;
        }
    }
}
#endif
