using System.Collections.Generic;
using UnityEngine;

// Attach to each test character/object in YOUR Unity project.
// Assign the actual collider used by your game's hit/collision system.
public sealed class DebugTarget : MonoBehaviour
{
    public string targetName = "Test Target";
    [Min(0f)] public float health = 100f;
    public Collider targetCollider;

    private static readonly HashSet<DebugTarget> ActiveTargets =
        new HashSet<DebugTarget>();

    public static IEnumerable<DebugTarget> GetActiveTargets()
    {
        return ActiveTargets;
    }

    private void Reset()
    {
        targetCollider = GetComponentInChildren<Collider>();
    }

    private void OnEnable()
    {
        ActiveTargets.Add(this);
    }

    private void OnDisable()
    {
        ActiveTargets.Remove(this);
    }

    private void OnDestroy()
    {
        ActiveTargets.Remove(this);
    }
}
