using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Threading;
using System.Windows.Input;
using OneShot_ModLoader.Backend;
using OneShot_ModLoader.DevTools;

namespace OneShot_ModLoader
{
    // class for the fancy buttons in the menu that glow when you hover over them
    public class GlowButton : PictureBox
    {
        protected string spriteName;

        protected GlowButton(string spriteName)
        {
            this.spriteName = spriteName;
        }

        protected void Glow(PictureBox picture)
        {
            // dispose the original image
            picture.Image.Dispose();
            // wow! glow!
            picture.Image = Image.FromFile($"{Static.spritesPath}{spriteName}_glow.png");
        }

        protected void GlowOut(PictureBox picture)
        {
            picture.Image.Dispose();
            picture.Image = Image.FromFile($"{Static.spritesPath}{spriteName}.png");
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Glow(this);
            Audio.PlaySound("sfx_select", false);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            GlowOut(this);
        }
    }

    public class ModsButton : GlowButton
    {
        public ModsButton() : base("button_mods")
        {
            Image button = Image.FromFile($"{Static.spritesPath}{spriteName}.png");
            Image = button;
            Size = button.Size;
            Location = new Point(30, 130);
        }

        protected override void OnClick(EventArgs e)
        {
            Audio.PlaySound("sfx_decision", false);
            MainForm.instance.Controls.Clear();
            MainForm.instance.InitModsMenu();
        }
    }

    public class SetupButton : GlowButton
    {
        public SetupButton() : base("button_setup")
        {
            Image button = Image.FromFile($"{Static.spritesPath}{spriteName}.png");
            Image = button;
            Size = button.Size;
            Location = new Point(200, 130);
        }

        protected override void OnClick(EventArgs e)
        {
            Audio.PlaySound("sfx_decision", false);
            MainForm.instance.Controls.Clear();
            MainForm.instance.InitSetupMenu();
        }
    }

    public class DevToolsButton : GlowButton
    {
        public DevToolsButton() : base("button_tools")
        {
            Image = Image.FromFile($"{Static.spritesPath}{spriteName}.png");
            Size = Image.Size;
            Location = new Point(390, 10);
        }

        protected override void OnClick(EventArgs e)
        {
            if (DevToolsForm.instance == null)
            {
                Audio.PlaySound("sfx_decision", false);
                new DevToolsForm();
            }
            else Audio.PlaySound("sfx_denied", false);
        }
    }

    public class SetupPrompt : TextBox
    {
        public static SetupPrompt instance;

        public SetupPrompt()
        {
            instance = this;

            Location = new Point(0, 100);
            Size = new Size(600, 200);
        }
    }

    public class SetupDone : Button
    {
        public SetupDone()
        {
            Enabled = true;
            Location = new Point(230, 180);
            Size = new Size(55, 50);
            Text = "Done";

            Font = Static.GetTerminusFont(8);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;
        }

        protected override void OnClick(EventArgs e)
        {
            string path = SetupPrompt.instance.Text;
            MainForm.instance.Controls.Clear();

            // initialize loading box
            PictureBox pb = new PictureBox();
            pb.Image = Image.FromFile(Static.spritesPath + "loading.png");
            pb.Size = pb.Image.Size;
            pb.Location = new Point(20, 20);
            MainForm.instance.Controls.Add(pb);

            SetupManage.ActuallyDoStuff(path);
        }
    }

    public class BackButton : Button
    {
        private bool stopAudio;

        public BackButton()
        {
            Enabled = true;
            Location = new Point(0, 230);
            Size = new Size(65, 50);
            Text = "Back";

            Font = Static.GetTerminusFont(8);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;
        }

        public BackButton(bool audioStop)
        {
            Enabled = true;
            Location = new Point(0, 230);
            Size = new Size(65, 50);
            Text = "Back";

            Font = Static.GetTerminusFont(8);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;

            stopAudio = audioStop;
        }

        protected override void OnClick(EventArgs e)
        {
            Audio.PlaySound("sfx_back", false);
            MainForm.instance.Controls.Clear();
            MainForm.instance.InitStartMenu();

            if (stopAudio) Audio.Stop();
        }
    }
    public class AddToList : Button
    {
        public static AddToList instance;

        public AddToList()
        {
            instance = this;

            Enabled = true;
            Location = new Point(125, 230);
            Size = new Size(50, 50);
            Text = "Add to List";
            
            Font = Static.GetTerminusFont(8);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Audio.PlaySound("sfx_select", false);
        }

        protected override void OnClick(EventArgs e)
        {
            if (InactiveMods.instance.SelectedNode != null)
            {
                TreeNode node = InactiveMods.instance.SelectedNode;
                ActiveMods.instance.ActivateMod(node.Text);
                InactiveMods.instance.Nodes.Remove(node);

                Audio.PlaySound("sfx_decision", false);
            }
        }
    }
    public class RemoveFromList : Button
    {
        public static RemoveFromList instance;
        public RemoveFromList()
        {
            instance = this;

            Enabled = true;
            Location = new Point(230, 230);
            Size = new Size(55, 50);
            Text = "Remove from List";

            Font = Static.GetTerminusFont(8);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Audio.PlaySound("sfx_select", false);
        }

        protected override void OnClick(EventArgs e)
        {
            if (ActiveMods.instance.SelectedNode != null && ActiveMods.instance.SelectedNode.Text != "base oneshot")
            {
                TreeNode node = ActiveMods.instance.SelectedNode;

                InactiveMods.instance.Nodes.Add((TreeNode)node.Clone());
                ActiveMods.instance.Nodes.Remove(node);

                Audio.PlaySound("sfx_back", false);
            }
        }
    }
    public class ApplyChanges : Button
    {
        public static ApplyChanges instance;
        public ApplyChanges()
        {
            instance = this;

            Enabled = true;
            Location = new Point(335, 230);
            Size = new Size(65, 50);
            Text = "Apply\nChanges";

            Font = Static.GetTerminusFont(8);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Audio.PlaySound("sfx_select", false);
        }

        protected override async void OnClick(EventArgs e)
        {
            MainForm.instance.Controls.Clear();

            // initialize loading box
            PictureBox pb = new PictureBox();
            pb.Image = Image.FromFile(Static.spritesPath + "loading.png");
            pb.Size = pb.Image.Size;
            pb.Location = new Point(20, 20);
            MainForm.instance.Controls.Add(pb);

            await Task.Delay(1);

            try // why is this in a try catch i can't remember
            {
                ChangesManage.MultithreadStuff(false, new LoadingBar(MainForm.instance));
            }
            catch { }

            //Form1.instance.Controls.Clear();
            //Form1.instance.InitStartMenu();
        }
    }
}
