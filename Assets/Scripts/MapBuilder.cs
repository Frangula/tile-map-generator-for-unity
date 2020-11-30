using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine.SceneManagement;
using TileMapGenerator.NavigationGraphGenerator;

namespace TileMapGenerator.MapGenerator
{
    public class MapBuilder
    {
        private bool[,] mapMatrix;

        private NavGraphGenerator navGraphGenerator;

        private List<RoomBase> nextVar = new List<RoomBase>();
        private Dictionary<int, PathNode> navigationGraph = new Dictionary<int, PathNode>();
        //private int 

        // list of all possible rooms
        public List<RoomBase> rooms;
        // list of map rooms
        public List<RoomBase> mapList;
        public int2 mapOrigin;

        public MapBuilder(int sizeX, int sizeY, int origX, int origY)
        {
            mapMatrix = new bool[sizeX, sizeY];
            Array.Clear(mapMatrix, 0, mapMatrix.Length);
            mapOrigin = new int2(origX, origY);

            navGraphGenerator = new NavGraphGenerator();
        }

        public void BuildMap()
        {
            LoadRoomsForScene(SceneManager.GetActiveScene().name);
            InitializeMap();
            BuildConnections(mapList[0], 4);
            //BuildNavGraph();
            FillTilemap(SceneManager.GetActiveScene().name);

            BuildNavGraph();

            rooms.Clear();
        }

        private void InitializeMap()
        {
            mapList = new List<RoomBase>();
            mapList.Add(rooms[0]);
            rooms.RemoveAt(0);
            Debug.Log("Start room index: " + mapList[0].roomIndex);

            mapList[0].roomAnchor = new int2(0, 0);
            mapList[0].baseEdge = 3;
            int matrixX = mapMatrix.GetLength(0) / 2;
            int matrixY = mapMatrix.GetLength(1) / 2;
            mapMatrix[matrixX, matrixY] = true;
        }

        // Old one
        //private void BuildConnections(RoomBase room, int mapLength = 5)
        //{
        //    RoomBase nextRoom;
        //    int randSide;
        //    int side;
        //    int x = 0;
        //    int y = 0;

        //    if (mapList.Count < mapLength && rooms.Count != 0)
        //    {
        //        randSide = UnityEngine.Random.Range(0, 7) != 7 ? 0 : 1;
        //        if (horizontalSideIndexes[randSide] != room.baseEdge)
        //        {
        //            switch (randSide)
        //            {
        //                case 0:
        //                    x = room.NormX + 1;
        //                    y = room.NormY;
        //                    break;
        //                case 1:
        //                    x = room.NormX - 1;
        //                    y = room.NormY;
        //                    break;
        //            }
        //            if (x >= 0 && x < mapMatrix.GetLength(0) && !mapMatrix[x, y])
        //            {
        //                side = horizontalSideIndexes[randSide];
        //                nextRoom = GetNextRoom(room.edgeWeight[side], side);
        //                if (nextRoom != null)
        //                {
        //                    room.AddConnection(nextRoom, side);
        //                    mapMatrix[x, y] = true;
        //                }
        //            }
        //        }

        //        randSide = UnityEngine.Random.Range(0, 5) != 4 ? 1 : 0;
        //        if (verticalSideIndexes[randSide] != room.baseEdge)
        //        {
        //            switch (randSide)
        //            {
        //                case 0:
        //                    x = room.NormX;
        //                    y = room.NormY + 1;
        //                    break;
        //                case 1:
        //                    x = room.NormX;
        //                    y = room.NormY - 1;
        //                    break;
        //            }
        //            if (y >= 0 && y < mapMatrix.GetLength(1) && !mapMatrix[x, y])
        //            {
        //                side = verticalSideIndexes[randSide];
        //                nextRoom = GetNextRoom(room.edgeWeight[side], side);
        //                if (nextRoom != null)
        //                {
        //                    room.AddConnection(nextRoom, side);
        //                    mapMatrix[x, y] = true;
        //                }
        //            }
        //        }
        //        foreach (RoomBase rb in room.nextRooms)
        //        {
        //            if (rb != null)
        //                BuildConnections(rb, mapLength);
        //        }
        //    }
        //}

