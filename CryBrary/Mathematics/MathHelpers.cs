﻿using System;

namespace CryEngine.Mathematics
{
	/// <summary>
	/// Defines some useful and not so much mathematical functions.
	/// </summary>
	public static class MathHelpers
	{
		/// <summary>
		/// Moves <paramref name="actual" /> to <paramref name="goal" /> at given <paramref
		/// name="speed" />.
		/// </summary>
		/// <remarks>
		/// Do not use this method if <paramref name="speed" /> is too small when compared to
		/// difference between <paramref name="actual" /> and <paramref name="goal" />.
		/// </remarks>
		/// <param name="actual"> Value to move. </param>
		/// <param name="goal">   Destination of movement. </param>
		/// <param name="speed">  Speed of movement. </param>
		/// <param name="limit">  Limit of movement. </param>
		public static void Interpolate(ref Vector3 actual, Vector3 goal, float speed, float limit = 0)
		{
			var delta = goal - actual;

			if (limit > 0.001f)
			{
				float length = delta.Length;

				if (length > limit)
				{
					delta /= length;
					delta *= limit;
				}
			}

			actual += delta * Min(Time.DeltaTime * speed, 1.0f);
		}
		/// <summary>
		/// Moves <paramref name="actual" /> to <paramref name="goal" /> at given <paramref
		/// name="speed" />.
		/// </summary>
		/// <remarks>
		/// Do not use this method if <paramref name="speed" /> is too small when compared to
		/// difference between <paramref name="actual" /> and <paramref name="goal" />.
		/// </remarks>
		/// <param name="actual"> Value to move. </param>
		/// <param name="goal">   Destination of movement. </param>
		/// <param name="speed">  Speed of movement. </param>
		/// <param name="limit">  Limit of movement. </param>
		public static void Interpolate(ref Vector2 actual, Vector2 goal, float speed, float limit = 0)
		{
			var delta = goal - actual;

			if (limit > 0.001f)
			{
				float length = delta.Length;

				if (length > limit)
				{
					delta /= length;
					delta *= limit;
				}
			}

			actual += delta * Min(Time.DeltaTime * speed, 1.0f);
		}
		/// <summary>
		/// Moves <paramref name="actual" /> to <paramref name="goal" /> at given <paramref
		/// name="speed" />.
		/// </summary>
		/// <remarks>
		/// Do not use this method if <paramref name="speed" /> is too small when compared to
		/// difference between <paramref name="actual" /> and <paramref name="goal" />.
		/// </remarks>
		/// <param name="actual"> Value to move. </param>
		/// <param name="goal">   Destination of movement. </param>
		/// <param name="speed">  Speed of movement. </param>
		/// <param name="limit">  Limit of movement. </param>
		public static void Interpolate(ref float actual, float goal, float speed, float limit = 0)
		{
			float delta = goal - actual;

			if (limit > 0.001f)
				delta = Max(Min(delta, limit), -limit);

			actual += delta * Min(Time.DeltaTime * speed, 1.0f);
		}
		/// <summary>
		/// Returns reciprocal square root of the value.
		/// </summary>
		/// <param name="d"> Value to calculate reciprocal square root from. </param>
		/// <returns> 1 / <paramref name="d" />. </returns>
		public static double ReciprocalSquareRoot(double d)
		{
			return 1.0 / Math.Sqrt(d);
		}
		/// <summary>
		/// Returns reciprocal square root of the value.
		/// </summary>
		/// <param name="d"> Value to calculate reciprocal square root from. </param>
		/// <returns> 1 / <paramref name="d" />. </returns>
		public static float ReciprocalSquareRoot(float d)
		{
			return (float)(1.0 / Math.Sqrt(d));
		}
		/// <summary>
		/// Calculates sine and cosine at the same time.
		/// </summary>
		/// <param name="a">      Angle to calculate sine and cosine of. </param>
		/// <param name="sinVal"> Resultant sine. </param>
		/// <param name="cosVal"> Resultant cosine. </param>
		public static void SinCos(double a, out double sinVal, out double cosVal)
		{
			sinVal = Math.Sin(a);

			cosVal = Math.Sqrt(1.0 - sinVal * sinVal);
		}
		/// <summary>
		/// Calculates sine and cosine at the same time.
		/// </summary>
		/// <param name="a">      Angle to calculate sine and cosine of. </param>
		/// <param name="sinVal"> Resultant sine. </param>
		/// <param name="cosVal"> Resultant cosine. </param>
		public static void SinCos(float a, out float sinVal, out float cosVal)
		{
			sinVal = (float)Math.Sin(a);

			cosVal = (float)Math.Sqrt(1.0f - sinVal * sinVal);
		}
		/// <summary>
		/// Calculates logarithm of the quaternion.
		/// </summary>
		/// <param name="quaternion"> Quaternion to calculate logarithm from. </param>
		/// <returns>
		/// Vector for which <see cref="Exp(Vector3)" /> returns <paramref name="quaternion" />.
		/// </returns>
		public static Vector3 Log(Quaternion quaternion)
		{
			var lensqr = quaternion.V.LengthSquared;
			if (!(lensqr > 0.0f))
			{
				// logarithm of a quaternion, imaginary part (the real part of the logarithm is
				// always 0).
				return new Vector3(0);
			}
			var len = Math.Sqrt(lensqr);
			var angle = Math.Atan2(len, quaternion.W) / len;
			return quaternion.V * (float)angle;
		}
		/// <summary>
		/// Calculates a unit quaternion from a vector.
		/// </summary>
		/// <param name="v"> Vector to calculate quaternion from. </param>
		/// <returns> A new unit quaternion. </returns>
		public static Quaternion Exp(Vector3 v)
		{
			var lensqr = v.LengthSquared;
			if (!(lensqr > 0.0f))
			{
				return Quaternion.Identity;
			}
			var len = (float)Math.Sqrt(lensqr);
			float s, c;
			SinCos(len, out s, out c);
			s /= len;
			return new Quaternion(c, v.X * s, v.Y * s, v.Z * s);
		}
		/// <summary>
		/// Determines whether a value is inside the specified range.
		/// </summary>
		/// <typeparam name="T"> Type that implements <see cref="IComparable{T}" /> interface. </typeparam>
		/// <param name="value"> Value to check. </param>
		/// <param name="min">   Value that defines left bound of the range. </param>
		/// <param name="max">   Value that defines right bound of the range. </param>
		/// <returns> True, <paramref name="value" /> if it is within the range. Otherwise false. </returns>
		public static bool IsInRange<T>(T value, T min, T max) where T : IComparable<T>
		{
			if (value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0)
				return true;

			return false;
		}
		/// <summary>
		/// Clamps a value into a specified range.
		/// </summary>
		/// <typeparam name="T"> Type that implements <see cref="IComparable{T}" /> interface. </typeparam>
		/// <param name="value"> Value to clamp.</param>
		/// <param name="min">   Value that defines left bound of the range.</param>
		/// <param name="max">   Value that defines right bound of the range.</param>
		/// <returns>
		/// <para><paramref name="value" /> if it is within the range;</para>
		///
		/// <para><paramref name="min" />, if <paramref name="value" /> is less then <paramref name="min" />;</para>
		///
		/// <para><paramref name="max" />, if <paramref name="value" /> is greater then <paramref name="max" />.</para>
		/// </returns>
		public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
		{
			if (value.CompareTo(min) < 0)
				return min;
			if (value.CompareTo(max) > 0)
				return max;

			return value;
		}
		/// <summary>
		/// Normalizes the angle and then clamps it into given range.
		/// </summary>
		/// <param name="angle"> Angle to clamp. </param>
		/// <param name="min">   Number that defines left bound of the range. </param>
		/// <param name="max">   Number that defines right bound of the range. </param>
		/// <returns>
		/// <para><paramref name="angle" /> if it is within the range after normalization;</para>
		///
		/// <para><paramref name="min" />, if <paramref name="angle" /> is less then <paramref name="min" />;</para>
		///
		/// <para><paramref name="max" />, if <paramref name="angle" /> is greater then <paramref name="max" />.</para>
		/// </returns>
		public static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360)
				angle += 360;
			if (angle > 360)
				angle -= 360;

