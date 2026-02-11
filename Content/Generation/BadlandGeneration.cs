using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Terraria.GameContent.Generation;
using static Terraria.WorldGen;
using static tModPorter.ProgressUpdate;

using Badlands.Common;

namespace Badlands.Content.Generation
{
    public class BadlandGeneration : ModSystem
    {
        //TESTING
        //Generation values
        static int PlaceX;
        static int PlaceY;
        static int BiomeWidth;
        static int BiomeHeightLimit;

        private void BadlandGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Friend Inside Me";

            //Biome width and height
            BiomeWidth = Main.maxTilesX >= 8400 ? 289 : Main.maxTilesX >= 6400 ? 231 : 185;
			BiomeHeightLimit = Main.maxTilesY / 2;

            //Get a good area to place
            int minX;
            int maxX;

            if (GenVars.dungeonSide == -1) //On the left
			{
				int dungeonToSnow = GenVars.snowOriginLeft - GenVars.dungeonX;
                int snowToCenter = (Main.maxTilesX / 2) - GenVars.snowOriginRight;

                if (dungeonToSnow > snowToCenter)
                {
                    minX = GenVars.dungeonX + BiomeWidth + 105;
                    maxX = GenVars.snowOriginLeft - BiomeWidth - 45;

                    PlaceX = minX < maxX ? WorldGen.genRand.Next(minX, maxX) : minX;
                }
                else
                {
                    minX = GenVars.snowOriginRight + BiomeWidth + 45;
                    maxX = (Main.maxTilesX / 2) - BiomeWidth - 105;

                    PlaceX = minX < maxX ? WorldGen.genRand.Next(minX, maxX) : maxX;
                }
			}
			else //On the right
			{
				int centerToSnow = GenVars.snowOriginLeft - (Main.maxTilesX / 2);
                int snowToDungeon = GenVars.dungeonX - GenVars.snowOriginRight;

                if (centerToSnow > snowToDungeon)
                {
                    minX = (Main.maxTilesX / 2) + BiomeWidth + 105;
                    maxX = GenVars.snowOriginLeft - BiomeWidth - 45;

                    PlaceX = minX < maxX ? WorldGen.genRand.Next(minX, maxX) : minX;
                }
                else
                {
                    minX = GenVars.snowOriginRight + BiomeWidth + 45;
                    maxX = GenVars.dungeonX - BiomeWidth - 105;

                    PlaceX = minX < maxX ? WorldGen.genRand.Next(minX, maxX) : maxX;
                }
			}

            int heightLeft = FindGround(PlaceX - BiomeWidth) - 10;
            int heightRight = FindGround(PlaceX + BiomeWidth) - 10;

            if (heightLeft > (Main.worldSurface - 35) && heightRight > (Main.worldSurface - 35))
            {
                PlaceY = (int)Main.worldSurface - 45;
            }
            else
            {
                PlaceY = (heightLeft < heightRight) ? heightLeft : heightRight;
            }

            int StartX = PlaceX - BiomeWidth;
			int EndX = PlaceX + BiomeWidth;

            //Tile replacement loop
            for (int x = StartX; x <= EndX; x++)
            {
                //Progress setters
				progress.Set((float)(x - StartX) / (EndX - StartX));

                for (int y = PlaceY; y < BiomeHeightLimit; y++)
                {
                    Tile tile = Main.tile[x, y];

					//Replace tiles
					if (tile.HasTile && tile.TileType != TileID.Cloud && tile.TileType != TileID.RainCloud)
					{
						//Clear if not solid
						if (WorldGen.SolidTile(x, y) && (tile.TileType != TileID.LeafBlock || tile.TileType != TileID.LivingWood))
						{
							tile.TileType = TileID.Sandstone;
						}
						else
						{
							Main.tile[x, y].ClearEverything();
						}
					}

                    //Place tiles on ebonstone and crimstone walls because THEY SUCK
					if (!tile.HasTile && (tile.WallType == WallID.EbonstoneUnsafe || tile.WallType == WallID.CrimstoneUnsafe))
					{
						tile.TileType = TileID.Sandstone;
					}

					//Replace walls
					if (tile.WallType > WallID.None)
					{
						tile.WallType = WallID.Sandstone;
					}

					//Place walls
					if(tile.WallType == WallID.None && y > (int)Main.worldSurface)
                    {
                        WorldGen.PlaceWall(x, y, WallID.Sandstone, true);
                    }
                }
            }

