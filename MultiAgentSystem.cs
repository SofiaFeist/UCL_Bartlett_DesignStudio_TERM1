using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MultiAgentSystem : MonoBehaviour
{
    CommonMethods CM = new CommonMethods();
    SwarmOptimization SO;

    // Agent Properties
    public GameObject agent;
    Agents agents;
    float velocity = 1.0f;
    float acceleration = 0.0f;
    float minDistance = 1.0f;       // Minimum Distance between Agents: 1.0 = r*2 of a unit Agent

    // Seed Properties
    public GameObject seed;
    static Vector3 SeedPosition = new Vector3(50, 0, 50);
    Seed SeedInstance;

    // Leader Properties
    public GameObject leader;
    Vector3 leaderStartPosition = new Vector3(50, 0, 50);
    Leader LeaderInstance;


    // Environment Properties
    SpatialSubdivision Subdivision = new SpatialSubdivision(AreaMin, AreaMax, division);
    public static float AreaMin = -50f;
    public static float AreaMax = 50f;
    static int division = 50;             // Grid Subdivision: in how many cells is the grid divided into?
    float AreaInfluence = 3f;            // Area of Influence of each agent: how far they can "see" and react
    static int NumAgents = 100;



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
        imagePixels = PointsFromImage(new Vector3(40, 0, 40));
        NumAgents = imagePixels.Count;

        // INSTANTIATE AGENTS
        agents = new Agents(agent, whiteGlowMaterial, NumAgents);
        agents.PlaceAgentsRandomly(AreaMin, AreaMax, minDistance);
        //agents.PlaceAgentsInRows(new Vector3(-10, 0, 26));
        //agents.PlaceAgentsRandomlyDictionary(AreaMin, AreaMax, minDistance);
        //agents.PlaceAgentsInRowsDictionary(new Vector3(0, 0, 0));


        // LIST/COLLECTIONS INSTANTIATION
        randomStartPositions = agents.randomStartPositions;
        listAgents = agents.listAgents;
        steeringForces = CM.StartingSteeringDirections(listAgents.Count, velocity);
        dictionaryAgents = agents.dictionaryAgents;



        // INSTANTIATE SEED
        //SeedInstance = new Seed(seed, SeedPosition, blueGlowMaterial);


        // INSTANTIATE LEADER
        //LeaderInstance = new Leader(leader, leaderStartPosition, blueGlowMaterial, velocity);


        //foreach (var item in PointsFromImage(new Vector3(40, 0, 40)))
        //{
        //    Instantiate(agent, item, Quaternion.identity);
        //}

        // SQUARE MARCH (-> Use with PlaceAgentsInRows)
        //StartCoroutine("SquareMarch");


        // MESSAGE PROPAGATION ALGORITHM (-> Use with PlaceAgentsRandomly)
        //GameObject emitter = Instantiate(agent, SeedPosition, Quaternion.identity);
        //emitter.GetComponent<Renderer>().material = blueGlowMaterial;
        //StartCoroutine(Blink(emitter));
        //StartCoroutine("MessagePropagation");



        // SELF ASSEMBLY ALGORITHM (^ uncomment the imagePixels instantiation at the start of the Start Method)
        //StartCoroutine("SelfAssemblyOneByOne");
        //StartCoroutine("SequentialSelfAssembly");
        //StartCoroutine(SequentialAssembly(SeekAssignedPosition));



        // PARTICLE SWARM OPTIMIZATION
        //SO = new SwarmOptimization(randomStartPositions, NumAgents, AreaMin, AreaMax, velocity);
        //StartCoroutine(ParticleSwarmOptimization(200));
        
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        // AGENT WALKING METHODS
        //RandomWalk();
        //RandomWalkNoCollisions();
        //RandomWalkNoCollisionsDictionary();
        //Wander();

        //EdgeFollowing(listAgents[0], 2);     // (Use with PlaceAgentsInRows)



        // AGENT/SEED BEHAVIOURS
        //DiffusionLimitedAggregation();
        //Queue();



        // LEADER WALKING METHODS
        //LeaderInstance.RandomWalk(listAgents.Select(a => a.transform.position).ToList(), velocity, minDistance, AreaMin, AreaMax);
        //LeaderInstance.EvadeClosestAgent(listAgents.Select(a => a.transform.position).ToList(), velocity, minDistance, AreaMin, AreaMax);
        //LeaderInstance.RandomEvade(listAgents.Select(a => a.transform.position).ToList(), velocity, minDistance, AreaMin, AreaMax);
        //LeaderInstance.Wander(listAgents.Select(a => a.transform.position).ToList(), velocity, minDistance, AreaMin, AreaMax);

        // AGENT/LEADER BEHAVIOURS
        //FollowTheLeader();
        //AvoidTheLeader();







        // RESET (with Timer)
        //timer += Time.deltaTime;
        //if (timer < 5)
        //{
        //    RandomWalkNoCollisions();
        //}
        //Invoke("Reset", 5);   // Time in seconds
    }










    ////////////////////////   PARTICLE SWARM OPTIMIZATION  ////////////////////////  

    // Code reference: https://gist.github.com/IranNeto/21542660d740ac02dfce2f6aeb11ebeb#file-pso-py by Iran Neto

    // ParticleSwarmOptimization: Uses particle swarm mouvement to search and find a function minimum 
    IEnumerator ParticleSwarmOptimization(int NumIterations)
    {
        int iteration = 0;
        while (iteration < NumIterations)
        {
            SO.SetBest(listAgents, SO.TestFunction);
            SO.MoveAgents(listAgents);
            print(SO.SwarmBestPosition.ToString() + ", " + SO.SwarmBest.ToString());

            iteration++;
            yield return null;
        }

        print("The best position is " + SO.SwarmBestPosition.ToString() + ", with a fitness of " + SO.SwarmBest.ToString() + ".");
    }









    ////////////////////////   DISTRIBUTED SELF-ASSEMBLY ALGORITHM  ////////////////////////  
    //   =>  Mo agent has the full picture of the whole and have to rely on local sensing/interactions to figure out their positions

    // DistributedSelfAssembly: Agents move one by one to form an image; next agent only starts after the first one reaches its desired position  






    ////////////////////////   NON DISTRIBUTED SELF-ASSEMBLY ALGORITHM  //////////////////////

    // SelfAssembly delegate
    public delegate IEnumerator SelfAssembly(GameObject agent, Vector3 vector);  // or Func<GameObject, Vector3, IEnumerator> Name

    // SequentialAssembly: Agents move one by one to form an image; next agent starts a given time after the first one, forming a queue  
    IEnumerator SequentialAssembly(SelfAssembly SelfAssemblyMethod)  
    {
        for (int i = 0; i < listAgents.Count; i++)
        {
            StartCoroutine(SelfAssemblyMethod(listAgents[i], imagePixels[i]));
            yield return new WaitForSeconds(1.4f);
        }
    }


    // SeekAssignedPosition: Agents seek their own assigned position from a list of positions
    IEnumerator SeekAssignedPosition(GameObject agent, Vector3 desiredPosition)
    {
        while (true)
        {
            Vector3 agentPosition = agent.transform.position;
            Vector3 seekDirection = (desiredPosition - agentPosition).normalized * velocity;
            Vector3 newPosition = agentPosition + seekDirection;
            List<Vector3> agentPositions = listAgents.Select(a => a.transform.position).ToList();

            Vector3 closestAgent = CM.ClosestAgent(agentPosition, agentPositions);
            Vector3 awayFromAgent = (agentPosition - closestAgent).normalized * velocity;

            if (Vector3.Distance(agentPosition, desiredPosition) <= minDistance)    /// REDUCE VELOCITY
            {
                agent.transform.Translate(desiredPosition - agentPosition);
                agent.GetComponent<Renderer>().material = blueGlowMaterial;
                break;
            }
            else if (!CM.Colliding(AreaInfluence, agentPosition, newPosition, agentPositions) ||
                     Vector3.Distance(agentPosition, desiredPosition) < AreaInfluence ||
                     !CM.OpposingForces(seekDirection, awayFromAgent))
            {
                agent.transform.Translate(seekDirection);
            }
            else
            {
                EdgeFollowing(agent, minDistance * 2f);
            }
            yield return new WaitForSeconds(0.03f);
        }
    }


    // SeekClosestPosition: Agents seek the closest position to themselves from the list of positions   -> DOESN'T WORK VERY WELL BECAUSE AGENTS FORM A BARRICADE ON THE OUTLINE OF THE SHAPE
    IEnumerator SeekClosestPosition(GameObject agent, Vector3 unnecessaryVector)    // TRY USING CLASSES/INTERFACE TO AVOID THIS
    {
        while (true)
        {
            Vector3 agentPosition = agent.transform.position;
            Vector3 closestImagePoint = CM.ClosestPosition(agentPosition, imagePixels);
            Vector3 seekDirection = (closestImagePoint - agentPosition).normalized * velocity;
            Vector3 newPosition = agentPosition + seekDirection;
            List<Vector3> agentPositions = listAgents.Select(a => a.transform.position).ToList();

            if (Vector3.Distance(agentPosition, closestImagePoint) <= minDistance)
            {
                agent.transform.Translate(closestImagePoint - agentPosition);
                agent.GetComponent<Renderer>().material = blueGlowMaterial;
                agent.tag = "Static";
                imagePixels.Remove(closestImagePoint);
                break;
            }
            else if (!CM.Colliding(AreaInfluence, agentPosition, newPosition, agentPositions))
            {
                agent.transform.Translate(seekDirection);
            }
            else
            {
                Vector3 closestAgent = CM.ClosestAgent(agentPosition, agentPositions);
                Vector3 awayFromAgent = (agentPosition - closestAgent).normalized * velocity;

                if (CM.OpposingForces(seekDirection, awayFromAgent))
                    EdgeFollowing(agent, minDistance * 2f);
                else
                    agent.transform.Translate(seekDirection);
            }

            yield return new WaitForSeconds(0.03f);
        }
    }

   
    
    // SelfAssemblyOneByOne: Agents move one by one to form an image; next agent only starts after the first one reaches its desired position; no regard for collisions (NAIVE APPROACH)
    IEnumerator SelfAssemblyOneByOne()
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
    public List<Vector3> PointsFromImage(Vector3 imagePosition)
    {
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < texture.width; i += 2)
        {
            for (int j = 0; j < texture.height; j += 2)
            {
                Color color = texture.GetPixel(i, j);

                if (color.r < 1)
                    positions.Add(new Vector3(i + imagePosition.x, 0, j + imagePosition.z));

            }
        }
        return positions;
    }







    // EdgeFollowing: Makes a given agent follow along the edges of a group of agents, while maintaining a given distance
    public void EdgeFollowing(GameObject agent, float distance)
    {
        Vector3 agentPosition = agent.transform.position;   
        List<Vector3> agentPositions = listAgents.Select(a => a.transform.position).ToList();

        Vector3 closestAgent = CM.ClosestAgent(agentPosition, agentPositions);
        float distanceToColidingAgent = Vector3.Distance(agentPosition, closestAgent);
        Vector3 awayFromAgent = (agentPosition - closestAgent).normalized * velocity;
        Vector3 closerToAgent = -awayFromAgent;
        Vector3 perpendicularToAgent = CM.PerpendicularVector(awayFromAgent, 1);

        if (distanceToColidingAgent > distance)
            agent.transform.Translate((perpendicularToAgent + closerToAgent * 0.2f).normalized * velocity);    // To account for small curving displacements
        else if (distanceToColidingAgent < minDistance)
            agent.transform.Translate((perpendicularToAgent + awayFromAgent * 0.2f).normalized * velocity);
        else
            agent.transform.Translate(perpendicularToAgent);
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
                    List<Vector3> agentPositions = listAgents.Select(a => a.transform.position).ToList();

                    if (CM.Collides(AreaInfluence, agentPosition, staticAgents))
                    {
                        listAgents[i].GetComponent<Renderer>().material = blueGlowMaterial;
                        listAgents[i].tag = "Transmitting";
                        staticAgents.Add(agentPosition);
                    }
                    else
                    if (!CM.Colliding(minDistance, agentPosition, newPosition, agentPositions) &&
                        !CM.OutsideBoundaries(newPosition, AreaMin, AreaMax))
                    {
                        listAgents[i].transform.Translate(newDirection);
                        steeringForces[i] = newDirection;

                    }
                    else if (CM.OutsideBoundaries(newPosition, AreaMin, AreaMax))
                    {
                        Vector3 correctiveVector = newDirection * 0.2f + CM.BackToBoundaries(agentPosition, newDirection, velocity, AreaMin, AreaMax) * 0.8f;    // Use repellingForce too?
                        listAgents[i].transform.Translate(correctiveVector);
                        steeringForces[i] = correctiveVector;
                    }
                    else
                    {
                        Vector3 colidingAgent = CM.ClosestAgent(agentPosition, agentPositions);
                        float distanceToColidingAgent = Vector3.Distance(newPosition, colidingAgent);
                        Vector3 perpendicularToAgent = CM.AvoidObstacle(agentPosition, colidingAgent, newDirection, velocity);
                        float repellingForce = distanceToColidingAgent / AreaInfluence;

                        Vector3 correctiveVector = newDirection * repellingForce + perpendicularToAgent * (1 - repellingForce);
                        listAgents[i].transform.Translate(correctiveVector);
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
            Vector3 pursueLeader = (LeaderInstance.Position - agentPosition).normalized * velocity * 0.5f;
            Vector3 newPosition = agentPosition + pursueLeader;

            if (!CM.OutsideBoundaries(newPosition, AreaMin, AreaMax) &&
                !CM.Colliding(minDistance * 2, agentPosition, newPosition, new List<Vector3> { LeaderInstance.Position }) &&
                !CM.Colliding(minDistance * 2, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()))
            {
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

            if (CM.Colliding(areaInfluence, agentPosition, newRandomPosition, new List<Vector3>() { LeaderInstance.Position }))
            {
                listAgents[i].GetComponent<Renderer>().material = redGlowMaterial;
                listAgents[i].transform.Translate(avoidLeader);
            }
            if (!CM.OutsideBoundaries(newRandomPosition, AreaMin, AreaMax) &&
                !CM.Colliding(minDistance, agentPosition, newRandomPosition, listAgents.Select(a => a.transform.position).ToList()))
            {
                listAgents[i].GetComponent<Renderer>().material = whiteGlowMaterial;
                listAgents[i].transform.Translate(randomDirection);
            }
            else
            {
                Vector3 correctiveVector = randomDirection * 0.2f + CM.BackToBoundaries(agentPosition, randomDirection, velocity, AreaMin, AreaMax) * 0.8f;
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
                if (CM.Collides(minDistance, agentPosition, staticAgents))  
                {
                    Vector3 closestStaticAgent = CM.ClosestAgent(agentPosition, staticAgents);
                    float distanceToStaticAgent = Vector3.Distance(agentPosition, closestStaticAgent);
                    Vector3 moveCloserToStaticAgent = (closestStaticAgent - agentPosition).normalized * velocity;
                    Vector3 move = moveCloserToStaticAgent * (distanceToStaticAgent - minDistance);
                    listAgents[i].transform.Translate(move);

                    listAgents[i].GetComponent<Renderer>().material = blueGlowMaterial;
                    listAgents[i].tag = "Static";
                    staticAgents.Add(agentPosition);
                }
                else
                if (!CM.Colliding(minDistance, agentPosition, newPosition, listAgents.Where(a => a.tag == "Moving").Select(a => a.transform.position).ToList()) &&
                    !CM.OutsideBoundaries(newPosition, AreaMin, AreaMax))               
                {
                    listAgents[i].transform.Translate(newDirection);
                }
            }
        }
    }


    // Queue: Same principle as the DLA algorithm but the seed is constantly being replaced by the last agent to become static
    public void Queue()              
    {
        for (int i = 0; i < listAgents.Count; i++)
        {
            Vector3 newDirection = CM.RandomVector(velocity);
            Vector3 agentPosition = listAgents[i].transform.position;
            Vector3 newPosition = agentPosition + newDirection;

            if (listAgents[i].tag == "Moving")
            {
                if (CM.Collides(minDistance * 2, agentPosition, staticAgents))            // CHECK: Something is not okay with these calculations
                {
                    float distanceToStaticAgent = Vector3.Distance(agentPosition, staticAgents[0]);
                    Vector3 moveCloserToStaticAgent = (staticAgents[0] - agentPosition).normalized * velocity;
                    Vector3 move = moveCloserToStaticAgent * (distanceToStaticAgent - minDistance);             
                    listAgents[i].transform.Translate(move);

                    listAgents[i].GetComponent<Renderer>().material = blueGlowMaterial;
                    listAgents[i].tag = "Static";
                    staticAgents[0] = agentPosition;
                }
                else
                if (!CM.Colliding(minDistance, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()) &&
                    !CM.OutsideBoundaries(newPosition, AreaMin, AreaMax))                  
                {
                    listAgents[i].transform.Translate(newDirection);
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

    // Wander: Smooth form of random walk; small random displacements in the trajectory but no abrupt turns
    public void Wander()
    {
        for (int i = 0; i < listAgents.Count; i++)
        {
            Vector3 agentPosition = listAgents[i].transform.position;
            Vector3 newDirection = CM.ConstrainedRandomVector(steeringForces[i], velocity, Mathf.PI / 6);     // ConstrainedRandomVector ~ PerlinVector:
            //Vector3 newDirection = CM.PerlinVector(steeringForces[i], agentPosition, velocity);             // Two different implementation approaches for the same result
            Vector3 newPosition = agentPosition + newDirection;

            if (!CM.Colliding(AreaInfluence, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()) &&
                !CM.OutsideBoundaries(newPosition, AreaMin, AreaMax))
            {
                listAgents[i].transform.Translate(newDirection);
                steeringForces[i] = newDirection;
            }
            else if (CM.OutsideBoundaries(newPosition, AreaMin, AreaMax))
            {
                float repellingForce;
                if (newPosition.x >= AreaMax)
                    repellingForce = Mathf.Abs(newPosition.x - AreaMax);
                else if (newPosition.z >= AreaMax)
                    repellingForce = Mathf.Abs(newPosition.z - AreaMax);
                else if (newPosition.x <= AreaMin)
                    repellingForce = Mathf.Abs(newPosition.x - AreaMin);
                else
                    repellingForce = Mathf.Abs(newPosition.z - AreaMin);

                Vector3 correctiveVector = newDirection * (1 - repellingForce) + CM.BackToBoundaries(agentPosition, newDirection, velocity, AreaMin, AreaMax) * repellingForce; 
                listAgents[i].transform.Translate(correctiveVector);
                steeringForces[i] = correctiveVector;
            }
            else
            {
                Vector3 colidingAgent = CM.ClosestAgent(agentPosition, listAgents.Select(a => a.transform.position).ToList());
                float distanceToColidingAgent = Vector3.Distance(newPosition, colidingAgent);
                //Vector3 awayFromAgent = (agentPosition - colidingAgent).normalized * velocity;
                Vector3 perpendicularToAgent = CM.AvoidObstacle(agentPosition, colidingAgent, newDirection, velocity);
                float repellingForce = distanceToColidingAgent / AreaInfluence;

                Vector3 correctiveVector = newDirection * repellingForce + perpendicularToAgent * (1 - repellingForce);
                listAgents[i].transform.Translate(correctiveVector);
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

            if (!CM.OutsideBoundaries(newPosition, AreaMin, AreaMax))
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

            if (!CM.Colliding(minDistance, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()) &&
                !CM.OutsideBoundaries(newPosition, AreaMin, AreaMax))
            {
                listAgents[i].GetComponent<Renderer>().material = whiteGlowMaterial;
                listAgents[i].transform.Translate(newDirection);
            }
            else if (CM.Colliding(minDistance, agentPosition, newPosition, listAgents.Select(a => a.transform.position).ToList()))
            {
                listAgents[i].GetComponent<Renderer>().material = redGlowMaterial;
            }
        }
    }


    // RandomWalkNoCollisionsDictionary: Same but with a dictionary for spatial subdivision  ->  Doesn't work very well... Code is slower...
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

                if (!CM.Colliding(dictionaryAgents, minDistance, agentPosition, newPosition, neighbouringCells) &&
                    !CM.OutsideBoundaries(newPosition, AreaMin, AreaMax))
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