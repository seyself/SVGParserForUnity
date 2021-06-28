using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace seyself
{
	public class ColorUtil
	{
		public static Color ChangeAlpha(Color color, float alpha)
		{
			color.a = alpha;
			return color;
		}

		public static Color ChangeGray(Color color, float value)
		{
			color.r = value;
			color.g = value;
			color.b = value;
			return color;
		}

		public static Color ChangeRGB(Color color, float r, float g, float b)
		{
			color.r = r;
			color.g = g;
			color.b = b;
			return color;
		}

		public static Color GrayScale(Color color)
		{
			float r = color.r * 0.3f;
			float g = color.g * 0.59f;
			float b = color.b * 0.11f;
			float gray = r + g + b;
			return new Color(gray, gray, gray, color.a);
		}

		public static Color FromHex(uint rgb)
		{
			float r = (float)(rgb >> 16 & 255) / 255f;
			float g = (float)(rgb >>  8 & 255) / 255f;
			float b = (float)(rgb >>  0 & 255) / 255f;
			return new Color(r, g, b, 1);
		}
	}
}
