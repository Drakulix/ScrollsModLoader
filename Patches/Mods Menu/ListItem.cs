using System;
using UnityEngine;

namespace ScrollsModLoader
{
	public interface Item
	{
		bool selectable();
		Texture getImage();
		String getName();
		String getDesc();
	}
}

