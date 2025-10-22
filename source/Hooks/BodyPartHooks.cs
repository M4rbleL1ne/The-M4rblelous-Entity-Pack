global using static LBMergedMods.Hooks.BodyPartHooks;
using UnityEngine;
using RWCustom;
using System;

namespace LBMergedMods.Hooks;

public static class BodyPartHooks
{
    internal static void On_BodyPart_ConnectToPoint(On.BodyPart.orig_ConnectToPoint orig, BodyPart self, Vector2 pnt, float connectionRad, bool push, float elasticMovement, Vector2 hostVel, float adaptVel, float exaggerateVel)
    {
        if (self.owner is SporantulaGraphics g && g.bug is BigSpider b && connectionRad == 12f)
        {
            var chs = b.bodyChunks;
            var pos0 = chs[0].pos;
            var pos1 = chs[1].pos;
            orig(self, pos1 + Custom.DirVec(pos0, pos1) * 42f + Custom.PerpendicularVector(pos0, pos1) * g.flip * 15f, 25f, push, elasticMovement, hostVel, adaptVel, exaggerateVel);
        }
        else
            orig(self, pnt, connectionRad, push, elasticMovement, hostVel, adaptVel, exaggerateVel);
    }

    internal static bool On_BodyPart_OnOtherSideOfTerrain(On.BodyPart.orig_OnOtherSideOfTerrain orig, BodyPart self, Vector2 conPos, float minAffectRadius)
    {
        var res = orig(self, conPos, minAffectRadius);
        if (self is Limb && self.owner is SurfaceSwimmerGraphics g && g.bug is SurfaceSwimmer surf && surf.room is Room rm)
        {
            if (rm.GetTile(self.pos).AnyWater)
                res = true;
            else
            {
                var tlPs = Room.StaticGetTilePosition(self.pos);
                var a = IntVector2.ClampAtOne(Room.StaticGetTilePosition(conPos) - tlPs);
                if (a.x != 0 && a.y != 0)
                {
                    if (Math.Abs(conPos.x - self.pos.x) > Math.Abs(conPos.y - self.pos.y))
                        a.y = 0;
                    else
                        a.x = 0;
                }
                res = res || rm.GetTile(tlPs + a).AnyWater;
            }
        }
        return res;
    }

    internal static void On_BodyPart_PushOutOfTerrain(On.BodyPart.orig_PushOutOfTerrain orig, BodyPart self, Room room, Vector2 basePoint)
    {
        if (self is Limb && self.owner is SurfaceSwimmerGraphics)
            self.PushBugLimbOutOfTerrain(room);
        else
            orig(self, room, basePoint);
    }

    internal static void On_Limb_FindGrip(On.Limb.orig_FindGrip orig, Limb self, Room room, Vector2 attachedPos, Vector2 searchFromPos, float maximumRadiusFromAttachedPos, Vector2 goalPos, int forbiddenXDirs, int forbiddenYDirs, bool behindWalls)
    {
        if (self.owner is SurfaceSwimmerGraphics)
            self.FindBugGrip(room, attachedPos, searchFromPos, maximumRadiusFromAttachedPos, goalPos, forbiddenXDirs, forbiddenYDirs);
        else
            orig(self, room, attachedPos, searchFromPos, maximumRadiusFromAttachedPos, goalPos, forbiddenXDirs, forbiddenYDirs, behindWalls);
    }

