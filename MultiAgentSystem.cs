using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class MultiAgentSystem : MonoBehaviour 
{
    CommonMethods CM = new CommonMethods();

    // Agent Properties
    public GameObject agent;
    Agents agents;
    float velocity = 1.0f;
    float acceleration = 0.0f;
    float minDistance = 1.0f;       // Minimum Distance between Agents: 1.0 = r*2 of a unit Agent

    // Seed Properties
    public GameObject seed;
    static Vector3 SeedPosition = new Vector3(10, 0, 20);
    Seed SeedInstance;

    // Leader Properties
    public GameObject leader;
    Vector3 leaderStartPosition = new Vector3(50, 0, 50);
    Leader LeaderInstance;


    // Environment Properties
    SpatialSubdivision Subdivision = new SpatialSubdivision(0, AreaWidth, division);
    static float AreaWidth = 100f;
    static int division = 10;             // Grid Subdivision: in how many cells is the grid divided into?
    float AreaInfluence = 5f;             // Area of Influence of each agent: how far do they "see"
    static int NumAgents = 100;



    // Lists/Collections
    List<GameObject> listAgents = new List<GameObject>(NumAgents);
    Dictionary<Vector2Int, List<GameObject>> dictionaryAgents = new Dictionary<Vector2Int, List<GameObject>>(NumAgents);

    static List<Vector3> agentStartPositions = new List<Vector3>(NumAgents);
    //List<Vector3> agentPositions = agentStartPositions.ConvertAll(p => new Vector3(p.x, p.y, p.z));   // I don't think I need this one (delete?)
    List<Vector3> staticAgents = new List<Vector3>(NumAgents + 1) { SeedPosition };



    // Render Effects
    public Material whiteGlowMaterial;
    public Material redGlowMaterial;
    public Material yellowGlowMaterial;
    public Material blueGlowMaterial;



    // Animation Properties
    float timer = 0;


    


    // Start is called before the first frame update
    void Start()
    {
        // Instantiate Agents
        agents = new Agents(agent, whiteGlowMaterial, NumAgents);
        agents.PlaceAgents(0, AreaWidth, minDistance);

        agentStartPositions = agents.agentStartPositions;
        listAgents = agents.listAgents;
        

        // Instantiate the Seed
        //SeedInstance = new Seed(seed, SeedPosition, blueGlowMaterial);

        // Instantiate the Leader
        //LeaderInstance = new Leader(leader, leaderStartPosition, blueGlowMaterial);
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //RandomWalk();
        //RandomWalkNoCollisions();


        // Needs a Seed:
        //DiffusionLimitedAggregation();
        //Queue();
        //Communication();


        // Needs a Leader
        //LeaderInstance.RandomWalk(listAgents.Select(a => a.transform.position).ToList(), velocity, minDistance, 0, AreaWidth);
        //LeaderInstance.EvadeClosestAgent(listAgents.Select(a => a.transform.position).ToList(), velocity, minDistance, 0, AreaWidth);
        //LeaderInstance.RandomEvade(listAgents.Select(a => a.transform.position).ToList(), velocity, minDistance, 0, AreaWidth);
        //FollowTheLeader();
        //AvoidTheLeader();


        // Animation Sequence (with timer)
        //timer += Time.deltaTime;
        //if (timer < 5)
        //{
        //    RandomWalkNoCollisions();
        //}
        //Invoke("Reset", 5);   // Time in seconds
    }







    ////////////////////////   COMMUNICATION  ////////////////////////

    //Communication: Agents simulate communication using colors
    public void Communication()
    {
        float areaOfInfluence = 15f;

        for (int i = 0; i < listAgents.Count; i++)
        {
            if (listAgents[i].tag == "Moving")
            {
                Vector3 newDirection = CM.RandomVector(velocity);
                Vector3 agentPosition = listAgents[i].transform.position;
                Vector3 newPosition = agentPosition + newDirection;

                if (CM.Collides(areaOfInfluence, agentPosition, agentPosition, staticAgents))
                {
                    listAgents[i].GetComponent<Renderer>().material = blueGlowMaterial;
                    listAgents[i].tag = "Transmitting";
                    staticAgents.Add(agentPosition);
                }
                else
                if (!CM.Collides(minDistance, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()) &&
                    !CM.OutsideBoundaries(newPosition, 0, AreaWidth))
                {
                    listAgents[i].GetComponent<Renderer>().material = whiteGlowMaterial;
                    listAgents[i].transform.Translate(newDirection);
                }

            }
            else if (listAgents[i].tag == "Transmitting")
            {
                StartCoroutine(Blink(listAgents[i]));
            }
        }
    }


    // Blink
    IEnumerator Blink(GameObject agent)
    {
        if (agent.GetComponent<Renderer>().material = blueGlowMaterial)
            agent.GetComponent<Renderer>().material = redGlowMaterial;
        else
            agent.GetComponent<Renderer>().material = blueGlowMaterial;
        yield return new WaitForSeconds(5f);
    }






    ////////////////////////   FOLLOW THE LEADER  ////////////////////////

    //FollowTheLeader: Agents move torwards the Leader
    public void FollowTheLeader()
    {                      
        for (int i = 0; i < listAgents.Count; i++)
        {
            Vector3 agentPosition = listAgents[i].transform.position;
            Vector3 pursueLeader = (LeaderInstance.Position - agentPosition).normalized * velocity * 0.1f;
            Vector3 newPosition = agentPosition + pursueLeader;

            if (!CM.OutsideBoundaries(newPosition, 0, AreaWidth) &&
                !CM.Collides(minDistance * 2, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()))
            {
                listAgents[i].GetComponent<Renderer>().material = whiteGlowMaterial;
                listAgents[i].transform.Translate(pursueLeader);
                //agentPositions[i] = newPosition;
            }
        }
    }


    //AvoidTheLeader: Agents run away from the Leader
    public void AvoidTheLeader()
    {
        float areaInfluence = 15.0f;

        for (int i = 0; i < listAgents.Count; i++)
        {
            Vector3 agentPosition = listAgents[i].transform.position;
            Vector3 randomDirection = CM.RandomVector(velocity);
            Vector3 newRandomPosition = agentPosition + randomDirection;
            Vector3 avoidLeader = (agentPosition - LeaderInstance.Position).normalized * velocity;
            Vector3 awayFromLeader = agentPosition + avoidLeader;

            if (CM.Collides(areaInfluence, agentPosition, newRandomPosition, new List<Vector3>() { LeaderInstance.Position }))
            {
                listAgents[i].GetComponent<Renderer>().material = redGlowMaterial;
                listAgents[i].transform.Translate(avoidLeader);
                //agentPositions[i] = awayFromLeader;
            }
            if (!CM.OutsideBoundaries(newRandomPosition, 0, AreaWidth) &&
                !CM.Collides(minDistance, agentPosition, newRandomPosition, listAgents.Select(a => a.transform.position).ToList()))
            {
                listAgents[i].GetComponent<Renderer>().material = whiteGlowMaterial;
                listAgents[i].transform.Translate(randomDirection);
                //agentPositions[i] = newRandomPosition;
            }
            else
            {
                Vector3 back = CM.BackToBoundaries(agentPosition, velocity, 0, AreaWidth) + randomDirection;
                listAgents[i].transform.Translate(back);
                //agentPositions[i] = agentPosition + back;
            }
        }
    }






    ////////////////////////   DIFFUSION LIMITED AGGREGATION ALGORITHM  ////////////////////////

    // DiffusionLimitedAggregation: moves the agents randomly in space until they find the seed or the agents attached to it
    public void DiffusionLimitedAggregation()
    {
        for (int i = 0; i < listAgents.Count; i++)
        {
            Vector3 newDirection = CM.RandomVector(velocity);
            Vector3 agentPosition = listAgents[i].transform.position;
            Vector3 newPosition = agentPosition + newDirection;

            if (listAgents[i].tag == "Moving")  
            {
                if (CM.Collides(minDistance * 1.2f, agentPosition, agentPosition, staticAgents))    // 1.2f is cheating -> CORRECT THAT
                {
                    // Corrective Vector: to be corrected !!!!!!!!!!!!!!!!!!!!!!
                    float distanceToClosestAgent = Mathf.Abs(minDistance * 1.2f - Vector3.Distance(agentPosition, CM.ClosestAgent(agentPosition, staticAgents))) / 2;
                    Vector3 moveCloserToStaticAgent = Vector3.MoveTowards(agentPosition, CM.ClosestAgent(agentPosition, staticAgents), distanceToClosestAgent) - agentPosition;
                    listAgents[i].transform.Translate(moveCloserToStaticAgent);

                    listAgents[i].GetComponent<Renderer>().material = blueGlowMaterial;
                    listAgents[i].tag = "Static";
                    staticAgents.Add(agentPosition);
                }
                else
                if (!CM.Collides(minDistance, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()) &&  
                    !CM.OutsideBoundaries(newPosition, 0, AreaWidth))                   // (a => a.tag == "Moving" ? a.transform.position : agentPosition).ToList()) &&    This last agentPosition is WRONG ^ Is there a NULL Type for Vector3?
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



    public void Queue()
    {
        for (int i = 0; i < listAgents.Count; i++) 
        {
            Vector3 newDirection = CM.RandomVector(velocity);
            Vector3 agentPosition = listAgents[i].transform.position;
            Vector3 newPosition = agentPosition + newDirection;

            if (listAgents[i].tag == "Moving")
            {
                if (CM.Collides(minDistance * 1.2f, agentPosition, agentPosition, staticAgents))    // 1.2f is cheating -> CORRECT THAT
                {
                    // Corrective Vector: to be corrected !!!!!!!!!!!!!!!!!!!!!!
                    float distanceToClosestAgent = Mathf.Abs(minDistance * 1.2f - Vector3.Distance(agentPosition, CM.ClosestAgent(agentPosition, staticAgents))) / 2;
                    Vector3 moveCloserToStaticAgent = Vector3.MoveTowards(agentPosition, CM.ClosestAgent(agentPosition, staticAgents), distanceToClosestAgent) - agentPosition;
                    listAgents[i].transform.Translate(moveCloserToStaticAgent);

                    listAgents[i].GetComponent<Renderer>().material = blueGlowMaterial;
                    listAgents[i].tag = "Static";
                    staticAgents[0] = agentPosition;
                }
                else
                if (!CM.Collides(minDistance, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()) &&
                    !CM.OutsideBoundaries(newPosition, 0, AreaWidth))                   // (a => a.tag == "Moving" ? a.transform.position : agentPosition).ToList()) &&    This last agentPosition is WRONG ^ Is there a NULL Type for Vector3?
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







    ////////////////////////   RESET  ////////////////////////

    // Reset: moves the agents back to their Start Position 
    public void Reset()
    {   
        for (int i = 0; i < listAgents.Count; i++)
        {
            if (listAgents[i].tag == "Moving")
            {
                Vector3 agentPosition = listAgents[i].transform.position;
                Vector3 backToStart = (agentStartPositions[i] - agentPosition).normalized * velocity * 0.5f;
                //Vector3 backToStart = Vector3.MoveTowards(agentPosition, agentStartPositions[i], velocity);
                listAgents[i].transform.Translate(backToStart);

                if (agentPosition == agentStartPositions[i] || Vector3.Distance(agentPosition, agentStartPositions[i]) <= 0.5)  // 0.5 = Tolerance
                {
                    listAgents[i].GetComponent<Renderer>().material = blueGlowMaterial;
                    listAgents[i].tag = "Static";       // Deceleration instead of static?
                }
            }
        }
    }






    ////////////////////////////   RANDOM WALK  ////////////////////////////

    // MoveAgentsRandomly: moves the agents randomly in space with no regards for their surroundings
    public void RandomWalk()
    {
        foreach (GameObject agent in listAgents)
        {
            Vector3 newDirection = CM.RandomVectorXZ(velocity);
            Vector3 newPosition = agent.transform.position + newDirection;

            if (!CM.OutsideBoundaries(newPosition, 0, AreaWidth))
            {
                agent.transform.Translate(newDirection);
            }
        }
    }


    // MoveAgentsNoColisions: moves the agents randomly in space while trying to avoid intersecting other agents
    public void RandomWalkNoCollisions()
    {
        for (int i = 0; i < listAgents.Count; i++)
        {
            Vector3 newDirection = CM.RandomVector(velocity);
            Vector3 agentPosition = listAgents[i].transform.position;
            Vector3 newPosition = agentPosition + newDirection;

            if (!CM.Collides(minDistance, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()) &&
                !CM.OutsideBoundaries(newPosition, 0, AreaWidth))
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

    // With Dictionary
    //public void RandomWalkNoCollisions()
    //{
    //    foreach (var cell in dictionaryAgents.Keys.ToList())
    //    {
    //        foreach (GameObject agent in dictionaryAgents[cell].ToList())
    //        {
    //            Vector3 newDirection = CM.RandomVector(velocity);
    //            Vector3 agentPosition = agent.transform.position;
    //            Vector3 newPosition = agentPosition + newDirection;

    //            Vector2Int newCell = Subdivision.GridLocation(newPosition);
    //            List<Vector2Int> neighbouringCells = Subdivision.ClosestCells(newPosition, newCell, AreaInfluence);

    //            if (!Subdivision.CollidesD(minDistance, agentPosition, newPosition, neighbouringCells) &&
    //                !CM.OutsideBoundaries(newPosition, 0, AreaWidth))
    //            {
    //                agent.GetComponent<Renderer>().material = whiteGlowMaterial;
    //                agent.transform.Translate(newDirection);

    //                if (cell != newCell)
    //                    Subdivision.UpdateDictionary(cell, newCell, agent);
    //            }
    //            else
    //            {
    //                agent.GetComponent<Renderer>().material = redGlowMaterial;
    //            }
    //        }
    //    }
    //}
}

