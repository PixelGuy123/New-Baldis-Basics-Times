using BBTimes.CustomComponents;
using BBTimes.CustomContent.NPCs;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_HeadachePill : Item, IItemPrefab
	{
		public void SetupPrefab() =>
			audSwallow = this.GetSound("swallow.wav", "HDP_Swallow", SoundType.Effect, Color.white);
		
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }
		public override bool Use(PlayerManager pm)
		{
			bool flag = false;

			for (int i = 0; i < Adverto.affectedPlayers.Count; i++)
			{
				if (Adverto.affectedPlayers[i].Value == pm)
				{
					Adverto.affectedPlayers[i].Key.CleanUpAds();
					flag = true;
				}
			}

			for (int i = 0; i < Quiker.affectedPlayers.Count; i++)
			{
				if (Quiker.affectedPlayers[i].Key.Key == pm)
				{
					Quiker.affectedPlayers[i].Key.Key.Am.moveMods.Remove(Quiker.affectedPlayers[i].Value.Key);
					pm.ec.RemoveFog(Quiker.affectedPlayers[i].Value.Value.Key);
					if (Quiker.affectedPlayers[i].Value.Value.Value != null)
						Quiker.affectedPlayers[i].Key.Value.StopCoroutine(Quiker.affectedPlayers[i].Value.Value.Value);
					Quiker.affectedPlayers.RemoveAt(i--);
					flag = true;
				}
			}

			for (int i = 0; i < Stunly.affectedByStunly.Count; i++)
			{
				if (Stunly.affectedByStunly[i].Value == pm)
				{
					flag = true;
					Stunly.affectedByStunly[i].Key.CancelStunEffect();
					i--;
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
