/*!
    \file   TankListState.cs
    \brief  VTank's tank list state - the second state where the user can select, add, edit, delete their tank.
    \author (C) Copyright 2009 by Vermont Technical College

*/
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameForms.Forms;
using Exceptions;
using Client.src.service;
using System.Collections.Generic;
using GameForms.Controls;
using GameForms;
using Client.src.util;
using Renderer.SceneTools;
using TomShane.Neoforce.Controls;
using Renderer;
using Client.src.util.game;
using System.Threading;

namespace Client.src.states.gamestate
{
    public class TankListState : State
    {
        #region Members
        TankList form;
        VTankObject.TankAttributes selectedTank;
        VTankObject.TankAttributes[] tanks;
        GameTime time;
        private int tank = -1;
        private int turret = -1;
        private Renderer.SceneTools.Entities.Object3 tankObj;
        private Renderer.SceneTools.Entities.Object3 turretObj;
        private Rank rank;
        private bool textureRenderSupported = true;

        #endregion
        
        #region Constructors
        public TankListState() : this(null)
        {
            
        }

        /// <summary>
        /// Constructor: Will automatically select the tank that is passed in if it exists.
        /// </summary>
        public TankListState(VTankObject.TankAttributes _selectedTank)
        {
            ServiceManager.Game.Renderer.ActiveScene.ClearAll();
            form = new TankList(ServiceManager.Game.Manager);
            form.CreateButton.Click += new TomShane.Neoforce.Controls.EventHandler(CreateButton_Click);
            form.EditButton.Click += new TomShane.Neoforce.Controls.EventHandler(EditButton_Click);
            form.DeleteButton.Click += new TomShane.Neoforce.Controls.EventHandler(DeleteButton_Click);
            form.PlayButton.Click += new TomShane.Neoforce.Controls.EventHandler(PlayButton_Click);
            form.BackButton.Click += new TomShane.Neoforce.Controls.EventHandler(BackButton_Click);
            form.SelectionChanged += new TomShane.Neoforce.Controls.EventHandler(SelectionChanged);

            form.PlayButton.Enabled = false;
            form.EditButton.Enabled = false;
            form.DeleteButton.Enabled = false;

            selectedTank = _selectedTank;
            RefreshTankList();

            ServiceManager.Game.Renderer.ActiveScene.SwitchCamera("Tank Display View");
            GraphicOptions.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            Renderer.GraphicOptions.BackgroundColor = Color.Black;
        }

        /// <summary>
        /// Refresh the current list of tanks
        /// </summary>
        private void RefreshTankList()
        {
            bool tankListAvailable = ServiceManager.Echelon.GetTankListAsync((x) =>
            {
                ServiceManager.Game.Invoke((y) =>
                {
                    tanks = x.ReturnedResult;
                    RefreshTankListForm();
                }, x);
            });

            if (tankListAvailable)
            {
                tanks = ServiceManager.Echelon.GetTankList();
                RefreshTankListForm();
            }
        }

