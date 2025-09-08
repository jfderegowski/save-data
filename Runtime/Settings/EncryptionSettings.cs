using System;

namespace fefek5.Variables.SaveDataVariable.Runtime.Settings
{
    [Serializable]
    public class EncryptionSettings
    {
        public string Salt = "g46dzQ80";
        public string InitVector = "OFRna74m*aze01xY";
        public string Password = Guid.NewGuid().ToString();
    }
}