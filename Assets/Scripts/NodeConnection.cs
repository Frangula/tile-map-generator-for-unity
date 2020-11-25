using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileMapGenerator.NavigationGraphGenerator
{
    public class NodeConnection
    {
        public enum MoveType { Walk, Jump, Fall };
        public PathNode nextNode;
        public MoveType moveType;

        public NodeConnection(PathNode node, int type)
        {
            nextNode = node;
            moveType = (MoveType)type;
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj == null || obj.GetType() != typeof(NodeConnection))
        //        return false;
        //    else
        //    {
        //        NodeConnection nc = (NodeConnection)obj;
        //        return nextNode.Equals(nc.nextNode) && moveType == nc.moveType;
        //    }
        //}

        //public override int GetHashCode()
        //{
        //    return ((nextNode.xPos + nextNode.yPos) * (nextNode.xPos + nextNode.yPos + 1) / 2) + nextNode.yPos;
        //}
    }
}
