using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class NullCullingManager : MonoBehaviour
	{
		public void CheckAllChunks() =>
			rendererPairs.ForEach(x => x.UpdateChunk());
		
		public void AddRendererToCell(Cell cell, Renderer newRend)
		{
			int idx = rendererPairs.FindIndex(x => x.cells.Contains(cell) || x.Renderers.Contains(newRend));
			if (idx == -1)
			{
				rendererPairs.Add(new([cell], [newRend]));
			}
			else
			{
				rendererPairs[idx].Renderers.Add(newRend);
				if (!rendererPairs[idx].cells.Contains(cell))
					rendererPairs[idx].cells.Add(cell);
			}
		}

		readonly List<ChunkGroup> rendererPairs = [];

		[SerializeField]
		internal CullingManager cullMan;

		public readonly struct ChunkGroup(List<Cell> cells,  List<Renderer> renderers)
		{
			internal readonly List<Cell> cells = cells;
			internal readonly List<Renderer> Renderers = renderers;

			internal readonly void UpdateChunk()
			{
				bool isEnabled = IsEnabled;
				for (int i = 0; i < Renderers.Count; i++)
					Renderers[i].enabled = isEnabled;
			}

			public readonly bool IsEnabled => cells.Exists(x => x.Chunk?.Rendering ?? false);

		}
	}
}
