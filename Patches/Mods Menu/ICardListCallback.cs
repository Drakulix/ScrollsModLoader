using System;
using System.Collections.Generic;

namespace ScrollsModLoader {
	public interface IListCallback
	{
    	void ButtonClicked(UIListPopup popup, ECardListButton button, Item card);
		void ButtonClicked(UIListPopup popup, ECardListButton button, List<Item> selectedCards, Item card);
		void ItemButtonClicked(UIListPopup popup, Item card);
		void ItemClicked(UIListPopup popup, Item card);
		void ItemHovered(UIListPopup popup, Item card);
		void ItemCanceled(UIListPopup popup, Item card);
	}
}