using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileSystem
{
    /// <summary>
    /// 内存组类
    /// </summary>
    class Memory
    {
        /// <summary>
        /// 内存块数组
        /// </summary>
        public Block[] blocks;

        /// <summary>
        /// 内存块索引器
        /// </summary>
        /// <param name="i">内存块号</param>
        /// <returns>对应内存块</returns>
        public Block this[int i]
        {
            get { return blocks[i]; }
            set { blocks[i] = value; }
        }

        /// <summary>
        /// 新建各个内存块并进行初始化
        /// </summary>
        public Memory()
        {
            blocks = new Block[1024];
            for(int i=0;i<1024;i++)
            {
                blocks[i] = new Block();
            }
        }

        /// <summary>
        /// 内存数据持久化到本地
        /// </summary>
        /// <param name="stream">文件输出流</param>
        public void save(ref FileStream stream)
        {
            BinaryWriter b = new BinaryWriter(stream);
            for(int i=0;i<1024;i++)
            {
                b.Write(blocks[i].content);
            }
        }

        /// <summary>
        /// 磁盘格式化
        /// </summary>
        public void clear()
        {
            for (int i = 0; i < 1024; i++) blocks[i].initialize();
        }
    }

    /// <summary>
    /// 数据块类
    /// </summary>
    class Block
    {
        /// <summary>
        /// 1KByte字节数组（本体）
        /// </summary>
        public byte[] content;

        /// <summary>
        /// 字节数组索引器
        /// </summary>
        /// <param name="i">字节号</param>
        /// <returns>对应字节</returns>
        public byte this[int i]
        {
            get { return content[i]; }
            set { content[i] = value; }
        }
        public Block()
        {
            initialize();
        }

        /// <summary>
        /// 获取字节数组
        /// </summary>
        /// <returns>字节数组</returns>
        public byte[] getContent() { return content; }

        /// <summary>
        /// 初始化/格式化数据块
        /// </summary>
        public void initialize()
        {
            content = new byte[1024];
            for (int i = 0; i < 1024; i++)
            {
                content[i] = Convert.ToByte('\0');
            }
        }
    }
}
