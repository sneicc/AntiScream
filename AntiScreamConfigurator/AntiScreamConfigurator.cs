using AntiScreamSaveManager;
using NAudio.CoreAudioApi;
using System.Diagnostics;

namespace AntiScreamConfigurator
{
    internal static class AntiScreamConfigurator
    {
        private static double _screamThreshold = 0.5;
        private static int _screamTimeout = 1000;
        private static string _alertMessage = "Stop screaming!";
        private static int _micID = 0;
        private static List<string> _programsToKill = new List<string>();

        [STAThread]
        static void Main()
        {
            bool exit = false;
            _programsToKill.Add("Roblox"); // Default game variant

            LoadSettings();

            while (!exit)
            {
                DrawMenu();

                var userInput = GetUserInput<int>(); ;

                switch ((MenuFunction)userInput)
                {
                    case MenuFunction.ChangeMic:
                        ChooseMic();
                        break;
                    case MenuFunction.SetScreamThreshold:
                        SetScreamThreshold();
                        break;
                    case MenuFunction.SetScreamTimeout:
                        SetScreamTimeout();
                        break;
                    case MenuFunction.SaveSettings:
                        SaveSettings();
                        break;
                    case MenuFunction.AddProgrammToKill:
                        AddProgrammToKill();
                        break;
                    case MenuFunction.RemoveProgrammToKill:
                        RemoveProgrammToKill();
                        break;
                    case MenuFunction.Start:
                        LaunchAntiScream();
                        break;
                    case MenuFunction.Stop:
                        CloseAntiScream();
                        break;
                    case MenuFunction.Exit:
                        exit = true;
                        break;
                    default:
                        Console.WriteLine();
                        Console.WriteLine("Incorrect menu input!");
                        break;
                }
            }
        }

        private static void AddProgrammToKill()
        {
            Console.WriteLine();
            Console.WriteLine("Input process name to add");

            var userInput = Console.ReadLine();
            _programsToKill.Add(userInput);
        }

        private static void RemoveProgrammToKill()
        {
            Console.WriteLine();
            Console.WriteLine("Input process name to remove");

            var userInput = Console.ReadLine();
            _programsToKill.Remove(userInput);
        }

        private static void LaunchAntiScream()
        {
            Process.Start("AntiScream");
        }

        private static void CloseAntiScream()
        {
            Process[] processes = Process.GetProcessesByName("AntiScream");
            foreach (Process process in processes)
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        private static void SetScreamThreshold()
        {
            double userInput = GetUserInput<double>();

            if (userInput > 0 && userInput <= 1)
            {
                _screamThreshold = userInput;
            }
            else
            {
                Console.WriteLine("Incorrect value!");
            }
        }

        private static void SetScreamTimeout()
        {
            int userInput = GetUserInput<int>();

            if (userInput > 0)
            {
                _screamTimeout = userInput;
            }
            else
            {
                Console.WriteLine("Incorrect value! ");
            }
        }

        private static T GetUserInput<T>() where T : struct, IConvertible
        {
            while (true)
            {
                Console.WriteLine();
                Console.Write("Enter value: ");

                string userInput = Console.ReadLine();

                try
                {
                    T convertedValue = (T)Convert.ChangeType(userInput, typeof(T));
                    return convertedValue;
                }
                catch
                {
                    Console.WriteLine("Incorrect value format!");
                }
            }
        }

        private static void LoadSettings()
        {
            DataHolder loadedData = SaveManager.Load();
            if (loadedData == null)
            {
                SaveSettings();
                return;
            }
            
            _screamThreshold = loadedData.ScreamThreshold;
            _screamTimeout = loadedData.ScreamTimeout;
            _alertMessage = loadedData.AlertMessage;
            _micID = loadedData.MicID;
            _programsToKill = loadedData.ProgramsToKill;
        }

        private static void SaveSettings()
        {
            DataHolder dataToSave = new DataHolder();

            dataToSave.ScreamThreshold = _screamThreshold;
            dataToSave.ScreamTimeout = _screamTimeout;
            dataToSave.AlertMessage = _alertMessage;
            dataToSave.MicID = _micID;
            dataToSave.ProgramsToKill = _programsToKill;

            SaveManager.Save(dataToSave);
        }

        private static void DrawMenu()
        {
            string processes = String.Join(", ", _programsToKill.ToArray());

            Console.WriteLine();
            Console.WriteLine("---------Menu--------- ");
            Console.WriteLine($"1. Set micophone ({_micID})");
            Console.WriteLine($"2. Set scream threshold ({_screamThreshold})");
            Console.WriteLine($"3. Set scream timeout ({_screamTimeout})");
            Console.WriteLine($"5. Add programm to kill ({processes})");
            Console.WriteLine("6. Remove programm to kill");
            Console.WriteLine("7. Save settings");
            Console.WriteLine("8. Start");
            Console.WriteLine("9. Stop");
            Console.WriteLine("10. Exit");
            Console.WriteLine("---------------------- ");
        }

        private static void ChooseMic()
        {
            List<string> mics = GetMicNames();
            foreach (string m in mics)
            {
                Console.WriteLine(m);
            }

            Console.Write($"Choose mic (from 0 to {mics.Count - 1}): ");
            var userInput = Console.ReadLine();
            bool isParsed = Int32.TryParse(userInput, out int micID);

            if (micID < 0 || micID >= mics.Count || isParsed == false)
            {
                Console.WriteLine("Incorrect mic ID !");
            }
            else
            {
                _micID = micID;
            }
        }

        private static List<string> GetMicNames()
        {
            List<string> mics = new List<string>();
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            foreach (var device in devices)
                mics.Add(device.FriendlyName);

            return mics;
        }

        enum MenuFunction
        {
            ChangeMic = 1,
            SetScreamThreshold = 2,
            SetScreamTimeout = 3,
            SetScreamMode = 4,
            AddProgrammToKill = 5,
            RemoveProgrammToKill = 6,
            SaveSettings = 7,
            Start = 8,
            Stop = 9,
            Exit = 10
        }
    }
}