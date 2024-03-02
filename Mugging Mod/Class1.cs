using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Mugging_Mod
{
    public class MyCustomScript : Script
    {
        private Ped nearestPed;
        private HashSet<Ped> muggedPeds = new HashSet<Ped>();

        public MyCustomScript()
        {
            Tick += OnTick;
            Aborted += OnAborted;

            AfterInitialization();
        }
        private void OnTick(object sender, EventArgs e)
        {
            if (Game.Player.Character.IsAiming)
            {
                Vector3 aimCoords = GameplayCamera.Direction;
                Vector3 raycastStart = Game.Player.Character.Position + new Vector3(0, 0, Game.Player.Character.HeightAboveGround);
                Vector3 raycastEnd = raycastStart + aimCoords * 200f;
                RaycastResult raycast = World.Raycast(raycastStart, raycastEnd, IntersectFlags.Everything);

                // Draw the raycast line for debugging
                World.DrawLine(raycastStart, raycastEnd, System.Drawing.Color.Red);

                if (raycast.DidHit && raycast.HitEntity is Ped targetedPed && !muggedPeds.Contains(targetedPed) && targetedPed.IsHuman && targetedPed != Game.Player.Character)
                {
                    nearestPed = targetedPed;
                    muggedPeds.Add(nearestPed);

                    Random random = new Random();
                    int chance = random.Next(0, 10);

                    if (chance <= 6)
                    {
                        MuggingPed(nearestPed);
                    }
                    else
                    {
                        EngagePedInCombat(nearestPed);
                    }
                }
            }
        }

        private void MuggingPed(Ped ped)
        {
            Function.Call(Hash.TASK_HANDS_UP, ped, 5000);
            GTA.UI.Screen.ShowHelpText("~w~You are mugging the person.");

            ped.BlockPermanentEvents = true;
            ped.AlwaysKeepTask = true;
            ped.IsEnemy = true;

            Wait(5000);

            // Calculate the amount of money to drop
            Random random = new Random();
            int moneyAmount = random.Next(1, 301); // Random amount between $100 and $500

            // Drop the money on the ground at the ped's location
            Vector3 pedPosition = ped.Position;
            Function.Call(GTA.Native.Hash.CREATE_MONEY_PICKUPS, pedPosition.X, pedPosition.Y, pedPosition.Z, moneyAmount, 1, 0);

            GTA.UI.Screen.ShowHelpText($"~w~Money dropped: ~g~${moneyAmount}");

            MakePedFlee(ped);
        }

        private void EngagePedInCombat(Ped ped)
        {
            Function.Call(Hash.GIVE_WEAPON_TO_PED, ped, (int)WeaponHash.Pistol, 100, true, true);
            ped.Task.ShootAt(Game.Player.Character);
            ped.BlockPermanentEvents = true;
            ped.AlwaysKeepTask = true;
            ped.IsEnemy = true;
            GTA.UI.Screen.ShowHelpText("~r~The person is fighting back!");
        }

        private void MakePedFlee(Ped ped)
        {
            // Make the ped flee from the player
            Function.Call(Hash.TASK_SMART_FLEE_PED, ped, Game.Player.Character, 100f, -1, true, false);
        }

        private void OnAborted(object sender, EventArgs e)
        {
            if (nearestPed != null && nearestPed.Exists())
            {
                nearestPed.Delete();
            }
        }

        private void AfterInitialization()
        {
            GTA.UI.Screen.ShowHelpText("Mugging Mod Loaded - v2");
        }
    }
}