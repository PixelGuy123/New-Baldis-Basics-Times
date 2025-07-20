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
			if (enable)
			{
				animComp.animation = sprEnabled;
				animComp.ResetFrame(true);
				loopingAudMan.maintainLoop = true;
				loopingAudMan.SetLoop(true);
				loopingAudMan.QueueAudio(audLoop);

				teleporting = false;
				audMan.pitchModifier = 1f;
				return;
			}

			if (teleportCor != null)
				StopCoroutine(teleportCor);

			animComp.ChangeRendererSpritesTo(sprDisabled);
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
			if (!active || teleporting) return;
			var e = other.GetComponent<Entity>();
			if (e && !alreadyTouchedEntities.Contains(e))
			{
				alreadyTouchedEntities.Add(e);

				if (teleportCor != null)
					StopCoroutine(teleportCor);
				teleportCor = StartCoroutine(TeleportToRandomTep(e));
			}
		}

		void OnTriggerExit(Collider other)
		{
			var e = other.GetComponent<Entity>();
			if (e)
				alreadyTouchedEntities.Remove(e);
		}

		public void AddEntityToAlreadyTouchedOnes(Entity e) =>
			alreadyTouchedEntities.Add(e);

		IEnumerator TeleportToRandomTep(Entity e)
		{
			teleporting = true;
			float t = 0f;

			while (t < timeToTeleport)
			{
				t += Time.deltaTime * ec.EnvironmentTimeScale;
				audMan.pitchModifier = Mathf.Lerp(1f, maxPitchBeforeTeleporting, t / timeToTeleport);

				if (!alreadyTouchedEntities.Contains(e))
				{
					audMan.pitchModifier = 1f;
					yield break;
				}

				yield return null;
			}

			audMan.pitchModifier = 1f;

			var tep = adjacentTeleporters[Random.Range(0, adjacentTeleporters.Count)];
			tep.AddEntityToAlreadyTouchedOnes(e);

			yield return null; // Wait a frame to avoid teleporting twice

			e.Teleport(tep.transform.position);
			audMan.PlaySingle(audTeleport);
			tep.audMan.PlaySingle(tep.audTeleport);

			teleporting = false;
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
		internal Sprite sprDisabled;

		[SerializeField]
		internal Sprite[] sprEnabled;

		[SerializeField]
		internal float pitchSpeedPerTeleport = 1.25f, maxPitchBeforeTeleporting = 1.75f, timeToTeleport = 1f;
	}
}
