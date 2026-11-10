using System.Collections.Generic;

public class Selector : BTNode
{
    public Selector() : base() { }
    public Selector(List<BTNode> children) : base(children) { }

    public override NodeState Evaluate()
    {
        foreach (BTNode node in children)
        {
            switch (node.Evaluate())
            {
                case NodeState.FAILURE:
                    // Si este comportamiento no se puede cumplir, probamos el siguiente hijo
                    continue;
                case NodeState.SUCCESS:
                    state = NodeState.SUCCESS;
                    return state;
                case NodeState.RUNNING:
                    state = NodeState.RUNNING;
                    return state;
                default:
                    continue;
            }
        }

        // Si todos los hijos fallaron, el selector entero falla
        state = NodeState.FAILURE;
        return state;
    }
}