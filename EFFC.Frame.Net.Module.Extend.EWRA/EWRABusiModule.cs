﻿using EFFC.Frame.Net.Module.Business;
using EFFC.Frame.Net.Module.Extend.EWRA.DataCollections;
using EFFC.Frame.Net.Module.Extend.EWRA.Logic;
using EFFC.Frame.Net.Module.Extend.EWRA.Parameters;
using System;
using System.Collections.Generic;
using System.Text;
using EFFC.Frame.Net.Base.Module.Proxy;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Global;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Module.Extend.EWRA.Attributes;

namespace EFFC.Frame.Net.Module.Extend.EWRA
{
    public class EWRABusiModule : BaseBusiModule<RestLogic, EWRAParameter, EWRAData>
    {
        static Regex _reg_brace_p_ = new Regex(@"\{[A-Za-z0-9_]+\}", RegexOptions.IgnoreCase);
        static RestPointContext _rpcontext = null;
        static Type _auth = null;
        public override string Name => "EWRABusiness";

        public override string Description => "EFFC Web Rest API Business Module";

        public override void Dispose()
        {

        }

        protected override bool CheckMyParametersAndConfig(EWRAParameter p, EWRAData d)
        {
            return true;
        }

        protected override void InvokeBusiness(EWRAParameter p, EWRAData d)
        {
            //如果不请求任何资源，则返回API接口文档
            if (p.RequestRoute == "" || p.RequestRoute == "/")
            {
                d.StatusCode = Constants.RestStatusCode.OK;
                d.Result = _rpcontext.MainRouteDesc;
                return;
            }
            //如果是进行验证请求
            var strarr = p.RequestRoute.ToLower().Split('/').Where(sp => sp != "").ToArray();
            var authlogic = (AuthorizationLogic)Activator.CreateInstance(_auth);
            if (strarr[0] == "auth"
                || strarr[0] == "authorize")
            {
                authlogic.process(p, d);
                return;
            }
            //执行校验获得校验结果
            p.__AuthMethod = "ValidAuth";
            authlogic.process(p, d);
            var authMsg = p.__Auth_ValidMsg;


            object[] parseParams = null;
            //var dt = DateTime.Now;
            var invokelist = _rpcontext.FindByRoute(p.RequestRoute, p.MethodName, ref parseParams);
            //GlobalCommon.Logger.WriteLog(LoggerLevel.DEBUG, $"路由搜索消耗时间：{(DateTime.Now - dt).TotalMilliseconds}ms"); dt = DateTime.Now;

            if (invokelist == null)
            {
                GlobalCommon.Logger.WriteLog(LoggerLevel.ERROR, $"未找到[{p.MethodName}-{p.RequestRoute}]的执行方法");
                //设置返回值结果
                d.StatusCode = Constants.RestStatusCode.NOT_FOUND;
                d.Error = "请求的资源不存在";
                return;
            }
            //授权判定
            foreach (var rie in invokelist)
            {
                //判断method是否需要进行校验
                var authattri = rie.InvokeMethod.GetCustomAttribute<EWRAAuthAttribute>();
                //如果需要校验，但前面校验的结果不通过的话，则中断执行
                if ((authattri == null || authattri.IsNeedAuth) && !p.IsAuth)
                {
                    d.StatusCode = Constants.RestStatusCode.UNAUTHORIZED;
                    d.Error = authMsg;
                    return;
                }
            }

            var index = 0;
            var preIndex = 0;
            object preResult = null;
            var key = $"restAPI_{p.MethodName}:{p.RequestRoute}";
            var isNeedInvoke = true;
            if (p.MethodName == "get")
            {
                if (GlobalCommon.ApplicationCache.Get(key) != null)
                {
                    preResult = GlobalCommon.ApplicationCache.Get(key);
                    isNeedInvoke = false;
                }
            }
            if (isNeedInvoke)
            {
                foreach (var rie in invokelist)
                {
                    var pList = new List<object>();
                    pList.AddRange(parseParams.Skip(preIndex).Take(rie.ParameterCountWithOutParent).ToArray());
                    if (rie.HasParentParameter)
                    {
                        //如果父节点get的结果为null的话，则直接中断执行（安全需要）
                        if (preResult == null)
                        {
                            //设置返回值结果
                            d.StatusCode = Constants.RestStatusCode.NOT_FOUND;
                            d.Error = "请求的资源不存在";
                            return;
                        }
                        pList.Add(preResult);
                    }

                    //查找cache中是否已经存在结果
                    //只有get才做cache
                    if (rie.InvokeName == "get")
                    {
                        var reg = new Regex(rie.RouteRegExpress);
                        var tmpkey = $"restAPI_{rie.InvokeName}:{reg.Match(p.RequestRoute).Value}";


                        if (GlobalCommon.ApplicationCache.Get(tmpkey) != null)
                        {
                            preResult = GlobalCommon.ApplicationCache.Get(tmpkey);
                        }
                        else
                        {
                            preResult = DoInvoke(rie, pList.ToArray(), p, d);
                        }
                    }
                    else
                    {
                        preResult = DoInvoke(rie, pList.ToArray(), p, d);
                    }
                    preIndex += rie.ParameterCountWithOutParent;
                    index++;
                }
            }

            d.Result = preResult;

            //刷新缓存
            if (d.IsCache)
            {
                GlobalCommon.ApplicationCache.Set(key, preResult, TimeSpan.FromHours(2));
            }

            //GlobalCommon.Logger.WriteLog(LoggerLevel.DEBUG, $"方法执行消耗时间：{(DateTime.Now - dt).TotalMilliseconds}ms"); dt = DateTime.Now;
            //如果为post,put,patch等操作时，需要做cache的更新操作(如果设定不做cache，则不处理)
            preIndex = 0;
            preResult = null;
            if (new string[] { "post", "put", "patch" }.Contains(p.MethodName))
            {
                var rcl = d.RefreshCacheList.Distinct().ToArray();
                foreach (var rr in rcl)
                {
                    var refreshInvokelist = _rpcontext.FindByRoute(rr, "get", ref parseParams);
                    if (refreshInvokelist != null)
                    {
                        foreach (var rie in refreshInvokelist)
                        {
                            var reg = new Regex(rie.RouteRegExpress);
                            var tmpkey = $"restAPI_get:{reg.Match(rr).Value}";

                            var pList = new List<object>();
                            pList.AddRange(parseParams.Skip(preIndex).Take(rie.ParameterCountWithOutParent).ToArray());
                            if (rie.HasParentParameter)
                            {
                                //如果父节点get的结果为null的话，则直接中断执行（安全需要）
                                if (preResult == null)
                                {
                                    GlobalCommon.Logger.WriteLog(LoggerLevel.WARN, $"执行{p.MethodName}-{p.RequestRoute}完成后，刷新缓存失败标记为{tmpkey}的缓存会被清除");
                                    GlobalCommon.ApplicationCache.Remove(tmpkey);
                                    break;
                                }
                                pList.Add(preResult);
                            }

                            preResult = DoInvoke(rie, pList.ToArray(), p, d);

                            if (preResult != null)
                            {
                                GlobalCommon.Logger.WriteLog(LoggerLevel.INFO, $"执行{p.MethodName}-{p.RequestRoute}完成后，刷新缓存标记为{tmpkey}的缓存成功");
                                GlobalCommon.ApplicationCache.Set(tmpkey, preResult, TimeSpan.FromHours(2));
                            }

                            preIndex += rie.ParameterCountWithOutParent;
                            index++;
                        }

                    }
                }
            }


        }

