﻿using EFFC.Frame.Net.Module.Extend.WebGo.Logic;
using System;
using System.Collections.Generic;
using System.Text;
using EFFC.Frame.Net.Module.Extend.WebGo.Datas;
using EFFC.Frame.Net.Base.Data.Base;

namespace Web.Business
{
    public class Index : GoLogic
    {
        protected override Func<LogicData, object> GetFunction(string actionName)
        {
            return Load;
        }

        private object Load(LogicData arg)
        {
            SetContentType(EFFC.Frame.Net.Base.Constants.GoResponseDataType.RazorView);
            Razor.SetViewPath("~/Views/Index.cshtml");
            //Razor.SetMvcModuleData(new
            //{
            //    Name="ych",
            //    Age = 30
            //});
            Razor.SetViewData("dt", DateTime.Now);
            return FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");
        }
    }
}
