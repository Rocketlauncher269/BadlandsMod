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
	}
}