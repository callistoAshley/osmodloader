using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using NAudio;
using NAudio.Wave;
using NAudio.Vorbis;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using OneShot_ModLoader.Backend;

namespace OneShot_ModLoader
{
    public static class Audio
    {
        private static List<WaveOutEvent> activeAudio = new List<WaveOutEvent>();

        public static async void PlaySound(string sound, bool loop)
        {
            Logger.WriteLine($"attempting to play sound: {sound}");

            try
            {
                // gc isn't happy rn
                // create wave out event and initialize it with the vorbis wave reader
                using (WaveOutEvent waveOut = new WaveOutEvent())
                {
                    // create vorbis wave reader
                    using (VorbisWaveReader v = new VorbisWaveReader($"{Static.audioPath}\\{sound}.ogg"))
                    {
                        // also create a loop stream and initialize the wave out event with the loop stream instead of loop is true
                        using (LoopStream loopStream = new LoopStream(v))
                        {
                            if (loop)
                                waveOut.Init(loopStream);
                            else
                                waveOut.Init(v);

                            // flush and dispose the streams after playback stops
                            void Dispose(object sender, StoppedEventArgs e)
                            {
                                v.Flush();
                                v.Dispose();
                                waveOut.Dispose();
                                loopStream.Flush();
                                loopStream.Dispose();
                            }
                            waveOut.PlaybackStopped += Dispose;

                            // play
                            waveOut.Play();

                            // add the wave out event to the active audio list so it can be stopped manually
                            activeAudio.Add(waveOut);

                            // wait the duration of the sound
                            await Task.Delay(v.TotalTime);
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                ExceptionMessage.New(ex, true);
            }
        }

        public static void Stop()
        {
            // dispose the fields of each file
            foreach (WaveOutEvent w in activeAudio)
            {
                // stop the playback and dispose
                w.Stop();
                w.Dispose();
            }
            activeAudio.Clear();
        }
    }
}