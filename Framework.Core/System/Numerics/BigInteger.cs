﻿#if LESSTHAN_NET40 || NETSTANDARD1_0

#pragma warning disable CA2225 // Operator overloads have named alternates

using System.Globalization;
using Theraot.Core;

namespace System.Numerics
{
    /// <summary>Represents an arbitrarily large signed integer.</summary>
    [Serializable]
    public partial struct BigInteger : IFormattable, IComparable, IComparable<BigInteger>, IEquatable<BigInteger>
    {
        internal readonly uint[]? InternalBits;
        internal readonly int InternalSign;
        private static readonly BigInteger _bigIntegerMinInt = new BigInteger(-1, new[] { unchecked((uint)int.MinValue) });

        /// <summary>
        ///     Initializes a new instance of the <see cref="BigInteger" /> structure using a 32-bit signed
        ///     integer value.
        /// </summary>
        /// <param name="value">A 32-bit signed integer.</param>
        public BigInteger(int value)
        {
            if (value != int.MinValue)
            {
                InternalSign = value;
                InternalBits = null;
            }
            else
            {
                this = _bigIntegerMinInt;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BigInteger" /> structure using an unsigned
        ///     32-bit integer value.
        /// </summary>
        /// <param name="value">An unsigned 32-bit integer value.</param>
        [CLSCompliant(false)]
        public BigInteger(uint value)
        {
            if (value > int.MaxValue)
            {
                InternalSign = 1;
                InternalBits = new[] { value };
            }
            else
            {
                InternalSign = (int)value;
                InternalBits = null;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BigInteger" /> structure using a 64-bit signed
        ///     integer value.
        /// </summary>
        /// <param name="value">A 64-bit signed integer.</param>
        public BigInteger(long value)
        {
            if (int.MinValue <= value && value <= int.MaxValue)
            {
                if (value != int.MinValue)
                {
                    InternalSign = (int)value;
                    InternalBits = null;
                }
                else
                {
                    this = _bigIntegerMinInt;
                }

                return;
            }

            ulong num;
            if (value >= 0)
            {
                num = (ulong)value;
                InternalSign = 1;
            }
            else
            {
                num = (ulong)-value;
                InternalSign = -1;
            }

            InternalBits = new[] { (uint)num, (uint)(num >> 32) };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BigInteger" /> structure with an unsigned
        ///     64-bit integer value.
        /// </summary>
        /// <param name="value">An unsigned 64-bit integer.</param>
        [CLSCompliant(false)]
        public BigInteger(ulong value)
        {
            if (value > int.MaxValue)
            {
                InternalSign = 1;
                InternalBits = new[] { (uint)value, (uint)(value >> 32) };
            }
            else
            {
                InternalSign = (int)value;
                InternalBits = null;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BigInteger" /> structure using a
        ///     single-precision floating-point value.
        /// </summary>
        /// <param name="value">A single-precision floating-point value.</param>
        /// <exception cref="OverflowException">
        ///     The value of <paramref name="value" /> is
        ///     <see cref="System.Single.NaN" />.-or-The value of <paramref name="value" /> is
        ///     <see cref="System.Single.NegativeInfinity" />.-or-The value of <paramref name="value" /> is
        ///     <see cref="System.Single.PositiveInfinity" />.
        /// </exception>
        public BigInteger(float value)
        {
            if (float.IsInfinity(value))
            {
                throw new OverflowException("BigInteger cannot represent infinity.");
            }

            if (float.IsNaN(value))
            {
                throw new OverflowException("The value is not a number.");
            }

            SetBitsFromDouble(value, out InternalBits, out InternalSign);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BigInteger" /> structure using a
        ///     double-precision floating-point value.
        /// </summary>
        /// <param name="value">A double-precision floating-point value.</param>
        /// <exception cref="OverflowException">
        ///     The value of <paramref name="value" /> is
        ///     <see cref="System.Double.NaN" />.-or-The value of <paramref name="value" /> is
        ///     <see cref="System.Double.NegativeInfinity" />.-or-The value of <paramref name="value" /> is
        ///     <see cref="System.Double.PositiveInfinity" />.
        /// </exception>
        public BigInteger(double value)
        {
            if (double.IsInfinity(value))
            {
                throw new OverflowException("BigInteger cannot represent infinity.");
            }

            if (double.IsNaN(value))
            {
                throw new OverflowException("The value is not a number.");
            }

            SetBitsFromDouble(value, out InternalBits, out InternalSign);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BigInteger" /> structure using a
        ///     <see cref="decimal" /> value.
        /// </summary>
        /// <param name="value">A decimal number.</param>
        public BigInteger(decimal value)
        {
            var bits = decimal.GetBits(decimal.Truncate(value));
            var size = 3;
            while (size > 0 && bits[size - 1] == 0)
            {
                size--;
            }

            if (size == 0)
            {
                this = Zero;
            }
            else if (size != 1 || bits[0] <= 0)
            {
                InternalBits = new uint[size];
                InternalBits[0] = (uint)bits[0];
                if (size > 1)
                {
                    InternalBits[1] = (uint)bits[1];
                }

                if (size > 2)
                {
                    InternalBits[2] = (uint)bits[2];
                }

                InternalSign = (bits[3] & int.MinValue) == 0 ? 1 : -1;
            }
            else
            {
                InternalSign = bits[0];
                InternalSign *= (bits[3] & int.MinValue) == 0 ? 1 : -1;
                InternalBits = null;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BigInteger" /> structure using the values in a
        ///     byte array.
        /// </summary>
        /// <param name="value">An array of byte values in little-endian order.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" /> is null.
        /// </exception>
        [CLSCompliant(false)]
        public BigInteger(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var valueLength = value.Length;
            var isNegative = valueLength > 0 && (value[valueLength - 1] & 128) == 128;
            while (valueLength > 0 && value[valueLength - 1] == 0)
            {
                valueLength--;
            }

            if (valueLength == 0)
            {
                InternalSign = 0;
                InternalBits = null;
                return;
            }

            if (valueLength > 4)
            {
                var unalignedBytes = valueLength % 4;
                var dwordCount = (valueLength / 4) + (unalignedBytes == 0 ? 0 : 1);
                var isZero = true;
                var internalBits = new uint[dwordCount];
                var byteIndex = 3;
                var dwordIndex = 0;
                for (; dwordIndex < dwordCount - (unalignedBytes == 0 ? 0 : 1); dwordIndex++)
                {
                    ref var current = ref internalBits[dwordIndex];
                    for (var byteInDword = 0; byteInDword < 4; byteInDword++)
                    {
                        isZero &= value[byteIndex] == 0;
                        current <<= 8;
                        current |= value[byteIndex];
                        byteIndex--;
                    }

                    byteIndex += 8;
                }

                if (unalignedBytes != 0)
                {
                    if (isNegative)
                    {
                        internalBits[dwordCount - 1] = 0xffffffff;
                    }

                    for (byteIndex = valueLength - 1; byteIndex >= valueLength - unalignedBytes; byteIndex--)
                    {
                        ref var current = ref internalBits[dwordIndex];
                        ref var currentValue = ref value[byteIndex];
                        isZero &= currentValue == 0;
                        current <<= 8;
                        current |= currentValue;
                    }
                }

                if (isZero)
                {
                    this = Zero;
                }
                else if (isNegative)
                {
                    NumericsHelpers.DangerousMakeTwosComplement(internalBits);
                    dwordCount = internalBits.Length;
                    while (dwordCount > 0 && internalBits[dwordCount - 1] == 0)
                    {
                        dwordCount--;
                    }

                    if (dwordCount == 1 && internalBits[0] > 0)
                    {
                        switch (internalBits[0])
                        {
                            case 1:
                                this = MinusOne;
                                break;

                            case unchecked((uint)int.MinValue):
                                this = _bigIntegerMinInt;
                                break;

                            default:
                                InternalSign = -1;
                                InternalBits = internalBits;
                                break;
                        }
                    }
                    else
                    {
                        if (dwordCount == internalBits.Length)
                        {
                            InternalSign = -1;
                            InternalBits = internalBits;
                        }
                        else
                        {
                            InternalSign = -1;
                            InternalBits = new uint[dwordCount];
                            Array.Copy(internalBits, InternalBits, dwordCount);
                        }
                    }
                }
                else
                {
                    InternalSign = 1;
                    InternalBits = internalBits;
                }
            }
            else
            {
                InternalSign = isNegative ? -1 : 0;
                for (var index = valueLength - 1; index >= 0; index--)
                {
                    InternalSign <<= 8;
                    InternalSign |= value[index];
                }

                InternalBits = null;
                if (InternalSign < 0 && !isNegative)
                {
                    InternalBits = new uint[1];
                    InternalBits[0] = (uint)InternalSign;
                    InternalSign = 1;
                }

                if (InternalSign == int.MinValue)
                {
                    this = _bigIntegerMinInt;
                }
            }
        }

        internal BigInteger(int internalSign, uint[]? rgu)
        {
            InternalSign = internalSign;
            InternalBits = rgu;
        }

        internal BigInteger(uint[] value, bool negative)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var length = value.Length;
            while (length > 0 && value[length - 1] == 0)
            {
                length--;
            }

            if (length == 0)
            {
                this = Zero;
            }
            else if (length != 1 || value[0] >= unchecked((uint)int.MinValue))
            {
                InternalSign = !negative ? 1 : -1;
                InternalBits = new uint[length];
                Array.Copy(value, InternalBits, length);
            }
            else
            {
                InternalSign = !negative ? (int)value[0] : (int)-value[0];
                InternalBits = null;
                if (InternalSign == int.MinValue)
                {
                    this = _bigIntegerMinInt;
                }
            }
        }

        private BigInteger(uint[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var dwordCount = value.Length;
            var isNegative = dwordCount > 0 && (value[dwordCount - 1] & unchecked((uint)int.MinValue)) == unchecked((uint)int.MinValue);
            while (dwordCount > 0 && value[dwordCount - 1] == 0)
            {
                dwordCount--;
            }

            switch (dwordCount)
            {
                case 0:
                    this = Zero;
                    return;

                case 1:
                    if ((int)value[0] < 0 && !isNegative)
                    {
                        InternalBits = new[] { value[0] };
                        InternalSign = 1;
                    }
                    else if (value[0] != unchecked((uint)int.MinValue))
                    {
                        InternalSign = (int)value[0];
                        InternalBits = null;
                    }
                    else
                    {
                        this = _bigIntegerMinInt;
                    }

                    return;

                default:
                    break;
            }

            if (!isNegative)
            {
                if (dwordCount == value.Length)
                {
                    InternalSign = 1;
                    InternalBits = value;
                }
                else
                {
                    InternalSign = 1;
                    InternalBits = new uint[dwordCount];
                    Array.Copy(value, InternalBits, dwordCount);
                }

                return;
            }

            NumericsHelpers.DangerousMakeTwosComplement(value);
            var length = value.Length;
            while (length > 0 && value[length - 1] == 0)
            {
                length--;
            }

            if (length != 1 || value[0] <= 0)
            {
                if (length == value.Length)
                {
                    InternalSign = -1;
                    InternalBits = value;
                }
                else
                {
                    InternalSign = -1;
                    InternalBits = new uint[length];
                    Array.Copy(value, InternalBits, length);
                }
            }
            else if (value[0] == 1)
            {
                this = MinusOne;
            }
            else if (value[0] != unchecked((uint)int.MinValue))
            {
                InternalSign = (int)(-1 * value[0]);
                InternalBits = null;
            }
            else
            {
                this = _bigIntegerMinInt;
            }
        }

        /// <summary>Gets a value that represents the number negative one (-1).</summary>
        /// <returns>An integer whose value is negative one (-1).</returns>
        public static BigInteger MinusOne { get; } = new BigInteger(-1);

        /// <summary>Gets a value that represents the number one (1).</summary>
        /// <returns>An object whose value is one (1).</returns>
        public static BigInteger One { get; } = new BigInteger(1);

        /// <summary>Gets a value that represents the number 0 (zero).</summary>
        /// <returns>An integer whose value is 0 (zero).</returns>
        public static BigInteger Zero { get; } = new BigInteger(0);

        /// <summary>
        ///     Indicates whether the value of the current <see cref="BigInteger" /> object is an even
        ///     number.
        /// </summary>
        /// <returns>
        ///     true if the value of the <see cref="BigInteger" /> object is an even number; otherwise,
        ///     false.
        /// </returns>
        public bool IsEven => InternalBits != null ? (InternalBits[0] & 1) == 0 : (InternalSign & 1) == 0;

        /// <summary>
        ///     Indicates whether the value of the current <see cref="BigInteger" /> object is
        ///     <see cref="One" />.
        /// </summary>
        /// <returns>
        ///     true if the value of the <see cref="BigInteger" /> object is
        ///     <see cref="One" />; otherwise, false.
        /// </returns>
        public bool IsOne => InternalSign == 1 && InternalBits == null;

        /// <summary>
        ///     Indicates whether the value of the current <see cref="BigInteger" /> object is a power of
        ///     two.
        /// </summary>
        /// <returns>
        ///     true if the value of the <see cref="BigInteger" /> object is a power of two; otherwise,
        ///     false.
        /// </returns>
        public bool IsPowerOfTwo
        {
            get
            {
                if (InternalBits == null)
                {
                    return (InternalSign & (InternalSign - 1)) == 0 && InternalSign != 0;
                }

                if (InternalSign != 1)
                {
                    return false;
                }

                var index = Length(InternalBits) - 1;
                if ((InternalBits[index] & (InternalBits[index] - 1)) != 0)
                {
                    return false;
                }

                do
                {
                    index--;
                    if (index >= 0)
                    {
                        continue;
                    }

                    return true;
                } while (InternalBits[index] == 0);

                return false;
            }
        }

        /// <summary>
        ///     Indicates whether the value of the current <see cref="BigInteger" /> object is
        ///     <see cref="Zero" />.
        /// </summary>
        /// <returns>
        ///     true if the value of the <see cref="BigInteger" /> object is
        ///     <see cref="Zero" />; otherwise, false.
        /// </returns>
        public bool IsZero => InternalSign == 0;

        /// <summary>
        ///     Gets a number that indicates the sign (negative, positive, or zero) of the current
        ///     <see cref="BigInteger" /> object.
        /// </summary>
        /// <returns>
        ///     A number that indicates the sign of the <see cref="BigInteger" /> object, as shown in the
        ///     following table.NumberDescription-1The value of this object is negative.0The value of this object is 0 (zero).1The
        ///     value of this object is positive.
        /// </returns>
        public int Sign => (InternalSign >> 31) - (-InternalSign >> 31);

        /// <summary>Gets the absolute value of a <see cref="BigInteger" /> object.</summary>
        /// <returns>The absolute value of <paramref name="value" />.</returns>
        /// <param name="value">A number.</param>
        public static BigInteger Abs(BigInteger value)
        {
            return value < Zero ? -value : value;
        }

        /// <summary>Adds two <see cref="BigInteger" /> values and returns the result.</summary>
        /// <returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        public static BigInteger Add(BigInteger left, BigInteger right)
        {
            return left + right;
        }

        /// <summary>
        ///     Compares two <see cref="BigInteger" /> values and returns an integer that indicates whether
        ///     the first value is less than, equal to, or greater than the second value.
        /// </summary>
        /// <returns>
        ///     A signed integer that indicates the relative values of <paramref name="left" /> and <paramref name="right" />,
        ///     as shown in the following table.ValueConditionLess than zero<paramref name="left" /> is less than
        ///     <paramref name="right" />.Zero<paramref name="left" /> equals <paramref name="right" />.Greater than zero
        ///     <paramref name="left" /> is greater than <paramref name="right" />.
        /// </returns>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        public static int Compare(BigInteger left, BigInteger right)
        {
            return left.CompareTo(right);
        }

        /// <summary>Divides one <see cref="BigInteger" /> value by another and returns the result.</summary>
        /// <returns>The quotient of the division.</returns>
        /// <param name="dividend">The value to be divided.</param>
        /// <param name="divisor">The value to divide by.</param>
        /// <exception cref="DivideByZeroException">
        ///     <paramref name="divisor" /> is 0 (zero).
        /// </exception>
        public static BigInteger Divide(BigInteger dividend, BigInteger divisor)
        {
            return dividend / divisor;
        }

        /// <summary>
        ///     Divides one <see cref="BigInteger" /> value by another, returns the result, and returns the
        ///     remainder in an output parameter.
        /// </summary>
        /// <returns>The quotient of the division.</returns>
        /// <param name="dividend">The value to be divided.</param>
        /// <param name="divisor">The value to divide by.</param>
        /// <param name="remainder">
        ///     When this method returns, contains a <see cref="BigInteger" /> value that
        ///     represents the remainder from the division. This parameter is passed uninitialized.
        /// </param>
        /// <exception cref="DivideByZeroException">
        ///     <paramref name="divisor" /> is 0 (zero).
        /// </exception>
        public static BigInteger DivRem(BigInteger dividend, BigInteger divisor, out BigInteger remainder)
        {
            var signNum = 1;
            var signDen = 1;
            var regNum = new BigIntegerBuilder(dividend, ref signNum);
            var regDen = new BigIntegerBuilder(divisor, ref signDen);
            var regQuo = new BigIntegerBuilder();
            regNum.ModDiv(ref regDen, ref regQuo);
            remainder = regNum.GetInteger(signNum);
            return regQuo.GetInteger(signNum * signDen);
        }

        /// <summary>Finds the greatest common divisor of two <see cref="BigInteger" /> values.</summary>
        /// <returns>The greatest common divisor of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        public static BigInteger GreatestCommonDivisor(BigInteger left, BigInteger right)
        {
            if (left.InternalSign == 0)
            {
                return Abs(right);
            }

            if (right.InternalSign == 0)
            {
                return Abs(left);
            }

            var bigIntegerBuilder = new BigIntegerBuilder(left);
            var bigIntegerBuilder1 = new BigIntegerBuilder(right);
            BigIntegerBuilder.Gcd(ref bigIntegerBuilder, ref bigIntegerBuilder1);
            return bigIntegerBuilder.GetInteger(1);
        }

        /// <summary>Returns the natural (base e) logarithm of a specified number.</summary>
        /// <returns>The natural (base e) logarithm of <paramref name="value" />, as shown in the table in the Remarks section.</returns>
        /// <param name="value">The number whose logarithm is to be found.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     The natural log of <paramref name="value" /> is out of range of
        ///     the <see cref="double" /> data type.
        /// </exception>
        public static double Log(BigInteger value)
        {
            return Log(value, Math.E);
        }

        /// <summary>Returns the logarithm of a specified number in a specified base.</summary>
        /// <returns>
        ///     The base <paramref name="baseValue" /> logarithm of <paramref name="value" />, as shown in the table in the
        ///     Remarks section.
        /// </returns>
        /// <param name="value">A number whose logarithm is to be found.</param>
        /// <param name="baseValue">The base of the logarithm.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     The log of <paramref name="value" /> is out of range of the
        ///     <see cref="double" /> data type.
        /// </exception>
        public static double Log(BigInteger value, double baseValue)
        {
            if (value.InternalSign < 0 || NumericHelper.IsOne(baseValue))
            {
                return double.NaN;
            }

            if (double.IsPositiveInfinity(baseValue))
            {
                return !value.IsOne ? double.NaN : 0;
            }

            if (NumericHelper.IsZero(baseValue) && !value.IsOne)
            {
                return double.NaN;
            }

            if (value.InternalBits == null)
            {
                return Math.Log(value.InternalSign, baseValue);
            }

            double c = 0;
            var d = 0.5;
            var length = Length(value.InternalBits);
            var topBits = BitLengthOfUInt(value.InternalBits[length - 1]);
            var bitlen = ((length - 1) * 32) + topBits;
            var currentBitMask = (uint)(1 << ((topBits - 1) & 31));
            for (var index = length - 1; index >= 0; index--)
            {
                while (currentBitMask != 0)
                {
                    if ((value.InternalBits[index] & currentBitMask) != 0)
                    {
                        c += d;
                    }

                    d *= 0.5;
                    currentBitMask >>= 1;
                }

                currentBitMask = unchecked((uint)int.MinValue);
            }

            return (Math.Log(c) + (0.69314718055994529D * bitlen)) / Math.Log(baseValue);
        }

        /// <summary>Returns the base 10 logarithm of a specified number.</summary>
        /// <returns>The base 10 logarithm of <paramref name="value" />, as shown in the table in the Remarks section.</returns>
        /// <param name="value">A number whose logarithm is to be found.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     The base 10 log of <paramref name="value" /> is out of range of
        ///     the <see cref="double" /> data type.
        /// </exception>
        public static double Log10(BigInteger value)
        {
            return Log(value, 10);
        }

        /// <summary>Returns the larger of two <see cref="BigInteger" /> values.</summary>
        /// <returns>The <paramref name="left" /> or <paramref name="right" /> parameter, whichever is larger.</returns>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        public static BigInteger Max(BigInteger left, BigInteger right)
        {
            return left.CompareTo(right) < 0 ? right : left;
        }

        /// <summary>Returns the smaller of two <see cref="BigInteger" /> values.</summary>
        /// <returns>The <paramref name="left" /> or <paramref name="right" /> parameter, whichever is smaller.</returns>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        public static BigInteger Min(BigInteger left, BigInteger right)
        {
            return left.CompareTo(right) <= 0 ? left : right;
        }

        /// <summary>Performs modulus division on a number raised to the power of another number.</summary>
        /// <returns>The remainder after dividing <paramref name="value" />exponent by <paramref name="modulus" />.</returns>
        /// <param name="value">The number to raise to the <paramref name="exponent" /> power.</param>
        /// <param name="exponent">The exponent to raise <paramref name="value" /> by.</param>
        /// <param name="modulus">
        ///     The number by which to divide <paramref name="value" /> raised to the
        ///     <paramref name="exponent" /> power.
        /// </param>
        /// <exception cref="DivideByZeroException">
        ///     <paramref name="modulus" /> is zero.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="exponent" /> is negative.
        /// </exception>
        public static BigInteger ModPow(BigInteger value, BigInteger exponent, BigInteger modulus)
        {
            if (exponent.Sign < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exponent), "The number must be greater than or equal to zero.");
            }

            var signRes = 1;
            var signVal = 1;
            var signMod = 1;
            var isEven = exponent.IsEven;
            var regRes = new BigIntegerBuilder(One, ref signRes);
            var regVal = new BigIntegerBuilder(value, ref signVal);
            var regMod = new BigIntegerBuilder(modulus, ref signMod);
            var regTmp = new BigIntegerBuilder(regVal.Size);
            regRes.Mod(ref regMod);
            if (exponent.InternalBits != null)
            {
                var length = Length(exponent.InternalBits);
                for (var index = 0; index < length - 1; index++)
                {
                    var num5 = exponent.InternalBits[index];
                    ModPowInner32(num5, ref regRes, ref regVal, ref regMod, ref regTmp);
                }

                ModPowInner(exponent.InternalBits[length - 1], ref regRes, ref regVal, ref regMod, ref regTmp);
            }
            else
            {
                ModPowInner((uint)exponent.InternalSign, ref regRes, ref regVal, ref regMod, ref regTmp);
            }

            return regRes.GetInteger(value.InternalSign <= 0 ? !isEven ? -1 : 1 : 1);
        }

        /// <summary>Returns the product of two <see cref="BigInteger" /> values.</summary>
        /// <returns>The product of the <paramref name="left" /> and <paramref name="right" /> parameters.</returns>
        /// <param name="left">The first number to multiply.</param>
        /// <param name="right">The second number to multiply.</param>
        public static BigInteger Multiply(BigInteger left, BigInteger right)
        {
            return left * right;
        }

        /// <summary>Negates a specified <see cref="BigInteger" /> value.</summary>
        /// <returns>The result of the <paramref name="value" /> parameter multiplied by negative one (-1).</returns>
        /// <param name="value">The value to negate.</param>
        public static BigInteger Negate(BigInteger value)
        {
            return -value;
        }

        /// <summary>Converts the string representation of a number to its <see cref="BigInteger" /> equivalent.</summary>
        /// <returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" /> is null.
        /// </exception>
        /// <exception cref="FormatException">
        ///     <paramref name="value" /> is not in the correct format.
        /// </exception>
        public static BigInteger Parse(string value)
        {
            return ParseBigInteger(value, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }

        /// <summary>
        ///     Converts the string representation of a number in a specified style to its
        ///     <see cref="BigInteger" /> equivalent.
        /// </summary>
        /// <returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
        /// <param name="value">A string that contains a number to convert. </param>
        /// <param name="style">
        ///     A bitwise combination of the enumeration values that specify the permitted format of
        ///     <paramref name="value" />.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="style" /> is not a <see cref="NumberStyles" /> value.-or-
        ///     <paramref name="style" /> includes the <see cref="NumberStyles.AllowHexSpecifier" /> or
        ///     <see cref="NumberStyles.HexNumber" /> flag along with another value.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" /> is null.
        /// </exception>
        /// <exception cref="FormatException">
        ///     <paramref name="value" /> does not comply with the input pattern specified by
        ///     <see cref="NumberStyles" />.
        /// </exception>
        public static BigInteger Parse(string value, NumberStyles style)
        {
            return ParseBigInteger(value, style, NumberFormatInfo.CurrentInfo);
        }

        /// <summary>
        ///     Converts the string representation of a number in a specified culture-specific format to its
        ///     <see cref="BigInteger" /> equivalent.
        /// </summary>
        /// <returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
        /// <param name="value">A string that contains a number to convert.</param>
        /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="value" />.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" /> is null.
        /// </exception>
        /// <exception cref="FormatException">
        ///     <paramref name="value" /> is not in the correct format.
        /// </exception>
        public static BigInteger Parse(string value, IFormatProvider provider)
        {
            return ParseBigInteger(value, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }

        /// <summary>
        ///     Converts the string representation of a number in a specified style and culture-specific format to its
        ///     <see cref="BigInteger" /> equivalent.
        /// </summary>
        /// <returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
        /// <param name="value">A string that contains a number to convert.</param>
        /// <param name="style">
        ///     A bitwise combination of the enumeration values that specify the permitted format of
        ///     <paramref name="value" />.
        /// </param>
        /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="value" />.</param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="style" /> is not a <see cref="NumberStyles" /> value.-or-
        ///     <paramref name="style" /> includes the <see cref="NumberStyles.AllowHexSpecifier" /> or
        ///     <see cref="NumberStyles.HexNumber" /> flag along with another value.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" /> is null.
        /// </exception>
        /// <exception cref="FormatException">
        ///     <paramref name="value" /> does not comply with the input pattern specified by <paramref name="style" />.
        /// </exception>
        public static BigInteger Parse(string value, NumberStyles style, IFormatProvider provider)
        {
            return ParseBigInteger(value, style, NumberFormatInfo.GetInstance(provider));
        }

        /// <summary>Raises a <see cref="BigInteger" /> value to the power of a specified value.</summary>
        /// <returns>The result of raising <paramref name="value" /> to the <paramref name="exponent" /> power.</returns>
        /// <param name="value">The number to raise to the <paramref name="exponent" /> power.</param>
        /// <param name="exponent">The exponent to raise <paramref name="value" /> by.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     The value of the <paramref name="exponent" /> parameter is
        ///     negative.
        /// </exception>
        public static BigInteger Pow(BigInteger value, int exponent)
        {
            if (exponent < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exponent), "The number must be greater than or equal to zero.");
            }

            switch (exponent)
            {
                case 0:
                    return One;

                case 1:
                    return value;

                default:
                    break;
            }

            if (value.InternalBits == null)
            {
                switch (value.InternalSign)
                {
                    case 1:
                        return value;

                    case -1:
                        return (exponent & 1) == 0 ? 1 : value;

                    case 0:
                        return value;

                    default:
                        break;
                }
            }

            var sign = 1;
            var regSquare = new BigIntegerBuilder(value, ref sign);
            var minSquareSize = regSquare.Size;
            var maxSquareSize = minSquareSize;
            var minSquare = regSquare.High;
            var maxSquare = minSquare + 1;
            if (maxSquare == 0)
            {
                maxSquareSize++;
                maxSquare = 1;
            }

            var minResultSize = 1;
            var maxResultSize = 1;
            uint resultMin = 1;
            uint resultMax = 1;
            for (var expTmp = exponent; ;)
            {
                if ((expTmp & 1) != 0)
                {
                    MulUpper(ref resultMax, ref maxResultSize, maxSquare, maxSquareSize);
                    MulLower(ref resultMin, ref minResultSize, minSquare, minSquareSize);
                }

                expTmp >>= 1;
                if (expTmp == 0)
                {
                    break;
                }

                MulUpper(ref maxSquare, ref maxSquareSize, maxSquare, maxSquareSize);
                MulLower(ref minSquare, ref minSquareSize, minSquare, minSquareSize);
            }

            if (maxResultSize > 1)
            {
                regSquare.EnsureWritable(maxResultSize, 0);
            }

            var regTmp = new BigIntegerBuilder(maxResultSize);
            var regResult = new BigIntegerBuilder(maxResultSize);
            regResult.Set(1);
            if ((exponent & 1) == 0)
            {
                sign = 1;
            }

            for (var expTmp = exponent; ;)
            {
                if ((expTmp & 1) != 0)
                {
                    NumericHelper.Swap(ref regResult, ref regTmp);
                    regResult.Mul(ref regSquare, ref regTmp);
                }

                expTmp >>= 1;
                if (expTmp == 0)
                {
                    break;
                }

                NumericHelper.Swap(ref regSquare, ref regTmp);
                regSquare.Mul(ref regTmp, ref regTmp);
            }

            return regResult.GetInteger(sign);
        }

        /// <summary>Performs integer division on two <see cref="BigInteger" /> values and returns the remainder.</summary>
        /// <returns>The remainder after dividing <paramref name="dividend" /> by <paramref name="divisor" />.</returns>
        /// <param name="dividend">The value to be divided.</param>
        /// <param name="divisor">The value to divide by.</param>
        /// <exception cref="DivideByZeroException">
        ///     <paramref name="divisor" /> is 0 (zero).
        /// </exception>
        public static BigInteger Remainder(BigInteger dividend, BigInteger divisor)
        {
            return dividend % divisor;
        }

        /// <summary>Subtracts one <see cref="BigInteger" /> value from another and returns the result.</summary>
        /// <returns>The result of subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
        /// <param name="left">The value to subtract from (the minuend).</param>
        /// <param name="right">The value to subtract (the subtrahend).</param>
        public static BigInteger Subtract(BigInteger left, BigInteger right)
        {
            return left - right;
        }

        /// <summary>
        ///     Tries to convert the string representation of a number to its <see cref="BigInteger" />
        ///     equivalent, and returns a value that indicates whether the conversion succeeded.
        /// </summary>
        /// <returns>true if <paramref name="value" /> was converted successfully; otherwise, false.</returns>
        /// <param name="value">The string representation of a number.</param>
        /// <param name="result">
        ///     When this method returns, contains the <see cref="BigInteger" /> equivalent to
        ///     the number that is contained in <paramref name="value" />, or zero (0) if the conversion fails. The conversion
        ///     fails if the <paramref name="value" /> parameter is null or is not of the correct format. This parameter is passed
        ///     uninitialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" /> is null.
        /// </exception>
        public static bool TryParse(string value, out BigInteger result)
        {
            return TryParseBigInteger(value, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        /// <summary>
        ///     Tries to convert the string representation of a number in a specified style and culture-specific format to its
        ///     <see cref="BigInteger" /> equivalent, and returns a value that indicates whether the conversion
        ///     succeeded.
        /// </summary>
        /// <returns>true if the <paramref name="value" /> parameter was converted successfully; otherwise, false.</returns>
        /// <param name="value">
        ///     The string representation of a number. The string is interpreted using the style specified by
        ///     <paramref name="style" />.
        /// </param>
        /// <param name="style">
        ///     A bitwise combination of enumeration values that indicates the style elements that can be present
        ///     in <paramref name="value" />. A typical value to specify is
        ///     <see cref="NumberStyles.Integer" />.
        /// </param>
        /// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="value" />.</param>
        /// <param name="result">
        ///     When this method returns, contains the <see cref="BigInteger" /> equivalent to
        ///     the number that is contained in <paramref name="value" />, or <see cref="Zero" /> if
        ///     the conversion failed. The conversion fails if the <paramref name="value" /> parameter is null or is not in a
        ///     format that is compliant with <paramref name="style" />. This parameter is passed uninitialized.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="style" /> is not a <see cref="NumberStyles" /> value.-or-
        ///     <paramref name="style" /> includes the <see cref="NumberStyles.AllowHexSpecifier" /> or
        ///     <see cref="NumberStyles.HexNumber" /> flag along with another value.
        /// </exception>
        public static bool TryParse(string value, NumberStyles style, IFormatProvider provider, out BigInteger result)
        {
            return TryParseBigInteger(value, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        /// <summary>
        ///     Compares this instance to a signed 64-bit integer and returns an integer that indicates whether the value of
        ///     this instance is less than, equal to, or greater than the value of the signed 64-bit integer.
        /// </summary>
        /// <returns>
        ///     A signed integer value that indicates the relationship of this instance to <paramref name="other" />, as shown
        ///     in the following table.Return valueDescriptionLess than zeroThe current instance is less than
        ///     <paramref name="other" />.ZeroThe current instance equals <paramref name="other" />.Greater than zeroThe current
        ///     instance is greater than <paramref name="other" />.
        /// </returns>
        /// <param name="other">The signed 64-bit integer to compare.</param>
        public int CompareTo(long other)
        {
            if (InternalBits == null)
            {
                return ((long)InternalSign).CompareTo(other);
            }

            if ((InternalSign ^ other) < 0)
            {
                return InternalSign;
            }

            var length = Length(InternalBits);
            if (length > 2)
            {
                return InternalSign;
            }

            var magnitude = other >= 0 ? (ulong)other : (ulong)-other;
            var unsigned = ULong(length, InternalBits);
            return InternalSign * unsigned.CompareTo(magnitude);
        }

        /// <summary>
        ///     Compares this instance to an unsigned 64-bit integer and returns an integer that indicates whether the value
        ///     of this instance is less than, equal to, or greater than the value of the unsigned 64-bit integer.
        /// </summary>
        /// <returns>
        ///     A signed integer that indicates the relative value of this instance and <paramref name="other" />, as shown in
        ///     the following table.Return valueDescriptionLess than zeroThe current instance is less than
        ///     <paramref name="other" />.ZeroThe current instance equals <paramref name="other" />.Greater than zeroThe current
        ///     instance is greater than <paramref name="other" />.
        /// </returns>
        /// <param name="other">The unsigned 64-bit integer to compare.</param>
        [CLSCompliant(false)]
        public int CompareTo(ulong other)
        {
            if (InternalSign < 0)
            {
                return -1;
            }

            if (InternalBits == null)
            {
                return ((ulong)InternalSign).CompareTo(other);
            }

            var length = Length(InternalBits);
            return length > 2 ? 1 : ULong(length, InternalBits).CompareTo(other);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Compares this instance to a second <see cref="BigInteger" /> and returns an integer that
        ///     indicates whether the value of this instance is less than, equal to, or greater than the value of the specified
        ///     object.
        /// </summary>
        /// <returns>
        ///     A signed integer value that indicates the relationship of this instance to <paramref name="other" />, as shown
        ///     in the following table.Return valueDescriptionLess than zeroThe current instance is less than
        ///     <paramref name="other" />.ZeroThe current instance equals <paramref name="other" />.Greater than zeroThe current
        ///     instance is greater than <paramref name="other" />.
        /// </returns>
        /// <param name="other">The object to compare.</param>
        public int CompareTo(BigInteger other)
        {
            if ((InternalSign ^ other.InternalSign) < 0)
            {
                return InternalSign >= 0 ? 1 : -1;
            }

            if (InternalBits == null)
            {
                if (other.InternalBits != null)
                {
                    return -other.InternalSign;
                }

                return InternalSign >= other.InternalSign ? InternalSign <= other.InternalSign ? 0 : 1 : -1;
            }

            if (other.InternalBits == null)
            {
                return InternalSign;
            }

            var length = Length(InternalBits);
            var otherLength = Length(other.InternalBits);
            if (length > otherLength)
            {
                return InternalSign;
            }

            if (length < otherLength)
            {
                return -InternalSign;
            }

            var diffLength = GetDiffLength(InternalBits, other.InternalBits, length);
            if (diffLength == 0)
            {
                return 0;
            }

            return InternalBits[diffLength - 1] >= other.InternalBits[diffLength - 1] ? InternalSign : -InternalSign;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Compares this instance to a specified object and returns an integer that indicates whether the value of this
        ///     instance is less than, equal to, or greater than the value of the specified object.
        /// </summary>
        /// <returns>
        ///     A signed integer that indicates the relationship of the current instance to the <paramref name="obj" />
        ///     parameter, as shown in the following table.Return valueDescriptionLess than zeroThe current instance is less than
        ///     <paramref name="obj" />.ZeroThe current instance equals <paramref name="obj" />.Greater than zeroThe current
        ///     instance is greater than <paramref name="obj" />, or the <paramref name="obj" /> parameter is null.
        /// </returns>
        /// <param name="obj">The object to compare.</param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="obj" /> is not a <see cref="BigInteger" />.
        /// </exception>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is BigInteger))
            {
                throw new ArgumentException("The parameter must be a BigInteger.");
            }

            return CompareTo((BigInteger)obj);
        }

        /// <summary>Returns a value that indicates whether the current instance and a specified object have the same value.</summary>
        /// <returns>
        ///     true if the <paramref name="obj" /> parameter is a <see cref="BigInteger" /> object or a
        ///     type capable of implicit conversion to a <see cref="BigInteger" /> value, and its value is equal
        ///     to the value of the current <see cref="BigInteger" /> object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare. </param>
        public override bool Equals(object obj)
        {
            return obj is BigInteger bigInteger && Equals(bigInteger);
        }

        /// <summary>Returns a value that indicates whether the current instance and a signed 64-bit integer have the same value.</summary>
        /// <returns>true if the signed 64-bit integer and the current instance have the same value; otherwise, false.</returns>
        /// <param name="other">The signed 64-bit integer value to compare.</param>
        public bool Equals(long other)
        {
            if (InternalBits == null)
            {
                return InternalSign == other;
            }

            if ((InternalSign ^ other) < 0)
            {
                return false;
            }

            var length = Length(InternalBits);
            if (length > 2)
            {
                return false;
            }

            var magnitude = other >= 0 ? (ulong)other : (ulong)-other;
            return ULong(length, InternalBits) == magnitude;
        }

        /// <summary>
        ///     Returns a value that indicates whether the current instance and an unsigned 64-bit integer have the same
        ///     value.
        /// </summary>
        /// <returns>true if the current instance and the unsigned 64-bit integer have the same value; otherwise, false.</returns>
        /// <param name="other">The unsigned 64-bit integer to compare.</param>
        [CLSCompliant(false)]
        public bool Equals(ulong other)
        {
            if (InternalSign < 0)
            {
                return false;
            }

            if (InternalBits == null)
            {
                return InternalSign == unchecked((long)other);
            }

            var length = Length(InternalBits);
            if (length > 2)
            {
                return false;
            }

            return ULong(length, InternalBits) == other;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns a value that indicates whether the current instance and a specified
        ///     <see cref="BigInteger" /> object have the same value.
        /// </summary>
        /// <returns>
        ///     true if this <see cref="BigInteger" /> object and <paramref name="other" /> have the same
        ///     value; otherwise, false.
        /// </returns>
        /// <param name="other">The object to compare.</param>
        public bool Equals(BigInteger other)
        {
            if (InternalSign != other.InternalSign)
            {
                return false;
            }

            if (InternalBits == other.InternalBits)
            {
                return true;
            }

            if (InternalBits == null || other.InternalBits == null)
            {
                return false;
            }

            var length = Length(InternalBits);
            if (length != Length(other.InternalBits))
            {
                return false;
            }

            var diffLength = GetDiffLength(InternalBits, other.InternalBits, length);
            return diffLength == 0;
        }

        /// <summary>Returns the hash code for the current <see cref="BigInteger" /> object.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            if (InternalBits == null)
            {
                return InternalSign;
            }

            var sign = InternalSign;
            var index = Length(InternalBits);
            while (true)
            {
                index--;
                if (index < 0)
                {
                    break;
                }

                sign = NumericsHelpers.CombineHash(sign, (int)InternalBits[index]);
            }

            return sign;
        }

        /// <summary>Converts a <see cref="BigInteger" /> value to a byte array.</summary>
        /// <returns>The value of the current <see cref="BigInteger" /> object converted to an array of bytes.</returns>
        public byte[] ToByteArray()
        {
            uint[] internalBits;
            byte highByte;
            switch (InternalBits)
            {
                case null when InternalSign == 0:
                    return new byte[1];

                case null:
                    internalBits = new[] { unchecked((uint)InternalSign) };
                    highByte = InternalSign < 0 ? (byte)0xff : (byte)0x00;
                    break;

                default:
                    if (InternalSign != -1)
                    {
                        internalBits = InternalBits;
                        highByte = 0;
                    }
                    else
                    {
                        internalBits = (uint[])InternalBits.Clone();
                        NumericsHelpers.DangerousMakeTwosComplement(internalBits);
                        highByte = 255;
                    }

                    break;
            }

            var bytes = new byte[checked(4 * internalBits.Length)];
            var index = 0;
            foreach (var value in internalBits)
            {
                var current = value;
                for (var j = 0; j < 4; j++)
                {
                    bytes[index] = (byte)(current & 255);
                    current >>= 8;
                    index++;
                }
            }

            var length = bytes.Length - 1;
            while (length > 0)
            {
                if (bytes[length] == highByte)
                {
                    length--;
                }
                else
                {
                    break;
                }
            }

            var extra = (bytes[length] & 128) != (highByte & 128);
            var result = new byte[length + 1 + (extra ? 1 : 0)];
            Array.Copy(bytes, result, length + 1);
            if (extra)
            {
                result[result.Length - 1] = highByte;
            }

            return result;
        }

        /// <summary>
        ///     Converts the numeric value of the current <see cref="BigInteger" /> object to its equivalent
        ///     string representation.
        /// </summary>
        /// <returns>The string representation of the current <see cref="BigInteger" /> value.</returns>
        public override string ToString()
        {
            return FormatBigInteger(this, null, NumberFormatInfo.CurrentInfo);
        }

        /// <summary>
        ///     Converts the numeric value of the current <see cref="BigInteger" /> object to its equivalent
        ///     string representation by using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        ///     The string representation of the current <see cref="BigInteger" /> value in the format
        ///     specified by the <paramref name="provider" /> parameter.
        /// </returns>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        public string ToString(IFormatProvider provider)
        {
            return FormatBigInteger(this, null, NumberFormatInfo.GetInstance(provider));
        }

        /// <summary>
        ///     Converts the numeric value of the current <see cref="BigInteger" /> object to its equivalent
        ///     string representation by using the specified format.
        /// </summary>
        /// <returns>
        ///     The string representation of the current <see cref="BigInteger" /> value in the format
        ///     specified by the <paramref name="format" /> parameter.
        /// </returns>
        /// <param name="format">A standard or custom numeric format string.</param>
        /// <exception cref="FormatException">
        ///     <paramref name="format" /> is not a valid format string.
        /// </exception>
        public string ToString(string format)
        {
            return FormatBigInteger(this, format, NumberFormatInfo.CurrentInfo);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Converts the numeric value of the current <see cref="BigInteger" /> object to its equivalent
        ///     string representation by using the specified format and culture-specific format information.
        /// </summary>
        /// <returns>
        ///     The string representation of the current <see cref="BigInteger" /> value as specified by the
        ///     <paramref name="format" /> and <paramref name="formatProvider" /> parameters.
        /// </returns>
        /// <param name="format">A standard or custom numeric format string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <exception cref="FormatException">
        ///     <paramref name="format" /> is not a valid format string.
        /// </exception>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return FormatBigInteger(this, format, NumberFormatInfo.GetInstance(formatProvider));
        }

        internal static int BitLengthOfUInt(uint x)
        {
            var numBits = 0;
            while (x > 0)
            {
                x >>= 1;
                numBits++;
            }

            return numBits;
        }

        internal static int GetDiffLength(uint[] internalBits, uint[] otherInternalBits, int length)
        {
            var index = length;
            do
            {
                index--;
                if (index >= 0)
                {
                    continue;
                }

                return 0;
            } while (internalBits[index] == otherInternalBits[index]);

            return index + 1;
        }

        internal static int Length(uint[] internalBits)
        {
            var length = internalBits.Length;
            if (internalBits[length - 1] != 0)
            {
                return length;
            }

            return length - 1;
        }

        private static bool GetPartsForBitManipulation(ref BigInteger x, out uint[] xd, out int xl)
        {
            xd = x.InternalBits ?? (x.InternalSign >= 0 ? new[] { unchecked((uint)x.InternalSign) } : new[] { unchecked((uint)-x.InternalSign) });
            xl = x.InternalBits?.Length ?? 1;
            return x.InternalSign < 0;
        }

        private static void ModPowInner(uint exp, ref BigIntegerBuilder regRes, ref BigIntegerBuilder regVal, ref BigIntegerBuilder regMod, ref BigIntegerBuilder regTmp)
        {
            while (exp != 0)
            {
                if ((exp & 1) == 1)
                {
                    ModPowUpdateResult(ref regRes, ref regVal, ref regMod, ref regTmp);
                }

                if (exp != 1)
                {
                    ModPowSquareModValue(ref regVal, ref regMod, ref regTmp);
                    exp >>= 1;
                }
                else
                {
                    break;
                }
            }
        }

        private static void ModPowInner32(uint exp, ref BigIntegerBuilder regRes, ref BigIntegerBuilder regVal, ref BigIntegerBuilder regMod, ref BigIntegerBuilder regTmp)
        {
            for (var index = 0; index < 32; index++)
            {
                if ((exp & 1) == 1)
                {
                    ModPowUpdateResult(ref regRes, ref regVal, ref regMod, ref regTmp);
                }

                ModPowSquareModValue(ref regVal, ref regMod, ref regTmp);
                exp >>= 1;
            }
        }

        private static void ModPowSquareModValue(ref BigIntegerBuilder regVal, ref BigIntegerBuilder regMod, ref BigIntegerBuilder regTmp)
        {
            NumericHelper.Swap(ref regVal, ref regTmp);
            regVal.Mul(ref regTmp, ref regTmp);
            regVal.Mod(ref regMod);
        }

        private static void ModPowUpdateResult(ref BigIntegerBuilder regRes, ref BigIntegerBuilder regVal, ref BigIntegerBuilder regMod, ref BigIntegerBuilder regTmp)
        {
            NumericHelper.Swap(ref regRes, ref regTmp);
            regRes.Mul(ref regTmp, ref regVal);
            regRes.Mod(ref regMod);
        }

        private static void MulLower(ref uint uHiRes, ref int cuRes, uint uHiMul, int cuMul)
        {
            var num = uHiRes * (ulong)uHiMul;
            var hi = NumericHelper.GetHi(num);
            if (hi == 0)
            {
                uHiRes = NumericHelper.GetLo(num);
                cuRes += cuMul - 1;
            }
            else
            {
                uHiRes = hi;
                cuRes += cuMul;
            }
        }

        private static void MulUpper(ref uint uHiRes, ref int cuRes, uint uHiMul, int cuMul)
        {
            var num = uHiRes * (ulong)uHiMul;
            var hi = NumericHelper.GetHi(num);
            if (hi == 0)
            {
                uHiRes = NumericHelper.GetLo(num);
                cuRes += cuMul - 1;
            }
            else
            {
                if (NumericHelper.GetLo(num) != 0)
                {
                    var num1 = hi + 1;
                    hi = num1;
                    if (num1 == 0)
                    {
                        hi = 1;
                        cuRes++;
                    }
                }

                uHiRes = hi;
                cuRes += cuMul;
            }
        }

        private static void SetBitsFromDouble(double value, out uint[]? bits, out int sign)
        {
            sign = 0;
            bits = null;
            NumericHelper.GetDoubleParts(value, out var valueSign, out var valueExp, out var valueMan, out _);
            if (valueMan == 0)
            {
                return;
            }

            if (valueExp <= 0)
            {
                if (valueExp <= -64)
                {
                    return;
                }

                var tmp = valueMan >> (-valueExp & 63);
                if (tmp > int.MaxValue)
                {
                    sign = 1;
                    bits = new[] { (uint)tmp, (uint)(tmp >> 32) };
                }
                else
                {
                    sign = (int)tmp;
                }

                if (valueSign < 0)
                {
                    sign = -sign;
                }
            }
            else if (valueExp > 11)
            {
                valueMan <<= 11;
                valueExp -= 11;
                var significantDword = ((valueExp - 1) / 32) + 1;
                var extraDword = (significantDword * 32) - valueExp;
                bits = new uint[significantDword + 2];
                bits[significantDword + 1] = (uint)(valueMan >> ((extraDword + 32) & 63));
                bits[significantDword] = (uint)(valueMan >> (extraDword & 63));
                if (extraDword > 0)
                {
                    bits[significantDword - 1] = (uint)valueMan << ((32 - extraDword) & 31);
                }

                sign = valueSign;
            }
            else
            {
                var tmp = valueMan << (valueExp & 63);
                if (tmp > int.MaxValue)
                {
                    sign = 1;
                    bits = new[] { (uint)tmp, (uint)(tmp >> 32) };
                }
                else
                {
                    sign = (int)tmp;
                }

                if (valueSign < 0)
                {
                    sign = -sign;
                }
            }
        }

        private static ulong ULong(int length, uint[] internalBits)
        {
            return length <= 1 ? internalBits[0] : NumericHelper.BuildUInt64(internalBits[1], internalBits[0]);
        }

        private uint[] ToUInt32Array()
        {
            uint[] internalBits;
            uint highDword;
            switch (InternalBits)
            {
                case null when InternalSign == 0:
                    return new uint[1];

                case null:
                    internalBits = new[] { unchecked((uint)InternalSign) };
                    highDword = (uint)(InternalSign >= 0 ? 0 : -1);
                    break;

                default:
                    if (InternalSign != -1)
                    {
                        internalBits = InternalBits;
                        highDword = 0;
                    }
                    else
                    {
                        internalBits = (uint[])InternalBits.Clone();
                        NumericsHelpers.DangerousMakeTwosComplement(internalBits);
                        highDword = unchecked((uint)-1);
                    }

                    break;
            }

            var length = internalBits.Length - 1;
            while (length > 0)
            {
                if (internalBits[length] == highDword)
                {
                    length--;
                }
                else
                {
                    break;
                }
            }

            var needExtraByte = (internalBits[length] & int.MinValue) != (highDword & int.MinValue);
            var trimmed = new uint[length + 1 + (!needExtraByte ? 0 : 1)];
            Array.Copy(internalBits, trimmed, length + 1);
            if (needExtraByte)
            {
                trimmed[trimmed.Length - 1] = highDword;
            }

            return trimmed;
        }
    }
}

#endif