using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace RJ_TC
{
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
            // 배경색을 그리기 위한 rect 설정
            Rect backgroundRect = new Rect(selectionRect);
            backgroundRect.x = 0;
            backgroundRect.width = EditorGUIUtility.currentViewWidth;

            // ICaching 컴포넌트 확인
            ICaching cachingComponent = gameObject.GetComponent<ICaching>();
            if (cachingComponent != null)
            {
                // ICaching을 상속받은 객체는 cachingMainColor로 표시
                EditorGUI.DrawRect(backgroundRect, cachingMainColor);
            }
            else
            {
                // 부모 객체들을 검사하여 가장 가까운 ICaching 찾기
                Transform parent = gameObject.transform.parent;
                while (parent != null)
                {
                    if (parent.GetComponent<ICaching>() != null)
                    {
                        // ICaching을 찾았으면 현재 오브젝트부터 위로 올라가면서 SearchBlocker 체크
                        Transform current = gameObject.transform;
                        bool shouldColor = true;
                        Color colorToUse = cachingColor;

                        while (current != parent && current != null)
                        {
                            SearchBlocker blocker = current.GetComponent<SearchBlocker>();
                            if (blocker != null)
                            {
                                if (!blocker.EnableSearchChildren)
                                {
                                    colorToUse = searchBlockerColor;
                                    break;
                                }
                                else if (current == gameObject.transform)
                                {
                                    // 현재 오브젝트가 SearchBlocker이면서 EnableSearchChildren이 true인 경우
                                    colorToUse = searchBlockerColor;
                                    break;
                                }
                            }
                            current = current.parent;
                        }

                        if (shouldColor)
                        {
                            EditorGUI.DrawRect(backgroundRect, colorToUse);
                        }
                        break;
                    }
                    parent = parent.parent;
                }
            }
        }
    }
}
#endif
}
