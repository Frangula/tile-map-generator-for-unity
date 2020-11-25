using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Linq;
using Unity.Mathematics;
using System;

namespace TileMapGenerator.MapGenerator
{
    public class TileMapFiller
    {
        private List<Tile> tileAsset = new List<Tile>();
        private List<string> filenames = new List<string>();
        private List<Tilemap> tileMapLayers;

        public int mapSizeX;
        public int mapSizeY;

        public TileMapFiller(string sceneName)
        {
            LoadSceneTiles(sceneName);
            tileMapLayers = new List<Tilemap>()
        {
            GameObject.Find("Ground").GetComponent<Tilemap>(),
            GameObject.Find("Water").GetComponent<Tilemap>(),
            GameObject.Find("InteractableObjects").GetComponent<Tilemap>()
        };
        }

        private string PadNumbers(string input)
        {
            return Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(3, '0'));
        }

        private void LoadAssetNames(string sceneName, string assetType)
        {
            if (filenames.Count > 0)
                filenames.Clear();

            string assetListJson = Resources.Load<TextAsset>("AssetsFileNames").ToString();
            JObject assetList = JObject.Parse(assetListJson);

            IList assetTypes = assetList["childs"].Children().ToList();
            foreach (JToken type in assetTypes)
            {
                if (type["name"].ToString() == assetType)
                {
                    IList tilesByScene = type["childs"].Children().ToList();
                    foreach (JToken scene in tilesByScene)
                    {
                        if (scene["name"].ToString() == sceneName)
                        {
                            filenames = scene["filenames"].Values<string>().ToList();
                            return;
                        }
                    }
                }
            }
        }

        private void LoadSceneTiles(string sceneName)
        {
            LoadAssetNames(sceneName, "Tiles");
            for (int i = 0; i < filenames.Count; i++)
            {
                tileAsset.Add(Resources.Load<Tile>("Tiles/" + sceneName + "/" + filenames[i]));
            }
            tileAsset.Sort((x, y) => PadNumbers(x.name).CompareTo(PadNumbers(y.name)));
        }

        private void LoadObjects(string sceneName)
        {
            LoadAssetNames(sceneName, "Prefabs");
        }

        public void DrawRoomOnGrid(int[] roomMap, int sizeX, int sizeY, int2 roomOrigin, int layer)
        {
            // The room starts to draw from the lower left corner
            // roomMap - array of tile indices
            //Array.Reverse(roomMap);
            int tileIndex;
            int worldX = sizeY - 1;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = sizeX - 1; x >= 0; x--)
                {
                    tileIndex = roomMap[x + y * sizeX] - 1;
                    if (tileIndex > -1)
                    {
                        tileMapLayers[layer].SetTile(new Vector3Int(x + roomOrigin.x, worldX + roomOrigin.y, 0), tileAsset[tileIndex]);
                        //Debug.Log("Set at " + x + ", " + y + " tile" + tileAsset[tileIndex].sprite.name);
                    }
                }
                worldX--;
            }
        }

        public void ClearTilemap()
        {
            foreach (Tilemap tilemap in tileMapLayers)
            {
                tilemap.ClearAllTiles();
            }
        }
    }
}
