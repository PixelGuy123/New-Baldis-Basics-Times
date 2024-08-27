using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Screwdriver : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			audScrew = this.GetSound("sd_screw.wav", "Vfx_SD_screw", SoundType.Effect, Color.white);
			item = ItmObj.itemType;
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			Destroy(gameObject);

			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach, pm.pc.ClickLayers))
			{
				var math = hit.transform.GetComponentInParent<MathMachine>();
				if (math && !math.IsCompleted)
				{
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audScrew);
					math.Completed(pm.playerNumber, true, math);
					return true;
				}
				var machine = hit.transform.GetComponent<IItemAcceptor>();
				if (machine != null && machine.ItemFits(item))
				{
					machine.InsertItem(pm, pm.ec);
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audScrew);
					return true;
				}
			}

			return false;
		}

		[SerializeField]
		internal SoundObject audScrew;

		[SerializeField]
		internal Items item;
	}
}
