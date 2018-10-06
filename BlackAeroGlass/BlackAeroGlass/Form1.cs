using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp1 //就这名字吧
{
    public partial class Form1 : Form
    {
        string text1 = BlackAeroGlass.OperateIniFile.ReadIniData("Text", "value", null, System.IO.Directory.GetCurrentDirectory() + "\\cfg.ini");

        public Form1() { InitializeComponent(); }
        //取窗口句柄
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //显示窗体
            Visible = true;
            WindowState = FormWindowState.Normal;
            Activate();
        }
        private void EndToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //关闭程序
            notifyIcon1.Visible = false;
            Close();
            Dispose();
            System.Environment.Exit(System.Environment.ExitCode);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //启动循环线程
            Form1 f = new Form1();
            Thread nonParameterThread = new Thread(new ThreadStart(f.Cycle));
            nonParameterThread.Start();

            //读配置
            textBox1.Text = text1.Replace("&n", "\r\n");
            if (BlackAeroGlass.OperateIniFile.ReadIniData("win10", "value", null, System.IO.Directory.GetCurrentDirectory() + "\\cfg.ini") == "true")
            {
                checkBox1.Checked = true;
            } else { checkBox1.Checked = false; }

            //隐藏窗体
            WindowState = FormWindowState.Minimized;
            notifyIcon1.Visible = true;
            Visible = false;
            Hide();
            this.ShowInTaskbar = false;
        }
        #region dwmapi.dll
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        static extern void DwmExtendFrameIntoClientArea(int hwnd, ref MARGINS margins);

        public void LoadAero1(int hwnd)
        {
            MARGINS m = new MARGINS();
            m.Right = -1;
            DwmExtendFrameIntoClientArea(hwnd, ref m);
        }
        #endregion
        #region win10 能用的AeroAPI user32.dll
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(int hwnd, ref WindowCompositionAttributeData data);
        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        public void LoadAero2(int hwnd)
        {
            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(hwnd, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                //隐藏窗口 不要关闭窗口
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                notifyIcon1.Visible = true;
                Hide();
                return;
            }
        }

        private void Cycle()
        {
            //循环检测 设定延迟刷新←_←
            while(true)
            {
                Thread.Sleep(1000);
                text1 = text1.Replace("&n", "\r\n");
                string[] s = text1.Split(new char[] { '\n' });
                for (int i = 0; i < s.Length; i++)
                {
                    //替换掉字符 换行 回车 行文本拆开
                    s[i] = s[i].Replace("\r", "");
                    Debug.WriteLine(s[i]);
                    int IntPtrs= FindWindow(null, s[i]);
                    Debug.WriteLine(IntPtrs );
                    if (checkBox1.Checked)
                    {
                        LoadAero2(IntPtrs);
                    } else { LoadAero1(IntPtrs); }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //保存配置项
            string Text = textBox1.Text;
            bool returns = BlackAeroGlass.OperateIniFile.WriteIniData("Text", "value", Text.Replace("\r\n", "&n"), System.IO.Directory.GetCurrentDirectory() + "\\cfg.ini");
            if (checkBox1.Checked)
            {
              returns =  BlackAeroGlass.OperateIniFile.WriteIniData ("win10","value","true", System.IO.Directory.GetCurrentDirectory() +"\\cfg.ini");
            } else { returns = BlackAeroGlass.OperateIniFile.WriteIniData ("win10", "value", "false", System.IO.Directory.GetCurrentDirectory() + "\\cfg.ini");}
            if (returns)
            {
                MessageBox.Show("保存配置成功","提示",MessageBoxButtons.OK ,MessageBoxIcon.Asterisk );
            } else { MessageBox.Show("保存配置失败..", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error );  }
        }
    }
}
