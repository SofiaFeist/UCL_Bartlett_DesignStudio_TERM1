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
    float velocity = 3.0f;
    float acceleration = 0.0f;
    float minDistance = 3.0f;       // Minimum Distance between Agents: 1.0 = r*2 of a unit Agent

    // Seed Properties
    public GameObject seed;
    static Vector3 SeedPosition = new Vector3(50, 0, 50);
    Seed SeedInstance;

    // Leader Properties
    public GameObject leader;
    Vector3 leaderStartPosition = new Vector3(50, 0, 50);
    Leader LeaderInstance;


    // Environment Properties
    SpatialSubdivision Subdivision = new SpatialSubdivision(0, AreaWidth, division);
    static float AreaWidth = 100f;
    static int division = 10;             // Grid Subdivision: in how many cells is the grid divided into?
    float AreaInfluence = 7f;            // Area of Influence of each agent: how far they can "see"
    static int NumAgents = 50;



    // Lists/Collections
    List<GameObject> listAgents = new List<GameObject>(NumAgents);
    Dictionary<Vector2Int, List<GameObject>> dictionaryAgents = new Dictionary<Vector2Int, List<GameObject>>(NumAgents);

    static List<Vector3> randomStartPositions = new List<Vector3>(NumAgents);
    //List<Vector3> agentPositions = agentStartPositions.ConvertAll(p => new Vector3(p.x, p.y, p.z));   // I don't think I need this one (delete?)
    List<Vector3> staticAgents = new List<Vector3>(NumAgents + 1) { SeedPosition };
    List<Vector3> steeringForces;
    List<Vector3> imagePixels;


    // Render Effects
    public Material whiteGlowMaterial;
    public Material redGlowMaterial;
    public Material yellowGlowMaterial;
    public Material blueGlowMaterial;

    public Texture2D texture;



    // Animation Properties
    float timer = 0;




    // Start is called before the first frame update
    void Start()
    {
        imagePixels = PointsFromImage();
        NumAgents = imagePixels.Count;

        // INSTANTIATE AGENTS
        agents = new Agents(agent, whiteGlowMaterial, NumAgents);
        //agents.PlaceAgentsRandomly(0, AreaWidth, minDistance);
        agents.PlaceAgentsInRows(new Vector3(AreaWidth, 0, 0), AreaWidth);
        //agents.PlaceAgentsInRows(new Vector3(15, 0, 15), AreaWidth);
        //agents.PlaceAgentsDictionary(0, AreaWidth, minDistance);


        // LIST/COLLECTIONS INSTANTIATION
        randomStartPositions = agents.randomStartPositions;
        listAgents = agents.listAgents;
        steeringForces = CM.StartingSteeringDirections(listAgents.Count, velocity);
        dictionaryAgents = agents.dictionaryAgents;



        // INSTANTIATE SEED
        //SeedInstance = new Seed(seed, SeedPosition, blueGlowMaterial);



        // INSTANTIATE LEADER
        //LeaderInstance = new Leader(leader, leaderStartPosition, blueGlowMaterial, velocity);






        // SQUARE MARCH (-> Use with PlaceAgentsInRows)
        //StartCoroutine("SquareMarch");


        // SELF ASSEMBLY ALGORITHM (^ uncomment the imagePixels instantiation at the start of the Start Method)
        StartCoroutine("SelfAssembly");


        // MESSAGE PROPAGATION ALGORITHM
        //GameObject emitter = Instantiate(agent, SeedPosition, Quaternion.identity);
        //emitter.GetComponent<Renderer>().material = blueGlowMaterial;
        //StartCoroutine(Blink(emitter));
        //StartCoroutine("MessagePropagation");
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        // AGENT WALKING METHODS
        //RandomWalk();
        //RandomWalkNoCollisions();
        //RandomWalkNoCollisionsDictionary();
        //Wander();
        //LinearWalk(new Vector3(velocity, 0, 0));




        // AGENT/SEED BEHAVIOURS
        //DiffusionLimitedAggregation();
        //Queue();
        //Communication();



        // LEADER WALKING METHODS
        //LeaderInstance.RandomWalk(listAgents.Select(a => a.transform.position).ToList(), velocity, minDistance, 0, AreaWidth);
        //LeaderInstance.EvadeClosestAgent(listAgents.Select(a => a.transform.position).ToList(), velocity, minDistance, 0, AreaWidth);
        //LeaderInstance.RandomEvade(listAgents.Select(a => a.transform.position).ToList(), velocity, minDistance, 0, AreaWidth);
        //LeaderInstance.Wander(listAgents.Select(a => a.transform.position).ToList(), velocity, minDistance, 0, AreaWidth);

        // AGENT/LEADER BEHAVIOURS
        //FollowTheLeader();
        //AvoidTheLeader();







        // ANIMATION SEQUENCE (with Timer)
        //timer += Time.deltaTime;
        //if (timer < 5)
        //{
        //    RandomWalkNoCollisions();
        //}
        //Invoke("Reset", 5);   // Time in seconds
    }










    ////////////////////////   (CENTRALIZED) SELF-ASSEMBLY ALGORITHM  ////////////////////////   =>  Future Work: Distributed Self-Assembly; no agent has the full picture of the whole 
                                                                                                 //        and have to rely on local sensing/interactions to figure out their positions
    // SelfAssembly: Agents move one by one to form an image  
    IEnumerator SelfAssembly()
    {
        for (int i = 0; i < listAgents.Count; i++)
        {
            while (true)
            {
                Vector3 agentPosition = listAgents[i].transform.position;
                Vector3 seekDirection = (imagePixels[i] - agentPosition).normalized * velocity;

                if (Vector3.Distance(agentPosition, imagePixels[i]) <= minDistance)
                {
                    listAgents[i].transform.Translate(imagePixels[i] - agentPosition);
                    listAgents[i].GetComponent<Renderer>().material = blueGlowMaterial;
                    break;
                }
                else
                {
                    listAgents[i].transform.Translate(seekDirection);
                }

                yield return null;
            }
        }
    }


    // PointsFromImage: Calculates the target points for the Self-Assembly Algorithm from an image input
    public List<Vector3> PointsFromImage()
    {
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < texture.width; i += 2)
        {
            for (int j = 0; j < texture.height; j += 2)
            {
                Color color = texture.GetPixel(i, j);

                if (color.r < 1)
                    positions.Add(new Vector3(i, 0, j));

            }
        }
        return positions;
    }










    ////////////////////////   MESSAGE PROPAGATION  ////////////////////////

    //MessagePropagation: Agents stop when they enter the area of Influence of a transmitting agent and become transmitters themselves.
    //                    Communication simulated using blinking colors
    IEnumerator MessagePropagation()
    {
        bool toggle = true;
        while (true)
        {
            toggle = !toggle;
            for (int i = 0; i < listAgents.Count; i++)
            {
                if (listAgents[i].tag == "Moving")
                {
                    Vector3 agentPosition = listAgents[i].transform.position;
                    Vector3 newDirection = CM.PerlinVector(steeringForces[i], agentPosition, velocity);
                    Vector3 newPosition = agentPosition + newDirection;

                    if (CM.Collides(AreaInfluence, agentPosition, agentPosition, staticAgents))
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
                    else if (CM.OutsideBoundaries(newPosition, 0, AreaWidth))
                    {
                        Vector3 correctiveVector = newDirection * 0.2f + CM.BackToBoundaries(agentPosition, newDirection, velocity, 0, AreaWidth) * 0.8f;  // CORRECT BUT USE ACCELERATION FORCES!!!!
                        listAgents[i].transform.Translate(correctiveVector);
                        steeringForces[i] = correctiveVector;
                    }
                    else
                    {
                        Vector3 correctiveVector = CM.PerpendicularVector(newDirection, 1).normalized * velocity;
                        listAgents[i].transform.Translate(correctiveVector);
                        listAgents[i].GetComponent<Renderer>().material = redGlowMaterial;
                    }
                }
                if (listAgents[i].tag == "Transmitting")
                {
                    var renderer = listAgents[i].GetComponent<Renderer>();
                    renderer.material = toggle ? redGlowMaterial : blueGlowMaterial;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }


    // Blink: Makes a Game Object intermittently change materials
    IEnumerator Blink(GameObject obj)
    {
        bool toggle = true;
        var renderer = obj.GetComponent<Renderer>();

        while (true)
        {
            toggle = !toggle;
            renderer.material = toggle ? blueGlowMaterial : redGlowMaterial;
            yield return new WaitForSeconds(0.5f);
        }
    }








    ////////////////////////   FOLLOW THE LEADER  ////////////////////////

    //FollowTheLeader: Agents move torwards the Leader
    public void FollowTheLeader()
    {
        for (int i = 0; i < listAgents.Count; i++)
        {
            Vector3 agentPosition = listAgents[i].transform.position;
            Vector3 pursueLeader = (LeaderInstance.Position - agentPosition).normalized * velocity * 0.8f;
            Vector3 newPosition = agentPosition + pursueLeader;

            if (!CM.OutsideBoundaries(newPosition, 0, AreaWidth) &&
                !CM.Collides(minDistance * 2, agentPosition, newPosition, new List<Vector3> { LeaderInstance.Position }) &&
                !CM.Collides(minDistance * 2, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()))
            {
                listAgents[i].GetComponent<Renderer>().material = whiteGlowMaterial;
                listAgents[i].transform.Translate(pursueLeader);
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
            }
            if (!CM.OutsideBoundaries(newRandomPosition, 0, AreaWidth) &&
                !CM.Collides(minDistance, agentPosition, newRandomPosition, listAgents.Select(a => a.transform.position).ToList()))
            {
                listAgents[i].GetComponent<Renderer>().material = whiteGlowMaterial;
                listAgents[i].transform.Translate(randomDirection);
            }
            else
            {
                Vector3 correctiveVector = randomDirection * 0.2f + CM.BackToBoundaries(agentPosition, randomDirection, velocity, 0, AreaWidth) * 0.8f;
                listAgents[i].transform.Translate(correctiveVector);
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
                if (CM.Collides(minDistance + velocity, agentPosition, agentPosition, staticAgents))  
                {
                    Vector3 moveCloserToStaticAgent = (CM.ClosestAgent(agentPosition, staticAgents) - agentPosition).normalized * velocity;
                    listAgents[i].transform.Translate(moveCloserToStaticAgent);

                    listAgents[i].GetComponent<Renderer>().material = blueGlowMaterial;
                    listAgents[i].tag = "Static";
                    staticAgents.Add(agentPosition);
                }
                else
                if (!CM.Collides(minDistance, agentPosition, newPosition, listAgents.Where(a => a.tag == "Moving").Select(a => a.transform.position).ToList()) &&
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
    }



    public void Queue()              // CHECK: SOMETHING IS NOT OKAY WITH THIS ONE
    {
        for (int i = 0; i < listAgents.Count; i++)
        {
            Vector3 newDirection = CM.RandomVector(velocity);
            Vector3 agentPosition = listAgents[i].transform.position;
            Vector3 newPosition = agentPosition + newDirection;

            if (listAgents[i].tag == "Moving")
            {
                if (CM.Collides(minDistance + velocity, agentPosition, agentPosition, staticAgents))  
                {
                    Vector3 moveCloserToStaticAgent = (staticAgents[0] - agentPosition).normalized * velocity;
                    listAgents[i].transform.Translate(moveCloserToStaticAgent);

                    listAgents[i].GetComponent<Renderer>().material = blueGlowMaterial;
                    listAgents[i].tag = "Static";
                    staticAgents[0] = agentPosition;
                }
                else
                if (!CM.Collides(minDistance, agentPosition, newPosition, listAgents.Where(a => a.tag == "Moving").Select(a => a.transform.position).ToList()) &&
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
                Vector3 backToStart = (randomStartPositions[i] - agentPosition).normalized * velocity * 0.5f;
                listAgents[i].transform.Translate(backToStart);

                if (Vector3.Distance(agentPosition, randomStartPositions[i]) <= minDistance)   
                {
                    listAgents[i].transform.Translate(randomStartPositions[i] - agentPosition);     // Deceleration to make stopping smoother?
                    listAgents[i].GetComponent<Renderer>().material = blueGlowMaterial;
                    listAgents[i].tag = "Static";       
                }
            }
        }
    }










    ////////////////////////////   WANDERING BEHAVIOUR  ////////////////////////////

    // Wander: 
    public void Wander()
    {
        for (int i = 0; i < listAgents.Count; i++)
        {
            Vector3 agentPosition = listAgents[i].transform.position;
            Vector3 newDirection = CM.ConstrainedRandomVector(steeringForces[i], velocity, Mathf.PI / 6);     // ConstrainedRandomVector ~ PerlinVector:
            //Vector3 newDirection = CM.PerlinVector(steeringForces[i], agentPosition, velocity);             // Two different implementation approaches for the same result
            Vector3 newPosition = agentPosition + newDirection;

            if (!CM.Collides(minDistance, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()) &&
                !CM.OutsideBoundaries(newPosition, 0, AreaWidth))
            {
                listAgents[i].GetComponent<Renderer>().material = whiteGlowMaterial;
                listAgents[i].transform.Translate(newDirection);
                steeringForces[i] = newDirection;
            }
            else if (CM.OutsideBoundaries(newPosition, 0, AreaWidth))
            {
                Vector3 correctiveVector = newDirection * 0.2f + CM.BackToBoundaries(agentPosition, newDirection, velocity, 0, AreaWidth) * 0.8f;  // CORRECT BUT USE ACCELERATION FORCES!!!!
                listAgents[i].transform.Translate(correctiveVector);
                steeringForces[i] = correctiveVector;
            }
            else
            {
                Vector3 correctiveVector = CM.PerpendicularVector(newDirection, 1).normalized * velocity;
                listAgents[i].transform.Translate(correctiveVector);
                listAgents[i].GetComponent<Renderer>().material = redGlowMaterial;
            }
        }
    }







    ////////////////////////////   SQUARE MARCH   ////////////////////////////

    // SquareMarch: Agents all simultaneously walk in a square
    IEnumerator SquareMarch()
    {
        int switchDirection = 0;

        while (true)
        {
            int area = 50;
            Vector3 direction;

            switch (switchDirection)
            {
                case 0:
                    direction = new Vector3(velocity, 0, 0);
                    break;
                case 1:
                    direction = new Vector3(0, 0, velocity);
                    break;
                case 2:
                    direction = new Vector3(-velocity, 0, 0);
                    break;
                default:
                    direction = new Vector3(0, 0, -velocity);
                    break;
            }

            while (area-- > 0)
            {
                LinearWalk(direction);
                yield return new WaitForSeconds(Time.deltaTime);
            }

            if (switchDirection < 3)
                switchDirection++;
            else
                switchDirection = 0;

        }
    }


    // LinearWalk: agents walk linearly in one direction
    public void LinearWalk(Vector3 direction)
    {
        for (int i = 0; i < listAgents.Count; i++)
        {
            listAgents[i].transform.Translate(direction);
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


    // RandomWalkNoCollisionsDictionary: Same but with a dictionary for spatial subdivision  ->  Doesn't work very well...
    public void RandomWalkNoCollisionsDictionary()
    {
        foreach (var cell in dictionaryAgents.Keys.ToList())
        {
            foreach (GameObject agent in dictionaryAgents[cell].ToList())
            {
                Vector3 newDirection = CM.RandomVector(velocity);
                Vector3 agentPosition = agent.transform.position;
                Vector3 newPosition = agentPosition + newDirection;

                Vector2Int newCell = Subdivision.GridLocation(newPosition);
                List<Vector2Int> neighbouringCells = Subdivision.ClosestCells(newPosition, newCell, AreaInfluence);

                if (!CM.Collides(dictionaryAgents, minDistance, agentPosition, newPosition, neighbouringCells) &&
                    !CM.OutsideBoundaries(newPosition, 0, AreaWidth))
                {
                    agent.GetComponent<Renderer>().material = whiteGlowMaterial;
                    agent.transform.Translate(newDirection);

                    if (cell != newCell)
                        Subdivision.UpdateDictionary(dictionaryAgents, cell, newCell, agent);
                }
                else
                {
                    agent.GetComponent<Renderer>().material = redGlowMaterial;
                }

            }

        }
        print(dictionaryAgents.Count);
    }
}