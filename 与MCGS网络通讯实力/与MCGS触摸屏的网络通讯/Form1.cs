using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace 与MCGS触摸屏的网络通讯
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }
        Socket socketSend;
        string m;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //创建个一Socket对象,使用IPv4地址,使用数据流,使用TCP协议
                socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //获得一个IP地址,服务器的地址,就是触屏上设置的地址
                IPAddress ip = IPAddress.Parse(textBox1.Text);
                //获取一个网络终结点,有IP地址和端口号组成,端口号就是触屏设置的端口
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(textBox2.Text));
                //把网络终结点添加到Socket类中,这样就链接上了
                socketSend.Connect(point);
                //把链接成功的信息加载到文本框中
                textBox4.AppendText("链接成功" + "\r\n");
                //创建一个线程，用于接收服务器发来的消息
                Thread th = new Thread(Recive);
                //把线程设置成后台运行
                th.IsBackground = true;
                //启动线程
                th.Start();
                timer1.Enabled = true;


            }
            catch (Exception)
            {

                MessageBox.Show("链接失败", "错误");
            }
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                //先创建一个字节数组，大小是12个字节，都是16进制的
                byte[] buffer = new byte[12];
                //事务处理序号
                buffer[0] = 0x00;
                buffer[1] = 0x00;
                //协议标识符
                buffer[2] = 0x00;
                buffer[3] = 0x00;
                //随后字节数，06代表后面有6个字节
                buffer[4] = 0x00;
                buffer[5] = 0x06;
                //单元标识符，这个就是我们之前在触屏上设置的设备地址了
                buffer[6] = 0x01;//MBAP报文头
                //功能码，03是用于读取4区的输出寄存器的（这个我是看触屏上的功能码说明）
                buffer[7] = 0x03;
                //寄存器起始地址，（触屏上的设备通道为4XXX0001）
                buffer[8] = 0x00;
                buffer[9] = 0x00;
                //请求字节长度02代表了要读4个字节长度（触屏上代表了一个32位数）
                buffer[10] = 0x00;
                buffer[11] = 0x02;
                //通过网口把请求命令发出去
                socketSend.Send(buffer);
                
            }
            catch
            {

            }

        }
        void Recive()
        {
            try
            {

                while (true)
                {
                    //创建一个字节数组，用于接收从触屏返回的数据
                    byte[] buffer = new byte[258];
                    //接收从触屏返回的数据，并返回接收数据的个数
                    int r = socketSend.Receive(buffer);
                    //当就收的数据等于0，就跳出循环
                    if (r == 0) break;
                    //声明一个字符串，用于接收从字节数组转换成16进制数的字符串
                    string s = "";
                    //接收到的事务处理序号
                    s += Convert.ToString(buffer[0], 16) + "  ";
                    s += Convert.ToString(buffer[1], 16) + "  ";
                    //接收到的协议标识符
                    s += Convert.ToString(buffer[2], 16) + "  ";
                    s += Convert.ToString(buffer[3], 16) + "  ";
                    //接收到的字节数，后面有多少数据
                    s += Convert.ToString(buffer[4], 16) + "  ";
                    s += Convert.ToString(buffer[5], 16) + "  ";
                    //接收到的单元标识符
                    s += Convert.ToString(buffer[6], 16) + "  ";//之前的都是MBAP报文头
                    //接收到的功能码，就是我们发送的03
                    s += Convert.ToString(buffer[7], 16) + "  ";
                    //接收到字节数，这里是4，就是我们之前写的02
                    s += Convert.ToString(buffer[8], 16) + "  ";
                    //下面的数据就是我们要的数据了，前面的数据就没什么用
                    s += Convert.ToString(buffer[9], 16) + "  ";
                    s += Convert.ToString(buffer[10], 16) + "  ";
                    s += Convert.ToString(buffer[11], 16) + "  ";
                    s += Convert.ToString(buffer[12], 16) + "  ";
                   // MessageBox.Show(s);
                    //下面我们来对数据进行处理
                    //如果要读取的数据是16位数据时，先要截取出我们需要的两位数据
                    byte[] buffer1 = buffer.Skip(9).Take(4).ToArray();
                   
                    //接着对数据进行高低位数据交换，
                   // byte bat = buffer1[0];
                   // byte bat1 = buffer1[1];
                   // buffer1[0] = bat1;
                   // buffer1[1] = bat;
                    //这样就可以获得16位数据了
                    ushort sh = BitConverter.ToUInt16(buffer1, 0);
                    //如果要读取的数据是32位数据时，先要截取出我们需要的4位数据
                  // buffer1 = buffer.Skip(9).Take(4).ToArray();
                    //接着对数据进行高低位数据交换，
                   // bat = buffer1[0];
                   // bat1 = buffer1[1];
                   // buffer1[0] = bat1;
                    //buffer1[1] = bat;
                    //bat = buffer1[2];
                    //bat1 = buffer1[3];
                   // buffer1[2] = bat1;
                    //buffer1[3] = bat;
                    //这样就可以获得32位数据了
                    ushort  sh1 = BitConverter.ToUInt16(buffer1,2);
                    float d = GetFloat(sh, sh1);
                    m = d.ToString();
                    // MessageBox.Show(m);
                    
                }
            }
            catch
            {

            }

        }
         public static float GetFloat(ushort P1, ushort P2)
         {
             //如果浮点数时0时，直接返回0.0，不做转换
             if (P1 == 0 && P2 == 0) return 0.0F;
             int intSign, intSignRest, intExponent, intExponentRest;
             float faResult, faDigit;
             intSign = P1 / 32768;
             intSignRest = P1 % 32768;
             intExponent = intSignRest / 128;
             intExponentRest = intSignRest % 128;
             faDigit = (float)(intExponentRest * 65536 + P2) / 8388608;
             faResult = (float)Math.Pow(-1, intSign) * (float)Math.Pow(2, intExponent - 127) * (faDigit + 1);
             return faResult;
         }        
        /*public float FloatingPointNumberObtained(byte[] byt)
        {
            try
            {
                byte[] buffer1 = byt.Skip(0).Take(2).ToArray();
                HighLowBytEswitching(buffer1);
                ushort sh = BitConverter.ToUInt16(buffer1, 0);
                byte[] buffer2 = byt.Skip(2).Take(2).ToArray();
                HighLowBytEswitching(buffer2);
                ushort sh1 = BitConverter.ToUInt16(buffer2, 0);
                float d = GetFloat(sh, sh1);  
                return d;               
            }
            catch
            {

                return 0.0F;
            }

        }*/

        /// <summary>
        /// 高低字节交换
        /// </summary>
        /// <param name="byt"></param>
        public byte[] HighLowBytEswitching(byte[] byt)
        {
            try
            {
                byte bat = byt[0];
                byte bat1 = byt[1];
                byt[0] = bat1;
                byt[1] = bat;
                return byt;
            }
            catch { return byt; }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                //先创建一个字节数组，大小是12个字节，都是16进制的
                byte[] buffer = new byte[12];
                //事务处理序号
                buffer[0] = 0x00;
                buffer[1] = 0x00;
                //协议标识符
                buffer[2] = 0x00;
                buffer[3] = 0x00;
                //随后字节数，06代表后面有6个字节
                buffer[4] = 0x00;
                buffer[5] = 0x06;
                //单元标识符，这个就是我们之前在触屏上设置的设备地址了
                buffer[6] = 0x01;//MBAP报文头
                //功能码，03是用于读取4区的输出寄存器的（这个我是看触屏上的功能码说明）
                buffer[7] = 0x03;
                //寄存器起始地址，（触屏上的设备通道为4XXX0001）
                buffer[8] = 0x00;
                buffer[9] = 0x00;
                //请求字节长度02代表了要读4个字节长度（触屏上代表了一个32位数）
                buffer[10] = 0x00;
                buffer[11] = 0x02;
                //通过网口把请求命令发出去
                socketSend.Send(buffer);

            }
            catch
            {

            }
            textBox3.Text = m;
        }
    }
}
