﻿using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFFC.Frame.Net.Module.Extend.WebGo.Logic
{
    public partial class GoLogic
    {
        private ClientInfoProperty _ci;
        private ServerInfoProperty _si;
        private EFFCFrameInfoProperty _fi;
        /// <summary>
        /// 客户端信息集
        /// </summary>
        public ClientInfoProperty ClientInfo
        {
            get
            {
                if (_ci == null)
                {
                    _ci = new ClientInfoProperty(this);
                }
                return _ci;
            }
        }
        /// <summary>
        /// 服务器端信息集
        /// </summary>
        public ServerInfoProperty ServerInfo
        {
            get
            {
                if (_si == null)
                {
                    _si = new ServerInfoProperty(this);
                }
                return _si;
            }
        }
        /// <summary>
        /// EFFC框架信息
        /// </summary>
        public EFFCFrameInfoProperty EFFCFrameInfo
        {
            get
            {
                if (_fi == null)
                {
                    _fi = new EFFCFrameInfoProperty(this);
                }
                return _fi;
            }
        }
        public class ServerInfoProperty
        {
            GoLogic _logic;

            string _ip;
            string _servername;
            public ServerInfoProperty(GoLogic logic)
            {
                _logic = logic;
                _ip = ComFunc.nvl(_logic.CallContext_Parameter.WebParam[DomainKey.APPLICATION_ENVIRONMENT, "serverinfo_ip"]);
                _servername = ComFunc.nvl(_logic.CallContext_Parameter.WebParam[DomainKey.APPLICATION_ENVIRONMENT, "server_servername"]);
            }
            /// <summary>
            /// IP
            /// </summary>
            public string IP { get { return _ip; } }
            /// <summary>
            /// server的机器名称
            /// </summary>
            public string ServerName { get { return _servername; } }

            /// <summary>
            /// Web服务器的物理路径
            /// </summary>
            public string ServerRootPath
            {
                get { return _logic.CallContext_Parameter.WebParam.ServerRootPath; }
            }
            /// <summary>
            /// Web服务器的站点路径
            /// </summary>
            public string ServerRootPath_URL
            {
                get { return _logic.CallContext_Parameter.WebParam.ServerRootPath_URL; }
            }
            /// <summary>
            /// 站点Host的url
            /// </summary>
            public string SiteHostUrl
            {
                get { return _logic.CallContext_Parameter.WebParam.SiteUrl; }
            }

            /// <summary>
            /// 获取站点的域名
            /// </summary>
            public string Domain
            {
                get
                {
                    return _logic.CallContext_Parameter.WebParam.Domain;
                }
            }
        }

        public class ClientInfoProperty
        {
            string _ip;
            string _userhostname;
            string _browserversion;
            string _platform;
            GoLogic _logic;
            public ClientInfoProperty(GoLogic logic)
            {
                _logic = logic;
            }
            /// <summary>
            /// IP
            /// </summary>
            public string IP { get { return ComFunc.nvl(_logic.CallContext_Parameter.WebParam[DomainKey.APPLICATION_ENVIRONMENT, "clientinfo_ip"]); } }
            /// <summary>
            /// Client端机器名称
            /// </summary>
            public string UserHostName { get { return ComFunc.nvl(_logic.CallContext_Parameter.WebParam[DomainKey.APPLICATION_ENVIRONMENT, "clientinfo_userhostname"]); } }
            /// <summary>
            /// Client端浏览器版本号
            /// </summary>
            public string BrowserVersion { get { return ComFunc.nvl(_logic.CallContext_Parameter.WebParam[DomainKey.APPLICATION_ENVIRONMENT, "clientinfo_browserversion"]); } }
            /// <summary>
            /// Client端操作平台名称
            /// </summary>
            public string Platform { get { return ComFunc.nvl(_logic.CallContext_Parameter.WebParam[DomainKey.APPLICATION_ENVIRONMENT, "clientinfo_platform"]); } }
        }

        public class EFFCFrameInfoProperty
        {
            GoLogic _logic;
            public EFFCFrameInfoProperty(GoLogic logic)
            {
                _logic = logic;
            }
            /// <summary>
            /// EFFC.Base程式版本号
            /// </summary>
            public string Base_Version { get { return ComFunc.nvl(_logic.CallContext_Parameter.WebParam[DomainKey.APPLICATION_ENVIRONMENT, "effcinfo_base_version"]); } }
            /// <summary>
            /// EFFC.Base产品版本号
            /// </summary>
            public string Base_Product_Version { get { return ComFunc.nvl(_logic.CallContext_Parameter.WebParam[DomainKey.APPLICATION_ENVIRONMENT, "effcinfo_base_product_version"]); } }
        }
    }
}
