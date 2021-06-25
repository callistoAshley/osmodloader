using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO.Compression;

namespace OneShot_ModLoader
{
    public class DTCompressionButton : Button
    {
        public DTCompressionButton()
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

            if (browse.SelectedPath != string.Empty)
            {
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
                    Console.WriteLine(message);
                    MessageBox.Show(message);
                }
            }

            DevToolsForm.instance.Init();
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            MessageBox.Show("====Compress mod to .osml====\nCompresses a directory to a .osml file using zip compression, " +
                "which OneShot ModLoader can run and use for One-Click Install.");
        }
    }
}
