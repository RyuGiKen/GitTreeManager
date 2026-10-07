using System;
using System.Threading;
using System.Windows.Forms;

namespace GitTreeManager
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            // 全局兜底：UI 线程异常弹框，非 UI 线程异常至少写控制台，避免"程序闪退且用户不知道为什么"
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnUiThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                // Application.Run 之外的启动期异常（如 MainForm 构造函数炸）
                MessageBox.Show("启动失败：" + ex.Message + "\n\n" + ex.StackTrace,
                    "GitTreeManager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void OnUiThreadException(object sender, ThreadExceptionEventArgs e)
        {
            try
            {
                MessageBox.Show(
                    "界面线程异常：\n" + e.Exception.Message + "\n\n" + e.Exception.GetType().Name,
                    "GitTreeManager - 已捕获",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch { }
        }

        private static void OnDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = e.ExceptionObject as Exception;
                MessageBox.Show(
                    "非界面线程未处理异常：" + (ex != null ? ex.Message : "未知") +
                    "\n\nIsTerminating=" + e.IsTerminating,
                    "GitTreeManager - 致命",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
        }
    }
}
