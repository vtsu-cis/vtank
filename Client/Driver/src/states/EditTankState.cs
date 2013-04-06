/*!
    \file   EditTankState.cs
    \brief  State allowing for editing of a selected tank
    \author (C) Copyright 2008 by Vermont Technical College
*/
using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Client.src.config;
using Client.src.callbacks;

using System.IO;
using Exceptions;
using GameForms.Controls;
using GameForms.Forms;
using Client.src.service;
using Client.src.util;
using GameForms;
using TomShane.Neoforce.Controls;
using Renderer.SceneTools;
using Network.Exception;
using Renderer.SceneTools.Entities;
using Client.src.service.services;
using Client.src.util.game;

namespace Client.src.states.gamestate
{
    /// <summary>
    /// Allows for a user to change the selected tank attributes
    /// </summary>
    public class EditTankState : State
    {
        #region Members
        private VTankObject.TankAttributes tank;
        private TankCreation form;
        private List<Weapon> weapons;
        private Object3 selectedTank;
        private int selectedTankRenderID = 0;
        private string selectedTankModelName = null;
        private string oldSkin;
        private Object3 selectedTurret;
        private int selectedTurretRenderID = 0;
        private string selectedTurretName = null;
        private bool textureRenderSupported = true;
        private bool changesMade = false;
        private int skinIndex;
        private List<string> skinList;
        private int defaultIndex;
        #endregion
        
        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_selectedTank">The selected tank used for editing</param>
        public EditTankState(VTankObject.TankAttributes _selectedTank)
        {
            tank = _selectedTank;
            selectedTankModelName = tank.model;
            selectedTurretName = WeaponLoader.GetWeapon(tank.weaponID).Name;
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// Initializes components
        /// </summary>
        public override void Initialize()
        {
            ServiceManager.Scene.ClearAll();

            form = new TankCreation(ServiceManager.Game.Manager);
            ServiceManager.Game.FormManager.SwitchWindows(form.Window);
            form.Cancel.Click += new TomShane.Neoforce.Controls.EventHandler(Cancel_Click);
            form.Create.Text = "Edit Tank";
            form.Create.Click += new TomShane.Neoforce.Controls.EventHandler(Edit_Click);
            form.TankSelectionChanged += new TomShane.Neoforce.Controls.EventHandler(TankSelectionChanged);
            form.TurretSelectionChanged += new TomShane.Neoforce.Controls.EventHandler(TurretSelectionChanged);
            form.ScrollSkinLeft.Click += new TomShane.Neoforce.Controls.EventHandler(ScrollSkinLeft_Click);
            form.ScrollSkinRight.Click += new TomShane.Neoforce.Controls.EventHandler(ScrollSkinRight_Click);
            skinList = Toolkit.GetSkinList();
            for (int i = 0; i < skinList.Count; ++i)
            {
                if (skinList[i] == tank.skin)
                {
                    defaultIndex = i;
                    break;
                }
            }

            skinIndex = defaultIndex;
            oldSkin = tank.skin;

            PopulateBoxes();

            form.TurretIndex = 0;
            form.TankIndex = 0;
            
            form.TankName = tank.name;
            form.TankSkin = tank.skin;
            form.NameEditable = false;

            form.TankColor = Toolkit.GetColor(tank.color);
            form.TankSkin = skinList[skinIndex];
            ApplySkin();

            SetStatistics();
        }

        private void SetStatistics()
        {
            form.RatioValue = Utils.ConvertFactorToProgressBar(tank.armorFactor);

            form.TankIndex = form.Tanks.IndexOf(tank.model);
            form.TurretIndex = form.Turrets.IndexOf(WeaponLoader.GetWeapon(tank.weaponID).Name);
        }

        /// <summary>
        /// Populate the listboxes with turret and tank models
        /// </summary>
        private void PopulateBoxes()
        {
            List<object> turrets = new List<object>();

            //Populate turrets

            weapons = WeaponLoader.GetWeaponsAsList();

            foreach (Weapon weapon in weapons)
            {
                turrets.Add(weapon.Name);
            }

            form.Turrets = turrets;
            form.Tanks = Utils.GetTankModels();

            Renderer.SceneTools.Scene scene = ServiceManager.Scene;

            selectedTankRenderID = scene.Add(
                 ServiceManager.Resources.GetModel("tanks\\" + tank.model),
                 Vector3.Zero, 0);
            selectedTank = scene.Access3D(selectedTankRenderID);

            selectedTurretRenderID = scene.Add(
                ServiceManager.Resources.GetModel("weapons\\" + WeaponLoader.GetWeapon(tank.weaponID).Model),
                Vector3.Zero, 0);
            selectedTurret = scene.Access3D(selectedTurretRenderID);
            selectedTurret.Attach(selectedTank, Constants.TURRET_MOUNT);
        }

        /// <summary>
        /// Loads the on-screen forums and gets the data to render on screen
        /// </summary>
        public override void LoadContent()
        {
            
        }

        /// <summary>
        /// Acts as an event handler for the "Create" button.  This method is called when the user
        /// clicks on the button.
        /// </summary>
        /// <param name="obj">Access to the object clicked. Unused.</param>
        /// <param name="e">Event arguments. Unused.</param>
        void Edit_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            MessageBox.Show(ServiceManager.Game.Manager, 
                MessageBox.MessageBoxType.YES_NO, 
                "Are you sure you want to edit this tank?", 
                "Confirmation").Closed += new TomShane.Neoforce.Controls.WindowClosedEventHandler(Edit_Window_Closed);
        }

