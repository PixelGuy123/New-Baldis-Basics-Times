using BBTimes.CustomComponents.CustomDatas;
using PixelInternalAPI.Extensions;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace BBTimes.ModPatches.GeneratorPatches
{
	[HarmonyPatch(typeof(LevelBuilder))]
	internal class ReplacementNPCPatch
	{
		[HarmonyPatch("CreateMap")]
		[HarmonyPrefix]
		private static void ReplacementNPCOperation(LevelBuilder __instance)
		{
			if (__instance.Ec.npcsToSpawn.Count == 0) return;

			foreach (var ld in __instance.ld.previousLevels)
				ReplaceNpcsWithLevelObject(ld);

			ReplaceNpcsWithLevelObject(__instance.ld);


			void ReplaceNpcsWithLevelObject(LevelObject ld)
			{
				List<NPC> replacementNpcs = [];
				var metas = ld.forcedNpcs;
				for (int i = 0; i < metas.Length; i++)
					if (metas[i].GetComponent<CustomNPCData>()) // Replacement npcs will always be in this array. That's why there's no check for the npc replacing field.
						replacementNpcs.Add(metas[i]);

				if (replacementNpcs.Count == 0) return;

				foreach (var npc in replacementNpcs)
					__instance.Ec.npcsToSpawn.RemoveAll(x => x.GetComponent<CustomNPCData>() && x.Character == npc.Character); // Just remove any replacementnpc from the list (since they are inside the forcedNpc list)


				// Every replacement npc will have the same weight, in other words, Random.Range :)

				int max = replacementNpcs.Count;

				for (int i = 0; i < max; i++)
				{
					if (__instance.controlledRNG.NextDouble() >= 0.5f) continue; // Random chance to add a replacement npc
					int rIndex = __instance.controlledRNG.Next(replacementNpcs.Count);
					var data = replacementNpcs[rIndex].GetComponent<CustomNPCData>();
					List<Character> npcsBeingReplaced = [.. data.npcsBeingReplaced.Where(x => __instance.Ec.npcsToSpawn.Any(z => z.Character == x))];

					if (npcsBeingReplaced.Count == 0) // Fail safe to not select a replacementNpc on a seed that doesn't contain targets
					{
						replacementNpcs.RemoveAt(rIndex);
						continue;
					}

					var target = npcsBeingReplaced[__instance.controlledRNG.Next(npcsBeingReplaced.Count)];

					__instance.Ec.npcsToSpawn.Replace(x => x.Character == target, replacementNpcs[rIndex]);
					replacementNpcs.RemoveAt(rIndex);
				}
			}

		}
	}
}
