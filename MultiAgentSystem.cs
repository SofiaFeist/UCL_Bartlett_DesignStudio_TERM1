using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MultiAgentSystem : MonoBehaviour
{
    // Agent Properties
    public GameObject Agent;
    float velocity = 1.0f;


    // Seed Properties
    public GameObject seed;
    static Vector3 SeedPosition = new Vector3(50, 0, 50);


    // Environment Properties
    float AreaWidth = 100f;
    static int NumAgents = 100;


    // Lists/Collections
    List<GameObject> listAgents = new List<GameObject>(NumAgents);
    Dictionary<int[,], GameObject> dictionaryAgents = new Dictionary<int[,], GameObject>();

    List<Vector3> movingAgents = new List<Vector3>(NumAgents);
    List<Vector3> staticAgents = new List<Vector3>(NumAgents + 1) { SeedPosition };


    // Render Effects
    public Material whiteGlowMaterial;
    public Material redGlowMaterial;
    public Material yellowGlowMaterial;
    public Material blueGlowMaterial;




    // Start is called before the first frame update
    void Start()
    {
        PlaceAgents();
        new Seed(seed, SeedPosition, blueGlowMaterial);
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //MoveAgentsRandomly();
        //MoveAgentsNoCollisions();
        DiffusionLimitedAggregation();

    }





    ////////////////////////   DIFFUSION LIMITED AGGREGATION ALGORITHM  ////////////////////////

    // DiffusionLimitedAggregation: moves the agents randomly in space until they find the seed or the agents attached to it
    public void DiffusionLimitedAggregation()
    {
        int i = -1;                   ///// i -> counter
        int tries = 10000;            ///// tries -> loop failsafe
        float minDistance = 1.0f;     ///// 1.0 = r*2 of a unit Agent

        while (i++ < listAgents.Count - 1 && tries-- > 0)
        {
            Vector3 newDirection = RandomVector(velocity);
            Vector3 agentPosition = listAgents[i].transform.position;
            Vector3 newPosition = agentPosition + newDirection;

            if (listAgents[i].tag == "Moving")  
            {
                if (Collides(minDistance * 1.2f, agentPosition, agentPosition, staticAgents))    // 1.2f is cheating -> CORRECT THAT
                {
                    float distanceToClosestAgent = Mathf.Abs(minDistance * 1.2f - Vector3.Distance(agentPosition, ClosestAgent(agentPosition, staticAgents))) / 2;
                    Vector3 moveCloserToStaticAgent = Vector3.MoveTowards(agentPosition, ClosestAgent(agentPosition, staticAgents), distanceToClosestAgent) - agentPosition;
                    listAgents[i].transform.Translate(moveCloserToStaticAgent);
                    listAgents[i].GetComponent<Renderer>().material = blueGlowMaterial;
                    listAgents[i].tag = "Static";
                    staticAgents.Add(agentPosition);
                }
                else
                if (!Collides(minDistance, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()) &&  
                    !OutsideBoundaries(newPosition, 0, AreaWidth))                   // (a => a.tag == "Moving" ? a.transform.position : agentPosition).ToList()) &&    This last agentPosition is WRONG ^ Is there a NULL Type for Vector3?
                {
                    listAgents[i].GetComponent<Renderer>().material = whiteGlowMaterial;
                    listAgents[i].transform.Translate(newDirection);
                }
                else
                {
                    listAgents[i].GetComponent<Renderer>().material = redGlowMaterial;
                }
            }
        }
    }


    // ClosestAgent: Calculates the closest agent between a given agent position and a list of agent positions
    Vector3 ClosestAgent(Vector3 position, List<Vector3> listPositions)
    {
        Vector3 closestAgent = new Vector3();
        List<float> distances = new List<float>();

        foreach (var other in listPositions)
        {
            if (position != other)
            {
                float distance = Vector3.Distance(position, other);
                distances.Add(distance);
                if (distances.All(d => distance <= d)) 
                    closestAgent = other;
            }
        }
        return closestAgent;
    }


    // AbsVector: Returns a vector whose elements are the absolute values of each of the specified vector's elements.
    Vector3 AbsVector(Vector3 vector)
    {
        vector.x = Mathf.Abs(vector.x);
        vector.y = Mathf.Abs(vector.y);
        vector.z = Mathf.Abs(vector.z);
        return vector;
    }





    ////////////////////////////   AGENT MOUVEMENT  ////////////////////////////

    // MoveAgentsRandomly: moves the agents randomly in space with no regards for their surroundings
    public void MoveAgentsRandomly()
    {
        foreach (GameObject agent in listAgents)
        {
            agent.transform.Translate(RandomVectorXZ(velocity));
        }
    }


    // MoveAgentsNoColisions: moves the agents randomly in space while trying to avoid intersecting other agents
    // TRY: Spatial data structures (e.g. bin-lattice spatial subdivision) to reduce algorithmic complexity
    public void MoveAgentsNoCollisions()
    {
        int i = -1;                   ///// i -> counter
        int tries = 10000;            ///// tries -> loop failsafe
        float minDistance = 1.0f;     ///// 1.0 = r*2 of a unit Agent

        while (i++ < listAgents.Count - 1 && tries-- > 0)
        {
            Vector3 newDirection = RandomVector(velocity);
            Vector3 agentPosition = listAgents[i].transform.position;
            Vector3 newPosition = agentPosition + newDirection;

            if (!Collides(minDistance, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()) &&
                !OutsideBoundaries(newPosition, 0, AreaWidth))
            {
                listAgents[i].GetComponent<Renderer>().material = whiteGlowMaterial;
                listAgents[i].transform.Translate(newDirection);
            }
            else
            {
                listAgents[i].GetComponent<Renderer>().material = redGlowMaterial;
            }
        }
    }


    // Collides: Checks if an agent is moving into another agent's space
    bool Collides(float minDistance, Vector3 position, Vector3 newPosition, List<Vector3> listPositions)
    {
        foreach (var other in listPositions)
        {
            if (position != other)
            {
                var distance = Vector3.Distance(newPosition, other);
                if (distance <= minDistance)
                    return true;
            }    
        }
        return false;
    }


    // OutsideBoundaries: Checks if a given vector is located outside of the given boundaries (square)
    bool OutsideBoundaries(Vector3 position, float min, float max)
    {
        if (position.x > max ||
            position.z > max ||
            position.x < min ||
            position.z < min)
            return true;
        else
            return false;
    }


    // Random Direction Vector in Polar coordinates: infinite possible directions (0º -> 360º float)
    Vector3 RandomVector(float velocity)
    {
        float pi = Mathf.PI;
        float angle = Random.Range(-pi, pi);
        Vector3 vector = new Vector3(velocity * Mathf.Cos(angle), 0, velocity * Mathf.Sin(angle));

        return vector;
    }


    // Random Direction Vector in Cartesian coordinates: 4 possible directions (+x, -x, +z, -z)
    Vector3 RandomVectorXZ(float velocity)
    {
        Vector3 vector;
        int random = Random.Range(0, 4);

        switch (random)
        {
            case 0:
                vector = new Vector3(velocity, 0, 0);
                break;
            case 1:
                vector = new Vector3(-velocity, 0, 0);
                break;
            case 2:
                vector = new Vector3(0, 0, velocity);
                break;
            default:
                vector = new Vector3(0, 0, -velocity);
                break;
        }
        return vector;
    }









    ////////////////////////////   INITIAL AGENT PLACEMENT  ////////////////////////////

    // placeAgents: places the agents according to the AgentStartPositions list
    public List<GameObject> PlaceAgents()
    {
        foreach (Vector3 position in AgentStartPositions())
        {
            GameObject placeAgent = Instantiate(Agent, position, Quaternion.identity);
            placeAgent.GetComponent<Renderer>().material = whiteGlowMaterial;
            placeAgent.tag = "Moving";
            listAgents.Add(placeAgent);
        }
        return listAgents;
    }


    // Agent Start Positions: list of randomly created vectors (with no overlap/intersections) representing the starting positions of the agents
    public List<Vector3> AgentStartPositions()
    {
        int tries = 10000;            ///// tries -> loop failsafe
        float minDistance = 1.0f;     ///// 1.0 = r*2 of a unit Agent

        while (movingAgents.Count < NumAgents && tries-- > 0)
        {
            Vector3 position = RandomPosition();
            if (!movingAgents.Any(p => Vector3.Distance(p, position) < minDistance))
                movingAgents.Add(position);

        }
        return movingAgents;
    }


    // Random Position (= Vector), with the X and Z coordinates placed randomly between the interval of 0 and AreaWidth
    Vector3 RandomPosition()
    {
        return new Vector3(Random.Range(0.0f, AreaWidth), 0, Random.Range(0.0f, AreaWidth));
    }







    ////////////////////////////   SPATIAL SUBDIVISION  ////////////////////////////

    int[,] AgentLocation()
    {
        int spatialDimension = (int) AreaWidth;
        int[,] location = new int[spatialDimension, spatialDimension];

        for (int i = 0; i < location.GetLength(0); i++)
        {
            for (int j = 0; j < location.GetLength(1); j++)
            {
                location[i, j] = location.GetLength(0) * i + j;
            }
        }
        return location;
    }
}








// Eventually: Organize everything into classes...
public class Agents
{
   

}

