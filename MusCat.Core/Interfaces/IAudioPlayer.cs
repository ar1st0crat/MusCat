namespace MusCat.Core.Interfaces
{
    public interface IAudioPlayer
    {
        void Play(string location);
        void Stop(bool manualStop = true);
        void Pause();
        void Resume();
        void Seek(double percent);
        double TimePercent { get; }

        void SetVolume(float volume);

        PlaybackState SongPlaybackState { get; set; }
        bool IsStopped { get; }
        bool IsStoppedManually { get; set; }
        
        void Close();
    }

    public enum PlaybackState
    {
        Play,
        Pause,
        Stop
    }
}
