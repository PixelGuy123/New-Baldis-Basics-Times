using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(Principal))]
	internal class PrincipalWhistleAnimation
	{
		[HarmonyPatch("WhistleChance")]
		[HarmonyPrefix]
		private static void GetI(Principal __instance, ref AudioManager ___audMan)
		{
			i = __instance;
			man = ___audMan;
		}

		[HarmonyPatch("WhistleChance")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> Animation(IEnumerable<CodeInstruction> instructions) =>
			new CodeMatcher(instructions)
			.MatchForward(false,
				new CodeMatch(OpCodes.Ldarg_0),
				new CodeMatch(CodeInstruction.LoadField(typeof(Principal), "audMan")),
				new CodeMatch(OpCodes.Ldarg_0),
				new CodeMatch(CodeInstruction.LoadField(typeof(Principal), "audWhistle")),
				new CodeMatch(OpCodes.Callvirt, AccessTools.Method("AudioManager:PlaySingle", [typeof(SoundObject)]))
				)
			.InsertAndAdvance(Transpilers.EmitDelegate(() =>
			{
				if (i == null || man == null) return;

				i.StartCoroutine(Animation(i, man));
			}))
			.InstructionEnumeration();

		static IEnumerator Animation(Principal p, AudioManager man)
		{
			bool turn = false;
			float scale = 1f;
			var target = p.spriteBase.transform.GetChild(0);
			yield return null; // Wait for audio to play

			while (man.QueuedAudioIsPlaying)
			{
				if (!turn)
				{
					scale += p.TimeScale * Time.deltaTime;
					if (scale >= 1.2f)
					{
						scale = 1.2f;
						turn = true;
					}
				}
				else
				{
					scale -= p.TimeScale * Time.deltaTime;
					if (scale <= 1f)
					{
						turn = false;
						scale = 1f;
					}
				}

				target.localScale = Vector3.one * scale;

				yield return null;
			}

			while (scale >= 1f)
			{
				scale -= p.TimeScale * Time.deltaTime;
				if (scale <= 1f)
					break;
				target.localScale = Vector3.one * scale;
				yield return null;
			}

			target.localScale = Vector3.one;

			yield break;
		}

		static Principal i;

		static AudioManager man;
	}
}
