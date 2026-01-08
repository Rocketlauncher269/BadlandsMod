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
        //Generation values
        public static int PlaceX;
        public static int PlaceY;
        public static int BiomeWidth;
        public static int BiomeHeightLimit;

        private void BadlandGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Friend Inside Me";

            //Biome width and height
            BiomeWidth = Main.maxTilesX >= 8400 ? 289 : Main.maxTilesX >= 6400 ? 231 : 185;
			BiomeHeightLimit = (int)Main.worldSurface;

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

            PlaceY = (int)Main.worldSurface - (Main.maxTilesY / 8);

            //Tile replacement loop
            for (int x = PlaceX - BiomeWidth; x <= PlaceX + BiomeWidth; x++)
            {
                for (int y = PlaceY; y < BiomeHeightLimit; y++)
                {
                    if (WorldgenTools.NoFloatingIslands(x, y, 45))
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

        public void BadlandFlattening(GenerationProgress progress, GameConfiguration configuration)
        {
            int startX = PlaceX - BiomeWidth;
            int endX = PlaceX + BiomeWidth;
            int middleX = PlaceX;

            //Ground points
            int leftY = 0;
            int rightY = 0;

            //Search values
            bool foundGroundLeft = false;
            int attemptsLeft = 0;

            //Find values
            while (!foundGroundLeft && attemptsLeft++ < 100000)
			{
				if (Main.tile[startX, leftY].TileType != TileID.Sandstone || !WorldgenTools.NoFloatingIslands(startX, leftY, 45) && leftY < Main.maxTilesY)
				{
					leftY++;
				}

				if ((WorldGen.SolidTile(startX, leftY) || Main.tile[startX, leftY].WallType > WallID.None) && WorldgenTools.NoFloatingIslands(startX, leftY, 45))
				{
					foundGroundLeft = true;
				}
			}

            bool foundGroundRight = false;
            attemptsLeft = 0;

            while (!foundGroundRight && attemptsLeft++ < 100000)
			{
				if (Main.tile[endX, rightY].TileType != TileID.Sandstone || !WorldgenTools.NoFloatingIslands(endX, rightY, 45) && rightY < Main.maxTilesY)
				{
					rightY++;
				}

				if ((WorldGen.SolidTile(endX, rightY) || Main.tile[endX, rightY].WallType > WallID.None) && WorldgenTools.NoFloatingIslands(endX, rightY, 45))
				{
					foundGroundRight = true;
				}
			}

            int middleY = leftY > rightY ? leftY : rightY;

			if (Math.Abs(leftY - rightY) > 100)
			{
				middleY -= (Math.Abs(leftY - rightY) / 2) + 25;
			}

            //Connect points
            ConnectPoints(new Vector2(startX, leftY), new Vector2(middleX, middleY), new Vector2(endX, rightY));
        }

        public static void ConnectPoints(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            int segments = 10000;

            //Place the base curve
            for(int i = 0; i < segments; i++)
            {
                float t = i / (float)segments;
                Vector2 position = BezierCurve.CuadraticBezier(t, p0, p1, p2);

                //Place tiles
                for (int y = (int)position.Y; y < (int)Main.worldSurface; y++)
                {
                    Tile tile = Main.tile[(int)position.X, y];

					if (!tile.HasTile)
					{
						WorldGen.PlaceTile((int)position.X, y, TileID.Sandstone, true);
                        WorldGen.PlaceWall((int)position.X, y, WallID.Sandstone, true);
					}
                }

                //Clear tiles above the line
				int heightLimit = (int)(Main.worldSurface * 0.35f);

				for (int y = heightLimit; y <= (int)position.Y; y++)
				{
					if(y != (int)position.Y)
                    {
                        if (Main.tile[(int)position.X, y].TileType == TileID.Sandstone || Main.tile[(int)position.X, y].WallType == WallID.Sandstone)
                        {
                            Main.tile[(int)position.X, y].ClearEverything();
                        }
                    }
                    else
                    {
                        if(Main.tile[(int)position.X, y].WallType != WallID.None)
                        {
                            WorldGen.KillWall((int)position.X, y);
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
			}
		}
    }
}