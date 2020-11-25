using UnityEngine;
using Unity.Mathematics;
using TileMapGenerator.MapGenerator;

public class Testing : MonoBehaviour
{
    private MapBuilder mapBuilder;

    private void Start()
    {
        mapBuilder = new MapBuilder(14, 14, -70, -70);
        mapBuilder.BuildMap();
    }
}
