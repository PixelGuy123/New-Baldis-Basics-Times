using BBTimes.CustomComponents;
using BBTimes.CustomContent.NPCs;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_CleaningCloth : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			audUse = this.GetSound("Cloth_Clean.wav", "Vfx_CleaningCloth_Clean", SoundType.Effect, Color.white);
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string Category => "items";
		
		public ItemObject ItmObj { get; set; }



		public override bool Use(PlayerManager pm)
		{
			bool flag = false;
			for (int i = 0; i < InkArtist.affectedPlayers.Count; i++)
			{
				if (InkArtist.affectedPlayers[i].Key == pm)
				{
					InkArtist.affectedPlayers[i--].Value.CancelCameraInk();
					flag = true;
				}
			}

			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach))
			{
				var functions = pm.ec.CellFromPosition(hit.transform.parent.position).room.functions.functions;
				int idx = functions.FindIndex(x => x is Chalkboard);
				if (idx != -1)
				{
					var chalkboard = (Chalkboard)functions[idx];
					if (chalkboard.chalkboard.transform == hit.transform.parent && chalkboard.chalkFace.charge > 0f)
					{
						chalkboard.chalkFace.state.Cancel();
						chalkboard.chalkFace.charge = 0f;
						flag = true;
					}
				}
			}

			if (flag)
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);

			Destroy(gameObject);
			return flag;
		}

		[SerializeField]
		internal SoundObject audUse;
	}
}
