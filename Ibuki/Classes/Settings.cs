using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IbukiBooruLibrary.Booru;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Windows.Storage;
using Ibuki.Utils;

namespace Ibuki.Classes {
    public class PasswordCredentials {
        [JsonIgnore]
        public const string SALT = "choujin-steiner--";
        public string Booru { get; set; }
        public int ID { get; set; }
        public bool IsAPIKey { get; set; }
        public bool IsActive { get; set; }
        public string Username { get; set; }
        [JsonProperty]
        public string EncryptedPassword { get; private set; }
        [JsonIgnore]
        public string Password { 
            get => StringEncryptor.Decrypt(EncryptedPassword, SALT); 
            set {
                EncryptedPassword = StringEncryptor.Encrypt(value, SALT);
            } 
        }
    }

    public static class ObservableCollectionExtensions {
        public static int IndexOf(this ObservableCollection<BooruData> collection, string displayName) {
            for (int i = 0; i < collection.Count; i++) {
                if (displayName == collection[i].DisplayName)
                    return i;
            }
            return -1;
        }
    }

    public class Settings {
        public ObservableCollection<BooruData> Boorus { get; set; }
        public List<PasswordCredentials> Credentials { get; set; }
        public int ActiveBooru { get; set; } = -1;
        //public int ActiveAccount { get; set; } = -1;
        public static Settings FromJSON(string json) {
            return JsonConvert.DeserializeObject<Settings>(json);
        }
        public static string ToJSON(Settings settings) {
            return JsonConvert.SerializeObject(settings);
        }

        public List<PasswordCredentials> GetCredentialsForBooru(string booru) {
            List<PasswordCredentials> result = new List<PasswordCredentials>();
            for (int i = 0; i < Credentials.Count; i++) {
                if (Credentials[i].Booru == booru)
                    result.Add(Credentials[i]);
            }
            return result;
        }

        public PasswordCredentials GetActiveCredentialsForBooru(string booru) {
            for (int i = 0; i < Credentials.Count; i++) {
                if (Credentials[i].Booru == booru && Credentials[i].IsActive) {
                    return Credentials[i];
                }
            }
            return null;
        }

        public PasswordCredentials GetActiveCredentialsForActiveBooru() {
            return GetActiveCredentialsForBooru(Boorus[ActiveBooru].DisplayName);
        }

        public BooruData GetActiveBooru() {
            return Boorus[ActiveBooru];
        }

        public string GetFormatedCredentialsForActiveBooru() {
            PasswordCredentials ActiveCredentials = GetActiveCredentialsForActiveBooru();
            string AuthFormatString = Boorus[ActiveBooru].AuthFormat;
            string result = "";
            if (ActiveCredentials != null && AuthFormatString != "") {
                Dictionary<string, string> parameters = new Dictionary<string, string>() {
                    { "USERNAME", ActiveCredentials.Username }
                };

                if (AuthFormatString.Contains("PASSWORD"))
                    parameters.Add("PASSWORD", ActiveCredentials.Password);
                else
                    parameters.Add("API_KEY", ActiveCredentials.Password);

                result = Helpers.FormatBooruString(AuthFormatString, parameters);
            }
            return result;
        }

        public static Settings LoadFromLocalStorage() {
            return JsonConvert.DeserializeObject<Settings>(ApplicationData.Current.LocalSettings.Values["SETTINGS_JSON"] as string);
        }

        public void SaveToLocalStorage() {
            ApplicationData.Current.LocalSettings.Values["SETTINGS_JSON"] = JsonConvert.SerializeObject(this);
        }


        public void Init() {
            Boorus = new ObservableCollection<BooruData>();
            Credentials = new List<PasswordCredentials>();

            Boorus.Add(JsonConvert.DeserializeObject<BooruData>(BooruPresets.Danbooru));
            ActiveBooru = 0;
        }
    }
}
