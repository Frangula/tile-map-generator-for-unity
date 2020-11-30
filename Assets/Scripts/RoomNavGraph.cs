using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Mathematics;

namespace TileMapGenerator.NavigationGraphGenerator
{
    public class RoomNavGraph
    {
        public Dictionary<int, PathNode> nodes;
        private int[] roomMap;

        // list of nodes on the edge of the room
        public List<PathNode> blindNodes = new List<PathNode>();
        // list of temp nodes on the bottom edge of the room
        private List<PathNode> tempNodes = new List<PathNode>();

        public int unitMaxSpeed = 9;
        public int unitMaxJumpHeight = 4;
        public int heightDivisions = 4;
        public int speedDivisions = 3;

        public int2 RoomOrigin { get; private set; }
        public int RoomSizeY { get; private set; }
        public int RoomSizeX { get; private set; }

        public RoomNavGraph(int[] map, int sizeX, int sizeY, int2 roomOrigin, int[] neighbourMap)
        {
            //roomMap = new int[map.Length];
            RoomSizeX = sizeX;
            RoomSizeY = sizeY;
            RoomOrigin = roomOrigin;

            nodes = new Dictionary<int, PathNode>();
            roomMap = ReshapeMapArray(map);
            if (neighbourMap == null)
                GenerateRoomNavGraph();
            else
                GenerateRoomNavGraph(ReshapeMapArray(neighbourMap));
        }

        private int[] ReshapeMapArray(int[] map)
        {
            int worldY = RoomSizeY - 1;
            int[] tempMap = new int[map.Length];
            //Array.Reverse(map);
            for (int y = 0; y < RoomSizeY; y++)
            {
                for (int x = 0; x <RoomSizeX; x++)
                {
                    tempMap[worldY * RoomSizeX + x] = map[y * RoomSizeX + x];
                }
                worldY--;
            }
            return tempMap;
        }

        public void GenerateRoomNavGraph()
        {
            for (int y = 0; y < RoomSizeY; y++)
            {
                for (int x = 0; x < RoomSizeX; x++)
                {
                    if ((y == 0 && roomMap[x + y * RoomSizeX] == 0) || (roomMap[x + y * RoomSizeX] == 0 && roomMap[x + (y - 1) * RoomSizeX] != 0))
                    {
                        // Consider that we cant jump through platforms
                        // Nodes must have LOCAL SPACE coordinates for proper neighbourhood detection
                        //if (x == RoomSizeX - 1 || roomMap[x + 1 + y * RoomSizeX] != 0 || (y > 0 && roomMap[x + 1 + (y - 1) * RoomSizeX] == 0))
                        //{
                        //    tempNode = new PathNode(x, y, PathNode.NodeType.RightEdge);
                        //    nodes.Add(tempNode.GetHashCode(), tempNode);
                        //    continue;
                        //}
                        //else if (x == 0 || roomMap[x - 1 + y * RoomSizeX] != 0 || (y > 0 && roomMap[x - 1 + (y - 1) * RoomSizeX] == 0))
                        //{
                        //    tempNode = new PathNode(x, y, PathNode.NodeType.LeftEdge);
                        //    nodes.Add(tempNode.GetHashCode(), tempNode);
                        //    continue;
                        //}
                        //else
                        //{
                        //    tempNode = new PathNode(x, y);
                        //    nodes.Add(tempNode.GetHashCode(), tempNode);
                        //}
                        AddNodeToGraph(x, y);
                    }
                }
            }
        }

        public void GenerateRoomNavGraph(int[] neighbourMap)
        {
            for (int y = 0; y < RoomSizeY; y++)
            {
                for (int x = 0; x < RoomSizeX; x++)
                {
                    if (y == 0 && roomMap[x + y * RoomSizeX] == 0 && neighbourMap[x + RoomSizeX * (RoomSizeY - 1)] != 0)
                    {
                        AddNodeToGraph(x, y);
                    }
                    else if (y > 0 && roomMap[x + (y * RoomSizeX)] == 0 && roomMap[x + (y - 1) * RoomSizeX] != 0)
                    {
                        AddNodeToGraph(x, y);
                    }
                }
            }
        }

