using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            public Sound(byte[] audioData)
            {
                waveStream = new Mp3FileReader(new MemoryStream(audioData));
                waveOut = new();
                waveOut.Init(waveStream);
            }

            public void Play()
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                }
                waveStream.Position = 0;
                waveOut.Play();
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
                stream.Close();
                _sounds[key] = new(data);
            }
        }

        private void DisposeSounds()
        {
            foreach (var sound in _sounds.Values)
            {
                sound?.Dispose();
            }
            _sounds.Clear();
        }
    }
}
