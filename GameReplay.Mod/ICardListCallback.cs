using System;
using System.Collections.Generic;

namespace GameReplay.Mod {
	public interface IListCallback
	{
    	void ButtonClicked(UIListPopup popup, ECardListButton button);
		void ButtonClicked(UIListPopup popup, ECardListButton button, List<Item> selectedCards);
		void ItemButtonClicked(UIListPopup popup, Item card);
		void ItemClicked(UIListPopup popup, Item card);
		void ItemHovered(UIListPopup popup, Item card);
	}
}