
using JobManagerByQuartz.CommonService;
using JobManagerByQuartz.Factories;
using JobManagerByQuartz.Models;
using Quartz;
using Quartz.Net_Core.JobTriggerAbstract;
using Quartz.Net_EFModel_MySql;
using Quartz.Net_RepositoryInterface;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Quartz.Xml.JobSchedulingData20;


namespace JobManagerByQuartz.Controllers
{
    public class JobManagerController : Controller
    {
        public ICustomerJobInfoRepository _customerJobInfoRepository;
        public IServiceGetter _serviceGetter;
        public JobBaseTrigger _triggerBase;

        public JobManagerController(ICustomerJobInfoRepository _customerJobInfoRepository, IServiceGetter _serviceGetter)
        {
            this._customerJobInfoRepository = _customerJobInfoRepository;
            this._serviceGetter = _serviceGetter;
        }
        public JobManagerController()
        {
        }
        // GET: Default
        public ActionResult Index()
        {
            return View();
        }
        // GET: Default/Create
        public JsonResult Test()
        {
            return Json("{123}");
        }
        // GET: Default/Edit/5
        public JsonResult Test1()
        {
            return Json("{123}");
        }


        [HttpPost]
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="addJobViewModel">添加任务模型</param>
        /// <returns></returns>

        public JsonResult AddJob(AddJobViewModel addJobViewModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(ResponseDataFactory.CreateAjaxResponseData("-10001", "添加失败", "输入参数错误:" + ModelState.Values.Where(item => item.Errors.Count == 1).Aggregate(string.Empty, (current, item) => current + (item.Errors[0].ErrorMessage + ";   "))));
            }

            if (addJobViewModel.TriggerState == 2)
            {
                var lengh = addJobViewModel.JobName.Length;
                var originalJobName = addJobViewModel.JobName.Substring(0, lengh - 19);

                var originalJob =
                    _customerJobInfoRepository.LoadCustomerInfo(x => x.JobName == originalJobName);

                var jobInfo = new customer_quartzjobinfo
                {
                    CreateTime = DateTime.Now,
                    Cron = originalJob.Cron,
                    Description = addJobViewModel.JobDescription,
                    JobGroupName = addJobViewModel.JobGroupName,
                    JobName = addJobViewModel.JobName,
                    TriggerState = 2,
                    TriggerName = addJobViewModel.TriggerName,
                    TriggerGroupName = addJobViewModel.TriggerGroupName,
                    DLLName = ConfigurationManager.AppSettings["dllName"],
                    FullJobName = ConfigurationManager.AppSettings["FullJobName"],
                    RequestUrl = originalJob.RequestUrl,
                    Deleted = false,
                    Cycle = originalJob.Cycle,
                    TriggerType = originalJob.TriggerType,
                    RepeatCount = originalJob.RepeatCount
                };
                var jobId = _customerJobInfoRepository.AddCustomerJobInfo(jobInfo);
                return Json(ResponseDataFactory.CreateAjaxResponseData("1", "操作成功", jobId));
            }
            else
            {
                var jobId = _customerJobInfoRepository.AddCustomerJobInfo(addJobViewModel.JobName, addJobViewModel.JobGroupName, addJobViewModel.TriggerName, addJobViewModel.TriggerGroupName, addJobViewModel.CronJob == null ? null : addJobViewModel.CronJob.Cron, addJobViewModel.JobDescription, addJobViewModel.RequestUrl, addJobViewModel.SimpleJob == null ? null : addJobViewModel.SimpleJob.Cycle, addJobViewModel.SimpleJob == null ? null : addJobViewModel.SimpleJob.RepeatCount, addJobViewModel.TriggerType);
                return Json(ResponseDataFactory.CreateAjaxResponseData("1", "添加成功", jobId));
            }
        }
        [HttpPost]
        /// <summary>
        /// 更改任务执行周期（任务运行中）
        /// </summary>
        /// <param name="jobId">任务编号</param>
        /// <param name="cron">任务执行表达式</param>
        /// <returns></returns>
        public JsonResult ModifyJobCron(int jobId, string cron)
        {
            var ajaxResponseData = _operateJob(jobId, (jobDetail) => { jobDetail.Cron = cron; _customerJobInfoRepository.UpdateCustomerJobInfo(jobDetail); return _triggerBase.ModifyJobCron(jobDetail); });
            return Json(ajaxResponseData);
        }
        [HttpPost]
        /// <summary>
        /// 提供服务调用更改jobInfo接口
        /// </summary>
        /// <param name="updateJobInfoViewModel"></param>
        /// <returns></returns>
        public async Task<JsonResult> UpdateJobInfo(UpdateJobInfoViewModel updateJobInfoViewModel)
        {
            var jobId = await _customerJobInfoRepository.UpdateCustomerJobInfo(new customer_quartzjobinfo { JobName = updateJobInfoViewModel.JobName, Deleted = updateJobInfoViewModel.Deleted, TriggerState = updateJobInfoViewModel.JobState, Exception = updateJobInfoViewModel.Exception, PreTime = updateJobInfoViewModel.PreTime, NextTime = updateJobInfoViewModel.NextTime });

            return Json(ResponseDataFactory.CreateAjaxResponseData("1", "操作成功", jobId));
        }

