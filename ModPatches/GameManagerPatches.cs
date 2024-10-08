using BBTimes.CustomContent.Misc;
using BBTimes.Extensions;
using BBTimes.Plugin;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI.Components;
using System.Linq;
using BBTimes.CustomComponents;

namespace BBTimes.ModPatches
{

	[HarmonyPatch(typeof(MainGameManager))]
	internal class MainGameManagerPatches
	{

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
			if (!__instance.name.StartsWith("Lvl3")) // Not F3
				Singleton<MusicManager>.Instance.PlayMidi("Level_1_End", true); // Music
		}

		[HarmonyPatch("LoadNextLevel")]
		[HarmonyReversePatch(HarmonyReversePatchType.Original)]
		static void LoadNextLevel(object instance) =>
			throw new System.NotImplementedException("stub");

		[HarmonyPatch("LoadNextLevel")]
		[HarmonyPrefix]
		static bool PlayCutscene(MainGameManager __instance, bool ___allNotebooksFound)
		{
			if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free || !__instance.name.StartsWith("Lvl3") || !___allNotebooksFound) return true;
			var elevator = __instance.Ec.elevators.FirstOrDefault(x => x.IsOpen);
			if (!elevator) // failsafe
				return true;
			var cam = new GameObject("CameraView").AddComponent<Camera>();
			var player = Singleton<CoreGameManager>.Instance.GetPlayer(0); // Specifically for Player 0
			while (!player.plm.Entity.InteractionDisabled || !player.plm.Entity.Frozen)
			{
				player.plm.Entity.SetInteractionState(false);
				player.plm.Entity.SetFrozen(true);
			}
			player.Teleport(__instance.Ec.CellFromPosition(player.transform.position).CenterWorldPosition);

			cam.transform.position = player.transform.position;
			cam.transform.rotation = elevator.Door.direction.ToRotation();

			Singleton<CoreGameManager>.Instance.GetCamera(0).SetControllable(false);
			Singleton<CoreGameManager>.Instance.disablePause = true;

			var baldi = __instance.Ec.GetBaldi();
			baldi.enabled = false;
			baldi.Navigator.Entity.Unsquish();
			baldi.Navigator.enabled = false;
			baldi.transform.position = __instance.Ec.CellFromPosition(__instance.Ec.CellFromPosition(player.transform.position).position + elevator.Door.direction.GetOpposite().ToIntVector2() * 2).CenterWorldPosition;

			var cell = __instance.Ec.CellFromPosition(__instance.Ec.GetBaldi().transform.position);

			__instance.StartCoroutine(Animation(cam, __instance, elevator, [cell, .. __instance.Ec.GetCellNeighbors(cell.position)]));

			IEnumerator Animation(Camera cam, MainGameManager man, Elevator el, List<Cell> cellsForFire)
			{
				bool subs = Singleton<PlayerFileManager>.Instance.subtitles;
				Singleton<PlayerFileManager>.Instance.subtitles = false;

				Vector3 target = cam.transform.position + el.Door.direction.GetOpposite().ToVector3() * 13f;
				target.y = 5f;
				Vector3 src = cam.transform.position;

				float t = 0f;
				while (true)
				{
					
					t += (1.05f - t) * 3f * Time.deltaTime;
					if (t >= 1f)
						break;
					cam.transform.position = Vector3.Lerp(src, target, t);
					yield return null;
				}
				cam.transform.position = target;
				float cool = 1.5f;
				while (cool > 0f)
				{
					cool -= Time.deltaTime;
					yield return null;
				}
				el.Door.Shut();
				cool = 0.8f;
				while (cool > 0f)
				{
					cool -= Time.deltaTime;
					yield return null;
				}

				var tarRot = el.Door.direction.GetOpposite().ToRotation().eulerAngles;
				var rot = cam.transform.rotation.eulerAngles;

				t = 0f;
				while (true)
				{
					t += Mathf.Abs(Mathf.Cos(t) * 1.9f) * Time.deltaTime;
					if (t >= 1)
						break;
					cam.transform.rotation = Quaternion.Euler(Vector3.Lerp(rot, tarRot, t));
					yield return null;
				}

				cam.transform.rotation = Quaternion.Euler(tarRot);

				cool = 2f;
				while (cool > 0f)
				{
					cool -= Time.deltaTime;
					yield return null;
				}

				Vector3 ogPos = cam.transform.position;

				cool = 0f;
				while (cool <= 17f)
				{
					cool += Time.deltaTime * 3f;
					cam.transform.position = ogPos + new Vector3(Random.Range(-cool, cool), Random.Range(-cool, cool), Random.Range(-cool, cool)) * 0.05f;
					if (cool <= 8f)
						AddFire(cellsForFire[Random.Range(0, cellsForFire.Count)], __instance.Ec);
					yield return null;
				}

				Shader.SetGlobalColor("_SkyboxColor", Color.black);

				cam.transform.position = Vector3.down * 20f;
				cam.transform.forward = Vector3.down;
				Singleton<CoreGameManager>.Instance.GetCamera(0).UpdateTargets(cam.transform, 24);
				Singleton<MusicManager>.Instance.StopMidi();
				Singleton<MusicManager>.Instance.StopFile();

				cool = 4f;
				while (cool > 0f)
				{
					cool -= Time.deltaTime;
					yield return null;
				}
				Singleton<PlayerFileManager>.Instance.subtitles = subs;
				Object.Destroy(cam); // Only the camera lol

				LoadNextLevel(man);
				yield break;
			}

