using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileSystem
{
    class Shell
    {
        FileSystem filesys;
        FileStream stream;

        public Shell()
        {
            filesys = new FileSystem();
            stream = new FileStream("filesystem", FileMode.OpenOrCreate);
        }

        /// <summary>
        /// 输出指示
        /// </summary>
        void instruction()
        {
            Console.WriteLine("==========================================================================");
            Console.WriteLine("—                          欢迎使用文件管理系统                            ");
            Console.WriteLine("—                             1452764 何冬怡                              ");
            Console.WriteLine("==========================================================================");
            Console.WriteLine("—    本系统使用类Linux命令，支持下列命令                                    ");
            Console.WriteLine("—    ls              列出当前目录下所有文件/文件夹                          ");
            Console.WriteLine("—    cd <path>       跳转到目标路径，支持..回到上一级,绝对路径及相对路径     ");
            Console.WriteLine("—    mkdir <name>    在当前目录下新建文件夹                                ");
            Console.WriteLine("—    mkfile <name>   在当前目录下新建文档                                  ");
            Console.WriteLine("—    rm <name>       删除当前目录下的文件/文件夹                           ");
            Console.WriteLine("—    edit <name>     编辑文件内容                                         ");
            Console.WriteLine("—    open <name>     打开文件内容                                         ");
            Console.WriteLine("—    rmall           系统格式化                                           ");
            Console.WriteLine("—    help            获取帮助                                             ");
            Console.WriteLine("—    quit            退出系统                                             ");
            Console.WriteLine("==========================================================================");
        }

        /// <summary>
        /// 输出shell开头
        /// </summary>
        void writeShell()
        {
            Console.Write("[" + filesys.user + "@" + filesys.getCurrentFolderName() + "]# ");
        }

        /// <summary>
        /// 获取用户名
        /// </summary>
        void getUser()
        {
            Console.Write("Please input your user name: ");
            filesys.user = Console.ReadLine();
        }

        /// <summary>
        /// 跳转指令
        /// </summary>
        /// <param name="para">参数</param>
        void cd(string para)
        {
            //存储原路径
            int oldpath = filesys.currentFolder;
            string path = parsePath(para);

            if (path != null && filesys.cd(path)) return;
            else filesys.jumpTo(oldpath);       //跳转失败则回到原路径

        }

        /// <summary>
        /// 新建文件夹
        /// </summary>
        /// <param name="para">参数</param>
        void mkdir(string para)
        {
            int oldpath = filesys.currentFolder;
            string name = parsePath(para);
            if (name != null) filesys.mkdir(name);
            filesys.jumpTo(oldpath);
        }

        /// <summary>
        /// 新建文本文件
        /// </summary>
        /// <param name="para">参数</param>
        void mkfile(string para)
        {
            int oldpath = filesys.currentFolder;
            string name = parsePath(para);
            if (name != null) filesys.mkfile(name);
            filesys.jumpTo(oldpath);
        }

        /// <summary>
        /// 删除指令
        /// </summary>
        /// <param name="para">参数</param>
        void rm(string para)
        {
            int oldpath = filesys.currentFolder;
            string name = parsePath(para);
            if (name != null) filesys.rm(name);
            filesys.jumpTo(oldpath);
        }

        /// <summary>
        /// 打开文本文件
        /// </summary>
        /// <param name="para">参数</param>
        void open(string para)
        {
            int oldpath = filesys.currentFolder;
            string name = parsePath(para);
            if (name != null) filesys.read(name);
            filesys.jumpTo(oldpath);
        }

        /// <summary>
        /// 编辑指令
        /// </summary>
        /// <param name="para">参数</param>
        void edit(string para)
        {
            int oldpath = filesys.currentFolder;
            string name = parsePath(para);
            if (name != null)
            {
                int fcb = filesys.read(name);
                if (fcb == -1) return;
                filesys.write(fcb,getInput());
            }
            filesys.jumpTo(oldpath);
        }
        string getInput()
        {
            Console.WriteLine("Please input the content of file. End up input with double click Enter.");
            string str="", substr;
            while((substr=Console.ReadLine())!=null)
            {
                if (substr == "") break;
                str += substr;
            }
            return str;
        }
        /// <summary>
        /// 列出文件目录
        /// </summary>
        /// <param name="para">参数</param>
        void ls(string para)
        {
            if (para == "")
            {
                filesys.ls("");
                return;
            }
            int oldpath = filesys.currentFolder;
            string name = parsePath(para);
            if (name != null) filesys.ls(name);
            filesys.jumpTo(oldpath);
        }

        /// <summary>
        /// 格式化内存
        /// </summary>
        void clear()
        {
            filesys.clear();
        }

        /// <summary>
        /// 退出文件系统
        /// </summary>
        void quit()
        {
            filesys.save(ref stream);
            Console.WriteLine("Quit successfully.");
        }

        /// <summary>
        /// 解析参数中的路径，跳转到对应位置，并返回最后一个参数
        /// </summary>
        /// <param name="para">参数/后面的东西</param>
        /// <returns>路径最后的部分，可能为文件名/文件夹名</returns>
        string parsePath(string para)
        {
            if (para == "") return null;
            //去除多余空格
            int i;
            for (i = 0; i < para.Length; i++)
            {
                if (para[i] == ' ' || para[i] == '\t') continue;
                else break;
            }
            para = para.Substring(i);
            if (para == "") return null;

            //判断是否绝对路径
            if (para[0] == '/')
            {
                filesys.cd("/");
                para = para.Substring(1);
            }
            //进行跳转
            int read = 0, current = 0;
            for (; current < para.Length; current++)
            {
                string path;
                if (para[current] == '/')
                {
                    path = para.Substring(read, current - read);
                    read = current + 1;
                    if (!filesys.cd(path)) return null;
                }
            }
            return para.Substring(read);
        }

        /// <summary>
        /// 启动shell
        /// </summary>
        public void run()
        {
            instruction();
            getUser();
            filesys.initialize(ref stream);
            while (true)
            {
                string command;
                string token = null;
                writeShell();
                command = Console.ReadLine();
                int current; //标记读取指针
                for (current = 0; current < command.Length; current++)
                {
                    if (command[current] == ' ' || command[current] == '\t' || command[current] == '\0')
                    {
                        token = command.Substring(0, current);
                        break;
                    }
                }
                if (token == null)
                {
                    token = command;
                    command = "";
                }
                else command = command.Substring(current);
                switch (token)
                {
                    case "cd": cd(command); break;
                    case "mkdir": mkdir(command); break;
                    case "mkfile": mkfile(command); break;
                    case "rm": rm(command); break;
                    case "edit": edit(command); break;
                    case "ls": ls(command); break;
                    case "open": open(command); break;
                    case "rmall": clear(); break;
                    case "help": instruction(); break;
                    case "quit": quit(); return;
                    default:
                        filesys.showError(Error.CommandNotFound, null);
                        break;
                }
            }
        }
    }
}
