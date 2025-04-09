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
			RaycastHit hit;
			if (cleanClothItem) // If item is not null, this is a dirty cloth
			{
				Destroy(gameObject);
				if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.reach))
				{
					var water = hit.transform.GetComponent<WaterFountain>();
					if (water)
					{
						this.pm = pm;
						InteractWithFountain(water);
						pm.itm.SetItem(cleanClothItem, pm.itm.selectedItem);
					}
				}
				return false;
			}

			bool flag = false;
			for (int i = 0; i < InkArtist.affectedPlayers.Count; i++)
			{
				if (InkArtist.affectedPlayers[i].Key == pm)
				{
					InkArtist.affectedPlayers[i--].Value.CancelCameraInk();
					flag = true;
				}
			}

			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.reach))
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

			Destroy(gameObject);

			if (flag)
			{
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
				pm.itm.SetItem(dirtyClothItem, pm.itm.selectedItem);
			}
			
			return false;
		}

		void InteractWithFountain(WaterFountain fountain)
		{
			fountain.audMan.PlaySingle(fountain.audSip);
		} // will be used to be patched by BB+ Animations aswell

		[SerializeField]
		internal SoundObject audUse;

		[SerializeField]
		internal ItemObject cleanClothItem = null, dirtyClothItem = null;
	}
}
