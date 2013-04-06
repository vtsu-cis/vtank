/*!
    \file   TankCreationState.cs
    \brief  VTank's tank creation state - the third state where the user can create a customized tank.
    \author (C) Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameForms.Forms;
using Exceptions;
using Client.src.service;
using System.IO;
using GameForms.Controls;
using Client.src.util;
using GameForms;
using Network.Exception;
using TomShane.Neoforce.Controls;
using Renderer.SceneTools.Entities;
using Renderer.SceneTools;
using Client.src.util.game;

namespace Client.src.states.gamestate
{
    public class TankCreationState : State
    {
        #region Members
        private TankCreation form;
        private List<Weapon> weapons;
        private Object3 selectedTank;
        private int selectedTankRenderID = 0;
        private Object3 selectedTurret;
        private int selectedTurretRenderID = 0;
        private bool textureRenderSupported = true;
        private List<string> skinList;
        private int skinIndex = 0;
        private int defaultIndex = 0;
        private static readonly string DEFAULT_CAMO = "camo-tan";
        #endregion

        public TankCreationState() { }

        /// <summary>
        /// Initialize any components required by this state.
        /// </summary>
        public override void Initialize()
        {
            ServiceManager.Game.Renderer.ActiveScene.ClearAll();
            
            form = new TankCreation(ServiceManager.Game.Manager);
            ServiceManager.Game.FormManager.SwitchWindows(form.Window);
            form.Cancel.Click += new TomShane.Neoforce.Controls.EventHandler(Cancel_Click);
            form.Create.Click += new TomShane.Neoforce.Controls.EventHandler(Create_Click);
            form.TankSelectionChanged += new TomShane.Neoforce.Controls.EventHandler(TankSelectionChanged);
            form.TurretSelectionChanged += new TomShane.Neoforce.Controls.EventHandler(TurretSelectionChanged);
            form.ScrollSkinLeft.Click += new TomShane.Neoforce.Controls.EventHandler(ScrollSkinLeft_Click);
            form.ScrollSkinRight.Click += new TomShane.Neoforce.Controls.EventHandler(ScrollSkinRight_Click);

            skinList = Toolkit.GetSkinList();
            for (int i = 0; i < skinList.Count; ++i)
            {
                if (skinList[i] == DEFAULT_CAMO)
                {
                    defaultIndex = i;
                    break;
                }
            }

            form.TankSkin = skinList[defaultIndex];
            if (defaultIndex == 0)
                form.ScrollSkinLeft.Enabled = false;
            else if (defaultIndex + 1 >= skinList.Count)
                form.ScrollSkinRight.Enabled = false;

            skinIndex = defaultIndex;

            PopulateBoxes();

            form.TurretIndex = 0;
            form.TankIndex = 0;

            ApplySkin();
        }

        /// <summary>
        /// Populate the listboxes with turret and tank models
        /// </summary>
        private void PopulateBoxes()
        {
            //Populate turrets

            weapons = WeaponLoader.GetWeaponsAsList();
            List<object> weaponNames = new List<object>();

            foreach (Weapon weapon in weapons)
            {
                weaponNames.Add(weapon.Name);
            }

            form.Turrets = weaponNames;
            form.Tanks = Utils.GetTankModels();

            Scene scene = ServiceManager.Game.Renderer.ActiveScene;

            selectedTankRenderID = scene.Add(
                 ServiceManager.Resources.GetModel("tanks\\" + form.Tanks[0]),
                 Vector3.Zero, 0);
            selectedTank = scene.Access3D(selectedTankRenderID);

            selectedTurretRenderID = scene.Add(
                ServiceManager.Resources.GetModel("weapons\\" + weapons[0].Model),
                Vector3.Zero, 0);
            selectedTurret = scene.Access3D(selectedTurretRenderID);
            selectedTurret.Attach(selectedTank, "Mount");
        }

        /// <summary>
        /// Load any content (textures, fonts, etc) required for this state.
        /// </summary>
        public override void LoadContent()
        {
            
        }
        
        /// <summary>
        /// Unload any content (textures, fonts, etc) used by this state. Called when the state is removed.
        /// </summary>
        public override void UnloadContent()
        {
            
        }

        /// <summary>
        /// Logical updates occur here.
        /// </summary>
        /// <param name="gameTime">Delta time calculated by XNA.</param>
        public override void Update()
        {
            Scene scene = ServiceManager.Game.Renderer.ActiveScene;
            selectedTank.RotateZ(MathHelper.ToRadians((float)(10 * ServiceManager.Game.DeltaTime)));
            selectedTurret.RotateZ(MathHelper.ToRadians((float)(10 * ServiceManager.Game.DeltaTime)));
            
            // TODO: This is done here for lack of color-event handling.
            selectedTank.MeshColor = form.TankColor;
            selectedTurret.MeshColor = form.TankColor;

            ServiceManager.Game.Renderer.Update();
        }

        /// <summary>
        /// Draw this state to the screen.
        /// </summary>
        /// <param name="gameTime">Delta time calculated by XNA.</param>
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
        /// Acts as an event handler for the "Create" button.  This method is called when the user
        /// clicks on the button.
        /// </summary>
        /// <param name="obj">Access to the object clicked. Unused.</param>
        /// <param name="e">Event arguments. Unused.</param>
        private void Create_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            if (String.IsNullOrEmpty(form.TankName))
            {
                MessageBox.Show(ServiceManager.Game.Manager, 
                    MessageBox.MessageBoxType.ERROR, "You must specify a tank name", 
                    "Missing fields.");
            }
            else
            {
                CreateTank();
            }
        }


        /// <summary>
        /// Sends the selected information to the server and tries to create a new tank
        /// </summary>
        private void CreateTank()
        {
            //RGB color values 
            VTankObject.VTankColor color = Toolkit.GetVTankColor(form.TankColor);

            float armorFactor = GameForms.Utils.ConvertProgressBarToFactor(form.Armor);
            float speedFactor = 2.0f - armorFactor;

            //Rounds to prevent creation errors
            armorFactor = (float)Math.Round(armorFactor, 2);
            speedFactor = (float)Math.Round(speedFactor, 2);

            bool result = false;

            try
            {
                result = ServiceManager.Echelon.CreateTank(
                    form.TankName.Trim(), 
                    speedFactor, armorFactor, 
                    form.Tanks[form.TankIndex].ToString(),
                    form.TankSkin,
                    weapons[form.TurretIndex].ID,
                    color);
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

            if (result)
            {
                MessageBox.Show(ServiceManager.Game.Manager,
                    MessageBox.MessageBoxType.OKAY,
                    form.TankName + " created successfully!", "Success!")
                        .Closed += new TomShane.Neoforce.Controls.WindowClosedEventHandler(Confirm_Closed);
            }
            else
            {
                // TODO: Implement me.
            }
        }

        /// <summary>
        /// Event handler for the creation successful dialog
        /// </summary>
        void Confirm_Closed(object sender, TomShane.Neoforce.Controls.WindowClosedEventArgs e)
        {
            VTankObject.TankAttributes selTank = new VTankObject.TankAttributes();
            selTank.name = form.TankName;
            ServiceManager.StateManager.ChangeState(new TankListState(selTank));
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

        /// <summary>
        /// Event handler for the cancel creation dialog
        /// </summary>
        void Window_Closed(object sender, TomShane.Neoforce.Controls.WindowClosedEventArgs e)
        {
            if (((Window)sender).ModalResult == TomShane.Neoforce.Controls.ModalResult.Yes)
            {
                ServiceManager.StateManager.ChangeState(new TankListState());
            }
        }

        /// <summary>
        /// Event handler for when the user selects a different turret
        /// </summary>
        void TurretSelectionChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            Weapon selectedWeapon = weapons[form.TurretIndex];

            form.TurretPower = Utils.ConvertPowerToProgressBar(
                selectedWeapon.Projectile.AverageDamage);
            form.TurretRange = Utils.ConvertRangeToProgressBar(selectedWeapon.Projectile.Range);
            form.TurretRate = Utils.ConvertRateToProgressBar(
                selectedWeapon.Cooldown);

            Scene scene = ServiceManager.Game.Renderer.ActiveScene;
            scene.Delete(selectedTurretRenderID);
            
            selectedTurretRenderID = ServiceManager.Game.Renderer.ActiveScene.Add(
                ServiceManager.Resources.GetModel("weapons\\" + selectedWeapon.Model),
                Vector3.Zero, 0);
            float oldRotation = selectedTurret.ZRotation;
            selectedTurret = scene.Access3D(selectedTurretRenderID);
            selectedTurret.Attach(selectedTank, "Mount");
            selectedTurret.ZRotation = oldRotation;

            ApplySkin();
        }

        /// <summary>
        /// Event handler for when the user selects a different tank
        /// </summary>
        void TankSelectionChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            Scene scene = ServiceManager.Game.Renderer.ActiveScene;
            scene.Delete(selectedTankRenderID);

            selectedTankRenderID = scene.Add(
                 ServiceManager.Resources.GetModel("tanks\\" + form.Tanks[form.TankIndex]),
                 Vector3.Zero, 0);
            float oldRotation = selectedTank.ZRotation;
            selectedTank = scene.Access3D(selectedTankRenderID);
            selectedTurret.Attach(selectedTank, "Mount");
            selectedTank.ZRotation = oldRotation;

            ApplySkin();
        }

        /// <summary>
        /// Event handler for when the user changes the color sliders
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private void OnColorSliderChange(object obj, TomShane.Neoforce.Controls.EventArgs e)
        {
            // TODO: This is never called!
            selectedTank.MeshColor = form.TankColor;
            selectedTurret.MeshColor = form.TankColor;
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
    }
}
