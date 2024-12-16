using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class SnowPile : EnvironmentObject, IClickable<int>
	{

		[SerializeField]
		internal GameObject mainRenderer;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audPop;

		[SerializeField]
		internal BoxCollider collider;

		[SerializeField]
		internal ParticleSystem snowPopParts;

		bool canBeUsedByAShovel = true;

		public void AssignParticlePlanes(params Transform[] planeReferences)
		{
			var collision = snowPopParts.collision;
			collision.enabled = true;
			collision.enableDynamicColliders = false;
			collision.bounceMultiplier = 1.05f;

			for (int i = 0; i < planeReferences.Length; i++)
				collision.AddPlane(planeReferences[i]);
		}

		public void SetItUsableAgain() => canBeUsedByAShovel = true;

		public void Clicked(int player)
		{
			if (canBeUsedByAShovel && SnowShovel.PlayerHasShovel(Singleton<CoreGameManager>.Instance.GetPlayer(player), out var shovel))
			{
				canBeUsedByAShovel = false;
				shovel.RestInPlace(true);
				audMan.PlaySingle(audPop);

				snowPopParts.Emit(Random.Range(45, 75));

				mainRenderer.SetActive(false);

				ec.CreateItem(
					ec.CellFromPosition(transform.position).room,
					items[Random.Range(0, items.Length)],
					new(transform.position.x, transform.position.z)
					);

				collider.enabled = false;
			}

		}
		public bool ClickableHidden() => !canBeUsedByAShovel;
		public bool ClickableRequiresNormalHeight() => false;
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }


		internal static void SetupItemRandomization()
		{
			var shops = GameExtensions.GetAllShoppingItems();

			items = [.. shops];
		}

		static ItemObject[] items;
	}
}
