using UnityEngine;

[CreateAssetMenu(fileName = "RandomWalkSOParameters", menuName = "PCG/RandomWalkSO")]
public class RandomWalkSO : ScriptableObject
{
    public int iterations = 10, walkLength = 10;
}
