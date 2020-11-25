using System;
using System.IO;
using System.Collections.Generic;

namespace CS_lab1
{
    class Human
    {
        string name;
        int age;
        string occupation;

        public Human(string name, string occupation, int age)
        {
            this.name = name;
            this.age = age;
            this.occupation = occupation;
        }

        public string GetInfo()
        {
            return $"{name}, {occupation}, {age}";
        }
    }

    class FileData
    {
        public int id;
        List<Human> humanList;

        public FileData()
        {
            id = Guid.NewGuid().GetHashCode();
            humanList = new List<Human>();
        }

        public FileData(List<Human> humanList)
        {
            id = Guid.NewGuid().GetHashCode();
            this.humanList = humanList;
        }

        public string GetListAsString()
        {
            string resultList = "";

            for(int i = 0; i < humanList.Count - 1; ++i)
            {
                resultList += humanList[i].GetInfo() + "\n";
            }

            resultList += humanList[humanList.Count - 1].GetInfo();

            return resultList;
        }

        public static FileData TextToFileData(string text)
        {
            FileData result = new FileData();
            string[] lines = text.Split('\n');
            foreach (string line in lines)
            {
                string[] props = line.Split(',');
                for (int i = 0; i < 3; ++i)
                    props[i] = props[i].Trim();
                result.humanList.Add(new Human(props[0], props[1], Convert.ToInt32(props[2])));
            }
            return result;
        }
    }

    class Program
    {
        static bool continueWork = true;
        static string filesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\") + @"\files";

        static void Main(string[] args)
        {
            DirectoryInfo filesDirInfo = new DirectoryInfo(filesDirPath);

            if (!filesDirInfo.Exists)
                filesDirInfo.Create();
            while (continueWork)
            {
                MainMenuOptions();
            }
        }

        static void MainMenuOptions()
        {
            Console.WriteLine("Choose option:\n");

            Console.WriteLine("1. Create new file");
            Console.WriteLine("2. Open file");
            Console.WriteLine("3. Open all files");
            Console.WriteLine("4. Copy file");
            Console.WriteLine("5. Rename file");
            Console.WriteLine("6. Exit");

            Console.WriteLine();

            string choice = Console.ReadLine();

            Console.Clear();

            switch (choice)
            {
                case "1":
                    CreateNewFile();
                    break;
                case "2":
                    OpenFileMenu();
                    break;
                case "3":
                    OpenAllFiles();
                    break;
                case "4":
                    CopyFile();
                    break;
                case "5":
                    RenameFile();
                    break;
                case "6":
                    continueWork = false;
                    break;
                default:
                    Console.WriteLine("Wrong input!\nPress any button...");
                    Console.ReadKey();
                    break;
            }
        }

        static void CreateNewFile()
        { 
            int counter = 1;
            List<Human> humanList = new List<Human>();

            while (true)
            {
                string input;

                Console.WriteLine(counter.ToString() + ". ");

                Console.Write("Enter name: ");
                input = Console.ReadLine();
                string name = input;

                int age;
                while (true)
                {
                    Console.Write("Enter age: ");
                    input = Console.ReadLine();
                    try
                    {
                        age = Convert.ToInt32(input);
                        break;
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Wrong input!");
                    }
                }

                Console.Write("Enter occupation: ");
                input = Console.ReadLine();
                string occupation = input;

                Human human = new Human(name, occupation, age);

                humanList.Add(human);
                counter++;

                Console.Write("Press f to finish editing or any other key to continue...");
                ConsoleKeyInfo key = Console.ReadKey();

                if (key.KeyChar == 'f')
                    break;

                Console.WriteLine();
            }
            FileData data = new FileData(humanList);
            Console.Clear();

            using (FileStream writeStream = new FileStream(filesDirPath + $"\\{data.id}.txt", FileMode.OpenOrCreate))
            {
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(data.GetListAsString());
                writeStream.Write(byteArray, 0, byteArray.Length);
            }

            Console.Clear();
        }

        static void OpenFileMenu()
        {
            string input;

            while (true)
            {
                Console.Write("Enter file name (with extension): ");
                input = Console.ReadLine();
                if (File.Exists(filesDirPath + $"\\{input}"))
                    break;
                else
                {
                    Console.WriteLine("File not found!");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }

            Console.Clear();

            OpenFile(input);

            Console.Write("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }

        static void OpenAllFiles()
        {
            string[] files = Directory.GetFiles(filesDirPath);

            for (int i = 0; i < files.Length; ++i)
                files[i] = Path.GetFileName(files[i]);

            foreach (string file in files)
                OpenFile(file);

            Console.Write("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }

        static void OpenFile(string fileName)
        {
            string filePath = filesDirPath + $"\\{fileName}";
            string textFromFile = "";
            using (FileStream readStream = File.OpenRead(filePath))
            {
                byte[] byteArray = new byte[readStream.Length];

                readStream.Read(byteArray, 0, byteArray.Length);
                textFromFile = System.Text.Encoding.Default.GetString(byteArray);
            }

            Console.WriteLine($"{fileName}:\n");
            Console.Write(textFromFile + "\n\n");
        }

        static void CopyFile()
        {
            string sourceFile;
            string destinationFile;

            string input;

            while (true)
            {
                Console.Write("Enter file name (with extension): ");
                input = Console.ReadLine();
                if (File.Exists(filesDirPath + $"\\{input}"))
                    break;
                else
                {
                    Console.WriteLine("File not found!");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
            string fileName = input;
            sourceFile = filesDirPath + $"\\{input}";

            while (true)
            {
                Console.WriteLine("Enter path of directory you want to copy file to: ");
                input = Console.ReadLine();
                if (Directory.Exists(@input))
                {
                    destinationFile = @input + $"\\{fileName}";
                    try
                    {
                        File.Copy(sourceFile, destinationFile, true);
                    }
                    catch (IOException iox)
                    {
                        Console.WriteLine(iox.Message);
                    }
                    break;
                }
                else
                    Console.WriteLine("Dir doesn't exists!");
            }
            Console.Clear();
        }

        static void RenameFile()
        {
            string oldName;
            string newName;

            string input;

            while (true)
            {
                Console.Write("Enter file name (with extension): ");
                input = Console.ReadLine();
                if (File.Exists(filesDirPath + $"\\{input}"))
                    break;
                else
                {
                    Console.WriteLine("File not found!");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }

            oldName = filesDirPath + $"\\{input}";

            Console.Write("Enter new name (with extension): ");
            input = Console.ReadLine();
            newName = filesDirPath + $"\\{input}";

            File.Move(oldName, newName);
            Console.Clear();
        }
    }
}