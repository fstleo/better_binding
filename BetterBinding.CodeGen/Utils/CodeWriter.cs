#nullable enable

using System;
using System.Text;

namespace BetterBinding.CodeGen.Utils;

public class CodeWriter
{
    private readonly struct IndentScope : IDisposable
    {
        private readonly CodeWriter _source;

        public IndentScope(CodeWriter source)
        {
            _source = source;
            source.IncreaseIndent();
        }

        public void Dispose()
        {
            _source.DecreaseIndent();
        }
    }

    private readonly struct BlockScope : IDisposable
    {
        private readonly CodeWriter _source;
        private readonly char? _appendSemicolon;

        public BlockScope(CodeWriter source, string startLine = "", char? appendSemicolon = null)
        {
            _appendSemicolon = appendSemicolon;
            _source = source;
            source.AppendLine(startLine);
            source.BeginBlock();
        }

        public void Dispose()
        {
            _source.EndBlock(_appendSemicolon);
        }
    }

    private readonly StringBuilder _buffer = new();
    private int _indentLevel;

    public void AppendLine(string value = "")
    {
        if (string.IsNullOrEmpty(value))
        {
            _buffer.AppendLine();
        }
        else
        {
            _buffer.AppendLine($"{new string(' ', _indentLevel * 4)} {value}");
        }
    }

    public override string ToString() => _buffer.ToString();

    public IDisposable BeginIndentScope() => new IndentScope(this);
    public IDisposable BeginBlockScope(string startLine = "", char? appendSymbol = null) => new BlockScope(this, startLine, appendSymbol);

    public void IncreaseIndent()
    {
        _indentLevel++;
    }

    public void DecreaseIndent()
    {
        if (_indentLevel > 0)
            _indentLevel--;
    }

    public void BeginBlock()
    {
        AppendLine("{");
        IncreaseIndent();
    }

    public void EndBlock(char? endSymbol = null)
    {
        DecreaseIndent();
        AppendLine($"}}{endSymbol ?? '\n'}");
    }

    public void Clear()
    {
        _buffer.Clear();
        _indentLevel = 0;
    }
}
