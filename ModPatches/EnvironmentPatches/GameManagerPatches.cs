using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BBTimes.CustomComponents;
using BBTimes.CustomContent.Misc;
using BBTimes.Extensions;
using BBTimes.Manager;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.ModPatches.EnvironmentPatches
{

	[ConditionalPatchNoMod("rad.rulerp.baldiplus.arcaderenovations")]
	[HarmonyPatch(typeof(MainGameManager))]
	internal class MainGameManagerPatches
	{

		// SECRET ENDING PATCH

		[HarmonyPatch("LoadSceneObject")]
		[HarmonyPrefix]
		private static void RedirectEndingIfPossible(MainGameManager __instance, ref SceneObject sceneObject, bool restarting)
		{
			if (restarting || !allowEndingToBePlayed || !__instance.levelObject.finalLevel) return;

			sceneObject = secretEndingObj;
		}


		public static bool allowEndingToBePlayed = false;

		internal static SceneObject secretEndingObj;


		// -------------------------

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

			if (
				!BBTimesManager.plug.disableSchoolhouseEscape.Value &&
				!__instance.Ec.timeOut &&
				__instance.levelObject != null && // Premade levels have this as null, so a careful check is required
				!__instance.levelObject.finalLevel) // Not F3
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
			// if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free || !__instance.levelObject.finalLevel || !___allNotebooksFound) return true;

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
			PlayerVisual.GetPlayerVisual(0).SetEmotion(1); // emotion 1 = sad

			player.Teleport(__instance.Ec.CellFromPosition(player.transform.position).CenterWorldPosition);

			cam.transform.position = player.transform.position;
			cam.transform.rotation = elevator.Door.direction.ToRotation();

			Singleton<CoreGameManager>.Instance.GetCamera(0).SetControllable(false);
			Singleton<CoreGameManager>.Instance.disablePause = true;

			var schoolBaldi = __instance.Ec.GetBaldi();
			schoolBaldi?.Navigator.Entity.SetActive(false); // We won't be using original Baldi anymore, but a copy of it
			Vector3 elvPos = __instance.Ec.CellFromPosition(__instance.Ec.CellFromPosition(player.transform.position).position + (elevator.Door.direction.GetOpposite().ToIntVector2() * 2)).CenterWorldPosition;

			var baldi = Object.Instantiate(placeholderBaldi);
			baldi.transform.position = elvPos;

			var cell = __instance.Ec.CellFromPosition(elvPos);

			__instance.StartCoroutine(Animation(cam, __instance, elevator));

			IEnumerator Animation(Camera cam, MainGameManager man, Elevator el)
			{
				// ---------------------------------------------------------------------
				// PHASE 1: SETUP - Prepare for the cutscene
				// ---------------------------------------------------------------------
				Debug.Log("Starting BBT Elevator Chase Cutscene...");

				// Temporarily disable subtitles
				bool subs = Singleton<PlayerFileManager>.Instance.subtitles;
				Singleton<PlayerFileManager>.Instance.subtitles = false;

				// Store camera's original state to restore it later
				Transform originalCamParent = cam.transform.parent;
				Vector3 originalCamLocalPos = cam.transform.localPosition;
				Quaternion originalCamLocalRot = cam.transform.localRotation;

				// --- Cinematic Timings (Adjust these to change the pacing!) ---
				float walkIntoElevatorTime = 2.0f;
				float lookAroundTime = 3.0f;
				float snapTurnTime = 0.25f;
				float standoffTime = 3.0f;
				float baldiChargeTime = 0.7f;
				float doorCloseDelay = 0.15f; // How long after Baldi starts charging the door begins to close

				// Unparent the camera from the player for cinematic control
				cam.transform.SetParent(null);
				Vector3 playerStartPos = cam.transform.position;

				// ---------------------------------------------------------------------
				// SHOT 1: THE ENTRANCE (Image 1 & 2)
				// The player walks into the elevator and looks around.
				// ---------------------------------------------------------------------

				// Define the path into the elevator
				Vector3 walkTargetPos = el.transform.position - (el.Door.direction.ToVector3() * 5f) + Vector3.up * 1.5f; // 5 units in
				Vector3 lookAroundTargetPos = el.transform.position - (el.Door.direction.ToVector3() * 1.5f) + Vector3.up * 1.5f; // Final position, near center

				// Player walks in normally
				yield return man.StartCoroutine(MoveWithHeadbob(cam.transform, playerStartPos, walkTargetPos, walkIntoElevatorTime, 0.1f, 4f));

				// Player slows down and looks around while moving to the center
				Quaternion lookLeft = Quaternion.LookRotation(cam.transform.forward - cam.transform.right * 0.4f);
				Quaternion lookRight = Quaternion.LookRotation(cam.transform.forward + cam.transform.right * 0.4f);

				float lookTimer = 0f;
				Vector3 lookStartPos = cam.transform.position; // Start from current pos
				while (lookTimer < lookAroundTime)
				{
					lookTimer += Time.deltaTime;
					float p = lookTimer / lookAroundTime;

					// Move to final position
					cam.transform.position = Vector3.Lerp(lookStartPos, lookAroundTargetPos, p);

					// Look from left to right
					cam.transform.rotation = Quaternion.Slerp(lookLeft, lookRight, p);

					// Add a slower headbob
					float headbobOffset = Mathf.Sin(Time.time * 2f) * 0.05f;
					cam.transform.position += new Vector3(0, headbobOffset, 0);

					yield return null;
				}

				// ---------------------------------------------------------------------
				// SHOT 2: THE REVEAL (Image 3)
				// A noise is heard, the player snaps around to see Baldi.
				// ---------------------------------------------------------------------

				// NOTE: Play the single ruler slap sound here!
				// el.audioDevice.PlayOneShot(yourSlapSound);

				// Lean head up slightly
				yield return man.StartCoroutine(RotateCamera(cam.transform, cam.transform.rotation * Quaternion.Euler(-10, 0, 0), 0.2f));
				yield return new WaitForSeconds(0.3f);

				// Teleport Baldi to the entrance while the player isn't looking
				Vector3 baldiSpawnPos = el.transform.position + (el.Door.direction.ToVector3() * 4f);
				baldi.transform.position = baldiSpawnPos;

				// Snap turn to look at Baldi
				Quaternion lookAtBaldiRotation = Quaternion.LookRotation(baldi.transform.position - cam.transform.position);
				yield return man.StartCoroutine(RotateCamera(cam.transform, lookAtBaldiRotation, snapTurnTime));

				// Make Baldi look at the player
				baldi.transform.LookAt(new Vector3(cam.transform.position.x, baldi.transform.position.y, cam.transform.position.z));

				// ---------------------------------------------------------------------
				// SHOT 3: THE STANDOFF & ESCAPE (Image 4 & After)
				// Baldi charges, the doors close, chaos ensues.
				// ---------------------------------------------------------------------

				// Start a "spooked" camera shake
				Coroutine shakeRoutine = man.StartCoroutine(SpookedCameraShake(cam.transform, 0.03f, 5f));

				// Wait for the standoff period
				yield return new WaitForSeconds(standoffTime);

				// Baldi slides forward slightly, then charges
				Vector3 baldiSlidePos = baldi.transform.position - baldi.transform.forward * 1f;
				Vector3 baldiChargeTargetPos = el.transform.position + el.Door.direction.ToVector3(); // Target is the door threshold

				// Animate Baldi's movement (as a coroutine so it doesn't block)
				man.StartCoroutine(AnimateBaldiCharge(man, baldi.transform, baldiSlidePos, baldiChargeTargetPos, baldiChargeTime));

				// Wait a brief moment, then close the doors!
				yield return new WaitForSeconds(doorCloseDelay);

				// Player panics and looks away (upwards)
				man.StopCoroutine(shakeRoutine); // Stop the shake
				cam.transform.localRotation = lookAtBaldiRotation; // Reset from shake
				yield return man.StartCoroutine(RotateCamera(cam.transform, cam.transform.rotation * Quaternion.Euler(-45, 0, 0), 0.3f));

				el.Close(); // Close the doors! This should play the door close sound.

				// Wait for the doors to fully close
				yield return new WaitForSeconds(1.5f);

				// NOTE: Play a few rapid "banging" sounds on the door
				// for (int i = 0; i < 3; i++) {
				//     el.audioDevice.PlayOneShot(yourBangSound);
				//     yield return new WaitForSeconds(Random.Range(0.2f, 0.4f));
				// }

				// Player realizes they are safe and slowly looks back at the door
				Quaternion lookAtDoorRotation = Quaternion.LookRotation(el.Door.direction.ToVector3());
				yield return man.StartCoroutine(RotateCamera(cam.transform, lookAtDoorRotation, 2.0f));

				// Deactivate Baldi now that he's off-screen and his role is done
				baldi.gameObject.SetActive(false);

				// NOTE: Play distant explosion sounds
				// el.audioDevice.PlayOneShot(yourExplosionSound);
				yield return new WaitForSeconds(2f);

				// ---------------------------------------------------------------------
				// PHASE 4: CLEANUP & TRANSITION
				// ---------------------------------------------------------------------
				Debug.Log("BBT Cutscene Finished. Starting elevator travel.");

				Shader.SetGlobalColor("_SkyboxColor", Color.black);

				cam.transform.position = Vector3.down * 20f;
				cam.transform.forward = Vector3.down;
				Singleton<CoreGameManager>.Instance.GetCamera(0).UpdateTargets(cam.transform, 24);
				Singleton<MusicManager>.Instance.StopMidi();
				Singleton<MusicManager>.Instance.StopFile();

				Singleton<PlayerFileManager>.Instance.subtitles = subs;
				Object.Destroy(cam); // Only the camera lol

				PlayerVisual.GetPlayerVisual(0).SetEmotion(0);

				LoadNextLevel(man);
			}


			// ---------------------------------------------------------------------
			// HELPER COROUTINES - Place these in the same class
			// ---------------------------------------------------------------------

			IEnumerator MoveWithHeadbob(Transform target, Vector3 startPos, Vector3 endPos, float duration, float bobMagnitude, float bobFrequency)
			{
				float timer = 0f;
				while (timer < duration)
				{
					timer += Time.deltaTime;
					float p = timer / duration;
					p = Mathf.Sin(p * Mathf.PI * 0.5f); // Ease-out effect

					Vector3 newPos = Vector3.Lerp(startPos, endPos, p);

					// Add headbob
					float headbobOffset = Mathf.Sin(Time.time * bobFrequency) * bobMagnitude;
					newPos.y += headbobOffset;

					target.position = newPos;
					yield return null;
				}
				target.position = endPos; // Snap to final position
			}

			IEnumerator RotateCamera(Transform target, Quaternion targetRotation, float duration)
			{
				float timer = 0f;
				Quaternion startRotation = target.rotation;
				while (timer < duration)
				{
					timer += Time.deltaTime;
					float p = Mathf.SmoothStep(0, 1, timer / duration);
					target.rotation = Quaternion.Slerp(startRotation, targetRotation, p);
					yield return null;
				}
				target.rotation = targetRotation;
			}

			IEnumerator SpookedCameraShake(Transform camTransform, float magnitude, float frequency)
			{
				Vector3 originalPos = camTransform.localPosition;
				while (true)
				{
					float x = (Mathf.PerlinNoise(Time.time * frequency, 0) * 2 - 1) * magnitude;
					float y = (Mathf.PerlinNoise(0, Time.time * frequency) * 2 - 1) * magnitude;
					camTransform.localPosition = originalPos + new Vector3(x, y, 0);
					yield return null;
				}
			}

			IEnumerator AnimateBaldiCharge(MainGameManager man, Transform baldi, Vector3 slidePos, Vector3 chargePos, float chargeTime)
			{
				// Brief hesitation slide
				float slideDuration = 0.5f;
				yield return man.StartCoroutine(MoveTransform(baldi, baldi.position, slidePos, slideDuration));

				// Full charge
				yield return man.StartCoroutine(MoveTransform(baldi, slidePos, chargePos, chargeTime));
			}

			IEnumerator MoveTransform(Transform target, Vector3 start, Vector3 end, float duration)
			{
				float timer = 0f;
				while (timer < duration)
				{
					timer += Time.deltaTime;
					target.position = Vector3.Lerp(start, end, timer / duration);
					yield return null;
				}
				target.position = end;
			}

			return false;
		}

		// Time Out patch here because it's related to below
		[HarmonyPatch(typeof(TimeOut), "Begin")]
		[HarmonyPostfix]
		static void FixMusicSpeed() =>
			Singleton<MusicManager>.Instance.SetSpeed(1f);

		// ******* Base Game Manager *******

		[HarmonyPatch(typeof(BaseGameManager), "ElevatorClosed")]
		[HarmonyPostfix]
		private static void REDAnimation(Elevator elevator, BaseGameManager __instance, int ___elevatorsClosed, EnvironmentController ___ec)
		{
			if (___ec.timeOut || __instance.levelObject == null || __instance.GetType() != typeof(MainGameManager) || Singleton<CoreGameManager>.Instance.currentMode == Mode.Free) // MainGameManager expected
				return;

			if (___elevatorsClosed == 1)
			{
				Shader.SetGlobalColor("_SkyboxColor", Color.red);
				Singleton<MusicManager>.Instance.SetSpeed(0.1f);
				__instance.StartCoroutine(___ec.LightChanger(___ec.AllExistentCells(), 0.2f));
				if (__instance.levelObject.finalLevel)
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
					c.SetPower(true);
					c.SetLight(true);
				}

				return;
			}

			if (!__instance.levelObject.finalLevel)
				return;

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

				Baldi baldiToFollow = null;

				for (int i = 0; i < ___ec.Npcs.Count; i++)
				{
					var npc = ___ec.Npcs[i];
					try
					{
						if (npc is Baldi bald)
						{
							bald = (Baldi)npc;
							bald.StartCoroutine(GameExtensions.InfiniteAnger(bald, 0.6f));
							if (npc.Character == Character.Baldi) // Check Baldi enum (TeacherAPI has unique enums, so it's fine)
								baldiToFollow = bald;

							continue;
						}

						npc.Despawn();
						i--;
					}
					catch (System.Exception e)
					{
						Object.Destroy(npc.gameObject);
						___ec.Npcs.RemoveAt(i--);

						Debug.LogWarning($"-------- The NPC {npc.name} failed to be despawned or is not Baldi --------");
						Debug.LogException(e);
					}
				}

				___ec.SetTimeLimit(9999f);
				___ec.StartCoroutine(SpawnFires());
				if (baldiToFollow)
					___ec.StartCoroutine(DangerousAngryBaldiAnimation(___ec, baldiToFollow));


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

		static IEnumerator DangerousAngryBaldiAnimation(EnvironmentController ec, Baldi baldi) // Yeah, made the main IEnumerator, then passed through DeepSeek R1 to add random animations to the camera because it's hellish to code that manually
		{
			const float distanceFromBaldi = 14.5f;
			const float shakeIntensity = 0.35f;
			const float shakeSpeed = 20f;
			const float maxRoll = 4f;
			const float fovEnd = 100f;
			const float fovInitialStart = 65f;
			const float framerate = 24.85f;

			baldi.enabled = false;
			baldi.animator.enabled = false;
			baldi.volumeAnimator.enabled = false;

			TimeScaleModifier timeScaleMod = new(0f, 0f, 0f);
			ec.AddTimeScale(timeScaleMod);

			float elevatorDelay = 1.5f;
			bool camFovThing = false;

			for (int i = 0; i < Singleton<CoreGameManager>.Instance.setPlayers; i++)
				Singleton<CoreGameManager>.Instance.GetCamera(i).GetCustomCam().ReverseSlideFOVAnimation(new ValueModifier(), 35f, 8f);

			ValueModifier mod = new();
			while (elevatorDelay > 0f || mod.addend >= fovInitialStart)
			{
				elevatorDelay -= Time.deltaTime;
				if (!camFovThing && elevatorDelay < 0.5f)
				{
					camFovThing = true;
					for (int i = 0; i < Singleton<CoreGameManager>.Instance.setPlayers; i++)
						Singleton<CoreGameManager>.Instance.GetCamera(i).GetCustomCam().SlideFOVAnimation(mod, fovInitialStart, 10f, framerate);
				}
				yield return null;
			}

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(angryBal);

			var cam = new GameObject("BaldiAngryCamView").AddComponent<Camera>();
			cam.gameObject.AddComponent<CullAffector>();
			float fovStart = cam.fieldOfView;

			Vector3 basePosition = baldi.transform.position + (baldi.transform.forward * distanceFromBaldi);
			Vector3 startCamPos = baldi.transform.position + (baldi.transform.forward * 0.5f);
			var cell = ec.CellFromPosition(basePosition);
			if (cell.Null || cell.HasWallInDirection(Directions.DirFromVector3(baldi.transform.forward, 45f).GetOpposite()))
			{
				basePosition = baldi.transform.position - (baldi.transform.forward * distanceFromBaldi);
				startCamPos = baldi.transform.position - (baldi.transform.forward * 0.5f);
			}

			Vector3 finalCamPos = basePosition;

			cam.transform.position = startCamPos;
			cam.transform.LookAt(baldi.transform);

			float frame = 0f;
			float finalFrameIndex = angryBaldiAnimation.Length + 1.5f;
			float baseShakeSeed = Random.Range(0f, 100f);

			while (true)
			{
				if (Time.timeScale == 0f)
				{
					yield return null;
					continue;
				}

				float progress = frame / finalFrameIndex;
				float intensityMultiplier = Mathf.Clamp01(progress * 2f);

				Vector3 targetPos = Vector3.Lerp(startCamPos, finalCamPos, EaseInOutQuad(progress));
				cam.transform.position = targetPos;

				float shakeX = (Mathf.PerlinNoise(baseShakeSeed + (Time.time * shakeSpeed), 0) * 2) - 1;
				float shakeY = (Mathf.PerlinNoise(0, baseShakeSeed + (Time.time * shakeSpeed)) * 2) - 1;
				Vector3 shakeOffset = (cam.transform.right * shakeX) + (cam.transform.up * shakeY);
				cam.transform.position += shakeOffset * shakeIntensity * intensityMultiplier;

				cam.fieldOfView = Mathf.Lerp(fovStart, fovEnd, progress * progress);

				cam.transform.LookAt(baldi.transform);
				float roll = Mathf.Sin(Time.time * 40f) * maxRoll * intensityMultiplier;
				cam.transform.Rotate(0, 0, roll, Space.Self);

				if (Mathf.FloorToInt(frame % 10) == 0)
				{
					cam.transform.position += cam.transform.forward * 0.4f * intensityMultiplier;
				}

				frame += Time.deltaTime * framerate;
				if (frame >= angryBaldiAnimation.Length)
					break;

				baldi.spriteRenderer[0].sprite = angryBaldiAnimation[Mathf.FloorToInt(frame)];
				yield return null;
			}

			float punchTimer = 0f;
			while (punchTimer < 0.2f)
			{
				punchTimer += Time.deltaTime;
				cam.transform.position += cam.transform.forward * 75f * Time.deltaTime;
				cam.fieldOfView += Time.deltaTime * 120f;
				yield return null;
			}

			ec.RemoveTimeScale(timeScaleMod);
			baldi.enabled = true;
			baldi.animator.enabled = true;
			Object.Destroy(cam.gameObject);

			for (int i = 0; i < Singleton<CoreGameManager>.Instance.setPlayers; i++)
				Singleton<CoreGameManager>.Instance.GetCamera(i).GetCustomCam().ResetSlideFOVAnimation(mod, 10f, framerate); // Animation (weird way, I know)


			static float EaseInOutQuad(float t) =>
				t < 0.5f ? 2 * t * t : 1 - (Mathf.Pow((-2 * t) + 2, 2) / 2);

		}

		static void AddFire(Cell cell, EnvironmentController ec, float smoothness = 5f)
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
			f.StartCoroutine(f.Spawn(scale, smoothness));
		}

		const float fireCooldown = 3f;
		internal static LoopingSoundObject chaos0; // that very silent noise from Classic
		internal static LoopingSoundObject chaos1; // Chaos noises
		internal static LoopingSoundObject chaos2;
		internal static SoundObject angryBal;
		internal static GameObject fire;
		internal static Texture2D[] gateTextures = new Texture2D[3];
		internal static GameObject placeholderBaldi;
		internal static Sprite[] angryBaldiAnimation;
	}
}
