using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomComponents;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class ComputerTeleporter : EnvironmentObject
	{
		public void EnableMachine(bool enable)
		{
			alreadyTouchedEntities.Clear();
			teleporting = false;
			DisableTeleportSequence();

			if (enable)
			{
				animComp.animation = sprEnabled;
				animComp.ResetFrame(true);
				loopingAudMan.maintainLoop = true;
				loopingAudMan.SetLoop(true);
				loopingAudMan.QueueAudio(audLoop);

				loopingAudMan.pitchModifier = 1f;
				return;
			}

			animComp.animation = sprDisabled;
			animComp.ResetFrame(true);
			loopingAudMan.FlushQueue(true);
		}
		public override void LoadingFinished()
		{
			base.LoadingFinished();
			adjacentTeleporters.AddRange(FindObjectsOfType<ComputerTeleporter>());
			adjacentTeleporters.Remove(this);

			animComp.Initialize(ec);

			if (adjacentTeleporters.Count == 0)
			{
				EnableMachine(false);
				return;
			}
			loopingAudMan.maintainLoop = true;
			loopingAudMan.SetLoop(true);
			loopingAudMan.QueueAudio(audLoop);
			active = true;
		}

		void OnTriggerStay(Collider other)
		{
			if (!other.isTrigger || !active || teleporting) return;
			var e = other.GetComponent<Entity>();
			if (e && !alreadyTouchedEntities.Contains(e))
			{
				alreadyTouchedEntities.Add(e);

				DisableTeleportSequence();
				teleportCor = StartCoroutine(TeleportToRandomTep(e));

				if (other.CompareTag("NPC"))
					e.ExternalActivity.moveMods.Add(moveMod); // To make sure they actually stay to be teleported
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (!other.isTrigger) return;

			var e = other.GetComponent<Entity>();
			if (e)
			{
				alreadyTouchedEntities.Remove(e);
				if (other.CompareTag("NPC"))
					e.ExternalActivity.moveMods.Remove(moveMod); // To make sure they actually stay to be teleported
			}
		}

		void DisableTeleportSequence()
		{
			if (activeEntity)
			{
				activeEntity.ExternalActivity.moveMods.Remove(moveMod);
				alreadyTouchedEntities.Remove(activeEntity);
				activeEntity = null;
			}

			if (teleportCor != null)
				StopCoroutine(teleportCor);
		}

		IEnumerator TeleportToRandomTep(Entity e)
		{
			activeEntity = e;
			teleporting = true;
			float t = 0f;

			yield return null;

			while (t < timeToTeleport)
			{
				t += Time.deltaTime * ec.EnvironmentTimeScale;
				loopingAudMan.pitchModifier = Mathf.Lerp(1f, maxPitchBeforeTeleporting, t / timeToTeleport);

				if (!alreadyTouchedEntities.Contains(e))
				{
					loopingAudMan.pitchModifier = 1f;
					DisableTeleportSequence();
					yield break;
				}

				yield return null;
			}

			loopingAudMan.pitchModifier = 1f;

			var tep = adjacentTeleporters[Random.Range(0, adjacentTeleporters.Count)];
			tep.alreadyTouchedEntities.Add(e); // Adds to the next teleporter to prevent teleporting back

			yield return null; // Wait a frame to avoid teleporting twice

			e.Teleport(tep.transform.position);
			audMan.PlaySingle(audTeleport);
			tep.audMan.PlaySingle(tep.audTeleport);

			alreadyTouchedEntities.Remove(e); // Removes this

			yield return null;

			teleporting = false;
			activeEntity = null;
		}

		void OnDisable()
		{
			DisableTeleportSequence();
		}

		readonly HashSet<Entity> alreadyTouchedEntities = [];
		readonly List<ComputerTeleporter> adjacentTeleporters = [];
		bool active = false, teleporting = false;
		Coroutine teleportCor;

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal PropagatedAudioManager audMan, loopingAudMan;

		[SerializeField]
		internal SoundObject audTeleport, audLoop;

		[SerializeField]
		internal Sprite[] sprDisabled;

		[SerializeField]
		internal Sprite[] sprEnabled;

		[SerializeField]
		internal float pitchSpeedPerTeleport = 1.25f, maxPitchBeforeTeleporting = 1.75f, timeToTeleport = 1f;

		MovementModifier moveMod = new(Vector3.zero, 0.25f);
		Entity activeEntity;
	}
}
