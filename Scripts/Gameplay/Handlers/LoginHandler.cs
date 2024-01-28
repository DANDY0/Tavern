using System.IO;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class LoginHandler
    {
        private readonly INetworkManager _networkManager;
        private readonly string _loginCodeFileName = "LoginCode.txt";
        private readonly string _defaultLoginCode = "qwerty123";
        private readonly string _loginCodePath;
        
        public LoginHandler(INetworkManager networkManager)
        {
            _networkManager = networkManager;
            _loginCodePath = $"{Application.dataPath}{_loginCodeFileName}";
            CreateLoginCodeFile();
        }

        private void CreateLoginCodeFile()
        {
#if DEBUGMODE
            if (!File.Exists(_loginCodePath)) 
                File.WriteAllText(_loginCodePath, _defaultLoginCode);
#endif
            if (!File.Exists(_loginCodePath)) 
                File.WriteAllText(_loginCodePath, _networkManager.UPID);
        }

        public string ReadLoginCodeFromFile()
        {
            if (File.Exists(_loginCodePath))
            {
                string loginCode = File.ReadAllText(_loginCodePath).Trim();
                if (!string.IsNullOrEmpty(loginCode))
                    return loginCode;
            }
            return null;
        }

    }
}