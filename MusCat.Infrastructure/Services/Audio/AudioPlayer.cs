using System;
using MusCat.Core.Interfaces.Audio;
using NAudio.Wave;
using PlaybackState = MusCat.Core.Interfaces.Audio.PlaybackState;

namespace MusCat.Infrastructure.Services.Audio
{
    /// <summary>
    /// Simple mp3 player wrapped around NAudio.WaveOut and NAudio.Mp3FileReader
    /// </summary>
    public class AudioPlayer : IAudioPlayer
    {
        /// <summary>
        /// NAudio component for a song playback
        /// </summary>
        WaveOut _waveOut;

        /// <summary>
        /// Underlying mp3 file reader
        /// </summary>
        Mp3FileReader _mp3Reader;

        public PlaybackState SongPlaybackState { get; set; }
        public bool IsStoppedManually { get; set; }
        
        public AudioPlayer()
        {
            SongPlaybackState = PlaybackState.Stop;
            IsStoppedManually = false;
        }

        public void SetVolume(float volume)
        {
            _waveOut.Volume = volume;
        }

        /// <summary>
        /// Start playing new song
        /// </summary>
        /// <param name="filename">Location of an audio file</param>
        /// <exception cref="Exception">Thrown if an mp3 file can't be opened</exception>
        public void Play(string filename)
        {
            // if some track is currently playing, then stop it and dispose waveOut and mp3FileReader
            Close();
            
            _waveOut = new WaveOut();

            // here we may face some problems depending on particular mp3 file (exception is possible)
            _mp3Reader = new Mp3FileReader(filename);
            _waveOut.Init(_mp3Reader);
            _waveOut.Play();

            // update player state
            SongPlaybackState = PlaybackState.Play;
            IsStoppedManually = false;
        }

        public void Stop(bool manualStop = true)
        {
            IsStoppedManually = manualStop;
            _waveOut?.Stop();
            SongPlaybackState = PlaybackState.Stop;
        }

        public void Pause()
        {
            _waveOut.Pause();
            SongPlaybackState = PlaybackState.Pause;
        }

        public void Resume()
        {
            _waveOut.Resume();
            SongPlaybackState = PlaybackState.Play;
        }

        /// <summary>
        /// Sets the current playback position in Mp3FileReader according to the percent of elapsed time
        /// </summary>
        /// <param name="percent">Percent of elapsed time (0.0-100.0)</param>
        public void Seek(double percent)
        {
            if (_mp3Reader == null)
            {
                return;
            }

            var totalSeconds = _mp3Reader.TotalTime.TotalSeconds * percent;
            var seekPos = TimeSpan.FromSeconds(totalSeconds);
            _mp3Reader.CurrentTime = seekPos;
        }

        /// <summary>
        /// Returns elapsed time in seconds
        /// </summary>
        public double PlayedTime
        {
            get
            {
                if (_mp3Reader == null)
                {
                    return 0.0;
                }

                return _mp3Reader.CurrentTime.TotalSeconds;
            }
        }

        /// <summary>
        /// Evaluates the percent of elapsed time according to current playback position in Mp3FileReader
        /// </summary>
        /// <returns>The percent of elapsed time (0.0-1.0)</returns>
        public double PlayedTimePercent
        {
            get
            {
                if (_mp3Reader == null)
                {
                    return 0.0;
                }

                return _mp3Reader.CurrentTime.TotalSeconds / _mp3Reader.TotalTime.TotalSeconds;
            }
        }

        /// <summary>
        /// Stop playing current track and dispose waveOut and Mp3FileReader
        /// </summary>
        public void Close()
        {
            if (_waveOut == null)
            {
                return;
            }

            if (_waveOut.PlaybackState != NAudio.Wave.PlaybackState.Stopped)
            {
                Stop();
            }

            _waveOut.Dispose();
            _waveOut = null;
                
            if (_mp3Reader != null)
            {
                _mp3Reader.Dispose();
                _mp3Reader = null;
            }
        }

        public bool IsStopped => 
            _waveOut == null || _waveOut.PlaybackState == NAudio.Wave.PlaybackState.Stopped;
    }
}
