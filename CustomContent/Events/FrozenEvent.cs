using UnityEngine;
using System.Collections.Generic;
using BBTimes.CustomComponents;
using UnityEngine.UI;
using BBTimes.Extensions;
using PixelInternalAPI.Extensions;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Manager;
using System.Collections;


namespace BBTimes.CustomContent.Events
{
    public class FrozenEvent : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{
			eventIntro = this.GetSound("baldi_freeze.wav", "Event_FreezeEvent0", SoundType.Effect, Color.green);
			eventIntro.additionalKeys = [
				new() {time = 1.495f, key = "Event_FreezeEvent1"},
				new() {time = 4.156f, key = "Event_FreezeEvent2"},
				new() {time = 6.911f, key = "Event_FreezeEvent3"}
				];
			audMan = gameObject.CreateAudioManager(65, 85).MakeAudioManagerNonPositional();

			audFreeze = this.GetSoundNoSub("freeze.wav", SoundType.Effect);

			var canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(transform);
			canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			canvas.name = "iceOverlay";
			ObjectCreationExtensions.CreateImage(canvas, this.GetSprite(1f, "icehud.png"), true); // stunly stare moment
			canvas.gameObject.SetActive(false);

			canvasPre = canvas;

			slipMatPre = BBTimesManager.man.Get<SlippingMaterial>("SlipperyMatPrefab").SafeDuplicatePrefab(true);
			((SpriteRenderer)slipMatPre.GetComponent<RendererContainer>().renderers[0]).sprite = this.GetSprite(16.5f, "wat.png");
			slipMatPre.name = "IcePatch";
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("events", "Textures");
		public string SoundPath => this.GenerateDataPath("events", "Audios");
		// ---------------------------------------------------

		public override void Begin()
		{
			base.Begin();

			audMan.PlaySingle(audFreeze);
			for (int i = 0; i < ec.Npcs.Count; i++)
			{
				if (ec.Npcs[i] != null && ec.Npcs[i] && ec.Npcs[i].Navigator.isActiveAndEnabled) // null check is different from checking if the object exists
				{
					var mod = new MovementModifier(Vector3.zero, 1f);
					moveMods.Add(new(ec.Npcs[i], mod));
					ec.Npcs[i].Navigator.Entity.ExternalActivity.moveMods.Add(mod);
				}
			}
			foreach (var player in ec.Players)
			{
				if (player == null) continue; // I forgot the array include null items

				var mod = new MovementModifier(Vector3.zero, 1f);
				player.Am.moveMods.Add(mod);
				pMoveMods.Add(new(player.GetAttribute(), mod));

				var ca = Instantiate(canvasPre, transform);
				ca.transform.localPosition = Vector3.zero;
				ca.gameObject.SetActive(true);
				ca.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(player.playerNumber).canvasCam;
				canvasToDespawn.Add(ca.GetComponentInChildren<Image>());
			}
			foreach (var cell in ec.AllCells())
			{
				cellColors.Add(cell, new(cell.lightColor, cell.lightOn));
				cell.lightColor = Color.cyan;
				cell.SetLight(true);
			}

			activeFrozenEvents++;
			if (slipperGenerator != null)
				StopCoroutine(slipperGenerator);
			slipperGenerator = StartCoroutine(GenerateSlippers());
		}

		IEnumerator GenerateSlippers()
		{
			List<Cell> cells = ec.mainHall.AllTilesNoGarbage(false, false);
			int max = ec.levelSize.x * ec.levelSize.z / (ec.levelSize.x + ec.levelSize.z);
			int frameSkips = 0;

			for (int i = 0; i < max; i++)
			{
				if (cells.Count == 0) yield break;
				int x = Random.Range(0, cells.Count);
				SpawnSlipper(cells[x]);
				cells.RemoveAt(x);

				if (++frameSkips >= 25)
				{
					yield return null;
					frameSkips = 0;
				}
				
			}
		}

		void SpawnSlipper(Cell cell)
		{
			var slip = Instantiate(slipMatPre);
			slip.SetAnOwner(gameObject);
			slip.transform.position = cell.FloorWorldPosition;
			slips.Add(slip);
		}

