using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public static class CommonFunctions
    {
        #region Variables
        private static string _currentScenario = "";
        private static Vehicle _previousVehicle;

        internal static bool DriveToWpTaskActive = false;
        internal static bool DriveWanderTaskActive = false;
        #endregion

        #region some misc functions copied from base script
        /// <summary>
        /// Copy of <see cref="BaseScript.TriggerServerEvent(string, object[])"/>
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="args"></param>
        public static void TriggerServerEvent(string eventName, params object[] args)
        {
            BaseScript.TriggerServerEvent(eventName, args);
        }

        /// <summary>
        /// Copy of <see cref="BaseScript.TriggerEvent(string, object[])"/>
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="args"></param>
        public static void TriggerEvent(string eventName, params object[] args)
        {
            BaseScript.TriggerEvent(eventName, args);
        }

        /// <summary>
        /// Copy of <see cref="BaseScript.Delay(int)"/>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static async Task Delay(int time)
        {
            await BaseScript.Delay(time);
        }
        #endregion
        /*
        #region menu position
        public static bool RightAlignMenus() => UserDefaults.MiscRightAlignMenu;
        #endregion
        */
        #region Toggle vehicle alarm
        public static void ToggleVehicleAlarm(Vehicle vehicle)
        {
            if (vehicle != null && vehicle.Exists())
            {
                if (vehicle.IsAlarmSounding)
                {
                    // Set the duration to 0;
                    vehicle.AlarmTimeLeft = 0;
                    vehicle.IsAlarmSet = false;
                }
                else
                {
                    // Randomize duration of the alarm and start the alarm.
                    vehicle.IsAlarmSet = true;
                    vehicle.AlarmTimeLeft = new Random().Next(8000, 45000);
                    vehicle.StartAlarm();
                }
            }
        }
        #endregion

        #region lock or unlock vehicle doors
        public static async void LockOrUnlockDoors(Vehicle veh, bool lockDoors)
        {
            if (veh != null && veh.Exists())
            {
                for (int i = 0; i < 2; i++)
                {
                    int timer = GetGameTimer();
                    while (GetGameTimer() - timer < 50)
                    {
                        SoundVehicleHornThisFrame(veh.Handle);
                        await Delay(0);
                    }
                    await Delay(50);
                }
                if (lockDoors)
                {
                    Subtitle.Custom("Vehicle doors are now locked.");
                    SetVehicleDoorsLockedForAllPlayers(veh.Handle, true);
                }
                else
                {
                    Subtitle.Custom("Vehicle doors are now unlocked.");
                    SetVehicleDoorsLockedForAllPlayers(veh.Handle, false);
                }
            }
        }
        #endregion

        #region Get Localized Vehicle Display Name
        /// <summary>
        /// Get the localized model name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetVehDisplayNameFromModel(string name) => GetLabelText(GetDisplayNameFromVehicleModel((uint)GetHashKey(name)));
        #endregion

        #region DoesModelExist
        /// <summary>
        /// Does this model exist?
        /// </summary>
        /// <param name="modelName">The model name</param>
        /// <returns></returns>
        public static bool DoesModelExist(string modelName) => DoesModelExist((uint)GetHashKey(modelName));

        /// <summary>
        /// Does this model exist?
        /// </summary>
        /// <param name="modelHash">The model hash</param>
        /// <returns></returns>
        public static bool DoesModelExist(uint modelHash) => IsModelInCdimage(modelHash);
        #endregion

        #region GetVehicle from specified player id (if not specified, return the vehicle of the current player)
        /// <summary>
        /// Returns the current or last vehicle of the current player.
        /// </summary>
        /// <param name="lastVehicle"></param>
        /// <returns></returns>
        public static Vehicle GetVehicle(bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return Game.PlayerPed.LastVehicle;
            }
            else
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    return Game.PlayerPed.CurrentVehicle;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the current or last vehicle of the selected ped.
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="lastVehicle"></param>
        /// <returns></returns>
        public static Vehicle GetVehicle(Ped ped, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return ped.LastVehicle;
            }
            else
            {
                if (ped.IsInVehicle())
                {
                    return ped.CurrentVehicle;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the current or last vehicle of the selected player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="lastVehicle"></param>
        /// <returns></returns>
        public static Vehicle GetVehicle(Player player, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return player.Character.LastVehicle;
            }
            else
            {
                if (player.Character.IsInVehicle())
                {
                    return player.Character.CurrentVehicle;
                }
            }
            return null;
        }
        #endregion

        #region GetVehicleModel (uint)(hash) from Entity/Vehicle (int)
        /// <summary>
        /// Get the vehicle model hash (as uint) from the specified (int) entity/vehicle.
        /// </summary>
        /// <param name="vehicle">Entity/vehicle.</param>
        /// <returns>Returns the (uint) model hash from a (vehicle) entity.</returns>
        public static uint GetVehicleModel(int vehicle) => (uint)GetHashKey(GetEntityModel(vehicle).ToString());
        #endregion

        #region Is ped pointing
        /// <summary>
        /// Is ped pointing function returns true if the ped is currently pointing their finger.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsPedPointing(int handle)
        {
            return N_0x921ce12c489c4c41(handle);
        }

        /// <summary>
        /// Gets the finger pointing camera pitch.
        /// </summary>
        /// <returns></returns>
        public static float GetPointingPitch()
        {
            float pitch = GetGameplayCamRelativePitch();
            if (pitch < -70f)
            {
                pitch = -70f;
            }
            if (pitch > 42f)
            {
                pitch = 42f;
            }
            pitch += 70f;
            pitch /= 112f;

            return pitch;
        }
        /// <summary>
        /// Gets the finger pointing camera heading.
        /// </summary>
        /// <returns></returns>
        public static float GetPointingHeading()
        {
            float heading = GetGameplayCamRelativeHeading();
            if (heading < -180f)
            {
                heading = -180f;
            }
            if (heading > 180f)
            {
                heading = 180f;
            }
            heading += 180f;
            heading /= 360f;
            heading *= -1f;
            heading += 1f;

            return heading;
        }
        /// <summary>
        /// Returns true if finger pointing is blocked by any obstacle.
        /// </summary>
        /// <returns></returns>
        public static bool GetPointingIsBlocked()
        {
            bool hit = false;
            float rawHeading = GetGameplayCamRelativeHeading() / 90f;
            float heading = (float)MathUtil.Clamp(rawHeading, -180.0f, 180.0f);
            heading += 180.0f;
            heading /= 360.0f;
            float v1 = ((0.7f - 0.3f) * heading) + 0.3f;
            Vector3 pos0 = new Vector3(-0.2f, v1, 0.6f);
            Vector3 rot = new Vector3(0f, 0f, rawHeading);
            Vector3 vec1 = Vector3.Zero;

            // pos0, rot
            // ----
            float f0 = (float)Math.Cos(rot.X);
            float f1 = (float)Math.Sin(rot.X);
            vec1.X = pos0.X;
            vec1.Y = (f0 * pos0.Y) - (f1 * pos0.Z);
            vec1.Z = (f1 * pos0.Y) + (f0 * pos0.Z);
            pos0 = vec1;

            // ----
            f0 = (float)Math.Cos(rot.Y);
            f1 = (float)Math.Sin(rot.Y);
            vec1.X = (f0 * pos0.X) + (f1 * pos0.Z);
            vec1.Y = pos0.Y;
            vec1.Z = (f0 * pos0.Z) - (f1 * pos0.X);
            pos0 = vec1;

            // ----
            f0 = (float)Math.Cos(rot.Z);
            f1 = (float)Math.Sin(rot.Z);
            vec1.X = (f0 * pos0.X) - (f1 * pos0.Y);
            vec1.Y = (f1 * pos0.X) + (f0 * pos0.Y);
            vec1.Z = pos0.Z;
            pos0 = vec1;

            Vector3 pos1 = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, pos0.X, pos0.Y, pos0.Z);
            int handle = StartShapeTestCapsule(pos1.X, pos1.Y, (pos1.Z - 0.2f), pos1.X, pos1.Y, (pos1.Z + 0.2f), 0.4f, 95, Game.PlayerPed.Handle, 7);
            Vector3 outPos = Vector3.Zero;
            Vector3 surfaceNormal = Vector3.Zero;
            int entityHit = 0;
            GetShapeTestResult(handle, ref hit, ref outPos, ref surfaceNormal, ref entityHit);

            
            return hit;
        }
        #endregion

        #region Drive Tasks (WIP)
        /// <summary>
        /// Drives to waypoint
        /// </summary>
        public static void DriveToWp(int style = 0)
        {

            ClearPedTasks(Game.PlayerPed.Handle);
            DriveWanderTaskActive = false;
            DriveToWpTaskActive = true;

            Vector3 waypoint = World.WaypointPosition;

            Vehicle veh = GetVehicle();
            uint model = (uint)veh.Model.Hash;

            SetDriverAbility(Game.PlayerPed.Handle, 1f);
            SetDriverAggressiveness(Game.PlayerPed.Handle, 0f);

            TaskVehicleDriveToCoordLongrange(Game.PlayerPed.Handle, veh.Handle, waypoint.X, waypoint.Y, waypoint.Z, GetVehicleModelMaxSpeed(model), style, 10f);
        }

        /// <summary>
        /// Drives around the area.
        /// </summary>
        public static void DriveWander(int style = 0)
        {
            ClearPedTasks(Game.PlayerPed.Handle);
            DriveWanderTaskActive = true;
            DriveToWpTaskActive = false;

            Vehicle veh = GetVehicle();
            uint model = (uint)veh.Model.Hash;

            SetDriverAbility(Game.PlayerPed.Handle, 1f);
            SetDriverAggressiveness(Game.PlayerPed.Handle, 0f);

            TaskVehicleDriveWander(Game.PlayerPed.Handle, veh.Handle, GetVehicleModelMaxSpeed(model), style);
        }
        #endregion

        #region Quit session & Quit game
        /// <summary>
        /// Quit the current network session, but leaves you connected to the server so addons/resources are still streamed.
        /// </summary>
        public static void QuitSession() => NetworkSessionEnd(true, true);

        /// <summary>
        /// Quit the game after 5 seconds.
        /// </summary>
        public static async void QuitGame()
        {
            Notify.Info("The game will exit in 5 seconds.");
            Debug.WriteLine("Game will be terminated in 5 seconds, because the player used the Quit Game option in vMenu.");
            await BaseScript.Delay(5000);
            ForceSocialClubUpdate(); // bye bye
        }
        #endregion

        

        #region Teleport To Coords
        /// <summary>
        /// Teleport to the specified <see cref="pos"/>.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="safeModeDisabled"></param>
        /// <returns></returns>
        public static async Task TeleportToCoords(Vector3 pos, bool safeModeDisabled = false)
        {
            if (!safeModeDisabled)
            {
                // Is player in a vehicle and the driver? Then we'll use that to teleport.
                var veh = GetVehicle();
                bool inVehicle() => veh != null && veh.Exists() && Game.PlayerPed == veh.Driver;

                bool vehicleRestoreVisibility = inVehicle() && veh.IsVisible;
                bool pedRestoreVisibility = Game.PlayerPed.IsVisible;

                // Freeze vehicle or player location and fade out the entity to the network.
                if (inVehicle())
                {
                    veh.IsPositionFrozen = true;
                    if (veh.IsVisible)
                    {
                        NetworkFadeOutEntity(veh.Handle, true, false);
                    }
                }
                else
                {
                    ClearPedTasksImmediately(Game.PlayerPed.Handle);
                    Game.PlayerPed.IsPositionFrozen = true;
                    if (Game.PlayerPed.IsVisible)
                    {
                        NetworkFadeOutEntity(Game.PlayerPed.Handle, true, false);
                    }
                }

                // Fade out the screen and wait for it to be faded out completely.
                DoScreenFadeOut(500);
                while (!IsScreenFadedOut())
                {
                    await Delay(0);
                }

                // This will be used to get the return value from the groundz native.
                float groundZ = 850.0f;

                // Bool used to determine if the groundz coord could be found.
                bool found = false;

                // Loop from 950 to 0 for the ground z coord, and take away 25 each time.
                for (float zz = 950.0f; zz >= 0f; zz -= 25f)
                {
                    float z = zz;
                    // The z coord is alternating between a very high number, and a very low one.
                    // This way no matter the location, the actual ground z coord will always be found the fastest.
                    // If going from top > bottom then it could take a long time to reach the bottom. And vice versa.
                    // By alternating top/bottom each iteration, we minimize the time on average for ANY location on the map.
                    if (zz % 2 != 0)
                    {
                        z = 950f - zz;
                    }

                    // Request collision at the coord. I've never actually seen this do anything useful, but everyone keeps telling me this is needed.
                    // It doesn't matter to get the ground z coord, and neither does it actually prevent entities from falling through the map, nor does
                    // it seem to load the world ANY faster than without, but whatever.
                    RequestCollisionAtCoord(pos.X, pos.Y, z);

                    // Request a new scene. This will trigger the world to be loaded around that area.
                    NewLoadSceneStart(pos.X, pos.Y, z, pos.X, pos.Y, z, 50f, 0);

                    // Timer to make sure things don't get out of hand (player having to wait forever to get teleported if something fails).
                    int tempTimer = GetGameTimer();

                    // Wait for the new scene to be loaded.
                    while (IsNetworkLoadingScene())
                    {
                        // If this takes longer than 1 second, just abort. It's not worth waiting that long.
                        if (GetGameTimer() - tempTimer > 1000)
                        {
                            Log("Waiting for the scene to load is taking too long (more than 1s). Breaking from wait loop.");
                            break;
                        }

                        await Delay(0);
                    }

                    // If the player is in a vehicle, teleport the vehicle to this new position.
                    if (inVehicle())
                    {
                        SetEntityCoords(veh.Handle, pos.X, pos.Y, z, false, false, false, true);
                    }
                    // otherwise, teleport the player to this new position.
                    else
                    {
                        SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, z, false, false, false, true);
                    }

                    // Reset the timer.
                    tempTimer = GetGameTimer();

                    // Wait for the collision to be loaded around the entity in this new location.
                    while (!HasCollisionLoadedAroundEntity(Game.PlayerPed.Handle))
                    {
                        // If this takes too long, then just abort, it's not worth waiting that long since we haven't found the real ground coord yet anyway.
                        if (GetGameTimer() - tempTimer > 1000)
                        {
                            Log("Waiting for the collision is taking too long (more than 1s). Breaking from wait loop.");
                            break;
                        }
                        await Delay(0);
                    }

                    // Check for a ground z coord.
                    found = GetGroundZFor_3dCoord(pos.X, pos.Y, z, ref groundZ, false);

                    // If we found a ground z coord, then teleport the player (or their vehicle) to that new location and break from the loop.
                    if (found)
                    {
                        Log($"Ground coordinate found: {groundZ}");
                        if (inVehicle())
                        {
                            SetEntityCoords(veh.Handle, pos.X, pos.Y, groundZ, false, false, false, true);

                            // We need to unfreeze the vehicle because sometimes having it frozen doesn't place the vehicle on the ground properly.
                            veh.IsPositionFrozen = false;
                            veh.PlaceOnGround();
                            // Re-freeze until screen is faded in again.
                            veh.IsPositionFrozen = true;
                        }
                        else
                        {
                            SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, groundZ, false, false, false, true);
                        }
                        break;
                    }

                    // Wait 10ms before trying the next location.
                    await Delay(10);
                }

                // If the loop ends but the ground z coord has not been found yet, then get the nearest vehicle node as a fail-safe coord.
                if (!found)
                {
                    var safePos = pos;
                    GetNthClosestVehicleNode(pos.X, pos.Y, pos.Z, 0, ref safePos, 0, 0, 0);

                    // Notify the user that the ground z coord couldn't be found, so we will place them on a nearby road instead.
                    Notify.Alert("Could not find a safe ground coord. Placing you on the nearest road instead.");
                    Log("Could not find a safe ground coord. Placing you on the nearest road instead.");

                    // Teleport vehicle, or player.
                    if (inVehicle())
                    {
                        SetEntityCoords(veh.Handle, safePos.X, safePos.Y, safePos.Z, false, false, false, true);
                        veh.IsPositionFrozen = false;
                        veh.PlaceOnGround();
                        veh.IsPositionFrozen = true;
                    }
                    else
                    {
                        SetEntityCoords(Game.PlayerPed.Handle, safePos.X, safePos.Y, safePos.Z, false, false, false, true);
                    }
                }

                // Once the teleporting is done, unfreeze vehicle or player and fade them back in.
                if (inVehicle())
                {
                    if (vehicleRestoreVisibility)
                    {
                        NetworkFadeInEntity(veh.Handle, true);
                        if (!pedRestoreVisibility)
                        {
                            Game.PlayerPed.IsVisible = false;
                        }
                    }
                    veh.IsPositionFrozen = false;
                }
                else
                {
                    if (pedRestoreVisibility)
                    {
                        NetworkFadeInEntity(Game.PlayerPed.Handle, true);
                    }
                    Game.PlayerPed.IsPositionFrozen = false;
                }

                // Fade screen in and reset the camera angle.
                DoScreenFadeIn(500);
                SetGameplayCamRelativePitch(0.0f, 1.0f);
            }

            // Disable safe teleporting and go straight to the specified coords.
            else
            {
                RequestCollisionAtCoord(pos.X, pos.Y, pos.Z);

                // Teleport directly to the coords without trying to get a safe z pos.
                if (Game.PlayerPed.IsInVehicle() && GetVehicle().Driver == Game.PlayerPed)
                {
                    SetEntityCoords(GetVehicle().Handle, pos.X, pos.Y, pos.Z, false, false, false, true);
                }
                else
                {
                    SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, pos.Z, false, false, false, true);
                }
            }
        }
        #endregion

        #region ToProperString()
        /// <summary>
        /// Converts a PascalCaseString to a Propper Case String.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>Input string converted to a normal sentence.</returns>
        public static string ToProperString(string inputString)
        {
            var outputString = "";
            var prevUpper = true;
            foreach (char c in inputString)
            {
                if (char.IsLetter(c) && c != ' ' && c == char.Parse(c.ToString().ToUpper()))
                {
                    if (prevUpper)
                    {
                        outputString += $"{c}";
                    }
                    else
                    {
                        outputString += $" {c}";
                    }
                    prevUpper = true;
                }
                else
                {
                    prevUpper = false;
                    outputString += c.ToString();
                }
            }
            while (outputString.IndexOf("  ") != -1)
            {
                outputString = outputString.Replace("  ", " ");
            }
            return outputString;
        }
        #endregion

        

        #region Data parsing functions
        /// <summary>
        /// Converts a simple json string (only containing (string) key : (string) value).
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, string> JsonToDictionary(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        #endregion

        

        #region StringToStringArray
        /// <summary>
        /// Converts the inputString into a string[] (array).
        /// Each string in the array is up to 99 characters long at max.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string[] StringToArray(string inputString)
        {
            return CitizenFX.Core.UI.Screen.StringToArray(inputString);
        }
        #endregion

        #region Hud Functions
        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition) =>
            DrawTextOnScreen(text, xPosition, yPosition, size: 0.48f);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, CitizenFX.Core.UI.Alignment.Left);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, 6);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        /// <param name="font">Specify the font to use (0-8).</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, font, false);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        /// <param name="font">Specify the font to use (0-8).</param>
        /// <param name="disableTextOutline">Disables the default text outline.</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font, bool disableTextOutline)
        {
            if (IsHudPreferenceSwitchedOn() && Hud.IsVisible && !MainMenu.MiscSettingsMenu.HideHud && !IsPlayerSwitchInProgress() && IsScreenFadedIn() && !IsPauseMenuActive() && !IsFrontendFading() && !IsPauseMenuRestarting() && !IsHudHidden())
            {
                SetTextFont(font);
                SetTextScale(1.0f, size);
                if (justification == CitizenFX.Core.UI.Alignment.Right)
                {
                    SetTextWrap(0f, xPosition);
                }
                SetTextJustification((int)justification);
                if (!disableTextOutline) { SetTextOutline(); }
                BeginTextCommandDisplayText("STRING");
                AddTextComponentSubstringPlayerName(text);
                EndTextCommandDisplayText(xPosition, yPosition);
            }
        }
        #endregion

        #region ped info struct
        public struct PedInfo
        {
            public int version;
            public uint model;
            public bool isMpPed;
            public Dictionary<int, int> props;
            public Dictionary<int, int> propTextures;
            public Dictionary<int, int> drawableVariations;
            public Dictionary<int, int> drawableVariationTextures;
        };
        #endregion

        #region Set Player Skin
        /// <summary>
        /// Sets the player's model to the provided modelName.
        /// </summary>
        /// <param name="modelName">The model name.</param>
        public static async Task SetPlayerSkin(string modelName, PedInfo pedCustomizationOptions, bool keepWeapons = true) => await SetPlayerSkin((uint)GetHashKey(modelName), pedCustomizationOptions, keepWeapons);

        /// <summary>
        /// Sets the player's model to the provided modelHash.
        /// </summary>
        /// <param name="modelHash">The model hash.</param>
        public static async Task SetPlayerSkin(uint modelHash, PedInfo pedCustomizationOptions, bool keepWeapons = true)
        {
            if (IsModelInCdimage(modelHash))
            {
                if (keepWeapons)
                {
                    //SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn");
                    Log("saved from SetPlayerSkin()");
                }
                RequestModel(modelHash);
                while (!HasModelLoaded(modelHash))
                {
                    await Delay(0);
                }

                if ((uint)GetEntityModel(Game.PlayerPed.Handle) != modelHash) // only change skins if the player is not yet using the new skin.
                {
                    // check if the ped is in a vehicle.
                    bool wasInVehicle = Game.PlayerPed.IsInVehicle();
                    Vehicle veh = Game.PlayerPed.CurrentVehicle;
                    VehicleSeat seat = Game.PlayerPed.SeatIndex;

                    int maxHealth = Game.PlayerPed.MaxHealth;
                    int maxArmour = Game.Player.MaxArmor;
                    int health = Game.PlayerPed.Health;
                    int armour = Game.PlayerPed.Armor;

                    // set the model
                    SetPlayerModel(Game.Player.Handle, modelHash);

                    Game.Player.MaxArmor = maxArmour;
                    Game.PlayerPed.MaxHealth = maxHealth;
                    Game.PlayerPed.Health = health;
                    Game.PlayerPed.Armor = armour;

                    // warp ped into vehicle if the player was in a vehicle.
                    if (wasInVehicle && veh != null && seat != VehicleSeat.None)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, true);
                        int tmpTimer = GetGameTimer();
                        while (!Game.PlayerPed.IsInVehicle(veh))
                        {
                            // if it takes too long, stop trying to teleport.
                            if (GetGameTimer() - tmpTimer > 1000)
                            {
                                break;
                            }
                            ClearPedTasks(Game.PlayerPed.Handle);
                            await Delay(0);
                            TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, veh.Handle, (int)seat);
                        }
                        FreezeEntityPosition(Game.PlayerPed.Handle, false);
                    }
                }

                // Reset some stuff.
                SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                ClearAllPedProps(Game.PlayerPed.Handle);
                ClearPedDecorations(Game.PlayerPed.Handle);
                ClearPedFacialDecorations(Game.PlayerPed.Handle);

                if (pedCustomizationOptions.version == 1)
                {
                    var ped = Game.PlayerPed.Handle;
                    for (var drawable = 0; drawable < 21; drawable++)
                    {
                        SetPedComponentVariation(ped, drawable, pedCustomizationOptions.drawableVariations[drawable],
                            pedCustomizationOptions.drawableVariationTextures[drawable], 1);
                    }

                    for (var i = 0; i < 21; i++)
                    {
                        int prop = pedCustomizationOptions.props[i];
                        int propTexture = pedCustomizationOptions.propTextures[i];
                        if (prop == -1 || propTexture == -1)
                        {
                            ClearPedProp(ped, i);
                        }
                        else
                        {
                            SetPedPropIndex(ped, i, prop, propTexture, true);
                        }
                    }
                }
                else if (pedCustomizationOptions.version == -1)
                {
                    // do nothing.
                }
                else
                {
                    // notify user of unsupported version
                    Notify.Error("This is an unsupported saved ped version. Cannot restore appearance. :(");
                }
                if (keepWeapons)
                {
                    await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", false, true, false);
                }
                if (modelHash == (uint)GetHashKey("mp_f_freemode_01") || modelHash == (uint)GetHashKey("mp_m_freemode_01"))
                {
                    //var headBlendData = Game.PlayerPed.GetHeadBlendData();
                    if (pedCustomizationOptions.version == -1)
                    {
                        SetPedHeadBlendData(Game.PlayerPed.Handle, 0, 0, 0, 0, 0, 0, 0.5f, 0.5f, 0f, false);
                        while (!HasPedHeadBlendFinished(Game.PlayerPed.Handle))
                        {
                            await Delay(0);
                        }
                    }
                }
                SetModelAsNoLongerNeeded(modelHash);
            }
            else
            {
                Notify.Error(CommonErrors.InvalidModel);
            }
        }

        /// <summary>
        /// Set the player model by asking for user input.
        /// </summary>
        public static async void SpawnPedByName()
        {
            string input = await GetUserInput(windowTitle: "Enter Ped Model Name", maxInputLength: 30);
            if (!string.IsNullOrEmpty(input))
            {
                await SetPlayerSkin((uint)GetHashKey(input), new PedInfo() { version = -1 });
            }
            else
            {
                Notify.Error(CommonErrors.InvalidModel);
            }
        }
        #endregion

        #region Save Ped Model + Customizations
        /// <summary>
        /// Saves the current player ped.
        /// </summary>
        public static async Task<bool> SavePed(string forceName = null, bool overrideExistingPed = false)
        {
            string name = forceName;
            if (string.IsNullOrEmpty(name))
            {
                // Get the save name.
                name = await GetUserInput(windowTitle: "Enter a ped save name", maxInputLength: 30);
            }

            // If the save name is not invalid.
            if (!string.IsNullOrEmpty(name))
            {
                // Create a dictionary to store all data in.
                PedInfo data = new PedInfo();

                // Get the ped.
                int ped = Game.PlayerPed.Handle;

                data.version = 1;
                // Get the ped model hash & add it to the dictionary.
                uint model = (uint)GetEntityModel(ped);
                data.model = model;

                // Loop through all drawable variations.
                var drawables = new Dictionary<int, int>();
                var drawableTextures = new Dictionary<int, int>();
                for (var i = 0; i < 21; i++)
                {
                    int drawable = GetPedDrawableVariation(ped, i);
                    int textureVariation = GetPedTextureVariation(ped, i);
                    drawables.Add(i, drawable);
                    drawableTextures.Add(i, textureVariation);
                }
                data.drawableVariations = drawables;
                data.drawableVariationTextures = drawableTextures;

                var props = new Dictionary<int, int>();
                var propTextures = new Dictionary<int, int>();
                // Loop through all prop variations.
                for (var i = 0; i < 21; i++)
                {
                    int prop = GetPedPropIndex(ped, i);
                    int propTexture = GetPedPropTextureIndex(ped, i);
                    props.Add(i, prop);
                    propTextures.Add(i, propTexture);
                }
                data.props = props;
                data.propTextures = propTextures;

                data.isMpPed = (model == (uint)GetHashKey("mp_f_freemode_01") || model == (uint)GetHashKey("mp_m_freemode_01"));
                if (data.isMpPed)
                {
                    Notify.Alert("Note, you should probably use the MP Character creator if you want more advanced features. Saving Multiplayer characters with this function does NOT save a lot of the online peds customization.");
                }

                // Try to save the data, and save the result in a variable.
                bool saveSuccessful;
                if (name == "vMenu_tmp_saved_ped")
                {
                    saveSuccessful = StorageManager.SavePedInfo(name, data, true);
                }
                else
                {
                    saveSuccessful = StorageManager.SavePedInfo("ped_" + name, data, overrideExistingPed);
                }

                //if (name != "vMenu_tmp_saved_ped") // only send a notification if the save wasn't triggered because the player died.
                //{
                //    // If the save was successfull.
                //    if (saveSuccessful)
                //    {
                //        //Notify.Success("Ped saved.");
                //    }
                //    // Save was not successfull.
                //    else
                //    {
                //        Notify.Error(CommonErrors.SaveNameAlreadyExists, placeholderValue: name);
                //    }
                //}

                return saveSuccessful;
            }
            // User cancelled the saving or they did not enter a valid name.
            else
            {
                Notify.Error(CommonErrors.InvalidSaveName);
            }
            return false;
        }
        #endregion

        #region Load Saved Ped
        /// <summary>
        /// Load the saved ped and spawn it.
        /// </summary>
        /// <param name="savedName">The ped saved name</param>
        public static async void LoadSavedPed(string savedName, bool restoreWeapons)
        {
            if (savedName != "vMenu_tmp_saved_ped")
            {
                PedInfo pi = StorageManager.GetSavedPedInfo("ped_" + savedName);
                Log(JsonConvert.SerializeObject(pi));
                await SetPlayerSkin(pi.model, pi, restoreWeapons);
            }
            else
            {
                PedInfo pi = StorageManager.GetSavedPedInfo(savedName);
                Log(JsonConvert.SerializeObject(pi));
                await SetPlayerSkin(pi.model, pi, restoreWeapons);
                DeleteResourceKvp("vMenu_tmp_saved_ped");
            }

        }

        /// <summary>
        /// Checks if the ped is saved from before the player died.
        /// </summary>
        /// <returns></returns>
        public static bool IsTempPedSaved()
        {
            if (!string.IsNullOrEmpty(GetResourceKvpString("vMenu_tmp_saved_ped")))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region saved ped json string to ped info
        /// <summary>
        /// Load and convert json ped info into PedInfo struct.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static PedInfo JsonToPedInfo(string json)
        {
            return JsonConvert.DeserializeObject<PedInfo>(json);
        }
        #endregion

        #region Save and restore weapon loadouts when changing models
        //private struct WeaponInfo
        //{
        //    public int Ammo;
        //    public uint Hash;
        //    public List<uint> Components;
        //    public int Tint;
        //}

        //private static List<WeaponInfo> weaponsList = new List<WeaponInfo>();

        ///// <summary>
        ///// Saves all current weapons and components.
        ///// </summary>
        //public static async Task SaveWeaponLoadout()
        //{
        //    weaponsList.Clear();
        //    foreach (ValidWeapon vw in ValidWeapons.WeaponList)
        //    {
        //        if (HasPedGotWeapon(Game.PlayerPed.Handle, vw.Hash, false))
        //        {
        //            List<uint> components = new List<uint>();
        //            if (vw.Components != null && vw.Components.Count > 0)
        //            {
        //                foreach (var c in vw.Components)
        //                {
        //                    if (HasPedGotWeaponComponent(Game.PlayerPed.Handle, vw.Hash, c.Value))
        //                    {
        //                        components.Add(c.Value);
        //                    }
        //                }
        //            }
        //            weaponsList.Add(new WeaponInfo()
        //            {
        //                Ammo = GetAmmoInPedWeapon(Game.PlayerPed.Handle, vw.Hash),
        //                Components = components,
        //                Hash = vw.Hash,
        //                Tint = GetPedWeaponTintIndex(Game.PlayerPed.Handle, vw.Hash)
        //            });

        //        }
        //    }
        //    await Delay(0);
        //}

        ///// <summary>
        ///// Restores all weapons and components
        ///// </summary>
        //public static async void RestoreWeaponLoadout()
        //{
        //    await Delay(0);
        //    if (weaponsList.Count > 0)
        //    {
        //        foreach (WeaponInfo wi in weaponsList)
        //        {
        //            GiveWeaponToPed(Game.PlayerPed.Handle, wi.Hash, wi.Ammo, false, false);
        //            if (wi.Components.Count > 0)
        //            {
        //                foreach (var wc in wi.Components)
        //                {
        //                    GiveWeaponComponentToPed(Game.PlayerPed.Handle, wi.Hash, wc);
        //                }
        //            }
        //            // sometimes causes problems if this is not manually set.
        //            SetPedAmmo(Game.PlayerPed.Handle, wi.Hash, wi.Ammo);
        //            SetPedWeaponTintIndex(Game.PlayerPed.Handle, wi.Hash, wi.Tint);
        //        }
        //    }
        //}
        #endregion

        #region Get "Header" Menu Item
        /// <summary>
        /// Get a header menu item (text-centered, disabled MenuItem)
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static MenuItem GetSpacerMenuItem(string title, string description = null)
        {
            string output = "~h~";
            int length = title.Length;
            int totalSize = 80 - length;

            for (var i = 0; i < totalSize / 2 - (length / 2); i++)
            {
                output += " ";
            }
            output += title;
            MenuItem item = new MenuItem(output, description ?? "")
            {
                Enabled = false
            };
            return item;
        }
        #endregion

        #region Log Function
        /// <summary>
        /// Print data to the console and save it to the CitizenFX.log file. Only when vMenu debugging mode is enabled.
        /// </summary>
        /// <param name="data"></param>
        public static void Log(string data)
        {
            if (MainMenu.DebugMode) Debug.WriteLine(@data);
        }
        #endregion

        #region Get Currently Opened Menu
        /// <summary>
        /// Returns the currently opened menu, if no menu is open, it'll return null.
        /// </summary>
        /// <returns></returns>
        public static Menu GetOpenMenu()
        {
            return MenuController.GetCurrentMenu();
        }
        #endregion

        #region Weapon Options
        /// <summary>
        /// Set the ammo for all weapons in inventory to the custom amount entered by the user.
        /// </summary>
        public static async void SetAllWeaponsAmmo()
        {
            string inputAmmo = await GetUserInput(windowTitle: "Enter Ammo Amount", defaultText: "100");
            if (!string.IsNullOrEmpty(inputAmmo))
            {
                if (int.TryParse(inputAmmo, out int ammo))
                {
                    foreach (ValidWeapon vw in ValidWeapons.WeaponList)
                    {
                        if (HasPedGotWeapon(Game.PlayerPed.Handle, vw.Hash, false))
                        {
                            SetPedAmmo(Game.PlayerPed.Handle, vw.Hash, ammo);
                        }
                    }
                }
                else
                {
                    Notify.Error("You did not enter a valid number.");
                }
            }
            else
            {
                Notify.Error(CommonErrors.InvalidInput);
            }
        }

        /// <summary>
        /// Spawn a weapon by asking the player for the weapon name.
        /// </summary>
        public static async void SpawnCustomWeapon()
        {
            int ammo = 900;
            string inputName = await GetUserInput(windowTitle: "Enter Weapon Model Name", maxInputLength: 30);
            if (!string.IsNullOrEmpty(inputName))
            {
                if (!ValidWeapons.weaponPermissions.ContainsKey(inputName.ToLower()))
                {
                    if (!IsAllowed(Permission.WPSpawn))
                    {
                        Notify.Error("Sorry, you do not have permission to spawn this weapon.");
                        return;
                    }
                }
                else
                {
                    if (!IsAllowed(ValidWeapons.weaponPermissions[inputName.ToLower()]))
                    {
                        Notify.Error("Sorry, you are not allowed to spawn that weapon by name because it's a restricted weapon.");
                        return;
                    }
                }

                var model = (uint)GetHashKey(inputName.ToUpper());

                if (IsWeaponValid(model))
                {
                    GiveWeaponToPed(Game.PlayerPed.Handle, model, ammo, false, true);
                    Notify.Success("Added weapon to inventory.");
                }
                else
                {
                    Notify.Error($"This ({inputName}) is not a valid weapon model name, or the model hash ({model}) could not be found in the game files.");
                }
            }
            else
            {
                Notify.Error(CommonErrors.InvalidInput);
            }
        }
        #endregion

        

        /// <summary>
        /// Spawns a saved weapons loadout.
        /// </summary>
        /// <param name="saveName"></param>
        /// <param name="appendWeapons"></param>
        public static async Task SpawnWeaponLoadoutAsync(string saveName, bool appendWeapons, bool ignoreSettingsAndPerms, bool dontNotify)
        {

            var loadout = GetSavedWeaponLoadout(saveName);

            if (!ignoreSettingsAndPerms && saveName == "vmenu_temp_weapons_loadout_before_respawn")
            {
                string name = GetResourceKvpString("vmenu_string_default_loadout") ?? saveName;

                string kvp = GetResourceKvpString(name) ?? GetResourceKvpString("vmenu_temp_weapons_loadout_before_respawn");

                // if not allowed to use loadouts, fall back to normal restoring of weapons.
                if (MainMenu.WeaponLoadoutsMenu == null || !MainMenu.WeaponLoadoutsMenu.WeaponLoadoutsSetLoadoutOnRespawn || !IsAllowed(Permission.WLEquipOnRespawn))
                {
                    kvp = GetResourceKvpString("vmenu_temp_weapons_loadout_before_respawn");

                    if (!MainMenu.MiscSettingsMenu.RestorePlayerWeapons || !IsAllowed(Permission.MSRestoreWeapons))
                    {
                        // return because normal weapon restoring is not enabled or not allowed.
                        loadout = new List<ValidWeapon>();
                    }
                }

                if (string.IsNullOrEmpty(kvp))
                {
                    loadout = new List<ValidWeapon>();
                }
                else
                {
                    loadout = JsonConvert.DeserializeObject<List<ValidWeapon>>(kvp);
                }
            }

            Log(JsonConvert.SerializeObject(loadout));
            if (loadout.Count > 0)
            {
                // Remove all current weapons if we're not supposed to append this loadout.
                if (!appendWeapons)
                {
                    Game.PlayerPed.Weapons.RemoveAll();
                }

                // Check if any weapon is not allowed.
                if (!ignoreSettingsAndPerms && loadout.Any((wp) => !IsAllowed(wp.Perm)))
                {
                    Notify.Alert("One or more weapon(s) in this saved loadout are not allowed on this server. Those weapons will not be loaded.");
                }

                foreach (ValidWeapon w in loadout)
                {
                    if (ignoreSettingsAndPerms || IsAllowed(w.Perm))
                    {
                        // Give the weapon
                        GiveWeaponToPed(Game.PlayerPed.Handle, w.Hash, w.CurrentAmmo > -1 ? w.CurrentAmmo : w.GetMaxAmmo, false, false);

                        // Add components
                        if (w.Components.Count > 0)
                        {
                            foreach (var wc in w.Components)
                            {
                                if (DoesWeaponTakeWeaponComponent(w.Hash, wc.Value))
                                {
                                    GiveWeaponComponentToPed(Game.PlayerPed.Handle, w.Hash, wc.Value);
                                    int timer = GetGameTimer();
                                    while (!HasPedGotWeaponComponent(Game.PlayerPed.Handle, w.Hash, wc.Value))
                                    {
                                        await Delay(0);
                                        if (GetGameTimer() - timer > 1000)
                                        {
                                            // took too long
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // Set tint
                        SetPedWeaponTintIndex(Game.PlayerPed.Handle, w.Hash, w.CurrentTint);

                        if (w.CurrentAmmo > 0)
                        {
                            int ammo = w.CurrentAmmo;
                            if (w.CurrentAmmo > w.GetMaxAmmo)
                            {
                                ammo = w.GetMaxAmmo;
                            }
                            var doIt = false;
                            while (GetAmmoInPedWeapon(Game.PlayerPed.Handle, w.Hash) != ammo && w.CurrentAmmo != -1)
                            {
                                if (doIt)
                                {
                                    SetCurrentPedWeapon(Game.PlayerPed.Handle, w.Hash, true);
                                }
                                doIt = true;
                                int ammoInClip = GetMaxAmmoInClip(Game.PlayerPed.Handle, w.Hash, false);
                                if (ammoInClip > ammo)
                                {
                                    ammoInClip = ammo;
                                }
                                SetAmmoInClip(Game.PlayerPed.Handle, w.Hash, ammoInClip);
                                SetPedAmmo(Game.PlayerPed.Handle, w.Hash, ammo > -1 ? ammo : w.GetMaxAmmo);
                                Log($"waiting for ammo in {w.Name}");
                                await Delay(0);
                            }
                        }
                    }
                }

                // Set the current weapon to 'unarmed'.
                SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("weapon_unarmed"), true);

                if (!(saveName == "vmenu_temp_weapons_loadout_before_respawn" || dontNotify))
                    Notify.Success("Weapon loadout spawned.");
            }
        }

        /// <summary>
        /// Saves all current weapons the ped has. It does not check if the save already exists!
        /// </summary>
        /// <returns>A bool indicating if the save was successful</returns>
        public static bool SaveWeaponLoadout(string saveName)
        {
            // Stop if the savename is invalid.
            if (string.IsNullOrEmpty(saveName))
            {
                return false;
            }

            List<ValidWeapon> pedWeapons = new List<ValidWeapon>();

            // Loop through all possible weapons.
            foreach (var vw in ValidWeapons.WeaponList)
            {
                // Check if the ped has that specific weapon.
                if (HasPedGotWeapon(Game.PlayerPed.Handle, vw.Hash, false))
                {
                    // Create the weapon data with basic info.
                    ValidWeapon weapon = new ValidWeapon()
                    {
                        Hash = vw.Hash,
                        CurrentTint = GetPedWeaponTintIndex(Game.PlayerPed.Handle, vw.Hash),
                        Name = vw.Name,
                        Perm = vw.Perm,
                        SpawnName = vw.SpawnName,
                        Components = new Dictionary<string, uint>()
                    };

                    weapon.CurrentAmmo = GetAmmoInPedWeapon(Game.PlayerPed.Handle, vw.Hash);


                    // Check for and add components if applicable.
                    foreach (var comp in vw.Components)
                    {
                        if (DoesWeaponTakeWeaponComponent(weapon.Hash, comp.Value))
                        {
                            if (HasPedGotWeaponComponent(Game.PlayerPed.Handle, vw.Hash, comp.Value))
                            {
                                weapon.Components.Add(comp.Key, comp.Value);
                            }
                        }
                    }

                    // Add the weapon info to the list.
                    pedWeapons.Add(weapon);
                }
            }

            // Convert the weapons list to json string.
            string json = JsonConvert.SerializeObject(pedWeapons);

            // Save it.
            SetResourceKvp(saveName, json);

            // If the saved value is the same as the string we just provided, then the save was successful.
            if ((GetResourceKvpString(saveName) ?? "{}") == json)
            {
                Log("weapons save good.");
                return true;
            }

            // Save was unsuccessful.
            return false;
        }
        

        #region Set Player Walking Style
        /// <summary>
        /// Sets the walking style for this player.
        /// </summary>
        /// <param name="walkingStyle"></param>
        public static async void SetWalkingStyle(string walkingStyle)
        {
            if (IsPedModel(Game.PlayerPed.Handle, (uint)GetHashKey("mp_f_freemode_01")) || IsPedModel(Game.PlayerPed.Handle, (uint)GetHashKey("mp_m_freemode_01")))
            {
                bool isPedMale = IsPedModel(Game.PlayerPed.Handle, (uint)GetHashKey("mp_m_freemode_01"));
                ClearPedAlternateMovementAnim(Game.PlayerPed.Handle, 0, 1f);
                ClearPedAlternateMovementAnim(Game.PlayerPed.Handle, 1, 1f);
                ClearPedAlternateMovementAnim(Game.PlayerPed.Handle, 2, 1f);
                ClearPedAlternateWalkAnim(Game.PlayerPed.Handle, 1f);
                string animDict = null;
                if (walkingStyle == "Injured")
                {
                    animDict = isPedMale ? "move_m@injured" : "move_f@injured";
                }
                else if (walkingStyle == "Tough Guy")
                {
                    animDict = isPedMale ? "move_m@tough_guy@" : "move_f@tough_guy@";
                }
                else if (walkingStyle == "Femme")
                {
                    animDict = isPedMale ? "move_m@femme@" : "move_f@femme@";
                }
                else if (walkingStyle == "Gangster")
                {
                    animDict = isPedMale ? "move_m@gangster@a" : "move_f@gangster@ng";
                }
                else if (walkingStyle == "Posh")
                {
                    animDict = isPedMale ? "move_m@posh@" : "move_f@posh@";
                }
                else if (walkingStyle == "Sexy")
                {
                    animDict = isPedMale ? null : "move_f@sexy@a";
                }
                else if (walkingStyle == "Business")
                {
                    animDict = isPedMale ? null : "move_f@business@a";
                }
                else if (walkingStyle == "Drunk")
                {
                    animDict = isPedMale ? "move_m@drunk@a" : "move_f@drunk@a";
                }
                else if (walkingStyle == "Hipster")
                {
                    animDict = isPedMale ? "move_m@hipster@a" : null;
                }
                if (animDict != null)
                {
                    if (!HasAnimDictLoaded(animDict))
                    {
                        RequestAnimDict(animDict);
                        while (!HasAnimDictLoaded(animDict))
                        {
                            await Delay(0);
                        }
                    }
                    SetPedAlternateMovementAnim(Game.PlayerPed.Handle, 0, animDict, "idle", 1f, true);
                    SetPedAlternateMovementAnim(Game.PlayerPed.Handle, 1, animDict, "walk", 1f, true);
                    SetPedAlternateMovementAnim(Game.PlayerPed.Handle, 2, animDict, "run", 1f, true);
                }
                else if (walkingStyle != "Normal")
                {
                    if (isPedMale)
                    {
                        Notify.Error(CommonErrors.WalkingStyleNotForMale);
                    }
                    else
                    {
                        Notify.Error(CommonErrors.WalkingStyleNotForFemale);
                    }
                }
            }
            else
            {
                Notify.Error("This feature only supports the multiplayer freemode male/female ped models.");
            }
        }
        #endregion

        #region Disable Movement Controls
        /// <summary>
        /// Disables all movement and camera related controls this frame.
        /// </summary>
        /// <param name="disableMovement"></param>
        /// <param name="disableCameraMovement"></param>
        public static void DisableMovementControlsThisFrame(bool disableMovement, bool disableCameraMovement)
        {
            if (disableMovement)
            {
                Game.DisableControlThisFrame(0, Control.MoveDown);
                Game.DisableControlThisFrame(0, Control.MoveDownOnly);
                Game.DisableControlThisFrame(0, Control.MoveLeft);
                Game.DisableControlThisFrame(0, Control.MoveLeftOnly);
                Game.DisableControlThisFrame(0, Control.MoveLeftRight);
                Game.DisableControlThisFrame(0, Control.MoveRight);
                Game.DisableControlThisFrame(0, Control.MoveRightOnly);
                Game.DisableControlThisFrame(0, Control.MoveUp);
                Game.DisableControlThisFrame(0, Control.MoveUpDown);
                Game.DisableControlThisFrame(0, Control.MoveUpOnly);
                Game.DisableControlThisFrame(0, Control.VehicleFlyMouseControlOverride);
                Game.DisableControlThisFrame(0, Control.VehicleMouseControlOverride);
                Game.DisableControlThisFrame(0, Control.VehicleMoveDown);
                Game.DisableControlThisFrame(0, Control.VehicleMoveDownOnly);
                Game.DisableControlThisFrame(0, Control.VehicleMoveLeft);
                Game.DisableControlThisFrame(0, Control.VehicleMoveLeftRight);
                Game.DisableControlThisFrame(0, Control.VehicleMoveRight);
                Game.DisableControlThisFrame(0, Control.VehicleMoveRightOnly);
                Game.DisableControlThisFrame(0, Control.VehicleMoveUp);
                Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                Game.DisableControlThisFrame(0, Control.VehicleSubMouseControlOverride);
                Game.DisableControlThisFrame(0, Control.Duck);
                Game.DisableControlThisFrame(0, Control.SelectWeapon);
            }
            if (disableCameraMovement)
            {
                Game.DisableControlThisFrame(0, Control.LookBehind);
                Game.DisableControlThisFrame(0, Control.LookDown);
                Game.DisableControlThisFrame(0, Control.LookDownOnly);
                Game.DisableControlThisFrame(0, Control.LookLeft);
                Game.DisableControlThisFrame(0, Control.LookLeftOnly);
                Game.DisableControlThisFrame(0, Control.LookLeftRight);
                Game.DisableControlThisFrame(0, Control.LookRight);
                Game.DisableControlThisFrame(0, Control.LookRightOnly);
                Game.DisableControlThisFrame(0, Control.LookUp);
                Game.DisableControlThisFrame(0, Control.LookUpDown);
                Game.DisableControlThisFrame(0, Control.LookUpOnly);
                Game.DisableControlThisFrame(0, Control.ScaledLookDownOnly);
                Game.DisableControlThisFrame(0, Control.ScaledLookLeftOnly);
                Game.DisableControlThisFrame(0, Control.ScaledLookLeftRight);
                Game.DisableControlThisFrame(0, Control.ScaledLookUpDown);
                Game.DisableControlThisFrame(0, Control.ScaledLookUpOnly);
                Game.DisableControlThisFrame(0, Control.VehicleDriveLook);
                Game.DisableControlThisFrame(0, Control.VehicleDriveLook2);
                Game.DisableControlThisFrame(0, Control.VehicleLookBehind);
                Game.DisableControlThisFrame(0, Control.VehicleLookLeft);
                Game.DisableControlThisFrame(0, Control.VehicleLookRight);
                Game.DisableControlThisFrame(0, Control.NextCamera);
                Game.DisableControlThisFrame(0, Control.VehicleFlyAttackCamera);
                Game.DisableControlThisFrame(0, Control.VehicleCinCam);
            }
        }
        #endregion

        #region Set Correct Blip
        /// <summary>
        /// Sets the correct blip sprite for the specific ped and blip.
        /// This is the (old) backup method for setting the sprite if the decorators version doesn't work.
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="blip"></param>
        public static void SetCorrectBlipSprite(int ped, int blip)
        {
            if (IsPedInAnyVehicle(ped, false))
            {
                int vehicle = GetVehiclePedIsIn(ped, false);
                int blipSprite = BlipInfo.GetBlipSpriteForVehicle(vehicle);
                if (GetBlipSprite(blip) != blipSprite)
                {
                    SetBlipSprite(blip, blipSprite);
                }

            }
            else
            {
                SetBlipSprite(blip, 1);
            }
        }
        #endregion

        #region Get safe player name
        /// <summary>
        /// Returns a properly formatted and escaped player name for notifications.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSafePlayerName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }
            return name.Replace("^", @"\^").Replace("~", @"\~").Replace("<", "«").Replace(">", "»");
        }
        #endregion

        #region Draw model dimensions math util functions

        /*
            These util functions are taken from Deltanic's mapeditor resource for FiveM.
            https://gitlab.com/shockwave-fivem/mapeditor/tree/master
            Thank you Deltanic for allowing me to use these functions here.
        */

        /// <summary>
        /// Draws the bounding box for the entity with the provided rgba color.
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void DrawEntityBoundingBox(Entity ent, int r, int g, int b, int a)
        {
            var box = GetEntityBoundingBox(ent.Handle);
            DrawBoundingBox(box, r, g, b, a);
        }

        /// <summary>
        /// Gets the bounding box of the entity model in world coordinates, used by <see cref="DrawEntityBoundingBox(Entity, int, int, int, int)"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static Vector3[] GetEntityBoundingBox(int entity)
        {
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;

            GetModelDimensions((uint)GetEntityModel(entity), ref min, ref max);
            //const float pad = 0f;
            const float pad = 0.001f;
            var retval = new Vector3[8]
            {
                // Bottom
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, min.Y - pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, min.Y - pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, max.Y + pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, max.Y + pad, min.Z - pad),

                // Top
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, min.Y - pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, min.Y - pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, max.Y + pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, max.Y + pad, max.Z + pad)
            };
            return retval;
        }

        /// <summary>
        /// Draws the edge poly faces and the edge lines for the specific box coordinates using the specified rgba color.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        private static void DrawBoundingBox(Vector3[] box, int r, int g, int b, int a)
        {
            var polyMatrix = GetBoundingBoxPolyMatrix(box);
            var edgeMatrix = GetBoundingBoxEdgeMatrix(box);

            DrawPolyMatrix(polyMatrix, r, g, b, a);
            DrawEdgeMatrix(edgeMatrix, 255, 255, 255, 255);
        }

        /// <summary>
        /// Gets the coordinates for all poly box faces.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        private static Vector3[][] GetBoundingBoxPolyMatrix(Vector3[] box)
        {
            return new Vector3[12][]
            {
                new Vector3[3] { box[2], box[1], box[0] },
                new Vector3[3] { box[3], box[2], box[0] },

                new Vector3[3] { box[4], box[5], box[6] },
                new Vector3[3] { box[4], box[6], box[7] },

                new Vector3[3] { box[2], box[3], box[6] },
                new Vector3[3] { box[7], box[6], box[3] },

                new Vector3[3] { box[0], box[1], box[4] },
                new Vector3[3] { box[5], box[4], box[1] },

                new Vector3[3] { box[1], box[2], box[5] },
                new Vector3[3] { box[2], box[6], box[5] },

                new Vector3[3] { box[4], box[7], box[3] },
                new Vector3[3] { box[4], box[3], box[0] }
            };
        }

        /// <summary>
        /// Gets the coordinates for all edge coordinates.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        private static Vector3[][] GetBoundingBoxEdgeMatrix(Vector3[] box)
        {
            return new Vector3[12][]
            {
                new Vector3[2] { box[0], box[1] },
                new Vector3[2] { box[1], box[2] },
                new Vector3[2] { box[2], box[3] },
                new Vector3[2] { box[3], box[0] },

                new Vector3[2] { box[4], box[5] },
                new Vector3[2] { box[5], box[6] },
                new Vector3[2] { box[6], box[7] },
                new Vector3[2] { box[7], box[4] },

                new Vector3[2] { box[0], box[4] },
                new Vector3[2] { box[1], box[5] },
                new Vector3[2] { box[2], box[6] },
                new Vector3[2] { box[3], box[7] }
            };
        }

        /// <summary>
        /// Draws the poly matrix faces.
        /// </summary>
        /// <param name="polyCollection"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        private static void DrawPolyMatrix(Vector3[][] polyCollection, int r, int g, int b, int a)
        {
            foreach (var poly in polyCollection)
            {
                float x1 = poly[0].X;
                float y1 = poly[0].Y;
                float z1 = poly[0].Z;

                float x2 = poly[1].X;
                float y2 = poly[1].Y;
                float z2 = poly[1].Z;

                float x3 = poly[2].X;
                float y3 = poly[2].Y;
                float z3 = poly[2].Z;
                DrawPoly(x1, y1, z1, x2, y2, z2, x3, y3, z3, r, g, b, a);
            }
        }

        /// <summary>
        /// Draws the edge lines for the model dimensions.
        /// </summary>
        /// <param name="linesCollection"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        private static void DrawEdgeMatrix(Vector3[][] linesCollection, int r, int g, int b, int a)
        {
            foreach (var line in linesCollection)
            {
                float x1 = line[0].X;
                float y1 = line[0].Y;
                float z1 = line[0].Z;

                float x2 = line[1].X;
                float y2 = line[1].Y;
                float z2 = line[1].Z;

                DrawLine(x1, y1, z1, x2, y2, z2, r, g, b, a);
            }
        }
        #endregion

        #region Map (math util) function
        /// <summary>
        /// Maps the <paramref name="value"/> (which is a value between <paramref name="min_in"/> and <paramref name="max_in"/>) to a new value in the range of <paramref name="min_out"/> and <paramref name="max_out"/>.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="min_in">The minimum range value of the value.</param>
        /// <param name="max_in">The max range value of the value.</param>
        /// <param name="min_out">The min output range value.</param>
        /// <param name="max_out">The max output range value.</param>
        /// <returns></returns>
        public static float Map(float value, float min_in, float max_in, float min_out, float max_out)
        {
            return (value - min_in) * (max_out - min_out) / (max_in - min_in) + min_out;
        }

        /// <summary>
        /// Maps the <paramref name="value"/> (which is a value between <paramref name="min_in"/> and <paramref name="max_in"/>) to a new value in the range of <paramref name="min_out"/> and <paramref name="max_out"/>.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="min_in">The minimum range value of the value.</param>
        /// <param name="max_in">The max range value of the value.</param>
        /// <param name="min_out">The min output range value.</param>
        /// <param name="max_out">The max output range value.</param>
        /// <returns></returns>
        public static double Map(double value, double min_in, double max_in, double min_out, double max_out)
        {
            return (value - min_in) * (max_out - min_out) / (max_in - min_in) + min_out;
        }
        #endregion

        #region Private message notification
        public static void PrivateMessage(string source, string message) => PrivateMessage(source, message, false);
        public static async void PrivateMessage(string source, string message, bool sent)
        {
            MainMenu.PlayersList.RequestPlayerList();
            await MainMenu.PlayersList.WaitRequested();

            string name = MainMenu.PlayersList.ToList()
                .Find(plr => plr.ServerId.ToString() == source)?.Name ?? "**Invalid**";

            if (MainMenu.MiscSettingsMenu == null || MainMenu.MiscSettingsMenu.MiscDisablePrivateMessages)
            {
                if (!(sent && source == Game.Player.ServerId.ToString()))
                {
                    TriggerServerEvent("vMenu:PmsDisabled", source);
                }
                return;
            }

            Player sourcePlayer = new Player(GetPlayerFromServerId(int.Parse(source)));
            if (sourcePlayer != null)
            {
                int headshotHandle = RegisterPedheadshot(sourcePlayer.Character.Handle);
                int timer = GetGameTimer();
                bool tookTooLong = false;
                while (!IsPedheadshotReady(headshotHandle) || !IsPedheadshotValid(headshotHandle))
                {
                    await Delay(0);
                    if (GetGameTimer() - timer > 2000)
                    {
                        // took too long.
                        tookTooLong = true;
                        break;
                    }
                }
                if (!tookTooLong)
                {
                    string headshotTxd = GetPedheadshotTxdString(headshotHandle);
                    if (sent)
                    {
                        Notify.CustomImage(headshotTxd, headshotTxd, message, $"<C>{GetSafePlayerName(name)}</C>", "Message Sent", true, 1);
                    }
                    else
                    {
                        Notify.CustomImage(headshotTxd, headshotTxd, message, $"<C>{GetSafePlayerName(name)}</C>", "Message Received", true, 1);
                    }
                }
                else
                {
                    if (sent)
                    {
                        Notify.Custom($"PM From: <C>{GetSafePlayerName(name)}</C>. Message: {message}");
                    }
                    else
                    {
                        Notify.Custom($"PM To: <C>{GetSafePlayerName(name)}</C>. Message: {message}");
                    }
                }
                UnregisterPedheadshot(headshotHandle);
            }
        }
        #endregion

        #region Keyfob personal vehicle func
        public static async void PressKeyFob(Vehicle veh)
        {
            Player player = Game.Player;
            if (player != null && !player.IsDead && !player.Character.IsInVehicle())
            {
                uint KeyFobHashKey = (uint)GetHashKey("p_car_keys_01");
                RequestModel(KeyFobHashKey);
                while (!HasModelLoaded(KeyFobHashKey))
                {
                    await Delay(0);
                }

                int KeyFobObject = CreateObject((int)KeyFobHashKey, 0, 0, 0, true, true, true);
                AttachEntityToEntity(KeyFobObject, player.Character.Handle, GetPedBoneIndex(player.Character.Handle, 57005), 0.09f, 0.03f, -0.02f, -76f, 13f, 28f, false, true, true, true, 0, true);
                SetModelAsNoLongerNeeded(KeyFobHashKey); // cleanup model from memory

                ClearPedTasks(player.Character.Handle);
                SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("WEAPON_UNARMED"), true);
                //if (player.Character.Weapons.Current.Hash != WeaponHash.Unarmed)
                //{
                //    player.Character.Weapons.Give(WeaponHash.Unarmed, 1, true, true);
                //}

                // if (!HasEntityClearLosToEntityInFront(player.Character.Handle, veh.Handle))
                {
                    /*
                    TODO: Work out how to get proper heading between entities.
                    */


                    //SetPedDesiredHeading(player.Character.Handle, )
                    //float heading = GetHeadingFromVector_2d(player.Character.Position.X - veh.Position.Y, player.Character.Position.Y - veh.Position.X);
                    //double x = Math.Cos(player.Character.Position.X) * Math.Sin(player.Character.Position.Y - (double)veh.Position.Y);
                    //double y = Math.Cos(player.Character.Position.X) * Math.Sin(veh.Position.X) - Math.Sin(player.Character.Position.X) * Math.Cos(veh.Position.X) * Math.Cos(player.Character.Position.Y - (double)veh.Position.Y);
                    //float heading = (float)Math.Atan2(x, y);
                    //Debug.WriteLine(heading.ToString());
                    //SetPedDesiredHeading(player.Character.Handle, heading);

                    ClearPedTasks(Game.PlayerPed.Handle);
                    TaskTurnPedToFaceEntity(player.Character.Handle, veh.Handle, 500);
                }

                string animDict = "anim@mp_player_intmenu@key_fob@";
                RequestAnimDict(animDict);
                while (!HasAnimDictLoaded(animDict))
                {
                    await Delay(0);
                }
                player.Character.Task.PlayAnimation(animDict, "fob_click", 3f, 1000, AnimationFlags.UpperBodyOnly);
                PlaySoundFromEntity(-1, "Remote_Control_Fob", player.Character.Handle, "PI_Menu_Sounds", true, 0);


                await Delay(1250);
                DetachEntity(KeyFobObject, false, false);
                DeleteObject(ref KeyFobObject);
                RemoveAnimDict(animDict); // cleanup anim dict from memory
            }

            await Delay(0);
        }
        #endregion

        #region Encoded float to normal float
        ///// <summary>
        ///// Converts an IEEE 754 (int encoded) floating-point to a real float value.
        ///// </summary>
        ///// <param name="input"></param>
        ///// <returns></returns>
        //public static float IntToFloat(int input)
        //{
        //    // This function is based on the 'hex2float' snippet found here for Lua:
        //    // https://stackoverflow.com/a/19996852

        //    //string d = input.ToString("X8");

        //    var s1 = (char)int.Parse(d.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        //    var s2 = (char)int.Parse(d.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        //    var s3 = (char)int.Parse(d.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //    var s4 = (char)int.Parse(d.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

        //    var b1 = BitConverter.GetBytes(s1)[0];
        //    var b2 = BitConverter.GetBytes(s2)[0];
        //    var b3 = BitConverter.GetBytes(s3)[0];
        //    var b4 = BitConverter.GetBytes(s4)[0];

        //    int sign = b1 > 0x7F ? -1 : 1;
        //    int expo = ((b1 % 0x80) * 0x2) + (b2 / 0x80);
        //    float mant = ((b2 % 0x80) * 0x100 + b3) * 0x100 + b4;

        //    float n;
        //    if (mant == 0 && expo == 0)
        //    {
        //        n = sign * 0.0f;
        //    }
        //    else if (expo == 0xFF)
        //    {
        //        if (mant == 0)
        //        {
        //            n = (float)((double)sign * Math.E);
        //        }
        //        else
        //        {
        //            n = 0.0f;
        //        }
        //    }
        //    else
        //    {
        //        double left = 1.0 + mant / 0x800000;
        //        int right = expo - 0x7F;
        //        float other = (float)left * ((float)right * (float)right);
        //        n = (float)sign * (float)other;
        //    }
        //    return n;
        //}
        #endregion

        #region save player location to the server locations.json file
        /// <summary>
        /// Saves the player's location as a new teleport location in the teleport options menu.
        /// </summary>
        public static async void SavePlayerLocationToLocationsFile()
        {
            var pos = Game.PlayerPed.Position;
            var heading = Game.PlayerPed.Heading;
            string locationName = await GetUserInput("Enter location save name", 30);
            if (string.IsNullOrEmpty(locationName))
            {
                Notify.Error(CommonErrors.InvalidInput);
                return;
            }
            if (vMenuShared.ConfigManager.GetTeleportLocationsData().Any(loc => loc.name == locationName))
            {
                Notify.Error("This location name is already used, please use a different name.");
                return;
            }
            TriggerServerEvent("vMenu:SaveTeleportLocation", JsonConvert.SerializeObject(new vMenuShared.ConfigManager.TeleportLocation(locationName, pos, heading)));
            Notify.Success("The location was successfully saved.");
        }
        #endregion
    }
}
