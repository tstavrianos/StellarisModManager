using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PDXModLib.Utility;
using static CWTools.Parser.Types;
using CWTools.Process;
using Position = CWTools.Utilities.Position;
using ICSharpCode.SharpZipLib.Zip;
using Serilog.Core;

namespace PDXModLib.ModData
{
    using System.Text;

    using Serilog;
    using Serilog.Exceptions;

    public class Mod : IDisposable
    {
        private ZipFile _zipFile;

        private static readonly Logger Log;

        static Mod()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

#if DEBUG
            Log = new LoggerConfiguration()//
                .MinimumLevel.Debug()//
                .Enrich.WithExceptionDetails()//
                .Enrich.FromLogContext()//
                .WriteTo.File("Mod.log")//
                .CreateLogger();//
#endif
        }

        private string _archive;

        private string _folder;

        protected Mod(string id)
        {
            this.Id = id;
        }

        public string Id { get; }

        public string Key => $"mod/{this.Id}";

        private string _name;

        public string Name
        {
            get => this._name;
            set => this._name = value;
        }

        public List<ModFile> Files { get; } = new List<ModFile>();

        public List<string> Tags { get; } = new List<string>();
        public List<string> Dependencies { get; } = new List<string>();

        public virtual string FileName => this._archive;

        public virtual string Folder => this._folder;

        public bool ParseError { get; set; }

        public string Description { get; private set; }

        private string _pictureName;

        public string PictureName
        {
            get => this._pictureName;
            private set => this._pictureName = value;
        }

        private string _remoteFileId;

        public string RemoteFileId
        {
            get => this._remoteFileId;
            private set => this._remoteFileId = value;
        }

        public SupportedVersion SupportedVersion { get; protected set; }

        public bool Valid { get; private set; }

        public static Mod Load(string modDescriptor)
        {
            var id = Path.GetFileName(modDescriptor);
            var mod = new Mod(id);

            IEnumerable<string> tags = null;
            IEnumerable<string> dependencies = null;
            var adapter = CWToolsAdapter.Parse(modDescriptor);

            {
                adapter.Root.TryGetString("name", ref mod._name);
                adapter.Root.TryGetString("archive", ref mod._archive);
                adapter.Root.TryGetString("path", ref mod._folder);


                if (string.IsNullOrEmpty(mod._archive) &&
                    string.IsNullOrEmpty(mod._folder))
                {
                    Log?.Debug($"Both archive and folder for {modDescriptor} are empty");
                    mod.Name = id;
                    mod.ParseError = true;
                    return mod;
                }

                adapter.Root.TryGetString("picture", ref mod._pictureName);
                adapter.Root.TryGetStrings("tags", ref tags);
                adapter.Root.TryGetStrings("dependencies", ref dependencies);

                mod.SupportedVersion = adapter.Root.Exists("supported_version") ? new SupportedVersion(adapter.Root.Get("supported_version").AsString()) : new SupportedVersion(0, 0, 0);
                adapter.Root.TryGetString("remote_file_id", ref mod._remoteFileId);
            }

            if (tags != null)
            {
                mod.Description = string.Join(", ", tags);
                mod.Tags.AddRange(tags);
            }

            if (dependencies != null)
            {
                mod.Dependencies.AddRange(dependencies);
            }
            return mod;
        }

