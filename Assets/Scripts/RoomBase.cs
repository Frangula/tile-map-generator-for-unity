using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
using System.Linq;
using System.IO;
using ZstdNet;
using System.IO.Compression;
using TileMapGenerator.NavigationGraphGenerator;

namespace TileMapGenerator.MapGenerator
{
    public class RoomBase
    {
        public int roomIndex;
        public int sizeX;
        public int sizeY;
        public int baseEdge;
        public int2 roomAnchor;
        public int[] edgeWeight;
        public int[] oppositeEdges = new int[] { 2, 3, 0, 1 };

        public RoomNavGraph navGraph;

        public RoomBase[] nextRooms;

        private List<Layer> layers;
        private MapBuilder parentBase;

        //public enum Edges { Top, Left, Bottom, Right};

        private class Layer
        {
            private int[] decompessedMap;

            public string compressedMap;
            public string compressionType;
            public string encoding;

            public Layer(string map, string compType, string encod)
            {
                compressedMap = map;
                compressionType = compType;
                encoding = encod;
            }

            public int[] Map
            {
                get
                {
                    if (decompessedMap == null)
                    {
                        decompessedMap = DecompressMap();
                        return decompessedMap;
                    }
                    else return decompessedMap;
                }
            }

            public int[] DecompressMap()
            {
                switch (compressionType)
                {
                    case "zstd":
                        return DecompressZSTD(Convert.FromBase64String(compressedMap));
                    case "gzip":
                        return DecompressGZIP(Convert.FromBase64String(compressedMap));
                    case "zlib":
                        return DecompressZLIB(Convert.FromBase64String(compressedMap));
                    case "":
                        return null;
                    default:
                        return new int[] { -1 };
                }
            }

            private int[] DecompressZSTD(byte[] compressedByte)
            {
                Decompressor decompressor = new Decompressor();
                var decompBufferSize = Decompressor.GetDecompressedSize(compressedByte);
                byte[] decompBuffer = decompressor.Unwrap(compressedByte, (int)decompBufferSize);

                return GetMapAsIntArray(decompBuffer);
            }

            private int[] DecompressGZIP(byte[] compressedByte)
            {
                int decompBufferSize = BitConverter.ToInt32(compressedByte, compressedByte.Length - 4);
                byte[] decompBuffer = new byte[decompBufferSize];

                using (var ms = new MemoryStream(compressedByte))
                {
                    using (
                        var gzs = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        gzs.Read(decompBuffer, 0, decompBufferSize);
                    }
                }

                return GetMapAsIntArray(decompBuffer);
            }

            private int[] DecompressZLIB(byte[] compressedByte)
            {
                byte[] decompBuffer;

                using (var msOutput = new MemoryStream())
                {
                    using (var msInput = new MemoryStream(compressedByte, 2, compressedByte.Length - 2))
                    {
                        using (var decompressor = new DeflateStream(msInput, CompressionMode.Decompress))
                        {
                            decompressor.CopyTo(msOutput);
                        }
                        decompBuffer = msOutput.ToArray();
                    }
                }

                return GetMapAsIntArray(decompBuffer);
            }

            private int[] GetMapAsIntArray(byte[] mapByte)
            {
                int arraySize = mapByte.Length / sizeof(int);
                int[] map = new int[arraySize];
                for (int i = 0; i < arraySize; i++)
                {
                    map[i] = BitConverter.ToInt32(mapByte, i * sizeof(int));
                }
                Debug.Log("Decompressed array starts with " + map[0] + ", " + map[1]);
                return map;
            }
        }

        public RoomBase(MapBuilder aBase, int index, int sizeX, int sizeY, int[] weights)
        {
            roomIndex = index;
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            edgeWeight = weights.ToArray();
            layers = new List<Layer>();
            nextRooms = new RoomBase[4];
            parentBase = aBase;
        }

        public int NormX
        {
            get { return (roomAnchor.x - parentBase.mapOrigin.x) / sizeX; }
        }

        public int NormY
        {
            get { return (roomAnchor.y - parentBase.mapOrigin.y) / sizeY; }
        }

        public int NumberOfLayers
        {
            get { return layers.Count; }
        }

        public void AddLayer(string map, string compType, string encoding)
        {
            layers.Add(new Layer(map, compType, encoding));
        }

        public void SetRoomAnchor(int2 parentAnchor, bool horizontal, int offset)
        {
            roomAnchor = horizontal ? parentAnchor + new int2(offset, 0) : parentAnchor + new int2(0, offset);
        }

        public int ConnectedEdgeWeight(int side)
        {
            // Change edges comparation to use enum
            return edgeWeight[oppositeEdges[side]];
        }

        public void AddConnection(RoomBase nextRoom, int side)
        {
            switch (side)
            {
                case 0:
                    //nextTop = nextRoom;
                    //nextVertical = nextRoom;
                    //nextTop.baseEdge = 2;
                    //nextTop.roomAnchor.x = roomAnchor.x;
                    //nextTop.roomAnchor.y = roomAnchor.y + 10;
                    nextRoom.baseEdge = 2;
                    nextRoom.roomAnchor.x = roomAnchor.x;
                    nextRoom.roomAnchor.y = roomAnchor.y + 10;
                    break;
                case 1:
                    nextRoom.baseEdge = 3;
                    nextRoom.roomAnchor.x = roomAnchor.x + 10;
                    nextRoom.roomAnchor.y = roomAnchor.y;
                    break;
                case 2:
                    nextRoom.baseEdge = 0;
                    nextRoom.roomAnchor.x = roomAnchor.x;
                    nextRoom.roomAnchor.y = roomAnchor.y - 10;
                    break;
                case 3:
                    nextRoom.baseEdge = 1;
                    nextRoom.roomAnchor.x = roomAnchor.x - 10;
                    nextRoom.roomAnchor.y = roomAnchor.y;
                    break;
            }
            nextRooms[side] = nextRoom;
            parentBase.mapList.Add(nextRoom);
            parentBase.rooms.Remove(nextRoom);
            Debug.Log("Connect " + roomIndex + " to " + nextRoom.roomIndex + ", sides " + side + " to " + nextRoom.baseEdge);
        }

        public RoomBase GetBottomNeighbour()
        {
            return nextRooms[2];
        }

        public int[] GetDecompressedMap(int layerIndex)
        {
            if (layerIndex < layers.Count)
                return layers[layerIndex].Map;
            else
                return new int[] { -1 };
        }

        public void ConnectNavGraphToNeighbour()
        {
            foreach(RoomBase n in nextRooms)
            {
                if(n != null)
                {
                    navGraph.ConnectBlindNodes(n.navGraph);
                    n.navGraph.ConnectBlindNodes(navGraph);
                }
            }
            navGraph.CleanUp();
        }
    }
}
