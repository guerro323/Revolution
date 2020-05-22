namespace RevolutionSnapshot.Core.ECS
{
	public partial class RevolutionWorld
	{
		/// <summary>
		/// Clone this world.
		/// </summary>
		/// <returns>The cloned world.</returns>
		public RevolutionWorld Clone()
		{
			var cloned = new RevolutionWorld(entityInitialCapacity: this.entityToChunk.Count);
			CopyToAndIgnoreDuplicate(cloned);
			return cloned;
		}

		/// <summary>
		/// Copy entities and components of another world into a destination. Entities that already exist in destination will be ignored.
		/// </summary>
		/// <param name="destination"></param>
		public void CopyToAndIgnoreDuplicate(RevolutionWorld destination)
		{
			foreach (var chunk in Chunks)
			{
				var clonedChunk   = destination.GetOrCreateChunk(chunk.ComponentTypes);
				var chunkEntities = chunk.Span;
				for (var i = 0; i < chunkEntities.Length; i++)
				{
					// todo: copy identity
					var ent = destination.CreateEntityInChunk(clonedChunk);
				}

				foreach (var (type, componentArray) in chunk.Components)
				{
					var clonedComponentArray = clonedChunk.Components[type];
					componentArray.CopyTo(clonedComponentArray);
				}
			}
		}
	}
}