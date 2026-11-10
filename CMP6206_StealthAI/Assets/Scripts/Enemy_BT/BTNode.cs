using System.Collections.Generic;

// Los tres estados fundamentales de retorno de cualquier nodo en un Behaviour Tree
public enum NodeState
{
    RUNNING,
    SUCCESS,
    FAILURE
}

public abstract class BTNode
{
    // Estado actual del nodo tras su última evaluación
    protected NodeState state;

    // Referencia al nodo padre en la jerarquía del árbol
    public BTNode parent;

    // Lista de nodos hijos (utilizada por Composites como Selector y Sequence)
    protected List<BTNode> children = new List<BTNode>();

    // Diccionario compartido en memoria ("Blackboard") para intercambiar datos entre nodos
    private Dictionary<string, object> dataContext = new Dictionary<string, object>();

    public BTNode()
    {
        parent = null;
    }

    public BTNode(List<BTNode> children)
    {
        foreach (BTNode child in children)
        {
            Attach(child);
        }
    }

    // Vincula un nodo hijo asignándole este nodo como su padre
    private void Attach(BTNode child)
    {
        child.parent = this;
        children.Add(child);
    }

    // Método virtual puro: cada nodo hijo implementará aquí su propia lógica matemática
    public abstract NodeState Evaluate();

    // Almacena un dato en la memoria del árbol (ej. la última posición donde se vio al jugador)
    public void SetData(string key, object value)
    {
        dataContext[key] = value;
    }

    // Recupera un dato buscando hacia arriba en la jerarquía hasta la raíz
    public object GetData(string key)
    {
        if (dataContext.TryGetValue(key, out object value))
        {
            return value;
        }

        BTNode node = parent;
        while (node != null)
        {
            object val = node.GetData(key);
            if (val != null) return val;
            node = node.parent;
        }

        return null;
    }

    // Elimina un dato de la memoria cuando ya no es válido
    public bool ClearData(string key)
    {
        if (dataContext.ContainsKey(key))
        {
            dataContext.Remove(key);
            return true;
        }

        BTNode node = parent;
        while (node != null)
        {
            bool cleared = node.ClearData(key);
            if (cleared) return true;
            node = node.parent;
        }

        return false;
    }
}