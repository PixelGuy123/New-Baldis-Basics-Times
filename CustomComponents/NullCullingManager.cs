using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	internal class NullCullingManager : MonoBehaviour
	{
		public void CheckAllChunks(Chunk chunkToCull)
		{
			for (int i = 0; i < chunkToCull.visibleChunks.Length; i++)
			{
				int idx = rendererPairs.FindIndex(x => x.HasChunk(cullMan.allChunks[i]));
				if (idx == -1) continue;

				rendererPairs[idx].DisableChunk(!chunkToCull.visibleChunks[i]);
			}
		}

		public void AddRendererToCell(Cell cell, Renderer newRend)
		{
			int idx = rendererPairs.FindIndex(x => x.cells.Contains(cell) || x.Renderers.Contains(newRend));
			if (idx == -1)
				rendererPairs.Add(new([cell], [newRend]));
			else
			{
				rendererPairs[idx].Renderers.Add(newRend);
				rendererPairs[idx].cells.Add(cell);
			}
		}

		readonly List<ChunkGroup> rendererPairs = [];

		[SerializeField]
		internal CullingManager cullMan;

		internal struct ChunkGroup(List<Cell> cells,  List<Renderer> renderers)
		{
			internal readonly bool HasChunk(Chunk chunk) => cells.Exists(x => x.Chunk == chunk);
			internal readonly List<Cell> cells = cells;
			internal readonly List<Renderer> Renderers = renderers;

			int disables = 0;
			internal void DisableChunk(bool disable)
			{
				if (disable)
					disables++;
				else
					disables--;
				if (disables < 0)
					disables = 0;

				for (int i = 0; i < Renderers.Count; i++)
					Renderers[i].enabled = !IsDisabled;
			}

			internal readonly bool IsDisabled => disables > 0;

		}
	}
}
