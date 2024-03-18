using UnityEngine;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager // Literally just a storage for layers, so I can manage from here
	{
		internal readonly static LayerMask windowLayer = LayerMask.NameToLayer("Window");

		internal readonly static LayerMask billboardLayer = LayerMask.NameToLayer("Billboard");

		internal readonly static LayerMask iClickableLayer = LayerMask.NameToLayer("ClickableCollidableEntities"); // I had to manually use LayerMask to figure out this long name. Thank you UnityExplorer for making my life harder.
	}
}
