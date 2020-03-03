using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CWTools.Process;
using ICSharpCode.SharpZipLib.Zip;
using Serilog;
using StellarisModManager.PDXModLib.Utilities;
using static CWTools.Parser.Types;
using Position = CWTools.Utilities.Position;

namespace StellarisModManager.PDXModLib.ModData
{
    public class Mod: IDisposable
    {
        private ZipFile _zipFile;
        private string _archive;
        private string _folder;
        private readonly ILogger _logger;

        protected Mod(string id, ILogger logger)
        {
            this.Id = id;
            this._logger = logger.ForContext<Mod>();
        }
        
        public string Id { get; }

        public string Key => $"mod/{this.Id}";

        public string Name { get; set; }

        public List<ModFile> Files { get; } = new List<ModFile>();
        public List<string> Tags { get; } = new List<string>();

        public virtual string FileName => this._archive;

        public virtual string Folder => this._folder;

        public bool ParseError { get; set; }

        public string Description { get; private set; }

        public string PictureName { get; private set; }

        public string RemoteFileId { get; private set; }

        public SupportedVersion SupportedVersion { get; protected set; }

        public static Mod Load(string modDescriptor, ILogger logger)
        {
            var id = Path.GetFileName(modDescriptor);
            var mod = new Mod(id, logger);

            List<string> tags;
            var adapter = CwToolsAdapter.Parse(modDescriptor);
            {
                mod.Name = adapter.Root.Get("name").AsString(); 

                mod._archive = adapter.Root.Get("archive").AsString();
                mod._folder = adapter.Root.Get("path").AsString();

                if (string.IsNullOrEmpty(mod._archive) &&
                    string.IsNullOrEmpty(mod._folder))
                {
                    logger.Debug($"Both archive and folder for {modDescriptor} are empty");
                    mod.Name = id;
                    mod.ParseError = true;
                    return mod;
                }

                mod.PictureName = adapter.Root.Get("picture").AsString();

                tags = adapter.Root.Has("tags") ? adapter.Root.Child("tags").Value.LeafValues.Select(s => s.Value.ToRawString()).ToList() : new List<string>();
               
                mod.SupportedVersion = adapter.Root.Has("supported_version") ? new SupportedVersion(adapter.Root.Get("supported_version").AsString()) : null;
                mod.RemoteFileId = (adapter.Root.Get("remote_file_id").AsString());
            }

            mod.Description = string.Join(", ", tags.Where(t => t != null).ToArray());
            mod.Tags.AddRange(tags.Where(t => t != null));
            return mod;
        }
        
        public void LoadFiles(string basePath)
        {
            var mPath = Path.Combine(basePath, this._archive ?? this._folder);
            if (Path.GetExtension(mPath) == ".zip" && !File.Exists(mPath))
            {
                if (Directory.Exists(Path.GetDirectoryName(mPath))
                    && File.Exists(Path.Combine(Path.GetDirectoryName(mPath), "descriptor.mod")))
                {
                    mPath = Path.GetDirectoryName(mPath);
                    this._archive = null;
                    this._folder = mPath;
                }
            }

            if (Path.GetExtension(mPath) == ".zip")
            {
                this._zipFile = new ZipFile(mPath);

                foreach (var modFile in from item in this._zipFile.OfType<ZipEntry>() let filename = Path.GetFileName(item.Name) where string.Compare(item.Name, "descriptor.mod", StringComparison.OrdinalIgnoreCase) != 0 select ModFile.Load(new ZipFileLoader(this._zipFile, item), item.Name, this))
                {
                    this.Files.Add(modFile);
                }
            }
            else
            {
                mPath += Path.DirectorySeparatorChar;
                var paths = Directory.EnumerateFiles(mPath, "*.*", SearchOption.AllDirectories);
                foreach (var modFile in from path in paths where string.Compare(Path.GetFileName(path), "descriptor.mod", StringComparison.OrdinalIgnoreCase) != 0 let refPath = Uri.UnescapeDataString(new Uri(mPath).MakeRelativeUri(new Uri(path)).OriginalString) select ModFile.Load(new DiskFileLoader(path), refPath, this))
                {
                    this.Files.Add(modFile);
                }

            }
        }


