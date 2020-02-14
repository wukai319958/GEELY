using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DataAccess.Config;
using DataAccess.Distributing;
using System;

namespace DataAccess
{
    /// <summary>
    /// 提供基础数据初始化。
    /// </summary>
    public static class BaseDatasInitializer
    {
        /// <summary>
        /// 确保基础数据已经初始化。
        /// </summary>
        public static void EnsureInitialized()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                if (!dbContext.CFG_Employees.Any())
                {
                    //操作员
                    CFG_Employee cfgEmployee = new CFG_Employee();
                    cfgEmployee.Code = "Administrator";
                    cfgEmployee.Name = "管理员";
                    cfgEmployee.LoginName = "admin";
                    cfgEmployee.Password = string.Empty;
                    cfgEmployee.IsEnabled = true;

                    dbContext.CFG_Employees.Add(cfgEmployee);

                    //巷道
                    //转台上的标签，托盘有 5 个库位，但只在单侧使用 3 个标签
                    //7 个巷道共用 1 个 4 口 XGate，按现场走线，总线 1 对应 1、2 巷道，总线 2 对应 3 巷道，总线 3 对应 4、5 巷道，总线 4 对应 6、7 巷道
                    //标签地址分别为：1、2、3，4、5、6，51、52、53，101、102、103，104、105、106，151、152、153，154、155、156。
                    //托盘库位定义为：1、2 在远端需旋转，3、4、5 在近段
                    string channelXGateIP = "10.34.36.17";
                    Dictionary<int, byte[]> channelPtl900UAddressesByChannelNumber = new Dictionary<int, byte[]>();
                    channelPtl900UAddressesByChannelNumber.Add(1, new byte[] { 3, 1, 1, 2, 3 });
                    channelPtl900UAddressesByChannelNumber.Add(2, new byte[] { 6, 4, 4, 5, 6 });
                    channelPtl900UAddressesByChannelNumber.Add(3, new byte[] { 53, 51, 51, 52, 53 });
                    channelPtl900UAddressesByChannelNumber.Add(4, new byte[] { 103, 101, 101, 102, 103 });
                    channelPtl900UAddressesByChannelNumber.Add(5, new byte[] { 106, 104, 104, 105, 106 });
                    channelPtl900UAddressesByChannelNumber.Add(6, new byte[] { 153, 151, 151, 152, 153 });
                    channelPtl900UAddressesByChannelNumber.Add(7, new byte[] { 156, 154, 154, 155, 156 });

                    for (int channelNumber = 1; channelNumber <= channelPtl900UAddressesByChannelNumber.Count; channelNumber++)
                    {
                        CFG_Channel cfgChannel = new CFG_Channel();
                        cfgChannel.Code = string.Format(CultureInfo.InvariantCulture, "{0}", channelNumber);
                        cfgChannel.Name = string.Format(CultureInfo.InvariantCulture, "巷道 {0}", channelNumber);

                        dbContext.CFG_Channels.Add(cfgChannel);

                        //转台上的标签
                        byte[] deviceAddresses = channelPtl900UAddressesByChannelNumber[channelNumber];
                        for (int position = 1; position <= 5; position++)
                        {
                            byte deviceAddress = deviceAddresses[position - 1];
                            byte busIndex = (byte)(deviceAddress / 50);

                            CFG_ChannelPtlDevice cfgChannelPtlDevice = new CFG_ChannelPtlDevice();
                            cfgChannelPtlDevice.CFG_Channel = cfgChannel;
                            cfgChannelPtlDevice.Position = position;
                            cfgChannelPtlDevice.XGateIP = channelXGateIP;
                            cfgChannelPtlDevice.RS485BusIndex = busIndex;
                            cfgChannelPtlDevice.Ptl900UAddress = deviceAddress;

                            dbContext.CFG_ChannelPtlDevices.Add(cfgChannelPtlDevice);
                        }

                        //巷道边的 4 个车位，多加两个备用车位
                        for (int position = 1; position <= 6; position++)
                        {
                            CFG_ChannelCurrentCart cfgChannelCurrentCart = new CFG_ChannelCurrentCart();
                            cfgChannelCurrentCart.CFG_Channel = cfgChannel;
                            cfgChannelCurrentCart.Position = position;

                            dbContext.CFG_ChannelCurrentCarts.Add(cfgChannelCurrentCart);
                        }

                        //巷道上的 1 个托盘
                        CFG_ChannelCurrentPallet cfgChannelCurrentPallet = new CFG_ChannelCurrentPallet();
                        cfgChannelCurrentPallet.CFG_Channel = cfgChannel;

                        dbContext.CFG_ChannelCurrentPallets.Add(cfgChannelCurrentPallet);
                    }

                    //小车
                    for (int cartNumber = 1; cartNumber <= 100; cartNumber++)
                    {
                        CFG_Cart cfgCart = new CFG_Cart();
                        cfgCart.Code = string.Format(CultureInfo.InvariantCulture, "{0:000000}", 100000 + cartNumber);
                        cfgCart.Name = string.Format(CultureInfo.InvariantCulture, "料车 {0}", cartNumber);
                        cfgCart.Rfid1 = "AABBCCDDEEFF";
                        cfgCart.XGateIP = "192.168.0.10";
                        cfgCart.CartStatus = CartStatus.Free;

                        dbContext.CFG_Carts.Add(cfgCart);

                        //小车上的 8 个库位
                        for (int position = 1; position <= 8; position++)
                        {
                            CFG_CartCurrentMaterial cfgCartCurrentMaterial = new CFG_CartCurrentMaterial();
                            cfgCartCurrentMaterial.CFG_Cart = cfgCart;
                            cfgCartCurrentMaterial.Position = position;
                            cfgCartCurrentMaterial.Usability = CartPositionUsability.Enable;

                            dbContext.CFG_CartCurrentMaterials.Add(cfgCartCurrentMaterial);
                        }

                        //小车上的 10 个标签
                        for (byte deviceAddress = 1; deviceAddress <= 10; deviceAddress++)
                        {
                            CFG_CartPtlDevice cfgCartPtlDevice = new CFG_CartPtlDevice();
                            cfgCartPtlDevice.CFG_Cart = cfgCart;
                            cfgCartPtlDevice.DeviceAddress = deviceAddress;

                            dbContext.CFG_CartPtlDevices.Add(cfgCartPtlDevice);
                        }
                    }

                    //dbContext.SaveChanges();
                }

