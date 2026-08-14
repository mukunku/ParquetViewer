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
    public WaveFormat WaveFormat => _audioStream?.WaveFormat ?? throw new InvalidDataException(INVALID_AUDIO_ERROR_MESSAGE);

    public TimeSpan CurrentTime
    {
        get
        {
            return _audioStream?.CurrentTime ?? throw new InvalidDataException(INVALID_AUDIO_ERROR_MESSAGE);
        }
        set
        {
            if (_audioStream is null)
                throw new InvalidDataException(INVALID_AUDIO_ERROR_MESSAGE);

            var audioPlayer = GetOrCreateAudioPlayer();
            if (audioPlayer is null)
                return;

            if (audioPlayer.PlaybackState == PlaybackState.Stopped)
            {
                _postStopSeekLocation = null;
                _audioStream.CurrentTime = value;
            }
            else
            {
                //Stopping is recommended before seeking.
                //The OnStopped event will set the CurrentTime
                _postStopSeekLocation = value;
                Stop();
            }
        }
    }
    public TimeSpan TotalTime => _audioStream?.TotalTime ?? throw new InvalidDataException(INVALID_AUDIO_ERROR_MESSAGE);

    public AudioPlayer(byte[] data)
    {
        _inputStream = new MemoryStream(data);

        try
        {
            AudioFormat = AudioFormatType.Wav;
            _audioStream = new WaveFileReader(_inputStream);
        }
        catch
        {
            try
            {
                AudioFormat = AudioFormatType.Mp3;
                _audioStream = new Mp3FileReader(_inputStream);
            }
            catch
            {
                AudioFormat = AudioFormatType.Invalid;
                _audioStream = null;
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
            OnPlaybackStopped_Internal(null, new StoppedEventArgs());
        }
    }

    private WaveOutEvent? _audioPlayer;
    private WaveOutEvent? GetOrCreateAudioPlayer()
    {
        if (_audioStream is null)
            return null;

        if (_audioPlayer is null)
        {
            _audioPlayer = new WaveOutEvent();
            _audioPlayer.Init(_audioStream);
            _audioPlayer.PlaybackStopped += OnPlaybackStopped_Internal;
        }
        return _audioPlayer;
    }

    private void OnPlaybackStopped_Internal(object? source, StoppedEventArgs args)
    {
        if (_audioStream is null)
            return;

        _audioStream.Position = 0;
        if (_postStopSeekLocation is not null)
        {
            _audioStream.CurrentTime = _postStopSeekLocation.Value;
            _postStopSeekLocation = null;
        }

        PlaybackStopped?.Invoke(source, args);
    }

    public void Dispose()
    {
        _audioPlayer.DisposeSafely();
        _audioStream.DisposeSafely();
        _inputStream.DisposeSafely();
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