using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
    public enum DepartmentType
    {
        RevenueByDepartment = 1,//phòng ban
        RevenueByDepartmentsaler = 2,//nhân viên
        RevenueByDepartmentSupplier = 3,//nhà cung cấp
        RevenueByDepartmentClient = 4,// khác hàng
    }
    public enum DepartmentName
    {
        PHONG_KDKS = 16,//Phòng kinh doanh khách sạn Hà Nội
        PHONG_LH1 = 20,//Phòng lữ hành 1 - Hà Nội
        DH_TU_LD = 36,//nhà cung cấp
        PHONG_Inbound = 38,// khác hàng
        PHONG_LH1_HCM = 20,//Phòng Lữ hành 1 - Hồ Chí Minh
        PHONG_LH3_HCM = 51,//Phòng Lữ hành 3 - Hồ Chí Minh
        PHONG_SK = 50,//Phòng Sự Kiện
        PHONG_LH2_HN = 47,//Phòng lữ hành 2 - Hà Nội
        PHONG_LH3_HN = 48,//phòng Lữ hành 3 - Hà Nội
        PHONG_LH1_HN = 14,//Phòng lữ hành 1 - Hà Nội
    }
}
