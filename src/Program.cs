using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ParserDoc's purpose is to read files and produce an image diagram of the class hiararchies

class ParserDoc {

    // classes: we want to save their name (key), their parents, fields, properties, events, and methods
    Dictionary<string, DataClass>? _data_classes = new();
    // enums: we want to save their name (key) and name of its constants
    Dictionary<string, List<string>?>? _data_enums = new();

    ParserDoc() {

    }

    static void Main(string[] args) {

        ParserDoc parserDoc = new ParserDoc();

        foreach(var a in args) {
            parserDoc.AnalyzeFile(a);
        }

    }

    // parses file and navigates through descendants, saving relevant data.
    private void AnalyzeFile(string path) {

        if (!File.Exists(path) || !Path.GetExtension(path).Equals(".cs", StringComparison.OrdinalIgnoreCase)) return;

        // read and parse
        var code = File.ReadAllText(path);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        // check class nodes        
        foreach (var node in root.DescendantNodes()) {
            switch (node) {
                case ClassDeclarationSyntax cls:
                    // Console.WriteLine($"class found: {cls.Identifier.Text}");
                    break;

                case EnumDeclarationSyntax en:
                    List<string>? constants = []; // init list
                    foreach (var m in en.Members)
                        constants.Add(m.Identifier.Text);
                    // save enum data
                    _data_enums.Add(en.Identifier.Text, constants);
                    break;
            }
        }
    }
}

class DataClass {
    public List<string>? Parents { get; set; }
    public List<string>? Fields { get; set; }
    public List<string>? Properties { get; set; }
    public List<string>? Events { get; set; }
    public List<string>? Methods {get; set; }
}

