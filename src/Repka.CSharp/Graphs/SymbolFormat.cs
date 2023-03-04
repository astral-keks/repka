using Microsoft.CodeAnalysis;

namespace Repka.Graphs
{
    internal static class SymbolFormat
    {
        public static readonly SymbolDisplayFormat Default = new(
            globalNamespaceStyle: SymbolDisplayFormat.CSharpErrorMessageFormat.GlobalNamespaceStyle,
            typeQualificationStyle: SymbolDisplayFormat.CSharpErrorMessageFormat.TypeQualificationStyle,
            genericsOptions: SymbolDisplayFormat.CSharpErrorMessageFormat.GenericsOptions,
            memberOptions: SymbolDisplayFormat.CSharpErrorMessageFormat.MemberOptions,
            delegateStyle: SymbolDisplayFormat.CSharpErrorMessageFormat.DelegateStyle,
            extensionMethodStyle: SymbolDisplayExtensionMethodStyle.StaticMethod,
            parameterOptions: SymbolDisplayFormat.CSharpErrorMessageFormat.ParameterOptions,
            propertyStyle: SymbolDisplayFormat.CSharpErrorMessageFormat.PropertyStyle,
            localOptions: SymbolDisplayFormat.CSharpErrorMessageFormat.LocalOptions,
            kindOptions: SymbolDisplayFormat.CSharpErrorMessageFormat.KindOptions,
            miscellaneousOptions: SymbolDisplayFormat.CSharpErrorMessageFormat.MiscellaneousOptions);
    }
}