			return false;
		}

		// ******* Base Game Manager *******

		[HarmonyPatch(typeof(BaseGameManager), "ElevatorClosed")]
		[HarmonyPostfix]
		private static void REDAnimation(Elevator elevator, BaseGameManager __instance, int ___elevatorsClosed, EnvironmentController ___ec)
		{
			if (__instance is not MainGameManager || __instance.GetType().BaseType != typeof(BaseGameManager) || Singleton<CoreGameManager>.Instance.currentMode == Mode.Free) // MainGameManager expected
				return;

			if (___elevatorsClosed == 1)
			{
				Shader.SetGlobalColor("_SkyboxColor", Color.red);
				Singleton<MusicManager>.Instance.SetSpeed(0.1f);
				__instance.StartCoroutine(___ec.LightChanger(___ec.AllExistentCells(), 0.2f));
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
					c.lightColor = Color.red;
					c.SetLight(true);
				}

				return;
			}
			if (___elevatorsClosed == 3)
			{
				___ec.GetComponent<EnvironmentControllerData>()?.OngoingEvents.ForEach(x => { if (x != null) ___ec.StopCoroutine(x); }); // Disable active/about to activate events

				for (int i = 0; i < ___ec.CurrentEventTypes.Count; i++)
				{
					var v2 = ___ec.GetEvent(___ec.CurrentEventTypes[i]);
					if (v2.Active)
						v2.EndEarlier(); // End all events
				}

				foreach (var c in ___ec.AllExistentCells()) // To avoid events like Freezing event from overriding it wrong
				{
					c.lightColor = Color.red;
					c.SetLight(true);
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

				for (int i = 0; i < Singleton<CoreGameManager>.Instance.setPlayers; i++)
					Singleton<CoreGameManager>.Instance.GetCamera(i).GetCustomCam().ReverseSlideFOVAnimation(new ValueModifier(), 55f, 9.5f); // Animation (weird way, I know)

				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(angryBal);

				for (int i = 0; i < ___ec.Npcs.Count; i++)
				{
					if (___ec.Npcs[i].Character != Character.Baldi)
					{
						___ec.Npcs[i].Despawn();
						i--;
					}
					else if (___ec.Npcs[i].GetType() == typeof(Baldi))
						___ec.Npcs[i].StartCoroutine(GameExtensions.InfiniteAnger((Baldi)___ec.Npcs[i], 0.6f));
				}

				___ec.StartCoroutine(SpawnFires());
			}


			IEnumerator SpawnFires()
			{
				float cooldown = fireCooldown;
				float maxCooldown = fireCooldown;
				var cs = ___ec.AllTilesNoGarbage(false, true);
				while (cs.Count != 0)
				{
					cooldown -= Time.deltaTime * ___ec.EnvironmentTimeScale;
					if (cooldown <= 0f)
					{
						var c = Random.Range(0, cs.Count);
						maxCooldown -= ___ec.EnvironmentTimeScale * 1.2f;
						if (maxCooldown < 0.03f)
							maxCooldown = 0.03f; // just a limit

						cooldown += maxCooldown;
						AddFire(cs[c], ___ec);
						cs.RemoveAt(c);
					}

					yield return null;
				}

				yield break;
			}

		}

		static void AddFire(Cell cell, EnvironmentController ec)
		{
			var obj = Object.Instantiate(fire, cell.TileTransform);
			obj.transform.localScale = Vector3.one * Random.Range(0.6f, 1.5f);
			obj.transform.position = cell.FloorWorldPosition + new Vector3(Random.Range(-3f, 3f), obj.transform.localScale.y.LinearEquation(4f, 0.28f), Random.Range(-3f, 3f)); // 1º Function YAAAY y = ax + b >> y = 4x + 0.25 should give the expected y to the fire
			obj.SetActive(true);
			cell.AddRenderer(obj.GetComponent<SpriteRenderer>());

			var f = obj.GetComponent<SchoolFire>();
			f.Initialize(ec);
			Vector3 scale = f.transform.localScale;
			f.transform.localScale = Vector3.zero;
			f.StartCoroutine(f.Spawn(scale));
		}

		const float fireCooldown = 3f;
		internal static LoopingSoundObject chaos0; // that very silent noise from Classic
		internal static LoopingSoundObject chaos1; // Chaos noises
		internal static LoopingSoundObject chaos2;
		internal static SoundObject angryBal;
		internal static GameObject fire;
		internal static Texture2D[] gateTextures = new Texture2D[3];
	}
}
