using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchBlocker : MonoBehaviour, INotSearchable
{
    [field:SerializeField] public bool EnableSearchChildren { get; set; } = false;  
}
