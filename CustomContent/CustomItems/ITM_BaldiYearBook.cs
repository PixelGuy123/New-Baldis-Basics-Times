using BBTimes.CustomComponents;
using UnityEngine;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using UnityEngine.UI;
using BBTimes.Extensions;
using System.Collections;
using System.Collections.Generic;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using TMPro;

// TODO: Transform this into an actual Yearbook later on

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_BaldiYearbook : Item, IItemPrefab
	{
		[SerializeField] 
		internal Canvas canvas;

		[SerializeField]
		internal Image background, npcPosterVisual;

		[SerializeField] 
		internal SoundObject audFail, audOpenBook, audFlipOver;

		[SerializeField]
		internal StandardMenuButton exitButton, nextPageBut, previousPageBut;

		[SerializeField]
		internal TextMeshProUGUI pageDisplay, missingCharacterText;

		internal static Dictionary<System.Type, Sprite> npcBookRepresentations = [];

		public void SetupPrefab()
		{
			canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.name = "YearbookCanvas";
			canvas.gameObject.ConvertToPrefab(false);
			canvas.transform.SetParent(transform);

			canvas.GetComponent<PlaneDistance>().planeDistance = 0.31f; // required for GlobalCam pause
			var initiator = canvas.gameObject.AddComponent<CursorInitiator>();
			initiator.cursorPre = GenericExtensions.FindResourceObject<CursorController>();
			initiator.screenSize = new(480f, 360f);
			initiator.graphicRaycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
			initiator.graphicRaycaster.blockingMask = -1;

			background = ObjectCreationExtensions.CreateImage(
				canvas, 
				AssetLoader.SpriteFromTexture2D(TextureExtensions.CreateSolidTexture(480, 360, new(0.5f, 0.5f, 0.5f, 0.45f)), 1f), 
				true);
			background.name = "YearbookBg";

			var bookBg = ObjectCreationExtensions.CreateImage(
				canvas,
				this.GetSprite(1f, "bookRender.png"),
				true
				);
			bookBg.name = "YearbookActualBg";

			npcPosterVisual = ObjectCreationExtensions.CreateImage(canvas, false);
			npcPosterVisual.name = "YearbookNPCVisual";
			npcPosterVisual.transform.localScale = Vector3.one * 2.75f;
			npcPosterVisual.transform.localPosition = new(27.1f, 0f, 0f);

			pageDisplay = ObjectCreationExtensions.CreateTextMeshProUGUI(Color.black);
			pageDisplay.transform.SetParent(canvas.transform);
			pageDisplay.transform.localPosition = Vector3.down * 135f;
			pageDisplay.text = $"0/0 {Singleton<LocalizationManager>.Instance.GetLocalizedText("BaldiYearbook_PageDisplay_Label")}";
			pageDisplay.alignment = TextAlignmentOptions.Center;
			pageDisplay.name = "PageDisplay";

			missingCharacterText = ObjectCreationExtensions.CreateTextMeshProUGUI(Color.black);
			missingCharacterText.transform.SetParent(canvas.transform);
			missingCharacterText.transform.localPosition = new(23.83f, 25.69f);
			missingCharacterText.alignment = TextAlignmentOptions.Center;
			missingCharacterText.name = "MissingCharacterText";
			missingCharacterText.rectTransform.sizeDelta = new(245f, 100f);

			const int togglerSheetMax = 4;

			Sprite[] togglersSheet = new Sprite[togglerSheetMax];
			for (int i = 0; i < togglerSheetMax; i++)
			{
				string name = "MenuArrowSheet_" + i;
				togglersSheet[i] = GenericExtensions.FindResourceObjectByName<Sprite>(name);
			}

			nextPageBut = canvas.CreateImageButton("NextPageButton", togglersSheet[1], togglersSheet[3]);
			nextPageBut.transform.localScale = Vector3.one * 0.3f;
			nextPageBut.transform.localPosition = new(81.63f, -136.218f);

			previousPageBut = canvas.CreateImageButton("PreviousPageButton", togglersSheet[0], togglersSheet[2]);
			previousPageBut.transform.localScale = Vector3.one * 0.3f;
			previousPageBut.transform.localPosition = new(-77.77f, -136.218f);

			togglersSheet = new Sprite[2];
			for (int i = 0; i < 2; i++)
			{
				string name = "BackArrow_" + i;
				togglersSheet[i] = GenericExtensions.FindResourceObjectByName<Sprite>(name);
			}

			exitButton = canvas.CreateImageButton("ExitPage", togglersSheet[1], togglersSheet[0]);
			exitButton.transform.localScale = Vector3.one * 0.5f;
			exitButton.transform.localPosition = new(-135.72f, 148.45f);


			var stub = new GameObject("CursorStub");
			stub.transform.SetParent(canvas.transform);
			stub.transform.localPosition = Vector3.zero;
			stub.gameObject.layer = LayerStorage.ui;

			audFail = GenericExtensions.FindResourceObjectByName<SoundObject>("ErrorMaybe");
			audOpenBook = this.GetSoundNoSub("book_open.wav", SoundType.Effect);
			audFlipOver = this.GetSoundNoSub("book_pageTurn.wav", SoundType.Effect);

			// Order the child objects
			background.transform.SetAsLastSibling();
			bookBg.transform.SetAsLastSibling();
			npcPosterVisual.transform.SetAsLastSibling();
			missingCharacterText.transform.SetAsLastSibling();
			pageDisplay.transform.SetAsLastSibling();
			nextPageBut.transform.SetAsLastSibling();
			previousPageBut.transform.SetAsLastSibling();
			exitButton.transform.SetAsLastSibling();
			stub.transform.SetAsLastSibling();
		}

		public void SetupPrefabPost()
		{
			var generatorRef = Instantiate(GenericExtensions.FindResourceObject<TextTextureGenerator>()); // Must exist in the world to render text properly
			foreach (var npcMeta in NPCMetaStorage.Instance.All())
			{
				var npc = npcMeta.value;
				var npcType = npc.GetType();
				if (!npc.Poster || !npc.Poster.baseTexture || npcBookRepresentations.ContainsKey(npcType))
					continue;

	
				npcBookRepresentations.Add(npcType, 
					AssetLoader.SpriteFromTexture2D(
						generatorRef.GenerateTextTexture(npc.Poster)
					.ConvertToGrayscale(), 1f
					));
			}
			Destroy(generatorRef.gameObject);
		}

		public string Name { get; set; }
		public string Category => "items";
		public ItemObject ItmObj { get; set; }

		float previousTimeScale = 1f;
		bool closeBookTriggered = false;
		int currentNPCIndex = 0;

		void PauseGame()
		{
			// All the procedure to Pause the game without stop rendering and stuff
			if (pm.ec.map)
				Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).mapCam.enabled = false;

			previousTimeScale = Time.timeScale;
			Time.timeScale = 0f;

			pm.itm.Disable(true);

			canvas.gameObject.SetActive(true);
			canvas.worldCamera = Singleton<GlobalCam>.Instance.Cam;

			Singleton<CoreGameManager>.Instance.disablePause = true;
			Singleton<GlobalCam>.Instance.FadeIn(UiTransition.Dither, 0.01666667f);
			Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).SetControllable(false);

			AudioListener.pause = true;
			Singleton<InputManager>.Instance.ActivateActionSet("Interface");
			StartCoroutine(PauseCloseAwaiter());
		}

		void UnpauseGame()
		{
			Singleton<CoreGameManager>.Instance.disablePause = false;
			Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).SetControllable(true);
			Singleton<InputManager>.Instance.ActivateActionSet("InGame");
			Singleton<InputManager>.Instance.StopFrame();

			pm.itm.Disable(false);

			Time.timeScale = previousTimeScale;

			AudioListener.pause = false;
			Singleton<GlobalCam>.Instance.Transition(UiTransition.Dither, 0.01666667f);
			canvas.gameObject.SetActive(false);
		}

		public void AdvancePage(int offset) 
		{
			currentNPCIndex += offset;
			if (offset > 0)
				currentNPCIndex %= pm.ec.npcsToSpawn.Count;
			else if (currentNPCIndex < 0)
				currentNPCIndex = pm.ec.npcsToSpawn.Count - 1;

			if (offset != 0)
				Singleton<MusicManager>.Instance.PlaySoundEffect(audFlipOver);

			npcPosterVisual.sprite = TryGetNPCPoster(pm.ec.npcsToSpawn[currentNPCIndex]);
			pageDisplay.text = $"{currentNPCIndex + 1}/{pm.ec.npcsToSpawn.Count} {Singleton<LocalizationManager>.Instance.GetLocalizedText("BaldiYearbook_PageDisplay_Label")}";
		} // Should update the npc poster!

		Sprite TryGetNPCPoster(NPC npc)
		{
			if (npcBookRepresentations.TryGetValue(npc.GetType(), out var sprite))
			{
				npcPosterVisual.gameObject.SetActive(true);
				missingCharacterText.gameObject.SetActive(false);
				return sprite;
			}
			npcPosterVisual.gameObject.SetActive(false);
			missingCharacterText.gameObject.SetActive(true);
			missingCharacterText.text = $"<b>{npc.Character.ToStringExtended()}</b>\n{Singleton<LocalizationManager>.Instance.GetLocalizedText("BaldiYearbook_PageDisplay_Missing")}";
			return null;
		}

		public override bool Use(PlayerManager pm)
		{
			if (pm.ec.npcsToSpawn.Count == 0 || Singleton<CoreGameManager>.Instance.Paused)
			{
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audFail);
				Destroy(gameObject);
				return false;
			}
			this.pm = pm;
			AdvancePage(0);

			PauseGame();
			Singleton<MusicManager>.Instance.PlaySoundEffect(audOpenBook);
			

			exitButton.OnPress.AddListener(CloseBook);
			nextPageBut.OnPress.AddListener(() => AdvancePage(1));
			previousPageBut.OnPress.AddListener(() => AdvancePage(-1));

			return false;
		}

		IEnumerator PauseCloseAwaiter()
		{
			while (Singleton<GlobalCam>.Instance.TransitionActive || !Singleton<InputManager>.Instance.GetDigitalInput("Pause", true))
				yield return null;

			CloseBook();
			yield break;
		}

		public void CloseBook()
		{
			if (closeBookTriggered)
				return;
			closeBookTriggered = true;

			UnpauseGame();

			Destroy(gameObject);
		}
	}
}