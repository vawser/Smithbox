namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor.Highlight.CSharp
{
    using Hexa.NET.ImGui.Widgets.Extras.TextEditor;
    using Hexa.NET.Utilities;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    public unsafe class CSharpSyntaxHighlight : SyntaxHighlight
    {
        private SyntaxTree? syntaxTree;

        public CSharpSyntaxHighlight() : base("C#", @".*\.cs")
        {
            Initialize();
        }

        private static readonly CSharpParseOptions options = new(LanguageVersion.CSharp12);

        protected override unsafe void FindMatches(StdWString* text, Span<char> span)
        {
            base.FindMatches(text, span);

            if (syntaxTree == null)
            {
                syntaxTree = CSharpSyntaxTree.ParseText(text->ToString(), options);
            }
            else
            {
                syntaxTree = syntaxTree.WithChangedText(SourceText.From(text->ToString()));
            }

            var root = syntaxTree.GetRoot();

            foreach (var node in root.DescendantNodesAndTokens(descendIntoTrivia: true))
            {
                if (node.IsNode)
                {
                    var syntaxNode = node.AsNode();
                    if (syntaxNode != null)
                    {
                        HandleNode(syntaxNode);
                    }
                }
                else
                {
                    HandleToken(node.AsToken());
                }
            }
        }

        private unsafe void HandleNode(SyntaxNode node)
        {
            switch (node)
            {
                case ClassDeclarationSyntax classDecl:
                    AddSpan(classDecl.Identifier, 0x4ec9b0ff); // User-defined type color
                    break;

                case StructDeclarationSyntax structDecl:
                    AddSpan(structDecl.Identifier, 0x86c691ff);
                    break;

                case MethodDeclarationSyntax methodDecl:
                    AddSpan(methodDecl.Identifier, 0xdcdcaaff); // Method color
                    HandleParameterList(methodDecl.ParameterList);
                    HandleMethodBody(methodDecl.Body, true);
                    break;

                case LocalFunctionStatementSyntax localFunc:
                    AddSpan(localFunc.Identifier, 0xdcdcaaff); // Local function color
                    HandleParameterList(localFunc.ParameterList);
                    HandleMethodBody(localFunc.Body, true);
                    break;

                case AttributeSyntax attributeSyntax:
                    AddSpan(attributeSyntax.Name.Span, 0x4ec9b0ff);
                    break;

                case InvocationExpressionSyntax invocation:
                    if (invocation.Expression is IdentifierNameSyntax methodName)
                    {
                        AddSpan(methodName.Identifier, 0xdcdcaaff); // Method call color
                    }
                    else if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        if (memberAccess.Name is IdentifierNameSyntax memberName)
                        {
                            AddSpan(memberName.Identifier, 0xdcdcaaff); // Method call color
                        }
                        else if (memberAccess.Name is GenericNameSyntax genericName)
                        {
                            AddSpan(genericName.Identifier, 0xdcdcaaff); // Generic method call color
                        }
                    }
                    else if (invocation.Expression is GenericNameSyntax genericMethodName)
                    {
                        AddSpan(genericMethodName.Identifier, 0xdcdcaaff); // Generic method call color
                    }
                    break;

                    // Add more cases for different node types as needed
            }
        }

        private readonly HashSet<string> localDeclarationStatementSyntaxes = new();

        private unsafe void HandleMethodBody(BlockSyntax? body, bool clear)
        {
            if (clear)
            {
                localDeclarationStatementSyntaxes.Clear();
            }

            if (body == null)
                return;

            foreach (var statement in body.Statements)
            {
                HandleStatement(statement);
            }
        }

        private unsafe void HandleStatement(StatementSyntax statement)
        {
            switch (statement)
            {
                case LocalDeclarationStatementSyntax localDecl:
                    foreach (var variable in localDecl.Declaration.Variables)
                    {
                        localDeclarationStatementSyntaxes.Add(variable.Identifier.ValueText);
                        AddSpan(variable.Identifier, 0x9cdcfeff); // Local variable declaration color
                    }
                    break;

                case ExpressionStatementSyntax expressionStatement:
                    HandleExpression(expressionStatement.Expression);
                    break;

                case ForStatementSyntax forStatement:
                    if (forStatement.Declaration != null)
                    {
                        foreach (var variable in forStatement.Declaration.Variables)
                        {
                            localDeclarationStatementSyntaxes.Add(variable.Identifier.ValueText);
                            AddSpan(variable.Identifier, 0x9cdcfeff); // Local variable declaration in for loop color
                        }
                    }
                    HandleExpression(forStatement.Condition);
                    foreach (var expressionSyntax in forStatement.Incrementors)
                    {
                        HandleExpression(expressionSyntax);
                    }
                    break;

                case IfStatementSyntax ifStatement:
                    HandleExpression(ifStatement.Condition);
                    HandleStatement(ifStatement.Statement);
                    if (ifStatement.Else != null)
                    {
                        HandleStatement(ifStatement.Else.Statement);
                    }
                    break;

                case WhileStatementSyntax whileStatement:
                    HandleExpression(whileStatement.Condition);
                    HandleStatement(whileStatement.Statement);
                    break;

                case BlockSyntax block:
                    HandleMethodBody(block, false);
                    break;

                    // Add more cases for different statement types as needed
            }
        }

        private unsafe void HandleParameterList(ParameterListSyntax parameterList)
        {
            if (parameterList == null)
                return;

            foreach (var parameter in parameterList.Parameters)
            {
                AddSpan(parameter.Identifier, 0x9cdcfeff); // Parameter color
            }
        }

        private unsafe void HandleExpression(ExpressionSyntax? expression)
        {
            if (expression == null)
                return;

            foreach (var node in expression.DescendantNodesAndSelf())
            {
                if (node is InvocationExpressionSyntax invocationExpression)
                {
                    HandleExpression(invocationExpression.Expression);
                    continue;
                }

                if (node is IdentifierNameSyntax identifierName)
                {
                    if (localDeclarationStatementSyntaxes.Contains(identifierName.Identifier.ValueText))
                    {
                        AddSpan(identifierName.Identifier, 0x9cdcfeff); // Variable usage color
                    }
                }
            }
        }

        private unsafe void HandleToken(SyntaxToken token)
        {
            var kind = token.Kind();
            if (token.IsKeyword())
            {
                uint color = 0x569cd6ff;
                switch (kind)
                {
                    case SyntaxKind.ReturnKeyword:
                    case SyntaxKind.ThrowKeyword:
                    case SyntaxKind.TryKeyword:
                    case SyntaxKind.BreakKeyword:
                    case SyntaxKind.ContinueKeyword:
                    case SyntaxKind.YieldKeyword:
                    case SyntaxKind.SwitchKeyword:
                    case SyntaxKind.IfKeyword:
                    case SyntaxKind.DoKeyword:
                    case SyntaxKind.WhileKeyword:
                    case SyntaxKind.ForKeyword:
                    case SyntaxKind.ForEachKeyword:
                    case SyntaxKind.ElseKeyword:
                        color = 0xd6a0d2ff;
                        break;
                }
                AddSpan(token, color);
                return;
            }

            switch (kind)
            {
                case SyntaxKind.MultiLineRawStringLiteralToken:
                case SyntaxKind.StringLiteralToken:
                    AddSpan(token, 0xd69d85ff); // String color
                    break;

                case SyntaxKind.VarKeyword:
                    AddSpan(token, 0x569cd6ff);
                    break;

                case SyntaxKind.NumericLiteralToken:
                    AddSpan(token, 0xb5cea8ff);
                    break;

                case SyntaxKind.MultiLineDocumentationCommentTrivia:
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                    AddSpan(token, 0x608b4eff); // Comment color
                    break;
                    // Add more cases for different token types as needed
            }
        }

        private unsafe void AddSpan(SyntaxToken token, ColorRGBA color)
        {
            EmitSpan(token.SpanStart, token.Span.Length, color);
        }

        private unsafe void AddSpan(Microsoft.CodeAnalysis.Text.TextSpan token, ColorRGBA color)
        {
            EmitSpan(token.Start, token.Length, color);
        }
    }
}