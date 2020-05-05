using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;
using Tesseract;
using System.Runtime.InteropServices;
using System.Threading;
using YandexTranslate;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;

namespace CudafyByExample
{
    public class add_loop_gpu_alt
    {
        public const int N = 10;

        public static void Execute()
        {
            CudafyModule km = CudafyTranslator.Cudafy();

            GPGPU gpu = CudafyHost.GetDevice(eGPUType.Cuda);
            gpu.LoadModule(km);

            int[] a = new int[N];
            int[] b = new int[N];
            int[] c = new int[N];

            // allocate the memory on the GPU
            int[] dev_c = gpu.Allocate<int>(c);

            // fill the arrays 'a' and 'b' on the CPU
            for (int i = 0; i < N; i++)
            {
                a[i] = -i;
                b[i] = i * i;
            }

            // copy the arrays 'a' and 'b' to the GPU
            int[] dev_a = gpu.CopyToDevice(a);
            int[] dev_b = gpu.CopyToDevice(b);
            gpu.Launch(N, 1).add(dev_a, dev_b, dev_c);

            // copy the array 'c' back from the GPU to the CPU
            gpu.CopyFromDevice(dev_c, c);

            // display the results
            for (int i = 0; i < N; i++)
            {
                Console.WriteLine("{0} + {1} = {2}", a[i], b[i], c[i]);
            }

            // free the memory allocated on the GPU
            gpu.FreeAll();
        }

        [Cudafy]
        public static void add(GThread thread, int[] a, int[] b, int[] c)
        {
            int tid = thread.blockIdx.x;
            if (tid < N)
                c[tid] = a[tid] + b[tid];
        }
    }
}
namespace ShiukiTranslator
{
    [Cudafy(eCudafyType.Global)]
    public partial class Furry : Form
    {




        public Furry()
        {

            InitializeComponent();
            var aTimer = new System.Timers.Timer(1500);

            aTimer.Elapsed += new ElapsedEventHandler(ticku);

            aTimer.Interval = 1500;
            aTimer.Enabled = true;
        }


        [Cudafy]
        private void Button1_Click_1(object sender, EventArgs e)
        {

            try
            {

                Bitmap image = new Bitmap(CaptureApplication(finalfortrans));

                string FilePath = @Application.StartupPath + "/latest.png";

                image.Save(FilePath, System.Drawing.Imaging.ImageFormat.Png);

                var Result = GetText(image);






                Console.WriteLine(@Result);
                if (Result != "")
                {
                    richTextBox1.Text = $"{TranslateText(@Result)}";
                }
                else
                {
                    richTextBox1.Text = $"Error, Couldn't detect text on language: {froml} on Process {finalfortrans}  \nThe image might be blurry or maybe the From language is wrong, also did you remember to choose right process?";
                }
                image.Dispose();
            }
            catch
            {
                richTextBox1.Text = $"Error, Couldn't detect text on language: {froml} on Process {finalfortrans}  \nThe image might be blurry or maybe the From language is wrong, also did you remember to choose right process?" ;
            }
        }
        static string froml = "eng";
        [Cudafy]
        public static string GetText(Bitmap imgsource)
        {
            var ocrtext = string.Empty;
            using (var engine = new TesseractEngine(@"./tessdata", froml, EngineMode.Default))
            {
                using (var img = PixConverter.ToPix(imgsource))
                {
                    using (var page = engine.Process(img))
                    {
                        ocrtext = page.GetText();
                    }
                }
            }

            return ocrtext;
        }
        public static string lang = "en";
        [Cudafy]
        public string TranslateText(string input)
        {
            YandexTranslator yandexTranslator = new YandexTranslator();
            return yandexTranslator.translate(@input, "trnsl.1.1.20190711T150343Z.58bc9e7bb4409821.e42fded912dd04452d0e0efbeb771da70d10deb6",lang);
        }
        static int selected;
        static string finalfortrans;
        private void Button2_Click(object sender, EventArgs e)
        {
            Process[] processlist = Process.GetProcesses();
            listBox1.Items.Clear();
            foreach (Process theprocess in processlist)
            {
                listBox1.Items.Add((theprocess.ProcessName, theprocess.Id));
            }
            for (int n = listBox1.Items.Count - 1; n >= 0; --n)
            {
                string removelistitem = "host";
                if (listBox1.Items[n].ToString().Contains(removelistitem))
                {
                    listBox1.Items.RemoveAt(n);
                }
            }
        }
        [Cudafy]
        private void ListBox1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                selected = listBox1.SelectedIndex;
                string cursel = listBox1.SelectedItem.ToString();
                string cut = cursel.Replace("(", "");
                finalfortrans = cut.Remove(cut.LastIndexOf(","));
                string final = cut.Remove(cut.LastIndexOf(","));
                label3.Text = final;
                Console.WriteLine(finalfortrans);
            }
            catch
            {

            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);
        [Cudafy]
        public Bitmap CaptureApplication(string procName)
        {
            Process proc;

            try
            {
                proc = Process.GetProcessesByName(procName)[0];
            }
            catch (IndexOutOfRangeException e)
            {
                return null;
            }



            if (checkBox1.Checked != true)
            {
                ShowWindow(proc.MainWindowHandle, SW_RESTORE);

                SetForegroundWindow(proc.MainWindowHandle);
            }
            Thread.Sleep(100);

            Rect rect = new Rect();
            IntPtr error = GetWindowRect(proc.MainWindowHandle, ref rect);

            while (error == (IntPtr)0)
            {
                error = GetWindowRect(proc.MainWindowHandle, ref rect);
            }

            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format64bppPArgb);
            
