using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Mugging_Mod
{
    public class MyCustomScript : Script
    {
        private Ped nearestPed;
        private bool enableSiren = true;
        public MyCustomScript()
        {
            // Initialize your variables and set up event handlers here
            Tick += OnTick;
            KeyUp += OnKeyUp;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;

            // Add the code that should be executed after script initialization here
            AfterInitialization();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Game.Player.Character.IsInPoliceVehicle)
            {
                if (Game.IsKeyPressed(Keys.J))
                {
                    if (enableSiren)
                    {
                        Game.Player.Character.CurrentVehicle.IsSirenSilent = true;
                        enableSiren = false;
                    }
                    else
                    {
                        Game.Player.Character.CurrentVehicle.IsSirenSilent = false;
                        enableSiren = true;
                    }
                }
            }
        }

        private void OnKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Add the code that should be executed when a key is released here
        }

        private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if(e.KeyCode == Keys.E)
            {
                // Check if the player is aiming
                if (Game.Player.Character.IsAiming)
                {
                    if (Game.IsControlJustPressed(GTA.Control.Context))
                    {
                        // Perform a raycast from the player's aiming position and direction
                        Vector3 aimCoords = GameplayCamera.Direction;
                        Vector3 raycastStart = Game.Player.Character.Position + new Vector3(0, 0, Game.Player.Character.HeightAboveGround);
                        Vector3 raycastEnd = raycastStart + aimCoords * 100f; // Arbitrary distance, increase if needed
                        RaycastResult raycast = World.Raycast(raycastStart, raycastEnd, IntersectFlags.Everything);

                        // Check if the raycast hit a Ped
                        if (raycast.DidHit && raycast.HitEntity is Ped targetedPed)
                        {
                            // Save the targeted Ped to nearestPed
                            nearestPed = targetedPed;
                            Function.Call(Hash.TASK_HANDS_UP, nearestPed, -1);
                            nearestPed.BlockPermanentEvents = true; 
                            nearestPed.AlwaysKeepTask = true;
                            nearestPed.IsEnemy = true;
                            Wait(5000);
                            Game.Player.Money += 500;
                            GTA.UI.Screen.ShowHelpText("~w~You mugged the person for ~g~$500~w~.");
                            nearestPed.BlockPermanentEvents = false;
                            nearestPed.AlwaysKeepTask = false;
                            nearestPed.IsEnemy = false;
                            MakePedFlee(nearestPed);
                        }
                    }
                }
            }
        }

        private void OnAborted(object sender, EventArgs e)
        {
            // Add the code that should be executed when the script is aborted or reloaded here
            nearestPed.Delete();
        }

        private void MakePedFlee(Ped ped)
        {
            // Get the player's character
            Ped playerCharacter = Game.Player.Character;

            // Make the ped flee from the player
            Function.Call(Hash.TASK_SMART_FLEE_PED, ped, playerCharacter, 100f, -1, true, false);
        }

        private void AfterInitialization()
        {
            // Add your code to be executed after the script has been initialized
            // For example, display a notification, create objects, or set up timers
        }
    }
}
