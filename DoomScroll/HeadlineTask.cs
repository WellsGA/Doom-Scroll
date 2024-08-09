using Doom_Scroll;
using System;
using System.Linq;
using System.Text;

public enum CustomTaskTypes
{
    CreateHeadlineTask
}
public class HeadlineTask : NormalPlayerTask
{
    public override void Initialize()
    {
        base.Initialize();
        TaskType = TaskTypes.None;
        CustomTaskType = CustomTaskTypes.CreateHeadlineTask;
        Length = TaskLength.Common;
        HasLocation = false;
        StartAt = SystemTypes.Admin;
        taskStep = 0;
        MaxStep = 1;
        ShowTaskTimer = false;
        arrowSuspended = true;
    }


    // Token: 0x06002444 RID: 9284 RVA: 0x0009C36C File Offset: 0x0009A56C
    public override bool ValidConsole(Console console)
    {
        if (TaskType == TaskTypes.None && CustomTaskType == CustomTaskTypes.CreateHeadlineTask && console.TaskTypes.Contains(TaskTypes.None))
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
        int num = Data.Count((byte b) => b == 250);
        bool flag = num > 0;
        if (flag)
        {
            if (IsComplete)
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
        if (num < Data.Length)
        {
            sb.Append(" (");
            sb.Append(num);
            sb.Append("/");
            sb.Append(Data.Length);
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
