using UnityEngine;

// Base class
public abstract class UtilityEffectMB : MonoBehaviour
{
    public string effectName;
    [TextArea] public string description;

    public abstract void Execute();
}