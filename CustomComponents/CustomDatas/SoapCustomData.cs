using BBTimes.CustomContent.CustomItems;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class SoapCustomData : CustomItemData
	{
		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(25f, "soap.png")];
		protected override SoundObject[] GenerateSoundObjects() =>
			[GenericExtensions.FindResourceObjectByName<SoundObject>("Nana_Loop"),
		GenericExtensions.FindResourceObjectByName<SoundObject>("Nana_Slip"),
		BBTimesManager.man.Get<SoundObject>("audGenericPunch")];
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var soap = GetComponent<ITM_Soap>();
			soap.gameObject.layer = LayerStorage.standardEntities;

			var soapRenderer = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]);
			soapRenderer.transform.SetParent(soap.transform);
			soapRenderer.transform.localPosition = Vector3.zero;

			soap.entity = gameObject.CreateEntity(2.5f, 3.5f, soapRenderer.transform);
			soap.renderer = soapRenderer.transform;
			soap.audMan = gameObject.CreatePropagatedAudioManager(65f, 85f);
			soap.audThrow = soundObjects[1];
			soap.audRunLoop = soundObjects[0];
			soap.audHit = soundObjects[2];

		}
	}
}
