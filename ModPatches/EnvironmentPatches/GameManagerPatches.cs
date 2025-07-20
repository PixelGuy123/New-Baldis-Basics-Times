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
			// Only comment this to make it play everywhere for testing!
			if (!__instance.levelObject.finalLevel || !___allNotebooksFound || BBTimesManager.plug.disableRedEndingCutscene.Value) return true;

			bool explorerMode = Singleton<CoreGameManager>.Instance.currentMode == Mode.Free;
			__instance.Ec.AddTimeScale(new(0f, 1f, 0f)); // make sure to... well, pause the environment smh

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

			// SETUP CAMERA AND BALDI

			PlayerVisual.GetPlayerVisual(0).gameObject.SetActive(false); // emotion 1 = sad

			Singleton<CoreGameManager>.Instance.GetCamera(0).SetControllable(false);
			Singleton<CoreGameManager>.Instance.disablePause = true;


			SoundObject slapSound = null;
			Sprite baldiSlap = null, normalBaldiSprite = null;

			var schoolBaldi = __instance.Ec.GetBaldi();
			if (schoolBaldi)
			{
				// Update volume to avoid slap noise
				schoolBaldi.AudMan.volumeMultiplier = 0f;
				schoolBaldi.AudMan.UpdateAudioDeviceVolume();

				normalBaldiSprite = baldiSlap = schoolBaldi.spriteRenderer[0].sprite;

				// Get slap sound
				slapSound = schoolBaldi.slap;

				// Get slap variant
				schoolBaldi.SlapNormal();
				baldiSlap = schoolBaldi.spriteRenderer[0].sprite;

				schoolBaldi.Navigator.Entity.SetActive(false); // We won't be using original Baldi anymore, but a copy of it
			}
			else
				explorerMode = true; // Enable the cardboard Baldi cutscene

			// SET UP VARIABLES
			Direction elevatorDir = elevator.Door.direction.GetOpposite(), elevatorFacingDir = elevator.Door.direction;
			Vector3 elvCenterPos = elevator.Door.bTile.CenterWorldPosition;
			Vector3 elvFrontPos = elevator.Door.aTile.CenterWorldPosition;
			Vector3 frontOfElevatorPos = __instance.Ec.CellFromPosition(elevator.Door.aTile.position + elevatorDir.ToIntVector2()).CenterWorldPosition; // That front of the elevator
			frontOfElevatorPos.y = explorerMode ?
			3.9f : // To fit Cardboard Baldi height
			schoolBaldi.spriteRenderer[0].transform.position.y;

			var baldi = Object.Instantiate(placeholderBaldi);
			if (!explorerMode) // If it is explorer mode, placeholderBaldi already uses cardboard visual by default
				baldi.sprite = normalBaldiSprite;

			__instance.StartCoroutine(Animation(cam, __instance, elevator));

			IEnumerator Animation(Camera cam, MainGameManager man, Elevator el)
			{
				// ---------------------------------------------------------------------
				// PHASE 1: SETUP - Prepare for the cutscene
				// ---------------------------------------------------------------------

				// Position player correctly
				player.Teleport(elvCenterPos);

				// Temporarily disable subtitles
				bool subs = Singleton<PlayerFileManager>.Instance.subtitles;
				Singleton<PlayerFileManager>.Instance.subtitles = false;

				Vector3 startPos, endPos;
				Quaternion rotStart, rotEnd;
				int maxIndex = 2;
				const float
					ENTRANCE_TO_ELEVATOR_POSITION = 1.75f,
					ENTRANCE_TO_ELEVATOR_ROTATION = 1.25f,
					ENTRANCE_TO_ELEVATOR_NOTICE_DELAY = 0.15f,

					BALDI_ENCOUNTER_NOTICE_DELAY = 0.65f,
					BALDI_ENCOUNTER_NOTICE_ROTATION = 0.28f,

					BALDI_ENCOUNTER_CONFRONT_DELAY = 1.25f,
					BALDI_ENCOUNTER_CONFRONT_PLAYERSCARE_ROTATION = 0.55f,
					BALDI_ENCOUNTER_CONFRONT_PLAYERSCARE_POSITION = 0.45f,
					BALDI_ENCOUNTER_CONFRONT_BALDI_POSITION = 0.45f,

					POST_ENCOUNTER_DELAY = 0.95f,
					POST_ENCOUNTER_ROTATION = 2.15f,

					EXPLOSION_ROTATION = 0.55f;

				float[] times = new float[maxIndex], deltas = new float[maxIndex], maxTs = new float[maxIndex]; // 0 = position; 1 = rotation
				int finishedTimes = 0;

				// Headbob settings
				float bobFrequency = 0f;
				float bobMagnitude = 0f;
				Vector3 bobOffset = Vector3.zero;

				// ---------------------------------------------------------------------
				// PHASE 2: START - Play cutscene
				// ---------------------------------------------------------------------

				// Position camera at the right spot
				cam.transform.position = elvFrontPos + elevatorDir.ToVector3() * 1.75f;
				cam.transform.rotation = elevatorFacingDir.ToRotation(); // quick note: negative x axis is downwards, positive is upwards

				// ------- FIRST LOOK -------
				bobFrequency = 4f; // Agitated bob for movement
				bobMagnitude = 0.04f;

				startPos = cam.transform.position;
				endPos = elvFrontPos + elevatorDir.ToVector3();

				rotStart = cam.transform.rotation;
				rotEnd = Quaternion.Euler(-15f, elevatorFacingDir.ToDegrees() + 12f, 0f); // Leans a bit to the right

				maxTs[0] = ENTRANCE_TO_ELEVATOR_POSITION;
				maxTs[1] = ENTRANCE_TO_ELEVATOR_ROTATION;

				// The player get a little closer to the elevator, curiously to enter it (first, facing to the right upward)
				// ANIMATION: A slight smoothness with a bounce-out for the rotation;
				while (finishedTimes < maxIndex)
				{
					finishedTimes = 0;
					for (int i = 0; i < maxIndex; i++)
					{
						times[i] += Time.deltaTime;

						if (times[i] >= maxTs[i])
							finishedTimes++;

						deltas[i] = Mathf.Clamp01(times[i] / maxTs[i]);
					}

					float posDelta = Easing.EaseInCubic(deltas[0]);
					float rotDelta = Easing.EaseOutBack(deltas[1]); // Apply bounce-out easing

					cam.transform.position = Vector3.Lerp(startPos, endPos, posDelta);
					cam.transform.rotation = Quaternion.Slerp(rotStart, rotEnd, rotDelta);

					// Apply Headbob
					bobOffset = new Vector3(0, Mathf.Sin(Time.time * bobFrequency) * bobMagnitude, 0);
					cam.transform.position += bobOffset;

					yield return null;
				}

				// ------- SECOND LOOK -------
				startPos = cam.transform.position;
				endPos = elvFrontPos - elevatorDir.ToVector3() * 1.25f;

				rotStart = cam.transform.rotation;
				// Create an intermediate rotation for the curve
				Quaternion rotMid_look2 = Quaternion.Euler(10f, rotStart.eulerAngles.y, 0f); // Looks down slightly
				rotEnd = Quaternion.Euler(-15f, elevatorFacingDir.ToDegrees() - 15f, 0f); // Leans a bit to the left this time

				for (int i = 0; i < maxIndex; i++) { times[i] = 0f; deltas[i] = 0f; }
				finishedTimes = 0;

				// The player get even more closer to the elevator, curiously to enter it (facing to the left upward, this time)
				// ANIMATION: A slight smoothness with easing-in and bounce-out for the rotation, with a small curve. Position Slerps.
				while (finishedTimes < maxIndex)
				{
					finishedTimes = 0;
					for (int i = 0; i < maxIndex; i++)
					{
						times[i] += Time.deltaTime;

						if (times[i] >= maxTs[i])
							finishedTimes++;

						deltas[i] = Mathf.Clamp01(times[i] / maxTs[i]);
					}

					float posDelta = Easing.EaseOutCubic(deltas[0]); // Smooth position movement
					float rotDelta = Easing.EaseOutBackWeak(deltas[1]); // Custom curve for rotation

					// Slerp for a more curved positional path.
					cam.transform.position = Vector3.Slerp(startPos, endPos, posDelta);

					// Slerp through the intermediate rotation to create the requested curve.
					Quaternion midPath = Quaternion.Slerp(rotStart, rotMid_look2, rotDelta);
					Quaternion endPath = Quaternion.Slerp(rotMid_look2, rotEnd, rotDelta);
					cam.transform.rotation = Quaternion.Slerp(midPath, endPath, rotDelta);

					// Apply Headbob
					bobOffset = new Vector3(0, Mathf.Sin(Time.time * bobFrequency) * bobMagnitude, 0);
					cam.transform.position += bobOffset;

					yield return null;
				}

				// ------- Walk to the ELEVATOR ------
				bobFrequency = 5f; // Faster bob for walking
				bobMagnitude = 0.06f;

				startPos = cam.transform.position;
				endPos = elvCenterPos + (elevatorDir.ToVector3() * (explorerMode ? -2.25f : 3.5f));

				rotStart = cam.transform.rotation;
				rotEnd = Quaternion.Euler(25f, elevatorFacingDir.ToDegrees(), 0f); // Faces down while walking to the elevator

				for (int i = 0; i < maxIndex; i++) { times[i] = 0f; deltas[i] = 0f; }
				finishedTimes = 0;

				maxTs[0] = ENTRANCE_TO_ELEVATOR_POSITION * 1.25f;
				maxTs[1] = ENTRANCE_TO_ELEVATOR_ROTATION * 1.55f;

				// The player gets even closer to the elevator (almost getting fully in). Then, Baldi should appear next
				// ANIMATION: Easing-out for rotation; position moves slightly faster (easing-in).
				while (finishedTimes < maxIndex)
				{
					finishedTimes = 0;
					for (int i = 0; i < maxIndex; i++)
					{
						times[i] += Time.deltaTime;

						if (times[i] >= maxTs[i])
							finishedTimes++;

						deltas[i] = Mathf.Clamp01(times[i] / maxTs[i]);
					}

					float posDelta = Easing.EaseInOutCubic(deltas[0]); // Starts slow, ends... slow lol
					float rotDelta = Easing.EaseOutCubic(deltas[1]); // Starts fast, ends slow

					cam.transform.position = Vector3.Lerp(startPos, endPos, posDelta);
					cam.transform.rotation = Quaternion.Slerp(rotStart, rotEnd, rotDelta);

					// Apply Headbob
					bobOffset = new Vector3(0, Mathf.Sin(Time.time * bobFrequency) * bobMagnitude, 0);
					cam.transform.position += bobOffset;

					yield return null;
				}

				if (!explorerMode && slapSound)
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(slapSound);

				yield return new WaitForSecondsRealtime(ENTRANCE_TO_ELEVATOR_NOTICE_DELAY);

				bobFrequency = 1.5f; // Calm bob for standing still
				bobMagnitude = 0.015f;

				Cell[] cellsToSpawnFire = [
					__instance.Ec.CellFromPosition(__instance.Ec.CellFromPosition(frontOfElevatorPos).position + elevatorDir.PerpendicularList()[0].ToIntVector2()),
					__instance.Ec.CellFromPosition(__instance.Ec.CellFromPosition(frontOfElevatorPos).position + elevatorDir.PerpendicularList()[1].ToIntVector2()),

					__instance.Ec.CellFromPosition(el.Door.aTile.position + elevatorDir.PerpendicularList()[0].ToIntVector2()),
					__instance.Ec.CellFromPosition(el.Door.aTile.position + elevatorDir.PerpendicularList()[1].ToIntVector2()),
				];

				for (int amount = 0; amount < 4; amount++)
				{
					for (int i = 0; i < cellsToSpawnFire.Length; i++)
					{
						AddFire(cellsToSpawnFire[i], __instance.Ec, 1f); // Instantly spawn fire all around for extra drama or smth
					}
				}

				// ------- NOTICE SOMETHING ------
				rotStart = cam.transform.rotation;
				rotEnd = Quaternion.Euler(0f, elevatorFacingDir.ToDegrees(), 0f); // Faces normal x

				for (int i = 0; i < maxIndex; i++) { times[i] = 0f; deltas[i] = 0f; }
				finishedTimes = 0;
				maxTs[0] = 1f;
				maxTs[1] = BALDI_ENCOUNTER_NOTICE_ROTATION;

				// The player notices something
				// ANIMATION: A very weak bounce-out for the rotation;
				while (finishedTimes < maxIndex)
				{
					finishedTimes = 0;
					for (int i = 0; i < maxIndex; i++)
					{
						times[i] += Time.deltaTime;

						if (times[i] >= maxTs[i])
							finishedTimes++;

						deltas[i] = Mathf.Clamp01(times[i] / maxTs[i]);
					}

					float rotDelta = Easing.EaseOutBack(deltas[1]);
					float weakBounceDelta = Mathf.Lerp(deltas[1], rotDelta, 0.4f); // Blend with linear for a "weak" effect

					cam.transform.rotation = Quaternion.Slerp(rotStart, rotEnd, weakBounceDelta);

					yield return null;
				}

				yield return new WaitForSecondsRealtime(BALDI_ENCOUNTER_NOTICE_DELAY * 1.15f);

				// ------- Slight turn to the left ------
				rotStart = cam.transform.rotation;
				rotEnd = Quaternion.Euler(2.5f, elevatorFacingDir.ToDegrees() - 16f, 0f); // Faces a little to the left

				for (int i = 0; i < maxIndex; i++) { times[i] = 0f; deltas[i] = 0f; }
				finishedTimes = 0;
				maxTs[0] = 1f;
				maxTs[1] = BALDI_ENCOUNTER_NOTICE_ROTATION * 0.85f;

				// The player slightly turns left
				// ANIMATION: A normal easing-in very weak bounce-out for the rotation;
				while (finishedTimes < maxIndex)
				{
					finishedTimes = 0;
					for (int i = 0; i < maxIndex; i++)
					{
						times[i] += Time.deltaTime;

						if (times[i] >= maxTs[i])
							finishedTimes++;

						deltas[i] = Mathf.Clamp01(times[i] / maxTs[i]);
					}

					float rotDelta = Easing.EaseOutBackWeak(deltas[1]); // Custom blend

					cam.transform.rotation = Quaternion.Slerp(rotStart, rotEnd, rotDelta);

					yield return null;
				}

				yield return new WaitForSecondsRealtime(BALDI_ENCOUNTER_NOTICE_DELAY * 0.56f);

				// Spawn Baldi here
				baldi.transform.position = frontOfElevatorPos;

				// ------- Full Turn to Baldi ------
				startPos = cam.transform.position; // Position doesn't change here

				rotStart = cam.transform.rotation;
				rotEnd = Quaternion.Euler(0f, elevatorDir.ToDegrees(), 0f); // Dynamically look at Baldi

				for (int i = 0; i < maxIndex; i++) { times[i] = 0f; deltas[i] = 0f; }
				finishedTimes = 0;
				maxTs[0] = 1f;
				maxTs[1] = BALDI_ENCOUNTER_NOTICE_ROTATION * 1.25f;

				// The player fully turns back and see Baldi!
				// ANIMATION: A normal easing-in very weak bounce-out for the rotation;
				while (finishedTimes < maxIndex)
				{
					finishedTimes = 0;
					for (int i = 0; i < maxIndex; i++)
					{
						times[i] += Time.deltaTime;

						if (times[i] >= maxTs[i])
							finishedTimes++;

						deltas[i] = Mathf.Clamp01(times[i] / maxTs[i]);
					}

					float rotDelta = Easing.EaseOutBackWeak(deltas[1]); // Reuse custom blend

					cam.transform.position = startPos;
					cam.transform.rotation = Quaternion.Slerp(rotStart, rotEnd, rotDelta);

					yield return null;
				}

				yield return new WaitForSecondsRealtime(BALDI_ENCOUNTER_CONFRONT_DELAY);

				// ------- Backing off by a bit ------
				bobFrequency = 6f; // Scared, agitated bob
				bobMagnitude = 0.08f;

				if (explorerMode)
					goto explorerModeSkip;

				baldi.sprite = baldiSlap;
				if (slapSound)
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(slapSound);

				Vector3 baldi_startPos = baldi.transform.position;
				Vector3 baldi_endPos = elvFrontPos;
				baldi_endPos.y = baldi_startPos.y;

				maxIndex = 3;
				times = new float[maxIndex]; deltas = new float[maxIndex]; maxTs = new float[maxIndex];

				startPos = cam.transform.position;
				endPos = elvCenterPos - elevatorDir.ToVector3() * 2.25f;

				rotStart = cam.transform.rotation;
				rotEnd = Quaternion.Euler(25f, elevatorDir.ToDegrees() + 80f, 0f);

				for (int i = 0; i < maxIndex; i++) { times[i] = 0f; deltas[i] = 0f; }
				finishedTimes = 0;

				maxTs[0] = BALDI_ENCOUNTER_CONFRONT_PLAYERSCARE_POSITION;
				maxTs[1] = BALDI_ENCOUNTER_CONFRONT_PLAYERSCARE_ROTATION;
				maxTs[2] = BALDI_ENCOUNTER_CONFRONT_BALDI_POSITION;

				// The player looks away scared as Baldi, then, tries to get inside
				// ANIMATION: Cam position is linear. Rotation is smooth ease-in weak bounce-out. Baldi is linear.
				while (finishedTimes < maxIndex)
				{
					finishedTimes = 0;
					for (int i = 0; i < maxIndex; i++)
					{
						times[i] += Time.deltaTime;

						if (times[i] >= maxTs[i])
							finishedTimes++;

						deltas[i] = Mathf.Clamp01(times[i] / maxTs[i]);
					}

					if (deltas[1] >= 0.85f && el.Door.IsOpen)
						el.OpenDoor(false);


					float rotDelta = Easing.EaseOutBackWeak(deltas[1]); // Apply smooth curve to rotation

					cam.transform.position = Vector3.Lerp(startPos, endPos, deltas[0]);
					cam.transform.rotation = Quaternion.Slerp(rotStart, rotEnd, rotDelta);
					baldi.transform.position = Vector3.Lerp(baldi_startPos, baldi_endPos, deltas[2]);

					yield return null;
				}

				yield return new WaitForSecondsRealtime(POST_ENCOUNTER_DELAY);

				// ------- Slightly lean a bit ------
				int bangs = 0;
				float bangDelay = 0f;
				const float minBangAdd = 0.25f, maxBangAdd = 0.45f, bangMaxDelay = 0.35f;

				bobFrequency = 1.0f; // Calming down
				bobMagnitude = 0.02f;

				maxIndex = 1;
				bangs = 0;
				bangDelay = 0f;

				times = new float[maxIndex]; deltas = new float[maxIndex]; maxTs = new float[maxIndex];

				startPos = cam.transform.position;
				startPos.y = 5f;

				rotStart = cam.transform.rotation;
				rotEnd = Quaternion.Euler(9.5f, elevatorDir.ToDegrees() + 65f, 0f);

				for (int i = 0; i < maxIndex; i++) { times[i] = 0f; deltas[i] = 0f; }
				finishedTimes = 0;
				maxTs[0] = POST_ENCOUNTER_ROTATION * 0.25f;

				// The player slowly leans a bit up to realize he's still alive
				// ANIMATION: A normal easing-in-out for the rotation.
				while (finishedTimes < maxIndex || bangs < 4)
				{
					finishedTimes = 0;
					times[0] += Time.deltaTime;

					if (times[0] >= maxTs[0])
						finishedTimes++;

					deltas[0] = Mathf.Clamp01(times[0] / maxTs[0]);
					float rotDelta = Easing.EaseInOutCubic(deltas[0]); // Smooth in and out

					cam.transform.rotation = Quaternion.Slerp(rotStart, rotEnd, rotDelta);

					bangDelay += Time.deltaTime;
					if (bangDelay > bangMaxDelay)
					{
						el.audMan.PlaySingle(bal_bangDoor);
						bangDelay -= Random.Range(minBangAdd, maxBangAdd);
						bangs++;
					}

					// Apply Headbob
					bobOffset = new Vector3(0, Mathf.Sin(Time.time * bobFrequency) * bobMagnitude, 0);
					cam.transform.position = startPos + bobOffset;

					yield return null;
				}

				yield return new WaitForSecondsRealtime(POST_ENCOUNTER_DELAY * 0.65f);

				// ------- Turn back completely ------
				rotStart = cam.transform.rotation;
				rotEnd = Quaternion.Euler(0f, elevatorDir.ToDegrees(), 0f);

				// Create an intermediate point for the parabolic curve
				Vector3 midEuler = Vector3.Lerp(rotStart.eulerAngles, rotEnd.eulerAngles, 0.5f);
				midEuler.x = Mathf.Max(rotStart.eulerAngles.x, rotEnd.eulerAngles.x) + 20f; // Dip down
				Quaternion rotMid = Quaternion.Euler(midEuler);

				for (int i = 0; i < maxIndex; i++) { times[i] = 0f; deltas[i] = 0f; }
				bangs = 0;
				bangDelay = 0f;
				finishedTimes = 0;
				maxTs[0] = POST_ENCOUNTER_ROTATION * 0.85f;

				// The player slowly looks at the elevator door
				// ANIMATION: Parabolic curve for rotation, easing in and out.
				while (finishedTimes < maxIndex || bangs < 10)
				{
					finishedTimes = 0;

					times[0] += Time.deltaTime;
					if (times[0] < maxTs[0])
					{
						// Apply Headbob in here, so the position of the camera can be properly resetted
						bobOffset = new Vector3(0, Mathf.Sin(Time.time * bobFrequency) * bobMagnitude, 0);
						cam.transform.position = startPos + bobOffset;
					}
					else
					{
						finishedTimes++;
						cam.transform.position = startPos;
					}
					deltas[0] = Mathf.Clamp01(times[0] / maxTs[0]);

					float rotDelta = Easing.EaseInOutCubic(deltas[0]);

					// Quadratic Bezier curve using Slerp for a smooth parabolic path
					Quaternion part1 = Quaternion.Slerp(rotStart, rotMid, rotDelta);
					Quaternion part2 = Quaternion.Slerp(rotMid, rotEnd, rotDelta);
					cam.transform.rotation = Quaternion.Slerp(part1, part2, rotDelta);

					bangDelay += Time.deltaTime;
					if (bangDelay > bangMaxDelay)
					{
						el.audMan.PlaySingle(bal_bangDoor);
						bangDelay -= Random.Range(minBangAdd, maxBangAdd);
						bangs++;
					}

					yield return null;
				}

				cam.transform.position = startPos;

				// ---- All subsequent loops will use EaseInOutCubic for a smooth, standard feel ----
				// No more head bob :()
				el.audMan.PlaySingle(bal_explosionOutside);
				__instance.StartCoroutine(TriggerExplosions());

			explorerModeSkip:

				yield return new WaitForSecondsRealtime(POST_ENCOUNTER_DELAY * (explorerMode ? 2.25f : 0.25f));

				//  -------Turn a bit to the right while hearing the explosion -------
				if (explorerMode) // Close the door here
				{
					maxIndex = 1;
					startPos = cam.transform.position;
					startPos.y = 5f;
					if (el.Door.IsOpen)
						el.OpenDoor(false);
				}

				rotStart = cam.transform.rotation;
				rotEnd = Quaternion.Euler(-7f, elevatorDir.ToDegrees() + 15f, 0f);
				for (int i = 0; i < maxIndex; i++) { times[i] = 0f; deltas[i] = 0f; }
				maxTs[0] = EXPLOSION_ROTATION * 0.35f;

				finishedTimes = 0;

				while (finishedTimes < maxIndex)
				{
					for (int i = 0; i < maxIndex; i++)
					{
						times[i] += Time.deltaTime;

						if (times[i] >= maxTs[i])
							finishedTimes++;

						deltas[i] = Mathf.Clamp01(times[i] / maxTs[i]);
					}
					cam.transform.rotation = Quaternion.Slerp(rotStart, rotEnd, Easing.EaseInOutCubic(deltas[0]));
					yield return null;
				}

				yield return new WaitForSecondsRealtime(POST_ENCOUNTER_DELAY * 0.75f);

				// ------- Turn a bit to the left while hearing the explosion -------
				rotStart = cam.transform.rotation;
				rotEnd = Quaternion.Euler(-7f, elevatorDir.ToDegrees() - 15f, 0f);
				for (int i = 0; i < maxIndex; i++) { times[i] = 0f; deltas[i] = 0f; }
				maxTs[0] = EXPLOSION_ROTATION;

				finishedTimes = 0;

				while (finishedTimes < maxIndex)
				{
					for (int i = 0; i < maxIndex; i++)
					{
						times[i] += Time.deltaTime;

						if (times[i] >= maxTs[i])
							finishedTimes++;

						deltas[i] = Mathf.Clamp01(times[i] / maxTs[i]);
					}
					cam.transform.rotation = Quaternion.Slerp(rotStart, rotEnd, Easing.EaseInOutCubic(deltas[0]));
					yield return null;
				}

				yield return new WaitForSecondsRealtime(POST_ENCOUNTER_DELAY * 2.5f);

				// ----- Reset camera rotation once again -----
				rotStart = cam.transform.rotation;
				rotEnd = Quaternion.Euler(0f, elevatorDir.ToDegrees(), 0f);
				for (int i = 0; i < maxIndex; i++) { times[i] = 0f; deltas[i] = 0f; }
				maxTs[0] = explorerMode ? 0.85f : 2.75f;

				finishedTimes = 0;

				while (finishedTimes < maxIndex)
				{
					for (int i = 0; i < maxIndex; i++)
					{
						times[i] += Time.deltaTime;

						if (times[i] >= maxTs[i])
							finishedTimes++;

						deltas[i] = Mathf.Clamp01(times[i] / maxTs[i]);
					}
					cam.transform.rotation = Quaternion.Slerp(rotStart, rotEnd, Easing.EaseInOutCubic(deltas[0]));
					yield return null;
				}

				if (explorerMode)
					yield return new WaitForSecondsRealtime(0.75f);

				while (el.audMan.AnyAudioIsPlaying)
					yield return null;


				// yield return new WaitForSecondsRealtime(9999f); // Intentional debug yield to freeze the cutscene for long enough

				// ---------------------------------------------------------------------
				// PHASE 3: CLEANUP & TRANSITION
				// ---------------------------------------------------------------------

				var ogCam = Singleton<CoreGameManager>.Instance.GetCamera(0);
				ogCam.transform.position = cam.transform.position;
				ogCam.transform.rotation = cam.transform.rotation;
				player.transform.position = ogCam.transform.position;
				player.transform.rotation = ogCam.transform.rotation;
				Singleton<MusicManager>.Instance.StopMidi();
				Singleton<MusicManager>.Instance.StopFile();

				Singleton<PlayerFileManager>.Instance.subtitles = subs;
				Object.Destroy(cam); // Only the camera lol

				PlayerVisual.GetPlayerVisual(0).gameObject.SetActive(true);
				PlayerVisual.GetPlayerVisual(0).SetEmotion(0);

				LoadNextLevel(man);

				// IENUMERATOR HELPERS
				IEnumerator TriggerExplosions()
				{
					// Re-utilize bangs variable here to trigger many explosion sounds at random intervals
					bangs = 0;
					bangDelay = 0f;
					int max = Random.Range(3, 5);
					while (bangs <= max)
					{
						bangDelay += Time.deltaTime;
						if (bangDelay > 0.65f)
						{
							bangDelay -= 0.65f + Random.Range(-0.25f, 0.95f);
							el.audMan.PlaySingle(bal_explosionOutside);
							bangs++;
						}
						yield return null;
					}
				}
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
				// Workaround for factory
				var levelbox = Object.FindObjectOfType<Structure_LevelBox>();
				if (levelbox)
				{
					foreach (var meshRenderer in levelbox.GetComponentsInChildren<MeshRenderer>())
					{
						if (meshRenderer.material.shader.name == "Shader Graphs/Standard")
							meshRenderer.material.SetColor("_TextureColor", Color.red);
					}
				}

				Singleton<MusicManager>.Instance.SetSpeed(0.1f);
				__instance.StartCoroutine(___ec.LightChanger(___ec.AllExistentCells(), 0.2f));
				if (__instance.levelObject.finalLevel)
					Singleton<MusicManager>.Instance.QueueFile(chaos0, true);
				return;
			}
			if (___elevatorsClosed == 2)
			{
				___ec.SetTimeLimit(9999f);

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
				___ec.StartCoroutine(SpawnFires());
				if (baldiToFollow)
					___ec.StartCoroutine(DangerousAngryBaldiAnimation(___ec, baldiToFollow));


			}



			IEnumerator SpawnFires()
			{
				float cooldown = fireCooldown;
				float maxCooldown = fireCooldown;
				var cs = ___ec.AllTilesNoGarbage(false, true);

				foreach (var elevator in ___ec.elevators) // Fire shouldn't spawn in the elevators' entrances
				{
					cs.Remove(elevator.Door.aTile);
					cs.Remove(___ec.CellFromPosition(elevator.Door.aTile.position + elevator.Door.direction.PerpendicularList()[0].ToIntVector2()));
					cs.Remove(___ec.CellFromPosition(elevator.Door.aTile.position + elevator.Door.direction.PerpendicularList()[1].ToIntVector2()));
				}

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

			var baldiEntity = baldi.Navigator.Entity;
			baldiEntity.SetVisible(false);

			var animatedBaldi = Object.Instantiate(placeholderBaldi);
			animatedBaldi.sprite = angryBaldiAnimation[0];
			animatedBaldi.transform.position = baldiEntity.transform.position;

			TimeScaleModifier timeScaleMod = new(0f, 0f, 0f); // This should pause Baldi slap and literally anything
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

			Vector3 basePosition = animatedBaldi.transform.position + (animatedBaldi.transform.forward * distanceFromBaldi);
			Vector3 startCamPos = animatedBaldi.transform.position + (animatedBaldi.transform.forward * 0.5f);
			var cell = ec.CellFromPosition(basePosition);
			if (cell.Null || cell.HasWallInDirection(Directions.DirFromVector3(animatedBaldi.transform.forward, 45f).GetOpposite()))
			{
				basePosition = animatedBaldi.transform.position - (animatedBaldi.transform.forward * distanceFromBaldi);
				startCamPos = animatedBaldi.transform.position - (animatedBaldi.transform.forward * 0.5f);
			}

			Vector3 finalCamPos = basePosition;

			cam.transform.position = startCamPos;
			cam.transform.LookAt(animatedBaldi.transform);

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

				cam.transform.LookAt(animatedBaldi.transform);
				float roll = Mathf.Sin(Time.time * 40f) * maxRoll * intensityMultiplier;
				cam.transform.Rotate(0, 0, roll, Space.Self);

				if (Mathf.FloorToInt(frame % 10) == 0)
				{
					cam.transform.position += cam.transform.forward * 0.4f * intensityMultiplier;
				}

				frame += Time.deltaTime * framerate;
				if (frame >= angryBaldiAnimation.Length)
					break;

				animatedBaldi.sprite = angryBaldiAnimation[Mathf.FloorToInt(frame)];
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
			baldiEntity.SetVisible(true);

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
		internal static SpriteRenderer placeholderBaldi;
		internal static Sprite cardboardBaldi;
		internal static SoundObject bal_bangDoor, bal_explosionOutside;
		internal static Sprite[] angryBaldiAnimation;
	}

	static class Easing
	{
		// I don't know where to put this, so I'll put right below the main patch class.
		// This has a BUNCH of ease methods for bouncing and stuff

		private const float s = 1.70158f; // Standard overshoot value for ease back

		public static float EaseInBack(float t)
		{
			return t * t * ((s + 1) * t - s);
		}

		public static float EaseOutBack(float t)
		{
			return 1 + ((--t) * t * ((s + 1) * t + s));
		}

		public static float EaseInOutQuad(float t)
		{
			return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
		}

		public static float EaseOutCubic(float t)
		{
			return (--t) * t * t + 1;
		}

		public static float EaseInCubic(float t)
		{
			return t * t * t;
		}

		public static float EaseInOutCubic(float t)
		{
			return t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
		}

		// Custom blend for a soft start and a weak bounce at the end
		public static float EaseOutBackWeak(float t)
		{
			// Lerp between a smooth ease-in and a bounce-out based on progress
			return Mathf.Lerp(EaseInCubic(t), EaseOutBack(t), t);
		}
	}
}
