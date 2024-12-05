using UnityEngine;

namespace BBTimes.CustomComponents.SecretEndingComponents
{
	internal class NoRendererOnStart : MonoBehaviour, IItemAcceptor // Useful for invisibile walls
	{
		[SerializeField]
		public bool canBeDisabled = false; // Can be used by SecretBaldi to disable walls to access new areas

		[SerializeField]
		public bool affectedByScrewDriver = false, affectedByKey = false;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audUse;

		[SerializeField]
		internal BoxCollider collider;
		
		void Start() 
		{
			foreach (var r in GetComponentsInChildren<Renderer>())
				r.enabled = false;
		}

		public bool ItemFits(Items item) =>
			(affectedByKey && item == Items.DetentionKey) || (affectedByScrewDriver && item == MTM101BaldAPI.EnumExtensions.GetFromExtendedName<Items>("Screwdriver"));
		public void InsertItem(PlayerManager pm, EnvironmentController ec)
		{
			collider.enabled = false;
			if (audUse)
				audMan?.PlaySingle(audUse);
		}
	}
}
