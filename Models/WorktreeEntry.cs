namespace GitTreeManager.Models
{
    /// <summary>worktree 一行配置：路径 + 分支 + user.name + user.email。</summary>
    public class WorktreeEntry
    {
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
