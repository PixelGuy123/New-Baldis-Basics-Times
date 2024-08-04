using System.Collections;
using UnityEngine;
using TMPro;
using BBTimes.CustomComponents;

namespace BBTimes.CustomContent.Objects
{
	public class Trapdoor : MonoBehaviour
	{
		private void Start()
		{
			Shut(false);
			fakeTrapdoorPre.GetComponent<SpriteRenderer>().sprite = renderer.sprite;
		}

		private void OnTriggerStay(Collider other) // Stay because someone could be standing on it
		{
			if (ForceDisableCollision || !open) return;

			if (other.CompareTag("NPC") || other.CompareTag("Player"))
			{
				Entity e = other.GetComponent<Entity>();
				var pa = other.GetComponent<PlayerAttributesComponent>();
				if (e && e.Grounded && (pa == null || !pa.HasAttribute("boots")) && e.Override(overrider))
					StartCoroutine(Teleport(e));
				
			}
		}

		IEnumerator FakeTrapdoorSpawning(Transform fake)
		{
			fake.transform.localScale = Vector3.zero;
			float scale = 0f;
			while (true)
			{
				scale += Time.deltaTime * ec.EnvironmentTimeScale * fakeSpawnSpeed;
				if (scale > 1f)
					break;
				fake.transform.localScale = Vector3.one * scale;
				yield return null;
			}
			fake.transform.localScale = Vector3.one;


			yield break;

		}

		IEnumerator FakeTrapdoorDespawn(Transform fake)
		{
			float cooldown = Random.Range(3f, 6f);
			while (cooldown > 0f)
			{
				cooldown -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}

			float scale = 1f;
			while (true)
			{
				scale -= Time.deltaTime * ec.EnvironmentTimeScale * fakeSpawnSpeed;
				if (scale <= 0f)
					break;
				fake.transform.localScale = Vector3.one * scale;
				yield return null;
			}
			Destroy(fake.gameObject);


			yield break;
		}

		private Transform SpawnFakeTrapdoor(Vector3 pos)
		{
			var fake = Instantiate(fakeTrapdoorPre);
			fake.GetComponent<SpriteRenderer>().sprite = sprites[0];
			fake.transform.position = pos;
			fake.gameObject.SetActive(true);
			StartCoroutine(FakeTrapdoorSpawning(fake));
			return fake;
		}

		IEnumerator Teleport(Entity subject)
		{

			if (subject)
			{
				overrider.SetFrozen(true);
				overrider.SetInteractionState(false);
			}
			ForceDisableCollision = true;

			Vector3 newPos;
			Transform fake = null;
			if (linkedTrapdoor == null)
			{
				var list = ec.AllTilesNoGarbage(false, true);
				list.ConvertEntityUnsafeCells();
				list.Remove(ec.CellFromPosition(transform.position));
				newPos = list[Random.Range(0, list.Count)].FloorWorldPosition;
				fake = SpawnFakeTrapdoor(newPos + Vector3.up * 0.02f);
			}
			else
			{
				linkedTrapdoor.ForceDisableCollision = true;
				newPos = linkedTrapdoor.transform.position;
			}

			float height = subject.InternalHeight;
			subject.Teleport(transform.position);
			float sink = 1f;
			while (sink > 0.1f)
			{
				sink -= ec.EnvironmentTimeScale * Time.deltaTime * sinkSpeed;
				if (!subject) yield break;
				overrider.SetHeight(sink * height);
				yield return null;
			}

			if (fake != null)
			{
				fake.GetComponent<PropagatedAudioManager>().PlaySingle(aud_open);
				fake.GetComponent<SpriteRenderer>().sprite = sprites[1];
			}

			Shut(true, true);
			ForceDisableCollision = false;
			linkedTrapdoor?.Shut(false);

			subject.Teleport(newPos);

			while (sink < 1f)
			{
				sink += ec.EnvironmentTimeScale * Time.deltaTime * sinkSpeed;
				if (!subject) yield break;
				overrider.SetHeight(sink * height);
				yield return null;
			}

			if (subject)
			{
				overrider.SetHeight(height);
				overrider.SetFrozen(false);
				overrider.SetInteractionState(true);
				overrider.Release();
			}

			

			if (linkedTrapdoor != null)
			{
				linkedTrapdoor.Shut(true, true);
				linkedTrapdoor.ForceDisableCollision = false;
			}

			if (fake != null)
			{
				fake.GetComponent<SpriteRenderer>().sprite = sprites[0];
				fake.GetComponent<PropagatedAudioManager>().PlaySingle(aud_shut);
				StartCoroutine(FakeTrapdoorDespawn(fake));
			}
			
		}

		IEnumerator Timer()
		{
			cooldown = Random.Range(minCooldown, maxCooldown);
			while (cooldown > 0f)
			{
				text.text = Mathf.FloorToInt(cooldown).ToString();
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			Shut(false);

			yield break;
		}

		void Shut(bool shut, bool beginTimer = false)
		{
			if (shut)
			{
				if (open) audMan.PlaySingle(aud_shut); // Only plays if open and shut
			}
			else if (!open)
				audMan.PlaySingle(aud_open); // always lol

			renderer.sprite = sprites[shut ? 0 : 1];
			text.gameObject.SetActive(shut);
			open = !shut;
			if (shut && beginTimer)
			{
				if (currentTimer != null)
					StopCoroutine(currentTimer);
				currentTimer = StartCoroutine(Timer());
			}
		}


		// ************ Setup stuff *************
		public void SetEC(EnvironmentController ec) =>
			this.ec = ec;

		public void SetLinkedTrapDoor(Trapdoor d) =>
			linkedTrapdoor = d;
		

		EnvironmentController ec;

		Coroutine currentTimer;

		bool open = true;

		public bool ForceDisableCollision { get; set; }

		private Trapdoor linkedTrapdoor = null;

		public Trapdoor Link => linkedTrapdoor;

		[SerializeField]
		public TextMeshPro text;

		[SerializeField]
		public SpriteRenderer renderer;

		[SerializeField]
		public Sprite[] sprites;

		[SerializeField]
		public SoundObject aud_shut, aud_open;

		[SerializeField]
		public PropagatedAudioManager audMan;

		[SerializeField]
		public Transform fakeTrapdoorPre;

		readonly EntityOverrider overrider = new();

		const float minCooldown = 15f, maxCooldown = 30f, sinkSpeed = 0.5f, fakeSpawnSpeed = 5f;

		float cooldown;
	}
}
