using System.Collections.Generic;
using UnityEngine;

public class Leader 
{
    CommonMethods CM = new CommonMethods();

    GameObject leader;
    public Vector3 Position;
    Vector3 startingForce;



    // Constructor
    public Leader(GameObject prefab, Vector3 position, Material material, float velocity)
    {
        this.Position = position;
        leader = Object.Instantiate(prefab, Position, Quaternion.identity);
        leader.GetComponent<Renderer>().material = material;
        startingForce = CM.RandomVector(velocity);
    }







    ////////////////////////   LEADER WALKING METHODS  ////////////////////////

    // RandomWalk: Leader walks around randomly
    public void RandomWalk(List<Vector3> listPositions, float velocity, float minDistance, float AreaMin, float AreaMax)
    {
        Vector3 leaderPosition = leader.transform.position;
        Vector3 randomDirection = CM.RandomVector(velocity);
        Vector3 newPosition = leaderPosition + randomDirection;

        if (!CM.OutsideBoundaries(newPosition, AreaMin, AreaMax) &&
            !CM.WillCollide(minDistance, leaderPosition, newPosition, listPositions))
        {
            leader.transform.Translate(randomDirection);
            this.Position = newPosition;
        }
        else if (CM.OutsideBoundaries(newPosition, AreaMin, AreaMax))
        {
            Vector3 back = CM.BackToBoundaries(leaderPosition, velocity, AreaMin, AreaMax);
            leader.transform.Translate(back);
            this.Position = leaderPosition + back;
        }
        else
        {
            Vector3 colidingAgent = CM.ClosestAgent(leaderPosition, listPositions);
            Vector3 awayFromAgent = leaderPosition - colidingAgent;

            Vector3 correctiveVector = (awayFromAgent).normalized * velocity;
            leader.transform.Translate(correctiveVector);
            this.Position = leaderPosition + correctiveVector;
        }
    }


    // EvadeClosestAgent: Leader moves away from the closest Agent       <-   Doesn't work very well because the leader can quickly become cornered
    public void EvadeClosestAgent(List<Vector3> listPositions, float velocity, float minDistance, float AreaMin, float AreaMax)
    {
        Vector3 leaderPosition = leader.transform.position;
        Vector3 ClosestAgentPosition = CM.ClosestPosition(leaderPosition, listPositions);
        Vector3 awayFromClosestAgent = (leaderPosition - ClosestAgentPosition).normalized * velocity;

        if (!CM.OutsideBoundaries(leaderPosition, AreaMin, AreaMax))
        {
            leader.transform.Translate(awayFromClosestAgent);
            this.Position = leaderPosition + awayFromClosestAgent;
        }
        else 
        {
            Vector3 back = CM.BackToBoundaries(leaderPosition, velocity, AreaMin, AreaMax);
            leader.transform.Translate(back);
            this.Position = leaderPosition + back;
        }
    }


    // RandomEvade: Hybrid Method between Random Walk and Evade closest agent       <-   Evades only when agents are within a certain proximity 
    public void RandomEvade(List<Vector3> listPositions, float velocity, float minDistance, float AreaMin, float AreaMax)
    {
        float areaInfluence = 5.0f;
        Vector3 leaderPosition = leader.transform.position;
        Vector3 randomDirection = CM.RandomVector(velocity);
        Vector3 ClosestAgentPosition = CM.ClosestPosition(leaderPosition, listPositions);
        Vector3 awayFromClosestAgent = (leaderPosition - ClosestAgentPosition).normalized * velocity;

        if (CM.Colliding(areaInfluence, leaderPosition, listPositions))
        {
            Vector3 avoid = (randomDirection + awayFromClosestAgent).normalized * velocity;
            leader.transform.Translate(avoid);
            this.Position = leader.transform.position + avoid;
        }
        else if (!CM.OutsideBoundaries(leader.transform.position, AreaMin, AreaMax) &&
                 !CM.Colliding(minDistance, leaderPosition, listPositions))
        {
            leader.transform.Translate(randomDirection);
            this.Position = leaderPosition + randomDirection;
        }
        else
        {
            Vector3 back = (randomDirection + CM.BackToBoundaries(leaderPosition, velocity, AreaMin, AreaMax)).normalized * velocity;
            leader.transform.Translate(back);     
            this.Position = leaderPosition + back;
        }
    }


    // Wander: Smooth form of random walk; small random displacements in the trajectory but no abrupt turns
    public void Wander(List<Vector3> listPositions, float velocity, float areaInfluence, float AreaMin, float AreaMax)
    {
        Vector3 leaderPosition = leader.transform.position;
        //Vector3 newDirection = CM.ConstrainedRandomVector(startingForce, velocity, Mathf.PI / 6);        // ConstrainedRandomVector ~ PerlinVector:
        Vector3 newDirection = CM.PerlinVector(startingForce, leaderPosition, velocity);                   // Two different implementation approaches for the same result
        Vector3 newPosition = leaderPosition + newDirection;

        if (!CM.Colliding(areaInfluence, leaderPosition, listPositions) &&
            !CM.OutsideBoundaries(newPosition, AreaMin, AreaMax))
        {
            leader.transform.Translate(newDirection);
            this.Position = newPosition;
            startingForce = newDirection;
        }
        else if (CM.OutsideBoundaries(newPosition, AreaMin, AreaMax))
        {
            Vector3 back = (newDirection + CM.BackToBoundaries(leaderPosition, newDirection, velocity, AreaMin, AreaMax)).normalized * velocity;
            leader.transform.Translate(back);
            this.Position = leaderPosition + back;
            startingForce = back;
        }
        else 
        {
            Vector3 colidingAgent = CM.ClosestAgent(leaderPosition, listPositions);
            float distanceToColidingAgent = Vector3.Distance(leaderPosition, colidingAgent);
            Vector3 awayFromAgent = (leaderPosition - colidingAgent).normalized * velocity;
            float repellingForce = distanceToColidingAgent / areaInfluence;

            Vector3 correctiveVector = (newDirection * repellingForce + awayFromAgent * (1 - repellingForce)).normalized * velocity;
            leader.transform.Translate(correctiveVector);
            this.Position = leaderPosition + correctiveVector;
            startingForce = correctiveVector;
        }
    }
}