using System;

namespace KSoft.Collections;

/// <summary>A reference-backed typed facade over Phoenix's existing bit storage and XML name mapping.</summary>
/// <typeparam name="TBits">A dense code-enum domain whose numeric indices equal its proto-enum ordinal IDs.</typeparam>
/// <remarks>Accepted domains have exactly one member per index and names unique under the proto-enum's case-insensitive lookup. Gaps, aliases, negative/count sentinels, and FlagsAttribute domains are rejected. This is narrower than <see cref="EnumBitSet{TEnum}"/>; database-defined domains remain untyped <see cref="BBitSet"/>. Copying a reference shares mutations. Ordinary obsolete names remain represented, and XmlIgnoreAttribute does not independently filter the existing Phoenix bit-flag serializer.</remarks>
public sealed class BBitSet<TBits> : BBitSetBase where TBits : struct, Enum
{
	static readonly Lazy<BBitSetParams> kParams = new(CreateParams);

	/// <summary>Creates storage for the validated code-enum domain.</summary>
	/// <remarks>Domain validation occurs during construction, not compilation. A validation failure is cached for the closed type and rethrown by later constructions.</remarks>
	/// <exception cref="ArgumentException">The domain does not meet the numeric-index, ordinal-ID, or name-uniqueness requirements.</exception>
	public BBitSet() : base(kParams.Value) { }

	/// <summary>Gets or changes a declared bit on the shared set object.</summary>
	/// <param name="bit">The declared bit to address.</param>
	/// <returns>The stored bit state.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="bit"/> is not a usable declared member.</exception>
	public bool this[TBits bit]
	{
		get => Test(bit);
		set => Set(bit, value);
	}

	/// <summary>Tests a usable declared bit.</summary>
	/// <param name="bit">The declared bit to test.</param>
	/// <returns>The stored bit state.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="bit"/> is not a usable declared member.</exception>
	public bool Test(TBits bit) => GetBit(EnumBitTraits<TBits>.ToIndex(bit));
	/// <summary>Changes a declared bit in place, restoring code-enum storage when needed.</summary>
	/// <param name="bit">The declared bit to change.</param>
	/// <param name="value">The new bit state.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="bit"/> is not a usable declared member.</exception>
	public void Set(TBits bit, bool value = true) => base.Set(EnumBitTraits<TBits>.ToIndex(bit), value);
	/// <summary>Flips a declared bit on this shared set object.</summary>
	/// <param name="bit">The declared bit to change.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="bit"/> is not a usable declared member.</exception>
	public void Toggle(TBits bit) => Set(bit, !Test(bit));

	static BBitSetParams CreateParams()
	{
		var proto = new CodeEnum<TBits>();
		var values = Enum.GetValues<TBits>();
		var names = Enum.GetNames<TBits>();
		if (proto.MemberCount != EnumBitTraits<TBits>.Length)
		{
			throw new ArgumentException("The code enum must have one member for every numeric bit index and no sentinels.");
		}
		for (int i = 0; i < values.Length; i++)
		{
			if (!EnumBitTraits<TBits>.IsDefinedIndex(i) ||
				Reflection.EnumValue<TBits>.ToUInt64(values[i]) != (ulong)i ||
				proto.GetMemberId(names[i]) != i)
			{
				throw new ArgumentException("The code enum's names, ordinal IDs, and numeric bit indices must agree.");
			}
		}
		return new BBitSetParams(() => proto);
	}
}
