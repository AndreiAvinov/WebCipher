using Microsoft.AspNetCore.Http;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebCipher.Models
{
    public class CipherModel
    {
        [JsonIgnore]
        public IFormFile InputFile { get; set; }
        public string FileEncoding { get; set; } = "auto";
        public string Direction { get; set; } = "Cipher";
        public string InputText { get; set; } = "";
        public string Key { get; set; } = "";
        public bool UseFile { get; set; } = false;
        private static string[] permittedExtensions = { ".txt", ".docx" };
        public string OutputText { get; set; }
        public static string DoCiphering(string text, string key, bool isForward)
        {
            if (String.IsNullOrEmpty(text))
                return "";
            if (String.IsNullOrEmpty(key))
                return text;
            List<int> numberfiedWord = new();
            string ans = "";
            int index = 0;
            char[] alph = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя".ToCharArray();
            key = key.ToLower();
            foreach (var item in key)
            {
                if (alph.Contains(item))
                {
                    numberfiedWord.Add(Array.IndexOf(alph, item));
                }
                else if (Char.IsDigit(item))
                {
                    numberfiedWord.Add(item - '0');
                }
                else
                {
                    numberfiedWord.Add(0);
                }
            }
            text = text.ToLower();
            foreach (char item in text)
            {
                if (alph.Contains(item))
                {
                    int itemIndex = Array.IndexOf(alph, item);
                    ans += alph[(33 + itemIndex + (isForward ? 1 : -1) * numberfiedWord[index % numberfiedWord.Count]) % alph.Length];
                    index++;
                }
                else
                {
                    ans += item;
                }
            }
            return ans;
        }

        public static string DoCiphering(IFormFile textFile, string key, bool isForward, string fileEncoding)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (textFile == null)
            {
                return "Ошибка: файл не загружен";
            }
            var ext = Path.GetExtension(textFile.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return "Ошибка: поддерживаются только форматы txt и docx";
            }
            if (ext == ".txt")
            {
                using (Stream stream = textFile.OpenReadStream())
                {
                    if (fileEncoding == "auto")
                    {
                        using (StreamReader sr = new(stream, true))
                        {
                            return DoCiphering(sr.ReadToEnd(), key, isForward);
                        }
                    }
                    else if (fileEncoding == "ansi")
                    {
                        using (StreamReader sr = new(stream, Encoding.GetEncoding(1251), true))
                        {
                            return DoCiphering(sr.ReadToEnd(), key, isForward);
                        }
                    }
                }
            }
            if (ext == ".docx")
            {
                using (Stream stream = textFile.OpenReadStream())
                {
                    string docxText = "";
                    XWPFDocument doc = new(stream);
                    foreach (var item in doc.Paragraphs)
                    {
                        docxText += item.Text;
                    }
                    return DoCiphering(docxText, key, isForward);
                }
            }
            return "";
        }
}
}
