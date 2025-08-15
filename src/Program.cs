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

                // --------------------------------------------------------------------------------
                // classes
                // --------------------------------------------------------------------------------
                
                case ClassDeclarationSyntax cls:

                    // ---------- save class members data ---------- //
                    
                    var list_parents = cls.BaseList?.Types.Select(t => t.Type.ToString()).ToList() ?? new List<string>();
                    var list_fields = cls.Members.OfType<FieldDeclarationSyntax>().Select(t => t.ToString()).ToList();
                    var list_properties = cls.Members.OfType<PropertyDeclarationSyntax>().Select(t => t.ToString()).ToList();
                    var list_eventfields = cls.Members.OfType<EventFieldDeclarationSyntax>().Select(t => t.ToString()).ToList();
                    var list_methods = cls.Members.OfType<MethodDeclarationSyntax>().Select(v => v
                                .WithAttributeLists(default)
                                .WithLeadingTrivia(SyntaxTriviaList.Empty)
                                .WithBody(null)
                                .WithExpressionBody(null)
                                .WithConstraintClauses(default)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                .NormalizeWhitespace()
                                .ToFullString()).ToList();
                    
                    // ----------------- save data ----------------- //

                    DataClass data_class = new DataClass {
                        Parents    = list_parents,
                        Fields     = list_fields,
                        Properties = list_properties,
                        Events     = list_eventfields,
                        Methods    = list_methods
                    };
                    _data_classes?.Add(cls.Identifier.Text, data_class);
                    break;

                // --------------------------------------------------------------------------------
                // enums
                // --------------------------------------------------------------------------------

                case EnumDeclarationSyntax en:
                    var constants = en.Members.Select(t => t.Identifier.Text).ToList();
                    _data_enums?.Add(en.Identifier.Text, constants);
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

