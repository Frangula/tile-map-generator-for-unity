using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace TileMapGenerator.NavigationGraphGenerator
{
    public class JumpTrajectory
    {
        public List<int2> tPointCoords;
        //private float jumpDuration = 4f;
        //private float timeBPoits = .1f;
        //private float accelOGravity = 1.5f;

        public JumpTrajectory(int jumpHeight, int jumpSpeed, int startX, int startY, float gravity, float jumpDuration = 4f, float timeBPoits = .1f)
        {
            tPointCoords = new List<int2>();
            float jumpTime = 0f;
            float ySpeed = Mathf.Sqrt(jumpHeight * gravity * 2);

            while (jumpTime <= jumpDuration)
            {
                int x = Mathf.FloorToInt(jumpSpeed * jumpTime);
                int y = Mathf.FloorToInt(ySpeed * jumpTime - gravity * Mathf.Pow(jumpTime, 2) / 2);
                if (tPointCoords.Exists(el => el.x == x + startX && el.y == y + startY))
                {
                    jumpTime += timeBPoits;
                    continue;
                }
                else
                    tPointCoords.Add(new int2(x + startX, y + startY));
                jumpTime += timeBPoits;
            }
        }
    }
}
