using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.DirectoryServices;
using Microsoft.Web.Administration;
using System.Diagnostics;
using System.Text;
using System.Collections;
using MongoDB.Bson;
using System.Security.AccessControl;

namespace MZ.BusinessLogicLayer.Business
{
    public class IISHelper
    {
        public string ShowVersion()
        {
            DirectoryEntry getEntity = new DirectoryEntry("IIS://localhost/W3SVC/INFO");
            string Version = getEntity.Properties["MajorIISVersionNumber"].Value.ToString();
            return Version;
        }
        /// <summary>
        /// 判断程序池是否存在
        /// </summary>
        /// <param name="AppPoolName">程序池名称</param>
        /// <returns>true存在 false不存在</returns>
        private bool IsAppPoolName(string AppPoolName)
        {
            bool result = false;
            DirectoryEntry appPools = new DirectoryEntry("IIS://localhost/W3SVC/AppPools");
            foreach (DirectoryEntry getdir in appPools.Children)
            {
                if (getdir.Name.Equals(AppPoolName))
                {
                    result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// 删除指定程序池
        /// </summary>
        /// <param name="AppPoolName">程序池名称</param>
        /// <returns>true删除成功 false删除失败</returns>
        private bool DeleteAppPool(string AppPoolName)
        {
            bool result = false;
            DirectoryEntry appPools = new DirectoryEntry("IIS://localhost/W3SVC/AppPools");
            foreach (DirectoryEntry getdir in appPools.Children)
            {
                if (getdir.Name.Equals(AppPoolName))
                {
                    try
                    {
                        getdir.DeleteTree();
                        result = true;
                    }
                    catch
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///  创建应用程序池
        /// </summary>
        /// <returns></returns>
        private bool CreateAppPool()
        {
            string AppPoolName = "LamAppPool";
            if (!IsAppPoolName(AppPoolName))
            {
                DirectoryEntry newpool;
                DirectoryEntry appPools = new DirectoryEntry("IIS://localhost/W3SVC/AppPools");
                newpool = appPools.Children.Add(AppPoolName, "IIsApplicationPool");
                newpool.CommitChanges();
                // MessageBox.Show(AppPoolName + "程序池增加成功");
            }


            #region 修改应用程序的配置(包含托管模式及其NET运行版本)
            ServerManager sm = new ServerManager();
            sm.ApplicationPools[AppPoolName].ManagedRuntimeVersion = "v4.0";
            sm.ApplicationPools[AppPoolName].ManagedPipelineMode = ManagedPipelineMode.Classic; //托管模式Integrated为集成 Classic为经典
            sm.CommitChanges();
            #endregion
            return true;
        }

        /// <summary>
        /// 针对IIS6的NET版进行设置,针对IIS6的NET版进行设置;因为此处我是用到NET4.0所以V4.0.30319 若是NET2.0则在这进行修改 v2.0.50727
        /// </summary>
        /// <returns></returns>
        public bool IISVersion(DirectoryEntry vdEntry)
        {
            //启动aspnet_regiis.exe程序 
            string fileName = Environment.GetEnvironmentVariable("windir") + @"\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo(fileName);
            //处理目录路径 
            string path = vdEntry.Path.ToUpper();
            int index = path.IndexOf("W3SVC");
            path = path.Remove(0, index);
            //启动ASPnet_iis.exe程序,刷新脚本映射 
            startInfo.Arguments = "-s " + path;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            string errors = process.StandardError.ReadToEnd();
            return true;
        }

        public bool MIMESetting(DirectoryEntry rootEntry, Dictionary<string, string> exList)
        {

            if (rootEntry == null)
                return false;
            System.DirectoryServices.PropertyValueCollection mime = rootEntry.Properties["MimeMap"];

            foreach (string e in exList.Keys)
            {
                IISOle.MimeMapClass Ex = new IISOle.MimeMapClass();
                Ex.Extension = e;
                Ex.MimeType = exList[e];
                object v = ContianValue(Ex, mime);
                if (v != null)
                {
                    continue;
                    //mime.Remove(v);
                }
                mime.Add(Ex);
                Ex = null;
            }
            rootEntry.CommitChanges();
            return true;
        }
        /// <summary>
        /// 搜索取指定扩展名对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="mime"></param>
        /// <returns></returns>
        private static object ContianValue(IISOle.IISMimeType type, PropertyValueCollection mime)
        {
            foreach (object v in mime)
            {
                IISOle.IISMimeType e = (IISOle.IISMimeType)v;
                if (type.Extension.ToLower().Equals(e.Extension.ToLower()) && (type.MimeType.ToLower().Equals(e.MimeType.ToLower())))
                {
                    e = null;
                    return v;
                }
            }
            return null;
        }


        /// <summary>
        /// 获取新站点端口,默认为当前最大端口号加一
        /// </summary>
        /// <returns></returns>
        private string GetNewSitePort()
        {
            int iDefault = 8000;
            DirectoryEntry siteEntry = new DirectoryEntry("IIS://localhost/w3svc");
            foreach (DirectoryEntry childEntry in siteEntry.Children)
            {
                if (childEntry.SchemaClassName == "IIsWebServer")
                {
                    if (childEntry.Properties["ServerBindings"].Value != null)
                    {
                        string strSettings = childEntry.Properties["ServerBindings"].Value.ToString();
                        int iSettingPort = int.Parse(strSettings.Substring(strSettings.IndexOf(':') + 1, (strSettings.LastIndexOf(':') - strSettings.IndexOf(':') - 1)));
                        iDefault = iSettingPort > iDefault ? iSettingPort : iDefault;
                    }
                }
            }
            return (iDefault + 1).ToString();
        }

        /// 获取新网站ID
        /// </summary>
        /// <returns></returns>
        private string GetNewWebSiteID()
        {
            int iWebSiteCount = 0;
            DirectoryEntry siteEntry = new DirectoryEntry("IIS://localhost/w3svc");
            foreach (DirectoryEntry childEntry in siteEntry.Children)
            {
                if (childEntry.SchemaClassName == "IIsWebServer")
                {
                    var curSiteCount = 0;
                    if (int.TryParse(childEntry.Name, out curSiteCount))
                    {
                        if (curSiteCount > iWebSiteCount)
                        {
                            iWebSiteCount = curSiteCount;
                        }

                    }
                }
            }
            return (iWebSiteCount + 1).ToString();
        }

        /// <summary>
        /// 判断网站是否已经存在
        /// </summary>
        /// <param name="strSiteName"></param>
        /// <returns></returns>
        private bool IsExist(string strSiteName, string bindString)
        {
            bool blExist = false;
            DirectoryEntry siteEntry = new DirectoryEntry("IIS://localhost/w3svc");
            foreach (DirectoryEntry childEntry in siteEntry.Children)
            {
                if (childEntry.SchemaClassName == "IIsWebServer")
                {
                    if (childEntry.Properties["ServerComment"].Value != null)
                    {
                        if (childEntry.Properties["ServerComment"].Value.ToString() == strSiteName)
                        {
                            return true;
                        }
                    }
                    if (childEntry.Properties["ServerBindings"].Value != null)
                    {
                        if (childEntry.Properties["ServerBindings"].Value.ToString() == bindString)
                        {
                            return true;
                        }

                    }
                }


            }
            return blExist;
        }

        /// <summary>
        /// 获取网站对象
        /// </summary>
        /// <param name="strSiteName"></param>
        /// <param name="bindString"></param>
        /// <returns></returns>
        public DirectoryEntry GetDirEntry(string strSiteName, string bindString)
        {

            DirectoryEntry siteEntry = new DirectoryEntry("IIS://localhost/w3svc");
            foreach (DirectoryEntry childEntry in siteEntry.Children)
            {
                if (childEntry.SchemaClassName == "IIsWebServer")
                {
                    if (childEntry.Properties["ServerComment"].Value != null)
                    {
                        if (childEntry.Properties["ServerComment"].Value.ToString() == strSiteName)
                        {
                            return childEntry;
                        }
                    }
                    if (childEntry.Properties["ServerBindings"].Value != null)
                    {
                        if (childEntry.Properties["ServerBindings"].Value.ToString() == bindString)
                        {
                            return childEntry;
                        }
                    }
                }


            }
            return null;
        }

        /// <summary>
        /// 通过地址过去对应的站点地址
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public DirectoryEntry GetDirEntryByPath(string path)
        {

            DirectoryEntry siteEntry = new DirectoryEntry("IIS://localhost/w3svc");
            foreach (DirectoryEntry childEntry in siteEntry.Children)
            {
                if (childEntry.SchemaClassName == "IIsWebServer")
                {

                    if (GetWebsitePhysicalPath(childEntry).ToLower() == path.ToLower())
                    {
                        return childEntry;
                    }
                }

            }
            return null;
        }

        /// <summary>
        /// 获取虚拟目录
        /// </summary>
        /// <param name="rootEntry"></param>
        /// <returns></returns>
        public DirectoryEntry FinVirtual(DirectoryEntry rootEntry)
        {
            foreach (DirectoryEntry childEntry in rootEntry.Children)
            {
                if ((childEntry.SchemaClassName == "IIsWebVirtualDir") && (childEntry.Name.ToLower() == "root"))
                {
                    if (childEntry.Properties["Path"].Value != null)
                    {
                        return childEntry;
                    }

                }
            }
            return null;
            // return rootEntry.Children.Find(virtualName, root.SchemaClassName);//查找虚拟目录)
        }
        /// <summary>
        /// 得到网站的物理路径
        /// </summary>
        /// <param name="rootEntry">网站节点</param>
        /// <returns></returns>
        public static string GetWebsitePhysicalPath(DirectoryEntry rootEntry)
        {
            string physicalPath = "";
            foreach (DirectoryEntry childEntry in rootEntry.Children)
            {
                if ((childEntry.SchemaClassName == "IIsWebVirtualDir") && (childEntry.Name.ToLower() == "root"))
                {
                    if (childEntry.Properties["Path"].Value != null)
                    {
                        physicalPath = childEntry.Properties["Path"].Value.ToString();
                    }
                    else
                    {
                        physicalPath = "";
                    }
                }
            }
            return physicalPath;
        }


        /// <summary>
        /// 得到网站的物理路径
        /// </summary>
        /// <param name="rootEntry">网站节点</param>
        /// <returns></returns>
        public string GetWebsiteMimeMap(DirectoryEntry rootEntry)
        {
            StringBuilder SiteInfo = new StringBuilder();
            foreach (DirectoryEntry childEntry in rootEntry.Children)
            {
                if ((childEntry.SchemaClassName == "IIsWebVirtualDir") && (childEntry.Name.ToLower() == "root"))
                {
                    if (childEntry.Properties["MimeMap"].Value != null)
                    {
                        var vdEntryEnum = childEntry.Properties["MimeMap"].GetEnumerator();
                        var mimeMapSB = new StringBuilder();
                        while (vdEntryEnum.MoveNext())
                        {
                            IISOle.IISMimeType vdEntry = (IISOle.IISMimeType)vdEntryEnum.Current;
                            mimeMapSB.AppendFormat(" {0}|{1} ", vdEntry.Extension, vdEntry.MimeType);
                        }
                        SiteInfo.AppendFormat("{1};\n\r", "", mimeMapSB.ToString());
                    }
                    else
                    {
                        ;
                    }
                }
            }
            return SiteInfo.ToString();
        }


        /// <summary>
        /// 获取站点信息
        /// </summary>
        /// <param name="stringSiteName"></param>
        /// <returns></returns>
        public string GetSiteInfo(DirectoryEntry newSiteEntry)
        {

            var displayNameDic = new Dictionary<string, string>();
            var SiteInfo = new StringBuilder();
            if (newSiteEntry != null)
            {

                if (newSiteEntry.SchemaClassName == "IIsWebServer")
                {
                    //SiteInfo.AppendFormat("站点名称：{0}\n\r", GetSiteProperty(newSiteEntry, "ServerComment"));
                    //SiteInfo.AppendFormat("端口：{0}\n\r", GetSiteProperty(newSiteEntry, "ServerBindings"));
                    //SiteInfo.AppendFormat("版本：{0}\n\r", GetSiteProperty(newSiteEntry, "MajorIISVersionNumber"));
                    //SiteInfo.AppendFormat("路径：{0}\n\r", GetSiteProperty(newSiteEntry, "Path"));
                    //SiteInfo.AppendFormat("AppPoolId：{0}\n\r", GetSiteProperty(newSiteEntry, "AppPoolId"));
                    //SiteInfo.AppendFormat("启用默认文档：{0}\n\r", GetSiteProperty(newSiteEntry, "EnableDefaultDoc"));
                    //SiteInfo.AppendFormat("默认文档：{0}\n\r", GetSiteProperty(newSiteEntry, "DefaultDoc"));
                    //SiteInfo.AppendFormat("最大连接：{0}\n\r", GetSiteProperty(newSiteEntry, "MaxConnections"));
                    //SiteInfo.AppendFormat("连接超时时间：{0}\n\r", GetSiteProperty(newSiteEntry, "ConnectionTimeout"));
                    //SiteInfo.AppendFormat("最大绑定数：{0}\n\r", GetSiteProperty(newSiteEntry, "MaxBandwidth"));
                    //SiteInfo.AppendFormat("运行状态：{0}\n\r", GetSiteProperty(newSiteEntry, "ServerState"));
                    displayNameDic.Add("ServerComment", "站点名称");
                    displayNameDic.Add("ServerBindings", "端口");
                    displayNameDic.Add("MajorIISVersionNumber", "版本");
                    // displayNameDic.Add("Path", "路径");
                    displayNameDic.Add("AppPoolId", "默认应用程序池Id");
                    displayNameDic.Add("DefaultDoc", "默认文档");
                    displayNameDic.Add("EnableDefaultDoc", "启用默认文档");
                    displayNameDic.Add("MaxConnections", "最大连接");
                    displayNameDic.Add("ConnectionTimeout", "连接超时时间");
                    displayNameDic.Add("MaxBandwidth", "最大绑定数");
                    displayNameDic.Add("ServerState", "运行状态");

                    SiteInfo.AppendFormat("{0}：{1}\n\r", "路径", GetWebsitePhysicalPath(newSiteEntry));
                    SiteInfo.AppendFormat("{0}：{1}\n\r", "MimeMap", GetWebsiteMimeMap(newSiteEntry));

                    IDictionaryEnumerator idenum = newSiteEntry.Properties.GetEnumerator();
                    while (idenum.MoveNext())
                    {
                        PropertyValueCollection propertyValue = (PropertyValueCollection)idenum.Current;
                        var propertyName = propertyValue.PropertyName;
                        if (displayNameDic.ContainsKey(propertyValue.PropertyName))
                        {

                            propertyName = displayNameDic[propertyValue.PropertyName];
                        }
                        SiteInfo.AppendFormat("{0}：{1}\n\r", propertyName, propertyValue.Value);
                    }



                    //var vdEntryEnum = newSiteEntry.Children.GetEnumerator();

                    //var mimeMapSB = new StringBuilder();
                    //while (vdEntryEnum.MoveNext())
                    //{
                    //    DirectoryEntry vdEntry = (DirectoryEntry)vdEntryEnum.Current;
                    //    mimeMapSB.AppendFormat("{0}", vdEntry.Properties["MimeMap"].Value);
                    //  }
                    //SiteInfo.AppendFormat("{0}：{1}\n\r", "MimeMap", mimeMapSB.ToString());
                }

            }
            else
            {
                SiteInfo.Append("站点未存在！！请先创建");
            }
            return SiteInfo.ToString();
        }

        /// <summary>
        /// 获取站点信息
        /// </summary>
        /// <param name="stringSiteName"></param>
        /// <returns></returns>
        public string GetSiteInfo(string strSiteName, string bindString)
        {
            var newSiteEntry = GetDirEntry(strSiteName, bindString);
            return GetSiteInfo(newSiteEntry);
        }

        /// <summary>
        /// 获取属性名
        /// </summary>
        /// <param name="dirEntity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string GetSiteProperty(DirectoryEntry dirEntity, string propertyName)
        {
            if (dirEntity != null && dirEntity.Properties[propertyName].Value != null)
            {

                return dirEntity.Properties[propertyName].Value.ToString();
            }
            return string.Empty;
        }


        /// <summary>
        /// 创建网站
        /// </summary>
        /// <param name="siteInfo"></param>
        public DirectoryEntry CreateNewWebSite(NewWebSiteInfo siteInfo)
        {
            DirectoryEntry newSiteEntry = GetDirEntry(siteInfo.CommentOfWebSite, siteInfo.BindString);
            if (newSiteEntry != null)
            {
                //throw new Exception("该网站名或者端口已存在" + Environment.NewLine + siteInfo.BindString);
                return newSiteEntry;
            }
            DirectoryEntry rootEntry = GetDirectoryEntry(entPath);

            var newSiteNum = GetNewWebSiteID();
            newSiteEntry = rootEntry.Children.Add(newSiteNum, "IIsWebServer");
            newSiteEntry.CommitChanges();

            newSiteEntry.Properties["ServerBindings"].Value = siteInfo.BindString;
            newSiteEntry.Properties["ServerComment"].Value = siteInfo.CommentOfWebSite;
            newSiteEntry.CommitChanges();
            DirectoryEntry vdEntry = newSiteEntry.Children.Add("root", "IIsWebVirtualDir");
            vdEntry.CommitChanges();
            string ChangWebPath = siteInfo.WebPath.TrimEnd('\\');
            if (!Directory.Exists(ChangWebPath))
            {
                Directory.CreateDirectory(ChangWebPath);//创建目录
            }
            vdEntry.Properties["Path"].Value = ChangWebPath;


            vdEntry.Invoke("AppCreate", true);//创建应用程序

            vdEntry.Properties["AccessRead"][0] = true; //设置读取权限
            vdEntry.Properties["AccessWrite"][0] = true;
            vdEntry.Properties["AccessScript"][0] = true;//执行权限
            vdEntry.Properties["AccessExecute"][0] = false;
            vdEntry.Properties["DefaultDoc"][0] = "Default.aspx";//设置默认文档
            vdEntry.Properties["AppFriendlyName"][0] = siteInfo.CommentOfWebSite; //应用程序名称           
            vdEntry.Properties["AuthFlags"][0] = 1;//0表示不允许匿名访问,1表示就可以3为基本身份验证，7为windows继承身份验证
            vdEntry.CommitChanges();

            //操作增加MIME
            //.dwf drawing/x-dwf
            //.dwg application/autocad
            //.dxf application/dxf
            //X-Powered-By ASP.NET
            var mineDic = siteInfo.MineDic;
            foreach (var str in mineDic)
            {
                IISOle.MimeMapClass NewMime = new IISOle.MimeMapClass();
                NewMime.Extension = str.Key; NewMime.MimeType = str.Value;
                vdEntry.Properties["MimeMap"].Add(NewMime);
            }
            if (mineDic.Count() > 0)
            {
                vdEntry.CommitChanges();
                rootEntry.CommitChanges();
            }

            #region 针对IIS7
            DirectoryEntry getEntity = new DirectoryEntry("IIS://localhost/W3SVC/INFO");
            int Version = int.Parse(getEntity.Properties["MajorIISVersionNumber"].Value.ToString());
            if (Version > 6)
            {
                #region 创建应用程序池
                string AppPoolName = siteInfo.CommentOfWebSite;
                if (!IsAppPoolName(AppPoolName))
                {
                    DirectoryEntry newpool;
                    DirectoryEntry appPools = new DirectoryEntry("IIS://localhost/W3SVC/AppPools");
                    newpool = appPools.Children.Add(AppPoolName, "IIsApplicationPool");

                    newpool.CommitChanges();
                }
                #endregion

                #region 修改应用程序的配置(包含托管模式及其NET运行版本)
                ServerManager sm = new ServerManager();
                sm.ApplicationPools[AppPoolName].ManagedRuntimeVersion = "v4.0";
                sm.ApplicationPools[AppPoolName].ManagedPipelineMode = ManagedPipelineMode.Integrated; //托管模式Integrated为集成 Classic为经典
                sm.CommitChanges();
                #endregion

                vdEntry.Properties["AppPoolId"].Value = AppPoolName;
                vdEntry.CommitChanges();
            }
            #endregion


            //启动aspnet_regiis.exe程序 
            string fileName = Environment.GetEnvironmentVariable("windir") + @"\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo(fileName);
            //处理目录路径 
            string path = vdEntry.Path.ToUpper();
            int index = path.IndexOf("W3SVC");
            path = path.Remove(0, index);
            //启动ASPnet_iis.exe程序,刷新脚本映射 
            startInfo.Arguments = "-s " + path;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            string errors = process.StandardError.ReadToEnd();
            if (errors != string.Empty)
            {
                throw new Exception(errors);
            }
            return newSiteEntry;

        }
        string entPath = String.Format("IIS://{0}/w3svc", "localhost");

        public DirectoryEntry GetDirectoryEntry(string entPath)
        {
            DirectoryEntry ent = new DirectoryEntry(entPath);
            return ent;
        }

        public class NewWebSiteInfo
        {
            private string hostIP;   // 主机IP
            private string portNum;   // 网站端口号
            private string descOfWebSite; // 网站表示。一般为网站的网站名。例如"www.dns.com.cn"
            private string commentOfWebSite;// 网站注释。一般也为网站的网站名。
            private string webPath;   // 网站的主目录。例如"e:\ mp"

            private Dictionary<string, string> _mineDic;

            public NewWebSiteInfo(string hostIP, string portNum, string descOfWebSite, string commentOfWebSite, string webPath)
            {
                this.hostIP = hostIP;
                this.portNum = portNum;
                this.descOfWebSite = descOfWebSite;
                this.commentOfWebSite = commentOfWebSite;
                this.webPath = webPath;
                _mineDic = new Dictionary<string, string>();
                _mineDic.Add(".xaml", "application/xaml+xml");
                _mineDic.Add(".xap", "application/x-silverlight-app");
            }

            public Dictionary<string, string> MineDic
            {
                get { return _mineDic; }

            }

            public string BindString
            {
                get
                {
                    return String.Format(":{0}:", portNum); //网站标识（IP,端口，主机头值）
                }
            }

            public string DescOfWebSite
            {
                get
                {
                    return descOfWebSite;
                }
            }

            public string PortNum
            {
                get
                {
                    return portNum;
                }
            }

            public string CommentOfWebSite
            {
                get
                {
                    return commentOfWebSite;
                }
            }

            public string WebPath
            {
                get
                {
                    return webPath;
                }
            }
        }

        ///// <summary>
        ///// 设置文件夹权限 处理给EVERONE赋予所有权限
        ///// </summary>
        ///// <param name="FileAdd">文件夹路径</param>
        //public void SetFileRole()
        //{
        //    string FileAdd = this.Context.Parameters["installdir"].ToString();
        //    FileAdd = FileAdd.Remove(FileAdd.LastIndexOf('\\'), 1);
        //    DirectorySecurity fSec = new DirectorySecurity();
        //    fSec.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
        //    System.IO.Directory.SetAccessControl(FileAdd, fSec);
        //}

        /// <summary>
        /// 获取所有站点
        /// </summary>
        /// <returns></returns>
        public List<BsonDocument> GetSites()
        {
            var webManager = new Microsoft.Web.Administration.ServerManager();
            DirectoryEntry rootEntry = new DirectoryEntry("IIS://localhost/w3svc");
            var WebSiteInfoList = new List<BsonDocument>();

            var runState = new Dictionary<ObjectState, string>();
            runState.Add(ObjectState.Starting, "启动中");
            runState.Add(ObjectState.Started, "已启动");
            runState.Add(ObjectState.Stopping, "停止中");
            runState.Add(ObjectState.Stopped, "已停止");
            runState.Add(ObjectState.Unknown, "未知");

            //var displayNameDic = new Dictionary<string, string>();
            //displayNameDic.Add("ServerComment", "站点名称");
            //displayNameDic.Add("ServerBindings", "端口");
            //displayNameDic.Add("MajorIISVersionNumber", "版本");
            //displayNameDic.Add("Path", "路径");
            //displayNameDic.Add("AppPoolId", "默认应用程序池Id");
            //displayNameDic.Add("DefaultDoc", "默认文档");
            //displayNameDic.Add("EnableDefaultDoc", "启用默认文档");
            //displayNameDic.Add("MaxConnections", "最大连接");
            //displayNameDic.Add("ConnectionTimeout", "连接超时时间");
            //displayNameDic.Add("MaxBandwidth", "最大绑定数");
            //displayNameDic.Add("ServerState", "运行状态");


            var notPushFields = new List<string>();
            notPushFields.Add("ScriptMaps");
            notPushFields.Add("HttpErrors");
            foreach (DirectoryEntry entry in rootEntry.Children)
            {
                if (entry.SchemaClassName == "IIsWebServer")
                {
                    if (entry.Properties["ServerComment"].Value != null)
                    {
                        
                        var webSiteInfo = new BsonDocument();
                        foreach (PropertyValueCollection properties in entry.Properties)
                        {
                            var value = "";
                            if (properties != null && properties.Value != null)
                            {
                                if (IsArray(properties.Value) )
                                {
                                    object[] objectToArr = (object[])properties.Value;
                                    var objectToList = new List<string>();
                                    foreach (var objectTemp in objectToArr)
                                    {
                                        objectToList.Add(objectTemp.ToString());
                                    }
                                    value = string.Join(",", objectToList);
                                }
                                else
                                {
                                    value = entry.Properties[properties.PropertyName].Value.ToString();
                                }
                                if (!notPushFields.Contains(properties.PropertyName))
                                {
                                    webSiteInfo.Add(properties.PropertyName, value);
                                }
                            }
                        }
                        DirectoryEntry virEntry = new DirectoryEntry(entry.Path + "/ROOT");
                        foreach (PropertyValueCollection properties in virEntry.Properties)
                        {
                            var value = "";
                            if (properties != null && properties.Value != null)
                            {
                                if (IsArray(properties.Value))
                                {
                                    object[] objectToArr = (object[])properties.Value;
                                    var objectToList = new List<string>();
                                    foreach (var objectTemp in objectToArr)
                                    {
                                        objectToList.Add(objectTemp.ToString());
                                    }
                                    value = string.Join(",", objectToList);
                                }
                                else
                                {
                                    value = properties.Value.ToString();
                                }
                                if (!notPushFields.Contains(properties.PropertyName))
                                {
                                    webSiteInfo.Set(properties.PropertyName, value);
                                }
                                
                            }
                        }

                        var path = GetWebsitePhysicalPath(entry);
                        webSiteInfo.Add("PhysicalPath", path);

                        //object objectArr = entry.Properties["ServerComment"].Value.ToString();

                        //DirectoryEntry virEntry = new DirectoryEntry(entry.Path + "/ROOT");
                        //foreach (DirectoryEntry entryVirtual in virEntry.Children)
                        //{
                        //    if (entryVirtual.SchemaClassName.Equals("IIsWebVirtualDir", StringComparison.OrdinalIgnoreCase))
                        //    {

                        //        webSiteInfo.VirtualName = entryVirtual.SchemaClassName;
                        //        webSiteInfo.VirtualPath = entryVirtual.Properties["Path"].Value.ToString();
                        //    }
                        //}

                        var appPool = webManager.ApplicationPools[webSiteInfo.String("AppPoolId")];
                        webSiteInfo.Add("AppPoolState", runState[appPool.State]);

                        WebSiteInfoList.Add(webSiteInfo);
                    }
                }
            }
            return WebSiteInfoList;
        }

        /// <summary>
        /// 获取站点ID
        /// </summary>
        /// <param name="strSiteName"></param>
        /// <returns></returns>
        private string GetSiteID(string strSiteName)
        {
            DirectoryEntry siteEntry = new DirectoryEntry("IIS://localhost/w3svc");
            foreach (DirectoryEntry childEntry in siteEntry.Children)
            {
                if (childEntry.SchemaClassName == "IIsWebServer")
                {
                    if (childEntry.Properties["ServerComment"].Value != null)
                    {
                        if (childEntry.Properties["ServerComment"].Value.ToString() == strSiteName)
                        {
                            return childEntry.Name;
                        }
                    }

                }


            }
            return null;
        }

        #region 站点操作
        /// <summary>
        /// 站点操作
        /// </summary>
        /// <param name="siteName">站点名称</param>
        /// <param name="opType">1.启动 2.通知 3.重启 4.删除</param>
        /// <returns></returns>
        public bool OperateWebSite(string siteName, string opType)
        {
            var result = false;
            switch (opType)
            {
                case "1":
                    result = StartWebSite(siteName);
                    break;
                case "2":
                    result = StopWebSite(siteName);
                    break;
                case "3":
                    result = ReStartWebSite(siteName);
                    break;
                case "4":
                    result = DeleteSite(siteName);
                    break;
            }


            return result;
        }

        /// <summary>
        /// 停止站点
        /// </summary>
        /// <param name="startSiteName">站点名称</param>
        /// <returns></returns>
        public bool StopWebSite(string startSiteName)
        {
            var webManager = new ServerManager();
            var startSite = webManager.Sites[startSiteName];
            if (startSite == null)
            {
                return false;
            }
            startSite.Stop();
            return true;
        }

        /// <summary>
        /// 开启站点
        /// </summary>
        /// <param name="startSiteName">站点名称</param>
        /// <returns></returns>
        public bool StartWebSite(string startSiteName)
        {

            var webManager = new Microsoft.Web.Administration.ServerManager();
            var startSite = webManager.Sites[startSiteName];
            if (startSite == null)
            {
                return false;
            }

            startSite.Start();
            return true;

        }

        /// <summary>
        /// 重启站点
        /// </summary>
        /// <param name="startSiteName">站点名称</param>
        /// <returns></returns>
        public bool ReStartWebSite(string startSiteName)
        {

            var webManager = new Microsoft.Web.Administration.ServerManager();
            var startSite = webManager.Sites[startSiteName];
            if (startSite == null)
            {
                return false;
            }
            foreach (var site in webManager.Sites)
            {
                if (site.Name == startSiteName)
                    site.Stop();
            }

            startSite.Start();
            return true;

        }

        /// <summary>
        /// 删除站点
        /// </summary>
        /// <param name="WebSiteName">站点名</param>
        /// <returns>成功或失败信息!</returns>
        public bool DeleteSite(string WebSiteName)
        {
            try
            {
                string SiteID = GetSiteID(WebSiteName);
                if (SiteID == null) return false;

                DirectoryEntry deRoot = new DirectoryEntry("IIS://localhost/W3SVC");
                try
                {
                    DirectoryEntry deVDir = new DirectoryEntry();
                    deRoot.RefreshCache();
                    deVDir = deRoot.Children.Find(SiteID, "IIsWebServer");
                    deRoot.Children.Remove(deVDir);

                    deRoot.CommitChanges();
                    deRoot.Close();
                    return true;
                }
                catch (System.Exception)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #endregion 站点操作

        #region 应用程序池操作
        /// <summary>
        /// 应用程序池操作
        /// </summary>
        /// <param name="appPoolName"></param>
        /// <param name="opType">1：回收2:启动3：停止</param>
        /// <returns></returns>
        public bool OperateAppPools(string appPoolName, string opType)
        {
            var result = false;
            switch (opType)
            {
                case "1":
                    result = RecycleAppPool(appPoolName);
                    break;
                case "2":
                    result = StartAppPool(appPoolName);
                    break;
                case "3":
                    result = StopAppPool(appPoolName);
                    break;
            }


            return result;
        }

        /// <summary>
        /// 回收应用程序池
        /// </summary>
        /// <param name="appPoolName"></param>
        /// <returns></returns>
        public bool RecycleAppPool(string appPoolName)
        {

            var webManager = new Microsoft.Web.Administration.ServerManager();
            var appPool = webManager.ApplicationPools[appPoolName];
            if (appPool == null)
            {
                return false;
            }
            else
            {
                appPool.Recycle();
            }
            return true;

        }

        /// <summary>
        /// 开启应用程序池
        /// </summary>
        /// <param name="appPoolName"></param>
        /// <returns></returns>
        public bool StartAppPool(string appPoolName)
        {

            var webManager = new Microsoft.Web.Administration.ServerManager();
            var appPool = webManager.ApplicationPools[appPoolName];
            if (appPool == null)
            {
                return false;
            }
            else
            {
                appPool.Start();
            }
            return true;

        }

        /// <summary>
        /// 停止应用程序池
        /// </summary>
        /// <param name="appPoolName"></param>
        /// <returns></returns>
        public bool StopAppPool(string appPoolName)
        {

            var webManager = new Microsoft.Web.Administration.ServerManager();
            var appPool = webManager.ApplicationPools[appPoolName];
            if (appPool == null)
            {
                return false;
            }
            else
            {
                appPool.Stop();
            }
            return true;

        }

        #endregion 应用程序池操作


        /**//// <summary>
            /// iiswebserver的状态
            /// </summary>
        public enum iisserverstate
        {
            /**//// <summary>
                /// 
                /// </summary>
            starting = 1,
            /**//// <summary>
                /// 
                /// </summary>
            started = 2,
            /**//// <summary>
                /// 
                /// </summary>
            stopping = 3,
            /**//// <summary>
                /// 
                /// </summary>
            stopped = 4,
            /**//// <summary>
                /// 
                /// </summary>
            pausing = 5,
            /**//// <summary>
                /// 
                /// </summary>
            paused = 6,
            /**//// <summary>
                /// 
                /// </summary>
            continuing = 7
        }

        /// <summary>
        /// 判断object对象是否为数组
        /// </summary>
        public static bool IsArray(object o)
        {
            return o is Array;
        }


    }
}
