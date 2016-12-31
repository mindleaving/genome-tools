using System;
using System.Text;

namespace Commons
{
    public class Matrix : IEquatable<Matrix>
    {
        public int Rows { get; }
        public int Columns { get; }
        public double[,] Data { get; }

        public Matrix(int rows, int columns)
        {
            if (rows < 1 || columns < 1)
                throw new ArgumentException($"Matrix must be at least of size [1x1], but requested was [{rows}x{columns}]");

            Rows = rows;
            Columns = columns;
            Data = new double[rows, columns];
        }

        public void SetRow(int rowIdx, double[] values)
        {
            if (rowIdx < 0 || rowIdx >= Rows)
                throw new ArgumentOutOfRangeException(nameof(rowIdx));
            if (values.Length != Columns)
                throw new ArgumentException("Value array is not equal to number of columns");

            for (int columnIdx = 0; columnIdx < Columns; columnIdx++)
            {
                Data[rowIdx, columnIdx] = values[columnIdx];
            }
        }
        public void SetColumn(int columnIdx, double[] values)
        {
            if (columnIdx < 0 || columnIdx >= Columns)
                throw new ArgumentOutOfRangeException(nameof(columnIdx));
            if (values.Length != Rows)
                throw new ArgumentException("Value array is not equal to number of rows");

            for (int rowIdx = 0; rowIdx < Rows; rowIdx++)
            {
                Data[rowIdx, columnIdx] = values[rowIdx];
            }
        }
        public void Set(double[,] values)
        {
            if (values.GetLength(0) != Rows || values.GetLength(1) != Columns)
                throw new ArgumentException($"Data provided must have same size as matrix, [{Rows}x{Columns}], but was [{values.GetLength(0)}x{values.GetLength(1)}]");

            Array.Copy(values, Data, Rows * Columns);
        }

        public static Matrix operator +(Matrix mA, Matrix mB)
        {
            if (mA.Rows != mB.Rows || mA.Columns != mB.Columns)
                throw new InvalidOperationException("Cannot add matrices of different size");

            var mResult = new Matrix(mA.Rows, mA.Columns);
            mResult.Set(mA.Data.Sum(mB.Data));
            return mResult;
        }

        public static Matrix operator -(Matrix mA, Matrix mB)
        {
            if (mA.Rows != mB.Rows || mA.Columns != mB.Columns)
                throw new InvalidOperationException("Cannot add matrices of different size");

            var mResult = new Matrix(mA.Rows, mA.Columns);
            mResult.Set(mA.Data.Sum(mB.Data.ScalarMultiply(-1)));
            return mResult;
        }

        public double this[int rowIdx, int columnIdx]
        {
            get { return Data[rowIdx, columnIdx]; }
            set { Data[rowIdx, columnIdx] = value; }
        }

        public bool Equals(Matrix other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Rows == other.Rows && Columns == other.Columns)
            {
                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++)
                    {
                        if (Data[y, x] != other.Data[y, x]) return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Matrix;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Rows;
                hashCode = (hashCode * 397) ^ Columns;
                hashCode = (hashCode * 397) ^ (Data?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(Matrix left, Matrix right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Matrix left, Matrix right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            // Find column widths
            int[] columnWidths = new int[Columns];
            for (int y = 0; y < Rows; y++)
                for (int x = 0; x < Columns; x++)
                {
                    var valueString = Data[y, x].ToString();
                    columnWidths[x] = Math.Max(columnWidths[x], valueString.Length);
                }

            // Format output
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    var valueString = Data[y, x].ToString();
                    result.Append(valueString.PadLeft(columnWidths[x]));
                    if (x < Columns - 1) result.Append(' ');
                }
                if (y < Rows - 1) result.AppendLine();
            }

            return result.ToString();
        }
    }
}