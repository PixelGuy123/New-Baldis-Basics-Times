using BBTimes.CustomContent.NPCs;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_HeadachePill : Item
	{
		public override bool Use(PlayerManager pm)
		{
			Destroy(gameObject);
			bool flag = false;
			if (Stunly.affectedByStunly.Count != 0)
			{
				flag = true;
				while (Stunly.affectedByStunly.Count != 0)
					Stunly.affectedByStunly[0].CancelStunEffect();				
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

			return flag;
		}

		[SerializeField]
		internal SoundObject audSwallow;
	}
}
