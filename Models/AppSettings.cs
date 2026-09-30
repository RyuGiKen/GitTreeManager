using System.Collections.Generic;

namespace GitTreeManager.Models
{
    /// <summary>仓库根的处理模式。</summary>
    public enum RepoMode
    {
        /// <summary>在给定路径 git init。</summary>
        New,
        /// <summary>从远程 URL git clone 到给定路径。</summary>
        Clone
    }

    /// <summary>持久化的应用配置（对应 settings.xml）。</summary>
    public class AppSettings
    {
        public string RepoPath { get; set; }
        public RepoMode Mode { get; set; }
        public string RemoteUrl { get; set; }
        public string DefaultBranch { get; set; }
        public string GitExe { get; set; }
        public bool DryRun { get; set; }
        public List<WorktreeEntry> Entries { get; set; }

        public AppSettings()
        {
            Mode = RepoMode.New;
            DefaultBranch = "main";
            GitExe = "git";
            DryRun = true;
            Entries = new List<WorktreeEntry>();
        }
    }
}
