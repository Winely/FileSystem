using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileSystem
{
    class FileSystem
    {
        FAT fat = new FAT();
        Memory memory = new Memory();
        public string user { get; set; }
        /// <summary>
        /// 当前所在文件夹的fcb所在块号
        /// </summary>
        int currentFolder;

        /// <summary>
        /// 当前所在文件夹的fcb
        /// </summary>
        FCB current;
        FCB root;

        public string getCurrentFolderName() { return current.name.ToString(); }
        public FileSystem()
        {
            fat = new FAT();
            memory = new Memory();
            user = "root";
            currentFolder = 0;
            root = new FCB(0, "root", 1, 0, true, user);
            current = root;
        }
        /// <summary>
        /// 在当前目录添加文件夹
        /// </summary>
        /// <param name="name">文件夹名</param>
        public void mkdir(string name)
        {
            if (!folderPermission()) return;
            if (NameExist(name)) return;

            int fcbBlock = -1, contentBlock = -1;
            if (!getFreeBlock(ref fcbBlock, ref contentBlock)) return;

            fat[fcbBlock].isFCB = true;
            fat[contentBlock].use();

            FCB fcb = new FCB(fcbBlock, name, contentBlock, currentFolder, true, user);
            fcb.update(memory[contentBlock]);

            // 当前文件夹fcb完成更新后写入content
            current.addSub(fcbBlock);
            current.update(memory[currentFolder]);
        }

        /// <summary>
        /// 在当前目录下新建文件
        /// </summary>
        /// <param name="name">文件名</param>
        public void mkfile(string name)
        {
            if (!folderPermission()) return; 
            if (NameExist(name)) return;    

            int fcbBlock = -1, contentBlock = -1;
            if (!getFreeBlock(ref fcbBlock, ref contentBlock)) return;

            fat[fcbBlock].isFCB = true;
            fat[contentBlock].use();

            FCB fcb = new FCB(fcbBlock, name, contentBlock, currentFolder, false, user);
            fcb.update(memory[currentFolder]);

            // 当前文件夹fcb完成更新后写入content
            current.addSub(fcbBlock);
            current.update(memory[currentFolder]);
        }

        /// <summary>
        /// 跳转到目标路径
        /// </summary>
        /// <param name="path">已经过分段处理的路径</param>
        /// <returns>跳转成功则返回true</returns>
        public bool cd (string path)
        {
            if(path == "..")    //上层目录
            {
                currentFolder = current.SubFile[0];
                current = FCB.read(memory[currentFolder].getContent());
                return true;
            }
            if (path=="/")      //根目录
            {
                current = root;
                currentFolder = 0;
                return true;
            }
            //子目录
            FCB fcb = null;
            for(int i=1; i<current.SubFile.Count();i++)
            {
                fcb = FCB.read(memory[current.SubFile[i]].getContent());
                if (fcb.name.ToString() != path) continue;
                else
                {
                    if(!fcb.isFolder)
                    {
                        Console.WriteLine("Error: The path is not a folder.");
                        return false;
                    }
                    currentFolder = current.SubFile[i];
                    current = fcb;
                    return true;
                }
            }
            Console.WriteLine("Error: Folder not exist.");
            return false;
        }
        
        /// <summary>
        /// 为fcb和内容申请空余区块
        /// </summary>
        /// <param name="fcbBlock"></param>
        /// <param name="contentBlock"></param>
        /// <returns>申请失败则返回false</returns>
        public bool getFreeBlock(ref int fcbBlock, ref int contentBlock)
        {
            fcbBlock = fat.getFreeBlock();
            contentBlock = fat.getFreeBlock();
            if (fcbBlock == -1 || contentBlock == -1)
            {
                Console.WriteLine("内存空间不足");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查当前目录下名字是否存在
        /// </summary>
        /// <param name="name">待检测名</param>
        /// <returns></returns>
        public bool NameExist (string name)
        {
            FCB fcb;
            for(int i=1;i<current.SubFile.Count;i++)
            {
                fcb = FCB.read(memory[current.SubFile[i]].getContent());
                if (fcb.name.ToString() == name)
                {
                    Console.WriteLine("Name has existed.");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查对当前文件夹是否有写权限。若用户名与文件夹owner不符则无权限。
        /// </summary>
        /// <returns>无权限则返回false</returns>
        public bool folderPermission()
        {
            if (current.owner.ToString() == user) return true;
            Console.WriteLine("Error: Permission denied.");
            return false;
        }
    }

}
