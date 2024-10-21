using BBTimes.CustomComponents;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class ComputerTeleporter : EnvironmentObject
	{
		public override void LoadingFinished()
		{
			base.LoadingFinished();
			adjacentTeleporters.AddRange(FindObjectsOfType<ComputerTeleporter>());
			adjacentTeleporters.Remove(this);

			if (adjacentTeleporters.Count == 0)
			{
				animComp.enabled = false;
				animComp.renderer.sprite = sprDisabled;
				return;
			}
			loopingAudMan.maintainLoop = true;
			loopingAudMan.SetLoop(true);
			loopingAudMan.QueueAudio(audLoop);
			active = true;
			animComp.Initialize(ec);
		}

		void OnTriggerEnter(Collider other)
		{
			if (!active) return;
			var e = other.GetComponent<Entity>();
			if (e && !alreadyTouchedEntities.Contains(e))
				TeleportToRandomTep(e);
		}

		void OnTriggerExit(Collider other)
		{
			if (!active) return;
			var e = other.GetComponent<Entity>();
			if (e)
				alreadyTouchedEntities.Remove(e);
		}

		public void AddEntityToAlreadyTouchedOnes(Entity e) =>
			alreadyTouchedEntities.Add(e);

		void TeleportToRandomTep(Entity e)
		{
			var tep = adjacentTeleporters[Random.Range(0, adjacentTeleporters.Count)];
			tep.AddEntityToAlreadyTouchedOnes(e);

			e.Teleport(tep.transform.position);
			audMan.PlaySingle(audTeleport);
			tep.audMan.PlaySingle(tep.audTeleport);
		}

		readonly HashSet<Entity> alreadyTouchedEntities = [];
		readonly List<ComputerTeleporter> adjacentTeleporters = [];
		bool active = false;

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal PropagatedAudioManager audMan, loopingAudMan;

		[SerializeField]
		internal SoundObject audTeleport, audLoop;

		[SerializeField]
		internal Sprite sprDisabled;
	}
}
