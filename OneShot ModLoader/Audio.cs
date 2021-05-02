using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using NAudio;
using NAudio.Wave;
using System.Windows.Forms;
using System.Threading;

namespace OneShot_ModLoader
{
    public class Audio
    {
        public static List<WaveOutEvent> activeWaveOuts = new List<WaveOutEvent>();
        public static void PlaySound(string sound, bool loop) // this used to use the System.Media.SoundPlayer class, which could loop tracks
        {
            Console.WriteLine("attempting to play sound: " + sound);

            try
            {
                AudioFileReader a = new AudioFileReader(Constants.audioPath + sound);
                LoopStream l = new LoopStream(a);

                WaveOutEvent waveOut = new WaveOutEvent();

                if (loop)
                    waveOut.Init(l);
                else
                    waveOut.Init(a);
                waveOut.Play();

                activeWaveOuts.Add(waveOut);
            }
            catch (Exception ex)
            {
                string message = "An exception was encountered in the music player:\n---------------\n"
                    + ex.Message + "\n---------------\n" + ex.ToString();

                MessageBox.Show(message);
            }
        }

        public static void Stop()
        {
            foreach (WaveOutEvent w in activeWaveOuts) w.Stop();
            activeWaveOuts.Clear();
        }
    }
}
