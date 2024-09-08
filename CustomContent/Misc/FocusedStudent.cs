using BBTimes.CustomComponents;
using BBTimes.CustomContent.NPCs;
using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.Misc
{
	public class FocusedStudent : EnvironmentObject
	{
		public override void LoadingFinished()
		{
			base.LoadingFinished();
			var room = ec.CellFromPosition(transform.position).room;
			var fun = room.functionObject.GetComponent<FocusRoomFunction>();
			if (!fun)
			{
				fun = room.functionObject.AddComponent<FocusRoomFunction>();
				room.functions.AddFunction(fun);
				fun.Initialize(room);
			}
			fun.Setup(this);
			pos = transform.position;
			initialized = true;
		}

		public bool Disturbed(PlayerManager player)
		{
			speaking = true;

			if (++disturbedCount >= 3)
			{
				shaking = true;
				renderer.sprite = sprScreaming;
				FullyRelax();
				player.RuleBreak("Running", 5f, 0.2f);
				audMan.QueueAudio(audDisturbed);
				foreach (var n in ec.Npcs)
					if (n.Navigator.enabled && (n.Character == Character.Principal || (n.GetComponent<INPCPrefab>()?.ReplacesCharacter(Character.Principal) ?? false)))
						n.behaviorStateMachine.ChangeNavigationState(new NavigationState_FollowToSpot(n, ec.CellFromPosition(player.transform.position)));
				return true;
			}
			audMan.QueueAudio(disturbedCount == 1 ? audAskSilence : audAskSilence2);
			renderer.sprite = sprSpeaking;
			return false;
		}

		public void Relax() =>
			disturbedCount = Mathf.Max(0, disturbedCount - 1);

		public void FullyRelax() =>
			disturbedCount = 0;

		void Update()
		{
			if (!initialized) return;

			if (!audMan.QueuedAudioIsPlaying)
			{
				renderer.sprite = sprNormal;
				shaking = false;
				speaking = false;
				transform.position = pos;
			}

			if (shaking)
				transform.position = pos + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
		}


		Vector3 pos;
		bool shaking = false, speaking = false, initialized = false;
		public bool IsSpeaking => speaking;
		int disturbedCount = 0;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audAskSilence, audAskSilence2, audDisturbed;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite sprNormal, sprSpeaking, sprScreaming;
	}
}
