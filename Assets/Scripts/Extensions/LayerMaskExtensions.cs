using UnityEngine;

namespace Extensions {
	public static class LayerMaskExtensions {
		/// <summary>
		/// Returns true if <paramref name="mask"/> includes the layer with index <paramref name="layer"/>.
		/// </summary>
		public static bool IncludesLayer(this LayerMask mask, int layer) {
			return ((1 << layer) & mask.value) != 0;
		}
	}
}