        protected virtual Child ScName => Child.NewLeafC(new Leaf("name", Value.NewQString(this.Name), Position.range.Zero));
        protected virtual Child ScFileName => Child.NewLeafC(new Leaf("archive", Value.NewQString(this.FileName), Position.range.Zero));
        protected virtual Child ScTags => Child.NewNodeC(this.CreateTags());
        protected virtual Child ScSupportedVersion => Child.NewLeafC(new Leaf("supported_version", Value.NewQString(this.SupportedVersion.ToString()), Position.range.Zero));

		private Node CreateTags()
		{
            var result = new Node("tags")
            {
                AllChildren = this.Tags
                    .Select(s => Child.NewLeafValueC(new LeafValue(Value.NewQString(s), Position.range.Zero))).ToList()
            };
            return result;
		}

        public IEnumerable<Child> DescriptorContents => this.ToDescriptor();

        public IEnumerable<Child> ToDescriptor()
        {
            yield return this.ScName;
            yield return this.ScFileName;
            yield return this.ScTags;
            if (!string.IsNullOrEmpty(this.PictureName))
                yield return Child.NewLeafC(new Leaf("picture", Value.NewQString(this.PictureName), Position.range.Zero));

			yield return this.ScSupportedVersion;
        }

        public void Dispose()
        {
            //_zipFile?.Close();
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj as Mod)?.Id == this.Id;
        }

		public override string ToString()
		{
			return this.Name;
		}
	}

    public sealed class SupportedVersion
    {
        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }

        public SupportedVersion(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) source = "*.*.*";
            else source += ".*.*.*";

            var ver = source.Split('.');

            this.Major = ver[0] == "*" ? int.MaxValue : int.Parse(ver[0]);
            this.Minor = ver[1] == "*" ? int.MaxValue : int.Parse(ver[1]);
            this.Patch = ver[2] == "*" ? int.MaxValue : int.Parse(ver[2]);
        }

        public SupportedVersion(int maj, int min, int pat)
        {
            this.Major = maj;
            this.Minor = min;
            this.Patch = pat;
        }

        public static SupportedVersion Combine(IEnumerable<SupportedVersion> source)
        {
            var ma = int.MaxValue;
            var mi = int.MaxValue;
            var pa = int.MaxValue;

            foreach (var s in source)
            {
                if (ma > s.Major)
                {
                    ma = s.Major;
                    mi = int.MaxValue;
                    pa = int.MaxValue;
                }
                else if (ma == s.Major)
                {
                    if (mi > s.Minor)
                    {
                        mi = s.Minor;
                        pa = int.MaxValue;
                    }
                    else
                    {
                        if (pa > s.Patch)
                            pa = s.Patch;
                    }
                }
            }

            return new SupportedVersion(ma, mi, pa);
        }

        public override string ToString()
        {
            var mj = this.Major < int.MaxValue ? this.Major.ToString() : "*";
            var mi = this.Minor < int.MaxValue ? this.Minor.ToString() : "*";
            var pa = this.Patch < int.MaxValue ? this.Patch.ToString() : "*";

            return $"{mj}.{mi}.{pa}";
        }

    }

    public class MergedMod : Mod
    {
        public MergedMod(string name, IEnumerable<ModConflictDescriptor> source, ILogger logger)
            : base("Merge result", logger)
        {
            this.Name = name;

            var listSource = source.ToList();

            this.Tags.AddRange(listSource.Select(mcd => mcd.Mod).SelectMany(m => m.Tags).Distinct());

            this.SupportedVersion = SupportedVersion.Combine(listSource.Select(s => s.Mod.SupportedVersion));

            var distinctConflicts = listSource.SelectMany(mcd => mcd.FileConflicts).Distinct();

            foreach (var conflict in distinctConflicts)
            {
                this.Files.Add(!conflict.ConflictingModFiles.Any()
                    ? conflict.File
                    : new MergedModFile(conflict.File.Path, conflict.ConflictingModFiles.Concat(new[] {conflict.File}),
                        this));
            }
        }

        public override string FileName => this.Name;

        protected override Child ScFileName => Child.NewLeafC(new Leaf("path", Value.NewQString($"mod/{this.FileName}"), Position.range.Zero));

        public ModConflictDescriptor ToModConflictDescriptor()
        {
            return new ModConflictDescriptor(this, this.Files.Select(ToConflictDescriptor));
        }

        private static ModFileConflictDescriptor ToConflictDescriptor(ModFile file)
        {
            if (file is MergedModFile mmf)
            {
                return new ModFileConflictDescriptor(mmf, mmf.SourceFiles);
            }

            return new ModFileConflictDescriptor(file, Enumerable.Empty<ModFile>());
        }
    }
}