using System.IO;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class LoadObjectsManager : IService, ILoadObjectsManager
    {
        public void Deinit()
        {
          
        }

        public void Dispose()
        {
          
        }

        public void Init()
        {

        }

        public void Update()
        {
            
        }

        public T GetObjectByPath<T>(string path) where T : Object
        {
            return LoadFromResources<T>(path);
        }

        public string GetTextByPath(string path)
        {
            return File.ReadAllText(path);
        }

        public void SetTextByPath(string path, string data)
        {
            File.WriteAllText(path, data);
        }

        private T LoadFromResources<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }
    }
}