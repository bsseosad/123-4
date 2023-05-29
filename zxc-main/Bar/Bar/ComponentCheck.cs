using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bar
{
    public partial class ComponentCheck : Form
    {
        List<string> ComponentList = new List<string>();
        private string barcode;
        private string inverter;
        public ComponentCheck(string barcode, string inverter)
        {

            this.inverter = inverter;
            this.barcode = barcode;
            InstallHook();
            InitializeComponent();
        }

        private void ComponentCheck_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = ConnectDB.connectDB_TEAMDB())
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT CAP,IGBT FROM [dbo].[inverter_component] where barcode=@barcode";
                    cmd.Parameters.AddWithValue("@barcode", barcode);
                    try
                    {
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {

                                for (int i = 0; i < 2; i++)    //이부분은 정확하게 수정하기
                                {
                                    if (dr[i] != null)
                                    {
                                        TextBox textBox = new TextBox();
                                        textBox.Location = new Point(10, 20 + i * 30);
                                        textBox.Size = new Size(200, 20);
                                        this.Controls.Add(textBox);
                                        textBox.Text = dr[i].ToString();                                                                         
                                        CheckBox checkbox = new CheckBox();
                                        checkbox.Text = ""+dr[i].ToString()+i;
                                        checkbox.Location = new System.Drawing.Point(250, 20 + i * 30);
                                        checkbox.CheckedChanged += Checkbox_CheckedChanged;
                                        Controls.Add(checkbox);                                       
                                    }
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("오류발생");
                        Log.writeLog(ex.ToString());
                    }
                    cmd.CommandText = $"SELECT CAP_BARCODE,IGBT_BARCODE FROM [dbo].[inverter_component] where barcode=@barcode";
                    try
                    {
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                for (int i = 0; i < 2; i++)    //이부분은 정확하게 수정하기
                                {
                                    if (dr[i] != null)
                                    {
                                        ComponentList.Add(dr[i].ToString());

                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("오류발생");
                        Log.writeLog(ex.ToString());
                    }
                }
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private string barcodeData = "";

        private IntPtr hookHandle = IntPtr.Zero;

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    char key = (char)vkCode;

                    if (key == '\r')
                    {
                        string cleanedBarcodeData = barcodeData.Trim();
                        cleanedBarcodeData = RemoveSpecialCharacters(cleanedBarcodeData);
                        for (int i = 0; i < ComponentList.Count; i++)
                        {
                            if (cleanedBarcodeData == barcodeData)
                            {
                                MessageBox.Show("맞아요"); // 수정하기 
                                barcodeData = "";
                                break;
                            }
                        }

                    }

                    else
                    {
                        barcodeData += key.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.writeLog(ex.ToString());
            }
            return CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        private string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if (c >= 32 && c <= 126)
                {
                    sb.Append(c);
                }

            }
            return sb.ToString();
        }

        // Hook Install
        private void InstallHook()
        {
            LowLevelKeyboardProc proc = HookProc;
            IntPtr moduleHandle = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, proc, moduleHandle, 0);
        }

        // Hook Uninstall
        private void UninstallHook()
        {
            if (hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hookHandle);
                hookHandle = IntPtr.Zero;
            }
        }
        private void Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            if (checkbox.Checked)
            {
                Console.WriteLine(checkbox.Text + "가 선택되었습니다.");
            }
            else
            {
                Console.WriteLine(checkbox.Text + "가 해제되었습니다.");
            }
        }
    }
}
