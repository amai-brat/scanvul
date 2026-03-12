using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ScanVul.Server.Domain.Cve.ValueObjects.Versions;

public sealed class SoftwareVersion : IComparable<SoftwareVersion>, IEquatable<SoftwareVersion>
{
    private readonly string _originalString;
    public ulong Epoch { get; }
    private readonly IReadOnlyList<Token> _tokens;

    private SoftwareVersion(string original, ulong epoch, IReadOnlyList<Token> tokens)
    {
        _originalString = original;
        Epoch = epoch;
        _tokens = tokens;
    }

    public static bool TryParse(string? version, [NotNullWhen(true)] out SoftwareVersion? output)
    {
        output = null;
        if (string.IsNullOrWhiteSpace(version)) return false;

        var parseString = version.Trim();
        ulong epoch = 0;
        var startIndex = 0;

        var colonIndex = parseString.IndexOf(':');
        if (colonIndex > 0)
        {
            var isValidEpoch = true;
            for (var i = 0; i < colonIndex; i++)
            {
                if (char.IsDigit(parseString[i])) continue;
                
                isValidEpoch = false;
                break;
            }

            if (isValidEpoch && ulong.TryParse(parseString.AsSpan(0, colonIndex), out var parsedEpoch))
            {
                epoch = parsedEpoch;
                startIndex = colonIndex + 1;
            }
        }
        
        var tokens = new List<Token>();
        var currentNum = new StringBuilder();
        var currentStr = new StringBuilder();

        for (var i = startIndex; i < parseString.Length; i++)
        {
            var ch = parseString[i];
            if (char.IsDigit(ch))
            {
                FlushStr();
                currentNum.Append(ch);
            }
            else if (char.IsLetter(ch))
            {
                FlushNum();
                currentStr.Append(ch);
            }
            else if (ch == '~')
            {
                FlushNum();
                FlushStr();
                tokens.Add(new Token("~"));
            }
            else
            {
                FlushNum();
                FlushStr();
            }
        }

        FlushNum();
        FlushStr();

        if (tokens.Count == 0 && epoch == 0) return false;

        output = new SoftwareVersion(version, epoch, tokens);
        return true;

        void FlushStr()
        {
            if (currentStr.Length <= 0) return;
            
            tokens.Add(new Token(currentStr.ToString()));
            currentStr.Clear();
        }

        void FlushNum()
        {
            if (currentNum.Length <= 0) return;
            
            tokens.Add(ulong.TryParse(currentNum.ToString(), out var res)
                ? new Token(res)
                : new Token(currentNum.ToString()));

            currentNum.Clear();
        }
    }

    public int CompareTo(SoftwareVersion? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;

        var epochCmp = Epoch.CompareTo(other.Epoch);
        if (epochCmp != 0) return epochCmp;
        
        var maxCount = Math.Max(_tokens.Count, other._tokens.Count);
        for (var i = 0; i < maxCount; i++)
        {
            var tokA = i < _tokens.Count ? _tokens[i] : Token.None;
            var tokB = i < other._tokens.Count ? other._tokens[i] : Token.None;

            var cmp = Token.Compare(tokA, tokB);
            if (cmp != 0) return cmp;
        }

        return 0;
    }