		void Update()
		{
			if (!active || isPaused) return;

			for (int i = 0; i < moveMods.Count; i++)
			{
				if (!moveMods[i].Key)
				{
					moveMods.RemoveAt(i--);
					continue;
				}
				moveMods[i].Value.movementMultiplier -= slowDownMultiplier * ec.EnvironmentTimeScale * Time.deltaTime;
				moveMods[i].Value.movementMultiplier += moveMods[i].Key.Navigator.Velocity.magnitude * ec.EnvironmentTimeScale * Time.deltaTime * speedReduceFactor;
				moveMods[i].Value.movementMultiplier = Mathf.Clamp(moveMods[i].Value.movementMultiplier, 0.35f, maxVel);
			}

			for (int i = 0; i < pMoveMods.Count; i++)
			{
				var x = pMoveMods[i];

				if (x.Key.HasAttribute("hotchocactive"))
				{
					x.Value.movementMultiplier += slowDownMultiplier * ec.EnvironmentTimeScale * Time.deltaTime;
					x.Value.movementMultiplier = Mathf.Clamp(x.Value.movementMultiplier, 0.1f, 0.95f);
				}
				else
				{
					x.Value.movementMultiplier -= slowDownMultiplier * ec.EnvironmentTimeScale * Time.deltaTime;
					if (!float.IsNaN(x.Key.Pm.plm.RealVelocity)) // why tf does it give NaN when pausing the game
						x.Value.movementMultiplier += x.Key.Pm.plm.RealVelocity * ec.EnvironmentTimeScale * Time.deltaTime * speedReduceFactor;
					x.Value.movementMultiplier = Mathf.Clamp(x.Value.movementMultiplier, 0.1f, maxVel);
				}

				
				var co = canvasToDespawn[i].color;
				co.a = maxVel - x.Value.movementMultiplier + (1f - maxVel);
				canvasToDespawn[i].color = co;
			}

		}

		public override void End()
		{
			base.End();
			for (int i = 0; i < moveMods.Count; i++)
			{
				if (moveMods[i].Key)
					moveMods[i].Key.Navigator.Entity.ExternalActivity.moveMods.Remove(moveMods[i].Value);
			}

			pMoveMods.ForEach(x => x.Key.Pm.Am.moveMods.Remove(x.Value));

			moveMods.Clear(); // Just set it to nothingness
			pMoveMods.Clear();
			foreach (var cell in cellColors)
			{
				cell.Key.lightColor = cell.Value.Key;
				cell.Key.SetLight(true);
				cell.Key.SetLight(cell.Value.Value);
			}
			cellColors.Clear();

			canvasToDespawn.ForEach(x => Destroy(x.transform.parent.gameObject));
			canvasToDespawn.Clear();

			activeFrozenEvents--;

			if (slipperGenerator != null)
				StopCoroutine(slipperGenerator);
			while (slips.Count != 0)
			{
				Destroy(slips[0].gameObject);
				slips.RemoveAt(0);
			}	
		}
		public override void Pause()
		{
			base.Pause();
			isPaused = true;
		}
		public override void Unpause()
		{
			base.Unpause();
			isPaused = false;
		}

		void OnDestroy()
		{
			canvasToDespawn.ForEach(x => Destroy(x.transform.parent.gameObject));
			if (active)
				activeFrozenEvents--;
		}

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audFreeze;

		[SerializeField]
		internal Canvas canvasPre;

		[SerializeField]
		internal SlippingMaterial slipMatPre;

		readonly List<SlippingMaterial> slips = [];
		bool isPaused = false;
		Coroutine slipperGenerator;

		readonly List<KeyValuePair<NPC, MovementModifier>> moveMods = []; // I forgot KeyValuePairs are structs

		readonly List<KeyValuePair<PlayerAttributesComponent, MovementModifier>> pMoveMods = [];

		readonly List<Image> canvasToDespawn = [];

		readonly Dictionary<Cell, KeyValuePair<Color, bool>> cellColors = [];

		[SerializeField]
		[Range(0f, 1f)]
		internal float maxVel = 0.6f, slowDownMultiplier = 0.35f, speedReduceFactor = 0.085f;

		internal static int activeFrozenEvents = 0;
	}
}
