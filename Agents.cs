using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Agents
{
    CommonMethods CM = new CommonMethods();

    GameObject agent;
    Material material;
    int NumberOfAgents;

    public List<Vector3> randomStartPositions = new List<Vector3>();
    public List<GameObject> listAgents = new List<GameObject>();


    // Constructor
    public Agents(GameObject prefab, Material _material, int _NumberOfAgents)
    {
        this.agent = prefab;
        this.material = _material;
        this.NumberOfAgents = _NumberOfAgents;
    }




    ////////////////////////////   INITIAL AGENT PLACEMENT METHODS  ////////////////////////////

    //
    public List<GameObject> PlaceAgentsInRows(Vector3 position, float AreaMax)
    {
        int rows = (int) Mathf.Sqrt(NumberOfAgents) + 1;
        int columns = (int) Mathf.Sqrt(NumberOfAgents) + 1;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (listAgents.Count < NumberOfAgents)
                {
                    Vector3 _position = new Vector3(i + i, 0, j + j) + position;
                    GameObject placeAgent = Object.Instantiate(agent, _position, Quaternion.identity);
                    placeAgent.GetComponent<Renderer>().material = material;
                    placeAgent.tag = "Moving";
                    listAgents.Add(placeAgent);
                }
            }
        }
        return listAgents;
    }

    //PlaceAgentsRandomly: places the agents according to the AgentStartPositions random list
    public List<GameObject> PlaceAgentsRandomly(float AreaMin, float AreaMax, float minDistance)
    {
        foreach (Vector3 position in RandomStartPositions(AreaMin, AreaMax, minDistance))
        {
            GameObject placeAgent = Object.Instantiate(agent, position, Quaternion.identity);
            placeAgent.GetComponent<Renderer>().material = material;
            placeAgent.tag = "Moving";
            listAgents.Add(placeAgent);
        }
        return listAgents;
    }


    // RandomStartPositions: list of randomly created vectors (with no overlap/intersections) representing the starting positions of the agents
    public List<Vector3> RandomStartPositions(float AreaMin, float AreaMax, float minDistance)
    {
        int tries = 10000;            ///// tries -> loop failsafe

        while (randomStartPositions.Count < NumberOfAgents && tries-- > 0)
        {
            Vector3 position = RandomPosition(AreaMin, AreaMax);
            if (!randomStartPositions.Any(p => Vector3.Distance(p, position) < minDistance))
                randomStartPositions.Add(position);
        }
        return randomStartPositions;
    }


    // Random Position (= Vector), with the X and Z coordinates placed randomly between the interval of 0 and AreaWidth
    Vector3 RandomPosition(float AreaMin, float AreaMax)
    {
        return new Vector3(Random.Range(AreaMin, AreaMax), 0, Random.Range(AreaMin, AreaMax));
    }

}
