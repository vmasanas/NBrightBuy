﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using NEvoWeb.Modules.NB_Store;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class UserData
    {
        public NBrightInfo Info;
        private UserInfo _userInfo;

        public UserData()
        {
            Exists = false;
            _userInfo = UserController.GetCurrentUserInfo();
            if (_userInfo.UserID != -1) // only create userdata if we have a user logged in.
            {
                var modCtrl = new NBrightBuyController();
                Info = modCtrl.GetByType(_userInfo.PortalID, -1, "USERDATA", _userInfo.UserID.ToString(""));
                if (Info == null && _userInfo.UserID != -1)
                {
                    Info = new NBrightInfo();
                    Info.ItemID = -1;
                    Info.UserId = _userInfo.UserID;
                    Info.PortalId = _userInfo.PortalID;
                    Info.ModuleId = -1;
                    Info.TypeCode = "USERDATA";
                    Info.XMLData = "<genxml></genxml>";
                    Save();
                }
                else
                    Exists = true;
            }
        }

        /// <summary>
        /// Save cart
        /// </summary>
        public void Save(Boolean debugMode = false)
        {
            if (Info != null)
            {
                var modCtrl = new NBrightBuyController();
                Info.ItemID = modCtrl.Update(Info);
                if (debugMode) Info.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_userdata.xml");
                Exists = true;
            }
        }

        public void DeleteUserData()
        {
            //remove DB record
            var modCtrl = new NBrightBuyController();
            modCtrl.Delete(Info.ItemID);
            Exists = false;
        }

        /// <summary>
        /// Get NBright UserData
        /// </summary>
        /// <returns></returns>
        public NBrightInfo GetUserData()
        {
            return Info;
        }

        /// <summary>
        /// Set to true if usedata exists
        /// </summary>
        public bool Exists { get; private set; }


    }
}
