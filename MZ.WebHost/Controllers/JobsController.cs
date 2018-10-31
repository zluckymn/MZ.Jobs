using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using System.Collections;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Yinhe.ProcessingCenter;
using BusinessLogicLayer;
using MZ.Jobs.Core;
using MZ.Jobs.Core.Business.Info;

using MZ.BusinessLogicLayer.Business;
using System.Net.Http;
using log4net;
 

namespace MZ.WebHost.Controllers
{
 
    [RequestAuthorize]
    public class JobsController : Yinhe.ProcessingCenter.ApiControllerBase
    {

        public ILog jobLogger = LogManager.GetLogger(typeof(JobsController));
        //
        // GET: /Jobs/
        public string Index()
        {
            return "Hello!";
        }
       
        /// <summary>
        /// 返回job列表
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage JobList()
        {
            string customerCode = PageReq.GetString("customerCode");
            //jobList.ForEach(c =>c.Set("id",c.String("_id")).Remove("_id"));
            ResultInfo resultInfo = new ResultInfo
            {
                status = "true",
                message = "成功",
                data = _jobBll.FindCustomerRunningJobs(customerCode).ToJson()
            };
            //resultInfo.data = jobBll.FindCustomerRunningDetailJobs(customerCode).ToJson();
            return DataEncode(resultInfo);
        }

        /// <summary>
        /// 返回job主控
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage JobManage()
        {
            string customerCode = PageReq.GetString("customerCode");
            var job = _jobBll.FindManager(customerCode);
            var resultInfo = new ResultInfo
            {
                status = "true",
                message = "成功",
                data = job.ToJson()
            };
            return DataEncode(resultInfo);
        }

        /// <summary>
        /// 更新job状态
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage UpdateJobState()
        {
            string customerCode = PageReq.GetString("customerCode");
            string jobId = PageReq.GetString("jobId");
            string state = PageReq.GetString("state");
            string nextRunTime = PageReq.GetString("nextRunTime");
            string lastRunTime = PageReq.GetString("lastRunTime");
            var updateDoc = new BsonDocument {{"jobId", jobId}, {"customerCode", customerCode}};
            if (!string.IsNullOrEmpty(state)) {
                updateDoc.Add("state", state);
            }
            if (!string.IsNullOrEmpty(nextRunTime))
            {
                updateDoc.Add("nextRunTime", nextRunTime);
            }
            if (!string.IsNullOrEmpty(lastRunTime))
            {
                updateDoc.Add("lastRunTime", lastRunTime);
            }
            
            //添加到队列中进行处理
            var result= _jobBll.Update(updateDoc);//通过队列更新
            //此处判断autoUpdater更新器情况，让其只执行一次，或者增加执行次数判定，当执行repeatCount为0的时候不进行获取
            //InvokeResult result = dataOp.Update("BackgroundJob", Query.And(Query.EQ("customerCode", customerCode), Query.EQ("jobId", jobId)), updateDoc);
            //var dataBson = result.BsonInfo;
            //dataBson.Set("id", dataBson.String("_id")).Remove("_id");
            var resultInfo = new ResultInfo
            {
                status = result.Status == Status.Successful ? "true" : "false",
                message = result.Status == Status.Successful ? "成功" : "保存失败"
            };
            //resultInfo.data = dataBson.ToHashtable();
            return DataEncode(resultInfo);
        }
        /// <summary>
        /// 创建job日志
        /// 此处通过JobId或者类型添加到RabbMQ中进行分布式处理，当压力大的时候进行预警并进行添加RabbitMQ服务
        /// 可按照 jobType className accemblyName进行队列拆分
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage CreateJobLog()
        {
           
           //获取请求的IP地址
           var ipAddress = string.Empty;
            ipAddress=IpHelper.GetIPAddress; ;
            var customerCode = PageReq.GetString("customerCode");
            jobLogger.Info("CreateJobLog:"+customerCode);
            var jobId = PageReq.GetString("jobId");
            var name = HttpUtility.UrlDecode(PageReq.GetString("name"));
            var execDateTime = PageReq.GetString("execDateTime");
            var execDuration = PageReq.GetString("execDuration");
            var remark = HttpUtility.UrlDecode(PageReq.GetString("remark"));
            var accemblyName = PageReq.GetString("accemblyName");
            var className =PageReq.GetString("className");
            var jobType = PageReq.GetInt("jobType");//BackgroundJobType类型
            var dto = new BackgroundJobLog()
            {
                accemblyName = accemblyName,
                className = className,
                customerCode = customerCode,
                executionTime = execDateTime,
                executionDuration = execDuration,
                jobId = jobId,
                jobType = jobType.ToString(),
                name = name,
                runLog=remark,
                clientInfo = ipAddress
            };
            
            jobLogger.Info("begin:CreateLogViaQueue"+ dto.runLog.Length);
            var result = _jobLogBll.ProcessJobData(dto);
          
            var resultInfo = new ResultInfo
            {
                status = result.Status == Status.Successful ? "true" : "false",
                message = result.Status == Status.Successful ? "成功" : "保存失败",
                data = dto
            };
            jobLogger.Info("end:CreateLogViaQueue");
            return DataEncode(resultInfo);
        }
    }
}
