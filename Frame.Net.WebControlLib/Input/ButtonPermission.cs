﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.ComponentModel;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.WebControlLib.TypeConvert;
using System.Web.UI;
using EFFC.Frame.Net.WebControlLib.Interfaces;

[assembly: TagPrefix("EFFC.Frame.Net.WebControlLib.Input", "clinput")]
namespace EFFC.Frame.Net.WebControlLib.Input
{
    public class ButtonPermission:Button,IActionPermission
    {
        string _actionid = "";

        [Browsable(true)]
        [Category("PermissionInfo")]
        [Description("被授权的ActionID，可以为多个，每个之间用逗号分割")]
        [Bindable(true)]
        public string ActionID
        {
            get
            {
                return _actionid;
            }
            set
            {
                _actionid = value;
            }
        }

        public void ControlByPermission(string authorizedActionid)
        {
            if (SetAuthoritiesByAction != null)
            {
                SetAuthoritiesByAction(this, authorizedActionid);
            }
            else
            {
                string[] ss = authorizedActionid.Split(',');
                bool isAuthrized = false;
                foreach (string s in ss)
                {
                    if (ActionID.IndexOf(s) >= 0)
                    {
                        isAuthrized = true;
                        break;
                    }
                }

                if (isAuthrized)
                {
                    this.Visible = true;
                }
                else
                {
                    this.Visible = false;
                }
            }
        }

        public event ActionSettingEvent SetAuthoritiesByAction;
    }
}