        private void AddNodeToGraph(int x, int y)
        {
            if (x == RoomSizeX - 1 || roomMap[x + 1 + y * RoomSizeX] != 0 || (y > 0 && roomMap[x + 1 + (y - 1) * RoomSizeX] == 0))
            {
                nodes.Add(GetCoordinatesHash(x, y), new PathNode(x, y, PathNode.NodeType.RightEdge));
                return;
            }
            else if (x == 0 || roomMap[x - 1 + y * RoomSizeX] != 0 || (y > 0 && roomMap[x - 1 + (y - 1) * RoomSizeX] == 0))
            {
                nodes.Add(GetCoordinatesHash(x, y), new PathNode(x, y, PathNode.NodeType.LeftEdge));
                return;
            }
            else
            {
                nodes.Add(GetCoordinatesHash(x, y), new PathNode(x, y));
                return;
            }
        }

        public void ConnectPathNodes()
        {
            foreach (PathNode pathNode in nodes.Values)
            {
                AddWalkConnection(pathNode);
                AddJumpConnections(pathNode);
                if (pathNode.Type == PathNode.NodeType.LeftEdge || pathNode.Type == PathNode.NodeType.RightEdge)
                    AddFallConnection(pathNode);
                if (pathNode.Type == PathNode.NodeType.Ladder)
                    AddAscDescConnection(pathNode);
            }
        }

        public void ConnectBlindNodes(RoomNavGraph neighbourRoom)
        {
            foreach (PathNode localNode in blindNodes)
            {
                foreach (PathNode neighbourNode in neighbourRoom.blindNodes)
                {
                    if (Mathf.Abs((localNode.xPos + RoomOrigin.x) - (neighbourNode.xPos + neighbourRoom.RoomOrigin.x)) == 1
                        && (localNode.yPos + RoomOrigin.y) == (neighbourNode.yPos + neighbourRoom.RoomOrigin.y))
                        AddWalkConnection(localNode, neighbourNode, GetCoordinatesHash(neighbourRoom.RoomOrigin.x, neighbourRoom.RoomOrigin.y));
                    if (Mathf.Abs((localNode.xPos + RoomOrigin.x) - (neighbourNode.xPos + neighbourRoom.RoomOrigin.x)) == 1
                        && ((localNode.yPos + RoomOrigin.y) - (neighbourNode.yPos + neighbourRoom.RoomOrigin.y)) <= unitMaxJumpHeight
                        && ((localNode.yPos + RoomOrigin.y) - (neighbourNode.yPos + neighbourRoom.RoomOrigin.y)) >= 1)
                        AddJumpConnection(localNode, neighbourRoom);
                    if (localNode.Type == PathNode.NodeType.Ladder
                        && neighbourNode.Type == PathNode.NodeType.Ladder
                        && Mathf.Abs((localNode.yPos + RoomOrigin.y) - (neighbourNode.yPos + neighbourRoom.RoomOrigin.y)) == 1)
                        AddAscDescConnection(localNode, neighbourNode, GetCoordinatesHash(neighbourRoom.RoomOrigin.x, neighbourRoom.RoomOrigin.y));
                }
                if (localNode.Type != PathNode.NodeType.Platform && neighbourRoom.RoomOrigin.y <= RoomOrigin.y)
                {
                    AddFallConnection(localNode, neighbourRoom);
                }
            }
        }

        public void ModifyRoomNavGraph(int[] modifyingMap)
        {
            Array.Reverse(modifyingMap);
            if (modifyingMap.Length > 0)
            {
                PathNode tempNode;
                int tempHashCode;
                int worldX;

                for (int y = 0; y < RoomSizeY - 1; y++)
                {
                    worldX = 0;
                    for (int x = RoomSizeX - 1; x > 0; x--)
                    {
                        if (modifyingMap[x + y * RoomSizeX] != 0)
                        {
                            tempNode = new PathNode(worldX, y, PathNode.NodeType.Ladder);
                            tempHashCode = tempNode.GetHashCode();

                            if (nodes.ContainsKey(tempHashCode))
                                nodes[tempHashCode].Type = PathNode.NodeType.Ladder;
                            else
                                nodes.Add(tempHashCode, tempNode);
                        }
                        worldX++;
                    }
                }
            }
            ConnectPathNodes();
        }
        public void RecalcNodesCoordsToWorld()
        {
            foreach (PathNode node in nodes.Values)
            {
                node.xPos += RoomOrigin.x;
                node.yPos += RoomOrigin.y;
            }
        }

        public void CleanUp()
        {
            blindNodes.Clear();
            foreach (PathNode node in nodes.Values)
            {
                node.jumpTrajectoiries.Clear();
            }
        }