    public static void FindBugGrip(this Limb self, Room room, Vector2 attachedPos, Vector2 searchFromPos, float maximumRadiusFromAttachedPos, Vector2 goalPos, int forbiddenXDirs, int forbiddenYDirs)
    {
        if (!Custom.DistLess(attachedPos, searchFromPos, maximumRadiusFromAttachedPos))
            searchFromPos = attachedPos + Custom.DirVec(attachedPos, searchFromPos) * (maximumRadiusFromAttachedPos - 1f);
        if (!Custom.DistLess(attachedPos, goalPos, maximumRadiusFromAttachedPos))
            goalPos = attachedPos + Custom.DirVec(attachedPos, goalPos) * maximumRadiusFromAttachedPos;
        var tilePosition = Room.StaticGetTilePosition(searchFromPos);
        var vector = new Vector2(-100000f, -100000f);
        for (var i = 0; i < 9; i++)
        {
            var eightDir = Custom.eightDirectionsAndZero[i];
            if (eightDir.x == forbiddenXDirs || eightDir.y == forbiddenYDirs)
                continue;
            Vector2 vector2 = room.MiddleOfTile(tilePosition + eightDir), vector3 = new(Mathf.Clamp(goalPos.x, vector2.x - 10f, vector2.x + 10f), Mathf.Clamp(goalPos.y, vector2.y - 10f, vector2.y + 10f));
            var tl = room.GetTile(tilePosition + eightDir);
            if (tl.Solid || tl.AnyWater)
            {
                Room.Tile tl2 = room.GetTile(tilePosition + eightDir + new IntVector2(-eightDir.x, 0)), tl3 = room.GetTile(tilePosition + eightDir + new IntVector2(0, -eightDir.y));
                if (eightDir.x != 0 && !tl2.Solid && !tl2.AnyWater)
                    vector3.x = vector2.x - eightDir.x * 10f;
                if (eightDir.y != 0 && !tl3.Solid && !tl3.AnyWater)
                    vector3.y = vector2.y - eightDir.y * 10f;
                if (Custom.DistNoSqrt(goalPos, vector3) < Custom.DistNoSqrt(goalPos, vector) && Custom.DistLess(attachedPos, vector3, maximumRadiusFromAttachedPos))
                    vector = vector3;
            }
            else if (tl.Terrain == Room.Tile.TerrainType.Floor)
            {
                vector3.y = vector2.y + 10f;
                if (Custom.DistNoSqrt(goalPos, vector3) < Custom.DistNoSqrt(goalPos, vector) && Custom.DistLess(attachedPos, vector3, maximumRadiusFromAttachedPos))
                    vector = vector3;
            }
            else if (tl.Terrain == Room.Tile.TerrainType.Slope)
            {
                var sl = room.IdentifySlope(tilePosition + eightDir);
                if (sl == Room.SlopeDirection.DownLeft || sl == Room.SlopeDirection.UpRight)
                    vector3.y = vector2.y + 10f - (vector3.x - (vector2.x - 10f));
                else if (sl == Room.SlopeDirection.DownRight || sl == Room.SlopeDirection.UpLeft)
                    vector3.y = vector2.y - 10f + (vector3.x - (vector2.x - 10f));
                if (Custom.DistNoSqrt(goalPos, vector3) < Custom.DistNoSqrt(goalPos, vector) && Custom.DistLess(attachedPos, vector3, maximumRadiusFromAttachedPos))
                    vector = vector3;
            }
            if (tl.horizontalBeam)
            {
                vector3 = new Vector2(Mathf.Clamp(goalPos.x, vector2.x - 10f, vector2.x + 10f), vector2.y);
                if (Custom.DistNoSqrt(goalPos, vector3) < Custom.DistNoSqrt(goalPos, vector) && Custom.DistLess(attachedPos, vector3, maximumRadiusFromAttachedPos))
                    vector = vector3;
            }
            if (tl.verticalBeam)
            {
                vector3 = new Vector2(vector2.x, Mathf.Clamp(goalPos.y, vector2.y - 10f, vector2.y + 10f));
                if (Custom.DistNoSqrt(goalPos, vector3) < Custom.DistNoSqrt(goalPos, vector) && Custom.DistLess(attachedPos, vector3, maximumRadiusFromAttachedPos))
                    vector = vector3;
            }
        }
        if (vector.x != -100000f && vector.y != -100000f)
        {
            self.mode = Limb.Mode.HuntAbsolutePosition;
            self.absoluteHuntPos = vector;
            self.GrabbedTerrain();
        }
    }

