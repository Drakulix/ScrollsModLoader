using System;
using UnityEngine;

namespace GameReplay.Mod
{
	public interface Item
	{
		bool selectable();
		Texture getImage();
		String getName();
		String getDesc();
	}
}

