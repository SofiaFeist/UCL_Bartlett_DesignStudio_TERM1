using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Agents
{
    GameObject agent;
    Material material;
    int NumberOfAgents;

    public List<Vector3> agentStartPositions = new List<Vector3>();
    public List<GameObject> listAgents = new List<GameObject>();


    // Constructor
    public Agents(GameObject prefab, Material _material, int _NumberOfAgents)
    {
        this.agent = prefab;
        this.material = _material;
        this.NumberOfAgents = _NumberOfAgents;
    }




    ////////////////////////////   INITIAL AGENT PLACEMENT METHODS  ////////////////////////////

    //placeAgents: places the agents according to the AgentStartPositions list
    public List<GameObject> PlaceAgents(float AreaMin, float AreaMax, float minDistance)
    {
        foreach (Vector3 position in AgentStartPositions(AreaMin, AreaMax, minDistance))
        {
            GameObject placeAgent = Object.Instantiate(agent, position, Quaternion.identity);
            placeAgent.GetComponent<Renderer>().material = material;
            placeAgent.tag = "Moving";
            listAgents.Add(placeAgent);
        }
        return listAgents;
    }


    // Agent Start Positions: list of randomly created vectors (with no overlap/intersections) representing the starting positions of the agents
    public List<Vector3> AgentStartPositions(float AreaMin, float AreaMax, float minDistance)
    {
        int tries = 10000;            ///// tries -> loop failsafe

        while (agentStartPositions.Count < NumberOfAgents && tries-- > 0)
        {
            Vector3 position = RandomPosition(AreaMin, AreaMax);
            if (!agentStartPositions.Any(p => Vector3.Distance(p, position) < minDistance))
                agentStartPositions.Add(position);
        }
        return agentStartPositions;
    }


    // Random Position (= Vector), with the X and Z coordinates placed randomly between the interval of 0 and AreaWidth
    Vector3 RandomPosition(float AreaMin, float AreaMax)
    {
        return new Vector3(Random.Range(AreaMin, AreaMax), 0, Random.Range(AreaMin, AreaMax));
    }

}
