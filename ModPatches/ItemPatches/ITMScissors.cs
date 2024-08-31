using BBTimes.CustomComponents.NpcSpecificComponents;
using HarmonyLib;

namespace BBTimes.ModPatches.ItemPatches
{
	[HarmonyPatch(typeof(ITM_Scissors), "Use")]
	internal class ITMScissors
	{
		static void Postfix(PlayerManager pm, ref bool __result, SoundObject ___audSnip)
		{
			bool temp = __result;

			for (int i = 0; i < Bubble.affectedPlayers.Count; i++)
			{
				if (Bubble.affectedPlayers[i].Key == pm)
				{
					Bubble.affectedPlayers[i].Value.Pop();
					__result = true;
				}
			}

			if (!temp && __result)
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(___audSnip);
		}
	}
}
