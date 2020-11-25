using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace TileMapGenerator.NavigationGraphGenerator
{
    public class NavGraphGenerator
    {
        private List<RoomNavGraph> roomNavGraphs = new List<RoomNavGraph>();
        private Dictionary<int, PathNode> navigationGraph = new Dictionary<int, PathNode>();

        //public void AddRoomNavGraph(int[] map, int sizeX, int sizeY, int layer, int2 roomOrigin)
        //{
        //    // If we get the 'ground' layer of room map, we create a NavGraph of that room
        //    // else we modify existing graph
        //    if (layer == 0)
        //        roomNavGraphs.Add(new RoomNavGraph(map, sizeX, sizeY, roomOrigin));
        //    else
        //        roomNavGraphs[roomNavGraphs.Count - 1].ModifyRoomNavGraph(map);
        //}

        public RoomNavGraph GenerateRoomNavGraph(int[] map, int sizeX, int sizeY, int2 roomOrigin, int[] neighbourMap = null)
        {
            // Create NavGraph of 'ground' layer of the room
            RoomNavGraph rg = new RoomNavGraph(map, sizeX, sizeY, roomOrigin, neighbourMap);
            roomNavGraphs.Add(rg);
            return rg;
        }

        public void ConnectRoomNavGraphs()
        {
            for (int i = 0; i < roomNavGraphs.Count; i++)
            {
                for (int j = i + 1; j < roomNavGraphs.Count; j++)
                {
                    if (IsHorisontalNeighbours(i, j) || IsVerticalNeighbours(i, j))
                    {
                        roomNavGraphs[i].ConnectBlindNodes(roomNavGraphs[j]);
                        roomNavGraphs[j].ConnectBlindNodes(roomNavGraphs[i]);
                    }
                }
                roomNavGraphs[i].CleanUp();
            }

            BuildFullNavGraph();
            roomNavGraphs.Clear();
        }

        private void BuildFullNavGraph()
        {
            foreach (RoomNavGraph graph in roomNavGraphs)
            {
                graph.RecalcNodesCoordsToWorld();
                foreach (PathNode node in graph.nodes.Values)
                {
                    navigationGraph.Add(node.GetHashCode(), node);
                }
            }
            Debug.Log("There are " + navigationGraph.Count + " nodes in graph");
        }

        private bool IsHorisontalNeighbours(int i, int j)
        {
            return Mathf.Abs(roomNavGraphs[i].RoomOrigin.x - roomNavGraphs[j].RoomOrigin.x) == roomNavGraphs[i].RoomSizeX
                && roomNavGraphs[i].RoomOrigin.y == roomNavGraphs[j].RoomOrigin.y;
        }

        private bool IsVerticalNeighbours(int i, int j)
        {
            return Mathf.Abs(roomNavGraphs[i].RoomOrigin.y - roomNavGraphs[j].RoomOrigin.y) == roomNavGraphs[i].RoomSizeY
                && roomNavGraphs[i].RoomOrigin.x == roomNavGraphs[j].RoomOrigin.x;
        }
    }
}
