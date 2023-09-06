﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FTPFileWatcher.Parsers
{
    public class XMLParser
    {
        public static XMLDocument Load(StreamReader input)
        {
            var number = Split(Split(input.ReadLine(), "\"").Item2, "\"").Item1;
            if (int.Parse(number[0].ToString()) == 1)
                return new XMLDocument(FirstLoadNode(input));
            else
                return new XMLDocument(SecondLoadNode(input));
        }

        private static (string, string) Split(string line, string by = "\n")
        {
            int pos = line.IndexOf(by);
            if (pos == -1)
                pos = line.Length;
            string left = line.Substring(0, pos);

            if (pos < line.Length && pos + 1 < line.Length)
            {
                return (left, line.Substring(pos + by.Length));
            }
            else
            {
                return (left, string.Empty);
            }
        }

        private static string Lstrip(ref string line)
        {
            while (line != string.Empty && char.IsWhiteSpace(line[0]))
            {
                line = line.Remove(0, 1);
            }
            return line;
        }

        private static string Unquote(string value)
        {
            if (value != string.Empty && value.First() == '"')
            {
                value = value.Remove(0, 1);
            }
            if (value != string.Empty && value.Last() == '"')
            {
                value.Remove(value.Length - 2);
            }
            return value;
        }

        private static XMLNode SecondLoadNode(StreamReader input)
        {
            string rootName = input.ReadLine();

            var root = new XMLNode(rootName.Substring(1, rootName.Length - 2), new Dictionary<string, string>());
            for (string line; (line = input.ReadLine()) != string.Empty && Lstrip(ref line)[1] != '/';)
            {
                var (nodeName, attrs) = Split(Lstrip(ref line), " ");
                attrs = Split(attrs, ">").Item1;
                var nodeAttrs = new Dictionary<string, string>();
                while (attrs != string.Empty)
                {
                    var (head, tail) = Split(attrs, "\" ");
                    var (name, value) = Split(head, "=");
                    if (name != string.Empty && value != string.Empty)
                    {
                        nodeAttrs[Unquote(name)] = Unquote(value);
                    }
                    attrs = tail;
                }

                root.AddChild(new XMLNode(nodeName.Substring(1), nodeAttrs));
            }
            return root;
        }

        private static XMLNode FirstLoadNode(StreamReader input, string rootName = "")
        {
            if (rootName == string.Empty)
                rootName = input.ReadLine();

            var root = new XMLNode(rootName.Substring(1, rootName.Length - 2), new Dictionary<string, string>());
            var nodeAttrs = root.GetDictionary();
            for (string line; (line = input.ReadLine()) != string.Empty && Lstrip(ref line)[1] != '/';)
            {
                if (line == string.Concat("</", rootName.Substring(1)))
                    break;
                if (line == Split(line, "</").Item1)
                {
                    root.AddChild(FirstLoadNode(input, line));
                    continue;
                }
                var (name, closeAction) = Split(Lstrip(ref line), "</");
                var (nodeName, value) = Split(name, ">");
                nodeName = nodeName.Substring(1, nodeName.Length - 1);
                if (nodeName != closeAction.Substring(0, closeAction.LastIndexOf('>')))
                    throw new ArgumentException("Tag is not opened/closed:"
                        + nodeName + "!=" + closeAction.Substring(0, closeAction.Length - 2));
                if (name != string.Empty && value != string.Empty)
                {
                    nodeAttrs[nodeName] = value;
                }
            }
            return root;
        }
    }

    public class XMLNode
    {
        private string name;
        private List<XMLNode> children;
        private Dictionary<string, string> attributes;

        class FindNodeClass
        {
            private string _str;

            public FindNodeClass(string str)
            {
                _str = str;

            }
            public bool FindNode(XMLNode lhs)
            {
                return lhs.name == _str;
            }
        }

        public XMLNode(string name, Dictionary<string, string> attributes)
        {
            children = new List<XMLNode>();
            this.name = name;
            this.attributes = attributes;
        }

        public static Predicate<XMLNode> Find(string name)
        {
            var find = new FindNodeClass(name);
            return find.FindNode;
        }

        public ref List<XMLNode> Children()
        {
            return ref children;
        }

        public ref Dictionary<string, string> GetDictionary()
        {
            return ref attributes;
        }

        public void AddChild(XMLNode node)
        {
            children.Add(node);
        }

        public string Name()
        {
            return name;
        }

        public T AttributeValue<T>(string name)
        {
            return (T)Convert.ChangeType(attributes[name], typeof(T));
        }
    };

    public class XMLDocument
    {
        public XMLNode Root { get; private set; }

        public XMLDocument(XMLNode root)
        {
            Root = root;
        }
    };
}
