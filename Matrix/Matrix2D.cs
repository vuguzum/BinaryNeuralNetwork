using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matrix
{
	public class Row<T>
	{
		public T[] rowItems;// = { };
		public int Size
		{
			get { return rowItems.Length; }
		}
		public Row()
		{
			rowItems = new T[0];
		}
		public Row(int size)
		{
			rowItems = new T[size];
		}
		public Row(T[] values)
		{
			rowItems = new T[values.Length];
			Array.Copy(values, 0, rowItems, 0, values.Length);
		}
		public T this[int i]
		{
			get { return rowItems[i]; }
			set { rowItems[i] = value; }
		}
		public T Last => rowItems.Last();
	}

	public class Matrix2D<T>
	{
		public int xWidth { get { return Rows.Size == 0 ? 0 : Rows[0].Size; } }
		public int yHeight { get { return Rows.Size; } }

		public Row<Row<T>> Rows;
		//public Row<T>[] Rows = { };
		public Matrix2D(int width, int height)
		{
			Rows = CreateNew(width, height);
		}
		private Row<Row<T>> CreateNew(int width, int height)
		{
			var rows = new Row<Row<T>>(height);
			for (int i = 0; i < height; i++)
			{
				rows[i] = new Row<T>(width);
			}
			return rows;
		}
		/// <summary>
		/// Создает матрицу с одной строкой
		/// </summary>
		/// <param name="width"></param>
		public Matrix2D(int width)
		{
			Rows = CreateNew(width, 1);
		}
		public virtual void AppendRow()
		{
			Array.Resize<Row<T>>(ref Rows.rowItems, Rows.Size + 1);
			Rows[yHeight-1] = new Row<T>(xWidth);
		}
		public T this[int x, int y]
		{
			get { return Rows[y][x]; }
			set { Rows[y][x] = value; }
		}
	}


	/// <summary>
	/// Вариант с двумерным массивом (пока не используется)
	/// 2D-матрица [X,Y] X номер столбца, Y - номер строки. 
	/// Т.е. если N ширина, M высота, то
	///				  [1,M][2,M][3,M]...[N,M]
	///							...
	/// вторая строка [1,2][2,2][3,2]...[N,2]
	/// первая строка [1,1][2,1][3,1]...[N,1]
	/// Растет снизу вверх X-по горизонтали, Y-по вертикали
	/// </summary>
	/// <typeparam name="T">тип элемента</typeparam>
	public class Matrix2D__<T>
	{
		private T[,] items = { };
		public int Width
		{
			get { return items.GetLength(0); }
		}
		public int Height
		{
			get { return items.GetLength(1); }
		}
		public int Count
		{
			get { return items.Length; }
		}
		public Matrix2D__(int XWidth, int YHeight)
		{
			items = new T[YHeight, XWidth];
		}
	}
}
