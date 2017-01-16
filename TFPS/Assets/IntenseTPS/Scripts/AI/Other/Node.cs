using Actions;

public partial class Node
{
	public string Key { get; private set; }

	public AIAction upperAction;

	public float cost = 0f;
	public Node parent;

	public Node()
	{
	}

	public Node(string key)
	{
		Key = key;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_cost">当前节点的累计消耗</param>
	/// <param name="_parent">上一级节点</param>
	/// <param name="_upperAction">当前节点</param>
	public Node(float _cost, Node _parent, AIAction _upperAction)
	{
		cost = _cost;
		parent = _parent;
		upperAction = _upperAction;
	}

	/// <summary>
	/// 这里需要说明Node的key值是上一个节点的消耗+当前行为的消耗
	/// </summary>
	/// <param name="_key">上一个节点的消耗+当前行为的消耗</param>
	/// <param name="_cost">当前节点的累计消耗</param>
	/// <param name="_parent">上一级节点</param>
	/// <param name="_upperAction">当前节点</param>
	public Node(string _key, float _cost, Node _parent, AIAction _upperAction)
	{
		Key = _key;
		cost = _cost;
		parent = _parent;
		upperAction = _upperAction;
	}

	public override string ToString()
	{
		return string.Format("Key = {0}",
			Key);
	}
}