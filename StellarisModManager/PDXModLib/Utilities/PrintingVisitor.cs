using System;
using System.Text;
using CWTools.ExtensionPoints;
using CWTools.Process;

namespace StellarisModManager.PDXModLib.Utilities
{
    public sealed class PrintingVisitor : StatementVisitor
	{
		private const int SpacesPerIndent = 4;
		private readonly int _indentLevel;
		private readonly IndentingPersister _persistent;

		public PrintingVisitor()
		{
			this._indentLevel = -1;
			this._persistent = new IndentingPersister();
		}

		private PrintingVisitor(int indentLevel, IndentingPersister persistentData)
		{
			this._indentLevel = indentLevel;
			this._persistent = persistentData;
		}

		public override void Visit(Node value)
		{
			if (this._indentLevel >= 0)
			{
				if (value.Key != null)
				{
					this._persistent.Append(this._indentLevel, value.Key);
					this._persistent.Append(" = {");
				}
				else
					this._persistent.Append(this._indentLevel, "{");

				if (value.All.Length == 0)
				{
					this._persistent.Append("}");
					return;
				}
				this._persistent.AppendLine();
			}


			var inner = new PrintingVisitor(this._indentLevel + 1, this._persistent);

			foreach (var child in value.All)
			{
				inner.Visit(child);
				this._persistent.AppendLine();
			}

			if (this._indentLevel >= 0)
				this._persistent.Append(this._indentLevel, "}");

		}

		public override void Visit(Leaf value)
		{
			this._persistent.Append(this._indentLevel, value.Key);
			this._persistent.Append(" = ");
			this._persistent.Append(value.Value.ToString());
		}

		public override void Visit(LeafValue value)
		{
			this._persistent.Append(this._indentLevel, value.Value.ToString());
		}

		public override void Visit(string comment)
		{
			this._persistent.Append(this._indentLevel, $"#{comment}");
			this._persistent.AppendLine();
		}

		public string Result => this._persistent.GetResult();

		private sealed class IndentingPersister
		{
			private string[] _indents;
			private StringBuilder Builder { get; }
			public IndentingPersister()
			{
				this._indents = new string[0];
				this.Builder = new StringBuilder();
			}

			private string GetIndent(int level)
			{
				if (this._indents.Length <= level)
				{
					Array.Resize(ref this._indents, level + 1);
				}

				return this._indents[level] ?? (this._indents[level] = new string(' ', SpacesPerIndent * level));
			}

			public void Append(string value)
			{
				this.Builder.Append(value);
			}

			public void Append(int indent, string value)
			{
				this.Builder.Append(this.GetIndent(indent));
				this.Builder.Append(value);
			}

			public void AppendLine()
			{
				this.Builder.AppendLine();
			}

			public string GetResult() => this.Builder.ToString();
		}
	}
}