using UnityEngine;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager // Literally just a storage for layers, so I can manage from here
	{
		internal readonly static LayerMask windowLayer = LayerMask.NameToLayer("Window");

		internal readonly static LayerMask billboardLayer = LayerMask.NameToLayer("Billboard");
	}
}
