using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Rhythm Config", menuName = "Rhythm")]
[System.Serializable]
public class RhythmConfig : ScriptableObject
{
    public List<float> beats;
}
