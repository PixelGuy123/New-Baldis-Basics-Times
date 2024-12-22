using BBTimes.CustomContent.CustomItems;
using BBTimes.Extensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class MysteryTresentMaker : MonoBehaviour, IItemAcceptor
	{
		public void InsertItem(PlayerManager pm, EnvironmentController ec)
		{
			var tre = Instantiate(tresentPre);
			tre.Throw(ec, pm, transform.position, (pm.transform.position - transform.position).normalized);
			audMan.PlaySingle(audInsert);
		}

		public bool ItemFits(Items item) =>
			acceptableItems.Contains(item);

		public static void AddAcceptableItem(Items item) =>
			acceptableItems.Add(item);

		internal static HashSet<Items> acceptableItems = [Items.Quarter];

		[SerializeField]
		internal Tresent tresentPre;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audInsert;
	}

	public class Tresent : MonoBehaviour
		// ** The effects of the tresent will be hardcoded into one big class for now, because I'm lcaking time to make multiple classes for Tresents **
	{
		public void Throw(EnvironmentController ec, PlayerManager pm, Vector3 position, Vector3 direction)
		{
			this.pm = pm;
			this.ec = ec;
			entity.Initialize(ec, position);
			float speed = Random.Range(minThrowSpeed, maxThrowSpeed);
			entity.AddForce(new(direction, speed, -speed * 0.45f));
			audMan.PlaySingle(audThrow);

			StartCoroutine(TimedOpening());
		}

		IEnumerator TimedOpening()
		{
			float delay = Random.Range(minDelayForExplosion, maxDelayForExplosion);
			while (delay > 0f || entity.Velocity != Vector3.zero)
			{
				delay -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			audMan.FlushQueue(true);
			audMan.QueueAudio(audPrepExplode);
			float frame = 0f;
			while (true)
			{
				frame += ec.EnvironmentTimeScale * Time.deltaTime * explosionSpeed;
				if (frame > (sprsPrepareExplosion.Length - 1))
				{
					frame = sprsPrepareExplosion.Length - 1;
					renderer.sprite = sprsPrepareExplosion[Mathf.FloorToInt(frame)];
					break;
				}
				renderer.sprite = sprsPrepareExplosion[Mathf.FloorToInt(frame)];
				yield return null;
			}
			confettiParts.Emit(Random.Range(minConfettiParticles, maxConfettiParticles));
			audMan.FlushQueue(true);
			audMan.PlaySingle(audExplode);
			StartCoroutine(DisplayText());
			frame = 0f;
			while (true)
			{
				frame += ec.EnvironmentTimeScale * Time.deltaTime * explosionSpeed * 3.55f;
				if (frame > (sprsExploding.Length - 1))
				{
					frame = sprsExploding.Length - 1;
					renderer.sprite = sprsExploding[Mathf.FloorToInt(frame)];
					break;
				}
				renderer.sprite = sprsExploding[Mathf.FloorToInt(frame)];
				yield return null;
			}

			renderer.enabled = false;
			entity.SetFrozen(true);
		}

		IEnumerator DisplayText()
		{
			text.gameObject.SetActive(true);

			text.text = RandomEffect();
			

			float scale, t = 0f;
			while (t < 1f)
			{
				t += ec.EnvironmentTimeScale * 3.5f * Time.deltaTime;
				if (t >= 1f)
					t = 1f;

				scale = Mathf.Lerp(0f, 1f, t);
				text.transform.localScale = Vector3.one * scale;

				yield return null;
			}

			float delay = 5f;
			while (delay > 0f)
			{
				delay -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			t = 0f;
			while (t < 1f)
			{
				t += ec.EnvironmentTimeScale * 4f * Time.deltaTime;
				if (t >= 1f)
					t = 1f;

				scale = Mathf.Lerp(1f, 0f, t);
				text.transform.localScale = Vector3.one * scale;

				yield return null;
			}

			text.gameObject.SetActive(false);
		}

		string RandomEffect() // Main method for effects, definitely what I'll change to support Polymorphism properly when I split up into multiple Tresent types
		{
			bool flag = false; // In-case PlayerManager is null, this flag is to ensure it chooses a valid effect
			int effectid = -1;

			while (!flag)
			{
				List<NPC> npcs;

				effectid = Random.Range(1, 8);
				switch (effectid)
				{
					case 1:
						flag = true;
						StartCoroutine(SpeedEffectCharacter(slowMod));
						break;

					case 2:
						flag = true;
						StartCoroutine(SpeedEffectCharacter(speedMod));
						break;

					case 3:
						if (pm)
						{
							flag = true;
							pm.itm.AddItem(possibleItems[Random.Range(0, possibleItems.Count)]);
						}
						break;

					case 4:
						if (pm && pm.itm.HasItem())
						{
							flag = true;
							pm.itm.RemoveRandomItem();
						}
						break;

					case 5:
						if (ec.offices.Count == 0)
							break;

						npcs = GetValidListOfNPCs;
						if (npcs.Count == 0)
							break;

						var n = npcs[Random.Range(0, npcs.Count)];

						int num = Random.Range(0, ec.offices.Count);
						n.Navigator.Entity.Teleport(ec.offices[num].RandomEntitySafeCellNoGarbage().CenterWorldPosition);
						n.SentToDetention();

						flag = true;

						break;

					case 6:
						npcs = GetValidListOfNPCs;
						if (npcs.Count == 0)
							break;

						npcs[Random.Range(0, npcs.Count)].Navigator.Entity.Teleport(transform.position);
						flag = true;
						break;

					case 7:
						if (pm)
						{
							var bonker = Instantiate(bonkerPre);
							bonker.ThrowHammer(transform.position, (pm.transform.position - transform.position).normalized, ec, 65f, 4f);
							flag = true;
						}
						break;
				}
			}

			return Singleton<LocalizationManager>.Instance.GetLocalizedText("PST_MTM101_Effect" + effectid);
		}

		IEnumerator SpeedEffectCharacter(MovementModifier moveMod)
		{
			var npcs = GetValidListOfNPCs;

			if (npcs.Count == 0)
				yield break;

			var npc = npcs[Random.Range(0, npcs.Count)];

			npc.Navigator.Am.moveMods.Add(moveMod);

			while (effectDelay > 0f)
			{
				effectDelay -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			npc?.Navigator.Am.moveMods.Remove(moveMod);
		}

		protected List<NPC> GetValidListOfNPCs { get
			{
				List<NPC> npcs = new(ec.Npcs);
				for (int i = 0; i < npcs.Count; i++)
					if (!npcs[i] || !npcs[i].Navigator.isActiveAndEnabled)
						npcs.RemoveAt(i--);
				return npcs;
			}
		}


		protected void Despawn() =>
			Destroy(gameObject);

		readonly MovementModifier slowMod = new(Vector3.zero, 0.35f), speedMod = new(Vector3.zero, 1.75f); // These will also move once polymorphism exists here
		EnvironmentController ec;
		PlayerManager pm;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal float minThrowSpeed = 5f, maxThrowSpeed = 14f, minDelayForExplosion = 2.5f, maxDelayForExplosion = 5f, explosionSpeed = 8f, effectDelay = 10f; // effectDelay must be removed once polymorphism is added

		[SerializeField]
		internal int minConfettiParticles = 35, maxConfettiParticles = 50;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] sprsPrepareExplosion, sprsExploding;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audExplode, audThrow, audPrepExplode;

		[SerializeField]
		internal TextMeshPro text;

		[SerializeField]
		internal ParticleSystem confettiParts;

		[SerializeField]
		internal ITM_Hammer bonkerPre; // Something to also be in polymorphism...

		static List<ItemObject> possibleItems;

		internal static void GatherShopItems() =>
			possibleItems = GameExtensions.GetAllShoppingItems();

	}
}
