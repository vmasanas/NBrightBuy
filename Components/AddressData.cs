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
    public class AddressData
    {
        private List<NBrightInfo> _addressList;
        private UserData _uData;

        public AddressData()
        {
            Exists = false;
            _uData = new UserData();
            _addressList = GetAddressList();
            //if we have no address create a default one from DNN profile
            if (GetDefaultAddress() == null && _uData.Exists)
            {
                //var strXml = GenXmlFunctions.GetGenXml(new RepeaterItem(0,ListItemType.Item)); //get a empty xml structure so we can add to it
                var newDefault = new NBrightInfo(true);
                //newDefault.XMLData = strXml;
                newDefault.AddSingleNode("default", "True", "genxml/hidden");
                var prop = DnnUtils.GetCurrentUserProfileProperties();
                foreach (var p in prop)
                {
                    newDefault.SetXmlProperty("genxml/textbox/" + p.Key.ToLower(), p.Value);
                }
                AddAddress(newDefault);
            }
        }


        /// <summary>
        /// Save cart
        /// </summary>
        private void Save(Boolean debugMode = false)
        {
            if (_uData.Exists)
            {
                //save cart
                var strXML = "<address>";
                foreach (var info in _addressList)
                {
                    strXML += info.XMLData;
                }
                strXML += "</address>";
                _uData.Info.RemoveXmlNode("genxml/address");
                _uData.Info.AddXmlNode(strXML, "address", "genxml");
                _uData.Save(debugMode);

                Exists = true;
            }
        }

        #region "base methods"

        /// <summary>
        /// Add Adddress
        /// </summary>
        /// <param name="rpData"></param>
        /// <param name="debugMode"></param>
        public String AddAddress(Repeater rpData, Boolean debugMode = false)
        {
            var strXml = GenXmlFunctions.GetGenXml(rpData, "", PortalSettings.Current.HomeDirectoryMapPath + SharedFunctions.ORDERUPLOADFOLDER);
            // load into NBrigthInfo class, so it's easier to get at xml values.
            var objInfoIn = new NBrightInfo();
            objInfoIn.XMLData = strXml;
            AddAddress(objInfoIn);
            return ""; // if everything is OK, don't send a message back.
        }

        public String AddAddress(NBrightInfo addressInfo, Boolean debugMode = false)
        {
            // load into NBrigthInfo class, so it's easier to get at xml values.
            if (debugMode) addressInfo.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_addressadd.xml");
            if (!AddressExists(addressInfo))
            {
                _addressList.Add(addressInfo);
                Save(debugMode);
            }
            return ""; // if everything is OK, don't send a message back.
        }

        public void RemoveAddress(int index)
        {
            _addressList.RemoveAt(index);
            Save();
        }

        public void UpdateAddress(Repeater rpData, int index)
        {
            if (_addressList.Count > index)
            {
                var strXml = GenXmlFunctions.GetGenXml(rpData, "", PortalSettings.Current.HomeDirectoryMapPath + SharedFunctions.ORDERUPLOADFOLDER);
                _addressList[index].XMLData = strXml;
                Save();
                UpdateDnnProfile(_addressList[index]);
            }
        }

        /// <summary>
        /// Get Current Cart Item List
        /// </summary>
        /// <returns></returns>
        public List<NBrightInfo> GetAddressList()
        {
            var rtnList = new List<NBrightInfo>();
            if (_uData.Exists)
            {
                var xmlNodeList = _uData.Info.XMLDoc.SelectNodes("genxml/address/*");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode carNod in xmlNodeList)
                    {
                        var newInfo = new NBrightInfo {XMLData = carNod.OuterXml};
                        rtnList.Add(newInfo);
                    }
                }
            }
            return rtnList;
        }

        public NBrightInfo GetAddress(int index)
        {
            return _addressList[index];
        }

        /// <summary>
        /// Set to true if cart exists
        /// </summary>
        public bool Exists { get; private set; }


        public NBrightInfo GetDefaultAddress()
        {
            NBrightInfo aInfo = null; 
            if (_uData.Exists)
            {
                var xmlNodeList = _uData.Info.XMLDoc.SelectNodes("genxml/address/*[./genxml/hidden/default='True']");
                if (xmlNodeList != null && xmlNodeList.Count > 0)
                {
                    aInfo = new NBrightInfo { XMLData = xmlNodeList[0].OuterXml };
                }
            }
            return aInfo;
        }

        #endregion

        #region "private functions"

        private Boolean AddressExists(NBrightInfo nInfo)
        {
            var newAddr = GetCompareAddress(nInfo);
            //we don;t know the index, so search for it, if not found then create a new one. If found ignore
            var found = false;
            foreach (var a in _addressList)
            {
                var addr = GetCompareAddress(a);
                if (addr == newAddr)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        private String GetCompareAddress(NBrightInfo nInfo)
        {
            var newAddr = "";
            var nodlist = nInfo.XMLDoc.SelectNodes("genxml/textbox/*");
            if (nodlist != null)
                foreach (XmlNode n in nodlist)
                {
                    newAddr += n.InnerText.Replace(" ", "").ToLower();
                }
            return newAddr;
        }

        private void UpdateDnnProfile(NBrightInfo defaultAddr)
        {
            if (defaultAddr.GetXmlProperty("genxml/hidden/default") == "True")
            {
                var flag = false;
                var prop1 = DnnUtils.GetCurrentUserProfileProperties();
                foreach (var p in prop1)
                {
                    var n = defaultAddr.XMLDoc.SelectSingleNode("genxml/textbox/" + p.Key.ToLower());
                    if (n != null)
                    {
                        prop1[p.Key] = n.InnerText;
                        flag = true;
                    }
                }
                if (flag) DnnUtils.SetCurrentUserProfileProperties(prop1);
            }
        }

        #endregion


    }
}
