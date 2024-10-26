using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class NullCullingManager : MonoBehaviour
	{
		public void CheckAllChunks()
		{
			for (int i = 0; i < rendererPairs.Count; i++)
			{
				//Debug.Log($"Updating chunk group ({i})");
				rendererPairs[i].UpdateChunk();
			}
		}
		
		public void AddRendererToCell(Cell cell, Renderer newRend)
		{
			int idx = rendererPairs.FindIndex(x => x.cells.Contains(cell) || x.Renderers.Contains(newRend));
			if (idx == -1)
			{
				rendererPairs.Add(new() { cells = [cell], Renderers = [newRend] });
			}
			else
			{
				rendererPairs[idx].Renderers.Add(newRend);
				if (!rendererPairs[idx].cells.Contains(cell))
					rendererPairs[idx].cells.Add(cell);
			}
		}

		internal void ReorganizeRendererPairs()
		{
			List<ChunkGroup> copyPairs = new(rendererPairs);

			rendererPairs.Clear();
			ChunkGroup group = new();
			bool readyToAdd = true;

			for (int i = 0; i < copyPairs.Count; i++)  // Iterates first time to select every other renderer pair in sequence
			{
				readyToAdd = true;
				copyPairs[i].cells.ForEach(cell =>
				{
					if (!group.cells.Exists(x => x.Chunk == cell.Chunk))
					{
						group.cells.Add(cell);
						readyToAdd = false;
					}
				});

				group.Renderers.AddRange(copyPairs[i].Renderers.Except(group.Renderers));

				if (readyToAdd || i == copyPairs.Count - 1)
				{
					readyToAdd = false;
					rendererPairs.Add(group);
					group = new();
				}
			}
		}

		readonly List<ChunkGroup> rendererPairs = [];

		[SerializeField]
		internal CullingManager cullMan;

		public struct ChunkGroup()
		{
			internal List<Cell> cells = [];
			internal List<Renderer> Renderers = [];

			internal readonly void UpdateChunk()
			{
				bool isEnabled = IsEnabled;
				//Debug.Log("Should be enabled? " + isEnabled);
				for (int i = 0; i < Renderers.Count; i++)
					Renderers[i].enabled = isEnabled;
			}

			public readonly bool IsEnabled => cells.Exists(x => x.Chunk.Rendering);

		}
	}
}
