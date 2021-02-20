using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Input;

namespace OneShot_ModLoader
{
    public class Textbox
    {
        public Image textbox = Image.FromFile(Directory.GetCurrentDirectory() + "/Sprites/textbox.png");
        public PictureBox pictureBox = new PictureBox();
        public Label text = new Label();
        private void AddPBControl() { Form1.instance.Controls.Add(pictureBox); } 

        public Textbox(List<string> inputTexts, Size size)
        {
            pictureBox.Image = textbox;
            pictureBox.Size = size;
            pictureBox.Enabled = true;
            AddPBControl();

            Text(inputTexts);
        }

        public Textbox(List<string> inputTexts)
        {
            pictureBox.Image = textbox;
            pictureBox.Size = textbox.Size;
            pictureBox.Enabled = true;
            AddPBControl();

            Text(inputTexts);
        }

        // buttons constructor
        public Textbox(List<Button> inputButtons)
        {
            pictureBox.Image = textbox;
            pictureBox.Enabled = true;

            // define size
            Size size = new Size(0, 0);
            foreach (Button b in inputButtons)
            {
                size.Height += b.Height;
                pictureBox.Controls.Add(b);
            }
            pictureBox.Size = size;
        }

        public void Text (List<string> texts)
        {
            pictureBox.Controls.Add(text);
            text.Size = pictureBox.Size;
            text.BackColor = Color.Transparent;
            
            for (int i = 0; i < texts.Count; i++)
            {
                text.Text = "";
                for (int ii = 0; ii < texts[i].Length; ii++)
                {
                    text.Text = texts[i].Substring(0, ii);
                    Thread.Sleep(10);
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        text.Text = texts[i];
                        break;
                    }
                }
                Thread.Sleep(10);
                //while (Mouse.LeftButton != MouseButtonState.Pressed) { }
            }
            
            pictureBox.Controls.Remove(text);
        }
    }

    public class TestButton : Button
    {
        public TestButton()
        {
            Text = "hi";
        }
        protected override void OnClick(EventArgs e)
        {
            new Textbox(new List<string>
            {
                "hey look it's",
                "text in a textbox",
                "just kinda chillin"
            });
        }
    }
}
