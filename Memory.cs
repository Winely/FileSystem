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
        Block[] blocks;
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

    }

    class Block
    {
        byte[] content;
        public byte this[int i]
        {
            get { return content[i]; }
            set { content[i] = value; }
        }
        public Block()
        {
            content = new byte[1024];
            for(int i=0;i<1024;i++)
            {
                content[i] = Convert.ToByte('\0');
            }
        }

        public byte[] getContent() { return content; }
    }
}
