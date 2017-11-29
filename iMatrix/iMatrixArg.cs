using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iMatrixSpc
{
	public abstract class iMatrixArg
	{
		protected static int BitsInByte = 8;
		public abstract int[] ToBitArray();
		public abstract int GetSizeInBits();
	}
	public abstract class iMatrixRes
	{
		public iMatrixRes(int[] arr)
		{
			FromBitArray(arr);
		}
		protected static int BitsInByte = 8;
		public abstract int GetSizeInBits();
		public abstract void FromBitArray(int[] arr);
	}

	public class ByteArg : iMatrixArg
	{
		private byte value;

		public ByteArg(byte arg)
		{
			value = arg;
		}
		public override int[] ToBitArray()
		{
			var sizeInBits = 8;//GetSizeInBits()
			var sizeInBitsM1 = 8 - 1;
			var res = new int[sizeInBits];
			for (var i = 0; i < GetSizeInBits(); i++)
				res[sizeInBitsM1 - i] = value.GetBitByIndex(i);

			return res;
		}
		public override int GetSizeInBits() => 8;
		//{
		//    return sizeof(byte) * BitsInByte;
		//}
	}
	public class ByteRes : iMatrixRes
	{
		private byte _value = 0;
		public byte Value => _value;
		public ByteRes(int[] arr) : base(arr)
		{
		}
		public override void FromBitArray(int[] arr)
		{
			var reveresed = arr.Reverse().ToArray();
			if (arr.Length != GetSizeInBits())
				throw new Exception("Размер массива данных результата не соответствует числу битов выходного типа данных.");

			for (int i = 0; i < GetSizeInBits(); i++)
			{
				_value |= (byte) (reveresed[i] << i);
			}
		}
		public override int GetSizeInBits()
		{
			return sizeof(byte) * BitsInByte;
		}
	}

	public static class ExtensionMethods
	{
		public static int GetBitByIndex(this byte value, int bitIndex)
		{
			byte[] twoPow = { 1, 2, 4, 8, 16, 32, 64, 128 };
			return (value & twoPow[bitIndex]) >> bitIndex;
			//return ((((byte)value << (7 - bitIndex)) >> 7));
		}
	}
}
