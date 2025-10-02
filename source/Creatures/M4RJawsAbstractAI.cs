using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class M4RJawsAbstractAI : AbstractCreatureAI
{
	public List<WorldCoordinate> AllowedNodes = [];
    public int TimeInRoom, Wait;

	public M4RJawsAbstractAI(World world, AbstractCreature parent) : base(world, parent) => PopulateAllowedNodes(world);

	public virtual void PopulateAllowedNodes(World world)
	{
        var allowNds = AllowedNodes;
        allowNds.Clear();
		int i;
		AbstractRoomNode[] nodes;
		if (world.singleRoomWorld)
		{
			nodes = world.GetAbstractRoom(0).nodes;
            for (i = 0; i < nodes.Length; i++)
			{
				ref readonly var nd = ref nodes[i];
                if (nd.type == AbstractRoomNode.Type.SideExit && nd.entranceWidth >= 5)
                    allowNds.Add(new(0, -1, -1, i));
			}
		}
		else
		{
            var mytp = parent.creatureTemplate.type;
            var firstRoomIndex = world.firstRoomIndex;
            var totRooms = firstRoomIndex + world.NumberOfRooms - 1;
            for (i = firstRoomIndex; i < totRooms; i++)
            {
                var rm = world.GetAbstractRoom(i);
                var attr = rm.AttractionForCreature(mytp);
                if (attr == AbstractRoom.CreatureRoomAttraction.Like || attr == AbstractRoom.CreatureRoomAttraction.Stay)
                {
                    nodes = rm.nodes;
                    for (var k = 0; k < nodes.Length; k++)
                    {
                        ref readonly var node = ref nodes[k];
                        if (node.type == AbstractRoomNode.Type.SideExit && node.entranceWidth >= 5)
                        {
                            var nd = new WorldCoordinate(rm.index, -1, -1, k);
                            if (!allowNds.Contains(nd))
                                allowNds.Add(nd);
                        }
                    }
                }
            }
        }
	}

	public override void NewWorld(World newWorld)
	{
		base.NewWorld(newWorld);
		PopulateAllowedNodes(newWorld);
	}

	public override void AbstractBehavior(int time)
	{
		var rm = parent.Room;
		var game = world.game;
		if (ModManager.MSC && game.session is ArenaGameSession sess && sess.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge)
		{
			var totRooms = world.NumberOfRooms;
			var offScreenDen = world.offScreenDen;
            for (var i = 0; i < totRooms; i++)
			{
				var rmIndex = world.firstRoomIndex + i;
                if (world.GetAbstractRoom(rmIndex) != offScreenDen)
				{
					SetDestination(new(rmIndex, -1, -1, 0));
					break;
				}
			}
		}
		if (Wait > 0)
		{
			Wait -= time;
			return;
		}
		var destRoom = destination.room;
        var allowedNodes = AllowedNodes;
        if (destRoom != rm.index && game.IsStorySession)
		{
			var flag = false;
			if (destRoom == world.offScreenDen.index)
				flag = true;
			else
			{
				for (var i = 0; i < allowedNodes.Count; i++)
				{
                    if (allowedNodes[i].room == destRoom)
                    {
                        flag = true;
                        break;
                    }
                }
			}
			if (!flag)
			{
				Custom.LogWarning("M4RJaws attempted to enter illegal room " + world.GetAbstractRoom(destRoom)?.name + " " + parent);
				GoToDen();
			}
		}
		if (path.Count > 0 && parent.realizedCreature is null)
			FollowPath(time);
		else if (world.rainCycle.TimeUntilRain < 800)
		{
			if (denPosition is not WorldCoordinate den || !parent.pos.CompareDisregardingTile(den))
				GoToDen();
		}
		else if (allowedNodes.Count != 0)
		{
			if (path.Count == 0 && denPosition is WorldCoordinate den)
                SetDestination(den);
			if (parent.pos.room == world.offScreenDen.index)
				Raid(rm);
		}
	}

    public virtual void Raid(AbstractRoom rm)
	{
		if (AllowedNodes.Count == 0)
			return;
		WorldCoordinate coord = AllowedNodes[Random.Range(0, AllowedNodes.Count)],
			item = new(coord.room, -1, -1, -1);
		var room = world.GetAbstractRoom(coord);
        var nodes = room.nodes;
        for (var k = 0; k < nodes.Length; k++)
		{
			if (item.NodeDefined)
				break;
			var node = nodes[k];
			if (k != coord.abstractNode && node.type == AbstractRoomNode.Type.SideExit && node.entranceWidth >= 5 && room.ConnectionPossible(coord.abstractNode, k, parent.creatureTemplate))
				item.abstractNode = k;
		}
		SetDestination(parent.pos);
		path.Clear();
		if (denPosition is WorldCoordinate w)
			path.Add(w);
		if (item.NodeDefined)
			path.Add(item);
		path.Add(coord);
	}
}
