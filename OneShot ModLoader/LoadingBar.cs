﻿using System;
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
using OneShot_ModLoader.OCI;

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
            UpdateProgress,
            SetMaximumProgress,
            ResetProgress,
            Dispose,
            Forcequit,
            ForcequitOCI,
            ReturnToMenu,
            ReturnToSetupMenu,
        }

        public LoadingProgress progress = new LoadingProgress();
        public readonly Form form;

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
            if (this.form.InvokeRequired)
                this.form.Invoke(new Action(() => this.form.Controls.Add(text)));
            else
                this.form.Controls.Add(text);

            if (showProgressBar)
            {
                // wf loading bar
                progress.Location = new Point(0, 230);
                progress.Size = new Size(500, 20);
                progress.Style = ProgressBarStyle.Continuous;

                // yeah this is readable! don't feel like commenting it lol
                if (this.form.InvokeRequired)
                    this.form.Invoke(new Action(() => this.form.Controls.Add(progress)));
                else
                    this.form.Controls.Add(progress);
            }
        }

        // the way changes handles cross-threading stuff is by calling this method
        // which is basically some monolithic nuclear method for telling osml what to do on the main thread
        // it uses the user state passed through the "e" parameter, which is ProgressChangedEventArgs
        // and checks whether it's a string or a ProgressType enum
        // if it's a ProgressType enum, it does stuff
        // if it's a string, it sets the label text to the string
        // if it's any other type, it does nothing
        // i forgot why it's structured like an event handler but i guess that's not my problem right now :p
        public void ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is ProgressType)
            {
                switch (e.UserState)
                {
                    case ProgressType.UpdateProgress:
                        UpdateProgressBar(e.ProgressPercentage);
                        break;
                    case ProgressType.SetMaximumProgress:
                        SetMaximumProgress(e.ProgressPercentage);
                        break;
                    case ProgressType.ResetProgress:
                        if (progress.InvokeRequired)
                            progress.Invoke(new Action(() => { ResetProgress(); }));
                        else
                            ResetProgress();
                        break;
                    case ProgressType.Dispose:
                        Dispose();
                        break;
                    case ProgressType.Forcequit:
                        if (form.InvokeRequired)
                            form.Invoke(new Action(() => form.Close() ));
                        else
                            form.Close();
                        break;
                    case ProgressType.ForcequitOCI:
                        if (OCIForm.instance.InvokeRequired)
                            OCIForm.instance.Invoke(new Action(() => OCIForm.instance.Close()));
                        else
                            OCIForm.instance.Close();
                        break;
                    case ProgressType.ReturnToMenu:
                        if (MainForm.instance.InvokeRequired)
                        {
                            MainForm.instance.Invoke(new Action(() =>
                            {
                                MainForm.instance.Controls.Clear();
                                MainForm.instance.InitStartMenu();
                            }));
                        }
                        else
                        {
                            MainForm.instance.Controls.Clear();
                            MainForm.instance.InitStartMenu();
                        }
                        break;
                    case ProgressType.ReturnToSetupMenu:
                        if (MainForm.instance.InvokeRequired)
                        {
                            MainForm.instance.Invoke(new Action(() =>
                            {
                                MainForm.instance.Controls.Clear();
                                MainForm.instance.InitSetupMenu();
                            }));
                        }
                        else
                        {
                            MainForm.instance.Controls.Clear();
                            MainForm.instance.InitSetupMenu();
                        }
                        break;
                }
            }
            else
            {
                SetLoadingStatus(e.UserState.ToString());
            }
        }

        public string GetLoadingBGM() => "bgm_0" + new Random().Next(1, 6) + "";

        public void ResetProgress() => progress.Value = 0;

        public void SetMaximumProgress(int amount)
        {
            Action thingy = new Action(() => { progress.Maximum = amount; });

            if (progress.InvokeRequired)
                progress.Invoke(thingy);
            else
                thingy.Invoke();
        }

        public void UpdateProgressBar(int yo)
        {
            // you know the drill
            Action whaddayacallit = new Action(() => { if (progress.Value < progress.Maximum) progress.Value += yo; } );

            if (progress.InvokeRequired)
                progress.Invoke(whaddayacallit);
            else
                whaddayacallit.Invoke();
        }

        public void SetLoadingStatus(string status)
        {
            // create the delegate for what we're actually doing
            Action thingimabob = new Action(() =>
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

                    Logger.WriteLine(message + "\n---\n" + ex.ToString());
                }
            });

            // then we check for whether the text needs to be invoked
            if (text.InvokeRequired)
            {
                text.Invoke(thingimabob);
            }
            else
            {
                thingimabob.Invoke();
            }
        }

        public void Dispose()
        {
            text.Invoke(new Action(() => { text.Dispose(); } ));
            progress.Invoke(new Action(() => { text.Dispose(); } ));
        }

        // progress bar
        public class LoadingProgress : ProgressBar
        {
            // stole this code lol https://stackoverflow.com/questions/778678/how-to-change-the-color-of-progressbar-in-c-sharp-net-3-5
            public LoadingProgress()
            {
                SetStyle(ControlStyles.UserPaint, true);
                DoubleBuffered = true;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                // create action
                Action thing = new Action(() =>
                {
                    Rectangle rec = e.ClipRectangle;

                    rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 4; 
                    if (ProgressBarRenderer.IsSupported)
                        ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);
                    rec.Height = rec.Height - 4;
                    e.Graphics.FillRectangle(Brushes.MediumPurple, 2, 2, rec.Width, rec.Height);
                });

                // invoke on control if invoke is required
                if (InvokeRequired)
                    Invoke(thing);
                else
                    thing.Invoke(); // otherwise just invoke it directly on the same thread
            }
        }
    }
}
