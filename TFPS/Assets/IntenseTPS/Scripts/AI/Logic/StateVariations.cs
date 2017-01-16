using StateSystems;


//有点意思的设置 很常用 

/// <summary>
/// Inherits from abstract <see cref="State"/> and classifies states for clear execution of <see cref="AIStateSystem"/>'s
/// </summary>
public class IdleState : State
{
	public IdleState(AIStateSystem[] _stateSystems) : base(_stateSystems)
	{
		StateType = ET.StateType.Idle;
	}
}

/// <summary>
/// Inherits from abstract <see cref="State"/> and classifies states for clear execution of <see cref="AIStateSystem"/>'s
/// </summary>
public class AnimateState : State
{
	public AnimateState(AIStateSystem[] _stateSystems) : base(_stateSystems)
	{
		StateType = ET.StateType.Animate;
	}
}

/// <summary>
/// Inherits from abstract <see cref="State"/> and classifies states for clear execution of <see cref="AIStateSystem"/>'s
/// </summary>
public class MoveState : State
{
	public MoveState(AIStateSystem[] _stateSystems) : base(_stateSystems)
	{
		StateType = ET.StateType.Move;
	}
}