using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpatialSubdivision 
{
    float AreaMin;
    float AreaMax;
    int division;

    Dictionary<Vector2Int, List<GameObject>> dictionaryAgents = new Dictionary<Vector2Int, List<GameObject>>();

    public SpatialSubdivision(float _areaMin, float _areaMax, int _division)
    {
        this.AreaMin = _areaMin;
        this.AreaMax = _areaMax;
        this.division = _division;
    }


    
    
    
    
    //public bool CollidesD(float minDistance, Vector3 position, Vector3 newPosition, List<Vector2Int> listCells)
    //{
    //    foreach (var cell in listCells)
    //    {
    //        if (!dictionaryAgents.ContainsKey(cell)) continue;

    //        foreach (GameObject agent in dictionaryAgents[cell])
    //        {
    //            Vector3 otherPosition = agent.transform.position;

    //            if (position != otherPosition)
    //            {
    //                var distance = Vector3.Distance(newPosition, otherPosition);
    //                if (distance <= minDistance)
    //                    return true;
    //            }
    //        }
    //    }
    //    return false;
    //}


    // placeAgents: places the agents according to the AgentStartPositions list
    //public Dictionary<Vector2Int, List<GameObject>> PlaceAgents()
    //{
    //    foreach (Vector3 position in agents.AgentStartPositions(0, AreaWidth, minDistance))
    //    {
    //        Vector2Int cell = GridLocation(position);
    //        GameObject placeAgent = Instantiate(agent, position, Quaternion.identity);
    //        placeAgent.GetComponent<Renderer>().material = whiteGlowMaterial;
    //        placeAgent.tag = "Moving";
    //        listAgents.Add(placeAgent);          // Do I still need this list?

    //        if (dictionaryAgents.TryGetValue(cell, out List<GameObject> agentsInCell))
    //        {
    //            agentsInCell.Add(placeAgent);
    //        }
    //        else
    //        {
    //            agentsInCell = new List<GameObject>();
    //            agentsInCell.Add(placeAgent);
    //            dictionaryAgents.Add(cell, agentsInCell);
    //        }
    //    }
    //    return dictionaryAgents;
    //}



    ////////////////////////////   SPATIAL SUBDIVISION METHODS   ////////////////////////////

    // GridLocation: Finds the location in the grid of a given position 
    public Vector2Int GridLocation(Vector3 position)
    {
        int x = 0;
        int z = 0;
        float coordX = position.x;
        float coordZ = position.z;

        int FindCell(int count, float coordinate)
        {
            if (coordinate >= AreaMax)
                count = division - 1;
            else
            {
                while (coordinate >= (AreaMax / division) * (count + 1))
                {
                    count++;
                }
            }
            return count;
        }

        return new Vector2Int(FindCell(x, coordX), FindCell(z, coordZ));
    }


    // ClosestCells: Finds the closest cells within the Area of Influence of a given position
    public List<Vector2Int> ClosestCells(Vector3 position, Vector2Int cellPosition, float AreaInfluence)
    {
        Vector3 xMax = position + new Vector3(AreaInfluence, 0, 0);
        Vector3 xMin = position - new Vector3(AreaInfluence, 0, 0);
        Vector3 zMax = position + new Vector3(0, 0, AreaInfluence);
        Vector3 zMin = position - new Vector3(0, 0, AreaInfluence);

        Vector2Int cellPosXMax = GridLocation(xMax);
        Vector2Int cellPosXMin = GridLocation(xMin);
        Vector2Int cellPosYMax = GridLocation(zMax);
        Vector2Int cellPosYMin = GridLocation(zMin);

        List<Vector2Int> closestCells = new List<Vector2Int>();

        if (cellPosXMax != cellPosition &&
            cellPosXMin != cellPosition &&
            cellPosYMax != cellPosition &&
            cellPosYMin != cellPosition)
        {
            for (int i = cellPosXMin.x; i <= cellPosXMax.x; i++)
            {
                for (int j = cellPosYMin.y; j <= cellPosYMax.y; j++)
                {
                    closestCells.Add(new Vector2Int(i, j));
                }
            }
        }
        else
        {
            closestCells.Add(cellPosition);
        }

        return closestCells;
    }


    // UpdateDictionary: Updates dictionary cells as the agents move around
    public void UpdateDictionary(Vector2Int currentCell, Vector2Int newCell, GameObject agent)
    {
        if (dictionaryAgents.TryGetValue(newCell, out List<GameObject> agentsInCell))
        {
            agentsInCell.Add(agent);
            dictionaryAgents[currentCell].Remove(agent);
            if (!dictionaryAgents[currentCell].Any())
                dictionaryAgents.Remove(currentCell);
        }
        else
        {
            agentsInCell = new List<GameObject>();
            agentsInCell.Add(agent);
            dictionaryAgents.Add(newCell, agentsInCell);
        }
    }
}
