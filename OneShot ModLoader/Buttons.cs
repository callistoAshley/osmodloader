using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;
using System.IO;

namespace OneShot_ModLoader
{
    public class ButtonsGlobalStuff
    {
        public static void Glow (PictureBox picture, string name)
        {
            Image glow = Image.FromFile(Constants.spritesPath + name + "_glow.png");
            picture.Image = glow;
        }
        public static void GlowOut (PictureBox picture, string name)
        {
            picture.Image = Image.FromFile(Constants.spritesPath + name + ".png");
        }
    }

    public class ModsButton : PictureBox
    {
        public ModsButton()
        {
            Image button = Image.FromFile(Constants.spritesPath + "button_mods.png");
            Image = button;
            Size = button.Size;
            Location = new Point(30, 130);
            Form1.instance.Controls.Add(this);
        }

        protected override void OnMouseEnter(EventArgs e) 
        {
            ButtonsGlobalStuff.Glow(this, "button_mods");
            Audio.PlaySound("sfx_select.mp3", false); 
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            ButtonsGlobalStuff.GlowOut(this, "button_mods");
        }

        protected override void OnClick(EventArgs e)
        {
            Audio.PlaySound("sfx_decision.mp3", false);
            Form1.instance.Controls.Clear();
            Form1.instance.InitModsMenu();
        }
    }

    public class BrowseMods : PictureBox
    {
        public BrowseMods()
        {
            Image button = Image.FromFile(Constants.spritesPath + "button_browse.png");
            Image = button;
            Size = button.Size;
            Location = new Point(200, 130);
            Form1.instance.Controls.Add(this);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            ButtonsGlobalStuff.Glow(this, "button_browse");
            Audio.PlaySound("sfx_select.mp3", false);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            ButtonsGlobalStuff.GlowOut(this, "button_browse");
        }

        protected override void OnClick(EventArgs e)
        {
            Audio.PlaySound("sfx_decision.mp3", false);
            
            MessageBox.Show("browse mods button also this isn't done yet");
        }
    }

    public class Setup : PictureBox
    {
        public Setup()
        {
            Image button = Image.FromFile(Constants.spritesPath + "button_setup.png");
            Image = button;
            Size = button.Size;
            Location = new Point(200, 130);//new Point(370, 130); use this when browse mods is added
            Form1.instance.Controls.Add(this);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            ButtonsGlobalStuff.Glow(this, "button_setup");
            Audio.PlaySound("sfx_select.mp3", false);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            ButtonsGlobalStuff.GlowOut(this, "button_setup");
        }

        protected override void OnClick(EventArgs e)
        {
            Audio.PlaySound("sfx_decision.mp3", false);
            Form1.instance.Controls.Clear();
            Form1.instance.InitSetupMenu();
        }
    }

    public class DevToolsButton : PictureBox
    {
        public DevToolsButton()
        {
            Image = Image.FromFile(Constants.spritesPath + "button_tools.png");
            Size = Image.Size;
            Location = new Point(390, 10);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            ButtonsGlobalStuff.Glow(this, "button_tools");
            Audio.PlaySound("sfx_select.mp3", false);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            ButtonsGlobalStuff.GlowOut(this, "button_tools");
        }

        protected override void OnClick(EventArgs e)
        {
            if (DevToolsForm.instance == null)
            {
                Audio.PlaySound("sfx_decision.mp3", false);
                new DevToolsForm();
            }
            else Audio.PlaySound("sfx_denied.mp3", false);
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

            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(Constants.fontsPath + "TerminusTTF-Bold.ttf");
            Font = new Font(f.Families[0], 8, FontStyle.Bold);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;
        }

        protected override async void OnClick(EventArgs e)
        {
            string path = SetupPrompt.instance.Text;
            Form1.instance.Controls.Clear();

            // initialize loading box
            PictureBox pb = new PictureBox();
            pb.Image = Image.FromFile(Constants.spritesPath + "loading.png");
            pb.Size = pb.Image.Size;
            pb.Location = new Point(20, 20);
            Form1.instance.Controls.Add(pb);

            await SetupManage.DoStuff(path);
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

            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(Constants.fontsPath + "TerminusTTF-Bold.ttf");
            Font = new Font(f.Families[0], 8, FontStyle.Bold);

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

            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(Constants.fontsPath + "TerminusTTF-Bold.ttf");
            Font = new Font(f.Families[0], 8, FontStyle.Bold);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;

            stopAudio = audioStop;
        }

