

using PixelInternalAPI.Classes;
using UnityEngine;

namespace BBTimes.Extensions.ObjectCreationExtensions
{
	public static class ClickableLinkCreator
	{
		public static ClickableLink CreateClickableLink(this MonoBehaviour clickable, Vector3 clickableLocalPos)
		{
			if (clickable.GetComponent<IClickable<int>>() == null)
				throw new System.ArgumentException($"Given clickable ({clickable.name}) doesn\'t have any IClickable<int>");

			var obj = new GameObject($"{clickable.name}_Clickable");
			obj.transform.SetParent(clickable.transform);
			obj.transform.localPosition = clickableLocalPos;

			obj.gameObject.layer = LayerStorage.iClickableLayer;
			var gm = obj.AddComponent<ClickableLink>();
			gm.link = clickable;

			return gm;
		}
		public static ClickableLink CreateClickableLink(this MonoBehaviour clickable) =>
			clickable.CreateClickableLink(Vector3.zero);
		public static ClickableLink CopyColliderAttributes(this ClickableLink link, CapsuleCollider myCol)
		{
			var col = link.gameObject.AddComponent<CapsuleCollider>();
			col.isTrigger = true;
			col.height = myCol.height;
			col.direction = myCol.direction;
			col.radius = myCol.radius;
			return link;
		}
	}
}
