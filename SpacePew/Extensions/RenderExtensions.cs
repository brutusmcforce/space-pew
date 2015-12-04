using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpacePew.Models;

namespace SpacePew.Extensions
{
	public static class RenderExtensions
	{

		public static Vector2 AsVector2D(this Point p)
		{
			return new Vector2(p.X, p.Y);
		}

		public static void Draw(this SpriteBatch batch, TiledTexture texture, Rectangle destination, Color color)
		{
			Draw(batch, texture, destination, new Rectangle(0, 0, texture.Width, texture.Height), color);
		}

		public static void Draw(this SpriteBatch batch, TiledTexture texture, Rectangle dstRect, Rectangle srcRect, Color color)
		{
			//TODO: kolla om dom ens syns innan man renderar.. hur man nu ska kunna göra det..
			var wratio = dstRect.Width / (double)srcRect.Width;
			var hratio = dstRect.Height / (double)srcRect.Height;
			var pos = new Point(dstRect.X, dstRect.Y);
			dstRect = new Rectangle(0, 0, (int)(wratio * texture.TileWidth), (int)(hratio * texture.TileHeight));

			for (var j = 0; j < texture.XTiles; j++)
			{
				for (var i = 0; i < texture.YTiles; i++)
				{
					//if srcRect.Intersects tile
					var tile = texture[i * texture.XTiles + j];
					dstRect.X = pos.X + (int)(tile.Position.X * wratio);
					dstRect.Y = pos.Y + (int)(tile.Position.Y * hratio);

					batch.Draw(tile.Texture, dstRect, color);
				}
			}
		}
	}
}
