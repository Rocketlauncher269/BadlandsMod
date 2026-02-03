using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.ModLoader;

namespace Badlands.Content.Generation
{
	public class WorldgenTools
	{
		public static bool NoFloatingIslands(int X, int Y, int area)
		{
			for (int i = X - area; i < X + area; i++)
			{
				for (int j = Y - area; j < Y + area; j++)
				{
					if (WorldGen.InWorld(i, j))
					{
						if (Main.tile[i, j].TileType == TileID.Cloud || Main.tile[i, j].TileType == TileID.RainCloud || Main.tile[i, j].TileType == TileID.Sunplate)
						{
							return false;
						}
					}
				}
			}

			return true;
		}

		static public int MooreTiles(int x, int y)
        {
            int count = 0;

            for (int nebX = x - 1; nebX <= x + 1; nebX++)
            {
                for (int nebY = y - 1; nebY <= y + 1; nebY++)
                {
                    if (nebX != x || nebY != y)
                    {
                        if (Main.tile[nebX, nebY].HasTile)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

		static public int NeumanTiles(int x, int y)
        {
            int count = 0;

            if (Main.tile[x, y - 1].HasTile) count++;
			if (Main.tile[x - 1, y].HasTile) count++;
			if (Main.tile[x + 1, y].HasTile) count++;
			if (Main.tile[x, y + 1].HasTile) count++;

            return count;
        }

		static public int CheckTiles(int x, int y)
        {
            int count = 0;

            for (int nebX = x - 1; nebX <= x + 1; nebX++)
            {
                for (int nebY = y - 1; nebY <= y + 1; nebY++)
                {
                    if (nebX != x || nebY != y)
                    {
                        if (Main.tile[nebX, nebY].HasTile)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

		static public bool InsideEllipse(int x, int y, int h, int k, int a, int b)
        {
            double powXH = Math.Pow((x-h), 2);
            double powA = Math.Pow(a, 2);
            double powYK = Math.Pow((y-k), 2);
            double powB = Math.Pow(b, 2);

            double sum1 = powXH / powA;
            double sum2 = powYK / powB;

            double total = sum1 + sum2;

            return total <= 1;
        }

		#region "Perlin noise simple"
		private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);

		private static float Lerp(float a, float b, float t) => a + t * (b - a);

		private static float Grad(int p) => (p & 1) == 0 ? 1f : -1f;

		private static uint StableHash(int x)
		{
			uint u = (uint)x;
			u = ((u >> 16) ^ u) * 0x45d9f3b;
			u = ((u >> 16) ^ u) * 0x45d9f3b;
			u = (u >> 16) ^ u;
			return u;
		}

		private static float Perlin1D(float x, int seed)
		{
			int xi = (int)MathF.Floor(x);
			float xf = x - xi;

			int h0 = (int)StableHash(xi + seed);
			int h1 = (int)StableHash(xi + 1 + seed);

			float d0 = Grad(h0) * xf;
			float d1 = Grad(h1) * (xf - 1);

			float u = Fade(xf);

			return Lerp(d0, d1, u);
		}

		public static float Perlin(float x, int seed, int octaves = 4, float persistence = 0.5f)
		{
			float total = 0f;
			float frequency = 1f;
			float amplitude = 1f;
			float maxValue = 0f;

			for (int i = 0; i < octaves; i++)
			{
				total += Perlin1D(x * frequency, seed + i) * amplitude;
				maxValue += amplitude;

				amplitude *= persistence;
				frequency *= 2f;
			}

			return (total / maxValue) * 0.5f + 0.5f;
		}
		#endregion

		#region "Perlin noise 2D"
		internal static readonly List<Vector2> Directions = new List<Vector2>()
		{
			new Vector2(-1f, -1f),
			new Vector2(1f, -1f),
			new Vector2(-1f, 1f),
			new Vector2(1f, 1f),
			new Vector2(0f, -1f),
			new Vector2(-1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 0f),
		};
		
		public static float PerlinNoise2D(float x, float y, int octaves, int seed)
		{
			float SmoothFunction(float n) => 3f * n * n - 2f * n * n * n;

			float NoiseGradient(int s, int noiseX, int noiseY, float xd, float yd)
			{
				int hash = s;
				hash ^= 1619 * noiseX;
				hash ^= 31337 * noiseY;

				hash = hash * hash * hash * 60493;
				hash = (hash >> 13) ^ hash;

				Vector2 g = Directions[hash & 7];

				return xd * g.X + yd * g.Y;
			}

			int frequency = (int)Math.Pow(2D, octaves);
			x *= frequency;
			y *= frequency;

			int flooredX = (int)x;
			int flooredY = (int)y;
			int ceilingX = flooredX + 1;
			int ceilingY = flooredY + 1;
			float interpolatedX = x - flooredX;
			float interpolatedY = y - flooredY;
			float interpolatedX2 = interpolatedX - 1;
			float interpolatedY2 = interpolatedY - 1;

			float fadeX = SmoothFunction(interpolatedX);
			float fadeY = SmoothFunction(interpolatedY);

			float smoothX = MathHelper.Lerp(NoiseGradient(seed, flooredX, flooredY, interpolatedX, interpolatedY), NoiseGradient(seed, ceilingX, flooredY, interpolatedX2, interpolatedY), fadeX);
			float smoothY = MathHelper.Lerp(NoiseGradient(seed, flooredX, ceilingY, interpolatedX, interpolatedY2), NoiseGradient(seed, ceilingX, ceilingY, interpolatedX2, interpolatedY2), fadeX);

			return MathHelper.Lerp(smoothX, smoothY, fadeY);
		}
		#endregion
	}
}