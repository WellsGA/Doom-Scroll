using Doom_Scroll;
using System;
using System.Linq;
using System.Text;

// Token: 0x0200062D RID: 1581
public enum CustomTaskTypes
{
    CreateHeadlineTask
}
public class HeadlineTask : NormalPlayerTask
{
    public override void Initialize()
    {
        base.Initialize();
        this.TaskType = TaskTypes.None;
        this.CustomTaskType = CustomTaskTypes.CreateHeadlineTask;
        this.Length = NormalPlayerTask.TaskLength.Common;
        this.HasLocation = false;
        this.StartAt = SystemTypes.Admin;
        this.taskStep = 0;
        this.MaxStep = 1;
        this.ShowTaskTimer = false;
        this.arrowSuspended = true;
    }

    // Token: 0x02000622 RID: 1570
    public enum TaskLength
    {
        // Token: 0x04002141 RID: 8513
        None,
        // Token: 0x04002142 RID: 8514
        Common,
        // Token: 0x04002143 RID: 8515
        Short,
        // Token: 0x04002144 RID: 8516
        Long
    }
    // Token: 0x06002444 RID: 9284 RVA: 0x0009C36C File Offset: 0x0009A56C
    public override bool ValidConsole(global::Console console)
    {
        if (this.TaskType == TaskTypes.None && this.CustomTaskType == CustomTaskTypes.CreateHeadlineTask && console.TaskTypes.Contains(TaskTypes.None))
        {
            if (HeadlineManager.Instance.NewsModal != null && HeadlineManager.Instance.NewsModal.IsModalOpen == true)
            {
                return true;
            }
        }
        return false;
    }

    // Token: 0x06002445 RID: 9285 RVA: 0x0009C404 File Offset: 0x0009A604
    public void AppendTaskText(StringBuilder sb)
    {
        int num = this.Data.Count((byte b) => b == 250);
        bool flag = num > 0;
        if (flag)
        {
            if (this.IsComplete)
            {
                sb.Append("<color=#00DD00FF>");
            }
            else
            {
                sb.Append("<color=#FFFF00FF>");
            }
        }
        sb.Append("Headline Post button"); //Says place DestroyableSingleton<TranslationController>.Instance.GetString(this.StartAt)
        sb.Append(": ");
        sb.Append("Post a Headline");
        if (num < this.Data.Length)
        {
            sb.Append(" (");
            sb.Append(num);
            sb.Append("/");
            sb.Append(this.Data.Length);
            sb.Append(")");
        }
        if (flag)
        {
            sb.Append("</color>");
        }
        sb.AppendLine();
    }

    public CustomTaskTypes CustomTaskType;
}
