using NAudio.FileFormats.Wav;
using NAudio.Wave;
using System;
using System.IO;

namespace ParquetViewer.Helpers;

internal class AudioPlayer : IDisposable
{
    private const string INVALID_AUDIO_ERROR_MESSAGE = "Provided data was not a valid .mp3 or .wav file.";

    private readonly MemoryStream _inputStream;
    private readonly WaveStream? _audioStream;
    private TimeSpan? _postStopSeekLocation;

    public AudioFormatType AudioFormat { get; private set; }
    public event EventHandler? PlaybackStopped;
    public WaveFormat WaveFormat => this._audioStream?.WaveFormat ?? throw new InvalidDataException(INVALID_AUDIO_ERROR_MESSAGE);

    public TimeSpan CurrentTime
    {
        get
        {
            return _audioStream?.CurrentTime ?? throw new InvalidDataException(INVALID_AUDIO_ERROR_MESSAGE);
        }
        set
        {
            if (this._audioStream is null)
                throw new InvalidDataException(INVALID_AUDIO_ERROR_MESSAGE);

            var audioPlayer = GetOrCreateAudioPlayer();
            if (audioPlayer is null)
                return;

            if (audioPlayer.PlaybackState == PlaybackState.Stopped)
            {
                this._postStopSeekLocation = null;
                this._audioStream.CurrentTime = value;
            }
            else
            {
                //Stopping is recommended before seeking.
                //The OnStopped event will set the CurrentTime
                this._postStopSeekLocation = value;
                this.Stop();
            }
        }
    }
    public TimeSpan TotalTime => _audioStream?.TotalTime ?? throw new InvalidDataException(INVALID_AUDIO_ERROR_MESSAGE);

    public AudioPlayer(byte[] data)
    {
        this._inputStream = new MemoryStream(data);

        try
        {
            this.AudioFormat = AudioFormatType.Wav;
            this._audioStream = new WaveFileReader(this._inputStream);
        }
        catch
        {
            try
            {
                this.AudioFormat = AudioFormatType.Mp3;
                this._audioStream = new Mp3FileReader(this._inputStream);
            }
            catch
            {
                this.AudioFormat = AudioFormatType.Invalid;
                this._audioStream = null;
            }
        }
    }

    public void Pause()
    {
        var audioPlayer = GetOrCreateAudioPlayer();
        audioPlayer?.Pause();
    }

    public void Play()
    {
        var audioPlayer = GetOrCreateAudioPlayer();
        audioPlayer?.Play();
    }

    public void Stop()
    {
        var audioPlayer = GetOrCreateAudioPlayer();
        if (audioPlayer is null)
            return;

        if (audioPlayer.PlaybackState != PlaybackState.Stopped)
        {
            audioPlayer.Stop(); //Triggers playback stopped event
        }
        else
        {
            //If we're already stopped, trigger the playback stopped event ourselves
            this.OnPlaybackStopped_Internal(null, new StoppedEventArgs());
        }
    }

    private WaveOutEvent? _audioPlayer;
    private WaveOutEvent? GetOrCreateAudioPlayer()
    {
        if (this._audioStream is null)
            return null;

        if (this._audioPlayer == null)
        {
            this._audioPlayer = new WaveOutEvent();
            this._audioPlayer.Init(this._audioStream);
            this._audioPlayer.PlaybackStopped += OnPlaybackStopped_Internal;
        }
        return this._audioPlayer;
    }

    private void OnPlaybackStopped_Internal(object? source, StoppedEventArgs args)
    {
        if (this._audioStream is null)
            return;

        this._audioStream.Position = 0;
        if (this._postStopSeekLocation is not null)
        {
            this._audioStream.CurrentTime = this._postStopSeekLocation.Value;
            this._postStopSeekLocation = null;
        }

        PlaybackStopped?.Invoke(source, args);
    }

    public void Dispose()
    {
        this._audioPlayer.DisposeSafely();
        this._audioStream.DisposeSafely();
        this._inputStream.DisposeSafely();
    }

    public enum AudioFormatType
    {
        Invalid,
        Wav,
        Mp3,
    }

    public static bool IsAudio(byte[] data, out AudioFormatType audioFormat)
    {
        using var ms = new MemoryStream(data);
        try
        {
            var wavReader = new WaveFileChunkReader();
            wavReader.ReadWaveHeader(ms);
            audioFormat = AudioFormatType.Wav;
            return true;
        }
        catch
        {
            try
            {
                using var mp3Reader = new Mp3FileReaderBase(ms, Mp3FileReader.CreateAcmFrameDecompressor);
                audioFormat = AudioFormatType.Mp3;
                return true;

            }
            catch
            {
                audioFormat = AudioFormatType.Invalid;
                return false;
            }
        }
    }
}