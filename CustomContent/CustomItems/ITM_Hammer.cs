using BBTimes.CustomComponents;
using UnityEngine;
using PixelInternalAPI.Classes;
using BBTimes.Extensions;

namespace BBTimes.CustomContent.CustomItems
{
    public class ITM_Hammer : Item, IItemPrefab
    {
		public void SetupPrefab()
		{
			item = ItmObj.itemType;
		}

		public void SetupPrefabPost(){}

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }
		public override bool Use(PlayerManager pm)
        {
            Destroy(gameObject);
            if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var raycastHit, pm.pc.reach, LayerStorage.windowLayer, QueryTriggerInteraction.Collide) && raycastHit.transform.CompareTag("Window"))
            {
				var w = raycastHit.transform.GetComponent<Window>();
				bool broken = false;
				if (w)
				{ 
					w.Break(true);
					broken = !raycastHit.transform.GetComponent<CustomWindowComponent>()?.unbreakable ?? true;
					if (broken)
						pm.RuleBreak("breakingproperty", 3f, 0.15f);
				}
				return broken;
            }

			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach, 131072)) // Npc layer I guess? Not sure, it was from the scissors
			{
				IItemAcceptor component = hit.transform.GetComponent<IItemAcceptor>();
				if (component != null && component.ItemFits(item))
				{
					component.InsertItem(pm, pm.ec);
					return true;
				}
			}

			return false;
        }

		[SerializeField]
		internal Items item;
	}
}