            Graphics.FromImage(bmp).CopyFromScreen(rect.left,
                                                   rect.top,
                                                   0,
                                                   0,
                                                   new Size(width, height),
                                                   CopyPixelOperation.SourceErase);

            return bmp;
        }
        [Cudafy]
        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    listBox1.SetSelected(i, false);
                    if (listBox1.GetItemText(listBox1.Items[i]).Contains(textBox1.Text))
                    {
                        listBox1.SetSelected(i, true);
                        try
                        {
                            selected = listBox1.SelectedIndex;
                            string cursel = listBox1.SelectedItem.ToString();
                            string cut = cursel.Replace("(", "");
                            finalfortrans = cut.Remove(cut.LastIndexOf(","));
                            string final = cut.Remove(cut.LastIndexOf(","));
                            label3.Text = final;
                            Console.WriteLine(finalfortrans);
                        }
                        catch
                        {

                        }
                    }

                }
            }
        }

       

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            froml = comboBox1.SelectedItem.ToString();
        }

        private void ComboBox1_MouseClick(object sender, MouseEventArgs e)
        {
            comboBox1.Items.Clear();
            DirectoryInfo d = new DirectoryInfo(@"./tessdata");
            FileInfo[] Files = d.GetFiles("*.traineddata"); 
            string strr = "en";
            foreach (FileInfo file in Files)
            {
                strr = file.Name.Replace(".traineddata","");
                comboBox1.Items.Add(strr);
            }
        }

        private void ComboBox2_MouseClick(object sender, MouseEventArgs e)
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("ml");
            comboBox2.Items.Add("mt");
            comboBox2.Items.Add("mk");
            comboBox2.Items.Add("mi");
            comboBox2.Items.Add("mr");
            comboBox2.Items.Add("mhr");
            comboBox2.Items.Add("mn");
            comboBox2.Items.Add("de");
            comboBox2.Items.Add("ne");
            comboBox2.Items.Add("no");
            comboBox2.Items.Add("pa");
            comboBox2.Items.Add("pap");
            comboBox2.Items.Add("fa");
            comboBox2.Items.Add("pl");
            comboBox2.Items.Add("pt");
            comboBox2.Items.Add("ro");
            comboBox2.Items.Add("ru");
            comboBox2.Items.Add("ceb");
            comboBox2.Items.Add("sr");
            comboBox2.Items.Add("si");
            comboBox2.Items.Add("sk");
            comboBox2.Items.Add("sl");
            comboBox2.Items.Add("sw");
            comboBox2.Items.Add("su");
            comboBox2.Items.Add("tg");
            comboBox2.Items.Add("th");
            comboBox2.Items.Add("tl");
            comboBox2.Items.Add("ta");
            comboBox2.Items.Add("tt");
            comboBox2.Items.Add("te");
            comboBox2.Items.Add("tr");
            comboBox2.Items.Add("udm");
            comboBox2.Items.Add("uz");
            comboBox2.Items.Add("uk");
            comboBox2.Items.Add("ur");
            comboBox2.Items.Add("fi");
            comboBox2.Items.Add("fr");
            comboBox2.Items.Add("hi");
            comboBox2.Items.Add("hr");
            comboBox2.Items.Add("cs");
            comboBox2.Items.Add("sv");
            comboBox2.Items.Add("gd");
            comboBox2.Items.Add("et");
            comboBox2.Items.Add("eo");
            comboBox2.Items.Add("jv");
            comboBox2.Items.Add("ja");
            comboBox2.Items.Add("az");
            comboBox2.Items.Add("sq");
            comboBox2.Items.Add("am");
            comboBox2.Items.Add("en");
            comboBox2.Items.Add("ar");
            comboBox2.Items.Add("hy");
            comboBox2.Items.Add("af");
            comboBox2.Items.Add("eu");
            comboBox2.Items.Add("ba");
            comboBox2.Items.Add("be");
            comboBox2.Items.Add("bn");
            comboBox2.Items.Add("my");
            comboBox2.Items.Add("bg");
            comboBox2.Items.Add("bs");
            comboBox2.Items.Add("cy");
            comboBox2.Items.Add("hu");
            comboBox2.Items.Add("vi");
            comboBox2.Items.Add("ht");
            comboBox2.Items.Add("gl");
            comboBox2.Items.Add("nl");
            comboBox2.Items.Add("mrj");
            comboBox2.Items.Add("el");
            comboBox2.Items.Add("ka");
            comboBox2.Items.Add("gu");
            comboBox2.Items.Add("da");
            comboBox2.Items.Add("he");
            comboBox2.Items.Add("yi");
            comboBox2.Items.Add("id");
            comboBox2.Items.Add("ga");
            comboBox2.Items.Add("it");
            comboBox2.Items.Add("is");
            comboBox2.Items.Add("es");
            comboBox2.Items.Add("kk");
            comboBox2.Items.Add("kn");
            comboBox2.Items.Add("ca");
            comboBox2.Items.Add("ky");
            comboBox2.Items.Add("zh");
            comboBox2.Items.Add("ko");
            comboBox2.Items.Add("xh");
            comboBox2.Items.Add("km");
            comboBox2.Items.Add("lo");
            comboBox2.Items.Add("la");
            comboBox2.Items.Add("lv");
            comboBox2.Items.Add("lt");
            comboBox2.Items.Add("lb");
            comboBox2.Items.Add("mg");
            comboBox2.Items.Add("ms");
            comboBox2.Items.Add("");
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            lang = comboBox2.SelectedItem.ToString();
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();



        Point mouseDownPoint = Point.Empty;
       

        private void ToolStrip1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
        string lateees = "";

        private void ticku(object source, ElapsedEventArgs e)
        {
            
            CheckForIllegalCrossThreadCalls = false;
            if (checkBox1.Checked == true)
            {
                try
                {

                    Bitmap image = new Bitmap(CaptureApplication(finalfortrans));

                    string FilePath = @Application.StartupPath + "/latestauto.png";

                    image.Save(FilePath, System.Drawing.Imaging.ImageFormat.Png);

                    var Result = GetText(image);



                    if (Result != "" && lateees != Result)
                    {
                        Console.WriteLine(@Result);
                        richTextBox1.Text = $"{TranslateText(@Result)}";
                        lateees = Result;
                    }
                    image.Dispose();
                }
                catch
                {
                }
            }
        }
    }
}

