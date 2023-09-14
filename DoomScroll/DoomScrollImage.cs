using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doom_Scroll
{
	public class DoomScrollImage
    {
        private byte sender;
        private int imgNumber;
        private byte[] image;
        private byte[][] imageArray;

		// public gets
        public byte Sender { get; }
		public byte ImgNumber { get; }
		public byte[] Image { get; }
		public DoomScrollImage(int numMessages, byte send, int num)
		{
			sender = send;
			imgNumber = num;
			imageArray = new byte[numMessages][];
			DoomScroll._log.LogInfo($"imageList Count is {imageArray.Length}");

        }
		public int GetNumByteArraysExpected()
		{
			return imageArray.Length;
		}
        public byte[][] GetImageArray()
        {
            byte[][] newArray = new byte[imageArray.Length][];
			System.Array.Copy(imageArray, newArray, imageArray.Length);
			return newArray;
        }

        public bool SameImage(byte send, byte num)
		{
			return sender == send && imgNumber == (int)num;
		}
		public void InsertByteChunk(int sectionIndex, byte[] byteChunk)
		{
			if (sectionIndex >= 0 && sectionIndex < imageArray.GetLength(0))
			{
                DoomScroll._log.LogInfo($"Value at section index #{sectionIndex} is {imageArray[sectionIndex]}");
                imageArray[sectionIndex] = byteChunk;
				DoomScroll._log.LogInfo($"Value at section index #{sectionIndex} is now {byteChunk}");
			}
			else
			{
				DoomScroll._log.LogInfo($"sectionIndex {sectionIndex} either < 0 or >= {imageArray.GetLength(0)}. Default Length measure: {imageArray.Length}");
			}
        }
		public bool CompileImage()
		{
			bool flag = System.Array.IndexOf(imageArray, null) == -1;
			bool result;
			if (flag)
			{
				List<byte> list = new List<byte>();
				foreach (byte[] collection in imageArray)
				{
					list.AddRange(collection);
				}
				image = list.ToArray();
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}
		public List<int> GetMissingLines()
		{
			List<int> list = new List<int>();
			IEnumerable<int> enumerable = Enumerable.Range(0, imageArray.Length);
			foreach (int num in enumerable)
			{
				bool flag = imageArray[num] == null;
				if (flag)
				{
					list.Add(num);
				}
			}
			return list;
		}
		public override String ToString()
		{
			return $"player id is {sender}, image id is {imgNumber}";
		}
	}
}
