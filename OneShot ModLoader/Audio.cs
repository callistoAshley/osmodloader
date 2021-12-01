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

namespace OneShot_ModLoader
{
    public static class Audio
    {
        private static List<WaveOutEvent> activeAudio = new List<WaveOutEvent>();

        public static async void PlaySound(string sound, bool loop)
        {
            Console.WriteLine($"attempting to play sound: {sound}");

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
                // stop the playback
                // this will also invoke the playback stopped event i think 
                // i don't really know github is down right now so i can't check the source code
                w.Stop();
            activeAudio.Clear();
        }

        private struct AudioFile
        {
            private AudioFileReaderWrapped a;
            private LoopStream loopStream;
            private WaveOutEvent waveOut;

            // initialize a structure called AudioFile that contains an AudioFileReader, LoopStream and WaveOutEvent
            // that can each be disposed when playback stops
            public AudioFile(AudioFileReaderWrapped a, bool loop)
            {
                this.a = a;
                loopStream = new LoopStream(this.a);
                waveOut = new WaveOutEvent();

                if (loop) 
                    waveOut.Init(loopStream);
                else
                {
                    waveOut.Init(a);
                    waveOut.PlaybackStopped += DisposeStuff;
                }

                waveOut.Play();
            }

            public void DisposeStuff(object sender, StoppedEventArgs e)
            {
                waveOut.Dispose();
                a.Dispose();
                loopStream.Dispose();
                waveOut = null;
                a = null;
                loopStream = null;
            }
        }

        private class AudioFileReaderWrapped : AudioFileReader, IDisposable
        {
            // just call base ctor
            public AudioFileReaderWrapped(string fileName)
                : base(fileName) { }

            public new void Dispose() => Dispose(true);
        }
    }
}