        protected override void OnClick(EventArgs e)
        {
            Audio.PlaySound("sfx_back.mp3", false);
            Form1.instance.Controls.Clear();
            Form1.instance.InitStartMenu();

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

            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(Constants.fontsPath + "TerminusTTF-Bold.ttf");
            Font = new Font(f.Families[0], 8, FontStyle.Bold);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Audio.PlaySound("sfx_select.mp3", false);
        }

        protected override void OnClick(EventArgs e)
        {
            if (InactiveMods.instance.SelectedNode != null)
            {
                TreeNode node = InactiveMods.instance.SelectedNode;
                ActiveMods.instance.ActivateMod(node.Text);
                InactiveMods.instance.Nodes.Remove(node);

                Audio.PlaySound("sfx_decision.mp3", false);
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

            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(Constants.fontsPath + "TerminusTTF-Bold.ttf");
            Font = new Font(f.Families[0], 8, FontStyle.Bold);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Audio.PlaySound("sfx_select.mp3", false);
        }

        protected override void OnClick(EventArgs e)
        {
            if (ActiveMods.instance.SelectedNode != null && ActiveMods.instance.SelectedNode.Text != "base oneshot")
            {
                TreeNode node = ActiveMods.instance.SelectedNode;

                InactiveMods.instance.Nodes.Add((TreeNode)node.Clone());
                ActiveMods.instance.Nodes.Remove(node);

                Audio.PlaySound("sfx_back.mp3", false);
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

            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(Constants.fontsPath + "TerminusTTF-Bold.ttf");
            Font = new Font(f.Families[0], 8, FontStyle.Bold);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderColor = Color.MediumPurple;
            FlatAppearance.BorderSize = 3;
            ForeColor = Color.MediumPurple;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Audio.PlaySound("sfx_select.mp3", false);
        }

        protected override async void OnClick(EventArgs e)
        {
            Form1.instance.Controls.Clear();

            // initialize loading box
            PictureBox pb = new PictureBox();
            pb.Image = Image.FromFile(Constants.spritesPath + "loading.png");
            pb.Size = pb.Image.Size;
            pb.Location = new Point(20, 20);
            Form1.instance.Controls.Add(pb);

            await Task.Delay(1);
            try { await ChangesManage.Apply(new LoadingBar(Form1.instance)); }
            catch { }
            Form1.instance.Controls.Clear();
            Form1.instance.InitStartMenu();
        }
    }

    public class RefreshMods : Button
    {
        public static RefreshMods instance;
        public RefreshMods()
        {
            instance = this;

            Enabled = true;
            Location = new Point(5, 100);
            Size = new Size(60, 50);
            Text = "Refresh Mods";
        }

        protected override async void OnClick(EventArgs e)
        {
            await InactiveMods.instance.RefreshMods();
        }
    }

    public class CloverSecret : PictureBox
    {
        public CloverSecret()
        {
            Image = Image.FromFile(Constants.spritesPath + "clover.png");
            Size = Image.Size;
            Location = new Point(500, 290);
            Enabled = true;
            BackColor = Color.Transparent;
        }

        protected override void OnClick(EventArgs e)
        {
            Form1.instance.Controls.Clear();

            Label text = new Label();
            text.Text = "Perhaps you should return here later.\nYou'll know when you need to.";
            text.TextAlign = ContentAlignment.MiddleCenter;
            text.Location = new Point(120, 140);
            text.BackColor = Color.Transparent;
            text.ForeColor = Color.MediumPurple;

            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(Constants.fontsPath + "TerminusTTF-Bold.ttf");
            text.Font = new Font(f.Families[0], 12, FontStyle.Bold);
            text.AutoSize = true;
            text.Enabled = true;

            Form1.instance.Controls.Add(text);
            Form1.instance.Controls.Add(new BackButton(true));

            Audio.PlaySound("bgm_countdown.mp3", false);
        }
    }
}
