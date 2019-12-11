using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CommonMethods 
{


    ////////////////////////   AREA CHECKS  ////////////////////////

    // ClosestPoint: Calculates the closest position between an agent and an outside list of positions
    public Vector3 ClosestPosition(Vector3 position, List<Vector3> listPositions)
    {
        Vector3 closestPosition = new Vector3();
        List<float> distances = new List<float>();

        foreach (Vector3 p in listPositions)
        {
            float distance = Vector3.Distance(position, p);
            distances.Add(distance);
            if (distances.All(d => distance <= d))
                closestPosition = p;
        }
        return closestPosition;
    }

    // ClosestAgent: Calculates the closest agent between a given agent position and a list of agent positions
    public Vector3 ClosestAgent(Vector3 position, List<Vector3> listPositions)
    {
        Vector3 closestAgent = new Vector3();
        List<float> distances = new List<float>();

        foreach (Vector3 other in listPositions)
        {
            if (position != other)  // So that the agent doesn't compare against it's own position (assumes position can exist inside listPositions)
            {
                float distance = Vector3.Distance(position, other);
                distances.Add(distance);
                if (distances.All(d => distance <= d))
                    closestAgent = other;
            }
        }
        return closestAgent;
    }


    // Collides: Checks if an agent IS ALREADY in another agent's space
    public bool Collides(float minDistance, Vector3 position, List<Vector3> listPositions)
    {
        foreach (var other in listPositions)
        {
            if (position != other)
            {
                var distance = Vector3.Distance(position, other);
                if (distance <= minDistance)
                    return true;
            }
        }
        return false;
    }


    // Colliding: Checks if an agent IS MOVING into another agent's space
    public bool Colliding(float minDistance, Vector3 position, Vector3 newPosition, List<Vector3> listPositions)
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


    // Colliding: Same but only works with the spatial subdivision (dictionary of positions and game objects)
    public bool Colliding(Dictionary<Vector2Int, List<GameObject>> dictionary, float minDistance, Vector3 position, Vector3 newPosition, List<Vector2Int> listCells)
    {
        foreach (var cell in listCells)
        {
            if (!dictionary.ContainsKey(cell)) continue;

            foreach (GameObject agent in dictionary[cell])
            {
                Vector3 otherPosition = agent.transform.position;

                if (position != otherPosition)
                {
                    var distance = Vector3.Distance(newPosition, otherPosition);
                    if (distance <= minDistance)
                        return true;
                }
            }
        }
        return false;
    }


    // OutsideBoundaries: Checks if a given vector is located outside of the given boundaries (square)
    public bool OutsideBoundaries(Vector3 position, float AreaMin, float AreaMax)
    {
        if (position.x > AreaMax ||
            position.z > AreaMax ||
            position.x < AreaMin ||
            position.z < AreaMin)
            return true;
        else
            return false;
    }


    // BackToBoundaries: Corrective Vector that drives an agent back within the boundaries -> Perpendicular to Boundary
    public Vector3 BackToBoundaries(Vector3 position, float velocity, float AreaMin, float AreaMax)
    {
        if (position.x > AreaMax)
            return new Vector3(-velocity, 0, 0);
        else if (position.z > AreaMax)
            return new Vector3(0, 0, -velocity);
        else if (position.x < AreaMin)
            return new Vector3(velocity, 0, 0);
        else
            return new Vector3(0, 0, velocity);
    }


    // BackToBoundaries: Corrective Vector that drives an agent back within the boundaries -> Perpendicular to Steering Vector
    public Vector3 BackToBoundaries(Vector3 position, Vector3 steeringVector, float velocity, float AreaMin, float AreaMax) 
    {
        Vector3 PositiveVec = PerpendicularVector(steeringVector, 1).normalized * velocity;
        Vector3 NegativeVec = PerpendicularVector(steeringVector, -1).normalized * velocity;

        if (OutsideBoundaries(position + PositiveVec, AreaMin, AreaMax))
            return NegativeVec;
        else
            return PositiveVec;
    }


    // AvoidObstacle: Corrective Vector perpendicular to the current trajectory in order avoid the closest obstacle
    public Vector3 AvoidObstacle(Vector3 position, Vector3 closestObstacle, Vector3 steeringVector, float velocity)
    {
        Vector3 PositiveVec = PerpendicularVector(steeringVector, 1).normalized * velocity;
        Vector3 NegativeVec = PerpendicularVector(steeringVector, -1).normalized * velocity;

        if (Vector3.Distance(position + PositiveVec, closestObstacle) < Vector3.Distance(position + NegativeVec, closestObstacle))
            return NegativeVec;
        else
            return PositiveVec;
    }

    
    // OppositeForces: Checks if vectors are facing each other (with a range of 180º)
    public bool OpposingForces(Vector3 currentTrajectory, Vector3 repellingForce)
    {
        float dotProduct = Vector3.Dot(currentTrajectory.normalized, repellingForce.normalized);

        if (dotProduct < 0)
            return true;
        else
            return false;
    }










    ////////////////////////   VECTORS  ////////////////////////

    // AbsVector: Returns a vector whose elements are the absolute values of each of the specified vector's elements.
    public Vector3 AbsVector(Vector3 vector)
    {
        vector.x = Mathf.Abs(vector.x);
        vector.y = Mathf.Abs(vector.y);
        vector.z = Mathf.Abs(vector.z);
        return vector;
    }


    // PerpendicularVector: Returns a vector perpendicular to the given vector.
    public Vector3 PerpendicularVector(Vector3 vector, float direction)
    {
        float coordX = vector.z * (- direction);
        float coordZ = vector.x * direction;
        return new Vector3(coordX, 0, coordZ);
    }


    // PerlinVector: Vector using Perlin Noise
    public Vector3 PerlinVector(Vector3 steeringDirection, Vector3 position, float velocity)
    {
        float noise = Mathf.PerlinNoise(position.x, position.z);
        float angle = Mathf.Atan2(steeringDirection.z, steeringDirection.x);

        float range;
        if (noise < 0.5)
            range = angle - noise;
        else
            range = angle + noise * 0.5f;

        Vector3 vector = new Vector3(velocity * Mathf.Cos(range), 0, velocity * Mathf.Sin(range));
        return vector;
    }


    // ConstrainedRandomVector: Random Vector constrained to a given amplitude (-amplitude -> +amplitude)
    public Vector3 ConstrainedRandomVector(Vector3 steeringDirection, float velocity, float amplitude)
    {
        float angle = Mathf.Atan2(steeringDirection.z, steeringDirection.x);
        float range = Random.Range(angle - amplitude, angle + amplitude);
        Vector3 vector = new Vector3(velocity * Mathf.Cos(range), 0, velocity * Mathf.Sin(range));

        return vector;
    }


    //StartingSteeringDirections: Creates a list of random Vectors as starting steering directions
    public List<Vector3> StartingSteeringDirections(int numAgents, float velocity)
    {
        List<Vector3> vectorList = new List<Vector3>(numAgents);

        for (int i = 0; i < numAgents; i++)
        {
            vectorList.Add(RandomVector(velocity));
        }

        return vectorList;
    }


    // Random Direction Vector in Polar coordinates: infinite possible directions (0º -> 360º float)
    public Vector3 RandomVector(float velocity)
    {
        float pi = Mathf.PI;
        float angle = Random.Range(-pi, pi);
        Vector3 vector = new Vector3(velocity * Mathf.Cos(angle), 0, velocity * Mathf.Sin(angle));

        return vector;
    }


    // Random Direction Vector in Cartesian coordinates: 4 possible directions (+x, -x, +z, -z)
    public Vector3 RandomVectorXZ(float velocity)
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


    // Random Position (= Vector), with the X and Z coordinates placed randomly between the given boundaries
    public Vector3 RandomPosition(float AreaMin, float AreaMax)
    {
        return new Vector3(Random.Range(AreaMin, AreaMax), 0, Random.Range(AreaMin, AreaMax));
    }

}
