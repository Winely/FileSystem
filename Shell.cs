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
            //stream = new FileStream("filesystem", FileMode.OpenOrCreate);
            //if (stream.Length == 0) filesys = new FileSystem();
            //else
            //{
            //    BinaryFormatter b = new BinaryFormatter();
            //    filesys = b.Deserialize(stream) as FileSystem;
            //}
            filesys = new FileSystem();
        }

        public void instruction()
        {
            Console.WriteLine("==========================================================================");
            Console.WriteLine("—                          欢迎使用文件管理系统                            ");
            Console.WriteLine("—                             1452764 何冬怡                              ");
            Console.WriteLine("——————————————————————————————————————————————————————————————————————————");
            Console.WriteLine("—    本系统使用类Linux命令，支持下列命令                                    ");
            Console.WriteLine("—    ls              列出当前目录下所有文件/文件夹                          ");
            Console.WriteLine("—    cd <path>       跳转到目标路径，支持..回到上一级,绝对路径及相对路径     ");
            Console.WriteLine("—    mkdir <name>    在当前目录下新建文件夹                                ");
            Console.WriteLine("—    mkfile <name>   在当前目录下新建文档                                  ");
            Console.WriteLine("—    rm <name>       删除当前目录下的文件/文件夹                           ");
            Console.WriteLine("—    edit <name>     编辑文件内容                                         ");
            Console.WriteLine("—    help            获取帮助                                             ");
            Console.WriteLine("——————————————————————————————————————————————————————————————————————————");
        }

        void writeShell()
        {
            Console.Write("[" + filesys.user + "@" + filesys.getCurrentFolderName() + "]  # ");
        }
        void getUser()
        {
            Console.Write("Please input your user name: ");
            filesys.user = Console.ReadLine();
        }
        void cd(string para) { }
        void mkdir(string para) { }
        void mkfile(string para) { }
        void rm(string para) { }
        void edit(string para) { }
        void ls(string para) { }

        public void run()
        {
            instruction();
            getUser();
            while(true)
            {
                string command;
                string token;
                while(true)
                {
                    writeShell();
                    command = Console.ReadLine();
                    int current; //标记读取指针
                    for(current=0; current<command.Length; current++)
                    {
                        if (command[current] == ' ' || command[current] == '\t' || command[current] == '\0') break;
                    }
                    token = command.Substring(0, current);
                    command = command.Substring(current+1);
                    switch(token)
                    {
                        case "cd":          cd(command);        break;
                        case "mkdir":       mkdir(command);     break;
                        case "mkfile":      mkfile(command);    break;
                        case "rm":          rm(command);        break;
                        case "edit":        edit(command);      break;
                        case "ls":          ls(command);        break;
                        case "help":        instruction();      break;
                        default:
                            Console.WriteLine("Error: Command not found.");
                            break;
                    }
                }
            }
        }
    }
}
