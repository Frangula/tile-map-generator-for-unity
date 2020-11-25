using System.Collections.Generic;
using UnityEngine;

namespace TileMapGenerator.NavigationGraphGenerator
{
    public class PathNode
    {
        public enum NodeType { LeftEdge, RightEdge, Platform, Ladder }
        public int xPos;
        public int yPos;
        //public List<NodeConnection> connections = new List<NodeConnection>();
        public Dictionary<int, NodeConnection> nodeConnections;
        public List<JumpTrajectory> jumpTrajectoiries;

        public NodeType Type { get; set; }

        public PathNode(int x, int y, NodeType type = NodeType.Platform)
        {
            xPos = x;
            yPos = y;
            Type = type;

            nodeConnections = new Dictionary<int, NodeConnection>();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(PathNode))
                return false;
            else
            {
                PathNode pn = (PathNode)obj;
                return xPos == pn.xPos && yPos == pn.yPos;
            }
        }

        public override int GetHashCode()
        {
            int x = xPos >= 0 ? 2 * xPos : -2 * xPos - 1;
            int y = yPos >= 0 ? 2 * yPos : -2 * yPos - 1;
            return x >= y ? x * x + x + y : x + y * y;
        }

        public void AddJumpTrajectories(int heightDiv, int speedDiv, int unitMaxJumpHeight, int unitMaxJumpSpeed)
        {
            float gravity = 4.5f;
            jumpTrajectoiries = new List<JumpTrajectory>();
            for (int i = 2; i <= heightDiv; i++)
            {
                int jumpHeight = unitMaxJumpHeight * i / heightDiv;
                for (int j = 1; j <= speedDiv; j++)
                {
                    int jumpSpeed = unitMaxJumpSpeed * j / speedDiv;
                    if (jumpSpeed < Mathf.Sqrt(2 * jumpHeight * gravity))
                    {
                        continue;
                    }
                    else
                    {
                        jumpTrajectoiries.Add(new JumpTrajectory(jumpHeight, jumpSpeed, xPos, yPos, gravity));
                        jumpTrajectoiries.Add(new JumpTrajectory(jumpHeight, -jumpSpeed, xPos, yPos, gravity));
                    }

                    if (Type == NodeType.Ladder) break;
                }
            }
        }

        public void AddConnection(PathNode next, int roomCoordHash, int moveType)
        {
            //connections.Add(new NodeConnection(next, moveType));
            if (!nodeConnections.ContainsKey(next.GetHashCode() + roomCoordHash))
            {
                nodeConnections.Add(next.GetHashCode() + roomCoordHash, new NodeConnection(next, moveType));
                //Debug.Log("Adding connection");
            }
        }
    }
}
