using BBTimes.Extensions;
using HarmonyLib;
using System;
//using System.Reflection;
using static UnityEngine.Object;

namespace BBTimes.ModPatches.NpcPatches
{
	[HarmonyPatch(typeof(NPC), "Despawn")]
	internal class NPCOnDespawn
	{
		[HarmonyPrefix]
		private static void CustomDespawn(NPC __instance)
		{
			var type = __instance.GetType();
			if (type == typeof(Beans)) // beans
			{
				var gum = ((Beans)__instance).gum;
				gum.Reset(); // Removes any move mod
				Destroy(gum.gameObject);
				return;
			}
			if (type == typeof(Cumulo))
			{
				((Cumulo)__instance).audMan.FlushQueue(true); //((AudioManager)c_audMan.GetValue((Cumulo)__instance)).FlushQueue(true); // Ugly, but only way
				//Destroy(((BeltManager)c_windManager.GetValue((Cumulo)__instance)).gameObject
				Destroy(((Cumulo)__instance).windManager.gameObject);
				return;
			}
			if (type == typeof(LookAtGuy))
			{
				__instance.ec.RemoveFog(((LookAtGuy)__instance).fog);
				((LookAtGuy)__instance).FreezeNPCs(false);
				return;
			}
			if (type == typeof(NoLateTeacher))
			{
				//((NoLateIcon)n_mapIcon.GetValue((NoLateTeacher)__instance))?.gameObject.SetActive(false);
				((NoLateTeacher)__instance).mapIcon?.gameObject.SetActive(false);
				//((PlayerManager)n_targetedPlayer.GetValue((NoLateTeacher)__instance))?.Am.moveMods.Remove((MovementModifier)n_moveMod.GetValue((NoLateTeacher)__instance));
				((NoLateTeacher)__instance).targetedPlayer?.Am.moveMods.Remove(((NoLateTeacher)__instance).moveMod);
				return;
			}
			if (type == typeof(Playtime))
			{
				var rope = ((Playtime)__instance).currentJumprope; //(Jumprope)p_jumprope.GetValue((Playtime)__instance);
				if (rope)
					rope.Destroy();
				return;
			}

		}
		//// Cumulo fields
		//readonly static FieldInfo c_windManager = AccessTools.Field(typeof(Cumulo), "windManager");
		//readonly static FieldInfo c_audMan = AccessTools.Field(typeof(Cumulo), "audMan");

		//// Mrs Pomp fields
		//readonly static FieldInfo n_mapIcon = AccessTools.Field(typeof(NoLateTeacher), "mapIcon");
		//readonly static FieldInfo n_targetedPlayer = AccessTools.Field(typeof(NoLateTeacher), "targetedPlayer");
		//readonly static FieldInfo n_moveMod = AccessTools.Field(typeof(NoLateTeacher), "moveMod");

		////Playtime fields
		//readonly static FieldInfo p_jumprope = AccessTools.Field(typeof(Playtime), "currentJumprope");
	}

	[HarmonyPatch]
	internal class NPCOnDespawn_Chalkles
	{
		// Chalkles

		[HarmonyPatch(typeof(Chalkboard), "Update")]
		[HarmonyPrefix]
		private static bool DestroyIfRequired(Chalkboard __instance, ref RoomController ___room, ChalkFace ___chalkFace)
		{
			if (___chalkFace == null) // If it doesn't exist
			{
				___room.functions.RemoveFunction(__instance);
				Destroy(__instance.gameObject);
				return false;
			}

			return true;
		}
	}
	[HarmonyPatch]
	internal class NPCOnDespawn_FirstPrize
	{
		// First Prize

		[HarmonyPatch(typeof(FirstPrize_Active), "Update")]
		[HarmonyPrefix]
		private static bool WhenGetDestroyed(FirstPrize_Active __instance, FirstPrize ___firstPrize, ref PlayerManager ___currentPlayer, MoveModsManager ___moveModsMan)
		{
			if (___firstPrize == null) // if first prize just gone.
			{
				___moveModsMan.RemoveAll();
				if (___currentPlayer != null)
				{
					PlayerManager playerManager = ___currentPlayer;
					playerManager.onPlayerTeleport -= __instance.PlayerTeleported;
					___currentPlayer = null;
				}
				return false;
			}

			return true;
		}
	}

}
