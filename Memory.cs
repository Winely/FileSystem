using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileSystem
{
    class Memory
    {
        public Block[] blocks;
        public Block this[int i]
        {
            get { return blocks[i]; }
            set { blocks[i] = value; }
        }
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

    class Block
    {
        public byte[] content;
        public byte this[int i]
        {
            get { return content[i]; }
            set { content[i] = value; }
        }
        public Block()
        {
            initialize();
        }

        public byte[] getContent() { return content; }

        /// <summary>
        /// 初始化/格式化磁盘
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