        private void AddFallConnection(PathNode node)
        {
            if (node.xPos > 0 && node.xPos < RoomSizeX - 1)
            {
                int offset = 0;
                switch (node.Type)
                {
                    case PathNode.NodeType.LeftEdge:
                        offset = -1;
                        break;
                    case PathNode.NodeType.RightEdge:
                        offset = 1;
                        break;
                }

                if (roomMap[node.yPos * RoomSizeX + (node.xPos + offset)] != 0) return;

                for (int sY = node.yPos - 1; sY >= 0; sY--)
                {
                    if (sY == 0)
                        blindNodes.Add(node);
                    else if (nodes.ContainsKey(GetCoordinatesHash(node.xPos + offset, sY)) && roomMap[(sY - 1) * RoomSizeX + (node.xPos + offset)] != 0)
                    {
                        node.AddConnection(nodes[GetCoordinatesHash(node.xPos + offset, sY)], GetCoordinatesHash(RoomOrigin.x, RoomOrigin.y), 2);
                        Debug.Log("Adding FALL connection from (" + node.xPos + ", " + node.yPos + ") to (" + (node.xPos + offset) + ", " + sY + ").");
                        break;
                    }
                }
            }
        }

        private void AddFallConnection(PathNode localNode, RoomNavGraph neighbourRoom)
        {
            int offsetX = 0;
            int x;
            switch (localNode.Type)
            {
                case PathNode.NodeType.LeftEdge:
                    offsetX = -1;
                    break;
                case PathNode.NodeType.RightEdge:
                    offsetX = 1;
                    break;
            }
            for (int y = RoomOrigin.y > neighbourRoom.RoomOrigin.y ? RoomSizeY - 1 : localNode.yPos - 1; y > -1; y--)
            {
                x = (localNode.xPos + offsetX) + RoomOrigin.x - neighbourRoom.RoomOrigin.x;
                if (x >= 0 && x < RoomSizeX && neighbourRoom.roomMap[y * RoomSizeX + x] != 0) return;
                if (x >= 0 && neighbourRoom.nodes.ContainsKey(GetCoordinatesHash(x, y)) /*&& neighbourRoom.roomMap[(y - 1) * RoomSizeX + x] != 0*/)
                {
                    localNode.AddConnection(neighbourRoom.nodes[GetCoordinatesHash(x, y)], GetCoordinatesHash(neighbourRoom.RoomOrigin.x, neighbourRoom.RoomOrigin.y), 2);
                    Debug.Log("Adding FALL connection from (" + (localNode.xPos + RoomOrigin.x) + ", " + (localNode.yPos + RoomOrigin.y) +
                        ") to (" + (x + neighbourRoom.RoomOrigin.x) + ", " + (y + neighbourRoom.RoomOrigin.y) + ").");
                    return;
                }
            }
        }

        private void AddJumpConnections(PathNode node)
        {
            node.AddJumpTrajectories(heightDivisions, speedDivisions, unitMaxJumpHeight, unitMaxSpeed);
            foreach (JumpTrajectory trajectory in node.jumpTrajectoiries)
            {
                foreach (int2 coordPair in trajectory.tPointCoords)
                {
                    if (coordPair.x > RoomSizeX - 1 || coordPair.y > RoomSizeY - 1 ||
                        coordPair.x < 0 || coordPair.y <= 0 ||
                        roomMap[RoomSizeX * (coordPair.y + 1) - (coordPair.x + 1)] != 0) break;
                    if (coordPair.x != node.xPos && coordPair.y != node.yPos
                        && nodes.ContainsKey(GetCoordinatesHash(coordPair.x, coordPair.y)))
                    {
                        node.AddConnection(nodes[GetCoordinatesHash(coordPair.x, coordPair.y)], GetCoordinatesHash(RoomOrigin.x, RoomOrigin.y), 1);
                        //Debug.Log("Adding JUMP connection from (" + node.xPos + ", " + node.yPos + ") to (" + coordPair.x + ", " + coordPair.y + ").");
                        break;
                    }
                }
            }
        }

        private void AddJumpConnection(PathNode localNode, RoomNavGraph neighbourRoom)
        {
            int neighbourX = 0;
            int neighdourY = 0;

            foreach (JumpTrajectory trajectory in localNode.jumpTrajectoiries)
            {
                foreach (int2 coord in trajectory.tPointCoords)
                {
                    neighbourX = coord.x + RoomOrigin.x - neighbourRoom.RoomOrigin.x;
                    neighdourY = coord.y + RoomOrigin.y - neighbourRoom.RoomOrigin.y;
                    if (neighbourX > RoomSizeX - 1 || neighdourY > RoomSizeY - 1 ||
                        neighbourX < 0 || neighdourY < 0 ||
                        neighbourRoom.roomMap[RoomSizeX * (neighdourY + 1) - (neighbourX + 1)] != 0) break;
                    if (neighbourRoom.nodes.ContainsKey(GetCoordinatesHash(coord.x, coord.y)))
                    {
                        localNode.AddConnection(neighbourRoom.nodes[GetCoordinatesHash(coord.x, coord.y)], GetCoordinatesHash(neighbourRoom.RoomOrigin.x, neighbourRoom.RoomOrigin.y), 1);
                        Debug.Log("Adding JUMP connection from (" + localNode.xPos + ", " + localNode.yPos + ") to (" + coord.x + ", " + coord.y + ").");
                    }
                }
            }
        }

