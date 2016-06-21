using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileSystem
{
    class FileSystem
    {
        public FAT fat = new FAT();
        Memory memory = new Memory();
        public string user { get; set; }
        /// <summary>
        /// 当前所在文件夹的fcb所在块号
        /// </summary>
        public int currentFolder { get; set; }

        /// <summary>
        /// 当前所在文件夹的fcb
        /// </summary>
        FCB current;
        public string getCurrentFolderName()
        {
            return charToString(current.name);
        }
        public FileSystem()
        {
            fat = new FAT();
            memory = new Memory();
        }

        public void initialize(ref FileStream stream)
        {
            BinaryReader b = new BinaryReader(stream);
            for (int i = 0; i < 1024; i++)
            {
                fat[i].length_used = b.ReadInt32();
                fat[i].next = b.ReadInt32();
                fat[i].IsFCB = b.ReadBoolean();
            }
            for (int i = 0; i < 1024; i++)
            {
                memory[i].content = b.ReadBytes(1024);
            }
            current = FCB.read(memory[0].getContent());
            currentFolder = 0;
        }


        /// <summary>
        /// 在当前目录添加文件夹
        /// </summary>
        /// <param name="name">文件夹名</param>
        public void mkdir(string name)
        {
            if (!folderPermission()) return;
            if (NameExist(name))
            {
                Console.WriteLine("Error: Name has existed.");
                return;
            }

            int fcbBlock = -1, contentBlock = -1;
            if (!getFreeBlock(ref fcbBlock, ref contentBlock)) return;

            fat[fcbBlock].IsFCB = true;
            fat[contentBlock].use();

            FCB fcb = new FCB(fcbBlock, name, contentBlock, currentFolder, true, user);
            fcb.update(ref memory.blocks[fcbBlock]);

            // 当前文件夹fcb完成更新后写入content
            current.addSub(fcbBlock);
            current.update(ref memory.blocks[currentFolder]);
        }

        /// <summary>
        /// 在当前目录下新建文件
        /// </summary>
        /// <param name="name">文件名</param>
        public void mkfile(string name)
        {
            if (!folderPermission()) return;
            if (NameExist(name))
            {
                Console.WriteLine("Error: Name has existed.");
                return;
            }    

            int fcbBlock = -1, contentBlock = -1;
            if (!getFreeBlock(ref fcbBlock, ref contentBlock)) return;

            fat[fcbBlock].IsFCB = true;
            fat[contentBlock].use();

            FCB fcb = new FCB(fcbBlock, name, contentBlock, currentFolder, false, user);
            fcb.update(ref memory.blocks[fcbBlock]);

            // 当前文件夹fcb完成更新后写入content
            current.addSub(fcbBlock);
            current.update(ref memory.blocks[currentFolder]);
        }

        /// <summary>
        /// 删除当前路径下的文件/文件夹
        /// </summary>
        /// <param name="name"></param>
        public void rm(string name)
        {
            if (!folderPermission()) return;
            if(!NameExist(name))
            {
                Console.WriteLine("Error: File or folder " + name + " not exist.");
                return;
            }
            FCB fcb=null;
            int fcbBlock;
            for (fcbBlock = 1; fcbBlock < current.SubFile.Count; fcbBlock++)
            {
                fcb = FCB.read(memory[current.SubFile[fcbBlock]].getContent());
                if (fcb.name.ToString() == name) break;
            }
            del(fcbBlock);
            current.update(ref memory.blocks[currentFolder]);
        }

        /// <summary>
        /// 输出目标文件夹下的目录
        /// </summary>
        /// <param name="name">文件夹名</param>
        public void ls(string name)
        {
            if(name=="")
            {
                Console.WriteLine("name\t\ttype\t\towner\t\tcreateTime\t\tlastUpdate");
                for (int i = 1; i < current.SubFile.Count; i++)
                {
                    FCB a = FCB.read(memory[current.SubFile[i]].getContent());
                    Console.WriteLine(charToString(a.name) + "\t" + (a.isFolder ? "<dir>" : "<file>") + "\t" + charToString(a.owner) + "\t" + a.createTime + "\t" + a.lastUpdate);
                }
                return;
            }
            if (!NameExist(name))
            {
                Console.WriteLine("Error: File or folder " + name + " not exist.");
                return;
            }
            FCB fcb = null;
            int fcbBlock;
            for (fcbBlock = 1; fcbBlock < current.SubFile.Count; fcbBlock++)
            {
                fcb = FCB.read(memory[current.SubFile[fcbBlock]].getContent());
                if (charToString(fcb.name) == name) break;
            }
            if(!fcb.isFolder)
            {
                Console.WriteLine("Error: " + name + " is not a folder.");
                return;
            }
            Console.WriteLine("name\t\ttype\t\towner\t\tcreateTime\t\tlastUpdate");
            for(int i=1;i<fcb.SubFile.Count;i++)
            {
                FCB a = FCB.read(memory[fcb.SubFile[i]].getContent());
                Console.WriteLine(charToString(a.name) + "\t" + (a.isFolder ? "<dir>" : "<file>") + "\t" + charToString(a.owner) + "\t" + a.createTime + "\t" + a.lastUpdate);
            }
        }

        /// <summary>
        /// 删除某个块及其连续块
        /// </summary>
        /// <param name="block">目标块号</param>
        void delBlock(int block)
        {
            if(fat[block].next!=-1)
            {
                delBlock(fat[block].next);
            }
            memory[block].initialize();
            fat.clear(block);
        }

        /// <summary>
        /// 递归删除某个FCB的文件/文件夹
        /// </summary>
        /// <param name="block">fcb所在块</param>
        void del(int block)
        {
            FCB fcb = FCB.read(memory[block].getContent());
            if(fcb.isFolder && fcb.SubFile.Count>1)
            {
                for(int i=1;i<fcb.SubFile.Count;i++)
                {
                    del(fcb.SubFile[i]);
                }
            }
            else
            {
                delBlock(fcb.contentBlock);
                delBlock(block);
                current.SubFile.Remove(block);
            }
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
                jumpTo(current.SubFile[0]);
                return true;
            }
            if (path == ".") return true;
            if (path=="/")      //根目录
            {
                jumpTo(0);
                return true;
            }
            //子目录
            FCB fcb = null;
            for(int i=1; i<current.SubFile.Count();i++)
            {
                fcb = FCB.read(memory[current.SubFile[i]].getContent());
                if (charToString(fcb.name) != path) continue;
                else
                {
                    if(!fcb.isFolder)
                    {
                        Console.WriteLine("Error: The path "+path+" is not a folder.");
                        return false;
                    }
                    jumpTo(current.SubFile[i]);
                    return true;
                }
            }
            Console.WriteLine("Error: Folder "+path+" not exist.");
            return false;
        }
        /// <summary>
        /// 根据块号跳转
        /// </summary>
        /// <param name="block">目标块号</param>
        public void jumpTo(int block)
        {
            currentFolder = block;
            current = FCB.read(memory[block].getContent());
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
                if (charToString(fcb.name) == name) return true;
            }
            return false;
        }

        string charToString(char[] array)
        {
            StringBuilder s = new StringBuilder();
            foreach (char c in array)
            {
                if (c == '\0') break;
                s.Append(c);
            }
            return s.ToString();
        }
        /// <summary>
        /// 检查对当前文件夹是否有写权限。
        /// root创立的文件夹可以被任意用户修改
        /// root可以修改任意用户的文件
        /// </summary>
        /// <returns>无权限则返回false</returns>
        public bool folderPermission()
        {
            if (user=="root"|| charToString(current.owner) == user || charToString(current.owner)=="root") return true;
            Console.WriteLine("Error: Permission denied.");
            return false;
        }

        public void save(ref FileStream stream)
        {
            stream.Position = 0;
            BinaryFormatter b = new BinaryFormatter();
            fat.save(ref stream);
            memory.save(ref stream);
        }
    }

}
