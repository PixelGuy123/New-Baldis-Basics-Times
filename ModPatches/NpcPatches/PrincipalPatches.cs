using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection.Emit;
// using System.Reflection;

namespace BBTimes.ModPatches.NpcPatches
{
	[HarmonyPatch(typeof(Principal))]
	internal class PrincipalPatches
	{
		[HarmonyPatch("Scold")]
		[HarmonyPrefix]
		private static bool CustomScold(AudioManager ___audMan, string brokenRule)
		{
			if (ruleBreaks.ContainsKey(brokenRule))
			{
				___audMan.FlushQueue(true);
				___audMan.QueueAudio(ruleBreaks[brokenRule]);

				return false;
			}

			return true;
		}

		internal static Dictionary<string, SoundObject> ruleBreaks = [];
		

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
			.InsertAndAdvance(
			new(OpCodes.Ldarg_0),
			Transpilers.EmitDelegate<System.Action<Principal>>(i => i.Navigator.Entity.StartCoroutine(Animation(i, i.audMan))))
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

		//readonly static FieldInfo audTimes = AccessTools.Field(typeof(Principal), "audTimes");
		//readonly static FieldInfo audScolds = AccessTools.Field(typeof(Principal), "audScolds");
		//readonly static FieldInfo audDetention = AccessTools.Field(typeof(Principal), "audDetention");
		//readonly static FieldInfo audMan = AccessTools.Field(typeof(Principal), "audMan");



		[HarmonyPatch(typeof(Principal_ChasingNpc), "OnStateTriggerStay")]
		[HarmonyPrefix]
		static bool ActualNPCDetention(Collider other, ref NPC ___targetedNpc, ref Principal ___principal)
		{
			if (other.transform == ___targetedNpc.transform)
			{
				int num = Random.Range(0, ___principal.ec.offices.Count); // Stuff from the method itself
				___targetedNpc.transform.position = ___principal.ec.offices[num].RandomEntitySafeCellNoGarbage().CenterWorldPosition;
				___targetedNpc.SentToDetention();

				// Actual detention below
				var scolds = ___principal.audScolds; //(SoundObject[])audScolds.GetValue(___principal);
				var times = ___principal.audTimes; // (SoundObject[])audTimes.GetValue(___principal);
				var detention = ___principal.audDetention; //(SoundObject)audDetention.GetValue(___principal);

				___principal.transform.position = ___principal.ec.offices[num].RandomEntitySafeCellNoGarbage().CenterWorldPosition;
				var man = ___principal.audMan; //(AudioManager)audMan.GetValue(___principal);
				man.QueueAudio(times[0]);
				man.QueueAudio(detention);
				man.QueueAudio(scolds[Random.Range(0, scolds.Length)]);
				___principal.behaviorStateMachine.ChangeState(new Principal_Detention(___principal, 3f));

			}
			return false;
		}
	}
}
