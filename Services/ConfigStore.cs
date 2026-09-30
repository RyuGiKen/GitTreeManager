using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using GitTreeManager.Models;

namespace GitTreeManager.Services
{
    /// <summary>
    /// 读写 settings.xml。放在 exe 同目录（沿用你现有 C4D 工具的做法）。
    /// 结构：<settings><Repo/><Worktrees><Entry .../></Worktrees><Options/></settings>。
    /// </summary>
    public class ConfigStore
    {
        private readonly string _path;

        public ConfigStore() : this(DefaultPath()) { }

        public ConfigStore(string path)
        {
            _path = path;
        }

        private static string DefaultPath()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(dir)) dir = Path.GetDirectoryName(typeof(ConfigStore).Assembly.Location) ?? ".";
            return Path.Combine(dir, "settings.xml");
        }

        public AppSettings Load()
        {
            if (!File.Exists(_path)) return new AppSettings();
            XDocument doc;
            try { doc = XDocument.Load(_path); }
            catch { return new AppSettings(); }
            var root = doc.Root;
            if (root == null) return new AppSettings();

            var s = new AppSettings();
            var repo = root.Element("Repo");
            if (repo != null)
            {
                s.RepoPath = (string)repo.Attribute("path") ?? "";
                var mode = (string)repo.Attribute("mode");
                s.Mode = string.Equals(mode, "clone", StringComparison.OrdinalIgnoreCase) ? RepoMode.Clone : RepoMode.New;
                s.RemoteUrl = (string)repo.Attribute("remote") ?? "";
                s.DefaultBranch = (string)repo.Attribute("defaultBranch");
                if (string.IsNullOrEmpty(s.DefaultBranch)) s.DefaultBranch = "main";
            }
            var opts = root.Element("Options");
            if (opts != null)
            {
                s.GitExe = (string)opts.Attribute("gitExe");
                if (string.IsNullOrEmpty(s.GitExe)) s.GitExe = "git";
                var dry = (string)opts.Attribute("dryRun");
                s.DryRun = !string.Equals(dry, "false", StringComparison.OrdinalIgnoreCase);
            }
            var wts = root.Element("Worktrees");
            if (wts != null)
            {
                var list = new List<WorktreeEntry>();
                foreach (var el in wts.Elements("Entry"))
                {
                    list.Add(new WorktreeEntry
                    {
                        Path = (string)el.Attribute("path") ?? "",
                        Branch = (string)el.Attribute("branch") ?? "",
                        UserName = (string)el.Attribute("name") ?? "",
                        UserEmail = (string)el.Attribute("email") ?? ""
                    });
                }
                s.Entries = list;
            }
            return s;
        }

        public void Save(AppSettings s)
        {
            if (s == null) return;
            var doc = new XDocument(
                new XElement("settings",
                    new XAttribute("version", "1"),
                    new XElement("Repo",
                        new XAttribute("path", s.RepoPath ?? ""),
                        new XAttribute("mode", s.Mode == RepoMode.Clone ? "clone" : "new"),
                        new XAttribute("remote", s.RemoteUrl ?? ""),
                        new XAttribute("defaultBranch", s.DefaultBranch ?? "main")),
                    new XElement("Options",
                        new XAttribute("gitExe", s.GitExe ?? "git"),
                        new XAttribute("dryRun", s.DryRun ? "true" : "false")),
                    new XElement("Worktrees",
                        ToEntryElements(s.Entries))));

            string tmp = _path + ".tmp";
            doc.Save(tmp);
            // 覆盖写：File.Move 目标已存在会抛，用 Copy(true) 再删 tmp
            File.Copy(tmp, _path, true);
            try { File.Delete(tmp); } catch { }
        }

        private static IEnumerable<XElement> ToEntryElements(List<WorktreeEntry> entries)
        {
            if (entries == null) yield break;
            foreach (var en in entries)
            {
                if (en == null || en.IsEmpty) continue;
                yield return new XElement("Entry",
                    new XAttribute("path", en.Path ?? ""),
                    new XAttribute("branch", en.Branch ?? ""),
                    new XAttribute("name", en.UserName ?? ""),
                    new XAttribute("email", en.UserEmail ?? ""));
            }
        }
    }
}
