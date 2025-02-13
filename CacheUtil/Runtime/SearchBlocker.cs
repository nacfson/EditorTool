using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RJ_TC
{
public class SearchBlocker : MonoBehaviour, INotSearchable
{
    [field:SerializeField] public bool EnableSearchChildren { get; set; } = false;  
}

}

