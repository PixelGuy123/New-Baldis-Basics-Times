﻿using UnityEngine;
using TMPro;
using BBTimes.CustomContent.Builders;
using BBTimes.Extensions;
using BBTimes.CustomContent.Objects;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class TrapdoorBuilderCustomData : CustomObjectPrefabData
	{
		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(2, 2, 25f, "traps.png");

		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("trapDoor_shut.wav", "Sfx_Doors_StandardShut", SoundType.Voice, Color.white),
			GetSound("trapDoor_open.wav", "Sfx_Doors_StandardOpen", SoundType.Voice, Color.white)];
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var trapdoorholder = new GameObject("TrapDoor").AddComponent<Trapdoor>();
			trapdoorholder.gameObject.ConvertToPrefab(true);
			

			var text = new GameObject("TrapdoorText").AddComponent<TextMeshPro>();
			text.gameObject.layer = LayerStorage.billboardLayer;
			text.transform.SetParent(trapdoorholder.transform);
			text.transform.localPosition = Vector3.up * 0.02f;
			text.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			text.alignment = TextAlignmentOptions.Center;
			text.rectTransform.offsetMin = new(-4f, -3.99f);
			text.rectTransform.offsetMax = new(4f, 4.01f);
			trapdoorholder.text = text;

			var collider = trapdoorholder.gameObject.AddComponent<BoxCollider>();
			collider.size = Vector3.one * 4.9f;
			collider.isTrigger = true;

			var builder = GetComponent<TrapDoorBuilder>();
			builder.trapDoorpre = trapdoorholder;
			
			builder.closedSprites = [storedSprites[0], storedSprites[1]];
			builder.openSprites = [storedSprites[2], storedSprites[3]];

			trapdoorholder.aud_shut = soundObjects[0];
			trapdoorholder.aud_open = soundObjects[1];

			var trapdoor = ObjectCreationExtensions.CreateSpriteBillboard(builder.closedSprites[0], false);
			trapdoor.transform.SetParent(trapdoorholder.transform); // prefab stuf

			
			trapdoor.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			trapdoor.transform.localPosition = Vector3.up * 0.02f;
			trapdoor.name = "TrapdoorVisual";
			trapdoor.gameObject.layer = 0; // default layer

			trapdoorholder.renderer = trapdoor;
			trapdoorholder.audMan = trapdoorholder.gameObject.CreatePropagatedAudioManager(35f, 45f);

			// Fake trapdoor
			var fake = trapdoor.SafeDuplicatePrefab(true);
			fake.name = "FakeTrapDoor";
			fake.gameObject.CreatePropagatedAudioManager(35f, 45f);
			trapdoorholder.fakeTrapdoorPre = fake.transform;
		}
	}
}