            //Fixed base and hole fill
			for (int X = PlaceX - BiomeWidth; X <= PlaceX + BiomeWidth; X++)
			{
				for (int Y = (int)Main.worldSurface - 35; Y < BiomeHeightLimit; Y++)
				{
					Tile tile = Main.tile[X, Y];

					if (!tile.HasTile)
					{
						WorldGen.PlaceTile(X, Y, TileID.Sandstone, true);
						WorldGen.PlaceWall(X, Y, WallID.Sandstone, true);
					}
				}
			}
        }

        private void BadlandFlattening(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "This is some kind of... Badlands,,";

            int startX = PlaceX - BiomeWidth;
            int endX = PlaceX + BiomeWidth;
            int middleX = PlaceX;

            //Ground points
            int leftY = FindGround(startX);
            int rightY = FindGround(endX);

            int middleY = leftY < rightY ? leftY : rightY;

			if (Math.Abs(leftY - rightY) > 100)
			{
				middleY += (Math.Abs(leftY - rightY) / 2) - 25;
			}

            //Connect points
            ConnectPoints(new Vector2(startX, leftY), new Vector2(middleX, middleY), new Vector2(endX, rightY));
        }

        private void BadlandCaves(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Caving fr fr :fire:";

            int middlePoint = FindGround(PlaceX) + 25;

            //Width and height
            int undergroundWidth = BiomeWidth + 10;
            int undergroundHeight = (int)Math.Abs(middlePoint - (Main.maxTilesY * 0.65f));
            undergroundHeight /= 2;

            //Origin in the Y axis
            int originY = middlePoint + undergroundHeight;

            //Place an oval
            WorldUtils.Gen(new Point(PlaceX, originY), new Shapes.Circle(undergroundWidth, undergroundHeight), Actions.Chain(new GenAction[]
            {
                new Modifiers.Blotches(2, 0.4),
                new Actions.SetTile(TileID.Sandstone, true, true),
                new Actions.ClearWall(true),
            }));

            //Place walls
            WorldUtils.Gen(new Point(PlaceX, originY), new Shapes.Circle(undergroundWidth + 1, undergroundHeight + 1), Actions.Chain(new GenAction[]
			{
                new Modifiers.Blotches(2, 0.4),
				new Actions.PlaceWall(WallID.Sandstone, true),
			}));

            //Cave time
            int seed = WorldGen.genRand.Next();
			int octaves = 5;
			float clearChance = 0.625f;

            for (int X = PlaceX - undergroundWidth - 1; X <= PlaceX + undergroundWidth + 1; X++)
			{
				for (int Y = originY - undergroundHeight + 25; Y < originY + undergroundHeight + 1; Y++)
				{
					if (WorldgenTools.InsideEllipse(X, Y, PlaceX, originY, undergroundWidth + 2, undergroundHeight + 2))
                    {
                        //Perlin noise values
                        //Higher X values for more horizontal caves
                        float horizontalOffsetNoise = WorldgenTools.PerlinNoise2D(X / 225f, Y / 225f, octaves, unchecked(seed + 1)) * 0.01f;
                        float cavePerlinValue = WorldgenTools.PerlinNoise2D(X / 1050f, Y / 350f, octaves, seed) + 0.5f + horizontalOffsetNoise;
                        float cavePerlinValue2 = WorldgenTools.PerlinNoise2D(X / 1050f, Y / 350f, octaves, unchecked(seed - 1)) + 0.5f;
                        float caveNoiseMap = (cavePerlinValue + cavePerlinValue2) * 0.5f;
                        float caveCreationThreshold = horizontalOffsetNoise * 3.5f + 0.20f;

                        //Remove tiles based on the noise
                        if ((caveNoiseMap * caveNoiseMap > caveCreationThreshold) && (WorldGen.genRand.NextFloat() < clearChance))
                        {
                            WorldGen.KillTile(X, Y);
                        }
                    }
				}
			}

            //Smooth the noise
            SmoothNoise(10, PlaceX, originY, undergroundWidth, undergroundHeight);
        }