        /// <summary>
        /// After the yes button is presses, the tank modifications will be sent to the server
        /// </summary>
        /// <param name="obj">Not used</param>
        /// <param name="e">Not used</param>
        void Edit_Window_Closed(object sender, TomShane.Neoforce.Controls.WindowClosedEventArgs e)
        {
            if (((Window)sender).ModalResult == TomShane.Neoforce.Controls.ModalResult.Yes)
            {
                if (changesMade == true)
                    EditTank();
            }
        }

        /// <summary>
        /// Sends the modified values back to the server to save to the tank that was selected
        /// for editing.
        /// </summary>
        private void EditTank()
        {
            try
            {
                float armorFactor = GameForms.Utils.ConvertProgressBarToFactor(form.Armor);
                float speedFactor = 2.0f - armorFactor;

                //Rounds to prevent creation errors
                armorFactor = (float)Math.Round(armorFactor, 2);
                speedFactor = (float)Math.Round(speedFactor, 2);

                tank.model = form.Tanks[form.TankIndex].ToString();
                tank.color = Toolkit.GetVTankColor(form.TankColor);

                if (ServiceManager.Echelon.UpdateTank(tank.name,
                        speedFactor,
                        armorFactor,
                        tank.model,  
                        form.TankSkin,
                        weapons[form.TurretIndex].ID,
                        tank.color))
                {
                    MessageBox.Show(ServiceManager.Game.Manager, 
                        MessageBox.MessageBoxType.OKAY, 
                        form.TankName + " edited successfully", 
                        "Success").Closed += new TomShane.Neoforce.Controls.WindowClosedEventHandler(Confirm_Closed);
                }
                else
                {
                    MessageBox.Show(ServiceManager.Game.Manager,
                        MessageBox.MessageBoxType.OKAY,
                        form.TankName + " was not modified!",
                        "Success").Closed += new TomShane.Neoforce.Controls.WindowClosedEventHandler(Confirm_Closed);
                }
            }
            catch (InvalidValueException ive)
            {
                // User entered invalid information. (Detected by Network)
                MessageBox.Show(ServiceManager.Game.Manager, MessageBox.MessageBoxType.ERROR,
                    ive.Message, "Error!");
            }
            catch (BadInformationException ex)
            {
                // User entered invalid information. (Detected by server)
                MessageBox.Show(ServiceManager.Game.Manager, MessageBox.MessageBoxType.ERROR,
                    ex.reason, "Error!");
            }
            catch (VTankException e)
            {
                // Generic VTank exception, might contain internal VTank errors.
                MessageBox.Show(ServiceManager.Game.Manager, MessageBox.MessageBoxType.ERROR,
                   e.reason, "Error!");
            }
            catch (Ice.Exception e)
            {
                MessageBox.Show(ServiceManager.Game.Manager, MessageBox.MessageBoxType.ERROR,
                    "You have lost connection.\n" + e.Message, "Error!");
                // TODO: This should return to the login screen.
            }
            catch (Exception e)
            {
                // Unexpected code-related exception.
                MessageBox.Show(ServiceManager.Game.Manager, MessageBox.MessageBoxType.ERROR,
                    "Unknown error:\n" + e.Message, "Error!");
            }
        }

