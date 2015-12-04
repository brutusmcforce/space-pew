using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpacePew.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpacePew.Networking
{
	public class NetworkMessage
	{
		// TODO: Add SoundAssetName to make NetworkMessenger play custom sounds upon different messages
		public string Message { get; set; }
		public DateTime Sent { get; set; }
		public Color Color { get; set; }
		public bool IsChatMessage { get; set; }
	}
}
