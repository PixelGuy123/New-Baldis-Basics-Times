using BBTimes.CustomContent;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
    public class Advertisement : MonoBehaviour
	{
		public void Initialize(EnvironmentController ec, float lifeTime)
		{
			this.lifeTime = lifeTime;
			this.ec = ec;
			init = true;
		}

		public void AttachToCamera(Camera cam, Transform attachReference)
		{
			if (attached) return;

			att.AttachTo(attachReference);
			img.sprite = advertisements[Random.Range(0, advertisements.Length)];
			canvas.worldCamera = cam;
			canvas.gameObject.SetActive(true);
			img.transform.localPosition = new(Random.Range(-adOffsetRange, adOffsetRange), Random.Range(-adOffsetRange, adOffsetRange));
			attached = true;
		}

		public void AttachToNPC(NPC npc)
		{
			if (attached) return;

			attachedNPC = npc;
			att.AttachTo(npc.transform);
			npc.GetNPCContainer().AddLookerMod(blindMod);

			attached = true;
		}

		void Update()
		{
			if (!init || !attached) return;

			lifeTime -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (lifeTime <= 0f)
				Destroy(gameObject);
		}

		void OnDestroy()
		{
			if (attachedNPC)
				attachedNPC.GetNPCContainer().RemoveLookerMod(blindMod);
		}

		readonly ValueModifier blindMod = new(0f, 0f);
		NPC attachedNPC;
		float lifeTime = 1f;
		EnvironmentController ec;
		bool init = false, attached = false;
		public bool IsAttached => attached;

		[SerializeField]
		internal Canvas canvas;

		[SerializeField]
		internal UnityEngine.UI.Image img;

		[SerializeField]
		internal Sprite[] advertisements;

		[SerializeField]
		internal VisualAttacher att;

		[SerializeField]
		internal float adOffsetRange = 225f;
	}
}
