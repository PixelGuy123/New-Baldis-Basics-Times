using BBTimes.Extensions;
using BBTimes.CustomComponents;
using UnityEngine;
using System.Collections;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_DoorStopper : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			var spr = ObjectCreationExtensions.CreateSpriteBillboard(ItmObj.itemSpriteLarge);
			spr.name = "DoorStopperSprite";
			spr.transform.SetParent(transform);
			spr.transform.localPosition = Vector3.zero;
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach, pm.pc.ClickLayers))
			{
				var door = hit.transform.GetComponent<StandardDoor>();
				if (!door || door.locked)
				{
					Destroy(gameObject);
					return false;
				}

				transform.position = door.doors[0].transform.position;
				transform.position = new(transform.position.x, 0.3f, transform.position.z);

				StartCoroutine(Timer(door));
				return true;
			}
			Destroy(gameObject);
			return false;
		}

		IEnumerator Timer(StandardDoor door)
		{
			door.OpenTimed(doorStopMinimumCooldown + door.shutTime * doorStopEfficiency, false);

			while (door.IsOpen)
				yield return null;
			
			
			Destroy(gameObject);
		}

		[SerializeField]
		[Range(0f, 1f)]
		internal float doorStopEfficiency = 0.85f;

		[SerializeField]
		internal float doorStopMinimumCooldown = 45f;
	}
}
