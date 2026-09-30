namespace GitTreeManager.Models
{
    /// <summary>
    /// 表格一行：可能是主仓库（IsMain=true），也可能是一个 worktree（IsMain=false）。
    /// 主仓库：不做 worktree add，config 用 --local（无 --worktree 前缀）。
    /// worktree：走 worktree add + config --worktree。
    /// </summary>
    public class WorktreeEntry
    {
        public bool IsMain { get; set; }
        public string Path { get; set; }
        public string Branch { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrWhiteSpace(Path)
                    && string.IsNullOrWhiteSpace(Branch)
                    && string.IsNullOrWhiteSpace(UserName)
                    && string.IsNullOrWhiteSpace(UserEmail);
            }
        }
    }
}
