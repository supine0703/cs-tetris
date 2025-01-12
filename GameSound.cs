using System.Diagnostics;
using System.Reflection;
using NAudio.Wave;

namespace Tetris
{
    internal partial class GameControl
    {
        private class Mp3Sound
        {
            private readonly WaveStream waveStream;
            private readonly WaveOutEvent waveOut;
            private bool _playing = false;
            public bool Playing => _playing && waveOut.PlaybackState == PlaybackState.Playing;
            public bool AutoReplay { get; set; } = false;

            public Mp3Sound(byte[] audioData)
            {
                waveStream = new Mp3FileReader(new MemoryStream(audioData));
                waveOut = new();
                waveOut.Init(waveStream);
                waveOut.PlaybackStopped += (sender, e) =>
                {
                    if (AutoReplay && _playing) { RePlay(); }
                    _playing = waveOut.PlaybackState == PlaybackState.Playing;
                };
            }

            public void Stop() { waveOut.Stop(); _playing = false; }
            public void Play() { waveOut.Play(); _playing = true; }

            public void RePlay()
            {
                if (waveOut.PlaybackState == PlaybackState.Playing) { waveOut.Stop(); }
                waveStream.Position = 0;
                Play();
            }

            public void Dispose()
            {
                Stop();
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
            { "ReadyGo",    "ready_set_go-1" },
            { "GameOver",   "game_over" },
        };

        private readonly Dictionary<string, Mp3Sound?> _sounds = [];
        private Mp3Sound? _silentAudio; // For keeping the audio device alive
        private Mp3Sound? _backgroundMusic;
        private bool BgmMute { get; set; } = true;

        private void InitializeSounds()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            foreach (var (key, value) in _soundResources)
            {
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    _sounds[key] = LoadSoundResource(assembly, resourceNames, value);
                }
            }

            _backgroundMusic = LoadSoundResource(assembly, resourceNames, "Korobeiniki(Tetris Theme)-Kutay.mp3");
            _backgroundMusic.AutoReplay = true;
            BgmMute = !BgmMute;

            _silentAudio = LoadSoundResource(assembly, resourceNames, "silent_audio.mp3");
            _silentAudio.AutoReplay = true;
            _silentAudio.Play();

            // Load sound resources
            Mp3Sound LoadSoundResource(Assembly asm, string[] resNames, string resName)
            {
                var name = resNames.FirstOrDefault(n => n.Contains(resName))
                    ?? throw new Exception($"Cannot find the resource: {resName}");

                using var stream = asm.GetManifestResourceStream(name)
                    ?? throw new Exception($"Failed to load the sound resource: {name}");

                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);

                return new Mp3Sound(data);
            }
        }

        private void BgmMuteSwitch()
        {
            BgmMute = !BgmMute;
            if (BgmMute) { _backgroundMusic?.Stop(); }
            else { _backgroundMusic?.Play(); }
        }

        private void DisposeSounds()
        {
            _sounds.Values.ToList().ForEach(sound => sound?.Dispose());
            _sounds.Clear();
            _silentAudio?.Dispose();
            _backgroundMusic?.Dispose();
        }
    }
}