        /// <summary>
        /// Event handler for the edit successful dialog
        /// </summary>
        void Confirm_Closed(object sender, TomShane.Neoforce.Controls.WindowClosedEventArgs e)
        {
            ServiceManager.StateManager.ChangeState(new TankListState(tank));
        }

        /// <summary>
        /// Acts as an event handler for the "Cancel" button.  This method is called when the user
        /// clicks on the button.
        /// </summary>
        /// <param name="obj">Access to the object clicked. Unused.</param>
        /// <param name="e">Event arguments. Unused.</param>
        void Cancel_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            MessageBox.Show(ServiceManager.Game.Manager, 
                MessageBox.MessageBoxType.YES_NO, 
                "Are you sure you want to cancel and lose all changes?", 
                "Cancel").Closed += new TomShane.Neoforce.Controls.WindowClosedEventHandler(Window_Closed);
        }

        void Window_Closed(object sender, TomShane.Neoforce.Controls.WindowClosedEventArgs e)
        {
            if (((Window)sender).ModalResult == TomShane.Neoforce.Controls.ModalResult.Yes)
            {
                ServiceManager.StateManager.ChangeState(new TankListState(tank));
            }
        }

        void TurretSelectionChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            Weapon selectedWeapon = weapons[form.TurretIndex];

            form.TurretPower = Utils.ConvertPowerToProgressBar(
                selectedWeapon.Projectile.AverageDamage);
            form.TurretRange = Utils.ConvertRangeToProgressBar(selectedWeapon.Projectile.Range);
            form.TurretRate = Utils.ConvertRateToProgressBar(
                selectedWeapon.Cooldown);

            Renderer.SceneTools.Scene scene = ServiceManager.Scene;
            scene.Delete(selectedTurretRenderID);

            selectedTurretRenderID = ServiceManager.Scene.Add(
                ServiceManager.Resources.GetModel("weapons\\" + selectedWeapon.Model),
                Vector3.Zero, 0);
            float oldRotation = selectedTurret.ZRotation;
            selectedTurret = scene.Access3D(selectedTurretRenderID);
            selectedTurret.Attach(selectedTank, Constants.TURRET_MOUNT);
            selectedTurret.ZRotation = oldRotation;

