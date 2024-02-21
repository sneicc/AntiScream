using System.Text.Json;

namespace AntiScreamSaveManager
{
    public static class SaveManager
    {
        public static void Save(DataHolder dataHolder)
        {

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AntiScreamSave.json");

            try
            {
                string storedData = JsonSerializer.Serialize(dataHolder);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(storedData);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static DataHolder Load()
        {
            string fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AntiScreamSave.json");

            DataHolder? loadedData = null;
            if (File.Exists(fullPath))
            {
                try
                {
                    string dataToLoad = string.Empty;
                    using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            dataToLoad = sr.ReadToEnd();
                        }
                    }

                    loadedData = JsonSerializer.Deserialize<DataHolder>(dataToLoad);
                }
                catch (Exception e)
                {
                    throw;
                }
            }

            return loadedData;
        }
    }

    public class DataHolder
    {
        public double ScreamThreshold { get; set; }
        public int ScreamTimeout { get; set; }
        public string AlertMessage { get; set; }
        public int MicID { get; set; }
        public List<string>? ProgramsToKill { get; set; }

        public DataHolder()
        {
            ScreamThreshold = 0;
            ScreamTimeout = 0;
            AlertMessage = "";
            MicID = 0;
            ProgramsToKill = null;
        }
    }
}
