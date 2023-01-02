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
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class MainMenu : BaseScript
    {
        #region Variables

        public static bool PermissionsSetupComplete => ArePermissionsSetup;
        public static bool ConfigOptionsSetupComplete = false;

        public static Control MenuToggleKey { get { return MenuController.MenuToggleKey; } private set { MenuController.MenuToggleKey = value; } } // M by default (InteractionMenu)
        public static int NoClipKey { get; private set; } = 289; // F2 by default (ReplayStartStopRecordingSecondary)
        public static Menu Menu { get; private set; }
        public static Menu PlayerSubmenu { get; private set; }
        public static Menu VehicleSubmenu { get; private set; }
        public static Menu WorldSubmenu { get; private set; }

        public static PlayerOptions PlayerOptionsMenu { get; private set; }
        public static OnlinePlayers OnlinePlayersMenu { get; private set; }
        public static BannedPlayers BannedPlayersMenu { get; private set; }
        public static SavedVehicles SavedVehiclesMenu { get; private set; }
        public static PersonalVehicle PersonalVehicleMenu { get; private set; }
        public static VehicleOptions VehicleOptionsMenu { get; private set; }
        public static VehicleSpawner VehicleSpawnerMenu { get; private set; }
        public static PlayerAppearance PlayerAppearanceMenu { get; private set; }
        public static MpPedCustomization MpPedCustomizationMenu { get; private set; }
        public static TimeOptions TimeOptionsMenu { get; private set; }
        public static WeatherOptions WeatherOptionsMenu { get; private set; }
        public static WeaponOptions WeaponOptionsMenu { get; private set; }
        public static WeaponLoadouts WeaponLoadoutsMenu { get; private set; }
        public static Recording RecordingMenu { get; private set; }
        public static MiscSettings MiscSettingsMenu { get; private set; }
        public static VoiceChat VoiceChatSettingsMenu { get; private set; }
        public static About AboutMenu { get; private set; }
        public static bool NoClipEnabled { get { return NoClip.IsNoclipActive(); } set { NoClip.SetNoclipActive(value); } }
        public static IPlayerList PlayersList;

        public static bool DebugMode = GetResourceMetadata(GetCurrentResourceName(), "client_debug_mode", 0) == "true" ? true : false;
        public static bool EnableExperimentalFeatures = (GetResourceMetadata(GetCurrentResourceName(), "experimental_features_enabled", 0) ?? "0") == "1";
        public static string Version { get { return GetResourceMetadata(GetCurrentResourceName(), "version", 0); } }

        public static bool DontOpenMenus { get { return MenuController.DontOpenAnyMenu; } set { MenuController.DontOpenAnyMenu = value; } }
        public static bool DisableControls { get { return MenuController.DisableMenuButtons; } set { MenuController.DisableMenuButtons = value; } }

        private const int currentCleanupVersion = 2;
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainMenu()
        {
            

            if (GetCurrentResourceName() != "zgMenu")
            {
                MenuController.MainMenu = null;
                MenuController.DontOpenAnyMenu = true;
                MenuController.DisableMenuButtons = true;
                throw new Exception("\n[vMenu] INSTALLATION ERROR!\nThe name of the resource is not valid. Please change the folder name from '" + GetCurrentResourceName() + "' to 'vMenu' (case sensitive)!\n");
            }
            else
            {
                Tick += OnTick;
            }

            // Clear all previous pause menu info/brief messages on resource start.
            //ClearBrief();

            // Request the permissions data from the server.
            //TriggerServerEvent("vMenu:RequestPermissions");

            // Request server state from the server.
            //TriggerServerEvent("vMenu:RequestServerState");
        }

        #region Infinity bits
        [EventHandler("vMenu:SetServerState")]
        public void SetServerState(IDictionary<string, object> data)
        {
            if (data.TryGetValue("IsInfinity", out var isInfinity))
            {
                if (isInfinity is bool isInfinityBool)
                {
                    if (isInfinityBool)
                    {
                        PlayersList = new InfinityPlayerList(Players);
                    }
                }
            }
        }

        [EventHandler("vMenu:ReceivePlayerList")]
        public void ReceivedPlayerList(IList<object> players)
        {
            PlayersList?.ReceivedPlayerList(players);
        }

        public static async Task<Vector3> RequestPlayerCoordinates(int serverId)
        {
            Vector3 coords = Vector3.Zero;
            bool completed = false;

            // TODO: replace with client<->server RPC once implemented in CitizenFX!
            Func<Vector3, bool> CallbackFunction = (data) =>
            {
                coords = data;
                completed = true;
                return true;
            };

            TriggerServerEvent("vMenu:GetPlayerCoords", serverId, CallbackFunction);

            while (!completed)
            {
                await Delay(0);
            }

            return coords;
        }
        #endregion

        #region Set Permissions function
        /// <summary>
        /// Set the permissions for this client.
        /// </summary>
        /// <param name="dict"></param>
        public static async void SetPermissions(string permissionsList)
        {
            //vMenuShared.PermissionsManager.SetPermissions(permissionsList);

            ArePermissionsSetup = true;
            while (!ConfigOptionsSetupComplete)
            {
                await Delay(100);
            }
            PostPermissionsSetup();
        }
        #endregion

        /// <summary>
        /// This setups things as soon as the permissions are loaded.
        /// It triggers the menu creations, setting of initial flags like PVP, player stats,
        /// and triggers the creation of Tick functions from the FunctionsController class.
        /// </summary>
        private static void PostPermissionsSetup()
        {
            

            /*bool canUseMenu()
            {
                if (GetSettingsBool(Setting.vmenu_menu_staff_only) == false) return true;
                else if (IsAllowed(Permission.Staff)) return true;
                return false;
            }

            if (!canUseMenu())
            {
                MenuController.MainMenu = null;
                MenuController.DisableMenuButtons = true;
                MenuController.DontOpenAnyMenu = true;
                MenuController.MenuToggleKey = (Control)(-1); // disables the menu toggle key
                return;
            }*/

            if (GetSettingsInt(Setting.vmenu_menu_toggle_key) != -1)
            {
                MenuToggleKey = (Control)GetSettingsInt(Setting.vmenu_menu_toggle_key);
                //MenuToggleKey = GetSettingsInt(Setting.vmenu_menu_toggle_key);
            }
            

            // Create the main menu.
            Menu = new Menu(Game.Player.Name, "Main Menu");
            //PlayerSubmenu = new Menu(Game.Player.Name, "Player Related Options");
            //VehicleSubmenu = new Menu(Game.Player.Name, "Vehicle Related Options");
           // WorldSubmenu = new Menu(Game.Player.Name, "World Options");

            // Add the main menu to the menu pool.
            MenuController.AddMenu(Menu);
            MenuController.MainMenu = Menu;

            //MenuController.AddSubmenu(Menu, PlayerSubmenu);
            //MenuController.AddSubmenu(Menu, VehicleSubmenu);
            //MenuController.AddSubmenu(Menu, WorldSubmenu);

            // Create all (sub)menus.
            CreateSubmenus();

            
        }

        /// <summary>
        /// Main OnTick task runs every game tick and handles all the menu stuff.
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            // If the setup (permissions) is done, and it's not the first tick, then do this:
            if (ConfigOptionsSetupComplete)
            {
                #region Handle Opening/Closing of the menu.
                var tmpMenu = GetOpenMenu();
                if (MpPedCustomizationMenu != null)
                {
                    bool IsOpen()
                    {
                        return
                            MpPedCustomizationMenu.appearanceMenu.Visible ||
                            MpPedCustomizationMenu.faceShapeMenu.Visible ||
                            MpPedCustomizationMenu.createCharacterMenu.Visible ||
                            MpPedCustomizationMenu.inheritanceMenu.Visible ||
                            MpPedCustomizationMenu.propsMenu.Visible ||
                            MpPedCustomizationMenu.clothesMenu.Visible ||
                            MpPedCustomizationMenu.tattoosMenu.Visible;
                    }

                    if (IsOpen())
                    {
                        if (tmpMenu == MpPedCustomizationMenu.createCharacterMenu)
                        {
                            MpPedCustomization.DisableBackButton = true;
                        }
                        else
                        {
                            MpPedCustomization.DisableBackButton = false;
                        }
                        MpPedCustomization.DontCloseMenus = true;
                    }
                    else
                    {
                        MpPedCustomization.DisableBackButton = false;
                        MpPedCustomization.DontCloseMenus = false;
                    }
                }

                if (Game.IsDisabledControlJustReleased(0, Control.PhoneCancel) && MpPedCustomization.DisableBackButton)
                {
                    await Delay(0);
                    Notify.Alert("You must save your ped first before exiting, or click the ~r~Exit Without Saving~s~ button.");
                }



                // Menu toggle button.
                Game.DisableControlThisFrame(0, MenuToggleKey);
            }
        }

        #region Add Menu Function
        /// <summary>
        /// Add the menu to the menu pool and set it up correctly.
        /// Also add and bind the menu buttons.
        /// </summary>
        /// <param name="submenu"></param>
        /// <param name="menuButton"></param>
        private static void AddMenu(Menu parentMenu, Menu submenu, MenuItem menuButton)
        {
            parentMenu.AddMenuItem(menuButton);
            MenuController.AddSubmenu(parentMenu, submenu);
            MenuController.BindMenuItem(parentMenu, submenu, menuButton);
            submenu.RefreshIndex();
        }
        #endregion

        #region Create Submenus
        /// <summary>
        /// Creates all the submenus depending on the permissions of the user.
        /// </summary>
        private static void CreateSubmenus()
        {
            
                MpPedCustomizationMenu = new MpPedCustomization();
                Menu menu2 = MpPedCustomizationMenu.GetMenu();
                MenuItem button2 = new MenuItem("MP Ped Customization", "Create, edit, save and load multiplayer peds. ~r~Note, you can only save peds created in this submenu. vMenu can NOT detect peds created outside of this submenu. Simply due to GTA limitations.")
                {
                    Label = "→→→"
                };
                AddMenu(Menu, menu2, button2);
            
        }
    }
}