        public static void ConnectPoints(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            int segments = 10000;

            //Noise config
            float noiseScale = 0.055f;
            float noiseStrength = 8f;
            int seed = WorldGen.genRand.Next();

            //Do not repeat x values if possible
            HashSet<int> processedX = new HashSet<int>();

            //Place the base curve
            for(int i = 0; i < segments; i++)
            {
                float t = i / (float)segments;
                Vector2 position = BezierCurve.CuadraticBezier(t, p0, p1, p2);

                //The x value to use
                int valueX = (int)position.X;

                //Check if it was visited already, add it to the hashSet if not
                if (processedX.Contains(valueX)) continue;
                processedX.Add(valueX);

                //N O I S E
                float noiseOffset = (WorldgenTools.Perlin(valueX * noiseScale, seed, 3, 0.5f) - 0.5f) * noiseStrength;
                int valueY = (int)(position.Y + noiseOffset);

                //Place tiles
                for (int y = valueY; y < (int)Main.worldSurface; y++)
                {
                    if (y < 0 || y >= Main.maxTilesY) continue;

                    Tile tile = Main.tile[valueX, y];

					if (!tile.HasTile)
					{
						WorldGen.PlaceTile(valueX, y, TileID.Sandstone, true);
                        WorldGen.PlaceWall(valueX, y, WallID.Sandstone, true);
					}
                }

                //Clear tiles above the line
				int heightLimit = (int)(Main.worldSurface * 0.35f);

				for (int y = heightLimit; y <= valueY; y++)
				{
					if(y != valueY && WorldgenTools.NoFloatingIslands(valueX, y, 45))
                    {
                        Main.tile[valueX, y].ClearEverything();
                    }
                    else
                    {
                        if(Main.tile[valueX, y].WallType != WallID.None)
                        {
                            WorldGen.KillWall(valueX, y);
                        }
                    }
				}
            }
        }

        //Find ground
		public static int FindGround(int x)
		{
			int y = 0;

			//Search values
			bool foundGround = false;
			int attemptsLeft = 0;

			//Find values
			while (!foundGround && attemptsLeft++ < 100000)
			{
				if (!WorldGen.SolidTile(x, y) || !WorldgenTools.NoFloatingIslands(x, y, 45) && y < Main.maxTilesY)
				{
					y++;
				}

				if ((WorldGen.SolidTile(x, y) || Main.tile[x, y].WallType > WallID.None) && WorldgenTools.NoFloatingIslands(x, y, 45))
				{
					foundGround = true;
				}
			}

			return y;
		}

        static public void SmoothNoise(int loop, int cx, int cy, int w, int h)
        {
            for (int l = 0; l < loop; l++)
            {
                for (int x = cx - w; x <= cx + w; x++)
                {
                    for(int y = cy - h; y <= cy + h; y++)
                    {
                        if (WorldgenTools.InsideEllipse(x, y, cx, cy, w + 3, h + 3))
                        {
                            int tileCount = WorldgenTools.CheckTiles(x, y);

                            if (tileCount > 4)
                            {
                                WorldGen.PlaceTile(x, y, TileID.Sandstone, true);
                            }
                            else if (tileCount < 4)
                            {
                                WorldGen.KillTile(x, y);
                            }
                        }
                    }
                }
            }
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			//Add the biome in the worldgen task
			int BiomesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
			if (BiomesIndex != -1)
			{
				tasks.Insert(BiomesIndex + 1, new PassLegacy("Badland Biome", BadlandGen));
                tasks.Insert(BiomesIndex + 2, new PassLegacy("Badland Flattening", BadlandFlattening));
                tasks.Insert(BiomesIndex + 3, new PassLegacy("Badland Caves", BadlandCaves));
			}
		}
    }
}