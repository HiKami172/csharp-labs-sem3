using System;
using System.Collections.Generic;
using System.IO;
using static FTPFileWatcher.FTPRequestManager;

namespace FTPFileWatcher.Parsers
{
    class ParserManager
    {
        public string TargetPath { get; private set; }
        public string SourcePath { get; private set; }
        public bool IsParsed { get; private set; } = false;
        public Dictionary<EncryptCompressMode, string> Mods { get; private set; } = new Dictionary<EncryptCompressMode, string>();

        public void MakeParsed(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var stream = new StreamReader(file))
                {
                    if (path.Substring(path.LastIndexOf(".") + 1).ToLower() == "xml")
                        TryParse(XMLParser.Load(stream));
                    else
                        Parse(JSONParser.Load(stream));
                }
            }
        }

        private void TryParse(XMLDocument xmlDoc)
        {
            try
            {
                ParseFirstType(xmlDoc);

            }
            catch (Exception)
            {
                ParseSecondType(xmlDoc);
            }
        }

        private void ParseFirstType(XMLDocument xmlDoc)
        {
            TargetPath = xmlDoc.Root.AttributeValue<string>("TargetPath");
            SourcePath = xmlDoc.Root.AttributeValue<string>("SourcePath");
            foreach (var mod in xmlDoc.Root.Children().Find(XMLNode.Find("Mods")).GetDictionary())
            {
                Mods[(EncryptCompressMode)Enum.Parse(typeof(EncryptCompressMode), mod.Key)] = mod.Value;
            }
            IsParsed = true;
        }

        private void ParseSecondType(XMLDocument xmlDoc)
        {
            TargetPath = xmlDoc.Root.Children()[0].AttributeValue<string>("TargetPath");
            SourcePath = xmlDoc.Root.Children()[0].AttributeValue<string>("SourcePath");
            foreach (var mod in xmlDoc.Root.Children()[1].GetDictionary())
            {
                Mods[(EncryptCompressMode)Enum.Parse(typeof(EncryptCompressMode), mod.Key)] = mod.Value;
            }
            IsParsed = true;
        }

        private void Parse(JSONDocument jsonDoc)
        {
            TargetPath = jsonDoc.Root.AsDictionary()["TargetPath"].AsString();
            SourcePath = jsonDoc.Root.AsDictionary()["SourcePath"].AsString();
            foreach (var mod in jsonDoc.Root.AsDictionary()["Mods"].AsDictionary())
            {
                Mods[(EncryptCompressMode)Enum.Parse(typeof(EncryptCompressMode), mod.Key)] = mod.Value.AsString();
            }
            IsParsed = true;
        }
    }
}
