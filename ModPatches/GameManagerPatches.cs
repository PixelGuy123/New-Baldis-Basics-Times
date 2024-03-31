using BBTimes.CustomComponents;
using PixelInternalAPI.Components;
using PixelInternalAPI.Classes;
using BBTimes.CustomContent.Misc;
using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.ModPatches.GeneratorPatches;
using BBTimes.Plugin;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(MainGameManager))]
	internal class MainGameManagerPatches
	{
		[HarmonyPatch("BeginPlay")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> MusicChanges(IEnumerable<CodeInstruction> instructions) =>
			new CodeMatcher(instructions)
			.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "school", "school"))
			.SetInstruction(Transpilers.EmitDelegate(() =>
			{
				var comp = Singleton<BaseGameManager>.Instance.GetComponent<MainGameManagerExtraComponent>();
				var rng = new System.Random(PostRoomCreation.i?.controlledRNG.Next() ?? 0);
				if (comp == null)
					return "school";

				int idx = rng.Next(comp.midis.Length + 1);
				if (idx >= comp.midis.Length)
					return "school";

				return comp.midis[idx];
			}))
			.InstructionEnumeration();

		[HarmonyPatch("AllNotebooks")]
		[HarmonyPostfix]
		private static void BaldiAngerPhase(MainGameManager __instance)
		{
			var core = Singleton<CoreGameManager>.Instance;
			if (core.currentMode == Mode.Free) // no baldi audio
			{
				core.audMan.FlushQueue(true);
				return;
			}
			if (BooleanStorage.endGameMusic && !__instance.name.StartsWith("Lvl3")) // Not F3
				Singleton<MusicManager>.Instance.PlayMidi("Level_1_End", true); // Music

			for (int i = 0; i < core.setPlayers; i++)
			{
				var cam = core.GetCamera(i);
				cam.StartCoroutine(new BaseModifier().ReverseSlideFOVAnimation(cam.GetComponent<CustomPlayerCameraComponent>().fovModifiers, 35f, 5f)); // Animation (weird way, I know)
			}
		}

		// ******* Base Game Manager *******

		[HarmonyPatch(typeof(BaseGameManager), "ElevatorClosed")]
		[HarmonyPostfix]
		private static void REDAnimation(Elevator elevator, BaseGameManager __instance, int ___elevatorsClosed, EnvironmentController ___ec)
		{
			if (!BooleanStorage.endGameAnimation || __instance.GetType() != typeof(MainGameManager)) // MainGameManager expected
				return;

			if (___elevatorsClosed == 1)
			{
				List<Cell> list = [];
				foreach (Cell tileController in ___ec.AllExistentCells())
				{
					if (tileController.lightStrength <= 1)
					{
						tileController.lightColor = Color.red;
						___ec.SetLight(true, tileController);
					}
					else
					list.Add(tileController);
				}
				Shader.SetGlobalColor("_SkyboxColor", Color.red);
				Singleton<MusicManager>.Instance.SetSpeed(0.1f);
				__instance.StartCoroutine(___ec.LightChanger(list, true, 0.2f));
				if (__instance.name.StartsWith("Lvl3"))
					Singleton<MusicManager>.Instance.QueueFile(chaos0, true);
				return;
			}
			if (___elevatorsClosed == 2)
			{
				Singleton<MusicManager>.Instance.StopFile(); // Stop quiet noise
				Singleton<MusicManager>.Instance.QueueFile(chaos1, true);

				Singleton<MusicManager>.Instance.MidiPlayer.MPTK_Transpose = Random.Range(-24, -12);
				___ec.standardDarkLevel = new Color(1f, 0f, 0f);
				foreach (var c in ___ec.AllExistentCells())
				{
					___ec.SetLight(true, c);
					c.lightColor = Color.red;
				}

				return;
			}
			if (___elevatorsClosed == 3)
			{
				___ec.StopAllCoroutines(); // Might look a little dangerous, but I have no other way to disable events

				foreach (var v in ___ec.CurrentEventTypes)
				{
					var v2 = ___ec.GetEvent(v);
					if (v2.Active)
						v2.End(); // End all events
				}

				var gate = elevator.transform.Find("Gate");
				gate.transform.Find("Gate (1)").GetComponent<MeshRenderer>().material.mainTexture = gateTextures[0];
				gate.transform.Find("Gate (0)").GetComponent<MeshRenderer>().material.mainTexture = gateTextures[1];
				gate.transform.Find("Gate (2)").GetComponent<MeshRenderer>().material.mainTexture = gateTextures[2];

				Singleton<MusicManager>.Instance.QueueFile(chaos2, true);
				if (!Singleton<PlayerFileManager>.Instance.reduceFlashing)
				{
					___ec.standardDarkLevel = new Color(0.2f, 0f, 0f);
					___ec.FlickerLights(true);
				}
				for (int i = 0; i < Singleton<MusicManager>.Instance.MidiPlayer.Channels.Length; i++)
				{
					Singleton<MusicManager>.Instance.MidiPlayer.MPTK_ChannelEnableSet(i, false);
				}

				var core = Singleton<CoreGameManager>.Instance;
				for (int i = 0; i < core.setPlayers; i++)
				{
					var cam = core.GetCamera(i);
					cam.StartCoroutine(new BaseModifier().ReverseSlideFOVAnimation(cam.GetComponent<CustomPlayerCameraComponent>().fovModifiers, 55f, 9.5f)); // Animation (weird way, I know)
				}
				core.audMan.PlaySingle(angryBal);

				for (int i = 0; i < ___ec.Npcs.Count; i++)
				{
					if (___ec.Npcs[i].Character != Character.Baldi)
					{
						___ec.Npcs[i].Despawn();
						i--;
					}
					else if (___ec.Npcs[i].GetType() == typeof(Baldi))
						___ec.Npcs[i].StartCoroutine(GameExtensions.InfiniteAnger((Baldi)___ec.Npcs[i], 0.005f));
				}

				___ec.StartCoroutine(SpawnFires());




				// Notes: add gate change, fire, npc despawn functions (so they don't break the game)
			}


			IEnumerator SpawnFires()
			{
				float cooldown = fireCooldown;
				float maxCooldown = fireCooldown;
				var cs = ___ec.AllTilesNoGarbage(false, true);
				while (cs.Count > 0)
				{
					cooldown -= Time.deltaTime * ___ec.EnvironmentTimeScale;
					if (cooldown <= 0f)
					{
						var c = Random.Range(0, cs.Count);
						var obj = Object.Instantiate(fire, cs[c].TileTransform);
						obj.transform.localScale = Vector3.one * Random.Range(0.6f, 1.5f);
						obj.transform.position = cs[c].FloorWorldPosition + new Vector3(Random.Range(-3f, 3f), (4 * obj.transform.localScale.y) + 0.28f, Random.Range(-3f, 3f)); // 1º Function YAAAY y = ax + b >> y = 4x + 0.25 should give the expected y to the fire
						obj.SetActive(true);
						maxCooldown -= ___ec.EnvironmentTimeScale * 0.5f;
						if (maxCooldown < 0.1f)
							maxCooldown = 0.1f; // just a limit

						cooldown = maxCooldown;
						cs[c].AddRenderer(obj.GetComponent<SpriteRenderer>());
						cs.RemoveAt(c);

						var f = obj.GetComponent<SchoolFire>();
						f.ec = ___ec;
						Vector3 scale = f.transform.localScale;
						f.transform.localScale = Vector3.zero;
						f.StartCoroutine(f.Spawn(scale));
						
					}

					yield return null;
				}

				yield break;
			}
		}

		const float fireCooldown = 5f;
		internal static LoopingSoundObject chaos0; // that very silent noise from Classic
		internal static LoopingSoundObject chaos1; // Chaos noises
		internal static LoopingSoundObject chaos2;
		internal static SoundObject angryBal;
		internal static GameObject fire;
		internal static Texture2D[] gateTextures = new Texture2D[3];
	}

	// ********** Endless Game Manager ************
	[HarmonyPatch(typeof(EndlessGameManager))]

	internal class EndlessGameManagerPatches
	{
		[HarmonyPatch("BeginPlay")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> MusicChanges(IEnumerable<CodeInstruction> instructions) =>
			new CodeMatcher(instructions)
			.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "school", "school"))
			.SetInstruction(Transpilers.EmitDelegate(() =>
			{
				var midis = BBTimesManager.floorDatas[3].MidiFiles;
				var rng = new System.Random(PostRoomCreation.i?.controlledRNG.Next() ?? 0);
				if (midis.Count == 0) return "school"; // For some reason mthe MainGameManagerExtraComponent isn't added to random endless manager. So I'm manually selecting the floorData

				int idx = rng.Next(midis.Count + 1);
				if (idx >= midis.Count)
					return "school";

				return midis[rng.Next(midis.Count)];
			}))
			.InstructionEnumeration();
	}
}