        private void RefreshTankListForm()
        {
            // This action occurs in the GUI thread.
            if (tanks != null)
            {
                List<object> t = new List<object>();
                foreach (VTankObject.TankAttributes attr in tanks)
                {
                    t.Add((object)attr.name);
                }
                form.List = t;
            }

            if (tanks.Length > 0)
            {
                if (selectedTank != null)
                {
                    if (form.List.Contains(selectedTank.name))
                    {
                        form.SelectedIndex = form.List.IndexOf(selectedTank.name);
                        selectedTank = tanks[form.SelectedIndex];
                    }
                    else
                    {
                        form.SelectedIndex = 0;
                        selectedTank = tanks[0];
                    }
                }
                else
                {
                    form.SelectedIndex = 0;
                    selectedTank = tanks[0];
                }

                form.PlayButton.Enabled = true;
                form.EditButton.Enabled = true;
                form.DeleteButton.Enabled = true;
            }
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// Initialize any components required by this state.
        /// </summary>
        public override void Initialize()
        {
            ServiceManager.Game.FormManager.SwitchWindows(form.Window);
            time = new GameTime();

            RendererAssetPool.UniversalEffect.LightParameters.FogSeedPosition = Vector3.Zero;
            RendererAssetPool.DrawShadows = false;

            GetRank();
        }

        /// <summary>
        /// Get and display the user's tank.
        /// </summary>
        private void GetRank()
        {
            ServiceManager.Echelon.GetRankAsync((x) =>
            {
                ServiceManager.Game.Invoke((y) =>
                {
                    // This action is performed in the GUI thread:
                    int rankID = x.ReturnedResult;
                    rank = RankLoader.GetRank(rankID);
                    if (rank != null)
                    {
                        Texture2D rankTexture = rank.GetTexture();
                        if (rankTexture != null)
                        {
                            form.RankImage.Image = rankTexture;
                        }

                        form.RankLabel.Text = rank.Title;
                    }
                    else
                    {
                        System.Console.Error.WriteLine("Warning: Rank of rank ID {0} is null.", rankID);
                    }

                    const int GENERAL_RANK = 19;
                    if (rankID != GENERAL_RANK)
                    {
                        long minimumPoints = 0;
                        if (rankID != 0)
                        {
                            minimumPoints = ServiceManager.Echelon.GetPointsForRank(rankID);
                        }

                        long myPoints = ServiceManager.Echelon.GetProxy().GetAccountPoints();
                        long requiredPoints = ServiceManager.Echelon.GetPointsForRank(rankID + 1);

                        // Do calculation to figure out percentage of how far we have to go.
                        double pointsInRank = requiredPoints - minimumPoints;
                        double translatedPoints = myPoints - minimumPoints;

                        form.RankProgressBar.Value = (int)((translatedPoints / pointsInRank) * 100.0);
                        form.RankProgressBar.ToolTip.Text = String.Format("{0} / {1}", myPoints, requiredPoints);

                        int killsRequired = (int)((double)(requiredPoints - myPoints) / 10.0);
                        form.RankNextLabel.Text = String.Format(
                            "{0} more {1} to rank up.", killsRequired, killsRequired == 1 ? "kill" : "kills");
                    }
                    else
                    {
                        form.RankLabel.Text = "General";
                        form.RankProgressBar.Value = 100;
                        form.RankNextLabel.Text = "You are at the maximum rank!";
                    }
                }, x);
            });
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
            form.Window.Hide();
        }

        /// <summary>
        /// Logical updates occur here.
        /// </summary>
        /// <param name="gameTime">Delta time calculated by XNA.</param>
        public override void Update()
        {
            Scene scene = ServiceManager.Game.Renderer.ActiveScene;
            scene.PercentOfDayComplete = 0.5;
            if (tank >= 0)
            {
                tankObj.RotateZ(MathHelper.ToRadians((float)(10 * ServiceManager.Game.DeltaTime)));
            }

            if (turret >= 0)
            {
                turretObj.RotateZ(MathHelper.ToRadians((float)(10 * ServiceManager.Game.DeltaTime)));
            }
            ServiceManager.Game.Renderer.Update();

            if (KeyPressHelper.IsPressed(Keys.Escape))
            {
                SwitchToLoginScreenState();
            }
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
                    "Warning: Unable to render 3D models as a texture. {0}\nDetails: {1}", 
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
        void CreateButton_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            ServiceManager.StateManager.ChangeState(new TankCreationState());
        }

        /// <summary>
        /// Acts as an event handler for the "Back" button.  This method is called when the user
        /// clicks on the button.
        /// </summary>
        /// <param name="obj">Access to the object clicked. Unused.</param>
        /// <param name="e">Event arguments. Unused.</param>
        void BackButton_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            SwitchToLoginScreenState();
        }

        /// <summary>
        /// Switches to the LoginScreenState, performing any extra actions.
        /// </summary>
        private void SwitchToLoginScreenState()
        {
            ServiceManager.DestroyEchelonCommunicator();
            ServiceManager.StateManager.ChangeState(new LoginScreenState());
        }

        /// <summary>
        /// Acts as an event handler for the "Edit" button. This method is called when the user
        /// clicks on the button
        /// </summary>
        /// <param name="obj">Access to the object clicked. Unused</param>
        /// <param name="e">Event arguments. Unused</param>
        void EditButton_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            ServiceManager.StateManager.ChangeState(
                new EditTankState(tanks[form.SelectedIndex]));
        }

        /// <summary>
        /// Acts as an event handler for the "Play" button.  This method is called when the user
        /// clicks on the button.
        /// </summary>
        /// <param name="obj">Access to the object clicked. Unused.</param>
        /// <param name="e">Event arguments. Unused.</param>
        void PlayButton_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            String tankName = form.List[form.SelectedIndex].ToString();
            bool result = ServiceManager.Echelon.SelectTank(tankName);

