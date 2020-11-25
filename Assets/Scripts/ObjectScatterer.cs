using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace TileMapGenerator.MapGenerator
{
    public class ObjectScatterer : MonoBehaviour
    {
        [SerializeField] private List<GameObject> items;
        [SerializeField] private List<GameObject> enemies;
        [SerializeField] private List<GameObject> jumps;
        [SerializeField] private GameObject enter;
        [SerializeField] private GameObject exit;
        private Grid grid;
        private enum Objects { Item, Enemy, NPC, Enter, Exit, Jump }

        public void AddObjectsOnScene(int[] roomMap, int sizeX, int sizeY, int2 roomOrigin)
        {
            grid = GameObject.Find("Grid").GetComponent<Grid>();
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    //Objects type = (Objects)roomMap[x + y * sizeX] - 1;
                    if (roomMap[x + y * sizeX] - 1 >= 0)
                        switch ((Objects)roomMap[x + y * sizeX] - 1)
                        {
                            case Objects.Item:
                                Instantiate(items[0], new Vector3(x + roomOrigin.x, y + roomOrigin.y, 0), Quaternion.identity);
                                break;
                            case Objects.Enemy:
                                Instantiate(enemies[0], new Vector3(x + roomOrigin.x, y + roomOrigin.y, 0), Quaternion.identity);
                                break;
                            case Objects.Enter:
                                Instantiate(enter, new Vector3(x + roomOrigin.x, y + roomOrigin.y, 0), Quaternion.identity);
                                break;
                            case Objects.Exit:
                                Instantiate(exit, new Vector3(x + roomOrigin.x, y + roomOrigin.y, 0), Quaternion.identity);
                                break;
                            case Objects.Jump:
                                Instantiate(jumps[0], new Vector3(x + roomOrigin.x, y + roomOrigin.y, 0), Quaternion.identity);
                                break;
                            default:
                                Debug.Log("Can't find object with type index " + (roomMap[x + y * sizeX] - 1));
                                break;
                        }
                }
            }
        }
    }
}
