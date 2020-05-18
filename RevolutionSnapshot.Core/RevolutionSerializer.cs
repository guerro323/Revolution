using System;
using System.Collections.Generic;
using RevolutionSnapshot.Core.ECS;

namespace RevolutionSnapshot.Core
{
	public class RevolutionSerializer : IDisposable
	{
		protected internal List<RevolutionClient> Clients;
		
		public RevolutionWorld World;

		public RevolutionSerializer()
		{
			Clients = new List<RevolutionClient>();
			World = new RevolutionWorld(64);
		}

		public void SerializeForAll()
		{
			throw new NotImplementedException("Serialization should follow the old snapshot revolution format");
			
			foreach (var chunk in World.Chunks)
			{
				var entities = chunk.AsArray.AsSpan();
				foreach (ref readonly var entity in entities)
				{
					
				}
			}
		}

		public RevolutionClient CreateClient()
		{
			var client = new RevolutionClient(this);
			Clients.Add(client);
			return client;
		}

		public void Dispose()
		{
			World.Dispose();
		}
	}
}