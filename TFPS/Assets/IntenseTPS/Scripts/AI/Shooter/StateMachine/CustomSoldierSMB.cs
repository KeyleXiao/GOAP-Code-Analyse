using UnityEngine;

/// <summary>
/// Used to reference objects of <see cref="MonoBehaviour"/>
/// </summary>
public class CustomSoldierSMB : StateMachineBehaviour
{
    [HideInInspector]
    public ShooterBehaviour shooter;   // For referencing input from a MonoBehaviour

    public virtual void Init(Animator anim)
    { }
}