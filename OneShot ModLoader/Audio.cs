using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using NAudio;
using NAudio.Wave;
using System.Windows.Forms;

namespace OneShot_ModLoader
{
    public class Audio
    {
        public static WaveOutEvent activeWaveOut;
        public static void PlaySound(string sound, bool loop) // this used to use the System.Media.SoundPlayer class, which could loop tracks
        {
            Console.WriteLine("attempting to play sound: " + sound);

            try
            {
                AudioFileReader a = new AudioFileReader(Constants.audioPath + sound);
                WaveOutEvent waveOut = new WaveOutEvent();

                waveOut.Init(a);
                waveOut.Play();

                activeWaveOut = waveOut;
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
            activeWaveOut.Stop();
        }
    }
}
