using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iMatrixSpc;

namespace mks
{
	public class MatrixDraw
	{
		public static ConsoleColor[] wallColors = { ConsoleColor.Gray, ConsoleColor.Yellow, ConsoleColor.Green };
		public static void DrawMatrix(iMatrix matr)
		{
			for (var i = matr.yHeight - 1; i >= 0; i--)
			{
				DrawMatrixRowWithVerticalWalls(matr, i);
				DrawMatrixHorizontalWalls(matr, i);
				Console.ResetColor();
			}
		}
		private static void DrawMatrixHorizontalWalls(iMatrix matr, int rowNumber)
		{
			if (rowNumber >= matr.Rows.Size)
				return;
			else
			{
				var horWalls = matr.Rows[rowNumber].rowItems.Select(t => t.h).ToArray();
				for (var i = 0; i < horWalls.Length; i++)
				{
					Console.ForegroundColor = wallColors[(int) horWalls[i]];
					WriteHorizontalWallByIndex((int) horWalls[i]);
				}
				Console.WriteLine();
			}
		}
		private static void WriteHorizontalWallByIndex(int index)
		{
			Console.Write(" ");
			switch (index)
			{
				case 0:
					Console.Write(" -");
					break;
				case 1:
					Console.Write(" " + (char) 25); //Console.Write(" " + (char)31); //стрелка вниз
					break;
				case 2:
					Console.Write(" " + (char) 24); //Console.Write(" " + (char)30); //стрелка вверх
					break;
			}
		}
		private static void DrawMatrixRowWithVerticalWalls(iMatrix matr, int rowNumber)
		{
			//var vertWalls = matr.Rows[rowNumber].rowItems.Select(t => t.v).ToArray();
			var Cells = matr.Rows[rowNumber].rowItems.ToArray();
			for (var i = 0; i < Cells.Length; i++)
			{
				Console.ForegroundColor = wallColors[(int) Cells[i].v];
				WriteVertWallByIndex((int) Cells[i].v);
				//Console.Write("|");//vertWalls[i]);
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write(Cells[i].Memo);
				Console.ResetColor();
				Console.Write(Cells[i].Output);
			}
			Console.WriteLine();
		}
		private static void WriteVertWallByIndex(int index)
		{
			//Console.OutputEncoding = System.Text.Encoding.UTF8;

			switch (index)
			{
				case 0:
					Console.Write('|');
					break;
				case 1:
					Console.Write((char) 27); //Console.Write((char)17); стрелка влево
					break;
				case 2:
					Console.Write((char) 26); //Console.Write((char)16);//стрелка вправо 
					break;
			}
		}
	}
}
