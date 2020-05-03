using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Trie
{

    // ASCII size (# of symbols) 
    static readonly int ASCII_SIZE = 'z' - '&' + 1;

    public class MyDictionary : Dictionary<string, List<int>>  //A dictionary that can store the patterns found x times at y kmer length
    {
        public void CheckAndAdd(string fileName, int index)
        {
            if (!this.ContainsKey(fileName))
            {
                this.Add(fileName, index);
            }
            else
            {
                this[fileName].Add(index);
            }
        }
        public void Add(string fileName, int index)
        {
            List<int> indexes = new List<int>();
            indexes.Add(index);
            this.Add(fileName, indexes);
        }
        public void Delete(string fileName)
        {
            this.Remove(fileName);
        }
    }
    class TrieNode
    {        
        public TrieNode[] children = new TrieNode[ASCII_SIZE];
        public List<string> endOfWordinFile = new List<string>();
        public MyDictionary indexAtFile = new MyDictionary();
        public TrieNode()
        {
            for (int i = 0; i < ASCII_SIZE; i++)
                children[i] = null;
        }
    };

    static TrieNode root;

    // If not present, inserts key into trie 
    // If the key is prefix of trie node,  
    // just marks leaf node 
    static void insert(string fileName, String key)
    {
        int level;
        int length = key.Length;
        int index;

        TrieNode pCrawl = root;

        for (level = 0; level < length; level++)
        {
            index = key[level] - '&';
            if (pCrawl.children[index] == null)
                pCrawl.children[index] = new TrieNode();

            pCrawl = pCrawl.children[index];
            pCrawl.indexAtFile.Add(fileName,index);
        }
        // mark last node as leaf 
        pCrawl.endOfWordinFile.Add(fileName);
    }

    // Returns true if key  
    // presents in trie, else false 
    /*static MyDictionary SearchByWord(string key)
    {
        int level;
        int length = key.Length;
        int index;
        TrieNode pCrawl = root;
        MyDictionary FilesAndIndexes = new MyDictionary();
        for (level = 0; level < length; level++)
        {
            index = key[level] - '&';

            if (pCrawl.children[index] == null)
            {
                return FilesAndIndexes;
            }
            pCrawl = pCrawl.children[index];
        }
        //return (pCrawl != null && pCrawl.isEndOfWord);
    }
    static bool search(String key)
    {
        int level;
        int length = key.Length;
        int index;
        TrieNode pCrawl = root;

        for (level = 0; level < length; level++)
        {
            index = key[level] - '&';

            if (pCrawl.children[index] == null)
                return false;

            pCrawl = pCrawl.children[index];
        }

        return (pCrawl != null && pCrawl.isEndOfWord);
    }
    */
    static List<List<string>> Preprocessing(string path, List<FileInfo> files)
    {
        List<List<string>> listOfLists = new List<List<string>>();
        int index;
        for (index = 0; index < files.Count; index++)
        {
            string filePath = path + "\\" + files[index].Name; 
            List<string> filteredWords = new List<string>();
            using (StreamReader reader = new StreamReader(filePath))
            {
                string[] exceptArray = { "-", "\n", " ", "\r", "\r\n" };
                char[] splitArray = { ' ', '\n' };
                // Read entire text file with ReadToEnd.
                var words = reader.ReadToEnd().Split(splitArray).Except(exceptArray, StringComparer.OrdinalIgnoreCase);
                foreach (string word in words)
                {
                    string wordFiltered = word.Replace(",", "").Replace(".", "").Replace("\r", "").Replace("\n", "").ToLower(new CultureInfo("en-US", false)).ToString();
                    if(wordFiltered.Length > 0)
                        filteredWords.Add(wordFiltered);                
                }
                listOfLists.Add(filteredWords);
            }
        }        
        return listOfLists;
    }
    static List<FileInfo> SelectFiles(string path)
    {
        DirectoryInfo dir = new DirectoryInfo(@path);
        List<FileInfo> TXTFiles = dir.GetFiles("*.txt").ToList();
        List<FileInfo> searchedFiles = new List<FileInfo>();

        if (TXTFiles.Count == 0)
        {
            //No files present
        }
        int i = 0;
        foreach (var file in TXTFiles)
        {
            if (file.Exists)
            {
                Console.WriteLine(i + "-> " + file.Name);
                i++;
            }
        }
        Console.WriteLine("Please select files to be searched: (by index and commas like: 1,5,8)\nIf there is no input, all files will be selected by default");
        string[] files = Console.ReadLine().Split(',');
        
        if(files.Length == 0)
        {
            Console.WriteLine("There are no files in the current folder.");
        }
        else {
            Console.WriteLine("Selected Files: ");
            foreach (string file in files)
            {
                searchedFiles.Add(TXTFiles[Int32.Parse(file)]);
                Console.WriteLine(TXTFiles[Int32.Parse(file)].Name);
            }
            return searchedFiles;
        }  
        return TXTFiles;
    }
    public static void Main(string[] args)
    {
        string path = args[0];
        List<FileInfo> selectedFiles = SelectFiles(path);
        List<List<string>> allFilteredWords = Preprocessing(path,selectedFiles);

        String[] output = { "Not present in trie", "Present in trie" };
        
        root = new TrieNode();

        int i;
        for(int fileCount = 0; fileCount < allFilteredWords.Count; fileCount++)
        {
            for (i = 0; i < allFilteredWords[0].Count; i++)
                insert(selectedFiles[fileCount].Name,allFilteredWords[fileCount][i]);
        }        

        /* Search for different keys 
        if (search("alper") == true)
            Console.WriteLine("alper --- " + output[1]);
        else Console.WriteLine("alper --- " + output[0]);

        if (search("alperen") == true)
            Console.WriteLine("alperen --- " + output[1]);
        else Console.WriteLine("alperen --- " + output[0]);
        */
        Console.ReadLine();
    }
}
