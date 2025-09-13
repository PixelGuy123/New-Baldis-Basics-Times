using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class Toilet : EnvironmentObject, IClickable<int>
	{
		[SerializeField]
		internal SoundObject audFlush;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal float flushingDelay = 30f;

		[SerializeField]
		internal int minDistanceToAlertNPCs = 20;

		DijkstraMap map;

		float flushDelay = 0f;

		List<NavigationState_WanderFleeOverride> states = [];

		public override void LoadingFinished()
		{
			base.LoadingFinished();
			map = new(ec, PathType.Sound, int.MaxValue, transform);
		}

		public void Clicked(int player)
		{
			if (audMan.QueuedAudioIsPlaying || flushDelay > 0f)
				return;

			flushDelay = flushingDelay;
			audMan.PlaySingle(audFlush);

			map.Calculate();

			var npcs = ec.Npcs;
			for (int i = 0; i < npcs.Count; i++)
			{
				if (!npcs[i].Navigator.isActiveAndEnabled || !npcs[i].Entity.InBounds)
					continue;

				if (map.Value(IntVector2.GetGridPosition(npcs[i].transform.position)) <= minDistanceToAlertNPCs)
				{
					NavigationState_WanderFleeOverride overrideState = new(npcs[i], 31, map);
					states.Add(overrideState);
					npcs[i].navigationStateMachine.ChangeState(overrideState);
				}
			}

			StartCoroutine(Cooldown());
		}

		IEnumerator Cooldown()
		{
			map.Activate();
			map.QueueUpdate();

			float runningAwayTimer = flushingDelay * 0.5f;
			while (runningAwayTimer > 0f)
			{
				flushDelay -= ec.EnvironmentTimeScale * Time.deltaTime;
				runningAwayTimer -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			for (int i = 0; i < states.Count; i++)
				states[i].End();

			states.Clear();
			map.Deactivate();

			while (flushDelay > 0f)
			{
				flushDelay -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			yield break;
		}

		public bool ClickableHidden() => audMan.QueuedAudioIsPlaying || flushDelay > 0f;
		public bool ClickableRequiresNormalHeight() => true;
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }

	}
}