        protected override void OnUsed(ProxyManager ma, dynamic options)
        {
            var version = ComFunc.nvl(options.RestAPIMainVersion);
            var assemblyName = ComFunc.nvl(options.RestAPILogicAssemblyName);
            Type logicType = null;
            if (assemblyName == "")
            {
                GlobalCommon.Logger.WriteLog(Base.Constants.LoggerLevel.ERROR, $"{this.GetType().Name}加载Logic失败，原因:RestAPILogicAssemblyName为空，请在调用ProxyManager.UseProxy中的options中设定该参数（RestAPILogicAssemblyName为Logic所在的Assembly的Name）");
                return;
            }
            if (options.RestAPILogicBaseType == null)
            {
                logicType = typeof(RestLogic);
            }
            else
            {
                if (options.RestAPILogicBaseType is string)
                {
                    logicType = Type.GetType(ComFunc.nvl(options.RestAPILogicBaseType));
                }
                else if (options.RestAPILogicBaseType is Type && ((Type)options.RestAPILogicBaseType).GetTypeInfo().IsSubclassOf(typeof(RestLogic)))
                {
                    logicType = (Type)options.RestAPILogicBaseType;
                }
                else
                {
                    logicType = typeof(RestLogic);
                }
            }

            _rpcontext = RestPointContext.Create(assemblyName, version, logicType);
            GlobalCommon.Logger.WriteLog(Base.Constants.LoggerLevel.INFO, $"{this.GetType().Name}：当前加载的Logic的基类为{logicType.Name},如需要修改，请在调用ProxyManager.UseProxy中的options中设定该参数（RestAPILogicBaseType类型为Type类型或Type的Name，该Type必须为RestLogic的子类）");
            GlobalCommon.Logger.WriteLog(Base.Constants.LoggerLevel.INFO, $"{this.GetType().Name}：当前运行的API的主版本号为{_rpcontext.MainVersion},如需要修改，请在调用ProxyManager.UseProxy中的options中设定该参数（RestAPIMainVersion格式为:v+数字,如v1.1）");

            //加载验证logic
            if (options.RestAPIAuthLogicType == null)
            {
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
                var searchAuth = assembly.GetTypes().Where(p => p.GetTypeInfo().IsSubclassOf(typeof(AuthorizationLogic)));
                if (searchAuth.Count() > 0)
                {
                    _auth = searchAuth.First();
                }
                else
                {
                    _auth = typeof(AuthorizationLogic);
                }
            }
            else
            {
                if (options.RestAPIAuthLogicType is string)
                {
                    _auth = Type.GetType(ComFunc.nvl(options.RestAPIAuthLogicType));
                }
                else if (options.RestAPIAuthLogicType is Type && ((Type)options.RestAPIAuthLogicType).GetTypeInfo().IsSubclassOf(typeof(AuthorizationLogic)))
                {
                    _auth = (Type)options.RestAPIAuthLogicType;
                }
                else
                {
                    _auth = typeof(AuthorizationLogic);
                }
            }


            GlobalCommon.Logger.WriteLog(Base.Constants.LoggerLevel.INFO, $"{this.GetType().Name}：当前请求验证用的的Logic的为{_auth.Name},如需要修改，请在{assemblyName}继承AuthenrizeLogic实现相关方法，或者使用options.RestAPIAuthLogicType来指定验证Logic类型");
        }

        private object DoInvoke(RouteInvokeEntity rie, object[] listInvokeParamValues, EWRAParameter ewrap, EWRAData ewrad)
        {
            var instance = (RestLogic)Activator.CreateInstance(rie.InstanceType);

            //找到正确的参数对应顺序
            var parameterlist = rie.InvokeMethod.GetParameters().Select(p => p.Name.ToLower()).ToList();
            var matchs = _reg_brace_p_.Matches(rie.Route);
            var l = new List<string>();
            foreach (Match m in matchs)
            {
                var key = m.Value.ToLower().Replace("{", "").Replace("}", "");
                l.Add(key);
            }
            var newp = new List<object>();

            foreach (var item in parameterlist)
            {
                if (l.Contains(item))
                    newp.Add(listInvokeParamValues[l.IndexOf(item)]);
            }
            if (rie.HasParentParameter)
                newp.Add(listInvokeParamValues.Last());

            ewrad.InvokeParameters = newp.ToArray();
            ewrad.InvokeMethod = rie.InvokeMethod;
            //logic的预执行处理
            instance.process(ewrap, ewrad);
            return ewrad.Result;
        }

    }
}
