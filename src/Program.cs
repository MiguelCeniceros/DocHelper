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

                    // ------------------- fields ------------------- //
                    
                    var fields = cls.Members.OfType<FieldDeclarationSyntax>();
                    List<string>? list_fields = [];
                    foreach (var v in fields) {
                        list_fields.Add(v.ToString());
                    }

                    // ----------------- properties ----------------- //
                                            
                    var properties = cls.Members.OfType<PropertyDeclarationSyntax>();
                    List<string>? list_properties = [];
                    foreach (var v in properties) {
                        list_properties.Add(v.ToString());
                    }
                    
                    // var eventFields = cls.Members.OfType<EventFieldDeclarationSyntax>();
                    // var eventProps  = cls.Members.OfType<EventDeclarationSyntax>();
                    

                    // ------------------ methods ------------------ //
                    
                    var methods = cls.Members.OfType<MethodDeclarationSyntax>();
                    List<string>? list_methods = [];
                    foreach (var v in methods) {
                        var declaration_only = v
                            .WithAttributeLists(default)
                            .WithLeadingTrivia(SyntaxTriviaList.Empty)
                            .WithBody(null)
                            .WithExpressionBody(null)
                            .WithConstraintClauses(default)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            .NormalizeWhitespace();
                        // Console.WriteLine(declaration_only.ToFullString());
                        list_methods?.Add(declaration_only.ToFullString());
                    }

                    // --------------- add class data --------------- //

                    DataClass data_class = new DataClass {
                        Fields = list_fields,
                        Properties = list_properties,
                        Methods = list_methods
                    };
                    // save class data
                    _data_classes?.Add(cls.Identifier.Text, data_class);
                    break;

                // --------------------------------------------------------------------------------
                // enums
                // --------------------------------------------------------------------------------

                case EnumDeclarationSyntax en:
                    List<string>? constants = []; // init list
                    foreach (var m in en.Members)
                        constants.Add(m.Identifier.Text);
                    // save enum data
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

