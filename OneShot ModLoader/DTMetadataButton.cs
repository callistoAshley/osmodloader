﻿using System;
using System.Windows.Forms;
using System.Drawing;

namespace OneShot_ModLoader
{
    public class DTMetadataButton : Button
    {
        public DTMetadataButton()
        {
            Location = new Point(10, 35);
            Size = new Size(150, 20);
            BackColor = Color.White;
            Text = "Generate mod metadata";

            Focus();
        }

        protected override void OnClick(EventArgs e)
        {
            DevToolsForm.instance.Controls.Clear();

            FolderBrowserDialog browse = new FolderBrowserDialog();
            browse.Description = "Please navigate to your mod's path.";
            browse.ShowDialog();

            if (browse.SelectedPath != string.Empty)
            {
                try
                {
                    new MMDForm(browse.SelectedPath);
                }
                catch (Exception ex)
                {
                    string message = "An exception was encountered:\n" + ex.Message +
                        "\n------------------\n" + ex.ToString();
                    Console.WriteLine(message);
                    MessageBox.Show(message);
                }
            }

            DevToolsForm.instance.Init();
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            MessageBox.Show("====Generate mod metadata====\nCurrently obsolete. Will be used in a future update.");
        }
    }
}
