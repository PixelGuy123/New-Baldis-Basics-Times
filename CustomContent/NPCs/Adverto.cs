using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI;
using UnityEngine;
using System.Collections.Generic;

namespace BBTimes.CustomContent.NPCs
{
	public class Adverto : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			spriteRenderer[0].sprite = this.GetSprite(25f, "Adverto.png");
			Destroy(GetComponent<AudioManager>()); // he literally doesn't have an AudioManager

			var advertisementObject = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(35f, "AdWall.png"));
			adPre = advertisementObject.gameObject.AddComponent<Advertisement>();
			advertisementObject.name = "Advertisement";
			advertisementObject.gameObject.ConvertToPrefab(true);

			adPre.canvas = ObjectCreationExtensions.CreateCanvas();
			adPre.canvas.transform.SetParent(advertisementObject.transform);
			adPre.canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			adPre.canvas.name = "AdOverlay";
			adPre.canvas.gameObject.SetActive(false);

			adPre.img = ObjectCreationExtensions.CreateImage(adPre.canvas, BBTimesManager.man.Get<Sprite>("whiteScreen"));
			adPre.img.transform.localScale = Vector3.one * 0.5f;

			adPre.advertisements = this.GetSpriteSheet(7, 4, 1f, "Ads.png");

			adPre.att = advertisementObject.gameObject.AddComponent<VisualAttacher>();

			advertisementObject.gameObject.CreatePropagatedAudioManager(60f, 75f).AddStartingAudiosToAudioManager(false, [this.GetSoundNoSub("erro.mp3", SoundType.Voice)]);
		}

		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }

		// stuff above^^
		public override void Initialize()
		{
			base.Initialize();
			navigator.SetSpeed(15);
			navigator.maxSpeed = 15;
			behaviorStateMachine.ChangeState(new Adverto_Wander(this));
		}

		public void AdPlayer(PlayerManager pm) =>
			CreateAd().AttachToCamera(Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam, 
				Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform);
		public void AdNPC(NPC npc) =>
			CreateAd().AttachToNPC(npc);

		Advertisement CreateAd()
		{
			var ad = Instantiate(adPre);
			ad.Initialize(ec, adLifeTime);
			advertisements.Add(ad);
			return ad;
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			for (int i = 0; i < advertisements.Count; i++)
				if (!advertisements[i])
					advertisements.RemoveAt(i--);
		}

		public override void Despawn()
		{
			base.Despawn();
			while (advertisements.Count != 0)
			{
				Destroy(advertisements[0].gameObject);
				advertisements.RemoveAt(0);
			}
		}

		[SerializeField]
		internal Advertisement adPre;

		[SerializeField]
		internal float adLifeTime = 7.5f, timeBeforeAdvertisement = 0.08f;

		readonly List<Advertisement> advertisements = [];

	}

	internal class Adverto_StateBase(Adverto ad) : NpcState(ad)
	{
		protected Adverto ad = ad;
	}

	internal class Adverto_Wander(Adverto ad) : Adverto_StateBase(ad)
	{
		float adCooldown = 0f;
		bool CanAd => adCooldown < 0f;
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(ad, 0));
		}
		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			if (CanAd)
			{
				adCooldown += ad.timeBeforeAdvertisement;
				ad.AdPlayer(player);
			}
		}
		public override void Update()
		{
			base.Update();
			if (!CanAd)
				adCooldown -= ad.TimeScale * Time.deltaTime;
			else
			{
				foreach (NPC npc in ad.ec.Npcs)
				{
					if (npc != ad && npc.looker.enabled)
					{
						ad.looker.Raycast(npc.transform, Mathf.Min(
					(ad.transform.position - npc.transform.position).magnitude + npc.Navigator.Velocity.magnitude,
					ad.looker.distance,
					npc.ec.MaxRaycast
						), out bool flag);
						if (flag)
						{
							adCooldown += ad.timeBeforeAdvertisement;
							ad.AdNPC(npc);
							break;
						}
					}
				}
			}
		}
	}
}
