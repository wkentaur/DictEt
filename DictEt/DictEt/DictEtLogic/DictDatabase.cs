using PCLStorage;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DictEtLogic
{
    public static class DictDatabase
    {
        private static readonly SQLiteAsyncConnection sqlConn;
        private const string DB_FILE = "DictSqlite.db3";

        static DictDatabase()
        {
            sqlConn = new SQLiteAsyncConnection(GetLocalFilePath(DB_FILE));
            sqlConn.CreateTableAsync<EnWord>();
        }

        public static string GetDbFilePath()
        {
            return GetLocalFilePath(DB_FILE);
        }

        private static string GetLocalFilePath(string filename)
        {
            string path = "";

            if (filename.Length > 0)
            {
                IFolder folder = FileSystem.Current.LocalStorage;
                path = PortablePath.Combine(folder.Path.ToString(), filename);
            }

            return path;
        }

        public static async Task<int> WordCount()
        {
            return await sqlConn.Table<EnWord>().CountAsync();
        }


        public static async Task<List<EnWord>> GetAllWords()
        {
            return await sqlConn.Table<EnWord>().ToListAsync(); 
        }

        public static async Task<List<EnWord>> GetStartsWith(string theWord)
        {
            //FIXME sort in db query
            var query = sqlConn.Table<EnWord>()
                .Where(v => v.Word.StartsWith(theWord, StringComparison.OrdinalIgnoreCase));
            var tempList = await query.ToListAsync();
            tempList.Sort((x, y) =>
                    x.Word.CompareTo(y.Word));

            return tempList;
        }

        public static async Task<EnWord> GetEnWord(string theWord)
        {
            return await sqlConn.Table<EnWord>().Where(tdi => tdi.Word == theWord).FirstOrDefaultAsync();
        }

        public static async Task DeleteEnWord(EnWord theWord)
        {
            await sqlConn.DeleteAsync(theWord);
        }

        public static async Task AddEnWord(EnWord theWord)
        {
            await sqlConn.InsertAsync(theWord);
        }
    }


    [Table("enword")]
    public class EnWord
    {
        [PrimaryKey, MaxLength(150)]
        public string Word { get; set; }

        public override string ToString()
        {
            return this.Word;
        }
    }
}
