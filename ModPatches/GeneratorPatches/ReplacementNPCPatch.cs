using PixelInternalAPI.Extensions;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BBTimes.CustomComponents;

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
				List<WeightedNPC> replacementNpcs = [new() { weight = 100 }];
				var metas = ld.forcedNpcs;
				for (int i = 0; i < metas.Length; i++)
				{
					var co = metas[i].GetComponent<INPCPrefab>();
					if (co != null && co.ReplacementNpcs != null) // Replacement npcs will always be in this array. That's why there's no check for the npc replacing field.
						replacementNpcs.Add(new() { selection = metas[i], weight = co.ReplacementWeight });
				}

				if (replacementNpcs.Count <= 1) return;

#if CHEAT
				Debug.Log("----- replacementnpcs length: " + (replacementNpcs.Count - 1) + " -----");

				foreach (var npc in replacementNpcs)
					if (npc.selection != null)
						Debug.Log(npc.selection.name);

				Debug.Log("-----Og npc set to spawn before removal -----");
				__instance.Ec.npcsToSpawn.ForEach(x => Debug.Log(x.name));
#endif

				foreach (var npc in replacementNpcs)
				{
					if (npc.selection != null)
						__instance.Ec.npcsToSpawn.RemoveAll(x => x.GetComponent<INPCPrefab>() != null && x.Character == npc.selection.Character); // Just remove any replacementnpc from the list (since they are inside the forcedNpc list)
				}

#if CHEAT
				Debug.Log("-----Og npc set to spawn after removal -----");
				__instance.Ec.npcsToSpawn.ForEach(x => Debug.Log(x.name));
#endif
				int max = Mathf.CeilToInt(replacementNpcs.Count / 2);
#if CHEAT
				Debug.Log("----- replacement start -----");
#endif
				for (int i = 0; i < max; i++)
				{
					int rIndex = WeightedNPC.ControlledRandomIndexList(WeightedNPC.Convert(replacementNpcs), __instance.controlledRNG);

					if (rIndex == 0) continue; // the first index is null, which means there's no replacement npc

					var data = replacementNpcs[rIndex].selection.GetComponent<INPCPrefab>();
					List<Character> npcsBeingReplaced = [.. data.ReplacementNpcs.Where(x => __instance.Ec.npcsToSpawn.Any(z => z.Character == x))];
#if CHEAT
					Debug.Log("chosen replacement npc: " + replacementNpcs[rIndex].selection.name);
					Debug.Log("npcs to replace count: " + npcsBeingReplaced.Count);
#endif
					if (npcsBeingReplaced.Count == 0) // Fail safe to not select a replacementNpc on a seed that doesn't contain targets
					{
						replacementNpcs.RemoveAt(rIndex);
						continue;
					}

					var target = npcsBeingReplaced[__instance.controlledRNG.Next(npcsBeingReplaced.Count)];

					__instance.Ec.npcsToSpawn.Replace(x => x.Character == target, replacementNpcs[rIndex].selection);
					replacementNpcs.RemoveAt(rIndex);
					if (replacementNpcs.Count <= 1) return;
				}

#if CHEAT
				Debug.Log("----- Final npcs to spawn list -----");
				__instance.Ec.npcsToSpawn.ForEach(x => Debug.Log(x.name));
#endif
			}

		}
	}
}
