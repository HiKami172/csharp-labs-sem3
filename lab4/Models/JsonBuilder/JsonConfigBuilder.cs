using System;
using System.Collections.Generic;
using System.IO;
using FTPFileWatcher.Parsers;

namespace JsonBuilder
{

    public delegate string ActionHandler<T>(T node);
    public class TypeSwitch
    {
        Dictionary<Type, ActionHandler<object>> matches = new Dictionary<Type, ActionHandler<object>>();
        public TypeSwitch Case<T>(ActionHandler<T> action) { matches.Add(typeof(T), (x) => action((T)Convert.ChangeType(x, typeof(T)))); return this; }
        public string Switch(object x, Type xType) { return matches[xType](x); }
    }
    public class JsonConfigBuilder
    {
        private Dictionary<string, Node> json = new Dictionary<string, Node>();
        private static TypeSwitch ts = new TypeSwitch()
                .Case((Dictionary<string, Node> x) => MakeJson(x))
                .Case((List<Node> x) => MakeJson(x))
                .Case((string x) => MakeJson(x))
                .Case((int x) => MakeJson(x))
                .Case((DateTime x) => MakeJson(x));

        public void LoadTable<T>(IEnumerable<T> table, string tableName) where T : class
        {
            var tableNode = new List<Node>();
            foreach(T element in table)
            {
                var dictNode = new Dictionary<string, Node>();

                var fields = element.GetType().GetProperties();
                foreach (var field in fields)
                    dictNode.Add(field.Name, new Node(field.GetValue(element)));
                tableNode.Add(new Node(dictNode));
            }
            json.Add(tableName, new Node(tableNode));
        }

        public void MakeJsonConfig(string path, string fileName)
        {
            using(var file = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                using(var writer = new StreamWriter(file))
                {
                    writer.WriteLine(MakeJson(json));
                }
            }
        }

        private static string MakeJson(Dictionary<string, Node> nodes)
        {
            string res = "";
            bool isFirst = true;
            foreach(KeyValuePair<string, Node> node in nodes)
            {
                if (isFirst == false)
                    res += ",\n";
                res += MakeJson(node.Key) + ": " + MakeJson(node.Value);
                isFirst = false;
            }
            return "{\n" + res + "\n}";
        }

        private static string MakeJson(List<Node> nodes)
        {
            string res = "";
            bool isFirst = true;
            foreach (Node node in nodes)
            {
                if (isFirst == false)
                    res += ",\n";
                res += MakeJson(node);
                isFirst = false;
            }
            return "[\n" + res + "\n]";
        }

        private static string MakeJson(DateTime node)
        {
            var nodes = new Dictionary<string, Node>();

            nodes.Add("Year", new Node(node.Year));
            nodes.Add("Month", new Node(node.Month));
            nodes.Add("Day", new Node(node.Day));
            return MakeJson(nodes);
        }

        private static string MakeJson(string nodes)
        {
            return "\"" + nodes + "\"";
        }

        private static string MakeJson(int nodes)
        {
            return nodes.ToString();
        }

        private static string MakeJson(Node node)
        {
            return ts.Switch(node, node.GetType());
        }

    }
}
