using System;
using System.Collections.Generic;
using System.IO;

namespace Stellaris.Data.Parser
{
    public sealed class SimpleModParser: IParser
    {
        private int _pos = 0;
        private string _text;
        
        public IReadOnlyDictionary<string, IParsedEntry> Parse(string file)
        {
            this._pos = 0;
            this._text = File.ReadAllText(file);
            var ret = new Dictionary<string, IParsedEntry>();

            var expectingName = true;
            var expectingEqual = false;
            var lastName = string.Empty;
            while (this._pos < this._text.Length)
            {
                var c = this._text[this._pos];

                if (expectingName)
                {
                    lastName = this.GetName();
                    expectingName = false;
                    expectingEqual = true;
                } 
                else if (expectingEqual)
                {
                    this.Bypass();

                    if (c == '=')
                    {
                        expectingEqual = false;
                        this._pos++;
                    }
                    else
                    {
                        Mod.Logger.Error($"Unexpected character: {c}");
                        throw new Exception();
                    }
                }
                else if (c == '"')
                {
                    var value = this.GetString();
                    var entry = new ParsedStringEntry(lastName, value);
                    ret.Add(lastName, entry);
                    lastName = string.Empty;
                    this.Bypass();
                    expectingName = true;
                } 
                else if (c == '{')
                {
                    var value = this.GetList();
                    var entry = new ParsedListEntry(lastName, value);
                    ret.Add(lastName, entry);
                    lastName = string.Empty;
                    this.Bypass();
                    expectingName = true;
                }
                else
                {
                    this._pos++;
                }
            }

            return ret;        
        }
        
        private string GetName()
        {
            var c = this._text[this._pos];
            var ret = string.Empty;

            while (c == '_' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                ret += c;
                this._pos++;
                if(this._pos >= this._text.Length) break;
                c = this._text[this._pos];
            }

            this.Bypass();

            return ret;
        }

        private string GetString()
        {
            var c = this._text[this._pos];
            if (c != '"' || this._pos == this._text.Length - 1) return string.Empty;
            var ret = string.Empty;
            this._pos++;
            c = this._text[this._pos];
            while (c != '"' && c != '\n')
            {
                ret += c;
                this._pos++;
                if(this._pos >= this._text.Length) break;
                c = this._text[this._pos];
            }

            if (c == '"') this._pos++;
            this.Bypass();

            return ret;
        }

        private IList<string> GetList()
        {
            var ret = new List<string>();

            var c = this._text[this._pos];
            if (c != '{' || this._pos >= this._text.Length - 1) return ret;
            this._pos++;
            while (this._text[this._pos] != '}')
            {
                this.Bypass();
                var value = this.GetString();
                ret.Add(value);
            }

            if (this._pos <= this._text.Length - 1 && this._text[this._pos] == '}') this._pos++;
            return ret;
        }

        private void Bypass()
        {
            if (this._pos >= this._text.Length) return;
            var c = this._text[this._pos];
            while (c == ' ' || c == '\t' || c == '\n' || c == '\r')
            {
                this._pos++;
                if (this._pos >= this._text.Length) break;
                c = this._text[this._pos];
            }
        }
    }
}