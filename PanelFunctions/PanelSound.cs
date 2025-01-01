using System.Reflection;
using NAudio.Wave;

namespace Tetris
{
    internal partial class Panel
    {
        private class Sound
        {
            private readonly WaveStream waveStream;
            private readonly WaveOutEvent waveOut;
            private bool _playing = false;
            public bool Playing => _playing;

            public Sound(byte[] audioData)
            {
                waveStream = new Mp3FileReader(new MemoryStream(audioData));
                waveOut = new();
                waveOut.Init(waveStream);
            }

            public Sound(byte[] audioData, Action action) : this(audioData)
            {
                waveOut.PlaybackStopped += (sender, e) => action();
            }

            public void Stop() { waveOut.Stop(); _playing = false; }
            public void Play() { waveOut.Play(); _playing = true; }

            public void RePlay()
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                }
                waveStream.Position = 0;
                Play();
            }

            public void Dispose()
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveStream.Dispose();
            }
        }

        private readonly Dictionary<string, string> _soundResources = new() {
            { "Rotate",     "tap-1" },
            { "Move",       "tap-1" },
            { "Hold",       "pop-1" },
            { "Lock",       "pop-2" },
            { "Fall",       "fall_to_the_ground-1" },
            { "ClearLine",  "linging-1" },
            { "GameOver",   "game_over" },
        };

        private readonly Dictionary<string, Sound?> _sounds = [];
        private Sound? _backgroundMusic;
        private bool BgmMute { get; set; } = false;

        private void InitializeSounds()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            foreach (var (key, value) in _soundResources)
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                {
                    continue;
                }
                // Find the sound resource in the resources
                var name = resourceNames.FirstOrDefault(n => n.Contains(value))
                    ?? throw new Exception($"Cannot find the {value} sound resource");
                using var stream = assembly.GetManifestResourceStream(name)
                    ?? throw new Exception($"Failed to load the sound resource: {name}");
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                _sounds[key] = new(data);
            }

            // Background music
            var bgmName = resourceNames.FirstOrDefault(n => n.Contains("Korobeiniki(Tetris Theme)-Kutay.mp3"))
                ?? throw new Exception("Cannot find the resource: Korobeiniki(Tetris Theme)-Kutay.mp3");
            using var bgmStream = assembly.GetManifestResourceStream(bgmName)
                ?? throw new Exception("Failed to load the sound resource: Korobeiniki(Tetris Theme)-Kutay.mp3");
            var bgmData = new byte[bgmStream.Length];
            bgmStream.Read(bgmData, 0, bgmData.Length);
            _backgroundMusic = new(bgmData, () =>
            {
                if (_backgroundMusic is not null && _backgroundMusic.Playing)
                {
                    _backgroundMusic.RePlay();
                }
            });
            _backgroundMusic?.RePlay();
        }

        private void BgmMuteSwitch()
        {
            BgmMute = !BgmMute;
            if (BgmMute)
            {
                _backgroundMusic?.Stop();
            }
            else
            {
                _backgroundMusic?.Play();
            }
        }

        private void DisposeSounds()
        {
            foreach (var sound in _sounds.Values)
            {
                sound?.Dispose();
            }
            _sounds.Clear();
            _backgroundMusic?.Dispose();
        }
    }
}
