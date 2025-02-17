using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
     public class ClientType
    {
        public static int Saller = 6; //Saller
        public static int kl = 5; //khách le
        public static int DLC3 = 4; //Đại lý cấp 3
        public static int DALC2 = 3; //Đại lý cấp 2
        public static int DALC1 = 2; //Đại lý cấp 1
        public static int DL = 1; //Đại lý
        public static int DN = 7; //Doanh nghiêp
        public static int CTV = 8; //Cộng tác viên
        public static int NV = 9; //Nhân viên trong công ty
    }
    public enum ClientTypeEnum
    {
        DL = 1, //Đại lý
        DALC1 = 2, //Đại lý cấp 1
        DALC2 = 3, //Đại lý cấp 2
        DLC3 = 4, //Đại lý cấp 3
        kl = 5, //khách le
        Saller = 6, //Saller
        DN = 7, //Doanh nghiêp
        CTV = 8, //Cộng tác viên
        NV = 9, //Nhân viên trong công ty
    }
    public static class ClientTypeName
    {
        public static readonly Dictionary<Int16, string> service = new Dictionary<Int16, string>
        {
            {Convert.ToInt16(ClientTypeEnum.DL), "DL" },
            {Convert.ToInt16(ClientTypeEnum.DALC1), "CL" },
            {Convert.ToInt16(ClientTypeEnum.DALC2), "DL" },
            {Convert.ToInt16(ClientTypeEnum.DLC3), "DL" },
            {Convert.ToInt16(ClientTypeEnum.kl), "KL" },
            {Convert.ToInt16(ClientTypeEnum.Saller), "SL" },
            {Convert.ToInt16(ClientTypeEnum.DN), "DN" },
            {Convert.ToInt16(ClientTypeEnum.CTV), "CT" },
            {Convert.ToInt16(ClientTypeEnum.NV), "NV" }
        };
    }
}
