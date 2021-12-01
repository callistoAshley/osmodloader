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
using System.Media;
using System.IO.Compression;

namespace OneShot_ModLoader
{
    public class OCIForm : Form
    {
        public FileInfo modPath;
        public static OCIForm instance;

        public static bool ghajshdfjhjahskgkdjfahajsldkfGoodVariableName;

        public OCIForm(string[] things)
        {
            try
            {
                Logger.WriteLine("OCIForm intialized with args: ");
                foreach (string s in things) Logger.WriteLine(" " + s);
                if (!Program.doneSetup)
                {
                    Logger.WriteLine("base os not found, attempting to close oci form");
                    MessageBox.Show("A base oneshot could not be found. Please open the setup page and follow the instructions.");
                    Close();
                    return;
                }

                ghajshdfjhjahskgkdjfahajsldkfGoodVariableName = true;
                instance = this;

                modPath = new FileInfo(things[0]);

                FormBorderStyle = FormBorderStyle.FixedSingle;
                Text = "One-Click Install";
                Size = new Size(500, 400);
                SetTheme();
                Icon = new Icon(Static.spritesPath + "oci_icon.ico");

                MaximizeBox = false;
                MinimizeBox = false;
                HelpButton = true;

                Show();
                Audio.PlaySound("bgm_oci", false);

                // text
                Label text = new Label();
                text.Text = "OneShot ModLoader\nOne-Click Install\n" + modPath.Name;
                text.Location = new Point(10, 10);
                text.Font = Static.GetTerminusFont(16);
                text.AutoSize = true;
                text.ForeColor = Color.White;
                text.BackColor = Color.Transparent;

                Controls.Add(new OCIDoneButton());
                Controls.Add(new OCIDirectApply());
                Controls.Add(new OCIDeleteExisting());
                Controls.Add(text);
            }
            catch (Exception ex)
            {
                ExceptionMessage.New(ex, true, "\nOneShot ModLoader will now close.");
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Logger.ToFile();
        }

        private void SetTheme ()
        {
            int bonk = new Random().Next(1, 4);
            string themeString = "blue";

            switch (bonk)
            {
                case 1:
                    themeString = "blue";
                    break;
                case 2:
                    themeString = "green";
                    break;
                case 3:
                    themeString = "red";
                    break;
                    /*
                default:
                    themeString = "twm";
                    break;
                    */
            }

            // i have no idea if this is any better than just using a string but i'm doing it anyway
            OCILoadingBuddy.Theme theme = (OCILoadingBuddy.Theme)Enum.Parse(typeof(OCILoadingBuddy.Theme), themeString, true);

            new OCILoadingBuddy(theme);

            BackgroundImage = Image.FromFile($"{Static.spritesPath}oci_bg_{themeString}.png");
            BackgroundImageLayout = ImageLayout.Stretch;
        }
    }

    public class OCIDoneButton : Button
    {
        public OCIDoneButton()
        {
            Location = new Point(200, 300);

            Font = Static.GetTerminusFont(8);
            Text = "Ready";
        }

        protected override async void OnClick(EventArgs e)
        {
            string mod = OCIForm.instance.modPath.Name;
            OCIForm.instance.Controls.Clear();

            Audio.Stop();
            LoadingBar loadingBar = new LoadingBar(OCIForm.instance);
            loadingBar.SetLoadingStatus(string.Format("Extracting {0}, please wait...", OCIForm.instance.modPath.Name));
            Audio.PlaySound(loadingBar.GetLoadingBGM(), false);

            string zipDestination = Static.modsPath + mod;
            try
            {
                if (!Directory.Exists(zipDestination))
                {
                    await Task.Run(() =>
                    {
                        ZipFile.ExtractToDirectory(OCIForm.instance.modPath.FullName, zipDestination);
                    });
                }
            }
            catch (Exception ex)
            {
                string message = "An exception was encountered:\n---------------\n"
                    + ex.Message + "\n---------------\n" + ex.ToString();

                MessageBox.Show(message);
            }

            if (OCIDirectApply.instance.Checked)
            {
                ChangesManage.MultithreadStuff(true, new DirectoryInfo(mod), OCIDeleteExisting.instance.Checked);
            }
        }
    }

    public class OCIDirectApply : CheckBox
    {
        public static OCIDirectApply instance;

        public OCIDirectApply()
        {
            instance = this;

            Text = "Directly apply to OneShot";
            Font = Static.GetTerminusFont(12);

            Location = new Point(175, 150);
            Size = new Size(200, 50);

            ForeColor = Color.White;
            BackColor = Color.Transparent;
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            Audio.PlaySound("sfx_select", false);
            MessageBox.Show("Directly apply to OneShot\n" +
                "after extracting the .osml file to your mods directory,\n" +
                "OneShot ModLoader will directly apply the mod into OneShot.");
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            Audio.PlaySound(Checked ? "sfx_decision" : "sfx_back", false);
        }
    }

    public class OCIDeleteExisting : CheckBox
    {
        public static OCIDeleteExisting instance;

        public OCIDeleteExisting()
        {
            instance = this;

            Text = "Uninstall already activated mod(s)";
            Font = Static.GetTerminusFont(12);

            Location = new Point(175, 200);
            Size = new Size(200, 75);

            ForeColor = Color.White;
            BackColor = Color.Transparent;
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            Audio.PlaySound("sfx_select", false);
            MessageBox.Show("Uninstall already activated mod(s)\n" +
                "when \"Directly apply to OneShot\" is checked,\n" +
                "OneShot ModLoader will uninstall any already installed mods");
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            Audio.PlaySound(Checked ? "sfx_decision" : "sfx_back", false);
        }
    }
}
