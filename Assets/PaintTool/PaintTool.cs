
// ----- Credits
// ----- Riccardo Runci
// ----- PaintTool v1.3
/*
 * Notes 1.2:
 * + Opacity Feature
 * 
 * Notes 1.3:
 * + Custom GameObject parent
 * + Groups logic
 */

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class PaintTool : MonoBehaviour
{
    public List<PaintGroup> Groups;
    public List<GameObject> Cache;
    public bool Painting;
    public GameObject Parent;

    public float MinSize = 1f;
    public float MaxSize = 1.5f;

    public float MinRotation = 0;
    public float MaxRotation = 180;

    public float Opacity = 0.5f;
    public float BrushSize = 1f;
    public const int PaintSize = 10;

    public int GroupIndex;

    public LayerMask Layer = ~0;

    [ExecuteInEditMode]
    private void OnEnable()
    {
        Groups = new List<PaintGroup>();
        Cache = new List<GameObject>();
        Selection.selectionChanged += CheckDestroy;
    }

    private void CheckDestroy()
    {
        if (Selection.activeGameObject != null)
        {
            if (Selection.activeGameObject.name != this.name)
            {
                Painting = false;
            }            
        }
    }

    [ExecuteInEditMode]
    private void OnDestroy()
    {
        Selection.selectionChanged -= CheckDestroy;
    }

    private void OnDrawGizmos()
    {
        if (Painting)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hit.point, BrushSize);
            }
        }
    }

    public void RemoveElement(int iIndex)
    {
        Groups.RemoveAt(iIndex);
        if (GroupIndex >= Groups.Count)
            GroupIndex = Groups.Count - 1;
        if (GroupIndex < 0)
            GroupIndex = 0;
    }
}
#endif

[Serializable]
public class PaintGroup
{
    public List<GameObject> Prefabs;

    public PaintGroup()
    {
        Prefabs = new List<GameObject>();
    }
}