                ////线边工位的8个车位
                //List<CFG_WorkStation> cfgWorkStations = dbContext.CFG_WorkStations.ToList();
                //foreach (CFG_WorkStation cfgWorkStation in cfgWorkStations)
                //{
                //    if (cfgWorkStation.CFG_WorkStationCurrentCarts != null && cfgWorkStation.CFG_WorkStationCurrentCarts.Count > 0)
                //    {
                //        continue;
                //    }

                //    for (int position = 1; position <= 8; position++)
                //    {
                //        CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = new CFG_WorkStationCurrentCart();
                //        cfgWorkStationCurrentCart.CFG_WorkStation = cfgWorkStation;
                //        cfgWorkStationCurrentCart.Position = position;

                //        dbContext.CFG_WorkStationCurrentCarts.Add(cfgWorkStationCurrentCart);
                //    }
                //}

                //AGV开关信息
                if (!dbContext.DST_AgvSwitchs.Any())
                {
                    DST_AgvSwitch dstAgvSwitch = new DST_AgvSwitch();
                    dstAgvSwitch.isOpen = false;
                    dstAgvSwitch.lastCloseTime = DateTime.Now;

                    dbContext.DST_AgvSwitchs.Add(dstAgvSwitch);
                }

                dbContext.SaveChanges();
            }
        }
    }
}