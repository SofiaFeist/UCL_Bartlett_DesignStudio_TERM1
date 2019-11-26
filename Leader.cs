using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Leader 
{
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
        int tries = 10000;
        Vector3 leaderPosition = leader.transform.position;
        Vector3 randomDirection = randomVector(velocity);
        Vector3 newPosition = leaderPosition + randomDirection;

        if (!OutsideBoundaries(newPosition, AreaMin, AreaMax) &&
            !Collides(minDistance, leaderPosition, newPosition, listPositions))
        {
            leader.transform.Translate(randomDirection);
            this.Position = newPosition;
        }
        else
        {
            leader.transform.Translate(-randomDirection);
            this.Position = leader.transform.position - randomDirection;
        }
    }


    // EvadeClosestAgent: Leader moves away from the closest Agent
    public void EvadeClosestAgent(List<Vector3> listPositions, float velocity, float minDistance, float AreaMin, float AreaMax)
    {
        Vector3 leaderPosition = leader.transform.position;
        Vector3 ClosestAgentPosition = ClosestAgent(leaderPosition, listPositions);
        Vector3 awayFromClosestAgent = (leaderPosition - ClosestAgentPosition).normalized * velocity;

        if (!OutsideBoundaries(leader.transform.position, AreaMin, AreaMax) &&
            !Collides(minDistance, leaderPosition, awayFromClosestAgent, listPositions))
        {
            leader.transform.Translate(awayFromClosestAgent);
            this.Position = leader.transform.position + awayFromClosestAgent;
        }
        else 
        {
            leader.transform.Translate(- awayFromClosestAgent);   // REVIEW THIS VECTOR: When it reaches the area boundaries, it should do something
            this.Position = leaderPosition - awayFromClosestAgent;
        }
    }







    



    ////////////     Repeated Functions... => FIND A WAY TO REUSE THEM    ////////////
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

    Vector3 randomVector(float velocity)
    {
        float pi = Mathf.PI;
        float angle = Random.Range(-pi, pi);
        Vector3 vector = new Vector3(velocity * Mathf.Cos(angle), 0, velocity * Mathf.Sin(angle));

        return vector;
    }


    Vector3 ClosestAgent(Vector3 position, List<Vector3> listPositions)
    {
        Vector3 closestAgent = new Vector3();
        List<float> distances = new List<float>();

        foreach (var other in listPositions)
        {
            float distance = Vector3.Distance(position, other);
            distances.Add(distance);
            if (distances.All(d => distance <= d))
                closestAgent = other;
        }
        return closestAgent;
    }

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
}