            selectedTurretName = selectedWeapon.Name;
            ApplySkin();
        }

        void TankSelectionChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            Renderer.SceneTools.Scene scene = ServiceManager.Scene;
            scene.Delete(selectedTankRenderID);

            selectedTankRenderID = scene.Add(
                 ServiceManager.Resources.GetModel("tanks\\" + form.Tanks[form.TankIndex]),
                 Vector3.Zero, 0);
            float oldRotation = selectedTank.ZRotation;
            selectedTank = scene.Access3D(selectedTankRenderID);
            selectedTurret.Attach(selectedTank, Constants.TURRET_MOUNT);
            selectedTank.ZRotation = oldRotation;

            selectedTankModelName = (string)form.Tanks[form.TankIndex];
            ApplySkin();
        }

        /// <summary>
        /// Unloads all the content and lets the garbage collector clean up
        /// </summary>
        public override void UnloadContent()
        {
            
        }

        /// <summary>
        /// Updates the screen
        /// </summary>
        /// <param name="gameTime">XNA gameTime</param>
        public override void Update()
        {
            Renderer.SceneTools.Scene scene = ServiceManager.Scene;
            selectedTank.RotateZ(MathHelper.ToRadians((float)(10 * ServiceManager.Game.DeltaTime)));
            selectedTurret.RotateZ(MathHelper.ToRadians((float)(10 * ServiceManager.Game.DeltaTime)));

            if (selectedTurret.MeshColor != form.TankColor || selectedTank.MeshColor != form.TankColor)
                changesMade = true;

            this.DetectChanges();

            // TODO: This is done here for lack of color-event handling.
            selectedTank.MeshColor = form.TankColor;
            selectedTurret.MeshColor = form.TankColor;

            ServiceManager.Game.Renderer.Update();
        }

        /// <summary>
        /// Draws various game components on screen
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw()
        {
            DrawBack();
            try
            {
                if (textureRenderSupported)
                {
                    form.Image = ServiceManager.Game.Renderer.DrawAsImage();
                }
                else
                {
                    // It won't attempt to load it repeatedly -- it'll cache it.
                    form.Image = ServiceManager.Resources.LoadResource<Texture2D>(
                        "", ResourceManager.TexturePlaceholder);
                    SpriteBatch batch = ServiceManager.Game.Batch;
                    batch.Begin(SpriteBlendMode.AlphaBlend,
                        SpriteSortMode.FrontToBack, SaveStateMode.SaveState);
                    batch.DrawString(ServiceManager.Game.Font,
                        "Warning: Unable to preview tanks: rendering to texture not supported.",
                        Vector2.Zero, Color.White);
                    batch.End();
                }
            }
            catch (Exception e)
            {
                ServiceManager.Game.Console.DebugPrint(
                    "Warning: Unable to render 3D models as a texture. {0} Details: {1}",
                    "Placeholder texture loaded instead.", e.Message);
                textureRenderSupported = false;
            }
        }

        /// <summary>
        /// Check to see if any changes have been made to the tank or turret of this tank.
        /// </summary>
        private void DetectChanges()
        {
            string currentTankWeapon = WeaponLoader.GetWeapon(tank.weaponID).Name;
            string currentTankModel = tank.model;

            if (currentTankWeapon != selectedTurretName || currentTankModel != selectedTankModelName ||
                oldSkin != form.TankSkin)
            {
                changesMade = true;
            }

        }

        /// <summary>
        /// Apply the skin to the tank.
        /// </summary>
        private void ApplySkin()
        {
            selectedTank.ClearMeshSkins();
            selectedTurret.ClearMeshSkins();

            Texture2D skinTexture = Toolkit.GetSkin(form.TankSkin);
            const int ARBITRARY_CEILING = 2000;
            for (int i = 0; i < ARBITRARY_CEILING; ++i)
            {
                if (!selectedTank.AddMeshSkin("Body" + i, skinTexture))
                    break;
            }

            for (int i = 0; i < ARBITRARY_CEILING; ++i)
            {
                if (!selectedTurret.AddMeshSkin("Body" + i, skinTexture))
                    break;
            }
        }

        /// <summary>
        /// Handles the event where the 'right' scroller is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollSkinRight_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            skinIndex++;
            if (skinIndex + 1 >= skinList.Count)
                form.ScrollSkinRight.Enabled = false;
            if (skinIndex > 0)
                form.ScrollSkinLeft.Enabled = true;

            form.TankSkin = skinList[skinIndex];
            ApplySkin();
        }

        /// <summary>
        /// Handles the event where the 'left' scroller is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollSkinLeft_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            skinIndex--;
            if (skinIndex + 1 < skinList.Count)
                form.ScrollSkinRight.Enabled = true;
            if (skinIndex == 0)
                form.ScrollSkinLeft.Enabled = false;

            form.TankSkin = skinList[skinIndex];
            ApplySkin();
        }
        #endregion
    }
}
