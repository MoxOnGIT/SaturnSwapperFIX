
namespace Radon.CodeAnalysis.Binding.Semantics;

internal enum BoundNodeKind
{
    AssignmentExpression,
    BinaryExpression,
    ImportExpression,
    InvocationExpression,
    LiteralExpression,
    MemberAccessExpression,
    NewExpression,
    ThisExpression,
    UnaryExpression,
    ErrorExpression,
    BlockStatement,
    ExpressionStatement,
    InvalidStatement,
    SignStatement,
    VariableDeclarationStatement,
    Struct,
    Method,
    Field,
    Constructor,
    NameExpression,
    ErrorMember,
    ErrorStatement,
    ErrorType,
    ErrorNode,
    Assembly,
    ConversionExpression,
    Enum,
    EnumMember,
    DefaultExpression,
    Template,
    TemplateMethod,
    NewArrayExpression,
    ElementAccessExpression,
    ReturnStatement
}