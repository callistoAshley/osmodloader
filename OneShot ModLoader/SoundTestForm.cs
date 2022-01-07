using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneShot_ModLoader
{
    public class SoundTestForm : Form
    {
        public static SoundTestForm instance;

        private enum TreeViewType
        {
            Bgm,
            Sfx,
        }

        // the tree view that was clicked last
        private TreeViewType lastClicked;

        private TreeView bgm = new TreeView 
        {
            Location = new Point(50, 235),
            Size = new Size(150, 200)
        };
        private TreeView sfx = new TreeView
        {
            Location = new Point(290, 235),
            Size = new Size(150, 200)
        };

        public SoundTestForm()
        {
            instance = this;

            FormBorderStyle = FormBorderStyle.FixedSingle;
            Text = "Sound Test";
            Size = new Size(500, 550);
            BackgroundImage = Image.FromFile(Static.spritesPath + "\\bg.png");

            // image
            Controls.Add(new PictureBox
            {
                Location = new Point(95, 0),
                AutoSize = true,
                Image = Image.FromFile(Static.spritesPath + "\\st.png"),
                BackColor = Color.Transparent
            });

            // tree views
            Controls.Add(bgm);
            Controls.Add(sfx);

            // play button
            Controls.Add(new PlayButton());

            AddSounds();

            // events
            bgm.Click += new EventHandler(delegate (object sender, EventArgs e) { lastClicked = TreeViewType.Bgm; });
            sfx.Click += new EventHandler(delegate (object sender, EventArgs e) { lastClicked = TreeViewType.Sfx; });
        }

        private void AddSounds()
        {
            foreach (FileInfo f in new DirectoryInfo(Static.audioPath).GetFiles())
            {
                if (f.Name.StartsWith("bgm"))
                    bgm.Nodes.Add(f.Name.Replace(".ogg", string.Empty));
                else if (f.Name.StartsWith("sfx"))
                    sfx.Nodes.Add(f.Name.Replace(".ogg", string.Empty));
            }

            bgm.Refresh();
            sfx.Refresh();
        }

        private void Play(bool loop)
        {
            Audio.Stop();
            if (lastClicked == TreeViewType.Bgm)
                Audio.PlaySound(bgm.SelectedNode.Text, loop);
            else
                Audio.PlaySound(sfx.SelectedNode.Text, loop);

        }

        public class PlayButton : GlowButton
        {
            public PlayButton() : base("st_play")
            {
                Location = new Point(195, 440);
                AutoSize = true;
                Image = Image.FromFile($"{Static.spritesPath}{spriteName}.png");
            }

            // play
            protected override void OnClick(EventArgs e)
            {
                instance.Play(instance.lastClicked == TreeViewType.Bgm);
            }
        }
    }
}
