using NAudio.Wave;
using System;
using System.Threading;

namespace MusCatalog
{
    enum PlaybackState
    {
        PLAY,
        PAUSE,
        STOP
    }

    /// <summary>
    /// Simple mp3 player wrapped around NAudio.WaveOut and NAudio.Mp3FileReader
    /// </summary>
    class AudioPlayer
    {
        // NAudio component for a song playback
        WaveOut waveOut = null;
        // Underlying mp3 file reader
        Mp3FileReader mp3Reader = null;

        public PlaybackState SongPlaybackState { get; set; }
        public bool IsManualStop { get; set; }
        
        public AudioPlayer()
        {
            SongPlaybackState = PlaybackState.STOP;
            IsManualStop = false;
        }

        public void SetVolume(float vol)
        {
            waveOut.Volume = vol;
        }

        /// <summary>
        /// Start playing new song
        /// </summary>
        /// <param name="filename">Location of an audio file</param>
        /// <param name="PlaybackStopped">Stop playback event handler</param>
        /// <exception cref="Exception">Thrown if an mp3 file can't be opened</exception>
        public void Play(string filename, EventHandler<StoppedEventArgs> PlaybackStopped = null)
        {
            if (waveOut != null)
            {
                Freeze();
            }
            
            waveOut = new WaveOut();

            // here we may face some problems depending on particular mp3 file (exception is possible)
            mp3Reader = new Mp3FileReader(filename);
            waveOut.Init(mp3Reader);
            waveOut.PlaybackStopped += PlaybackStopped;
            waveOut.Play();

            SongPlaybackState = PlaybackState.PLAY;
            IsManualStop = false;
        }

        public void Stop( bool bManual = true )
        {
            IsManualStop = bManual;
            waveOut.Stop();
            SongPlaybackState = PlaybackState.STOP;
        }

        public void Pause()
        {
            waveOut.Pause();
            SongPlaybackState = PlaybackState.PAUSE;
        }

        public void Resume()
        {
            waveOut.Resume();
            SongPlaybackState = PlaybackState.PLAY;
        }

        /// <summary>
        /// Sets the current playback position in Mp3FileReader according to the percent of elapsed time
        /// </summary>
        /// <param name="percent">Percent of elapsed time (0.0-100.0)</param>
        public void Seek(double percent)
        {
            if (mp3Reader == null)
            {
                return;
            }

            double totalSeconds = mp3Reader.TotalTime.TotalSeconds * percent;
            TimeSpan seekPos = new TimeSpan((int)totalSeconds / 3600, (int)totalSeconds / 60, (int)totalSeconds % 60);
            mp3Reader.CurrentTime = seekPos;
        }

        /// <summary>
        /// Evaluates the percent of elapsed time according to current playback position in Mp3FileReader
        /// </summary>
        /// <returns>The percent of elapsed time (0.0-100.0)</returns>
        public double TimePercent()
        {
            if (mp3Reader == null)
            {
                return 0.0;
            }

            return mp3Reader.CurrentTime.TotalSeconds / mp3Reader.TotalTime.TotalSeconds;
        }

        /// <summary>
        /// Dispose waveOut and Mp3FileReader
        /// </summary>
        public void Freeze()
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState != NAudio.Wave.PlaybackState.Stopped)
                {
                    Stop();
                }

                waveOut.Dispose();
                waveOut = null;
                
                if (mp3Reader != null)
                {
                    mp3Reader.Dispose();
                    mp3Reader = null;
                }
            }
        }
    }
}
