using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Parser {
    public class IniParser {
    //Главное поле - список секций
        private List<Section> Sections = new();

        public class Section {
            //Поля - имя секции и список параметров ей принадлежащих
            public string Name;
            public List<Parameter> Parameters;

            public class Parameter {
                //Поля - имя параметра и его значение
                public string Name;
                public string Value;

                public Parameter(string parameter) {
                    var text = parameter.Split('=');
                    Name = text[0];
                    Value = text[1].TrimEnd('\n');
                }

                public override string ToString() {
                    return Name + " = " + Value;
                }
            }

            public Section(string name, List<Parameter> parameters) {
                Name = name;
                Parameters = parameters;
            }
        }

        // private static string Reforming(string text) {
        //     //Перевод текста в нормальный вид (без комментариев, без ненужных пробелов, без лишних переносов строки)
        //
        //     return Regex.Replace(Regex.Replace(Regex.Replace(text, @"\s*=\s*", "="), @"\;.+$",
        //             string.Empty,
        //             RegexOptions.Multiline).Trim("\n\r".ToCharArray()), @"[\n\r]{2,}", "\n");
        // }

        private static string Reforming(string text) {
            //Перевод текста в нормальный вид (без комментариев, без ненужных пробелов, без лишних переносов строки)
            text = Regex.Replace(text, @"\s*=\s*", "=");
            text = Regex.Replace(text, @"\;.*$", string.Empty, RegexOptions.Multiline);
            text = text.Trim("\n\r".ToCharArray());
            text = Regex.Replace(text, @"[\n\r]{2,}", "\n");
            return text;
        }

        public IniParser(string path) {
            if (!File.Exists(path))
                throw new FileNotFoundException("File doesn't exist");

            if (!Regex.IsMatch(path, @"\.ini$"))
                throw new FileLoadException("File isn't .ini file");

            //Чтение всего текста в единую строку
            var text = File.ReadAllText(path);
            //Перевод всего текста в нужный вид
            text = Reforming(text) + '\n';

            //Выделение всех секций
            var SectionsParsed = Regex.Matches(text, @"\[.*\]$", RegexOptions.Multiline).ToArray();

            //Выделение параметров каждой секции (элемент - строка содержащая все параметры секции)
            var ParameterStringList = Regex.Matches(text, @"([^\[\]\n]+\n)+", RegexOptions.Multiline).ToArray();

            for (var i = 0; i < SectionsParsed.Length; ++i) {
                //Список параметров
                var Parameters = new List<Section.Parameter>();
                //Для каждого параметра (то что соответстует паттерну @".+=.+")
                //добавляем в список через конструктор параметра
                foreach (var item in Regex.Matches(ParameterStringList[i].ToString(), @".+=.+"))
                    Parameters.Add(new Section.Parameter(item.ToString()));
                Sections.Add(new Section(SectionsParsed[i].ToString().Trim("[]".ToCharArray()), Parameters));
            }
        }

        public override string ToString() {
            var result = "";
            foreach (var section in Sections) {
                result += $"[{section.Name}]\n";
                result = section.Parameters.Aggregate(result,
                        (current, parameter) => current + ($"{parameter}\n"));
            }

            return result;
        }

        public int SearchInt(string SectionName, string ParameterName) {
            var a = Sections.Find(section => section.Name == SectionName);
            if (a == null) throw new ArgumentException("Section doesn't exist");
            var b = a.Parameters.Find(parameter => parameter.Name == ParameterName);
            if (b == null) throw new ArgumentException("Parameter doesn't exist");
            if (!int.TryParse(b.Value, out var result)) throw new Exception("Cannot convert to int");
            return result;
        }

        public double SearchDouble(string SectionName, string ParameterName) {
            var a = Sections.Find(section => section.Name == SectionName);
            if (a == null) throw new ArgumentException("Section doesn't exist");
            var b = a.Parameters.Find(parameter => parameter.Name == ParameterName);
            if (b == null) throw new ArgumentException("Parameter doesn't exist");
            var value = b.Value.Replace('.', ','); //без этой замены не хочет парсить
            if (!double.TryParse(value, out var result)) throw new Exception("Cannot convert to double");
            return result;
        }

        public string SearchString(string SectionName, string ParameterName) {
            var a = Sections.Find(section => section.Name == SectionName);
            if (a == null) throw new ArgumentException("Section doesn't exist");
            var b = a.Parameters.Find(parameter => parameter.Name == ParameterName);
            if (b == null) throw new ArgumentException("Parameter doesn't exist");
            return b.Value;
        }

        public string SearchFullSection(string SectionName) {
            var a = Sections.Find(section => section.Name == SectionName);
            if (a == null) throw new ArgumentException("Section doesn't exist");
            var result = $"[{a.Name}]\n";
            return a.Parameters.Aggregate(result, (current, parameter) => current + ($"{parameter}\n"));
        }
    }
}