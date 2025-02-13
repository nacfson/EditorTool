using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CacheTest : CacheUtil
{
    private void Awake()
    {
        //string name = CM.G_TC(this).dd.name;
        Debug.Log($"Name: {name}");
    }
}
