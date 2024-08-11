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



		[HarmonyPatch(typeof(Principal_ChasingNpc), "OnStateTriggerStay")]
		[HarmonyPrefix]
		static bool ActualNPCDetention(Collider other, ref NPC ___targetedNpc, ref Principal ___principal)
		{
			if (other.transform == ___targetedNpc.transform)
			{
				int num = Random.Range(0, ___principal.ec.offices.Count); // Stuff from the method itself
				___targetedNpc.Navigator.Entity.Teleport(___principal.ec.offices[num].RandomEntitySafeCellNoGarbage().FloorWorldPosition);
				___targetedNpc.SentToDetention();

				// Actual detention below
				var scolds = ___principal.audScolds; //(SoundObject[])audScolds.GetValue(___principal);
				var times = ___principal.audTimes; // (SoundObject[])audTimes.GetValue(___principal);
				var detention = ___principal.audDetention; //(SoundObject)audDetention.GetValue(___principal);

				___principal.Navigator.Entity.Teleport(___principal.ec.offices[num].RandomEntitySafeCellNoGarbage().FloorWorldPosition);
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
