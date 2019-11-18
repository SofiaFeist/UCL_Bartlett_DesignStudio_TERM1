using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Experiment1 : MonoBehaviour
{
    public GameObject Agent;
    int AreaWidth = 100;
    int numAgents = 500;

    static public List<GameObject> listAgents = new List<GameObject>();

    //List<GameObject> agents = placeAgents();

    // Start is called before the first frame update
    void Start()
    {
        placeAgents();

    }

    // Update is called once per frame
    void Update()
    {
        //placeAgents();
        foreach (GameObject agent in listAgents)
        {
            agent.transform.Translate(randomVector());
        }

    }

    public List<GameObject> placeAgents()
    {
        // Position Vectors: dictinctPositions makes sure that there are no Agents placed in overlapping positions
        List<Vector3> randomPositions = agentStartPositions();
        IEnumerable<Vector3> distinctPositions = randomPositions.Distinct();

        // Agent Placement
        foreach (Vector3 position in distinctPositions)
        {
            GameObject placeAgent = Object.Instantiate(Agent, position, Quaternion.identity);
            listAgents.Add(placeAgent);
        }
        return listAgents;
    }

    public List<Vector3> agentStartPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < numAgents; i++)
        {
            Vector3 startPosition = new Vector3(randomCoordinate(), 0, randomCoordinate());
            positions.Add(startPosition);
        }
        return positions;
    }

    int randomCoordinate()
    {
        return Random.Range(0, AreaWidth);
    }

    Vector3 randomVector()
    {
        Vector3 vector = new Vector3();
        int range = Random.Range(0, 4);

        switch (range)
        {
            case 0:
                vector = new Vector3(1, 0, 0);
                break;
            case 1:
                vector = new Vector3(-1, 0, 0);
                break;
            case 2:
                vector = new Vector3(0, 0, 1);
                break;
            default:
                vector = new Vector3(0, 0, -1);
                break;
        }
        return vector;
    }
}


// Class try
public class Agents
{
    

    public Agents()
    { 
    
    }

    
}

