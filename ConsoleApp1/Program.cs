using DictEtLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Dict resDict;

                var ct = Core.lookupCancelTS.Token;
                resDict = Core.GetWordsAsync("tom", ct).Result;

                Console.WriteLine(resDict.Lookup);
                Console.WriteLine(string.Join(", ", resDict.Words));
                foreach (var wordString in resDict.Words)
                {
                    EnWord uus = new EnWord() { Word = wordString};
                    //DictDatabase.AddEnWord(uus).Wait();
                }


                resDict = Core.GetDescAsync("combat", ct).Result;
                //resDict = Core.GetDescAsync("it", ct).Result;

                foreach (var item in resDict.WordDescs)
                {
                    Console.WriteLine($"{item.Key}");
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine($"{Core.AddHtmlHeaderFooter(item.Value)}");
                    Console.WriteLine("-----------------------------------");
            }


            //Core.lookupCancelTS.Cancel();


            try
            {
                var ct2 = Core.lookupCancelTS.Token;
                resDict = Core.GetWordsAsync("new", ct2).Result;

                Console.WriteLine(resDict.Lookup);
                Console.WriteLine(string.Join(", ", resDict.Words));
            }
            catch (Exception ex)
            {
                Console.WriteLine("teine try");
                Console.WriteLine(ex.Message);
            }

            //var allWords = DictDatabase.GetEnWords().Result;
            //foreach (var item in allWords)
            //{
            //    Console.WriteLine(item);
            //}

            Console.ReadKey();
        }
    }
}
