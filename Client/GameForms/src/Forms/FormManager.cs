using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using GameForms.Forms;
using GameForms.Controls;

namespace GameForms.Forms
{
    public class FormManager
    {
        public GraphicsDeviceManager graphics;
        public Manager manager;
        public Game parent;
        public Window currentWindow = null;


        public FormManager(GraphicsDeviceManager _graphics, Game _game, Manager _manager)
        {
            this.graphics = _graphics;
            this.parent = _game;

            manager = _manager;
        }

        public void AddWindow(GameForm form)
        {
            AddWindow(GetForm(form));
        }

        public void AddWindow(Window window)
        {
            this.currentWindow = window;
            manager.Add(currentWindow);
        }

        public void SwitchWindows(Window window)
        {
            if (currentWindow != null)
            {
                currentWindow.Close();
                manager.Remove(currentWindow);
            }

            currentWindow = window;
            manager.Add(currentWindow);
        }

        public void SwitchWindows(GameForm form)
        {
            SwitchWindows(GetForm(form));
        }

        public void RemoveWindow(Window window)
        {
            manager.Remove(window);
                currentWindow = null;
        }

        public void ClearWindow()
        {
            if (currentWindow != null)
            {
                manager.Remove(currentWindow);
                currentWindow = null;
            }
            else
                return;
        }

        private Window GetForm(GameForm form)
        {
            Window window;

            switch (form)
            {
                case GameForm.LOGIN_FORM:
                    window = (new LoginForm(manager)).Window;
                    break;

                case GameForm.TANK_LIST_FORM:
                    window = new TankList(manager).Window;
                    break;

                case GameForm.TANK_CREATION_FORM:
                    window = new TankCreation(manager).Window;
                    break;

                case GameForm.TANK_EDIT_FORM:
                    window = new Window(manager);
                    break;

                case GameForm.SERVER_LIST_FORM:
                    window = new Window(manager);
                    break;

                case GameForm.LOADING_SCREEN_FORM:
                    window = new Window(manager);
                    break;

                case GameForm.GAMEPLAY_FORM:
                    window = new Window(manager);
                    break;

                default:
                    window = new Window(manager);
                    break;
            }

            return window;
        }
    }
}
