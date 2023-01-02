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

        #region menu position
        public static bool RightAlignMenus() => UserDefaults.MiscRightAlignMenu;
        #endregion



        

        #region GetUserInput
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetUserInput() => await GetUserInput(null, null, 30);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(int maxInputLength) => await GetUserInput(null, null, maxInputLength);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle) => await GetUserInput(windowTitle, null, 30);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle, int maxInputLength) => await GetUserInput(windowTitle, null, maxInputLength);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="defaultText"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle, string defaultText) => await GetUserInput(windowTitle, defaultText, 30);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="defaultText"></param>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
        {
            // Create the window title string.
            var spacer = "\t";
            AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength} Characters)");

            // Display the input box.
            DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
            await Delay(0);
            // Wait for a result.
            while (true)
            {
                int keyboardStatus = UpdateOnscreenKeyboard();

                switch (keyboardStatus)
                {
                    case 3: // not displaying input field anymore somehow
                    case 2: // cancelled
                        return null;
                    case 1: // finished editing
                        return GetOnscreenKeyboardResult();
                    default:
                        await Delay(0);
                        break;
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
                    SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn");
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
