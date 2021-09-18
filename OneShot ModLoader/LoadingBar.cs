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
using System.ComponentModel;

namespace OneShot_ModLoader
{
    // TODO: make this nicer please
    public class LoadingBar : IDisposable
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

        public enum ProgressType
        {
            ResetProgress,
            Forcequit,
        }

        public LoadingProgress progress = new LoadingProgress();
        private Form form;

        public LoadingBar(Form form, LoadingBarType displayType = LoadingBarType.Efficient, bool showProgressBar = true)
        {
            text.ForeColor = Color.MediumPurple;
            text.Location = new Point(0, 190);
            text.AutoSize = true;

            text.Font = Static.GetTerminusFont(10);
            text.ForeColor = Color.MediumPurple;
            text.BackColor = Color.Transparent;

            this.displayType = displayType;

            this.form = form;
            this.form.Controls.Add(text);

            if (showProgressBar)
            {
                // wf loading bar
                progress.Location = new Point(0, 230);
                progress.Size = new Size(500, 20);
                progress.Style = ProgressBarStyle.Continuous;
                form.Controls.Add(progress);
            }
        }

        public void ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            UpdateProgressBar(e.ProgressPercentage);

            if (e.UserState is ProgressType)
            {
                switch (e.UserState)
                {
                    case ProgressType.ResetProgress:
                        ResetProgress();
                        break;
                    case ProgressType.Forcequit:
                        form.Close();
                        break;
                }
            }
            else
            {
                SetLoadingStatus(e.UserState.ToString());
            }
        }

        public string GetLoadingBGM() => "bgm_0" + new Random().Next(1, 6) + ".mp3";

        public void ResetProgress() => progress.Value = 0;

        public void UpdateProgressBar(int yo)
        {
            if (progress.Value < progress.Maximum) progress.Value += yo;
        }

        public void SetLoadingStatus(string status)
        {
            try
            {
                string finalStatus = status;

                // replace the working directory or oneshot path with an empty string to shorten the status
                if (finalStatus.Contains(Directory.GetCurrentDirectory()))
                    finalStatus = finalStatus.Replace(Directory.GetCurrentDirectory(), string.Empty);
                else if (finalStatus.Contains(Static.baseOneShotPath))
                    finalStatus = finalStatus.Replace(Static.baseOneShotPath, string.Empty);

                // set the status
                text.Text = finalStatus;
                text.Refresh();
            }
            catch (Exception ex)
            {
                text.Font = new Font(new FontFamily(GenericFontFamilies.Monospace), 10);
                string message = "exception encountered in loading bar: " + ex.Message;

                Console.WriteLine(message + "\n---\n" + ex.ToString());
            }
        }

        public void Dispose()
        {
            text.Dispose();
            progress.Dispose();
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
                e.Graphics.FillRectangle(Brushes.MediumPurple, 2, 2, rec.Width, rec.Height);
            }
        }
    }
}
