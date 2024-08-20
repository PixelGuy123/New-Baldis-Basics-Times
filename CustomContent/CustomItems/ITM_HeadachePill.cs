using BBTimes.CustomContent.NPCs;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_HeadachePill : Item
	{
		public override bool Use(PlayerManager pm)
		{
			bool flag = false;
			for (int i = 0; i < Stunly.affectedByStunly.Count; i++)
			{
				if (Stunly.affectedByStunly[i].Value == pm)
				{
					flag = true;
					Stunly.affectedByStunly[i].Key.CancelStunEffect();
					Stunly.affectedByStunly.RemoveAt(i--);
				}
			}

			for (int i = 0; i < CameraStand.affectedByCamStand.Count; i++)
			{
				if (CameraStand.affectedByCamStand[i].Value == pm)
				{
					flag = true;
					CameraStand.affectedByCamStand[i].Key.DisableLatestTimer();
				}
			}

			foreach (var npc in pm.ec.Npcs)
			{
				if (npc is LookAtGuy && npc.behaviorStateMachine.CurrentState is LookAtGuy_Blinding blinding) // Is that C# syntax, wtf???
				{
					blinding.time = 0f; // Resets The Test's blind effect
					flag = true;
				}
			}
			if (flag)
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audSwallow);
			Destroy(gameObject);

			return flag;
		}

		[SerializeField]
		internal SoundObject audSwallow;
	}
}
