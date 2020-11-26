using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectOnHoverButton : Button {
	public override void OnPointerEnter(PointerEventData eventData) {
		base.OnPointerEnter(eventData);
		Select();
	}
}