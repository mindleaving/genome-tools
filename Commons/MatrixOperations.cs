using System;
using System.Threading.Tasks;

namespace Commons
{
    public static class MatrixOperations
    {
        public static double[,] IdentityMatrix(int dimensions, double diagonalValue)
        {
            if (dimensions <= 0)
            {
                return null;
            }
            var identityMatrix = new double[dimensions, dimensions];
            for (int r = 0; r < dimensions; r++)
            {
                for (int c = 0; c < dimensions; c++)
                {
                    identityMatrix[r, c] = (r == c) ? diagonalValue : 0;
                }
            }
            return identityMatrix;
        }
        public static double[,] IdentityMatrix(int dimensions)
        {
            return IdentityMatrix(dimensions, 1);
        }

        public static double Norm(this double[] v)
        {
            if (v == null)
            {
                return 0;
            }
            double norm = 0;
            for (int r = 0; r < v.Length; r++)
            {
                norm += v[r] * v[r];
            }
            norm = Math.Sqrt(norm);
            return norm;
        }

        public static double[,] Transpose(this double[,] array)
        {
            if (array == null)
            {
                return null;
            }
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            var transposedArray = new double[cols, rows];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    transposedArray[c, r] = array[r, c];
                }
            }
            return transposedArray;
        }

        public static double[] Sum(this double[] a, double[] b)
        {
            if (a == null || b == null)
            {
                return null;
            }
            if (b.Length != a.Length)
            {
                throw new ArgumentException("Vectors must have the same length");
            }
            var sumVector = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                sumVector[i] = a[i] + b[i];
            }
            return sumVector;
        }

        public static double[,] Sum(this double[,] A, double[,] B)
        {
            if (A == null || B == null)
            {
                return null;
            }
            int rows = A.GetLength(0);
            int cols = A.GetLength(1);
            if (B.GetLength(0) != rows || B.GetLength(1) != cols)
            {
                throw new ArgumentException("Arrays must have the same size");
            }
            var sumArray = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    sumArray[r, c] = A[r, c] + B[r, c];
                }
            }
            return sumArray;
        }

        public static double[] Subtract(this double[] a, double[] b)
        {
            if (a == null || b == null)
            {
                return null;
            }
            if (b.Length != a.Length)
            {
                throw new ArgumentException("Vectors must have the same length");
            }
            var differenceVector = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                differenceVector[i] = a[i] - b[i];
            }
            return differenceVector;
        }

        public static double[,] Subtract(this double[,] A, double[,] B)
        {
            if (A == null || B == null)
            {
                return null;
            }
            int rows = A.GetLength(0);
            int cols = A.GetLength(1);
            if (B.GetLength(0) != rows || B.GetLength(1) != cols)
            {
                throw new ArgumentException("Arrays must have the same size");
            }
            var differenceArray = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    differenceArray[r, c] = A[r, c] - B[r, c];
                }
            }
            return differenceArray;
        }

        public static double[,] Multiply(this double[,] A, double[,] B)
        {
            int rowsA = A.GetLength(0);
            int colsA = A.GetLength(1);
            int rowsB = B.GetLength(0);
            int colsB = B.GetLength(1);
            if (colsA != rowsB)
            {
                return null;
            }
            var multiplyArray = new double[rowsA, colsB];

            //for (int r = 0; r < rowsA; r++)
            Parallel.For(0, rowsA, r =>
            {
                for (int c = 0; c < colsB; c++)
                {
                    double sum = 0;
                    for (int i = 0; i < colsA; i++)
                    {
                        sum += A[r, i] * B[i, c];
                    }
                    multiplyArray[r, c] = sum;
                }
            });
            return multiplyArray;
        }

        public static double[] Vectorize(this double[,] A)
        {
            if (A == null) return null;

            var rows = A.GetLength(0);
            var cols = A.GetLength(1);
            var vector = new double[rows * cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    vector[c * rows + r] = A[r, c];
                }
            }
            return vector;
        }

        public static double[] Row(this double[,] matrix, int rowIdx)
        {
            var rows = matrix.GetLength(0);
            if(rows >= rowIdx)
                throw new ArgumentOutOfRangeException(nameof(rowIdx));
            var cols = matrix.GetLength(1);
            var row = new double[cols];
            for (int columnIdx = 0; columnIdx < cols; columnIdx++)
            {
                row[columnIdx] = matrix[rowIdx, columnIdx];
            }
            return row;
        }

        public static double[] Column(this double[,] matrix, int columnIdx)
        {
            var cols = matrix.GetLength(1);
            if (cols >= columnIdx)
                throw new ArgumentOutOfRangeException(nameof(columnIdx));
            var rows = matrix.GetLength(0);
            var column = new double[rows];
            for (int rowIdx = 0; rowIdx < rows; rowIdx++)
            {
                column[rowIdx] = matrix[rowIdx, columnIdx];
            }
            return column;
        }

        public static double[,] ConvertToMatrix(this double[] vector)
        {
            if (vector == null) return null;

            var array = new double[vector.Length, 1];
            for (int i = 0; i < vector.Length; i++)
            {
                array[i, 0] = vector[i];
            }
            return array;
        }

        public static double[] ScalarMultiply(this double[] vector, double scalar)
        {
            var multiplyVector = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                multiplyVector[i] = vector[i] * scalar;
            }
            return multiplyVector;
        }

        public static double[,] ScalarMultiply(this double[,] array, double scalar)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            var multiplyArray = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    multiplyArray[r, c] = array[r, c] * scalar;
                }
            }
            return multiplyArray;
        }

        public static double InnerProduct(this double[] A, double[] B)
        {
            if (A.Length != B.Length)
            {
                return double.NaN;
            }
            double product = 0;

            for (int r = 0; r < A.Length; r++)
            {
                product += A[r] * B[r];
            }
            return product;
        }

        public static double[,] OuterProduct(this double[] A, double[] B)
        {
            var outerProductArray = new double[A.Length, B.Length];

            for (int r = 0; r < A.Length; r++)
            {
                for (int c = 0; c < B.Length; c++)
                {
                    outerProductArray[r, c] = A[r] * B[c];
                }
            }
            return outerProductArray;
        }

        public static double Determinant(this double[,] array)
        {
            if (array == null)
            {
                return double.NaN;
            }
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            if (rows != cols)
            {
                return 0;
            }
            double determinant = 0;

            if (rows > 2)
            {
                // Search columns or rows with many zeros:
                int[] ZerosInRow = new int[rows];
                int[] ZerosInColumn = new int[cols];
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (Math.Abs(array[r, c] - 0) < double.Epsilon)
                        {
                            ZerosInRow[r]++;
                            ZerosInColumn[c]++;
                        }
                    }
                }

                int maxZeros = 0;
                int maxZeroIndex = 0;
                bool maxZerosAreInRow = true;
                for (int r = 0; r < rows; r++)
                {
                    if (ZerosInRow[r] > maxZeros)
                    {
                        maxZeros = ZerosInRow[r];
                        maxZeroIndex = r;
                    }
                }
                for (int c = 0; c < cols; c++)
                {
                    if (ZerosInColumn[c] > maxZeros)
                    {
                        maxZeros = ZerosInColumn[c];
                        maxZeroIndex = c;
                        maxZerosAreInRow = false;
                    }
                }

                if (maxZerosAreInRow)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (Math.Abs(array[maxZeroIndex, c] - 0) < double.Epsilon)
                        {
                            continue;
                        }
                        double[,] reducedMatrix = ReducedMatrix(array, maxZeroIndex, c);
                        int sign = ((maxZeroIndex + c) % 2 == 0) ? 1 : -1;

                        determinant += sign * array[maxZeroIndex, c] * Determinant(reducedMatrix);
                    }

                }
                else
                {
                    for (int r = 0; r < rows; r++)
                    {
                        if (Math.Abs(array[r, maxZeroIndex] - 0) < double.Epsilon)
                        {
                            continue;
                        }
                        double[,] reducedMatrix = ReducedMatrix(array, r, maxZeroIndex);
                        int sign = ((maxZeroIndex + r) % 2 == 0) ? 1 : -1;

                        determinant += sign * array[r, maxZeroIndex] * Determinant(reducedMatrix);
                    }
                }

            }
            else
            {
                if (rows == 1)
                {
                    determinant = array[0, 0];
                }
                else
                {
                    determinant = array[0, 0] * array[1, 1] - array[0, 1] * array[1, 0];
                }
            }
            return determinant;
        }

        public static double[,] ReducedMatrix(this double[,] array, int rowRemove, int colRemove)
        {
            if (array == null)
            {
                return null;
            }
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            if (rows < 2 || cols < 2)
            {
                throw new ArgumentException("Array of size smaller than 2x2 cannot be reduced");
            }
            var reducedMatrix = new double[rows - 1, cols - 1];
            int r1 = 0;
            int c1 = 0;
            int rR = 0;
            int cR = 0;
            while (r1 < rows)
            {
                if (r1 == rowRemove)
                {
                    r1++;
                    if (r1 >= rows)
                    {
                        break;
                    }
                }
                while (c1 < cols)
                {
                    if (c1 == colRemove)
                    {
                        c1++;
                        if (c1 >= cols)
                        {
                            break;
                        }
                    }
                    reducedMatrix[rR, cR] = array[r1, c1];
                    cR++;
                    c1++;
                }
                cR = 0;
                c1 = 0;
                rR++;
                r1++;
            }
            return reducedMatrix;
        }

        public static double[,] Adjugated(this double[,] array)
        {
            if (array == null)
            {
                return null;
            }
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            if (rows != cols)
            {
                throw new ArgumentException("Array must be square to calculate the adjugated matrix");
            }
            var adjugatedArray = new double[rows, cols];
            if (rows < 2)
            {
                adjugatedArray[0, 0] = 1;
            }
            else
            {
                //for (int r = 0; r < rows; r++)
                Parallel.For(0, rows, r =>
                {
                    for (int c = 0; c < cols; c++)
                    {
                        int sign = 1;
                        if ((c + r) % 2 == 1)
                        {
                            sign = -1;
                        }
                        adjugatedArray[r, c] = sign * Determinant(ReducedMatrix(array, r, c));
                    }
                });
            }

            return adjugatedArray;
        }

        public static double[,] Inverse(this double[,] array)
        {
            if (array == null)
            {
                return null;
            }
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            if (rows != cols)
            {
                throw new ArgumentException("Array must be square to be inversed");
            }

            double arrayDeterminant = Determinant(array);
            if (arrayDeterminant == 0)
            {
                var zeroMatrix = new double[rows, rows];
                return zeroMatrix;
            }

            double[,] inverseArray = Adjugated(array);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    inverseArray[r, c] /= arrayDeterminant;
                }
            }
            return inverseArray;
        }

        public static double[,] Covariance(this double[,] array, int dimension)
        {
            if (array == null || dimension < 0 || dimension > 1)
            {
                return null;
            }
            if (dimension == 0)
            {
                array = Transpose(array);
            }
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            var covarianceArray = new double[cols, cols];

            var X = new double[rows];
            var Y = new double[rows];
            double meanX, meanY;


            for (int c1 = 0; c1 < cols; c1++)
            {
                // Get X-mean:
                for (int r = 0; r < rows; r++)
                {
                    X[r] = array[r, c1];
                }
                meanX = StatisticalOperations.Mean(X);
                for (int c2 = c1; c2 < cols; c2++)
                {

                    if (c1 == c2) // Diagonal
                    {
                        covarianceArray[c1, c2] = StatisticalOperations.Covariance(X, X, meanX, meanX);
                    }
                    else // Off-diagonal
                    {
                        // Get Y-mean:
                        for (int r = 0; r < rows; r++)
                        {
                            Y[r] = array[r, c2];
                        }
                        meanY = StatisticalOperations.Mean(Y);
                        // Calculate covariance. The Covariance matrix is symmetric!
                        covarianceArray[c1, c2] = covarianceArray[c2, c1] = StatisticalOperations.Covariance(X, Y, meanX, meanY);
                    }
                }
            }

            return covarianceArray;
        }

        public static double[] Eigenvalues(this double[,] array)
        {
            if (array == null)
            {
                return null;
            }
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            if (rows != cols)
            {
                return null;
            }

            array = HesselbergMatrix(array);

            var eigenvalueArray = new double[rows];
            for (int r = 0; r < rows; r++)
            {
                eigenvalueArray[r] = array[r, r];
            }

            double[,] Q;
            double[,] R;

            double change = double.MaxValue;
            int iterations = 0;
            while (change > rows * 1e-6 && iterations < (int)1e3)
            {
                iterations++;

                QRDecomposition(array, out Q, out R);
                array = Multiply(R, Q);

                change = 0;
                for (int r = 0; r < rows; r++)
                {
                    change += Math.Abs(eigenvalueArray[r] - array[r, r]);
                    eigenvalueArray[r] = array[r, r];
                }
            }

            return eigenvalueArray;
        }

        public static double[,] Eigenvectors(this double[,] array, double[] eigenvalues)
        {
            if (array == null)
            {
                return null;
            }
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            if (rows != cols || eigenvalues.Length != rows)
            {
                return null;
            }
            var eigenvectorsArray = new double[rows, rows];

            double[,] rrefArray;
            double norm;
            for (int i = 0; i < rows; i++)
            {
                // Substract the eigenvalue:
                for (int diag = 0; diag < rows; diag++)
                {
                    if (i > 0)
                    {
                        array[diag, diag] -= eigenvalues[i] - eigenvalues[i - 1];
                    }
                    else
                    {
                        array[diag, diag] -= eigenvalues[i];
                    }
                }
                rrefArray = ReducedRowEchelonForm(array);

                // TODO: Modify to catch Eigenvectors with multiplicity > 1
                norm = 1;
                for (int diag = 0; diag < rows; diag++)
                {
                    if (diag < rows - 1)
                    {
                        eigenvectorsArray[i, diag] = -rrefArray[diag, rows - 1];
                        norm += rrefArray[diag, rows - 1] * rrefArray[diag, rows - 1];
                    }
                    else
                    {
                        eigenvectorsArray[i, diag] = 1;
                    }
                }
                // Normalize:
                norm = Math.Sqrt(norm);
                for (int r = 0; r < rows; r++)
                {
                    eigenvectorsArray[i, r] /= norm;
                }
            }

            return eigenvectorsArray;
        }

        public static double[,] ReducedRowEchelonForm(this double[,] array)
        { // With a little cheating for zeros in the diagonal!!!
            if (array == null)
            {
                return null;
            }
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            double[,] rrefArray = new double[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    rrefArray[r, c] = array[r, c];
                }
            }

            for (int operationRow = 0; operationRow < rows; operationRow++)
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = cols - 1; c >= 0; c--)
                    {
                        if (Math.Abs(rrefArray[operationRow, operationRow]) < double.Epsilon)
                        {
                            operationRow++;
                            if (operationRow == rows)
                            {
                                return rrefArray;
                            }
                        }
                        if (r == operationRow)
                        {
                            if (Math.Abs(rrefArray[operationRow, operationRow]) < double.Epsilon)
                            {
                                rrefArray[operationRow, operationRow] = double.Epsilon;
                            }
                            rrefArray[r, c] /= rrefArray[operationRow, operationRow];
                        }
                        else
                        {
                            if (Math.Abs(rrefArray[operationRow, operationRow]) < double.Epsilon)
                            {
                                rrefArray[operationRow, operationRow] = double.Epsilon;
                            }
                            rrefArray[r, c] -= rrefArray[r, operationRow] * rrefArray[operationRow, c] / rrefArray[operationRow, operationRow];

                        }
                    }
                }
            }

            return rrefArray;
        }

        public static double[,] HesselbergMatrix(this double[,] array)
        {
            if (array == null)
            {
                return null;
            }
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            if (rows != cols)
            {
                return null;
            }
            var hesselbergArray = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    hesselbergArray[r, c] = array[r, c];
                }
            }

            double[,] P;
            double[] transformVector;
            for (int c = 0; c < cols - 2; c++)
            {
                transformVector = new double[cols - c - 1];
                for (int r = c + 1; r < rows; r++)
                {
                    transformVector[r - c - 1] = hesselbergArray[r, c];
                }
                P = HouseholderTransform(transformVector, rows);
                hesselbergArray = Multiply(P, hesselbergArray);
                hesselbergArray = Multiply(hesselbergArray, P);
            }

            return hesselbergArray;
        }

        public static double[,] HouseholderTransform(this double[] x, int dimensions)
        {
            if (x == null || dimensions <= 0)
            {
                return null;
            }
            double[,] householderTransformArray = IdentityMatrix(dimensions);

            var u = new double[x.Length];
            for (int r = 0; r < u.Length; r++)
            {
                u[r] = x[r];
            }
            u[0] -= Norm(x);
            double normU = Norm(u);
            var p = ScalarMultiply(OuterProduct(u, u), -2 / (normU * normU));
            p = Sum(IdentityMatrix(u.Length), p);
            for (int r = dimensions - u.Length; r < dimensions; r++) // Index relative to HouseholderTransformArray
            {
                for (int c = dimensions - u.Length; c < dimensions; c++) // Index relative to HouseholderTransformArray
                {
                    householderTransformArray[r, c] = p[r - dimensions + u.Length, c - dimensions + u.Length];
                }
            }

            return householderTransformArray;
        }

        public static bool QRDecomposition(this double[,] A, out double[,] Q, out double[,] R)
        {
            Q = null;
            R = null;
            if (A == null)
            {
                return false;
            }
            int rows = A.GetLength(0);
            int cols = A.GetLength(1);
            if (rows != cols)
            {
                return false;
            }
            double[,] Ak = new double[rows, rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Ak[r, c] = A[r, c];
                }
            }
            Q = new double[rows, rows];
            R = new double[rows, rows];
            double[,] Pk;
            double[] transformVector;
            for (int d = 0; d < rows - 1; d++)
            {
                transformVector = new double[rows - d];
                for (int r = d; r < rows; r++)
                {
                    transformVector[r - d] = Ak[r, d];
                }
                Pk = HouseholderTransform(transformVector, rows);
                Ak = Multiply(Pk, Ak);

                if (d == 0)
                {
                    Q = Pk;
                    R = Ak;
                }
                else
                {
                    Q = Multiply(Q, Pk);
                    R = Multiply(Pk, R);
                }
                // Are Q and R references?
                //if (d == 0)
                //{
                //    for (int r = 0; r < rows; r++)
                //    {
                //        for (int c = 0; c < cols; c++)
                //        {
                //            Q[r, c] = Pk[r, c];
                //            R[r, c] = Ak[r, c];
                //        }
                //    }
                //}

            }

            return true;
        }
    }
}