using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ICaching 
{
    public bool Caching();
    public void MakingScript();
    public bool IsCached {get; } 
    public bool IsCreatedScript {get;}
}
