using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;
using static vMenuClient.data.PedModels;

namespace vMenuClient
{
    /// <summary>
    /// This class manages all things that need to be done every tick based on
    /// checkboxes/things changing in any of the (sub) menus.
    /// </summary>
    class FunctionsController : BaseScript
    {
        private int LastVehicle = 0;
        private bool SwitchedVehicle = false;
        

        public FunctionsController() { }

        /// <summary>
        /// Setup the required tick functions
        /// </summary>
        

        /// Task related
        #region gc thread
        int gcTimer = GetGameTimer();
        /// <summary>
        /// Task for clearing unused memory periodically.
        /// </summary>
        /// <returns></returns>
        private async Task GcTick()
        {
            if (GetGameTimer() - gcTimer > 60000)
            {
                gcTimer = GetGameTimer();
                GC.Collect();
                Log($"[vMenu] GC at {GetGameTimer()} ({GetTimeAsString(GetGameTimer())}).");

            }
            await Delay(1000);
        }
        #endregion

        #region General Tasks
        /// <summary>
        /// All general tasks that run every 1 game ticks (and are not (sub)menu specific).
        /// </summary>
        /// <returns></returns>
        private async Task GeneralTasks()
        {
            // Check if the player has switched to a new vehicle.
            if (Game.PlayerPed.IsInVehicle()) // added this for improved performance.
            {
                var tmpVehicle = GetVehicle();
                if (tmpVehicle != null && tmpVehicle.Exists() && tmpVehicle.Handle != LastVehicle)
                {
                    // Set the last vehicle to the new vehicle entity.
                    LastVehicle = tmpVehicle.Handle;
                    SwitchedVehicle = true;
                }
            }
            // this can wait 1 ms
            await Delay(1);
        }
        #endregion

        #region Player Options Tasks
        /// <summary>
        /// Run all tasks for the Player Options menu.
        /// </summary>
        /// <returns></returns>
        private async Task PlayerOptions()
        {
            // perms
            bool godmodeAllowed = IsAllowed(Permission.POGod);
            bool noRagdollAllowed = IsAllowed(Permission.PONoRagdoll);

            if (MainMenu.MpPedCustomizationMenu != null && MainMenu.MpPedCustomizationMenu.appearanceMenu != null && MainMenu.MpPedCustomizationMenu.faceShapeMenu != null && MainMenu.MpPedCustomizationMenu.createCharacterMenu != null && MainMenu.MpPedCustomizationMenu.inheritanceMenu != null && MainMenu.MpPedCustomizationMenu.propsMenu != null && MainMenu.MpPedCustomizationMenu.clothesMenu != null && MainMenu.MpPedCustomizationMenu.tattoosMenu != null)
            {
                // Manage Player God Mode
                bool IsMpPedCreatorOpen()
                {
                    return
                        MainMenu.MpPedCustomizationMenu.appearanceMenu.Visible ||
                        MainMenu.MpPedCustomizationMenu.faceShapeMenu.Visible ||
                        MainMenu.MpPedCustomizationMenu.createCharacterMenu.Visible ||
                        MainMenu.MpPedCustomizationMenu.inheritanceMenu.Visible ||
                        MainMenu.MpPedCustomizationMenu.propsMenu.Visible ||
                        MainMenu.MpPedCustomizationMenu.clothesMenu.Visible ||
                        MainMenu.MpPedCustomizationMenu.tattoosMenu.Visible;
                }
                if (!IsMpPedCreatorOpen())
                {
                    //SetEntityInvincible(Game.PlayerPed.Handle, MainMenu.PlayerOptionsMenu.PlayerGodMode && godmodeAllowed);
                }
            }

            

            if (DriveToWpTaskActive && !Game.IsWaypointActive)
            {
                ClearPedTasks(Game.PlayerPed.Handle);
                Notify.Custom("Destination reached, the car will now stop driving!");
                DriveToWpTaskActive = false;
            }
            await Task.FromResult(0);
        }
        #endregion


        

        
        #region Player Appearance

        internal static bool reverseCamera = false;
        private static Camera camera;
        internal static float CameraFov { get; set; } = 45;
        internal static int CurrentCam { get; set; }
        internal static List<KeyValuePair<Vector3, Vector3>> CameraOffsets { get; } = new List<KeyValuePair<Vector3, Vector3>>()
        {
            // Full body
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 2.8f, 0.3f), new Vector3(0f, 0f, 0f)),

