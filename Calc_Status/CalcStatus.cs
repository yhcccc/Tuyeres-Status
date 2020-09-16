/*----------------------------------------------------------------
* 项目名称 ：CalcStatus
* 项目描述 ：用于计算风口状态中的风口标准和实际风速，鼓风动能，
*           理论燃烧温度和风口回旋区大小
* 类 名 称 ：CalcStatus
* 类 描 述 ：计算风口状态参数
* 所在的域 ：WISDRI
* 命名空间 ：CalcStatus
* 机器名称 ：D-14535 
* CLR 版本 ：4.0.30319.42000
* 作    者 ：严晗
* 创建时间 ：2020/8/14 星期五 15:49:49
* 更新时间 ：2020/8/21 星期五 15:49:49
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ 严晗 2020. All rights reserved.
*******************************************************************
----------------------------------------------------------------*/

using System;
using static System.Math;
using System.IO;
using System.Linq;

namespace TuyeresStatus
{
    public class CalcStatus
    {
        // 已知量
        private const double lambda = 0.0126, P0 = 0.1013, rho = 1.293;
        // 沿程阻力系数, pi, 标准大气压(MPa), 标态下空气密度(kg*m-3)
        private const int T0 = 273; // 常温常数
        private readonly double Q0; // 总流量(注意单位转换)(m3/s)
        private readonly double[] d, S = new double[10]; // 风口直径(m); 风口截面积(m2)
        private readonly int[] m; // 直径di风口数量
        private readonly int n; // 风口总数量
        private readonly double[] mu; // 直径di风口的支路流阻比
        private readonly double T_Bl; // 高炉入炉风温(°C)

        // 中间量
        private readonly double prop; // 压强温度换算比
        private double v_actua_ave; // 平均实际风速(m/s)
        private readonly bool data_proper; // 判断输入数据是否合理

        // 计算量
        private readonly double[] Q = new double[10];// 各风口流量(m3/s)
        private readonly double[] v_stand = new double[10]; // 风口标准风速(m/s)
        private readonly double[] v_actua = new double[10]; // 风口实际风速(m/s)
        private readonly double[] KE = new double[10]; // 鼓风动能(kJ/s)
        private double KE_ave; // 平均鼓风动能(kJ/s)
        private double RAFT; // 理论燃烧温度(°C)
        private readonly double[] D_Raceway = new double[10]; // 风口回旋区深度(m)
        private readonly double[] H_Raceway = new double[10]; // 风口回旋区高度(m)
        private readonly double[] W_Raceway = new double[10]; // 风口回旋区宽度(m)
        private double D_Raceway_ave, H_Raceway_ave, W_Raceway_ave; // 平均值(m)

        /// <summary>
        /// 计算风口状态参数
        /// </summary>
        /// <param name="Actual_Q">实际入炉风量(m3/min)</param>
        /// <param name="T_Bl">高炉入炉风温(°C)</param>
        /// <param name="P_Bl">风压(MPa)</param>
        /// <param name="d">风口直径d(m)</param>
        /// <param name="m">直径di风口数量</param>
        /// <param name="mu">直径di风口的支路流阻比</param>
        public CalcStatus(double Actual_Q, double T_Bl, double P_Bl, double[] d, int[] m, double[] mu)
        {
            /*******  初值赋值   ********/
            this.d = d;
            this.m = m;
            this.mu = mu;
            this.T_Bl = T_Bl;

            if (m.Sum() <= 0 || d.Sum() <= 0 || mu.Sum() <= 0) { data_proper = false; }
            else
            {
                data_proper = true;
                n = m.Sum(); // 风口总数
                prop = (T0 + T_Bl) * P0 / ((P0 + P_Bl) * T0);
                Q0 = Actual_Q / 60.0; // 单位转换


                for (int i = 0; i < d.Length; i++)
                {
                    S[i] = PI * Pow(d[i] / 2, 2);
                }
            }
        }

        // 计算标准风速与实际风速
        public void CalcWindSpeed()
        {
            if (!data_proper) return;
            // 计算公式中的求和项
            double sigma = 0;
            double[] ri1 = new double[d.Length];
            for (int k = 0; k < d.Length; k++)
            {
                ri1[k] = (Sqrt(Pow(mu[0], 2) + 1) / Sqrt(Pow(mu[k], 2) + 1))
                    * (1 - 3 * Sqrt(lambda) * Log10(d[0] / d[k]))
                    * Pow(Sqrt(d[k] / d[0]), 5);
                sigma += m[k] * ri1[k];
            }

            // 计算风量和风速
            /*******  需要考虑边界情况 m=0  ********/
            for (int i = 0; i < d.Length; i++)
            {
                // 风量
                Q[i] = ri1[i] * Q0 / sigma;

                // 标准风速
                v_stand[i] = 4 * Q[i] / (PI * Pow(d[i], 2));
                if (m[i] == 0) { Q[i] = -1; v_stand[i] = -1; }

                // 实际风速
                v_actua[i] = v_stand[i] == -1 ? -1 : v_stand[i] * prop;
            }

            // 计算风口面积和
            double sum_S = 0;
            for (int i = 0; i < m.Length; i++)
            {
                sum_S += m[i] * S[i];
            }
            // 计算平均实际风速
            v_actua_ave = Q0 * prop / sum_S;
        }

