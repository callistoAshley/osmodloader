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

        #region loading bar type enum
        public enum LoadingBarType
        {
            Detailed, // significantly slower, primarily for debug purposes
            Efficient, // around 40 seconds faster than detailed, doesn't show individual files
            Disabled // completely disabled
        }
        public LoadingBarType displayType;
        #endregion

        public LoadingProgress progress = new LoadingProgress();

        public LoadingBar(Form form, LoadingBarType type = LoadingBarType.Efficient)
        {
            text.ForeColor = Color.MediumPurple;
            text.Location = new Point(0, 190);
            text.AutoSize = true;

            text.Font = Constants.GetTerminusFont(10);
            text.ForeColor = Color.MediumPurple;
            text.BackColor = Color.Transparent;

            displayType = type;

            form.Controls.Add(text);

            // wf loading bar
            progress.Location = new Point(0, 230);
            progress.Size = new Size(500, 20);
            progress.Style = ProgressBarStyle.Continuous;
            form.Controls.Add(progress);
        }

        public string GetLoadingBGM()
        {
            return "bgm_0" + new Random().Next(1, 6) + ".mp3";
        }

        public void ResetProgress() => progress.Value = 0;

        public async Task UpdateProgress()
        {
            if (progress.Value < progress.Maximum) progress.Value++;
            await Task.Delay(0);
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
                text.Font = new Font(new FontFamily(GenericFontFamilies.Monospace), 10);
                string message = "exception encountered in loading bar: " + ex.Message;

                Console.WriteLine(message + "\n---\n" + ex.ToString());
            }
            
            await Task.Delay(
                displayType == LoadingBarType.Disabled ? 0 : 1 // if the loading bar is completely disabled, don't await
                );
        }

        // progress bar
        public class LoadingProgress : ProgressBar
        {
            // stole this code lol https://stackoverflow.com/questions/778678/how-to-change-the-color-of-progressbar-in-c-sharp-net-3-5
            public LoadingProgress()
            {
                SetStyle(ControlStyles.UserPaint, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Rectangle rec = e.ClipRectangle;

                rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 4;
                if (ProgressBarRenderer.IsSupported)
                    ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);
                rec.Height = rec.Height - 4;
                e.Graphics.FillRectangle(Brushes.MediumPurple, 0, 0, rec.Width, rec.Height);
            }
        }
    }
}
