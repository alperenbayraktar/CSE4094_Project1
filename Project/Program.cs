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
    static void insert(string fileName, string key, int index)
    {
        int level;
        int length = key.Length;
        int letter;

        TrieNode pCrawl = root;

        for (level = 0; level < length; level++)
        {
            letter = key[level] - '&';
            if (pCrawl.children[letter] == null)
                pCrawl.children[letter] = new TrieNode();

            pCrawl = pCrawl.children[letter];
            //Add the index to the file, if it exist in the file already, add it to index array.
            pCrawl.indexAtFile.CheckAndAdd(fileName,index);
        }
        //Add the file name to the word-end node. 
        pCrawl.endOfWordinFile.Add(fileName);
    }

    // Returns true if key  
    // presents in trie, else false 
    static void SearchByWord(string key)
    {
        int level;
        int length = key.Length;
        int letter;
        TrieNode pCrawl = root;
        for (level = 0; level < length; level++)
        {
            letter = key[level] - '&';

            if (pCrawl.children[letter] == null)
            {
            }
            pCrawl = pCrawl.children[letter];
        }
        List<string> output = new List<string>();
        if (pCrawl != null)
        {
            foreach (KeyValuePair<string, List<int>> pair in pCrawl.indexAtFile)
            {
                output.Add("In: " + pair.Key + ": " + key + " has been found at indexes: " + String.Join(", ", pair.Value));
            }
        }
        Console.WriteLine(String.Join("\n", output));
    }
    static void SearchAllWords(List<FileInfo> files, TrieNode root, char[] str, int level)
    {
        if (root.endOfWordinFile.Count > 0 && IsCommon(files,root.endOfWordinFile))
        {
            str[level] = '\0';
            for(int l = level; l + 1< str.Length; l++)
            {
                str[l + 1] = ' ';
            }
            Console.WriteLine(str);
        }
        int i;
        for (i = 0; i < ASCII_SIZE; i++)
        {
            if (root.children[i] != null)
            {
                str[level] = (char)(i + '&');
                SearchAllWords(files,root.children[i], str, level + 1);
            }
        }
    }
    static bool IsCommon(List<FileInfo> files,List<string> endOfWordFiles)
    {
        List<string> fileList = new List<string>();
        foreach(var file in files)
        {
            fileList.Add(file.Name);
        }
        return !fileList.Except(endOfWordFiles).Any();
    }
    static List<MyDictionary> Preprocessing(string path, List<FileInfo> files)
    {
        List<MyDictionary> listOfDicts = new List<MyDictionary>();
        int fileCount;
        
        for (fileCount = 0; fileCount < files.Count; fileCount++)
        {
            int indexAtFile = 0;
            string filePath = path + "\\" + files[fileCount].Name;
            //since a word could be seen many times in a file, it will have different indexes. So a string,List<int> dictionary is needed.
            MyDictionary filteredWordsandIndexes = new MyDictionary();
            using (StreamReader reader = new StreamReader(filePath))
            {
                string[] exceptArray = { "-", "\n", " ", "\r", "\r\n" };
                char[] splitArray = { ' ', '\n' };
                // Read entire text file with ReadToEnd.
                var words = reader.ReadToEnd().Split(splitArray).Where(x => !exceptArray.Contains(x));
                foreach (string word in words)
                {
                    //Current index is assigned to the current words index.
                    int thisWordsIndex = indexAtFile;
                    //Current index is incremented by current words length and one space since space is used for splitting.
                    indexAtFile += word.Length + 1; 
                    //Current word is cleared from the unnecessary characters.
                    string wordFiltered = word.Replace(",", "").Replace(".", "").Replace("\r", "").Replace("\n", "").ToLower(new CultureInfo("en-US", false)).ToString();
                    if(wordFiltered.Length > 0)
                        filteredWordsandIndexes.CheckAndAdd(wordFiltered, thisWordsIndex);                
                }
                listOfDicts.Add(filteredWordsandIndexes);
            }
        }        
        return listOfDicts;
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
        Console.WriteLine("Please select files to be searched: (by index and commas like: 1,5,8)\nEnter 'ALL' to select all files");
        string[] files = Console.ReadLine().Split(',');
        
        if(files[0] == "ALL")
        {
            Console.WriteLine("All files have been selected.");
            return TXTFiles;
        }
        else {
            Console.WriteLine("Selected Files: ");
            foreach (string file in files)
            {
                if(Int32.Parse(file) > TXTFiles.Count || Int32.Parse(file) < 0)
                {
                    Console.WriteLine("Give a proper integer value.");
                    return null;
                } 
                searchedFiles.Add(TXTFiles[Int32.Parse(file)]);
                Console.WriteLine(TXTFiles[Int32.Parse(file)].Name);
            }
            return searchedFiles;
        }        
    }
    static List<FileInfo> Prepare(string path)
    {
        List<FileInfo> selectedFiles = SelectFiles(path);
        List<MyDictionary> allFilteredWordsandIndexes = Preprocessing(path, selectedFiles);

        root = new TrieNode();

        //In each file,
        for (int fileCount = 0; fileCount < allFilteredWordsandIndexes.Count; fileCount++)
        {
            //in each word,
            foreach (KeyValuePair<string, List<int>> pair in allFilteredWordsandIndexes[fileCount])
            {
                //insert the word and its index.
                foreach (int index in pair.Value)
                {
                    insert(selectedFiles[fileCount].Name, pair.Key, index);
                }
            }
        }
        return selectedFiles;
    }
    public static void Main(string[] args)
    {
        string path = args[0];
        List<string> inputList = new List<string>();
        inputList.Add("1");
        inputList.Add("2");
        inputList.Add("3");
        while (true)
        {
            Console.WriteLine("What would you want to do?\n" +
                "*********************************** \n" +
                "1- Find files that includes words starting with your input\n" +
                "2- Find common words in files you will choose\n" +
                "3- Exit");
            string input = Console.ReadLine();
            if (!inputList.Contains(input))
            {
                Console.WriteLine("You did not enter an appropriate input. Please try again.");
                continue;
            }
            else
            {
                switch (input)
                {
                    case "1":
                        Prepare(path);
                        Console.WriteLine("Please enter a string to see occurances.");
                        string case1Input = Console.ReadLine();
                        SearchByWord(case1Input);
                        break;
                    case "2":
                        List<FileInfo> selectedFiles =Prepare(path);
                        char[] str = new char[20];
                        int level = 0;
                        Console.WriteLine("Common words:");
                        SearchAllWords(selectedFiles, root, str, level);
                        break;
                    case "3":
                        return;
                    default:
                        break;
                }
            }
        }
    }
}