            if (result)
            {
                PlayerManager.LocalPlayerName = tankName;
                ServiceManager.StateManager.ChangeState(new ServerListState());
            }
            else
            {
                MessageBox.Show(ServiceManager.Game.Manager, 
                    MessageBox.MessageBoxType.ERROR,
                    "Error encountered when selecting " + tankName + ".", 
                    "Unable to select tank.");
            }
        }

        /// <summary>
        /// Acts as an event handler for the "Delete" button.  This method is called when the user
        /// clicks on the button.
        /// </summary>
        /// <param name="obj">Access to the object clicked. Unused.</param>
        /// <param name="e">Event arguments. Unused.</param>
        void DeleteButton_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            DeleteTank();
        }


        /// <summary>
        /// Trys to delete the selected user tank
        /// </summary>
        private void DeleteTank()
        {
            string tankName = form.List[form.SelectedIndex].ToString();
            MessageBox.Show(ServiceManager.Game.Manager, 
                MessageBox.MessageBoxType.YES_NO, 
                String.Format("Are you sure you want to delete {0}?", tankName), 
                "Cancel").Closed += new TomShane.Neoforce.Controls.WindowClosedEventHandler(
                    Window_Closed);
        }

        /// <summary>
        /// Event handler for when the user confirms a deletion
        /// </summary>
        void Window_Closed(object sender, TomShane.Neoforce.Controls.WindowClosedEventArgs e)
        {
            Window window = (Window)sender;
            if (window.ModalResult == TomShane.Neoforce.Controls.ModalResult.Yes)
            {
                try
                {
                    string tankName = form.List[form.SelectedIndex].ToString();
                    bool result = ServiceManager.Echelon.DeleteTank(tankName);

                    if (result)
                    {
                        MessageBox.Show(ServiceManager.Game.Manager, 
                            MessageBox.MessageBoxType.OKAY,
                            String.Format("{0} was deleted.", tankName), 
                            "Successful.");
                    }
                    else
                    {
                        MessageBox.Show(ServiceManager.Game.Manager, 
                            MessageBox.MessageBoxType.ERROR,
                            "An unknown error prevented the tank from being deleted.", 
                            "Couldn't delete the tank.");
                    }

                    RefreshTankList();

                    if (form.List.Count > 0)
                        form.SelectedIndex = 0;
                }
                catch (Exceptions.BadInformationException ex)
                {
                    MessageBox.Show(ServiceManager.Game.Manager, 
                        MessageBox.MessageBoxType.ERROR, ex.reason, "Error!");
                }
                catch (Exceptions.PermissionDeniedException ex)
                {
                    MessageBox.Show(ServiceManager.Game.Manager, 
                        MessageBox.MessageBoxType.ERROR, ex.reason, "Error!");
                }
            }
        }

        /// <summary>
        /// Apply the skin to the tank.
        /// </summary>
        private void ApplySkin()
        {
            tankObj.ClearMeshSkins();
            turretObj.ClearMeshSkins();

            Texture2D skinTexture = Toolkit.GetSkin(selectedTank.skin);
            const int ARBITRARY_CEILING = 2000;
            for (int i = 0; i < ARBITRARY_CEILING; ++i)
            {
                if (!tankObj.AddMeshSkin("Body" + i, skinTexture))
                    break;
            }

            for (int i = 0; i < ARBITRARY_CEILING; ++i)
            {
                if (!turretObj.AddMeshSkin("Body" + i, skinTexture))
                    break;
            }
        }

        /// <summary>
        /// Event handler for when the user selects a different tank
        /// </summary>
        void SelectionChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            if (tanks.Length > 0)
            {
                form.PlayButton.Enabled = true;
                form.EditButton.Enabled = true;
                form.DeleteButton.Enabled = true;

                selectedTank = tanks[form.SelectedIndex];

                form.Armor = GameForms.Utils.ConvertFactorToProgressBar(selectedTank.armorFactor);
                form.Speed = GameForms.Utils.ConvertFactorToProgressBar(selectedTank.speedFactor);

                Scene scene = ServiceManager.Game.Renderer.ActiveScene;
                scene.ClearAll();

                tankObj = new Renderer.SceneTools.Entities.Object3(
                    ServiceManager.Resources.GetModel(@"tanks\" + selectedTank.model), Vector3.Zero);
                tankObj.MeshColor = Toolkit.GetColor(selectedTank.color);
                
                tank = scene.Add(tankObj, 0);
                turretObj = new Renderer.SceneTools.Entities.Object3(
                    ServiceManager.Resources.GetModel(@"weapons\" + WeaponLoader.GetWeapon(selectedTank.weaponID).Model),
                    Vector3.Zero);

                turretObj.MeshColor = Toolkit.GetColor(selectedTank.color);
                turret = scene.Add(turretObj, 0);
                turretObj.Attach(tankObj, Constants.TURRET_MOUNT);

                ApplySkin();
            }
        }

        #endregion
    }
}
