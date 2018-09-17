using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Client.src.utils;
using System.Runtime.InteropServices;
using System.IO;

namespace VTankOptionsSpace
{
    public partial class OptionsForm : Form
    {
        private bool changes = false;
        private VTankOptions options;

        /// <summary>
        /// Get the options used in this program.
        /// </summary>
        public VTankOptions Options
        {
            get
            {
                return options;
            }
        }

        public OptionsForm()
        {
            InitializeComponent();

            options = VTankOptions.ReadOptions();
            SetValues();
            SetChangeMade(false);
        }

        private int SortResolutions(object o1, object o2)
        {
            string resString1 = o1.ToString();
            string resString2 = o2.ToString();
            int index1 = resString1.IndexOf('x');
            int index2 = resString2.IndexOf('x');
            if (index2 > index1)
            {
                return -1;
            }
            else if (index1 > index2)
            {
                return 1;
            }

            int width1 = int.Parse(resString1.Substring(0, index1));
            int height1 = int.Parse(resString1.Substring(index1 + 1));
            int width2 = int.Parse(resString2.Substring(0, index2));
            int height2 = int.Parse(resString2.Substring(index2 + 1));
            
            if (width1 > width2) 
            {
                return 1;
            }
            else if (width2 > width1) 
            {
                return -1;
            }

            if (height1 > height2) 
            {
                return 1;
            }
            else if (height2 > height1)
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Set the values of each component to what it is configured to.
        /// </summary>
        private void SetValues()
        {
            User32 helper = new User32();
            helper.FindValidDisplayModes();

            List<User32.DEVMODE> screens = helper.AvailScrRes;
            List<object> screenList = new List<object>(screens.Count);
            for (int i = 0; i < screens.Count; i++)
            {
                if (screens[i].dmPelsWidth < 800 || screens[i].dmPelsHeight < 600)
                {
                    // Too small: skip.
                    continue;
                }

                object format = (screens[i].dmPelsWidth + "x" + screens[i].dmPelsHeight);
                if (screenList.Contains(format)) {
                    continue;
                }

                screenList.Add(format);
            }

            screenList.Sort(SortResolutions);

            // Video options:
            resolutionBox.Items.AddRange(screenList.ToArray());
            if (screenList.Contains(options.videoOptions.Resolution))
            {
                resolutionBox.SelectedItem = options.videoOptions.Resolution;
            }
            else
            {
                resolutionBox.SelectedItem = VTankOptions.getDefaultVideoOptions().Resolution;
            }
            
            checkBox1.Checked = !Boolean.Parse(options.videoOptions.Windowed);
            textureQualityBox.SelectedItem = options.videoOptions.TextureQuality;
            antialiasingBox.SelectedItem = options.videoOptions.AntiAliasing;
            shadingSupportCheckbox.Checked = Boolean.Parse(options.videoOptions.ShadingEnabled);

            // Audio options:
            soundBar.Value = int.Parse(options.audioOptions.ambientSound.Volume);
            soundMuteCheckbox.Checked = Boolean.Parse(
                options.audioOptions.ambientSound.Muted);
            soundBar.Enabled = !soundMuteCheckbox.Checked;

            bgMusicSlider.Value = int.Parse(options.audioOptions.backgroundSound.Volume);
            bgMuteCheckbox.Checked = Boolean.Parse(
                options.audioOptions.backgroundSound.Muted);
            bgMusicSlider.Enabled = !bgMuteCheckbox.Checked;

            // Gameplay options:
            showPlayerNamesCheckbox.Checked = Boolean.Parse(
                options.gamePlayOptions.ShowNames);
            profanityFilterCheckbox.Checked = Boolean.Parse(
                options.gamePlayOptions.ProfanityFilter);
            interfacePluginBox.SelectedIndex = 0;

            // Control options:
            KeysConverter kc = new KeysConverter();
            forwardValue.Text = kc.ConvertToString(options.keyBindings.Forward);
            backwardValue.Text = kc.ConvertToString(options.keyBindings.Backward);
            rotateLeftValue.Text = kc.ConvertToString(options.keyBindings.RotateLeft);
            rotateRightValue.Text = kc.ConvertToString(options.keyBindings.RotateRight);
            //firePrimaryValue.Text = kc.ConvertToString(options.keyBindings.FirePrimary);
            //fireSecondaryValue.Text = kc.ConvertToString(options.keyBindings.FireSecondary);
            menuValue.Text = kc.ConvertToString(options.keyBindings.Menu);
            largeMapValue.Text = kc.ConvertToString(options.keyBindings.Minimap);
            scoreboardValue.Text = kc.ConvertToString(options.keyBindings.Score);
            cameraValue.Text = kc.ConvertToString(options.keyBindings.Camera);

            ImageList images = new ImageList();
            Dictionary<String, Icon> cursors = GetAvailableCursors();
            foreach (string key in cursors.Keys)
            {
                Icon icon = cursors[key];
                images.Images.Add(key, icon);
            }

            pointerBox.ImageList = images;

            int index = 0;
            foreach (string key in cursors.Keys)
            {
                pointerBox.Items.Add(new ImageComboBoxItem(key, index));
                if (options.keyBindings.Pointer == key)
                {
                    pointerBox.SelectedIndex = index;
                }
                ++index;
            }

            pointerBox.SelectedIndexChanged += new EventHandler(pointerBox_SelectedIndexChanged);
        }

        public Dictionary<String, Icon> GetAvailableCursors()
        {
            Dictionary<String, Icon> cursors = new Dictionary<string, Icon>();

            DirectoryInfo info = new DirectoryInfo(".");
            FileInfo[] files = info.GetFiles("*.png");
            for (int i = 0; i < files.Length; ++i)
            {
                if (files[i].Extension.EndsWith("png"))
                {
                    Bitmap picture = new Bitmap(files[i].FullName);
                    Icon icon = new Icon(Icon.FromHandle(picture.GetHicon()), new Size(32, 32));
                    cursors.Add(files[i].Name.Replace(files[i].Extension, ""), icon);
                }
            }

            return cursors;
        }

        public void SetChangeMade(bool changeWasMade)
        {
            changes = changeWasMade;
            applyButton.Enabled = changes;
            //button9.Enabled = changes;
        }

        // Cancel button.
        private void button10_Click(object sender, EventArgs e)
        {
            if (changes)
            {
                DialogResult result = System.Windows.Forms.MessageBox.Show(
                    this, "Exit without saving?", "Confirm exit.", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                    Environment.Exit(0);
            }
            else
            {
                // Only ask for confirmation if they have made changes.
                this.Close();
            }
        }

        // Save button.
        private void button9_Click(object sender, EventArgs e)
        {
            VTankOptions.WriteOptions(options);
            SetChangeMade(false);

            this.Close();
        }

        // Apply button.
        private void applyButton_Click(object sender, EventArgs e)
        {
            VTankOptions.WriteOptions(options);
            SetChangeMade(false);
        }

        private void resolutionBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            options.videoOptions.Resolution = resolutionBox.SelectedItem.ToString();
            SetChangeMade(true);
        }

        // Windowed mode.
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            options.videoOptions.Windowed = (!checkBox1.Checked).ToString();
            SetChangeMade(true);
        }

