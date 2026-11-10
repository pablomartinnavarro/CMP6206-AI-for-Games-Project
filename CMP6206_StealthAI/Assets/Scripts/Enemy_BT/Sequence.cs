using System.Collections.Generic;

public class Sequence : BTNode
{
    public Sequence() : base() { }
    public Sequence(List<BTNode> children) : base(children) { }

    public override NodeState Evaluate()
    {
        foreach (BTNode node in children)
        {
            switch (node.Evaluate())
            {
                case NodeState.FAILURE:
                    // Si un paso falla, se interrumpe toda la secuencia
                    state = NodeState.FAILURE;
                    return state;
                case NodeState.SUCCESS:
                    // Si este paso tuvo éxito, pasamos a evaluar el siguiente en el próximo ciclo
                    continue;
                case NodeState.RUNNING:
                    // Si el paso está en proceso, la secuencia espera a que termine
                    state = NodeState.RUNNING;
                    return state;
                default:
                    state = NodeState.SUCCESS;
                    return state;
            }
        }

        // Todos los pasos se han completado con éxito
        state = NodeState.SUCCESS;
        return state;
    }
}