    public static void PushBugLimbOutOfTerrain(this BodyPart self, Room room)
    {
        // please forgive me m4rble, not only did the variables change but the code as well
        if (room.terrain is not null && self.owner.owner.Buried)
            return;
        self.terrainContact = false;
        Vector2 vector;
        if (room.terrain != null && room.terrain.TrySnapToTerrain(self.pos, self.rad, out vector, false))
        {
            
                self.terrainContact = true;
                self.pos = vector;
                self.vel.y = 0f;
                self.vel.x *= self.surfaceFric;
            
        }
        
        for (var i = 0; i < 9; i++)
        {
            var eightDir = Custom.eightDirectionsAndZero[i];
            var tlPos = Room.StaticGetTilePosition(self.pos) + eightDir;
            var tl = room.GetTile(tlPos);
            if (tl.Solid || tl.AnyWater)
            {
                vector = room.MiddleOfTile(tlPos);
                float num = 0f, num2 = 0f;
                if (self.pos.y >= vector.y - 10f && self.pos.y <= vector.y + 10f)
                {
                    if (self.lastPos.x < vector.x)
                    {
                        if (self.pos.x > vector.x - 10f - self.rad && room.GetTile(tlPos + new IntVector2(-1, 0)) is Room.Tile gtl && !gtl.Solid && !gtl.AnyWater)
                            num = vector.x - 10f - self.rad;
                    }
                    else if (self.pos.x < vector.x + 10f + self.rad && room.GetTile(tlPos + new IntVector2(1, 0)) is Room.Tile gtl && !gtl.Solid && !gtl.AnyWater)
                        num = vector.x + 10f + self.rad;
                }
                if (self.pos.x >= vector.x - 10f && self.pos.x <= vector.x + 10f)
                {
                    if (self.lastPos.y < vector.y)
                    {
                        if (self.pos.y > vector.y - 10f - self.rad && room.GetTile(tlPos + new IntVector2(0, -1)) is Room.Tile gtl && !gtl.Solid && !gtl.AnyWater)
                            num2 = vector.y - 10f - self.rad;
                    }
                    else if (self.pos.y < vector.y + 10f + self.rad && room.GetTile(tlPos + new IntVector2(0, 1)) is Room.Tile gtl && !gtl.Solid && !gtl.AnyWater)
                        num2 = vector.y + 10f + self.rad;
                }
                if (Math.Abs(self.pos.x - num) < Math.Abs(self.pos.y - num2) && num != 0f)
                {
                    self.pos.x = num;
                    self.vel.x = num - self.pos.x;
                    self.vel.y *= self.surfaceFric;
                    self.terrainContact = true;
                    continue;
                }
                if (num2 != 0f)
                {
                    self.pos.y = num2;
                    self.vel.y = num2 - self.pos.y;
                    self.vel.x *= self.surfaceFric;
                    self.terrainContact = true;
                    continue;
                }
                var vector2 = new Vector2(Mathf.Clamp(self.pos.x, vector.x - 10f, vector.x + 10f), Mathf.Clamp(self.pos.y, vector.y - 10f, vector.y + 10f));
                if (Custom.DistLess(self.pos, vector2, self.rad))
                {
                    var num3 = Vector2.Distance(self.pos, vector2);
                    var vector3 = Custom.DirVec(self.pos, vector2);
                    self.vel *= self.surfaceFric;
                    self.pos -= (self.rad - num3) * vector3;
                    self.vel -= (self.rad - num3) * vector3;
                    self.terrainContact = true;
                }
            }
            else
            {
                if (eightDir.x != 0 || tl.Terrain != Room.Tile.TerrainType.Slope)
                    continue;
                vector = room.MiddleOfTile(tlPos);
                var slope = room.IdentifySlope(tlPos);
                if (slope == Room.SlopeDirection.UpLeft)
                {
                    if (self.pos.y < vector.y - (vector.x - self.pos.x) + self.rad)
                    {
                        self.pos.y = vector.y - (vector.x - self.pos.x) + self.rad;
                        self.vel.y = 0f;
                        self.vel.x *= self.surfaceFric;
                        self.terrainContact = true;
                    }
                }
                else if (slope == Room.SlopeDirection.UpRight)
                {
                    if (self.pos.y < vector.y + (vector.x - self.pos.x) + self.rad)
                    {
                        self.pos.y = vector.y + (vector.x - self.pos.x) + self.rad;
                        self.vel.y = 0f;
                        self.vel.x *= self.surfaceFric;
                        self.terrainContact = true;
                    }
                }
                else if (slope == Room.SlopeDirection.DownLeft)
                {
                    if (self.pos.y > vector.y + (vector.x - self.pos.x) - self.rad)
                    {
                        self.pos.y = vector.y + (vector.x - self.pos.x) - self.rad;
                        self.vel.y = 0f;
                        self.vel.x *= self.surfaceFric;
                        self.terrainContact = true;
                    }
                }
                else if (slope == Room.SlopeDirection.DownRight && self.pos.y > vector.y - (vector.x - self.pos.x) - self.rad)
                {
                    self.pos.y = vector.y - (vector.x - self.pos.x) - self.rad;
                    self.vel.y = 0f;
                    self.vel.x *= self.surfaceFric;
                    self.terrainContact = true;
                }
            }
        }
    }
}