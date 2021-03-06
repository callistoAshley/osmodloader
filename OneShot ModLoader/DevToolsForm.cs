using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace OneShot_ModLoader
{
    public class DevToolsForm : Form
    {
        public static DevToolsForm instance;

        public DevToolsForm ()
        {
            instance = this;

            Size = new Size(300, 500);
            Text = "Dev Tools";
            BackgroundImage = Image.FromFile(Constants.spritesPath + "terminal.png");
            BackgroundImageLayout = ImageLayout.Center;
            BackColor = Color.Black;

            FormBorderStyle = FormBorderStyle.FixedSingle;
            Show();

            Init();
        }

        public void Init()
        {
            Controls.Add(new OSMLCompressionButton());
        }

        protected override void OnClosed(EventArgs e)
        {
            instance = null;
        }
    }

    public class OSMLCompressionButton : Button
    {
        public OSMLCompressionButton()
        {
            Location = new Point(10, 10);
            Size = new Size(150, 20);
            BackColor = Color.White;
            Text = "Compress mod to .osml";

            Focus();
        }

        protected override async void OnClick(EventArgs e)
        {
            DevToolsForm.instance.Controls.Clear();

            FolderBrowserDialog browse = new FolderBrowserDialog();
            browse.Description = "Please navigate to your mod's path.";
            browse.ShowDialog();

            try
            {
                LoadingBar loadingBar = new LoadingBar(DevToolsForm.instance);
                Audio.PlaySound(loadingBar.GetLoadingBGM(), false);
                await loadingBar.SetLoadingStatus("Please wait a moment...");

                await Task.Run(() =>
                {
                    ZipFile.CreateFromDirectory(browse.SelectedPath, browse.SelectedPath + ".osml");
                });

                Console.Beep();
                MessageBox.Show("All done!");

                loadingBar.text.Dispose();
                Audio.Stop();
            }
            catch (Exception ex)
            {
                string message = "An exception was encountered:\n" + ex.Message +
                    "\n------------------\n" + ex.ToString();
                MessageBox.Show(message);
            }

            DevToolsForm.instance.Init();
        }
    }
}
