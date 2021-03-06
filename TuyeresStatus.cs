﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using TuyeresStatus;

namespace TuyeresStatusApplication
{
    class TuyeresStatus
    {
        static void Main()
        {
            /****************
            展示*/
            double Actual_Q = 5000; // 实际入炉风量(m3/min)
            double[] d = { 0.12, 0.13 }; // 直径种类(m)
            int[] m = { 11, 17 }; // 各风口个数，它们的和为风口总数量
            double[] mu = { 0.8186 * Sqrt(1.9), Sqrt(1.9) }; // 各支路流阻比;变直径系数sqrt((12+15/13+15)^5)=0.8337；或sqrt((12/13)^5)=0.8186
            double T_Bl = 1200, P_Bl = 0.41; // 高炉入炉风温(°C); 风压(MPa)
            double phi_Bl_H2O = 24, OE = 5, m_Coal = 160, f_Coal_H2O = 2, V_Bl = 1080;
            // 鼓风湿度(g/m3), 富氧率(%), 吨铁喷煤量(kg/tHM), 煤粉含水量(%), 吨铁风量(m3/tHM)
            double M = 160, D_pc = 0.04, L = 0.55; // 煤比(kg/t)，焦炭的平均粒度(m)，风口长度(m)

            CalcStatus Calc = new CalcStatus(Actual_Q, T_Bl, P_Bl, d, m, mu);
            Calc.CalcWindSpeed();
            Calc.CalcKineticEnergy();
            Calc.CalcCombustionTemp(phi_Bl_H2O, OE, m_Coal, f_Coal_H2O, V_Bl);
            Calc.CalcRacewaySize(m_Coal, D_pc, L);
            Calc.Display();

            /******************/

            //using (StreamWriter sw = new StreamWriter(@"D:\WorkSpace\VS Project\Tuyeres-Status\v&KE&Q.txt", false))
            //{
            //    sw.WriteLine("风口直径(m)\t标准风速(m/s)\t实际风速(m/s)\t鼓风动能(J/s)\t风口风量(m3/s)\t流阻比");
            //}

            //for (double u = 0.1; u <= 1; u += 0.001)
            //{
            //    double Actual_Q = 6000; // 实际入炉风量
            //    double[] d = { 0.12, 0.13 }; // 直径种类
            //    int[] m = { 5, 27 }; // 各风口个数，它们的和为风口总数量
            //    double[] mu = { 0.8186 * u, u }; // 各支路流阻比;变直径系数sqrt((12+15/13+15)^5)=0.8337
            //    double T_Bl = 1200, P_Bl = 0.412; // 高炉入炉风温(°C); 风压(MPa)
            //    double phi_Bl_H2O = 11.2, OE = 3.52, m_Coal = 193.8, f_Coal_H2O = 1.87, V_Bl = 1248;
            //    // 鼓风湿度(g/m3), 富氧率(%), 吨铁喷煤量(kg/tHM), 煤粉含水量(%), 吨铁风量(m3/tHM)

            //    CalcStatus calc = new CalcStatus(Actual_Q, d, m, mu, T_Bl, P_Bl, phi_Bl_H2O, OE, m_Coal, f_Coal_H2O, V_Bl);
            //    // calcstatus calc = new calcstatus(2350, 0.12, 0.14, 18, 1, 1200, 0.3, 11.2, 3.52, 193.8, 1.87, 1248);
            //    calc.CalcWindSpeed();
            //    calc.CalcKineticEnergy();
            //    calc.WriteTXT();
            //}


            //for (int m_small = 0; m_small <= 32; m_small++)
            //{
            //    double Actual_Q = 6000; // 实际入炉风量
            //    double[] d = { 0.12, 0.13 }; // 直径种类
            //    int[] m = { m_small, 32 - m_small }; // 各风口个数，它们的和为风口总数量
            //    double[] mu = { 0.8337 * Sqrt(1.9), Sqrt(1.9) }; // 各支路流阻比;变直径系数sqrt((12+15/13+15)^5)=0.8337
            //    double T_Bl = 1180, P_Bl = 0.412; // 高炉入炉风温(°C); 风压(MPa)
            //    double phi_Bl_H2O = 3, OE = 5, m_Coal = 155, f_Coal_H2O = 2, V_Bl = 1080;
            //    // 鼓风湿度(g/m3), 富氧率(%), 吨铁喷煤量(kg/tHM), 煤粉含水量(%), 吨铁风量(m3/tHM)
            //    double M = 160, D_pc = 0.04, L = 0.55; // 煤比，焦炭的平均粒度，风口长度

            //    CalcStatus Calc = new CalcStatus(Actual_Q, d, m, mu, T_Bl, P_Bl);
            //    // CalcStatus Calc = new CalcStatus(2350, 0.12, 0.14, 18, 1, 1200, 0.3, 11.2, 3.52, 193.8, 1.87, 1248);
            //    Calc.CalcWindSpeed();
            //    Calc.CalcKineticEnergy();
            //    Calc.CalcCombustionTemp(phi_Bl_H2O, OE, m_Coal, f_Coal_H2O, V_Bl);
            //    Calc.CalcRacewaySize(M, D_pc, L);
            //    Calc.WriteTXT();
            //}
        }
    }
}
