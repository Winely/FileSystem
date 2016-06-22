using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileSystem
{
    enum Error { NotExist, CommandNotFound, HasExist, FileNotFound, NotFolder, NotFile, PermissionDenied, NoSpace, RmFather };
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

        /// <summary>
        /// 导入文件进行系统初始化
        /// </summary>
        /// <param name="stream">导入文件流</param>
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
            //current = new FCB(0, "/", 1, 0, true, user);
            //current.update(ref memory.blocks[0]);
            //fat[0].IsFCB = true;
            //fat[1].use();
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
                showError(Error.HasExist, name);
                return;
            }

            int fcbBlock = -1, contentBlock = -1;
            if ((fcbBlock=fat.getFreeBlock(0))==-1) return;

            fat[fcbBlock].IsFCB = true;

            FCB fcb = new FCB(name, contentBlock, currentFolder, true, user);
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
                showError(Error.HasExist, name);
                return;
            }    

            int fcbBlock = -1, contentBlock = -1;
            if (!getFreeBlock(ref fcbBlock, ref contentBlock)) return;

            fat[fcbBlock].IsFCB = true;
            fat[contentBlock].use();

            FCB fcb = new FCB(name, contentBlock, currentFolder, false, user);
            fcb.lastUpdate = DateTime.Now;
            fcb.update(ref memory.blocks[fcbBlock]);

            // 当前文件夹fcb完成更新后写入content
            current.addSub(fcbBlock);
            current.update(ref memory.blocks[currentFolder]);

            //更新上级fcb
            for (int i = current.SubFile[0]; i != 0; )
            {
                FCB father = FCB.read(memory[i].getContent());
                father.lastUpdate = DateTime.Now;
                father.update(ref memory.blocks[i]);
                i = father.SubFile[0];
            }
        }

        /// <summary>
        /// 删除当前路径下的文件/文件夹
        /// </summary>
        /// <param name="name"></param>
        public void rm(string name)
        {
            if (!folderPermission()) return;
            if(name=="/")
            {
                showError(Error.RmFather, "/");
            }
            if (isFather(name)) return;
            if(!NameExist(name))
            {
                showError(Error.NotExist, name);
                return;
            }
            FCB fcb=null;
            int fcbBlock;
            for (fcbBlock = 1; fcbBlock < current.SubFile.Count; fcbBlock++)
            {
                fcb = FCB.read(memory[current.SubFile[fcbBlock]].getContent());
                if (charToString(fcb.name) == name) break;
            }
            del(current.SubFile[fcbBlock]);
            current.SubFile.Remove(current.SubFile[fcbBlock]);
            current.size -= fcb.size;
            current.lastUpdate = DateTime.Now;
            current.update(ref memory.blocks[currentFolder]);
            //更新fcb
            for (int i = current.SubFile[0]; i != 0; )
            {
                FCB father = FCB.read(memory[i].getContent());
                father.size -= fcb.size;
                father.lastUpdate = DateTime.Now;
                father.update(ref memory.blocks[i]);
                i = father.SubFile[0];
            }
        }


        /// <summary>
        /// 输出目标文件夹下的目录
        /// </summary>
        /// <param name="name">文件夹名</param>
        public void ls(string name)
        {
            if(name=="")
            {
                Console.WriteLine("name\t\ttype\towner\tsize\tcreateTime\t\tlastUpdate");
                for (int i = 1; i < current.SubFile.Count; i++)
                {
                    FCB a = FCB.read(memory[current.SubFile[i]].getContent());
                    Console.WriteLine(
                        charToString(a.name) + "\t" + 
                        (a.isFolder ? "<dir>" : "<file>") + "\t" + 
                        charToString(a.owner) + "\t" +
                        a.size + "\t" +
                        a.createTime + "\t" + 
                        a.lastUpdate
                        );
                }
                return;
            }
            if (!NameExist(name))
            {
                showError(Error.NotExist, name);
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
                showError(Error.NotFolder, name);
                return;
            }
            Console.WriteLine("name\t\ttype\towner\tsize\tcreateTime\t\tlastUpdate");
            for(int i=1;i<fcb.SubFile.Count;i++)
            {
                FCB a = FCB.read(memory[fcb.SubFile[i]].getContent());
                Console.WriteLine(
                        charToString(a.name) + "\t" +
                        (a.isFolder ? "<dir>" : "<file>") + "\t" +
                        charToString(a.owner) + "\t" +
                        a.size + "\t" +
                        a.createTime + "\t" +
                        a.lastUpdate
                        );
            }
        }

        /// <summary>
        /// 输出文件内容到屏幕
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns>返回文件的fcb块号</returns>
        public int read(string name)
        {
            if (!NameExist(name))
            {
                showError(Error.FileNotFound, name);
                return -1;
            }
            FCB fcb = null;
            int fcbBlock;
            for (fcbBlock = 1; fcbBlock < current.SubFile.Count; fcbBlock++)
            {
                fcb = FCB.read(memory[current.SubFile[fcbBlock]].getContent());
                if (charToString(fcb.name) == name) break;
            }
            fcbBlock = current.SubFile[fcbBlock];
            if(fcb.isFolder)
            {
                showError(Error.NotFile, name);
                return -1;
            }
            string str = System.Text.Encoding.UTF8.GetString(memory[fcb.contentBlock].getContent(), 0, fat[fcb.contentBlock].length_used);
            //string str = System.Convert.ToBase64String(memory[fcb.contentBlock].getContent(), 0, fat[fcb.contentBlock].length_used);
            FATRecord fileFAT = fat[fcb.contentBlock];
            while(fileFAT.next!=-1)
            {
                str += System.Text.Encoding.UTF8.GetString(memory[fileFAT.next].getContent(), 0, fat[fileFAT.next].length_used);
                //str += System.Convert.ToBase64String(memory[fileFAT.next].getContent(), 0, fat[fileFAT.next].length_used);
                fileFAT = fat[fileFAT.next];
            }
            Console.WriteLine(str);
            return fcbBlock;
        }

        /// <summary>
        /// 编辑文本文档
        /// </summary>
        /// <param name="fcbBlock"></param>
        /// <param name="input"></param>
        public void write(int fcbBlock, string input)
        {
            FCB fcb = FCB.read(memory[fcbBlock].getContent());
            if (fcb == null) return;
            if(fat[fcb.contentBlock].next!=-1)
            {
                delBlock(fat[fcb.contentBlock].next);
                fat[fcb.contentBlock].next = -1;
            }
            byte[] content = System.Text.Encoding.UTF8.GetBytes(input);
            //byte[] content = System.Convert.FromBase64String(input);
            int writer = 0, contentblock=fcb.contentBlock, block_num = content.Length / 1024;
            if (block_num * 1024 == content.Length) block_num--;
            if(block_num>0)
            {
                //申请内存空间
                int[] blocks = new int[block_num];
                int offset = 0;//freeblock的搜索起点
                for (int i = 0; i < block_num; i++)
                {
                    if (-1 == (blocks[i] = fat.getFreeBlock(offset)))
                    {
                        showError(Error.NoSpace, null);
                        return;
                    }
                    offset = blocks[i] + 1;
                }
                //写入内存
                for (int i = 0; i < block_num;i++ ) //整块内存写入
                {
                    Array.Copy(content, writer, memory.blocks[contentblock].content, 0, 1024);
                    fat[contentblock].next = blocks[i];
                    fat[contentblock].length_used = 1024;
                    contentblock = blocks[i];
                    writer += 1024;
                }
            }
            //装不满的部分
            Array.Copy(content, writer, memory.blocks[contentblock].content, 0, content.Length-1024*block_num);
            fat[contentblock].length_used = content.Length - writer;
            //更新fcb
            fcb.size = content.Length;
            fcb.lastUpdate = DateTime.Now;
            fcb.update(ref memory.blocks[fcbBlock]);

            for(int i = fcb.SubFile[0];i!=0;)
            {
                FCB father = FCB.read(memory[i].getContent());
                father.size += content.Length;
                father.lastUpdate = DateTime.Now;
                father.update(ref memory.blocks[i]);
                i = father.SubFile[0];
            }
        }

        /// <summary>
        /// 格式化
        /// </summary>
        public void clear()
        {
            memory.clear();
            fat.clear();
            currentFolder = 0;
            current = new FCB("/", 1, 0, true, user);
            current.update(ref memory.blocks[0]);
            fat[0].IsFCB = true;
            fat[1].use();
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
            else if(fcb.isFolder)
            {
                delBlock(block);
                current.SubFile.Remove(block);
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
            if (path == "."||path=="") return true;
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
                        showError(Error.NotFolder, path);
                        return false;
                    }
                    jumpTo(current.SubFile[i]);
                    return true;
                }
            }
            showError(Error.NotExist, path);
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
            if ((fcbBlock = fat.getFreeBlock(0))==-1)
            {
                showError(Error.NoSpace, null);
                return false;
            }
            if ((contentBlock = fat.getFreeBlock(fcbBlock+1)) == -1)
            {
                showError(Error.NoSpace, null);
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

        /// <summary>
        /// 将char数组转换为string，去掉后方多余的\0
        /// </summary>
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
        /// public创立的文件夹可以被任意用户修改
        /// root可以修改任意用户的文件
        /// </summary>
        /// <returns>无权限则返回false</returns>
        public bool folderPermission()
        {
            if (user=="root"|| charToString(current.owner) == user || charToString(current.owner)=="public") return true;
            showError(Error.PermissionDenied, charToString(current.owner));
            return false;
        }

        /// <summary>
        /// 检测是否父文件夹
        /// </summary>
        bool isFather(string name)
        {
            int father = currentFolder;
            FCB fcb = current;
            while(father !=0)
            {
                if (charToString(fcb.name) == name)
                {
                    showError(Error.RmFather, name);
                    return true;
                }
                father = fcb.SubFile[0];
                fcb = FCB.read(memory[father].getContent());
            }
            return false;
        }
        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="error">报错类型</param>
        /// <param name="name">报错对象</param>
        public void showError(Error error, string name)
        {
            string msg = "Error: ";
            switch(error)
            {
                case Error.NotExist: msg += ("Folder or file \"" + name + "\" not exist."); break;
                case Error.HasExist: msg += ("Folder or file \"" + name + "\" has exist."); break;
                case Error.CommandNotFound: msg += "Command not found."; break;
                case Error.FileNotFound: msg += ("File \"" + name + "\" not found."); break;
                case Error.PermissionDenied: msg += ("Permission denied. Please login as the owner of \"" + name + "\" to establish operation."); break;
                case Error.NotFolder: msg += ("\"" + name + "\" is not a folder."); break;
                case Error.NotFile: msg += ("\"" + name + "\" is not a file."); break;
                case Error.NoSpace: msg += ("insufficient memory."); break;
                case Error.RmFather: msg += ("Cannot remove father folder \"" + name + "\", please jump to higher folder."); break;
            }
            Console.WriteLine(msg);
        }

        /// <summary>
        /// 保存至目标位置
        /// </summary>
        /// <param name="stream">目标文件流</param>
        public void save(ref FileStream stream)
        {
            stream.Position = 0;
            BinaryFormatter b = new BinaryFormatter();
            fat.save(ref stream);
            memory.save(ref stream);
        }
    }

}
