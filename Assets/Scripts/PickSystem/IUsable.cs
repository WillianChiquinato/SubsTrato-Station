using UnityEngine;
using UnityEngine.Events;

public interface IUsable
{
    void Use(GameObject actor);
    UnityEvent OnUse { get; }
}