        public void LoadFiles(string basePath)
        {
            this.Valid = true;
            if (string.IsNullOrWhiteSpace(this.Folder) && string.IsNullOrWhiteSpace(this.FileName))
            {
                this.Valid = false;
                Log?.Debug($"{this.Id} - Path and Archive are blank");
            }

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

            if (Path.GetExtension(mPath) == ".zip" && !File.Exists(mPath))
            {
                this.Valid = false;
                Log?.Debug($"{this.Id} - Archive does not exist");
            }
            else if (Path.GetExtension(mPath) != ".zip" && !Directory.Exists(mPath))
            {
                this.Valid = false;
                Log?.Debug($"{this.Id} - Path does not exist");
            }

            if (!this.Valid) return;

            if (Path.GetExtension(mPath) == ".zip")
            {
                this._zipFile = new ZipFile(mPath);

                foreach (var item in this._zipFile.OfType<ZipEntry>())
                {
                    var filename = Path.GetFileName(item.Name);
                    if (string.Compare(item.Name, "descriptor.mod", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        continue;
                    }

                    var modFile = ModFile.Load(new ZipFileLoader(this._zipFile, item), item.Name, this);
                    this.Files.Add(modFile);
                }
            }
            else
            {
                mPath = mPath + Path.DirectorySeparatorChar;
                var paths = Directory.EnumerateFiles(mPath, "*.*", SearchOption.AllDirectories);
                foreach (var path in paths)
                {
                    if (string.Compare(Path.GetFileName(path), "descriptor.mod", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        var refPath = Uri.UnescapeDataString(new Uri(mPath).MakeRelativeUri(new Uri(path)).OriginalString);
                        var modFile = ModFile.Load(new DiskFileLoader(path), refPath, this);
                        this.Files.Add(modFile);
                    }
                }
            }

            var scFiles = this.Files.Where(x => (x is SCModFile a) && a.ParseError).Cast<SCModFile>().ToArray();
            if (scFiles.Length > 0)
            {
                this.Valid = false;
                Log?.Debug($"{this.Id} - Files with errors");
                foreach (var x in scFiles)
                {
                    Log?.Debug($"{this.Id} - {x.Path} - {x.ParseErrorMessage}");
                }
            }
        }

        protected virtual Child SCName => Child.NewLeafC(new Leaf("name", Value.NewQString(this.Name), Position.range.Zero));
        protected virtual Child SCFileName => Child.NewLeafC(new Leaf("archive", Value.NewQString(this.FileName), Position.range.Zero));
        protected virtual Child SCTags => Child.NewNodeC(this.CreateTags());
        protected virtual Child SCSupportedVersion => Child.NewLeafC(new Leaf("supported_version", Value.NewQString(this.SupportedVersion.ToString()), Position.range.Zero));

        private Node CreateTags()
        {
            var result = new Node("tags");
            result.AllChildren = this.Tags.Select(s => Child.NewLeafValueC(new LeafValue(Value.NewQString(s), Position.range.Zero))).ToList();
            return result;
        }

        public IEnumerable<Child> DescriptorContents => this.ToDescriptor();

        public IEnumerable<Child> ToDescriptor()
        {
            yield return this.SCName;
            yield return this.SCFileName;
            yield return this.SCTags;
            if (!string.IsNullOrEmpty(this.PictureName))
                yield return Child.NewLeafC(new Leaf("picture", Value.NewQString(this.PictureName), Position.range.Zero));

            yield return this.SCSupportedVersion;
        }

        public void Dispose()
        {
            this._zipFile?.Close();
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

    public class SupportedVersion
    {
        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }

        public SupportedVersion(string source)
        {
            var ver = source.Split('.');

            this.Major = int.MaxValue;
            this.Minor = int.MaxValue;
            this.Patch = int.MaxValue;

            this.Major = ver[0] == "*" ? int.MaxValue : int.Parse(ver[0]);
            if (ver.Length > 1)
            {
                this.Minor = ver[1] == "*" ? int.MaxValue : int.Parse(ver[1]);
                if (ver.Length > 2)
                    this.Patch = ver[2] == "*" ? int.MaxValue : int.Parse(ver[2]);
            }
        }

        public SupportedVersion(int maj, int min, int pat)
        {
            this.Major = maj;
            this.Minor = min;
            this.Patch = pat;
        }

        public static SupportedVersion Combine(IEnumerable<SupportedVersion> source)
        {
            int ma = int.MaxValue;
            int mi = int.MaxValue;
            int pa = int.MaxValue;

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
        public MergedMod(string name, IEnumerable<ModConflictDescriptor> source)
            : base($"Merge result")
        {
            this.Name = name;

            var listSource = source.ToList();

            this.Tags.AddRange(listSource.Select(mcd => mcd.Mod).SelectMany(m => m.Tags).Distinct());

            this.SupportedVersion = SupportedVersion.Combine(listSource.Select(s => s.Mod.SupportedVersion));

            var distinctConflicts = listSource.SelectMany(mcd => mcd.FileConflicts).Distinct();

            foreach (var conflict in distinctConflicts)
            {
                if (!conflict.ConflictingModFiles.Any())
                {
                    this.Files.Add(conflict.File);
                }
                else
                {
                    this.Files.Add(new MergedModFile(conflict.File.Path, conflict.ConflictingModFiles.Concat(new[] { conflict.File }), this));
                }
            }
        }

        public override string FileName => this.Name;

        protected override Child SCFileName => Child.NewLeafC(new Leaf("path", Value.NewQString($"mod/{this.FileName}"), Position.range.Zero));

        public ModConflictDescriptor ToModConflictDescriptor()
        {
            return new ModConflictDescriptor(this, this.Files.Select(this.ToConflictDescriptor));
        }

        ModFileConflictDescriptor ToConflictDescriptor(ModFile file)
        {
            var mmf = file as MergedModFile;
            if (mmf != null)
            {
                return new ModFileConflictDescriptor(mmf, mmf.SourceFiles);
            }

            return new ModFileConflictDescriptor(file, Enumerable.Empty<ModFile>());
        }
    }
}