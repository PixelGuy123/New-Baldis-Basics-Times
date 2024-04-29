using BBTimes.CustomContent.CustomItems;
using BBTimes.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class HardHatCustomData : CustomItemData
	{
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var canvas = Instantiate(BBTimesManager.man.Get<Canvas>("CanvasPrefab"));
			canvas.transform.SetParent(transform);
			canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			canvas.name = "hardHatOverlay";
			canvas.GetComponentInChildren<Image>().sprite = storedSprites[0]; // stunly stare moment
			GetComponent<ITM_HardHat>().canvas = canvas;
		}
	}
}
