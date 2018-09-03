
using Quartz;
using Quartz.Net_Infrastructure.LogUtil;
using Quartz.Net_RemoteServer.Events;
using Quartz.Net_RemoteServer.Models;
using Quartz.Net_RepositoryInterface;
using System;

namespace Quartz.Net_RemoteServer.Listeners
{
    //TODOThink:log4netkey是否可以动态得到？
    //TODOThink:是否开始异步 减少监听器本身运行的时间
    internal class MyJobListener : SubjectBase, IJobListener
    {
        //public ICustomerJobInfoRepository _customerJobInfoRepository;

        public MyJobListener()
        {
            new Observer(this);
        }
        public string Name
        {
            get
            {
                return "customerJobListener";
            }
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {

            //var jobId = Convert.ToInt32(context.JobDetail.JobDataMap["jobId"]);
            //var name = context.Scheduler.GetTriggerState(context.Trigger.Key).ToString();
            ////var triggerState = _changeType(context.Scheduler.GetTriggerState(context.Trigger.Key));

            //var customerJobInfoModel = _customerJobInfoRepository.LoadCustomerInfo(x => x.Id == jobId);
            ////customerJobInfoModel.TriggerState = triggerState;
            //if (jobException == null)
            //{


            //    Console.WriteLine("任务编号{0}；执行时间：{1},状态：{2}", context.JobDetail.JobDataMap["jobId"], DateTime.Now, name);
            //}
            //else
            //{
            //    customerJobInfoModel.Exception = jobException.Message;
            //    Console.WriteLine("jobId{0}执行失败：{1}", context.JobDetail.JobDataMap["jobId"], jobException.Message);
            //}
            //_customerJobInfoRepository.UpdateCustomerJobInfo(customerJobInfoModel);
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            var jobName = context.JobDetail.Key.Name;
            var jobState = 6;
            var operateType = "运行";

            string exceptionMessage = jobException == null ? null : jobException.Message;

            #region RecodJobExecutedInfo

            //AddJob
            var jobNameTime = jobName + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            var jobGroupName = context.JobDetail.Key.Group;
            var triggerName = context.Trigger.Key.Name;
            var triggerGroupName = context.Trigger.Key.Group;

            var cron = "1";
            var jobDescription = jobName + DateTime.Now;
            var requestUrl = "1";
            var cycle = "1";
            var repeatCount = "1";
            var triggerType = "";

            this.NotifyAsync(new JobExcutedCallBackModel { IsJobDeleted = false, JobName = jobNameTime, JobState = 2, Log4NetKey_JobError = Log4NetKeys.Log4netJobErrorKey, Log4NetKey_JobInfo = Log4NetKeys.Log4netJobInfoKey, OperateType = operateType, RequestUrl = "http://localhost:52044/JobManager/AddJob", RequestBody = new { TriggerState = 2, JobName = jobNameTime, JobGroupName = jobGroupName, TriggerName = triggerName, TriggerGroupName = triggerGroupName, CronJob = new { Cron = cron }, JobDescription = jobDescription, RequestUrl = requestUrl, SimpleJob = new { Cycle = cycle, RepeatCount = repeatCount }, TriggerType = triggerType, JobState = jobState, Exception = exceptionMessage, PreTime = context.Trigger.GetPreviousFireTimeUtc().HasValue ? context.Trigger.GetPreviousFireTimeUtc().Value.LocalDateTime as DateTime? : null, NextTime = context.Trigger.GetNextFireTimeUtc().HasValue ? context.Trigger.GetNextFireTimeUtc().Value.LocalDateTime as DateTime? : null } });

            this.NotifyAsync(new JobExcutedCallBackModel { IsJobDeleted = false, JobName = jobNameTime, JobState = jobState, Log4NetKey_JobError = Log4NetKeys.Log4netJobErrorKey, Log4NetKey_JobInfo = Log4NetKeys.Log4netJobInfoKey, OperateType = operateType, RequestUrl = "http://localhost:52044/JobManager/UpdateJobInfo", RequestBody = new { JobName = jobNameTime, JobState = 2, Exception = exceptionMessage, PreTime = context.Trigger.GetPreviousFireTimeUtc().HasValue ? context.Trigger.GetPreviousFireTimeUtc().Value.LocalDateTime as DateTime? : null, NextTime = context.Trigger.GetNextFireTimeUtc().HasValue ? context.Trigger.GetNextFireTimeUtc().Value.LocalDateTime as DateTime? : null } });
            #endregion

            //UpdateJobInfo
            this.NotifyAsync(new JobExcutedCallBackModel { IsJobDeleted = false, JobName = jobName, JobState = jobState, Log4NetKey_JobError = Log4NetKeys.Log4netJobErrorKey, Log4NetKey_JobInfo = Log4NetKeys.Log4netJobInfoKey, OperateType = operateType, RequestUrl = "http://localhost:52044/JobManager/UpdateJobInfo", RequestBody = new { JobName = jobName, JobState = jobState, Exception = exceptionMessage, PreTime = context.Trigger.GetPreviousFireTimeUtc().HasValue ? context.Trigger.GetPreviousFireTimeUtc().Value.LocalDateTime as DateTime? : null, NextTime = context.Trigger.GetNextFireTimeUtc().HasValue ? context.Trigger.GetNextFireTimeUtc().Value.LocalDateTime as DateTime? : null } });

            

            CustomerLogUtil.Info(Log4NetKeys.Log4netJobInfoKey, CustomerLogFormatUtil.LogJobMsgFormat(jobName, jobState, operateType));
        }

    }
}
