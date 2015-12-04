using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpacePew.Common
{
	public enum UdpNetworkPacketType
	{
		PlayerJoining,
		PlayerJoined,
		PlayerDisconnecting,
		PlayerDisconnected,
		PlayerUpdate,
		PlayerDying,
		PlayerDied,
		MessageSent,
		MessageReceived,
		EntityCreated,
		EntitiesCreated,
		RequestingScoreboard,
		SendingScoreBoard,
		LevelRequest,
		LevelResponse,
		RegisterHost,
		RequestHostList,
		RequestIntroduction
	}
}