        // New one
        private void BuildConnections(RoomBase room, int mapLength)
        {
            RoomBase nextRoom;
            //int side;

            int x = 0;
            int y = 0;

            for (int i = 0; i < 4; i++)
            {
                if (i != room.baseEdge && mapList.Count < mapLength)
                {
                    // fix that switch!!!
                    switch (i)
                    {
                        case 0:
                            x = room.NormX;
                            y = room.NormY + 1;
                            if (room.NormY == mapMatrix.GetLength(1) || mapMatrix[x, y])
                                continue;
                            break;
                        case 1:
                            x = room.NormX + 1;
                            y = room.NormY;
                            if (room.NormY == mapMatrix.GetLength(0) || mapMatrix[x, y])
                                continue;
                            break;
                        case 2:
                            x = room.NormX;
                            y = room.NormY - 1;
                            if (room.NormY == 0 || mapMatrix[x, y])
                                continue;
                            break;
                        case 3:
                            x = room.NormX - 1;
                            y = room.NormY;
                            if (room.NormX == 0 || mapMatrix[x, y])
                                continue;
                            break;
                    }
                    nextRoom = GetNextRoom(room.edgeWeight[i], i);
                    if (nextRoom != null)
                    {
                        room.AddConnection(nextRoom, i);
                        mapMatrix[x, y] = true;
                    }
                }
                else if (mapList.Count >= mapLength)
                    break;
            }

            foreach (RoomBase rb in room.nextRooms)
            {
                if (rb != null)
                    BuildConnections(rb, mapLength);
            }
        }

        private RoomBase GetNextRoom(int edgeWeight, int side)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].ConnectedEdgeWeight(side) == edgeWeight)
                {
                    nextVar.Add(rooms[i]);
                }
            }

            if (nextVar.Count > 0)
            {
                RoomBase nextRoom = nextVar[UnityEngine.Random.Range(0, nextVar.Count)];
                nextVar.Clear();
                return nextRoom;
            }
            else return null;
        }

        private void LoadRoomsForScene(string sceneName)
        {
            rooms = new List<RoomBase>();

            DirectoryInfo dir = new DirectoryInfo("Assets/Resources/MapFiles/" + sceneName);
            FileInfo[] fileInfos = dir.GetFiles("*.json");
            foreach (FileInfo f in fileInfos)
            {
                rooms.Add(CreateRoomWithProperties(sceneName, Path.GetFileNameWithoutExtension(f.Name)));
            }
        }

        private RoomBase CreateRoomWithProperties(string sceneName, string filename)
        {
            string pathToFile = "MapFiles/" + sceneName + "/" + filename;
            string mapPropertiesJson = Resources.Load<TextAsset>(pathToFile).ToString();
            JObject mapProperties = JObject.Parse(mapPropertiesJson);

            int width = (int)mapProperties["width"];
            int height = (int)mapProperties["height"];

            IList layers = mapProperties["layers"].Children().ToList();

            int[] weights = new int[] {
                (int)mapProperties["properties"][3]["value"],
                (int)mapProperties["properties"][2]["value"],
                (int)mapProperties["properties"][0]["value"],
                (int)mapProperties["properties"][1]["value"]
            };

            RoomBase r = new RoomBase(this, 0, width, height, weights);

            foreach (JToken layer in layers)
            {
                r.AddLayer((string)layer["data"], (string)layer["compression"], (string)layer["encoding"]);
            }

            return r;
        }

        private void FillTilemap(string sceneName)
        {
            TileMapFiller filler = new TileMapFiller(sceneName);
            ObjectScatterer scatterer = GameObject.Find("Scatterer").GetComponent<ObjectScatterer>();
            int[] layerMap;
            foreach (RoomBase rb in mapList)
            {
                // Draw multilayer map:
                //     zero layer - ground,
                //     first layer - water (otional),
                //     second layer - ladders and jump-through platforms (optional),
                //     last layer - interactive objects (i.e traps, exit-enter, items, monsters)
                // NOTE: for now there are ALWAYS must be 3 or more layers, even if one of them is empty
                for (int i = 0; i < rb.NumberOfLayers; i++)
                {
                    if (i < 3)
                    {
                        layerMap = rb.GetDecompressedMap(i);
                        filler.DrawRoomOnGrid(layerMap, rb.sizeX, rb.sizeY, rb.roomAnchor, i);

                        if (i == 0)
                        {
                            RoomBase botNeighbour = rb.GetBottomNeighbour();
                            rb.navGraph = navGraphGenerator.GenerateRoomNavGraph(layerMap, rb.sizeX, rb.sizeY, rb.roomAnchor, botNeighbour?.GetDecompressedMap(0));
                        }
                        else if (i == 2)
                            rb.navGraph.ModifyRoomNavGraph(layerMap);
                    }
                    else if (CheckMapPassability())
                        // There must be map passability check before adding objects on scene
                        scatterer.AddObjectsOnScene(rb.GetDecompressedMap(i), rb.sizeX, rb.sizeY, rb.roomAnchor);
                    else
                        filler.ClearTilemap();
                }
            }

            //navGraphGenerator.ConnectRoomNavGraphs();
        }

        private void BuildNavGraph()
        {
            TileMapFiller grapthDrawer = new TileMapFiller();
            foreach(RoomBase rb in mapList)
            {
                rb.ConnectNavGraphToNeighbour();
                rb.navGraph.RecalcNodesCoordsToWorld();

                foreach(PathNode node in rb.navGraph.nodes.Values)
                {
                    navigationGraph.Add(node.GetHashCode(), node);
                    grapthDrawer.DrawGraphNodes(node);
                }
            }
        }

        private bool CheckMapPassability()
        {
            return true;
        }
    }
}
