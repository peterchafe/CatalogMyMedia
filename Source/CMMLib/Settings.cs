using System.Text.Json;
using System.Text.Json.Serialization;

namespace CMMLib
{
    public class Settings
    {
        public Settings()
        {
            CameraName = string.Empty;
            WorkingFolder = string.Empty;
        }

        public string CameraName { get; set; }

        public string WorkingFolder { get; set; }

        public uint Count { get; set; }

        public void AddRecord()
        {
            Count++;
            Save();
        }

        public Settings WithCameraName(string cameraName)
        {
            CameraName = cameraName;
            Save();
            return this;
        }

        public Settings WithWorkingFolder(string workingFolder)
        {
            WorkingFolder = workingFolder;
            return this;
        }    

        public void Save()
        {
            File.WriteAllText("settings.cfg", JsonSerializer.Serialize<Settings>(this, new JsonSerializerOptions() { WriteIndented = true }));
        }

        public static Settings Load()
        {
            if (File.Exists("settings.cfg") == false)
            {
                return new Settings();
            }

            return JsonSerializer.Deserialize<Settings>(File.ReadAllText("settings.cfg"), new JsonSerializerOptions() { WriteIndented = true }) ?? new Settings();
        }
    }
}
