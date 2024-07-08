using BBTimes.CustomContent.Misc;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class FocusRoomFunction : RoomFunction
	{
		public void Setup(FocusedStudent student) =>
			this.student = student;
		public override void OnPlayerEnter(PlayerManager player)
		{
			base.OnPlayerEnter(player);
			playersToWatch.Add(player);
			playersPatience.Add(0f);
		}
		public override void OnPlayerExit(PlayerManager player)
		{
			base.OnPlayerExit(player);
			int idx = playersToWatch.IndexOf(player);
			if (idx != -1)
			{
				playersToWatch.RemoveAt(idx);
				playersPatience.RemoveAt(idx);
			}
		}

		void Update()
		{
			relaxCooldown -= room.ec.EnvironmentTimeScale * Time.deltaTime;
			if (relaxCooldown <= 0f)
			{
				student.Relax();
				relaxCooldown += 15f;
			}

			if (!student.IsSpeaking)
			{
				for (int i = 0; i < playersToWatch.Count; i++)
				{
					if (playersToWatch[i].ruleBreak == "Running" && playersToWatch[i].guiltTime > 0f)
					{
						playersPatience[i] += room.ec.EnvironmentTimeScale * Time.deltaTime;
						if (playersPatience[i] >= 1.2f)
						{
							relaxCooldown = 30f;
							playersToWatch[i].ClearGuilt();
							playersPatience[i] = 0f;
							if (student.Disturbed(playersToWatch[i]))
							{
								playersToWatch.RemoveAt(i);
								playersPatience.RemoveAt(i);
							}
							return;
						}
					}
				}
			}
		}

		float relaxCooldown = 0f;

		readonly List<PlayerManager> playersToWatch = [];
		readonly List<float> playersPatience = [];

		FocusedStudent student;
	}
}
