﻿using EFFC.Frame.Net.Module.Extend.EWRA.Logic;
using System;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Data;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Module.Extend.WeChat;
using System.Collections.Generic;
using System.Linq;

namespace EFFC.Frame.Net.Module.Extend.WeixinWeb.Logic
{
    /// <summary>
    /// weixin公众号Logic
    /// </summary>
    public abstract partial class WeixinAuthorizationLogic : AuthorizationLogic
    {
        WeixinMPHelper _weixinmp = null;
        /// <summary>
        /// 微信小程序
        /// </summary>
        public WeixinMPHelper WeixinMP
        {
            get
            {
                if (_weixinmp == null) _weixinmp = new WeixinMPHelper(this);
                return _weixinmp;
            }


        }
        public class WeixinMPHelper : WechatMPHelper
        {
            WeixinAuthorizationLogic _logic;

            public WeixinMPHelper(WeixinAuthorizationLogic logic) : base("", "", "")
            {
                _logic = logic;
                AppID = ComFunc.nvl(_logic.CallContext_Parameter.ExtentionObj.weixinmp.appid);
                AppSecret = ComFunc.nvl(_logic.CallContext_Parameter.ExtentionObj.weixinmp.appsecret);

                Weixin_Mch_Ssl_Path = ComFunc.nvl(_logic.CallContext_Parameter.ExtentionObj.weixinmp.weixinmp_mch_ssl_path);
                Weixin_Mch_Ssl_Pass = ComFunc.nvl(_logic.CallContext_Parameter.ExtentionObj.weixinmp.weixinmp_mch_ssl_pass);
            }

        }
    }
}
