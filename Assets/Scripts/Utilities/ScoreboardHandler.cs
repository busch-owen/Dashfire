using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ScoreboardHandler : NetworkBehaviour
{
    public List<ScoreboardEntry> SortEntriesByScore(List<ScoreboardEntry> entries)
    {
        var sortedArray = entries.ToArray();
        Array.Sort(sortedArray, (x, y) => y.playerFrags.CompareTo(x.playerFrags));
        
        return sortedArray.ToList();
    }
}
