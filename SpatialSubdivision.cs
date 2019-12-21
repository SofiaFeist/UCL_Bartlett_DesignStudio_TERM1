using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpatialSubdivision 
{
    float AreaMin;
    float AreaMax;
    int division;

    


    public SpatialSubdivision(float _areaMin, float _areaMax, int _division)
    {
        this.AreaMin = _areaMin;
        this.AreaMax = _areaMax;
        this.division = _division;
    }









    ////////////////////////////   SPATIAL SUBDIVISION METHODS   ////////////////////////////

    // GridLocation: Finds the location in the grid of a given position 
    public Vector2Int GridLocation(Vector3 position)
    {
        int coordX;
        int coordZ;
        float subdivision = (AreaMax - AreaMin) / division;


        if(position.x >= AreaMax)
            coordX = division - 1;
        else if (position.x <= AreaMin)
            coordX = 0;
        else
            coordX = (int) Mathf.Floor(position.x / subdivision);


        if (position.z >= AreaMax)
            coordZ = division - 1;
        else if (position.z <= AreaMin)
            coordZ = 0;
        else
            coordZ = (int) Mathf.Floor(position.z / subdivision);


        return new Vector2Int(coordX, coordZ);
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


    // UpdateDictionary: Updates dictionary values as the agents move around
    public void UpdateDictionary(Dictionary<Vector2Int, List<GameObject>> dictionary, Vector2Int currentCell, Vector2Int newCell, GameObject agent)
    {
        if (dictionary.TryGetValue(newCell, out List<GameObject> agentsInCell))
        {
            agentsInCell.Add(agent);
            dictionary[currentCell].Remove(agent);
            if (!dictionary[currentCell].Any())
                dictionary.Remove(currentCell);
        }
        else
        {
            agentsInCell = new List<GameObject>();
            agentsInCell.Add(agent);
            dictionary.Add(newCell, agentsInCell);
        }
    }
}