    public override bool Equals(object? obj) => Equals(obj as SoftwareVersion);
    public bool Equals(SoftwareVersion? other) => other is not null && CompareTo(other) == 0;

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Epoch);

        var lastSignificant = -1;
        for (var i = 0; i < _tokens.Count; i++)
        {
            if (_tokens[i].Type != TokenType.Number || _tokens[i].NumVal != 0)
                lastSignificant = i;
        }

        for (var i = 0; i <= lastSignificant; i++)
            hash.Add(_tokens[i]);

        return hash.ToHashCode();
    }

    public override string ToString() => _originalString;
    public static bool operator ==(SoftwareVersion? left, SoftwareVersion? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(SoftwareVersion? left, SoftwareVersion? right) => !(left == right);
    public static bool operator <(SoftwareVersion? left, SoftwareVersion? right) => left is null ? right is null : left.CompareTo(right) < 0;
    public static bool operator <=(SoftwareVersion? left, SoftwareVersion? right) => left is null || left.CompareTo(right) <= 0;
    public static bool operator >(SoftwareVersion? left, SoftwareVersion? right) => left is not null && left.CompareTo(right) > 0;
    public static bool operator >=(SoftwareVersion? left, SoftwareVersion? right) => left is null ? right is null : left.CompareTo(right) >= 0;

    private enum TokenType { None, Number, String }

    private readonly struct Token : IEquatable<Token>
    {
        public readonly TokenType Type;
        public readonly ulong NumVal;
        public readonly string? StrVal;

        public static readonly Token None = new();

        public Token() { Type = TokenType.None; NumVal = 0; StrVal = null; }
        public Token(ulong num) { Type = TokenType.Number; NumVal = num; StrVal = null; }
        public Token(string str) { Type = TokenType.String; NumVal = 0; StrVal = str; }

        public static int Compare(Token x, Token y)
        {
            if (x.Type == y.Type)
            {
                if (x.Type == TokenType.None) return 0;
                if (x.Type == TokenType.Number) return x.NumVal.CompareTo(y.NumVal);
                if (x.Type == TokenType.String)
                {
                    var wx = GetStringWeight(x.StrVal!);
                    var wy = GetStringWeight(y.StrVal!);
                    if (wx != wy) return wx.CompareTo(wy);
                    
                    return string.Compare(x.StrVal, y.StrVal, StringComparison.OrdinalIgnoreCase);
                }
            }

            // None vs Number: Treat None equivalently to Number(0)
            if (x.Type == TokenType.None && y.Type == TokenType.Number) return y.NumVal == 0 ? 0 : -1;
            if (y.Type == TokenType.None && x.Type == TokenType.Number) return x.NumVal == 0 ? 0 : 1;

            // None vs String: Depends on the weight (Pre-release vs Post-release)
            if (x.Type == TokenType.None && y.Type == TokenType.String) return GetStringWeight(y.StrVal!) < 0 ? 1 : -1;
            if (y.Type == TokenType.None && x.Type == TokenType.String) return GetStringWeight(x.StrVal!) < 0 ? -1 : 1;

            // Number vs String
            // A number (even 0) behaves identical to 'None' against strings.
            // But a number > 0 always beats any string (e.g., 1.1 > 1.patch).
            if (x.Type == TokenType.Number && y.Type == TokenType.String)
            {
                if (x.NumVal == 0) return GetStringWeight(y.StrVal!) < 0 ? 1 : -1;
                return 1;
            }
            if (y.Type == TokenType.Number && x.Type == TokenType.String)
            {
                if (y.NumVal == 0) return GetStringWeight(x.StrVal!) < 0 ? -1 : 1;
                return -1;
            }

            return 0;
        }

        private static int GetStringWeight(string s)
        {
            if (s == "~") return -2; // Hard Debian rule: ~ sorts before EVERYTHING

            s = s.ToLowerInvariant();

            switch (s)
            {
                // Weight -1: Standard Pre-releases (sort before 'None'/'Number(0)')
                case "alpha" or "a":
                case "beta" or "b":
                case "rc" or "c":
                case "dev" or "pre" or "preview" or "test":
                    return -1;
                default:
                    // Weight +1: Post-releases, Patches, Unknown Strings (e.g., OpenSSH 'p', 'patch', 'post')
                    // These sort AFTER 'None'
                    return 1;
            }
        }

        public override string ToString() => Type switch
        {
            TokenType.None => "None",
            TokenType.Number => $"Number: {NumVal}",
            TokenType.String => $"String: {StrVal}",
            _ => "el psy kongroo"
        };

        public bool Equals(Token other) => Compare(this, other) == 0;
        public override bool Equals(object? obj) => obj is Token t && Equals(t);
        public override int GetHashCode() => HashCode.Combine(Type, NumVal, StrVal?.ToLowerInvariant());
    }
}