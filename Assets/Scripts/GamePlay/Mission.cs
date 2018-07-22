using UnityEngine;
using System.Collections;

public enum MissionType
{
	Score,
	Block,
	Ring,
}
public class Mission 
{
	public MissionType type = MissionType.Score;
	public int amount = 0;
	public Mission(int pAmount, MissionType missionType)
	{
		amount = pAmount;
		type = missionType;
	}
}
