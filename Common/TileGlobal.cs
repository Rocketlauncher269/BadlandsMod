using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Badlands.Common
{
	public class TileGlobal : GlobalTile
	{
		public static Vector2 TileOffset => Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
		public static Vector2 TileCustomPosition(int i, int j, Vector2 off = default) => (new Vector2(i, j) * 16) - Main.screenPosition - off + TileOffset;
	}
}