using System;

namespace MusCat.Core.Interfaces.Audio
{
    public interface IAudioPlayer
    {
        /// <summary>
        /// 
        /// </summary>
        PlaybackState SongPlaybackState { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool IsStoppedManually { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="volume"></param>
        void SetVolume(float volume);

        /// <summary>
        /// Start playing new song
        /// </summary>
        /// <param name="location">Location of an audio file</param>
        /// <exception cref="Exception">Thrown if the song file can't be opened</exception>
        void Play(string location);

        /// <summary>
        /// Stop playing current song
        /// </summary>
        /// <param name="manualStop"></param>
        void Stop(bool manualStop = true);

        /// <summary>
        /// Pause playing song
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Resume playing song
        /// </summary>
        void Resume();

        /// <summary>
        /// Sets the current playback position in Mp3FileReader according to the percent of elapsed time
        /// </summary>
        /// <param name="percent">Percent of elapsed time (0.0-100.0)</param>
        void Seek(double percent);

        /// <summary>
        /// Evaluates the percent of elapsed time according to current playback position in Mp3FileReader
        /// </summary>
        /// <returns>The percent of elapsed time (0.0-1.0)</returns>
        double TimePercent();

        /// <summary>
        /// Stop playing current track and dispose waveOut and Mp3FileReader
        /// </summary>
        void Close();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IsStopped();
    }

    public enum PlaybackState
    {
        Play,
        Pause,
        Stop
    }
}
