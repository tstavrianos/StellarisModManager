using System;
using System.Collections.Generic;
using System.IO;
using Pdoxcl2Sharp;
using Serilog;
using Serilog.Core;

namespace Stellaris.Data
{
    public class Mod : IParadoxRead, IParadoxWrite
    {
        internal static readonly Logger Logger;

        static Mod()
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("parser.log")
                .CreateLogger();
        }
        
        public string Id { get; }
        public string Name { get; private set; }
        public string Picture { get; private set; }
        public string SupportedVersion { get; private set; }
        public string Path { get; private set; }
        public string Archive { get; private set; }
        public long RemoteFileId { get; private set; }
        public string Version { get; private set; }
        public IList<string> Tags { get; private set; }
        public IList<string> Dependencies { get; private set; }
        public string Key => $"mod/{this.Id}";
        public bool Valid { get; private set; }
        public string File { get; }

        public Mod(string file)
        {
            this.File = file;
            this.Id = System.IO.Path.GetFileName(file);
            this.Valid = true;
        }
        
        public void TokenCallback(ParadoxParser parser, string token)
        {
            switch (token)
            {
                case "name": this.Name = parser.ReadString(); break;
                case "picture": this.Picture = parser.ReadString(); break;
                case "supported_version": this.SupportedVersion = parser.ReadString(); break;
                case "path": this.Path = parser.ReadString(); break;
                case "archive": this.Archive = parser.ReadString(); break;
                case "remote_file_id": 
                    var id = parser.ReadString();
                    if (!long.TryParse(id, out var idLong))
                    {
                        this.RemoteFileId = -1;
                    }
                    else
                    {
                        this.RemoteFileId = idLong;
                    }

                    if (this.RemoteFileId < 0) this.Valid = false;
                    break;
                case "version": this.Version = parser.ReadString(); break;
                case "tags": this.Tags = parser.ReadStringList(); break;
                case "dependencies": this.Dependencies = parser.ReadStringList(); break;
                default:
                    Logger.Information(token);
                    break;
            }
        }
        
        public void Write(ParadoxStreamWriter writer)
        {
            if (this.Name != null)
            {
                writer.WriteLine("name", this.Name, ValueWrite.Quoted);
            }        
            if (this.Picture != null)
            {
                writer.WriteLine("picture", this.Picture, ValueWrite.Quoted);
            }        
            if (this.SupportedVersion != null)
            {
                writer.WriteLine("supported_version", this.SupportedVersion, ValueWrite.Quoted);
            }        
            if (this.Path != null)
            {
                writer.WriteLine("path", this.Path, ValueWrite.Quoted);
            }        
            if (this.Archive != null)
            {
                writer.WriteLine("archive", this.Archive, ValueWrite.Quoted);
            }        
            if (this.RemoteFileId > 0)
            {
                writer.WriteLine("remote_file_id", $"{this.RemoteFileId}", ValueWrite.Quoted);
            }        
            if (this.Version != null)
            {
                writer.WriteLine("version", this.Version, ValueWrite.Quoted);
            }        
            if (this.Tags != null)
            {
                writer.Write("tags={ ");
                foreach (var val in this.Tags)
                {
                    writer.Write(val, ValueWrite.Quoted);
                    writer.Write(" ");
                }
                writer.WriteLine("}");
            }
            if (this.Dependencies != null)
            {
                writer.Write("dependencies={ ");
                foreach (var val in this.Dependencies)
                {
                    writer.Write(val, ValueWrite.Quoted);
                    writer.Write(" ");
                }
                writer.WriteLine("}");
            }
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Path) && string.IsNullOrWhiteSpace(this.Archive)) this.Valid = false;
            var basePath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(this.File));
            var mPath = System.IO.Path.Combine(basePath, this.Archive ?? this.Path);
            if (System.IO.Path.GetExtension(mPath) == ".zip" && !System.IO.File.Exists(mPath))
            {
                if (Directory.Exists(System.IO.Path.GetDirectoryName(mPath))
                    && System.IO.File.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(mPath), "descriptor.mod")))
                {
                    mPath = System.IO.Path.GetDirectoryName(mPath);
                    this.Archive = null;
                    this.Path = mPath;
                }
            }
            
            if (System.IO.Path.GetExtension(mPath) == ".zip" && !System.IO.File.Exists(mPath))
            {
                this.Valid = false;
            }
            else if (System.IO.Path.GetExtension(mPath) != ".zip" && !Directory.Exists(mPath))
            {
                this.Valid = false;
            }
        }
    }
}