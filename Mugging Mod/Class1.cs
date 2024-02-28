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
            KeyUp += OnKeyUp;
            KeyDown += OnKeyDown;
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

                if (raycast.DidHit && raycast.HitEntity is Ped targetedPed && !muggedPeds.Contains(targetedPed))
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
            GTA.UI.Screen.ShowHelpText("~w~You mugged the person for ~g~$500~w~.");
            ped.BlockPermanentEvents = true;
            ped.AlwaysKeepTask = true;
            ped.IsEnemy = true;

            Wait(5000);

            Game.Player.Money += 500;
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

        private void OnKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Key release logic here
        }

        private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Key press logic here
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
            GTA.UI.Screen.ShowHelpText("Mug Mod Loaded");
        }
    }
}