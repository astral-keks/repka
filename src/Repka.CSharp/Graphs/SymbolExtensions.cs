﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Repka.Graphs
{
    internal static class SymbolExtensions
    {
        public static ISymbol? GetDeclaredSymbol(this CSharpCompilation compilation, SyntaxNode syntax)
        {
            var semantic = compilation.GetSemanticModel(syntax.SyntaxTree, ignoreAccessibility: true);
            ISymbol? symbol = semantic.GetDeclaredSymbol(syntax);
            return symbol;
        }

        public static ISymbol? GetSymbol(this CSharpCompilation compilation, SyntaxNode syntax)
        {
            var semantic = compilation.GetSemanticModel(syntax.SyntaxTree, ignoreAccessibility: true);
            SymbolInfo symbolInfo = semantic.GetSymbolInfo(syntax);
            return symbolInfo.Symbol;
        }

        public static SyntaxNode? GetSyntaxRoot(this Document document)
        {
            return document.GetSyntaxRootAsync().GetAwaiter().GetResult();
        }

        public static SemanticModel? GetSemanticModel(this Document document)
        {
            return document.GetSemanticModelAsync().GetAwaiter().GetResult();
        }

        public static IEnumerable<INamedTypeSymbol> GetRefererTypes(this ISymbol symbol, Solution solution)
        {
            IEnumerable<ReferencedSymbol> references = SymbolFinder.FindReferencesAsync(symbol, solution)
                .GetAwaiter().GetResult();
            foreach (var reference in references)
            {
                INamedTypeSymbol? refererSymbol = reference.Definition.ContainingType;
                if (refererSymbol is not null)
                {
                    yield return refererSymbol;
                }
            }
        }
    }
}