            // Head level
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 0.9f, 0.65f), new Vector3(0f, 0f, 0.6f)),

            // Upper Body
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 1.4f, 0.5f), new Vector3(0f, 0f, 0.3f)),

            // Lower Body
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 1.6f, -0.3f), new Vector3(0f, 0f, -0.45f)),

            // Shoes
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 0.98f, -0.7f), new Vector3(0f, 0f, -0.90f)),

            // Lower Arms
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 0.98f, 0.1f), new Vector3(0f, 0f, 0f)),

            // Full arms
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 1.3f, 0.35f), new Vector3(0f, 0f, 0.15f)),
        };

        private async Task UpdateCamera(Camera oldCamera, Vector3 pos, Vector3 pointAt)
        {
            var newCam = CreateCam("DEFAULT_SCRIPTED_CAMERA", true);
            var newCamera = new Camera(newCam)
            {
                Position = pos,
                FieldOfView = CameraFov
            };
            newCamera.PointAt(pointAt);
            oldCamera.InterpTo(newCamera, 1000, true, true);
            while (oldCamera.IsInterpolating || !newCamera.IsActive)
            {
                SetEntityCollision(Game.PlayerPed.Handle, false, false);
                //Game.PlayerPed.IsInvincible = true;
                Game.PlayerPed.IsPositionFrozen = true;
                await Delay(0);
            }
            await Delay(50);
            oldCamera.Delete();
            CurrentCam = newCam;
            camera = newCamera;
        }

        private bool IsMpCharEditorOpen()
        {
            if (MainMenu.MpPedCustomizationMenu != null)
            {
                return
                    MainMenu.MpPedCustomizationMenu.appearanceMenu.Visible ||
                    MainMenu.MpPedCustomizationMenu.faceShapeMenu.Visible ||
                    MainMenu.MpPedCustomizationMenu.createCharacterMenu.Visible ||
                    MainMenu.MpPedCustomizationMenu.inheritanceMenu.Visible ||
                    MainMenu.MpPedCustomizationMenu.propsMenu.Visible ||
                    MainMenu.MpPedCustomizationMenu.clothesMenu.Visible ||
                    MainMenu.MpPedCustomizationMenu.tattoosMenu.Visible;
            }
            return false;
        }

        /// <summary>
        /// Manages the camera for the mp character customization menu
        /// </summary>
        /// <returns></returns>
        private async Task ManageCamera()
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                if (MainMenu.MpPedCustomizationMenu.editPedBtn != null && MainMenu.MpPedCustomizationMenu.editPedBtn.Enabled)
                {
                    MainMenu.MpPedCustomizationMenu.editPedBtn.Enabled = false;
                    MainMenu.MpPedCustomizationMenu.editPedBtn.LeftIcon = MenuItem.Icon.LOCK;
                    MainMenu.MpPedCustomizationMenu.editPedBtn.Description += " ~r~You need to get out of your vehicle before you can use this.";
                }
                if (MainMenu.MpPedCustomizationMenu.createMaleBtn != null && MainMenu.MpPedCustomizationMenu.createMaleBtn.Enabled)
                {
                    MainMenu.MpPedCustomizationMenu.createMaleBtn.Enabled = false;
                    MainMenu.MpPedCustomizationMenu.createMaleBtn.LeftIcon = MenuItem.Icon.LOCK;
                    MainMenu.MpPedCustomizationMenu.createMaleBtn.Description += " ~r~You need to get out of your vehicle before you can use this.";
                }
                if (MainMenu.MpPedCustomizationMenu.createFemaleBtn != null && MainMenu.MpPedCustomizationMenu.createFemaleBtn.Enabled)
                {
                    MainMenu.MpPedCustomizationMenu.createFemaleBtn.Enabled = false;
                    MainMenu.MpPedCustomizationMenu.createFemaleBtn.LeftIcon = MenuItem.Icon.LOCK;
                    MainMenu.MpPedCustomizationMenu.createFemaleBtn.Description += " ~r~You need to get out of your vehicle before you can use this.";
                }
            }
            else
            {
                if (MainMenu.MpPedCustomizationMenu.editPedBtn != null && !MainMenu.MpPedCustomizationMenu.editPedBtn.Enabled)
                {
                    MainMenu.MpPedCustomizationMenu.editPedBtn.Enabled = true;
                    MainMenu.MpPedCustomizationMenu.editPedBtn.LeftIcon = MenuItem.Icon.NONE;
                    MainMenu.MpPedCustomizationMenu.editPedBtn.Description = MainMenu.MpPedCustomizationMenu.editPedBtn.Description.Replace(" ~r~You need to get out of your vehicle before you can use this.", "");
                }
                if (MainMenu.MpPedCustomizationMenu.createMaleBtn != null && !MainMenu.MpPedCustomizationMenu.createMaleBtn.Enabled)
                {
                    MainMenu.MpPedCustomizationMenu.createMaleBtn.Enabled = true;
                    MainMenu.MpPedCustomizationMenu.createMaleBtn.LeftIcon = MenuItem.Icon.NONE;
                    MainMenu.MpPedCustomizationMenu.createMaleBtn.Description = MainMenu.MpPedCustomizationMenu.createMaleBtn.Description.Replace(" ~r~You need to get out of your vehicle before you can use this.", "");
                }
                if (MainMenu.MpPedCustomizationMenu.createFemaleBtn != null && !MainMenu.MpPedCustomizationMenu.createFemaleBtn.Enabled)
                {
                    MainMenu.MpPedCustomizationMenu.createFemaleBtn.Enabled = true;
                    MainMenu.MpPedCustomizationMenu.createFemaleBtn.LeftIcon = MenuItem.Icon.NONE;
                    MainMenu.MpPedCustomizationMenu.createFemaleBtn.Description = MainMenu.MpPedCustomizationMenu.createFemaleBtn.Description.Replace(" ~r~You need to get out of your vehicle before you can use this.", "");
                }
            }

            if (IsMpCharEditorOpen())
            {
                if (!HasAnimDictLoaded("anim@random@shop_clothes@watches"))
                {
                    RequestAnimDict("anim@random@shop_clothes@watches");
                }
                while (!HasAnimDictLoaded("anim@random@shop_clothes@watches"))
                {
                    await Delay(0);
                }

                while (IsMpCharEditorOpen())
                {
                    await Delay(0);

                    int index = GetCameraIndex(MenuController.GetCurrentMenu());
                    if (MenuController.GetCurrentMenu() == MainMenu.MpPedCustomizationMenu.propsMenu && MenuController.GetCurrentMenu().CurrentIndex == 3 && !reverseCamera)
                    {
                        TaskPlayAnim(Game.PlayerPed.Handle, "anim@random@shop_clothes@watches", "BASE", 8f, -8f, -1, 1, 0, false, false, false);
                    }
                    else
                    {
                        Game.PlayerPed.Task.ClearAll();
                    }

                    var xOffset = 0f;
                    var yOffset = 0f;

                    if ((Game.IsControlPressed(0, Control.ParachuteBrakeLeft) || Game.IsControlPressed(0, Control.ParachuteBrakeRight)) && !(Game.IsControlPressed(0, Control.ParachuteBrakeLeft) && Game.IsControlPressed(0, Control.ParachuteBrakeRight)))
                    {
                        switch (index)
                        {
                            case 0:
                                xOffset = 2.2f;
                                yOffset = -1f;
                                break;
                            case 1:
                                xOffset = 0.7f;
                                yOffset = -0.45f;
                                break;
                            case 2:
                                xOffset = 1.35f;
                                yOffset = -0.4f;
                                break;
                            case 3:
                                xOffset = 1.0f;
                                yOffset = -0.4f;
                                break;
                            case 4:
                                xOffset = 0.9f;
                                yOffset = -0.4f;
                                break;
                            case 5:
                                xOffset = 0.8f;
                                yOffset = -0.7f;
                                break;
                            case 6:
                                xOffset = 1.5f;
                                yOffset = -1.0f;
                                break;
                            default:
                                xOffset = 0f;
                                yOffset = 0.2f;
                                break;
                        }
                        if (Game.IsControlPressed(0, Control.ParachuteBrakeRight))
                        {
                            xOffset *= -1f;
                        }

                    }

                    Vector3 pos;
                    if (reverseCamera)
                        pos = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, (CameraOffsets[index].Key.X + xOffset) * -1f, (CameraOffsets[index].Key.Y + yOffset) * -1f, CameraOffsets[index].Key.Z);
                    else
                        pos = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, (CameraOffsets[index].Key.X + xOffset), (CameraOffsets[index].Key.Y + yOffset), CameraOffsets[index].Key.Z);
                    Vector3 pointAt = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, CameraOffsets[index].Value.X, CameraOffsets[index].Value.Y, CameraOffsets[index].Value.Z);

                    if (Game.IsControlPressed(0, Control.MoveLeftOnly))
                    {
                        Game.PlayerPed.Task.LookAt(GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 1.2f, .5f, .7f), 1100);
                    }
                    else if (Game.IsControlPressed(0, Control.MoveRightOnly))
                    {
                        Game.PlayerPed.Task.LookAt(GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, -1.2f, .5f, .7f), 1100);
                    }
                    else
                    {
                        Game.PlayerPed.Task.LookAt(GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, .5f, .7f), 1100);
                    }

                    if (Game.IsControlJustReleased(0, Control.Jump))
                    {
                        var Pos = Game.PlayerPed.Position;
                        SetEntityCollision(Game.PlayerPed.Handle, true, true);
                        FreezeEntityPosition(Game.PlayerPed.Handle, false);
                        TaskGoStraightToCoord(Game.PlayerPed.Handle, Pos.X, Pos.Y, Pos.Z, 8f, 1600, Game.PlayerPed.Heading + 180f, 0.1f);
                        int timer = GetGameTimer();
                        while (true)
                        {
                            await Delay(0);
                            //DisplayRadar(false);
                            Game.DisableAllControlsThisFrame(0);
                            if (GetGameTimer() - timer > 1600)
                            {
                                break;
                            }
                        }
                        ClearPedTasks(Game.PlayerPed.Handle);
                        Game.PlayerPed.PositionNoOffset = Pos;
                        FreezeEntityPosition(Game.PlayerPed.Handle, true);
                        SetEntityCollision(Game.PlayerPed.Handle, false, false);
                        reverseCamera = !reverseCamera;
                    }

                    SetEntityCollision(Game.PlayerPed.Handle, false, false);
                    //Game.PlayerPed.IsInvincible = true;
                    Game.PlayerPed.IsPositionFrozen = true;

                    if (!DoesCamExist(CurrentCam))
                    {
                        CurrentCam = CreateCam("DEFAULT_SCRIPTED_CAMERA", true);
                        camera = new Camera(CurrentCam)
                        {
                            Position = pos,
                            FieldOfView = CameraFov
                        };
                        camera.PointAt(pointAt);
                        RenderScriptCams(true, false, 0, false, false);
                        camera.IsActive = true;
                    }
                    else
                    {
                        if (camera.Position != pos)
                        {
                            await UpdateCamera(camera, pos, pointAt);
                        }
                    }
                }

                SetEntityCollision(Game.PlayerPed.Handle, true, true);

                Game.PlayerPed.IsPositionFrozen = false;

                DisplayHud(true);
                DisplayRadar(true);

                if (HasAnimDictLoaded("anim@random@shop_clothes@watches"))
                {
                    RemoveAnimDict("anim@random@shop_clothes@watches");
                }

                reverseCamera = false;
            }
            else
            {
                if (camera != null)
                {
                    ClearCamera();
                    camera = null;
                }
            }
        }

        private int GetCameraIndex(Menu menu)
        {
            if (menu != null)
            {
                if (menu == MainMenu.MpPedCustomizationMenu.inheritanceMenu)
                {
                    return 1;
                }
                else if (menu == MainMenu.MpPedCustomizationMenu.clothesMenu)
                {
                    switch (menu.CurrentIndex)
                    {
                        case 0: // masks
                            return 1;
                        case 1: // upper body
                            return 2;
                        case 2: // lower body
                            return 3;
                        case 3: // bags & parachutes
                            return 2;
                        case 4: // shoes
                            return 4;
                        case 5: // scarfs & chains
                            return 2;
                        case 6: // shirt & accessory
                            return 2;
                        case 7: // body armor & accessory
                            return 2;
                        case 8: // badges & logos
                            return 0;
                        case 9: // shirt overlay & jackets
                            return 2;
                        default:
                            return 0;
                    }
                }
                else if (menu == MainMenu.MpPedCustomizationMenu.propsMenu)
                {
                    switch (menu.CurrentIndex)
                    {
                        case 0: // hats & helmets
                        case 1: // glasses
                        case 2: // misc props
                            return 1;
                        case 3: // watches
                            return reverseCamera ? 5 : 6;
                        case 4: // bracelets
                            return 5;
                        default:
                            return 0;
                    }
                }
                else if (menu == MainMenu.MpPedCustomizationMenu.appearanceMenu)
                {
                    switch (menu.CurrentIndex)
                    {
                        case 0: // hair style
                        case 1: // hair color
                        case 2: // hair highlight color
                        case 3: // blemishes
                        case 4: // blemishes opacity
                        case 5: // beard style
                        case 6: // beard opacity
                        case 7: // beard color
                        case 8: // eyebrows style
                        case 9: // eyebrows opacity
                        case 10: // eyebrows color
                        case 11: // ageing style
                        case 12: // ageing opacity
                        case 13: // makeup style
                        case 14: // makeup opacity
                        case 15: // makeup color
                        case 16: // blush style
                        case 17: // blush opacity
                        case 18: // blush color
                        case 19: // complexion style
                        case 20: // complexion opacity
                        case 21: // sun damage style
                        case 22: // sun damage opacity
                        case 23: // lipstick style
                        case 24: // lipstick opacity
                        case 25: // lipstick color
                        case 26: // moles and freckles style
                        case 27: // moles and freckles opacity
                            return 1;
                        case 28: // chest hair style
                        case 29: // chest hair opacity
                        case 30: // chest hair color
                        case 31: // body blemishes style
                        case 32: // body blemishes opacity
                            return 2;
                        case 33: // eye colors
                            return 1;
                        default:
                            return 0;
                    }
                }
                else if (menu == MainMenu.MpPedCustomizationMenu.tattoosMenu)
                {
                    switch (menu.CurrentIndex)
                    {
                        case 0: // head
                            return 1;
                        case 1: // torso
                            return 2;
                        case 2: // left arm
                        case 3: // right arm
                            return 6;
                        case 4: // left leg 
                        case 5: // right leg
                            return 3;
                        case 6: // badges
                            return 2;
                        default:
                            return 0;
                    }
                }
                else if (menu == MainMenu.MpPedCustomizationMenu.faceShapeMenu)
                {
                    MenuItem item = menu.GetCurrentMenuItem();
                    if (item != null)
                    {
                        if (item.GetType() == typeof(MenuSliderItem))
                        {
                            return 1;
                        }
                    }
                    return 0;
                }
            }
            return 0;
        }

        internal static void ClearCamera()
        {
            camera.IsActive = false;
            RenderScriptCams(false, false, 0, false, false);
            DestroyCam(CurrentCam, false);
            CurrentCam = -1;
            camera.Delete();
        }

        /// <summary>
        /// Disables movement while the mp character creator is open.
        /// </summary>
        /// <returns></returns>
        private async Task DisableMovement()
        {
            if (IsMpCharEditorOpen())
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
                Game.DisableControlThisFrame(0, Control.NextCamera);
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
                Game.DisableControlThisFrame(0, Control.Aim);
                Game.DisableControlThisFrame(0, Control.AccurateAim);
                Game.DisableControlThisFrame(0, Control.Cover);
                Game.DisableControlThisFrame(0, Control.Duck);
                Game.DisableControlThisFrame(0, Control.Jump);
                Game.DisableControlThisFrame(0, Control.SelectNextWeapon);
                Game.DisableControlThisFrame(0, Control.PrevWeapon);
                Game.DisableControlThisFrame(0, Control.WeaponSpecial);
                Game.DisableControlThisFrame(0, Control.WeaponSpecial2);
                Game.DisableControlThisFrame(0, Control.WeaponWheelLeftRight);
                Game.DisableControlThisFrame(0, Control.WeaponWheelNext);
                Game.DisableControlThisFrame(0, Control.WeaponWheelPrev);
                Game.DisableControlThisFrame(0, Control.WeaponWheelUpDown);
                Game.DisableControlThisFrame(0, Control.VehicleExit);
                Game.DisableControlThisFrame(0, Control.Enter);
            }
            else
            {
                await Delay(0);
            }
        }
        #endregion

        

        

    }
}
