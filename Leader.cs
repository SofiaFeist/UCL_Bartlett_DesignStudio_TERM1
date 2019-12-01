using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Leader 
{
    CommonMethods CM = new CommonMethods();

    GameObject leader;
    public Vector3 Position;



    // Constructor
    public Leader(GameObject prefab, Vector3 position, Material material)
    {
        this.Position = position;
        leader = Object.Instantiate(prefab, Position, Quaternion.identity);
        leader.GetComponent<Renderer>().material = material;
    }




    // Leader Methods
    // RandomWalk: Leader walks around randomly
    public void RandomWalk(List<Vector3> listPositions, float velocity, float minDistance, float AreaMin, float AreaMax)
    {
        Vector3 leaderPosition = leader.transform.position;
        Vector3 randomDirection = CM.RandomVector(velocity);
        Vector3 newPosition = leaderPosition + randomDirection;

        if (!CM.OutsideBoundaries(newPosition, AreaMin, AreaMax) &&
            !CM.Collides(minDistance, leaderPosition, newPosition, listPositions))
        {
            leader.transform.Translate(randomDirection);
            this.Position = newPosition;
        }
        else
        {
            Vector3 back = CM.BackToBoundaries(leaderPosition, velocity, AreaMin, AreaMax) + randomDirection;
            leader.transform.Translate(back);
            this.Position = leaderPosition + back;
        }
    }


    // EvadeClosestAgent: Leader moves away from the closest Agent
    public void EvadeClosestAgent(List<Vector3> listPositions, float velocity, float minDistance, float AreaMin, float AreaMax)
    {
        Vector3 leaderPosition = leader.transform.position;
        Vector3 ClosestAgentPosition = CM.ClosestAgent(leaderPosition, listPositions);
        Vector3 awayFromClosestAgent = (leaderPosition - ClosestAgentPosition).normalized * velocity;

        if (!CM.OutsideBoundaries(leader.transform.position, AreaMin, AreaMax) &&
            !CM.Collides(minDistance, leaderPosition, awayFromClosestAgent, listPositions))
        {
            leader.transform.Translate(awayFromClosestAgent);
            this.Position = leaderPosition + awayFromClosestAgent;
        }
        else 
        {
            Vector3 back = CM.BackToBoundaries(leaderPosition, velocity, AreaMin, AreaMax) + awayFromClosestAgent;
            leader.transform.Translate(back);   
            this.Position = leaderPosition + back;
        }
    }


    // RandomEvade: Hybrid Method between Random Walk and Evade closest agent -> Evades only when agents are within a certain proximity
    public void RandomEvade(List<Vector3> listPositions, float velocity, float minDistance, float AreaMin, float AreaMax)
    {
        float areaInfluence = 5.0f;
        Vector3 leaderPosition = leader.transform.position;
        Vector3 randomDirection = CM.RandomVector(velocity);
        Vector3 newRandomPosition = leaderPosition + randomDirection;
        Vector3 ClosestAgentPosition = CM.ClosestAgent(leaderPosition, listPositions);
        Vector3 awayFromClosestAgent = (leaderPosition - ClosestAgentPosition).normalized * velocity;

        if (CM.Collides(areaInfluence, leaderPosition, newRandomPosition, listPositions))
        {
            leader.transform.Translate(awayFromClosestAgent);
            this.Position = leader.transform.position + awayFromClosestAgent;
        }
        else if (!CM.OutsideBoundaries(leader.transform.position, AreaMin, AreaMax) &&
                 !CM.Collides(minDistance, leaderPosition, newRandomPosition, listPositions))
        {
            leader.transform.Translate(randomDirection);
            this.Position = leaderPosition + randomDirection;
        }
        else
        {
            Vector3 back = CM.BackToBoundaries(leaderPosition, velocity, AreaMin, AreaMax) + randomDirection;
            leader.transform.Translate(back);     
            this.Position = leaderPosition + back;
        }
    }
}
