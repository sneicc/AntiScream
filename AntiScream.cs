using AntiScreamSaveManager;
using System.Diagnostics;
using System.Timers;

namespace AntiScream
{
    internal static class AntiScream
    {
        private static Task _messageTask;
        private static System.Timers.Timer _timer;
        private static double _screamThreshold = 0.5;
        private static int _screamTimeout = 1000;
        private static string _alertMessage = "Stop screaming!";
        private static int _micID = 0;
        private static List<string> _programsToKill = new List<string>();

        [STAThread]
        static async Task Main()
        {
            bool isCreatedNewMutex;
            using (Mutex mutex = new Mutex(true, "AntiScream", out isCreatedNewMutex))
            {
                if(!isCreatedNewMutex)
                {
                    return;
                }

                ApplicationConfiguration.Initialize();
                _programsToKill.Add("Roblox"); // Default game variant

                LoadSettings();

                _messageTask = new Task(() => ShowAlert(_alertMessage));
                _timer = new System.Timers.Timer(_screamTimeout);
                _timer.Elapsed += KillGames;

                StartListenMic(WaveIn_DataAvailable);

                await Task.Delay(Timeout.Infinite);
            }          
        }

        private static void StartListenMic(Action<object?, NAudio.Wave.WaveInEventArgs> waveIn_DataAvailable)
        {
            var waveIn = new NAudio.Wave.WaveInEvent
            {
                DeviceNumber = _micID,
                WaveFormat = new NAudio.Wave.WaveFormat(rate: 8000, bits: 16, channels: 1),
                BufferMilliseconds = 5
            };

            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.StartRecording();
        }

        static void WaveIn_DataAvailable(object? sender, NAudio.Wave.WaveInEventArgs e)
        {
            Int16[] values = new Int16[e.Buffer.Length];
            Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);

            float loudness = (float)values.Max() / Int16.MaxValue;

            HandleAlert(loudness);
            HandleDelayedGamesExit(loudness);
        }

        private static void HandleDelayedGamesExit(float loudness)
        {
            if (loudness > _screamThreshold)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }

        private static void HandleAlert(float loudness)
        {
            if (loudness > _screamThreshold && _messageTask.Status != TaskStatus.Running)
            {
                if (_messageTask.Status == TaskStatus.RanToCompletion || _messageTask.Status == TaskStatus.Faulted)
                {
                    _messageTask = new Task(() => ShowAlert(_alertMessage));
                }
                _messageTask.Start();
            }
        }

        private static void ShowAlert(string alert)
        {
            MessageBox.Show(alert, "BAN", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }

        private static void KillGames(object? sender, ElapsedEventArgs e)
        {
            foreach (var programm in _programsToKill)
            {
                Process[] processes = Process.GetProcessesByName(programm);
                foreach (Process process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }

            MessageBox.Show("hahaha");
        }

        private static void LoadSettings()
        {
            DataHolder loadedData = SaveManager.Load();
            if (loadedData == null)
            {
                return;
            }

            _screamThreshold = loadedData.ScreamThreshold;
            _screamTimeout = loadedData.ScreamTimeout;
            _alertMessage = loadedData.AlertMessage;
            _micID = loadedData.MicID;
            _programsToKill = loadedData.ProgramsToKill;
        }
    }
}