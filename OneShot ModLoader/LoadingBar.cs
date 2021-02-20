using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;
using System.Threading;
using System.IO;

namespace OneShot_ModLoader
{
    public class LoadingBar
    {
        public Label text = new Label();

        public LoadingBar()
        {
            text.ForeColor = Color.MediumPurple;
            text.Location = new Point(0, 190);
            text.AutoSize = true;

            PrivateFontCollection f = new PrivateFontCollection();
            f.AddFontFile(Constants.fontsPath + "TerminusTTF-Bold.ttf");
            text.Font = new Font(f.Families[0], 10);
            text.ForeColor = Color.MediumPurple;

            Form1.instance.Controls.Add(text);
        }

        public static string GetLoadingBGM()
        {
            return "bgm_0" + new Random().Next(1, 4) + ".mp3";
        }

        public async Task SetLoadingStatus(string status)
        {
            try
            {
                string finalStatus = status;

                // replace the working directory or oneshot path with an empty string to shorten the status
                if (finalStatus.Contains(Directory.GetCurrentDirectory()))
                    finalStatus = finalStatus.Replace(Directory.GetCurrentDirectory(), string.Empty);
                else if (finalStatus.Contains(Form1.baseOneShotPath))
                    finalStatus = finalStatus.Replace(Form1.baseOneShotPath, string.Empty);

                // set the status
                text.Text = finalStatus;
            }
            catch (Exception ex)
            {
                string message = "exception encountered in loading bar: " + ex.Message;
                text.Text = message;

                Console.WriteLine(message + "\n---\n" + ex.ToString());
            }
            
            await Task.Delay(1);
        }
    }
}
