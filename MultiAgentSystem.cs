using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MultiAgentSystem : MonoBehaviour
{
    public GameObject Agent;
    float AreaWidth = 100f;
    int numAgents = 50;

    public List<GameObject> listAgents = new List<GameObject>();
    


    // Start is called before the first frame update
    void Start()
    {
        placeAgents();
        /*
        foreach (var item in loopDebugging())
        {
            print(item);
        }
        */
    }


    // Update is called once per frame
    void Update()
    {
        //moveAgentsRandomly();
        moveAgentsNoColisions();
    }







    ////////////////////////////   AGENT MOUVEMENT  ////////////////////////////

    // moveAgentsRandomly: moves the agents randomly in space with no regards for their surroundings
    public void moveAgentsRandomly()
    {
        foreach (GameObject agent in listAgents)
        {
            agent.transform.Translate(randomVectorXZ());
        }
    }


    // moveAgentsNoColisions: moves the agents randomly in space while trying to avoid intersecting other agents -> TO BE COMPLETED
    public void moveAgentsNoColisions()
    {
        for (int i = 0; i < listAgents.Count; i++)
        {
            GameObject agent = listAgents[i];
            Vector3 newDirection = randomVector();
            Vector3 newPosition = agent.transform.position + newDirection;

            // TRY: Spatial data structures (e.g. bin-lattice spatial subdivision) to reduce algorithmic complexity
            for (int j = 0; j < listAgents.Count; j++)
            {
                if (i != j)
                {
                    float minDist = 1.0f;
                    float d = Vector3.Distance(newPosition, listAgents[j].transform.position);

                    if (d < minDist)
                    {
                        newDirection = randomVector();
                        newPosition = agent.transform.position + newDirection;
                        if (i > 0) 
                            i--;
                    }
                    else
                    {
                        agent.transform.Translate(newDirection);
                    }
                }
            }
        }
    }


    // Random Direction Vector in Polar coordinates: infinite possible directions (0º -> 360º float)
    Vector3 randomVector()
    {
        float r = 0.005f;
        float pi = Mathf.PI;
        float angle = Random.Range(-pi, pi);
        Vector3 vector = new Vector3(r * Mathf.Cos(angle), 0, r * Mathf.Sin(angle));

        return vector;
    }


    // Random Direction Vector in Cartesian coordinates: 4 possible directions (+x, -x, +z, -z)
    Vector3 randomVectorXZ()
    {
        Vector3 vector;
        float r = 1.0f;
        int random = Random.Range(0, 4);

        switch (random)
        {
            case 0:
                vector = new Vector3(r, 0, 0);
                break;
            case 1:
                vector = new Vector3(-r, 0, 0);
                break;
            case 2:
                vector = new Vector3(0, 0, r);
                break;
            default:
                vector = new Vector3(0, 0, -r);
                break;
        }
        return vector;
    }







    ////////////////////////////   INITIAL AGENT PLACEMENT  ////////////////////////////
   
    // placeAgents: places the agents according to the distinctStartPositions list
    public List<GameObject> placeAgents()
    {
        List<Vector3> startPositions = distinctStartPositions();
        
        foreach (Vector3 position in startPositions)
        {
            GameObject placeAgent = Instantiate(Agent, position, Quaternion.identity);
            listAgents.Add(placeAgent);
        }
        return listAgents;
    }


    // loopDebugging: Simplified version of distinctStartPositions(), to understand what's happening inside the loop -> TO BE DELETED (eventually)
    public List<int> loopDebugging()
    {
        List<int> list = new List<int>() { 1, 2, 8, 5, 3};
        int minDist = 2;

        // Counters
        int a = 0;
        int b = 0;
        int c = 0;

        for (int i = 0; i < list.Count; i++)
        {
            a++;
            
            for (int j = 0; j < list.Count; j++)
            {
                if (i != j)  // Ensures that we don't compare the same list element (i = j)
                {
                    float d = Mathf.Abs(list[i] - list[j]);
                    //print("d = " + d);
                    b++;

                    if(d < minDist) // Compares if the difference between elements is smaller than a given minDist; if so, replace with a new random
                    {
                        list[i] = Random.Range(0, 10);
                        c++;

                        if (i > 0)  // Repeats the loop for the newly replaced element; goes back one count in the loop to do it
                            i--;
                    }
                }

            }

        }
        print("a repeats " + a + " times");
        print("b repeats " + b + " times");
        print("c repeats " + c + " times");
        return list;
    }


    // Dictinct Start Positions: makes sure that there are no Agents placed in overlapping/intersecting positions
    public List<Vector3> distinctStartPositions()
    {
        List<Vector3> randomPositions = agentStartPositions();
        float minDist = 1.0f;     ///// 1.0 = r*2 of a unit Agent

        for (int i = 0; i < randomPositions.Count; i++)
        {
            for (int j = 0; j < randomPositions.Count; j++)
            {
                if (i != j)  // Ensures that we don't compare the same vector (i = j)
                {
                    float d = Vector3.Distance(randomPositions[i], randomPositions[j]);

                    if (d < minDist) // Compares if the distance between vectors is smaller than a given minDist; if so, replace with a new random vector
                    {
                        randomPositions[i] = new Vector3(randomCoordinate(), 0, randomCoordinate());

                        if (i > 0)  // Repeats the loop for the newly replaced vector; goes back one count in the loop to do it
                            i--;
                    }
                }
            }

        }

        return randomPositions;
    }


    // Agent Start Positions: list of randomly created vectors representing the starting position of the agents
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


    // Random Coordinate (= Float) between the interval of 0 and AreaWidth
    float randomCoordinate()
    {
        return Random.Range(0.0f, AreaWidth);
    }
}






// Eventually: Organize everything into classes...
public class Agents
{
   

}

