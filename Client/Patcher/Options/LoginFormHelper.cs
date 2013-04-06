using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace VTankTitleScreen
{
    public static class LoginFormHelper
    {
        public static void writeOptions(VTankOptions options)
        {
             ConfigurationManager.AppSettings.Set("ServerAddress", options.ServerAddress);
             ConfigurationManager.AppSettings.Set("ServerPort", options.ServerPort);
             ConfigurationManager.AppSettings.Set("DefaultAccount", options.DefaultAccount );
             ConfigurationManager.AppSettings.Set("MapsFolder", options.MapsFolder);
             ConfigurationManager.AppSettings.Set("options.video.Resolution", options.videoOptions.Resolution);
             ConfigurationManager.AppSettings.Set("options.video.Windowed", options.videoOptions.Windowed);
             ConfigurationManager.AppSettings.Set("Toptions.video.TextureQuality", options.videoOptions.TextureQuality);
             ConfigurationManager.AppSettings.Set("options.video.AntiAliasing", options.videoOptions.AntiAliasing );
             ConfigurationManager.AppSettings.Set("options.audio.ambientSound.Volume", options.audioOptions.ambientSound.Volume); 
             ConfigurationManager.AppSettings.Set("options.audio.ambientSound.Muted", options.audioOptions.ambientSound.Muted);
             ConfigurationManager.AppSettings.Set("options.audio.backgroundSound.Volume", options.audioOptions.backgroundSound.Volume);
             ConfigurationManager.AppSettings.Set("options.audio.backgroundSound.Muted", options.audioOptions.backgroundSound.Muted);
             ConfigurationManager.AppSettings.Set("options.gameplay.ShowNames", options.gamePlayOptions.ShowNames);
             ConfigurationManager.AppSettings.Set("options.gameplay.ProfanityFilter", options.gamePlayOptions.ProfanityFilter);
             ConfigurationManager.AppSettings.Set("options.gameplay.InterfacePlugin", options.gamePlayOptions.InterfacePlugin);
             ConfigurationManager.AppSettings.Set("options.keybindings.Forward", options.keyBindings.Forward);
             ConfigurationManager.AppSettings.Set("options.keybindings.Backward", options.keyBindings.Backward);
             ConfigurationManager.AppSettings.Set("options.keybindings.RotateLeft", options.keyBindings.RotateLeft);
             ConfigurationManager.AppSettings.Set("options.keybindings.RotateRight", options.keyBindings.RotateRight);
             ConfigurationManager.AppSettings.Set("options.keybindings.FirePrimary", options.keyBindings.FirePrimary);
             ConfigurationManager.AppSettings.Set("options.keybindings.FireSecondary", options.keyBindings.FireSecondary);
             ConfigurationManager.AppSettings.Set("options.keybindings.Menu", options.keyBindings.Menu);
             ConfigurationManager.AppSettings.Set("options.keybindings.Minimap", options.keyBindings.Minimap );
             ConfigurationManager.AppSettings.Set("options.keybindings.Score", options.keyBindings.Score);
             ConfigurationManager.AppSettings.Set("options.keybindings.Pointer", options.keyBindings.Pointer);
        }

        public static VTankOptions readOptions(String fileName)
        {
            VTankOptions options = new VTankOptions();
            options.ServerAddress = ConfigurationManager.AppSettings.Get("ServerAddress");
            options.ServerPort = ConfigurationManager.AppSettings.Get("ServerPort");
            options.DefaultAccount = ConfigurationManager.AppSettings.Get("DefaultAccount");
            options.MapsFolder = ConfigurationManager.AppSettings.Get("MapsFolder");
			options.videoOptions.Resolution = ConfigurationManager.AppSettings.Get("options.video.Resolution");
			options.videoOptions.Windowed = ConfigurationManager.AppSettings.Get("options.video.Windowed");
			options.videoOptions.TextureQuality = ConfigurationManager.AppSettings.Get("Toptions.video.TextureQuality");
            options.videoOptions.AntiAliasing = ConfigurationManager.AppSettings.Get("options.video.AntiAliasing");
		    options.audioOptions.ambientSound.Volume = ConfigurationManager.AppSettings.Get("options.audio.ambientSound.Volume");
			options.audioOptions.ambientSound.Muted = ConfigurationManager.AppSettings.Get("options.audio.ambientSound.Muted");
			options.audioOptions.backgroundSound.Volume = ConfigurationManager.AppSettings.Get("options.audio.backgroundSound.Volume");
			options.audioOptions.backgroundSound.Muted = ConfigurationManager.AppSettings.Get("options.audio.backgroundSound.Muted");
			options.gamePlayOptions.ShowNames = ConfigurationManager.AppSettings.Get("options.gameplay.ShowNames" );
			options.gamePlayOptions.ProfanityFilter = ConfigurationManager.AppSettings.Get("options.gameplay.ProfanityFilter");
			options.gamePlayOptions.InterfacePlugin = ConfigurationManager.AppSettings.Get("options.gameplay.InterfacePlugin");
			options.keyBindings.Forward = ConfigurationManager.AppSettings.Get("options.keybindings.Forward");
            options.keyBindings.Backward = ConfigurationManager.AppSettings.Get("options.keybindings.Backward");
            options.keyBindings.RotateLeft = ConfigurationManager.AppSettings.Get("options.keybindings.RotateLeft");
            options.keyBindings.RotateRight = ConfigurationManager.AppSettings.Get("options.keybindings.RotateRight");
            options.keyBindings.FirePrimary = ConfigurationManager.AppSettings.Get("options.keybindings.FirePrimary");
            options.keyBindings.FireSecondary = ConfigurationManager.AppSettings.Get("options.keybindings.FireSecondary");
            options.keyBindings.Menu = ConfigurationManager.AppSettings.Get("options.keybindings.Menu");
            options.keyBindings.Minimap = ConfigurationManager.AppSettings.Get("options.keybindings.Minimap");
            options.keyBindings.Score = ConfigurationManager.AppSettings.Get("options.keybindings.Score");
            options.keyBindings.Pointer = ConfigurationManager.AppSettings.Get("options.keybindings.Pointer");
            return options;
        }
    }
}