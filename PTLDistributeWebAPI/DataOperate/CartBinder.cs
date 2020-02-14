using PTLDistributeWebAPI.ForCartFindingClientService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Xml;

namespace PTLDistributeWebAPI.DataOperate
{
    public class CartBinder
    {
        /// <summary>
        /// 停靠小车
        /// </summary>
        /// <param name="nCartID"></param>
        /// <param name="sName"></param>
        /// <param name="sDescription"></param>
        /// <param name="nCount"></param>
        /// <param name="sUnit"></param>
        public static void DockCart(int nCartID, string sName, string sDescription, int nCount, string sUnit)
        {
            //设备控制
            //CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(nCartID);
            //Ptl900U ptl900UPublisher = cartPtl.GetPtl900UPublisher();

            //Display900UItem publisherDisplay900UItem = new Display900UItem();
            //publisherDisplay900UItem.Name = sName;
            //publisherDisplay900UItem.Description = sDescription;
            //publisherDisplay900UItem.Count = (ushort)nCount;
            //publisherDisplay900UItem.Unit = sUnit;

            //ptl900UPublisher.Clear(true);
            //ptl900UPublisher.Lock();
            //ptl900UPublisher.Display(publisherDisplay900UItem, LightColor.Off);

            BasicHttpBinding BINDING = new BasicHttpBinding("BasicHttpBinding_IForCartFindingClientService");
            string endpoint = GetSoapRemoteAddress("BasicHttpBinding_IForCartFindingClientService");
            ForCartFindingClientServiceClient service = new ForCartFindingClientServiceClient(BINDING, new EndpointAddress(endpoint));
            string result = service.DockCart(nCartID, sName, sDescription, nCount, sUnit);
            if (!result.Equals("Success"))
            {
                throw new Exception(result);
            }
        }

        /// <summary>
        /// 解绑小车
        /// </summary>
        /// <param name="nCartID"></param>
        public static void UnDockCart(int nCartID)
        {
            BasicHttpBinding BINDING = new BasicHttpBinding("BasicHttpBinding_IForCartFindingClientService");
            string endpoint = GetSoapRemoteAddress("BasicHttpBinding_IForCartFindingClientService");
            ForCartFindingClientServiceClient service = new ForCartFindingClientServiceClient(BINDING, new EndpointAddress(endpoint));
            service.UnDockCart(nCartID);
        }

        /// <summary>
        /// 从config文件获取WebService的连接地址
        /// </summary>
        /// <param name="endPointName">SOAP endPointName</param>
        /// <returns></returns>
        private static string GetSoapRemoteAddress(string endPointName)
        {
            string ConfigFile = AppDomain.CurrentDomain.BaseDirectory + "Web.config";
            XmlDocument xml = new XmlDocument();
            xml.Load(ConfigFile);
            string xpath = "configuration/system.serviceModel/client/endpoint[@name='" + endPointName + "']";
            XmlNode node = xml.SelectSingleNode(xpath);
            return node.Attributes["address"].Value;
        }
    }
}