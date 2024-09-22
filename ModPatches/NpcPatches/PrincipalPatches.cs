using HarmonyLib;
using System.Collections.Generic;

namespace BBTimes.ModPatches.NpcPatches
{
	[HarmonyPatch(typeof(Principal))]
	internal class PrincipalPatches
	{
		[HarmonyPatch("Scold")]
		[HarmonyPrefix]
		private static bool CustomScold(AudioManager ___audMan, string brokenRule)
		{
			if (ruleBreaks.TryGetValue(brokenRule, out SoundObject sound))
			{
				___audMan.FlushQueue(true);
				___audMan.QueueAudio(sound);

				return false;
			}

			return true;
		}

		internal static Dictionary<string, SoundObject> ruleBreaks = [];
	}
}
