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
        public bool Verify(string path)
        {
            string[] check =
            {
                "oneshot.exe",
                "_______.exe"
            };

            int amountFound = 0;
            foreach (string s in check)
                if (File.Exists(path + "/" + s)) amountFound++;

            return amountFound == check.Length;
        }

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

            await DoStuff(path);
        }

        public static bool runningSetup;
        public async Task DoStuff(string path)
        {
            runningSetup = true;

            Audio.PlaySound(LoadingBar.GetLoadingBGM(), true);
            LoadingBar loadingBar = new LoadingBar();

            if (!Verify(path)) // verify the installation
            {
                MessageBox.Show("Could not find a valid OneShot installation at that location. Please double-check for typos in your path.");
                Audio.Stop();
                Form1.instance.Controls.Clear();
                Form1.instance.InitSetupMenu();
                return;
            }

            try
            {
                Console.WriteLine("setup begin");

                await loadingBar.SetLoadingStatus("working, please wait a moment");

                // first we need to copy all of the directories
                string[] dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

                Console.WriteLine("dirs.Length {0}", dirs.Length);

                string shortDirCut = path; // shorten the directories ready to be cloned
                string[] shortDirs = dirs;
                for (int i = 0; i < shortDirs.Length; i++)
                    shortDirs[i] = shortDirs[i].Replace(shortDirCut, "");

                // create the new directory
                foreach (string s in shortDirs)
                {
                    await loadingBar.SetLoadingStatus("setup: " + s);

                    if (!Directory.Exists(Constants.modsPath + s))
                        Directory.CreateDirectory(Constants.modsPath + "base oneshot/" + s);
                }

                // finally, copy the files to the new location
                string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                string finalPath = Constants.modsPath + "base oneshot";

                Console.WriteLine("files.Length {0}", files.Length);
                for (int i = 0; i < files.Length; i++)
                {
                    string fileName = files[i].Replace(path, "");
                    File.Copy(files[i], finalPath + fileName, true);

                    await loadingBar.SetLoadingStatus("setup: " + fileName);
                }

                if (File.Exists(Constants.appDataPath + "path.molly"))
                    File.Delete(Constants.appDataPath + "path.molly");
                File.WriteAllText(Constants.appDataPath + "path.molly", path);

                await loadingBar.SetLoadingStatus("almost done!");

                Console.Beep();
                MessageBox.Show("All done!");

                Audio.Stop();

                Form1.instance.Controls.Clear();
                Form1.instance.InitStartMenu();

                runningSetup = false;
            }
            catch (Exception ex)
            {
                string message = "An exception was encountered:\n" +
                    ex.Message + "\n------------------\n" + ex.ToString() + "\nOneShot ModLoader will now close.";

                MessageBox.Show(message);
                Form1.instance.Close();
            }
        }
    }

    public class BackButton : Button
    {
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

        protected override void OnClick(EventArgs e)
        {
            Audio.PlaySound("sfx_back.mp3", false);
            Form1.instance.Controls.Clear();
            Form1.instance.InitStartMenu();
        }
    }
}
