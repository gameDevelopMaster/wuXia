using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Combine
{
	List<Item> items = new List<Item>();
	public Combine()
	{
		items.Clear();
	}
	public void AddItems(List<Item> pItems)
	{
		items.AddRange(pItems);
	}
	public void Destroy()
	{
		while(items.Count>0)
		{
			items[items.Count-1].Destroy();
			items.RemoveAt(items.Count-1);
		}
	}
}
