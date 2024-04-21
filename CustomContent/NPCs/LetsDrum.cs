using UnityEngine;
using BBTimes.CustomComponents.CustomDatas;
using System.Collections;

namespace BBTimes.CustomContent.NPCs
{
	public class LetsDrum : NPC
	{
		[SerializeField]
		internal LetsDrumCustomData dat;

		[SerializeField]
		public AudioManager musicMan;

		[SerializeField]
		public AudioManager voiceMan;

		[SerializeField]
		public AudioManager superLoudMan;

		const float speed = 15f;

		bool annoying = false;

		public override void Despawn()
		{
			if (annoying)
				AudioListener.pause = false;
			base.Despawn();
		}

		public override void Initialize()
		{
			base.Initialize();
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
			behaviorStateMachine.ChangeState(new LetsDrum_Wandering(this));
		}

		IEnumerator Annoyance()
		{
			annoying = true;
			AudioListener.pause = true;
			float cooldown = Random.Range(10f, 15f);
			bool paused = false;
			while (cooldown > 0f)
			{
				if (Singleton<CoreGameManager>.Instance.audMan.QueuedAudioIsPlaying)
					Singleton<CoreGameManager>.Instance.audMan.FlushQueue(true);

				if (Singleton<CoreGameManager>.Instance.Paused != paused)
				{
					paused = Singleton<CoreGameManager>.Instance.Paused;
					superLoudMan.ignoreListenerPause = !paused;
				}
				

				cooldown -= TimeScale * Time.deltaTime;
				yield return null;
			}

			superLoudMan.FlushQueue(true);
			AudioListener.pause = false;
			annoying = false;

			yield break;
		}

		public void BeginAnnoyance()
		{
			superLoudMan.maintainLoop = true;
			superLoudMan.SetLoop(true);
			superLoudMan.QueueAudio(dat.soundObjects[0]);
			
			StartCoroutine(Annoyance());
		}

		public void WannaDrum() =>
			voiceMan.PlaySingle(dat.soundObjects[2]);
	}

	internal class LetsDrum_Wandering(LetsDrum d) : NpcState(d)
	{
		readonly LetsDrum drum = d;

		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(drum, 0));
		}
		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			playerInSight = true;
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (active && !player.Tagged)
			{
				active = false;
				drum.WannaDrum();
				drum.StartCoroutine(WannaDrumWait(player));
			}
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			playerInSight = false;
		}

		IEnumerator WannaDrumWait(PlayerManager p)
		{
			while (drum.voiceMan.AnyAudioIsPlaying)
				yield return null;

			if (p.Tagged || !playerInSight)
			{
				drum.StartCoroutine(WaitForDrumAgain(false));
				yield break;
			}

			drum.BeginAnnoyance();
			drum.StartCoroutine(WaitForDrumAgain(true));

			yield break;
		}

		IEnumerator WaitForDrumAgain(bool playing)
		{
			if (playing)
				while (drum.superLoudMan.AnyAudioIsPlaying)
					yield return null;

			float cooldown = Random.Range(15f, 30f);
			while (cooldown > 0f)
			{
				cooldown -= drum.TimeScale * Time.deltaTime;
				yield return null;
			}
			active = true;

			yield break;
		}

		bool playerInSight = false, active = true;
	}
}