			return Clamp(angle, min, max);
		}
		/// <summary>
		/// Determines which of two comparable values is the biggest one.
		/// </summary>
		/// <typeparam name="T"> Type that implements <see cref="IComparable{T}" /> interface. </typeparam>
		/// <param name="val1"> First value. </param>
		/// <param name="val2"> Second value. </param>
		/// <returns> The value that is greater then another. </returns>
		public static T Max<T>(T val1, T val2) where T : IComparable<T>
		{
			return val1.CompareTo(val2) > 0 ? val1 : val2;
		}
		/// <summary>
		/// Determines which of two comparable values is the smallest one.
		/// </summary>
		/// <typeparam name="T"> Type that implements <see cref="IComparable{T}" /> interface. </typeparam>
		/// <param name="val1"> First value. </param>
		/// <param name="val2"> Second value. </param>
		/// <returns> The value that is smaller then another. </returns>
		public static T Min<T>(T val1, T val2) where T : IComparable<T>
		{
			return val1.CompareTo(val2) < 0 ? val1 : val2;
		}
		/// <summary>
		/// Determines whether given value is a power of 2.
		/// </summary>
		/// <param name="value"> Number to check. </param>
		/// <returns> True, if <paramref name="value" /> is a power of 2. </returns>
		public static bool IsPowerOfTwo(int value)
		{
			return (value & (value - 1)) == 0;
		}
		/// <summary>
		/// Determines whether given number is valid.
		/// </summary>
		/// <param name="value"> Number to check. </param>
		/// <returns> True, if <paramref name="value" /> is a valid number. </returns>
		public static bool IsNumberValid(double value)
		{
			const ulong mask = (UInt64)(255 << 55);

			return ((UInt64)value & mask) != mask;
		}
		/// <summary>
		/// Determines whether given number is valid.
		/// </summary>
		/// <param name="value"> Number to check. </param>
		/// <returns> True, if <paramref name="value" /> is a valid number. </returns>
		public static bool IsNumberValid(float value)
		{
			const int mask = 0xFF << 23;

			return ((UInt32)value & mask) != mask;
		}
		/// <summary>
		/// The value for which all absolute numbers smaller than are considered equal to zero.
		/// </summary>
		public const float ZeroTolerance = 1e-6f;
		/// <summary>
		/// Doubled <see cref="Math.PI" />.
		/// </summary>
		public const double PI2 = 2 * Math.PI;
	}
}