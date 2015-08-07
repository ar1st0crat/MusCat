using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusCatalog
{
    enum PlaybackState
    {
        PLAY,
        PAUSE,
        STOP
    }


    /// <summary>
    /// Simple mp3 player based on NAudio.WaveOut
    /// </summary>
    class MusCatPlayer
    {
        // NAudio component for a song playback
        WaveOut waveOut = new WaveOut();
        
        // Attached mp3 file reader
        Mp3FileReader mp3Reader = null;

        //
        public PlaybackState SongPlaybackState
        {
            get; set;
        }

        //
        public MusCatPlayer()
        {
            SongPlaybackState = PlaybackState.STOP;
        }
        
        //      
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
        public void Play(string filename, EventHandler<StoppedEventArgs> PlaybackStopped)  
        {
            if (waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                waveOut.Stop();
            }
            
            waveOut.Dispose();
            waveOut = null;

            if (mp3Reader != null)
            {
                mp3Reader.Dispose();
                mp3Reader = null;
            }

            waveOut = new WaveOut();

            // here we may face some problems depending on particular mp3 files or organization of user's file system
            // (exception is possible)
            mp3Reader = new Mp3FileReader( filename );        
            waveOut.Init( mp3Reader );

            waveOut.PlaybackStopped += PlaybackStopped;
            waveOut.Play();
                
            SongPlaybackState = PlaybackState.PLAY;
        }

        public void Stop()
        {
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

        public void Seek( double percent )
        {
            if (mp3Reader == null)
                return;

            double totalSeconds = mp3Reader.TotalTime.TotalSeconds * percent;
            TimeSpan seekPos = new TimeSpan( (int)totalSeconds / 3600, (int)totalSeconds / 60, (int)totalSeconds % 60 );
            mp3Reader.CurrentTime = seekPos;
        }
    }
}
