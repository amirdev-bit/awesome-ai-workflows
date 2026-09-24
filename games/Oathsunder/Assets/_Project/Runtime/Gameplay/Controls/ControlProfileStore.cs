using System;
using System.IO;
using Oathsunder.Combat.Definitions;
using Oathsunder.Controls;
using Oathsunder.Core.Serialization;
using UnityEngine;

namespace Oathsunder.Gameplay.Controls
{
    /// <summary>
    /// Persists control profiles as JSON under <c>Application.persistentDataPath/controls/</c>. Phase 10's save
    /// system syncs this folder to cloud saves. A corrupt file never blocks the game: it is backed up and the
    /// defaults are used.
    /// </summary>
    public static class ControlProfileStore
    {
        private static string Folder => Path.Combine(Application.persistentDataPath, "controls");

        private static string PathFor(int slot) => Path.Combine(Folder, $"controls.p{slot}.json");

        /// <summary>Loads a slot, falling back to defaults (Simplified on touch devices, Classic elsewhere).</summary>
        public static ControlProfile Load(int slot)
        {
            string path = PathFor(slot);
            if (File.Exists(path))
            {
                try
                {
                    return ControlProfile.FromJson(File.ReadAllText(path));
                }
                catch (Exception exception) when (exception is JsonSyntaxException || exception is JsonContentException || exception is IOException)
                {
                    Debug.LogError($"[Controls] Profile {path} is unreadable ({exception.Message}); using defaults.");
                    TryBackup(path);
                }
            }

            var scheme = Application.isMobilePlatform ? ControlScheme.Simplified : ControlScheme.Classic;
            var profile = ControlProfile.CreateDefault(scheme);
            profile.TouchLayoutId = IsTablet() ? TouchLayout.TabletId : TouchLayout.PhoneId;
            return profile;
        }

        /// <summary>Writes a slot atomically (temp file + replace).</summary>
        public static void Save(int slot, ControlProfile profile)
        {
            Directory.CreateDirectory(Folder);
            string path = PathFor(slot);
            string temp = path + ".tmp";
            File.WriteAllText(temp, profile.ToJson());
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Move(temp, path);
        }

        private static void TryBackup(string path)
        {
            try
            {
                File.Copy(path, path + ".corrupt", overwrite: true);
            }
            catch (IOException)
            {
                // Best effort only.
            }
        }

        private static bool IsTablet()
        {
            float dpi = Screen.dpi > 0 ? Screen.dpi : 160f;
            float diagonalInches = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height) / dpi;
            return diagonalInches >= 7.5f;
        }
    }
}
