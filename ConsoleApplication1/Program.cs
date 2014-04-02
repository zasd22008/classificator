using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            const int classesCount = 5;

            var list = new List<TextClass>();

            for (int i = 1; i <= classesCount; i++)
                list.Add(ReadFromFile("class" + i + ".txt"));

            var resolver = new Classificator(list);

            var exampleUkraine =
                new StreamReader(File.OpenRead("input.txt"), Encoding.GetEncoding("windows-1251")).ReadToEnd();

            resolver.Classificate(exampleUkraine, 5);

            Console.ReadLine();
        }

        private static TextClass ReadFromFile(string filename)
        {
            var list = new StreamReader(File.OpenRead(filename), Encoding.GetEncoding("windows-1251")).ReadToEnd().Split(new[]{"\r\n\r\n"}, StringSplitOptions.RemoveEmptyEntries).ToList();

            var c = new TextClass();

            c.name = list[0];
            c.texts = list.Skip(1).ToList();

            return c;
        }
    }

    public class TextClass
    {
        public string name;
        public List<string> texts;

        public List<Tuple<string, List<string>>> norm_texts;
    }

    public class Classificator
    {
        List<TextClass> _textClasses;

        public Classificator(List<TextClass> textClasses)
        {
            _textClasses = textClasses;

            foreach (var textClass in _textClasses)
                textClass.norm_texts = textClass.texts.Select(t => new Tuple<string, List<string>>(t, norm(t))).ToList();
        }

        public  void Classificate(string text, int neighbourCount)
        {
            var s = norm(text);

            var results = new List<Tuple<string, TextClass, double>>();

            foreach (var textClass in _textClasses)
            {
                foreach (var text2 in textClass.norm_texts)
                {
                    results.Add(new Tuple<string, TextClass, double>(text2.Item1, textClass, distance(text2.Item2, s)));
                }
            }

            var rating = results.OrderByDescending(r => r.Item3).Take(neighbourCount).ToList();
            
            // выводим результат
            Console.WriteLine("Первые {0} соседей:", neighbourCount);
            Console.WriteLine("Класс           Сходство");

            foreach (var rate in rating)
                Console.WriteLine("{0,-20}  {1}", rate.Item2.name, rate.Item3);
            
            var result = rating.GroupBy(r => r.Item2).ToList();

            var max = result.Max(r => r.Count());

            Console.WriteLine("Строка отнесена к классу {0}", result.First(r => r.Count() == max).Key.name);
        }

        private List<string> norm(string text)
        {
            var words = text.ToLower().Split(' ', '\t', '\r', '\n').Select(w => w.Trim(' ', '\t')).Where(w => !String.IsNullOrWhiteSpace(w));

            var stopSymbols = new[] {".", ",", "!", "?", ":", ";", "-", "\n", "\r", "(", ")"};

            var stopWords = new[]
            {
                "это", "как", "так", "и", "в", "над",
                "к", "до", "не", "на", "но", "за",
                "то", "с", "ли",
                "а", "во", "от",
                "со", "для", "о",
                "же", "ну", "вы",
                "бы", "что", "кто",
                "он", "она"
            };

            return words.Where(w => !stopWords.Contains(w) && !stopSymbols.Contains(w)).ToList();
        }

        public double distance(List<string> s1, List<string> s2)
        {
            var w1 = s1.Union(s2).Distinct();

            var v1 = new List<int>();
            var v2 = new List<int>();

            foreach (var word in w1)
            {
                v1.Add(s1.Count(s => s == word));
                v2.Add(s2.Count(s => s == word));
            }

            var distance = v1.Zip(v2, (q1, q2) => q1 * q2).Sum();

            return distance;
        }
    }

}
