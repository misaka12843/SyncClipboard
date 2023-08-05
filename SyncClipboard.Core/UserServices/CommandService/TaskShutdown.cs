using SyncClipboard.Core.Commons;
using SyncClipboard.Core.Utilities;
using SyncClipboard.Core.Utilities.Notification;

namespace SyncClipboard.Core.UserServices.Command;

public sealed class TaskShutdown
{
    private readonly uint shutdownTime;
    private readonly string tagName = "";
    private const string GROUP_NAME = "Command";
    private Counter? counter;
    private bool canceled = false;
    private static bool IsWorking = false;
    private static readonly object IsWorkingLocker = new();
    private readonly NotificationManager _notificationManager;

    public TaskShutdown(CommandInfo info, NotificationManager notificationManager, uint delay)
    {
        tagName = info.ToString() + DateTime.Now;
        shutdownTime = delay;
        _notificationManager = notificationManager;
    }

    public async Task ExecuteAsync()
    {
        lock (IsWorkingLocker)
        {
            if (IsWorking)
            {
                return;
            }
            IsWorking = true;
        }

        var progressBar = InitToastProgressBar();

        counter = new Counter(
            (count) => progressBar.ForceUpdate((double)count / shutdownTime, $"{shutdownTime - count} 秒后关机"),
            (ulong)shutdownTime * 1000
        );

        await counter.WaitAsync();
        progressBar.Buttons.Clear();
        progressBar.ProgressValueTip = canceled ? "已取消" : "正在关机";
        if (!canceled)
        {
            progressBar.IsIndeterminate = true;
            ShutdowntPc();
        }

        progressBar.Show();
        lock (IsWorkingLocker)
        {
            IsWorking = false;
        }
    }

    private ProgressBar InitToastProgressBar()
    {
        var progressBar = _notificationManager.CreateProgressNotification("远程命令");
        progressBar.Group = GROUP_NAME;
        progressBar.Tag = tagName;
        progressBar.ProgressStatus = "当前状态";
        progressBar.ProgressTitle = "正在关机";
        progressBar.ProgressValue = 0;
        progressBar.ProgressValueTip = "准备开始倒计时";

        progressBar.Buttons.Add(
            new Button("取消关机", new Callbacker($"{tagName}Cancel", _ => { canceled = true; counter?.Cancle(); })));
        progressBar.Buttons.Add(
            new Button("立刻关机", new Callbacker($"{tagName}RightNow", _ => counter?.Cancle())));
        return progressBar;
    }

    private static async void ShutdowntPc()
    {
        await Task.Run(() =>
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = @"/k shutdown.exe /s /t 5 /c ""use [ shutdown /a ] in 5s to undo shutdown.""";
            process.Start();
        });
    }
}