        // 计算鼓风动能
        public void CalcKineticEnergy()
        {
            if (!data_proper) return;
            // 计算各风口鼓风动能
            for (int i = 0; i < d.Length; i++)
            {
                KE[i] = v_stand[i] == -1 ? -1 : rho * v_stand[i] * S[i] * Pow(v_actua[i], 2) / (2.0 * 1000);
            }

            // 计算平均鼓风动能
            KE_ave = rho * Q0 * Pow(v_actua_ave, 2) / (2.0 * n * 1000);
        }

        /// <summary>
        /// 计算风口回旋区大小
        /// </summary>
        /// <param name="M">煤比(kg/t)</param>
        /// <param name="D_pc">入炉焦炭的平均粒度(m)</param>
        /// <param name="L">风口长度(m)</param>
        public void CalcRacewaySize(double M, double D_pc, double L)
        {
            if (!data_proper) return;
            // 经验公式
            // 计算风口回旋区深度
            for (int i = 0; i < d.Length; i++)
            {
                D_Raceway[i] = v_stand[i] == -1 ? -1 : 0.88 + 0.0029 * KE[i] - 0.0176 * M / n;
            }


            // 计算风口回旋区高度
            for (int i = 0; i < d.Length; i++)
            {
                H_Raceway[i] = v_stand[i] == -1 ? -1 : 70.856 * Pow(Pow(v_actua[i], 2) / (9.8 * D_pc), -0.404) / Pow(D_Raceway[i], 0.286);
            }

            // 计算风口回旋区宽度
            for (int i = 0; i < d.Length; i++)
            {
                W_Raceway[i] = v_stand[i] == -1 ? -1 : 2.631 * Pow(D_Raceway[i] / L, 0.311) * L;
            }

            //计算平均值
            D_Raceway_ave = 0.88 + 0.0029 * KE_ave - 0.0176 * M / n;
            H_Raceway_ave = 70.856 * Pow(Pow(v_actua_ave, 2) / (9.8 * D_pc), -0.404) / Pow(D_Raceway_ave, 0.286);
            W_Raceway_ave = 2.631 * Pow(D_Raceway_ave / L, 0.311) * L;

        }

        /// <summary>
        /// 计算理论燃烧温度
        /// </summary>
        /// <param name="phi_Bl_H2O">鼓风湿度(g/m3)</param>
        /// <param name="OE">富氧率(%)</param>
        /// <param name="m_Coal">吨铁喷煤量(kg/tHM)</param>
        /// <param name="f_Coal_H2O">煤粉含水量(%)</param>
        /// <param name="V_Bl">吨铁风量(m3/tHM)</param>
        public void CalcCombustionTemp(double phi_Bl_H2O, double OE, double m_Coal, double f_Coal_H2O, double V_Bl)
        {
            if (!data_proper) return;
            // 经验线性公式
            RAFT = 1489 + 0.82 * T_Bl - 5.705 * phi_Bl_H2O + 52.778 * OE - 18.01 * (m_Coal * (1 - f_Coal_H2O)) / V_Bl;
        }

        // 展示计算结果
        public void Display()
        {
            if (!data_proper) 
            {
                Console.WriteLine("未输入数据，或未作计算，或输入数据有误");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("风口直径(m)\t风口风量(m3/s)\t标准风速(m/s)\t实际风速(m/s)\t鼓风动能(kJ/s)\t回旋区深度\t回旋区高度\t回旋区宽度");
            for (int i = 0; i < d.Length; i++)
            {
                Console.WriteLine("{0:N3}\t\t{1:N3}\t\t{2:N3}\t\t{3:N3}\t\t{4:N3}\t\t{5:N3}\t\t{6:N3}\t\t{7:N3}", d[i], Q[i], v_stand[i], v_actua[i], KE[i], D_Raceway[i], H_Raceway[i], W_Raceway[i]);
            }
            Console.WriteLine("理论燃烧温度RAFT:{0:N3}", RAFT);
            Console.WriteLine("平均鼓风动能:{0:N3}", KE_ave);
            Console.WriteLine("平均回旋区深度:{0:N3}", D_Raceway_ave);
            Console.WriteLine("平均回旋区高度:{0:N3}", H_Raceway_ave);
            Console.WriteLine("平均回旋区宽度:{0:N3}", W_Raceway_ave);
            Console.ReadKey();
        }

        // 计算结果写入文件 "v&KE&Q.txt"
        public void WriteTXT()
        {
            if (!data_proper)
            {
                Console.WriteLine("未输入数据，或未作计算，或输入数据有误");
                return;
            }

            using (StreamWriter sw = new StreamWriter(@"D:\WorkSpace\VS Project\Tuyeres-Status\v&KE&Q.txt", true))
            {
                for (int i = 0; i < d.Length; i++)
                {
                    sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", d[i], v_stand[i], v_actua[i], KE[i], Q[i], mu[1]);
                }
            }
        }
    }
}