        [HttpPost]
        public async Task<JsonResult> RecordExecutedJob(RecordExecutedJobViewModel addJobViewModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(ResponseDataFactory.CreateAjaxResponseData("-10001", "添加失败", "输入参数错误:" + ModelState.Values.Where(item => item.Errors.Count == 1).Aggregate(string.Empty, (current, item) => current + (item.Errors[0].ErrorMessage + ";   "))));
            }

            var originalJob =
                _customerJobInfoRepository.LoadCustomerInfo(x => x.JobName == addJobViewModel.OriginalJobName);

            var jobInfo = new customer_quartzjobinfo
            {
                CreateTime = DateTime.Now,
                Cron = originalJob.Cron,
                Description = addJobViewModel.JobDescription,
                JobGroupName = addJobViewModel.JobGroupName,
                JobName = addJobViewModel.JobName,
                TriggerState = 2,
                TriggerName = addJobViewModel.TriggerName,
                TriggerGroupName = addJobViewModel.TriggerGroupName,
                DLLName = ConfigurationManager.AppSettings["dllName"],
                FullJobName = ConfigurationManager.AppSettings["FullJobName"],
                RequestUrl = originalJob.RequestUrl,
                Deleted = false,
                Cycle = originalJob.Cycle,
                TriggerType = originalJob.TriggerType,
                RepeatCount = originalJob.RepeatCount
            };
            var jobId = _customerJobInfoRepository.AddCustomerJobInfo(jobInfo);
            return Json(ResponseDataFactory.CreateAjaxResponseData("1", "操作成功", jobId));
        }

        [HttpPost]
        /// <summary>
        /// 启动任务
        /// </summary>
        /// <param name="jobId">任务编号</param>
        /// <returns></returns>
        public JsonResult RunJob(int jobId)
        {
            var ajaxResponseData = _operateJob(jobId, (jobDetail) =>
            {
                //if (jobDetail.TriggerState == -1)
                //{
                //    jobDetail.TriggerState = -2;
                //} else if (jobDetail.TriggerState == 1)
                //{
                //    jobDetail.TriggerState = 6;
                //}
                //_customerJobInfoRepository.UpdateCustomerJobInfo(jobDetail);
                return _triggerBase.RunJob(jobDetail);
            });
            //ChangeCustomerJobState(jobId, -2);
            return Json(ajaxResponseData);
        }
        [HttpPost]
        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="jobId">任务编号</param>
        /// <returns></returns>
        public JsonResult DeleteJob(int jobId)
        {
            var ajaxResponseData = _operateJob(jobId, (jobDetail) =>
            {
                jobDetail.TriggerState = 5;
                _customerJobInfoRepository.UpdateCustomerJobInfo(jobDetail);
                return _triggerBase.DeleteJob(jobDetail); });
            //ChangeCustomerJobState(jobId, 5);
            return Json(ajaxResponseData);
        }

        [HttpPost]
        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="jobId">任务编号</param>
        /// <returns></returns>
        public JsonResult PauseJob(int jobId)
        {
            var ajaxResponseData = _operateJob(jobId, (jobDetail) =>
            {
                //jobDetail.TriggerState = 1;
                //_customerJobInfoRepository.UpdateCustomerJobInfo(jobDetail);
                return _triggerBase.PauseJob(jobDetail);
            });
            //ChangeCustomerJobState(jobId, (int)TriggerState.Paused);
            return Json(ajaxResponseData);
        }
        [HttpPost]
        /// <summary>
        /// 恢复任务
        /// </summary>
        /// <param name="jobId">任务编号</param>
        /// <returns></returns>
        public JsonResult ResumeJob(int jobId)
        {
            //ChangeCustomerJobState(jobId, (int)TriggerState.Normal);
            var ajaxResponseData = _operateJob(jobId, (jobDetail) =>
            {
                //jobDetail.TriggerState = 6;
                //_customerJobInfoRepository.UpdateCustomerJobInfo(jobDetail);
                return _triggerBase.ResumeJob(jobDetail);
            });
            return Json(ajaxResponseData);
        }


        [HttpGet]
        /// <summary>
        /// 获取任务
        /// </summary>
        /// <param name="jobStatus">任务状态</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页数量</param>
        /// <returns></returns>

        public JsonResult GetJobList(int jobStatus, int pageIndex, int pageSize)
        {

            var jobQueryable = _customerJobInfoRepository.LoadCustomerInfoes(x => x.TriggerState == jobStatus, x => x.Id, false, pageIndex, pageSize);
            var JobList = jobQueryable.Item1.ToList().Select(x => new
            {
                x.Id,
                x.JobName,
                TriggerType = x.TriggerType == "JobCronTrigger" ? "复杂任务" : "简单任务",
                x.Description,
                x.Cron,
                PreTime = x.PreTime.HasValue ? x.PreTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty,
                NextTime = x.NextTime.HasValue ? x.NextTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty,
                JobStartTime = x.JobStartTime.HasValue ? x.JobStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty,
                CreateTime = x.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                x.Exception
            }).ToList();
            var ajaxResponseData = ResponseDataFactory.CreateAjaxResponseData("1", "获取成功", new { JobList = JobList, TotalCount = jobQueryable.Item2 });
            return Json(ajaxResponseData, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 操作任务
        /// </summary>
        /// <param name="jobId">任务编号</param>
        /// <param name="operateJobFunc">具体操作任务的委托</param>
        /// <returns></returns>
        private AjaxResponseData _operateJob(int jobId, Func<customer_quartzjobinfo, bool> operateJobFunc)
        {
            AjaxResponseData ajaxResponseData = null;
            var jobDetail = _customerJobInfoRepository.LoadCustomerInfo(x => x.Id == jobId);
            if (jobDetail == null)
            {
                ajaxResponseData = ResponseDataFactory.CreateAjaxResponseData("0", "无此任务", jobDetail);
            }
            else
            {
                _setSpecificTrigger(jobDetail.TriggerType);
                var isSuccess = operateJobFunc(jobDetail);
                if (isSuccess)
                {
                    ajaxResponseData = ResponseDataFactory.CreateAjaxResponseData("1", "操作成功", jobDetail);
                }
                else
                {
                    ajaxResponseData = ResponseDataFactory.CreateAjaxResponseData("-10001", "操作失败", jobDetail);
                }
            }
            return ajaxResponseData;
        }
        private void _setSpecificTrigger(string triggerType)
        {
            _triggerBase = _serviceGetter.GetByKeyed<JobBaseTrigger, string>(triggerType);
        }
    }
}