        private void shadingSupportCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            options.videoOptions.ShadingEnabled = shadingSupportCheckbox.Checked.ToString();
            SetChangeMade(true);
        }

        private void textureQualityBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            options.videoOptions.TextureQuality = 
                textureQualityBox.SelectedItem.ToString();
            SetChangeMade(true);
        }

        private void antialiasingBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            options.videoOptions.AntiAliasing =
                antialiasingBox.SelectedItem.ToString();
            SetChangeMade(true);
        }

        private void soundBar_Scroll(object sender, EventArgs e)
        {
            options.audioOptions.ambientSound.Volume = soundBar.Value.ToString();
            SetChangeMade(true);
        }

        private void soundMuteCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            options.audioOptions.ambientSound.Muted =
                soundMuteCheckbox.Checked.ToString();
            soundBar.Enabled = !soundMuteCheckbox.Checked;
            SetChangeMade(true);
        }

        private void bgMusicSlider_Scroll(object sender, EventArgs e)
        {
            options.audioOptions.backgroundSound.Volume = bgMusicSlider.Value.ToString();
            SetChangeMade(true);
        }

        private void bgMuteCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            options.audioOptions.backgroundSound.Muted =
                bgMuteCheckbox.Checked.ToString();
            bgMusicSlider.Enabled = !bgMuteCheckbox.Checked;
            SetChangeMade(true);
        }

        private void showPlayerNamesCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            options.gamePlayOptions.ShowNames = showPlayerNamesCheckbox.Checked.ToString();
            SetChangeMade(true);
        }

        private void profanityFilterCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            options.gamePlayOptions.ProfanityFilter = 
                profanityFilterCheckbox.Checked.ToString();
            SetChangeMade(true);
        }

        private void interfacePluginBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            options.gamePlayOptions.InterfacePlugin =
                interfacePluginBox.SelectedItem.ToString();
            SetChangeMade(true);
        }

        /// <summary>
        /// Check if the key is unique. That is, check if another key binding
        /// holds this key value already.
        /// </summary>
        /// <param name="key">Key to test.</param>
        /// <returns>True if the key is unused by other controls.</returns>
        private bool IsUniqueKey(Keys key)
        {
            return options.keyBindings.Forward != key &&
                options.keyBindings.Backward != key &&
                options.keyBindings.RotateLeft != key &&
                options.keyBindings.RotateRight != key &&
                options.keyBindings.FirePrimary != key &&
                options.keyBindings.FireSecondary != key &&
                options.keyBindings.Menu != key &&
                options.keyBindings.Minimap != key &&
                options.keyBindings.Score != key;
        }

        private void forwardCtrl_Click(object sender, EventArgs e)
        {
            KeyConfigurer kc = new KeyConfigurer();
            kc.ShowDialog(this);

            Keys key = kc.KeyCode;
            if (!kc.Canceled && key != options.keyBindings.Forward)
            {
                // Only update the key if it changed.
                if (!IsUniqueKey(key))
                {
                    MessageBox.Show(this, "This key is already bound to another control. " +
                        "Please unbind it and try again.", "Can't use that key.", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }
                
                KeysConverter converter = new KeysConverter();

                options.keyBindings.Forward = key;
                forwardValue.Text = converter.ConvertToString(key);
                SetChangeMade(true);
            }
        }

        private void backwardCtrl_Click(object sender, EventArgs e)
        {
            KeyConfigurer kc = new KeyConfigurer();
            kc.ShowDialog(this);

            Keys key = kc.KeyCode;
            if (!kc.Canceled && key != options.keyBindings.Backward)
            {
                // Only update the key if it changed.
                if (!IsUniqueKey(key))
                {
                    MessageBox.Show(this, "This key is already bound to another control. " +
                        "Please unbind it and try again.", "Can't use that key.",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                KeysConverter converter = new KeysConverter();

                options.keyBindings.Backward = key;
                backwardValue.Text = converter.ConvertToString(key);
                SetChangeMade(true);
            }
        }

        private void rotateLeftCtrl_Click(object sender, EventArgs e)
        {
            KeyConfigurer kc = new KeyConfigurer();
            kc.ShowDialog(this);

            Keys key = kc.KeyCode;
            if (!kc.Canceled && key != options.keyBindings.RotateLeft)
            {
                // Only update the key if it changed.
                if (!IsUniqueKey(key))
                {
                    MessageBox.Show(this, "This key is already bound to another control. " +
                        "Please unbind it and try again.", "Can't use that key.",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                KeysConverter converter = new KeysConverter();

                options.keyBindings.RotateLeft = key;
                rotateLeftValue.Text = converter.ConvertToString(key);
                SetChangeMade(true);
            }
        }

        private void rotateRightCtrl_Click(object sender, EventArgs e)
        {
            KeyConfigurer kc = new KeyConfigurer();
            kc.ShowDialog(this);

            Keys key = kc.KeyCode;
            if (!kc.Canceled && key != options.keyBindings.RotateRight)
            {
                // Only update the key if it changed.
                if (!IsUniqueKey(key))
                {
                    MessageBox.Show(this, "This key is already bound to another control. " +
                        "Please unbind it and try again.", "Can't use that key.",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                KeysConverter converter = new KeysConverter();

                options.keyBindings.RotateRight = key;
                rotateRightValue.Text = converter.ConvertToString(key);
                SetChangeMade(true);
            }
        }

        private void firePrimaryCtrl_Click(object sender, EventArgs e)
        {
            KeyConfigurer kc = new KeyConfigurer();
            kc.ShowDialog(this);

            Keys key = kc.KeyCode;
            if (!kc.Canceled && key != options.keyBindings.FirePrimary)
            {
                // Only update the key if it changed.
                if (!IsUniqueKey(key))
                {
                    MessageBox.Show(this, "This key is already bound to another control. " +
                        "Please unbind it and try again.", "Can't use that key.",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                KeysConverter converter = new KeysConverter();

                options.keyBindings.FirePrimary = key;
                firePrimaryValue.Text = converter.ConvertToString(key);
                SetChangeMade(true);
            }
        }

        private void fireSecondaryCtrl_Click(object sender, EventArgs e)
        {
            KeyConfigurer kc = new KeyConfigurer();
            kc.ShowDialog(this);

            Keys key = kc.KeyCode;
            if (!kc.Canceled && key != options.keyBindings.FireSecondary)
            {
                // Only update the key if it changed.
                if (!IsUniqueKey(key))
                {
                    MessageBox.Show(this, "This key is already bound to another control. " +
                        "Please unbind it and try again.", "Can't use that key.",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                KeysConverter converter = new KeysConverter();

                options.keyBindings.FireSecondary = key;
                fireSecondaryValue.Text = converter.ConvertToString(key);
                SetChangeMade(true);
            }
        }

        private void menuCtrl_Click(object sender, EventArgs e)
        {
            KeyConfigurer kc = new KeyConfigurer();
            kc.ShowDialog(this);

            Keys key = kc.KeyCode;
            if (!kc.Canceled && key != options.keyBindings.Menu)
            {
                // Only update the key if it changed.
                if (!IsUniqueKey(key))
                {
                    MessageBox.Show(this, "This key is already bound to another control. " +
                        "Please unbind it and try again.", "Can't use that key.",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                KeysConverter converter = new KeysConverter();

                options.keyBindings.Menu = key;
                menuValue.Text = converter.ConvertToString(key);
                SetChangeMade(true);
            }
        }

        private void largeMapCtrl_Click(object sender, EventArgs e)
        {
            KeyConfigurer kc = new KeyConfigurer();
            kc.ShowDialog(this);

            Keys key = kc.KeyCode;
            if (!kc.Canceled && key != options.keyBindings.Minimap)
            {
                // Only update the key if it changed.
                if (!IsUniqueKey(key))
                {
                    MessageBox.Show(this, "This key is already bound to another control. " +
                        "Please unbind it and try again.", "Can't use that key.",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                KeysConverter converter = new KeysConverter();

                options.keyBindings.Minimap = key;
                largeMapValue.Text = converter.ConvertToString(key);
                SetChangeMade(true);
            }
        }

        private void scoreboardCtrl_Click(object sender, EventArgs e)
        {
            KeyConfigurer kc = new KeyConfigurer();
            kc.ShowDialog(this);

            Keys key = kc.KeyCode;
            if (!kc.Canceled && key != options.keyBindings.Score)
            {
                // Only update the key if it changed.
                if (!IsUniqueKey(key))
                {
                    MessageBox.Show(this, "This key is already bound to another control. " +
                        "Please unbind it and try again.", "Can't use that key.",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                KeysConverter converter = new KeysConverter();

                options.keyBindings.Score = key;
                scoreboardValue.Text = converter.ConvertToString(key);
                SetChangeMade(true);
            }
        }

        private void cameraCtrl_Click(object sender, EventArgs e)
        {
            KeyConfigurer kc = new KeyConfigurer();
            kc.ShowDialog(this);

            Keys key = kc.KeyCode;
            if (!kc.Canceled && key != options.keyBindings.Camera)
            {
                // Only update the key if it changed.
                if (!IsUniqueKey(key))
                {
                    MessageBox.Show(this, "This key is already bound to another control. " +
                        "Please unbind it and try again.", "Can't use that key.",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                KeysConverter converter = new KeysConverter();

                options.keyBindings.Score = key;
                scoreboardValue.Text = converter.ConvertToString(key);
                SetChangeMade(true);
            }
        }

        void pointerBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ImageComboBoxItem item = (ImageComboBoxItem)pointerBox.SelectedItem;
            options.keyBindings.Pointer = item.Text;
            SetChangeMade(true);
        }
    }
}
