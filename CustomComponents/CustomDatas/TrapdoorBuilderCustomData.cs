﻿using UnityEngine;
using TMPro;
using BBTimes.CustomContent.Builders;
using BBTimes.Manager;
using MTM101BaldAPI.AssetTools;
using System.IO;
using BBTimes.CustomContent.Objects;
using PixelInternalAPI.Classes;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class TrapdoorBuilderCustomData : CustomObjectPrefabData
	{
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var trapdoorholder = new GameObject("TrapDoor").AddComponent<Trapdoor>();

			

			var text = new GameObject("TrapdoorText").AddComponent<TextMeshPro>();
			text.transform.SetParent(trapdoorholder.transform);
			text.transform.localPosition = Vector3.up * 0.02f;
			text.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			text.autoSizeTextContainer = true;
			text.text = "10";
			text.alignment = TextAlignmentOptions.Center;
			text.gameObject.layer = LayerStorage.billboardLayer;
			trapdoorholder.text = text;

			var collider = trapdoorholder.gameObject.AddComponent<BoxCollider>();
			collider.size = Vector3.one * 4.9f;
			collider.isTrigger = true;

			var builder = GetComponent<TrapDoorBuilder>();
			builder.trapDoorpre = trapdoorholder;
			builder.closedSprites = [AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(TexturePath, "trapdoor_rng.png")), 25f), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(TexturePath, "trapdoor.png")), 25f)];
			builder.openSprites = [AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(TexturePath, "trapdoor_rng_open.png")), 25f), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(TexturePath, "trapdoor_open.png")), 25f)];

			trapdoorholder.aud_shut = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "trapDoor_shut.wav")), "Sfx_Doors_StandardShut", SoundType.Voice, Color.white);
			trapdoorholder.aud_open = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "trapDoor_open.wav")), "Sfx_Doors_StandardOpen", SoundType.Voice, Color.white);

			trapdoorholder.audMan = trapdoorholder.gameObject.CreatePropagatedAudioManager(35f, 45f).SetAudioManagerAsPrefab();

			DontDestroyOnLoad(trapdoorholder.gameObject);
			trapdoorholder.gameObject.SetActive(false);

			var trapdoor = ObjectCreationExtensions.CreateSpriteBillboard(builder.closedSprites[0], false);
			trapdoor.gameObject.SetAsPrefab(true);
			trapdoor.transform.SetParent(trapdoorholder.transform);
			trapdoorholder.renderer = trapdoor;
			trapdoor.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			trapdoor.transform.localPosition = Vector3.up * 0.02f;
			trapdoor.name = "TrapdoorVisual";
			trapdoor.gameObject.layer = 0; // default layer

			// Fake trapdoor
			var fake = trapdoor.DuplicatePrefab();
			fake.name = "FakeTrapDoor";
			fake.gameObject.CreatePropagatedAudioManager(35f, 45f).SetAudioManagerAsPrefab();
			trapdoorholder.fakeTrapdoorPre = fake.transform;
			fake.gameObject.SetActive(false);
		}
	}
}
