﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class EventManager : BaseScript
    {
        public static bool IsSnowEnabled => GetSettingsBool(Setting.vmenu_enable_snow);
        public static int GetServerMinutes => MathUtil.Clamp(GetSettingsInt(Setting.vmenu_current_minute), 0, 59);
        public static int GetServerHours => MathUtil.Clamp(GetSettingsInt(Setting.vmenu_current_hour), 0, 23);
        public static int GetServerMinuteDuration => GetSettingsInt(Setting.vmenu_ingame_minute_duration);
        public static bool IsServerTimeFrozen => GetSettingsBool(Setting.vmenu_freeze_time);
        public static bool IsServerTimeSyncedWithMachineTime => GetSettingsBool(Setting.vmenu_sync_to_machine_time);
        public static string GetServerWeather => GetSettingsString(Setting.vmenu_current_weather, "CLEAR");
        public static bool DynamicWeatherEnabled => GetSettingsBool(Setting.vmenu_enable_dynamic_weather);
        public static bool IsBlackoutEnabled => GetSettingsBool(Setting.vmenu_blackout_enabled);
        public static int WeatherChangeTime => MathUtil.Clamp(GetSettingsInt(Setting.vmenu_weather_change_duration), 0, 45);

        /// <summary>
        /// Constructor.
        /// </summary>
        public EventManager()
        {
            // Add event handlers.
            EventHandlers.Add("vMenu:SetAddons", new Action(SetAddons));
            EventHandlers.Add("vMenu:SetPermissions", new Action<string>(MainMenu.SetPermissions));
            EventHandlers.Add("vMenu:GoToPlayer", new Action<string>(SummonPlayer));
            EventHandlers.Add("vMenu:KillMe", new Action<string>(KillMe));
            EventHandlers.Add("vMenu:Notify", new Action<string>(NotifyPlayer));
            EventHandlers.Add("vMenu:SetClouds", new Action<float, string>(SetClouds));
            EventHandlers.Add("vMenu:GoodBye", new Action(GoodBye));
            EventHandlers.Add("vMenu:SetBanList", new Action<string>(UpdateBanList));
            EventHandlers.Add("vMenu:ClearArea", new Action<float, float, float>(ClearAreaNearPos));
            EventHandlers.Add("vMenu:updatePedDecors", new Action(UpdatePedDecors));
            EventHandlers.Add("playerSpawned", new Action(SetAppearanceOnFirstSpawn));
            EventHandlers.Add("vMenu:GetOutOfCar", new Action<int, int>(GetOutOfCar));
            EventHandlers.Add("vMenu:PrivateMessage", new Action<string, string>(PrivateMessage));
            EventHandlers.Add("vMenu:UpdateTeleportLocations", new Action<string>(UpdateTeleportLocations));

            
            RegisterNuiCallbackType("disableImportExportNUI");
            RegisterNuiCallbackType("importData");
        }

        [EventHandler("__cfx_nui:importData")]
        internal void ImportData(IDictionary<string, object> data, CallbackDelegate cb)
        {
            SetNuiFocus(false, false);
            Notify.Info("Debug info: This feature is not yet available, check back later.");
            cb(JsonConvert.SerializeObject(new { ok = true }));
        }

        [EventHandler("__cfx_nui:disableImportExportNUI")]
        internal void DisableImportExportNUI(IDictionary<string, object> data, CallbackDelegate cb)
        {
            SetNuiFocus(false, false);
            Notify.Info("Debug info: Closing import/export NUI window.");
            cb(JsonConvert.SerializeObject(new { ok = true }));
        }

        

        /// <summary>
        /// Sets the addon models from the addons.json file.
        /// </summary>
        private void SetAddons()
        {
            MainMenu.ConfigOptionsSetupComplete = true;
        }

        

        /// <summary>
        /// Used for cheaters.
        /// </summary>
        private void GoodBye()
        {
            ForceSocialClubUpdate();
        }

        /// <summary>
        /// Loads/unloads the snow fx particles if needed.
        /// </summary>
        private async Task UpdateWeatherParticles()
        {
            ForceSnowPass(IsSnowEnabled);
            SetForceVehicleTrails(IsSnowEnabled);
            SetForcePedFootstepsTracks(IsSnowEnabled);
            if (IsSnowEnabled)
            {
                if (!HasNamedPtfxAssetLoaded("core_snow"))
                {
                    RequestNamedPtfxAsset("core_snow");
                    while (!HasNamedPtfxAssetLoaded("core_snow"))
                    {
                        await Delay(0);
                    }
                }
                UseParticleFxAssetNextCall("core_snow");
            }
            else
            {
                RemoveNamedPtfxAsset("core_snow");
            }
        }

        /// <summary>
        /// OnTick loop to keep the weather synced.
        /// </summary>
        /// <returns></returns>
        private async Task WeatherSync()
        {
            await UpdateWeatherParticles();
            SetArtificialLightsState(IsBlackoutEnabled);
            if (GetNextWeatherType() != GetHashKey(GetServerWeather))
            {
                SetWeatherTypeOvertimePersist(GetServerWeather, (float)WeatherChangeTime);
                await Delay(WeatherChangeTime * 1000 + 2000);

                TriggerEvent("vMenu:WeatherChangeComplete", GetServerWeather);
            }
            await Delay(1000);
        }

        /// <summary>
        /// This function will take care of time sync. It'll be called once, and never stop.
        /// </summary>
        /// <returns></returns>
        private async Task TimeSync()
        {
            NetworkOverrideClockTime(GetServerHours, GetServerMinutes, 0);
            if (IsServerTimeFrozen || IsServerTimeSyncedWithMachineTime)
            {
                await Delay(5);
            }
            else
            {
                await Delay(MathUtil.Clamp(GetServerMinuteDuration, 100, 2000));
            }
        }

        /// <summary>
        /// Set the cloud hat type.
        /// </summary>
        /// <param name="opacity"></param>
        /// <param name="cloudsType"></param>
        private void SetClouds(float opacity, string cloudsType)
        {
            if (opacity == 0f && cloudsType == "removed")
            {
                ClearCloudHat();
            }
            else
            {
                SetCloudHatOpacity(opacity);
                SetCloudHatTransition(cloudsType, 4f);
            }
        }

        /// <summary>
        /// Used by events triggered from the server to notify a user.
        /// </summary>
        /// <param name="message"></param>
        private void NotifyPlayer(string message)
        {
            Notify.Custom(message, true, true);
        }

        /// <summary>
        /// Kill this player, poor thing, someone wants you dead... R.I.P.
        /// </summary>
        private void KillMe(string sourceName)
        {
            Notify.Alert($"You have been killed by <C>{GetSafePlayerName(sourceName)}</C>~s~ using the ~r~Kill Player~s~ option in vMenu.");
            SetEntityHealth(Game.PlayerPed.Handle, 0);
        }

        

        /// <summary>
        /// Clear the area around the provided x, y, z coordinates. Clears everything like (destroyed) objects, peds, (ai) vehicles, etc.
        /// Also restores broken streetlights, etc.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void ClearAreaNearPos(float x, float y, float z)
        {
            ClearAreaOfEverything(x, y, z, 100f, false, false, false, false);
        }

        

        /// <summary>
        /// Updates ped decorators for the clothing animation when players have joined.
        /// </summary>
        private async void UpdatePedDecors()
        {
            await Delay(1000);
            int backup = PlayerAppearance.ClothingAnimationType;
            PlayerAppearance.ClothingAnimationType = -1;
            await Delay(100);
            PlayerAppearance.ClothingAnimationType = backup;
        }

        
    }
}