        private void AddWalkConnection(PathNode node)
        {
            if (node.xPos > 0 && nodes.ContainsKey(GetCoordinatesHash(node.xPos - 1, node.yPos)))
            {
                node.AddConnection(nodes[GetCoordinatesHash(node.xPos - 1, node.yPos)], GetCoordinatesHash(RoomOrigin.x, RoomOrigin.y), 0);
                //Debug.Log("Adding WALK connection from (" + node.xPos + ", " + node.yPos + ") to (" + (node.xPos - 1) + ", " + node.yPos + ").");
                //Debug.Log(string.Format("Adding WALK connection from ({0}, {1}) to ({2}, {1})", node.xPos, node.yPos, node.xPos - 1));
            }
            if (node.xPos < RoomSizeX - 1 && nodes.ContainsKey(GetCoordinatesHash(node.xPos + 1, node.yPos)))
            {
                node.AddConnection(nodes[GetCoordinatesHash(node.xPos + 1, node.yPos)], GetCoordinatesHash(RoomOrigin.x, RoomOrigin.y), 0);
                //Debug.Log(string.Format("Adding WALK connection from ({0}, {1}) to ({2}, {1})", node.xPos, node.yPos, node.xPos + 1));
            }
            if (node.xPos == 0 || node.xPos == RoomSizeX - 1)
            {
                blindNodes.Add(node);
            }
        }

        private void AddWalkConnection(PathNode localNode, PathNode neighbourNode, int neighbourRoomHash)
        {
            localNode.AddConnection(neighbourNode, neighbourRoomHash, 0);
            Debug.Log(string.Format("Adding WALK connection from ({0}, {1}) to ({2}, {1})",
                localNode.xPos + RoomOrigin.x, localNode.yPos + RoomOrigin.y, RoomOrigin.x - RoomSizeX + neighbourNode.xPos));
        }

        private void AddAscDescConnection(PathNode node)
        {
            if (node.yPos > 0 && nodes.ContainsKey(GetCoordinatesHash(node.xPos, node.yPos - 1)))
            {
                node.AddConnection(nodes[GetCoordinatesHash(node.xPos, node.yPos - 1)], GetCoordinatesHash(RoomOrigin.x, RoomOrigin.y), 0);
                //Debug.Log(string.Format("Adding DESCENT connection from ({0}, {1}) to ({0}, {2})", node.xPos, node.yPos, node.yPos - 1));
            }
            if (node.yPos < RoomSizeY - 1 && nodes.ContainsKey(GetCoordinatesHash(node.xPos, node.yPos + 1)))
            {
                node.AddConnection(nodes[GetCoordinatesHash(node.xPos, node.yPos + 1)], GetCoordinatesHash(RoomOrigin.x, RoomOrigin.y), 0);
                //Debug.Log(string.Format("Adding ASCENT connection from ({0}, {1}) to ({0}, {2})", node.xPos, node.yPos, node.yPos + 1));
            }
            if (node.yPos == 0 || node.yPos == RoomSizeX - 1)
            {
                blindNodes.Add(node);
            }
        }

        private void AddAscDescConnection(PathNode localNode, PathNode neighbourNode, int neighbourRoomHash)
        {
            localNode.AddConnection(neighbourNode, neighbourRoomHash, 0);
            Debug.Log(string.Format("Adding DESCENT connection from ({0}, {1}) to ({0}, {2})",
                localNode.xPos + RoomOrigin.x, localNode.yPos + RoomOrigin.y, RoomOrigin.y - RoomSizeY + neighbourNode.yPos));
        }

        private int GetCoordinatesHash(int x, int y)
        {
            int a = x >= 0 ? 2 * x : -2 * x - 1;
            int b = y >= 0 ? 2 * y : -2 * y - 1;
            return a >= b ? a * a + a + b : a + b * b;
            //return ((x + y) * (x + y + 1) / 2) + y;
        }
    }
}
