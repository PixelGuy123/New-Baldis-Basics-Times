using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class HardHatCustomData : CustomItemData
	{
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(transform);
			canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			canvas.name = "hardHatOverlay";
			ObjectCreationExtensions.CreateImage(canvas, storedSprites[0]);
			GetComponent<ITM_HardHat>().canvas = canvas;
		}
	}
}
