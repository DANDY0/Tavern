using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern.Helpers
{
    public static class InternalTools
    {
        private static string LINE_BREAK = "%n%";

        public static float DeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            return diagonalInches;
        }

        public static bool IsTabletScreen()
        {
#if FORCE_TABLET_UI
            return true;
#elif FORCE_PHONE_UI
            return false;
#else
            return DeviceDiagonalSizeInInches() > 6.5f;
#endif
        }

        public static Sequence DoActionDelayed(TweenCallback action, float delay = 0f)
        {
            if (action == null)
                return null;

            Sequence sequence = DOTween.Sequence();
            sequence.PrependInterval(delay);
            sequence.AppendCallback(action);

            return sequence;
        }

        public static string FormatStringToPascaleCase(string root)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(root.ToLower().Replace("_", " ")).Replace(" ", string.Empty);
        }

        public static void HapticVibration(int level = 0)
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Handheld.Vibrate();
#endif
        }

        public static Rect GetScreenCoordinates(RectTransform uiElement, GameObject canvas)
        {
            RectTransform canvasTransf = canvas.GetComponent<RectTransform>();

            Vector2 canvasSize = new Vector2(canvasTransf.rect.width, canvasTransf.rect.height);
            float koefX = Screen.width / canvasSize.x;
            float koefY = Screen.height / canvasSize.y;
            Vector2 position = Vector2.Scale(uiElement.anchorMin, canvasSize);
            float directionX = uiElement.pivot.x * -1;
            float directionY = uiElement.pivot.y * -1;

            var result = new Rect();
            result.width = uiElement.sizeDelta.x * koefX;
            result.height = uiElement.sizeDelta.y * koefX;
            result.x = position.x * koefX + uiElement.anchoredPosition.x * koefX + result.width * directionX;
            result.y = position.y * koefY + uiElement.anchoredPosition.y * koefX + result.height * directionY;
            return result;
        }

        public static T EnumFromString<T>(string value) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        public static void MoveToEndOfList<T>(IList<T> list, int index)
        {
            T item = list[index];
            list.RemoveAt(index);
            list.Add(item);
        }


        public static string ReplaceLineBreaks(string data)
        {
            if (data == null)
                return "";

            return data.Replace(LINE_BREAK, "\n");
        }

        public static void ShuffleList<T>(this IList<T> list)
        {
            System.Random rnd = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static List<T> GetRandomElements<T>(this IList<T> list, int count)
        {
            List<T> shuffledList = new List<T>(count);
            shuffledList.AddRange(list);

            if (list.Count <= count)
                return shuffledList;

            ShuffleList(shuffledList);
            return shuffledList.GetRange(0, count);
        }

        public static float LerpByStep(float current, float end, float step, bool applyDeltaTime = true)
        {
            if (applyDeltaTime)
            {
                step *= Time.deltaTime;
            }

            if (current > end)
            {
                step *= -1f;
            }

            return Mathf.Clamp(current + step, Mathf.Min(current, end), Mathf.Max(current, end)); 
        }

        public static T GetInstance<T>(string name, params object[] args)
        {
            return (T)Activator.CreateInstance(Type.GetType(name), args);
        }

        public static Tweener DOTextInt(this Text text, int initialValue, int finalValue, float duration)
        {
            return DOTween.To(
                 () => initialValue,
                 it =>
                 {
                     if (text != null && text) text.text = it.ToString();
                 },
                 finalValue,
                 duration
             );
        }

        public static Tweener DOTextInt(this TextMeshProUGUI text, int initialValue, int finalValue, float duration)
        {
            return DOTween.To(
                 () => initialValue,
                 it => 
                 { 
                     if (text != null && text) text.text = it.ToString(); 
                 },
                 finalValue,
                 duration
             );
        }

        public static List<T> ParseCSV<T>(string data)
        {
            CSVMap myMap = new CSVMap();
            myMap.defineColumns(typeof(T));
            ArrayList itemList = myMap.loadCsvFromString(data);
            return itemList.Cast<T>().ToList();
        }

        public static List<T> ParseCSVFromResources<T>(string path)
        {
            CSVMap myMap = new CSVMap();
            myMap.defineColumns(typeof(T));
            ArrayList itemList = myMap.loadCsvFromFile(path);
            return itemList.Cast<T>().ToList();
        }

        public static void ExportCSV<T>(List<T> genericList, string finalPath)
        {
            var sb = new StringBuilder();
            var header = string.Empty;
            var info = typeof(T).GetFields();

            if (!File.Exists(finalPath))
            {
                var file = File.Create(finalPath);
                file.Close();
                foreach (var prop in info)
                {
                    header += prop.Name + "; ";
                }
                header = header.Substring(0, header.Length - 2);
                sb.AppendLine(header);
                TextWriter sw = new StreamWriter(finalPath, true);
                sw.Write(sb.ToString());
                sw.Close();
            }

            foreach (var obj in genericList)
            {
                sb = new StringBuilder();
                var line = string.Empty;
                foreach (var prop in info)
                {
                    line += prop.GetValue(obj) + "; ";
                }
                line = line.Substring(0, line.Length - 2);
                sb.AppendLine(line);
                TextWriter sw = new StreamWriter(finalPath, true);
                sw.Write(sb.ToString());
                sw.Close();
            }
        }
    }
}
