using System;
using System.Collections.Generic;
using System.IO;

namespace FTPFileWatcher.Parsers
{
    public class JSONParser
    {
        public static JSONDocument Load(StreamReader input)
        {
            return new JSONDocument(LoadNode(input));
        }

        private static Node LoadNode(StreamReader input)
        {
            RemoveSpace(input);
            char c = Convert.ToChar(input.Peek());

            switch (c)
            {
                case '[':
                    {
                        input.Read();
                        RemoveSpace(input);
                        return LoadArray(input);
                    }
                case '{':
                    {
                        input.Read();
                        RemoveSpace(input);
                        return LoadDictionary(input);
                    }
                case '"':
                    {
                        input.Read();
                        RemoveSpace(input);
                        return LoadString(input);
                    }
                default:
                    {
                        return LoadInt(input);
                    }
            }
        }

        private static Node LoadArray(StreamReader input)
        {
            List<Node> result = new List<Node>();

            for (char c; RemoveSpace(input) && (c = Convert.ToChar(input.Peek())) != -1 && c != ']';)
            {
                if (c == ',')
                {
                    input.Read();
                }
                result.Add(LoadNode(input));
            }

            return new Node(result);
        }

        private static Node LoadInt(StreamReader input)
        {
            int result = 0;
            while (Char.IsDigit(Convert.ToChar(input.Peek())))
            {
                result *= 10;
                result += input.Read() - 48;
            }
            return new Node(result);
        }

        private static Node LoadString(StreamReader input)
        {
            string line;
            line = Separated(input, '"');
            return new Node(line);
        }

        private static Node LoadDictionary(StreamReader input)
        {
            var result = new Dictionary<string, Node>();


            for (char c; RemoveSpace(input) && (c = Convert.ToChar(input.Read())) != -1 && c != '}';)
            {
                if (c == ',')
                {
                    RemoveSpace(input);
                    c = Convert.ToChar(input.Read());
                }

                string key = LoadString(input).AsString();
                input.Read();
                result.Add(key, LoadNode(input));
            }

            return new Node(result);
        }

        private static string Separated(StreamReader input, char by = '\0')
        {
            var line = string.Empty;
            while (input.Peek() >= 0)
            {
                char c = (char)input.Read();
                if (c == '\n' || c == by || c == '\r')
                {
                    return line;
                }
                else
                {
                    line += c;
                }
            }
            return line;
        }

        private static bool RemoveSpace(StreamReader input)
        {
            while (input.Peek() == 32 || input.Peek() == '\r' || input.Peek() == '\n') input.Read();
            return true;
        }
    }

    public class Node
    {
        private readonly object data;

        public Node(object data)
        {
            this.data = data;
        }

        public List<Node> AsArray()
        {
            return (List<Node>)Convert.ChangeType(data, typeof(List<Node>));
        }

        public Dictionary<string, Node> AsDictionary()
        {
            return (Dictionary<string, Node>)Convert.ChangeType(data, typeof(Dictionary<string, Node>));
        }

        public int AsInt()
        {
            return (int)Convert.ChangeType(data, typeof(int));
        }

        public string AsString()
        {
            return (string)Convert.ChangeType(data, typeof(string));
        }
    }

    public class JSONDocument
    {
        public Node Root { get; private set; }

        public JSONDocument(Node root)
        {
            Root = root;
        }
    }
}
