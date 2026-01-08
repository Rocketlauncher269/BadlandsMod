using Microsoft.Xna.Framework;

namespace Badlands.Common
{
	public class BezierCurve
	{
		public static Vector2 CuadraticBezier(float t, Vector2 p0, Vector2 p1, Vector2 p2)
		{
			float a = 1 - t;
			float aa = a * a;
            float tt = t * t;
			
            Vector2 p = aa * p0;
            p += 2 * a * t * p1;
            p += tt * p2;
            return p;
		}
		
		public static Vector2 CubicBezier(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			float a = 1 - t;
			float aa = a * a;
			float aaa = aa * a;
            float tt = t * t;
			float ttt = tt * t;

            Vector2 p = aaa * p0;
            p += 3 * aa * t * p1;
			p += 3 * a * tt * p2;
			p += ttt * p3;
            return p;
		}
	}
}