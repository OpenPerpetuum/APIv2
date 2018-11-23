using OpenPerpetuum.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenPerpetuum.Core.Genxy
{
	/*
	 * Example GenxyString
	 * #zoneID=i4#victim=[|characterID=i2c9|nick=$Peanutbutterandjam|corporation=$Pretty Hate Machine|robot=i5cc|position=330f.42e.15a]#attackers=[|a0=[|attacker=[|characterID=i6a|nick=$Blocker|corporation=$Those Other Guys|robot=ice|position=3318.438.154]|damageDone=tD7907145|jammerTotal=i0|jammer=i0|demobilizer=i0|suppressor=i0|dispersion=t00000000|killingBlow=i0]|a1=[|attacker=[|characterID=i16|nick=$Rynikwiz|corporation=$Those Other Guys|robot=ice|position=3318.437.152]|damageDone=t15C7D844|jammerTotal=i0|jammer=i0|demobilizer=i0|suppressor=i0|dispersion=t00000000|killingBlow=i1]]
	 */
	public interface IGenxyReader
	{
		TObject Deserialise<TObject>(string serialisedData) where TObject : class, new();
	}

	internal sealed class GenxyReader : IGenxyReader
	{
		// Thread-safe way of lazily instantiating static values
		private static readonly Lazy<TypeInfo> typeInfo = new Lazy<TypeInfo>(() => typeof(GenxyReader).GetTypeInfo());

		public TObject Deserialise<TObject>(string serialisedData) where TObject : class, new()
		{
			using (TextReader reader = new StringReader(serialisedData))
			{
				return Deserialise<TObject>(reader);
			}
		}
		
		private TObject Deserialise<TObject>(TextReader reader) where TObject : class, new()
		{
			TObject outValue = new TObject();
			Type outType = typeof(TObject);

			PropertyInfo[] objectProps = outType.GetProperties();

			char currentChar;
			int nextValue;
			while ((nextValue = reader.Read()) > -1)
			{
				currentChar = (char)nextValue;
				if (currentChar == ']') // End of the object
					break;
				if (currentChar == '[') // start of the object
					continue;

				string currentKey = ReadKey(reader);
				// Get the information about that key on the object
				PropertyInfo prop = objectProps.SingleOrDefault(p => string.Equals(p.Name, currentKey, StringComparison.InvariantCultureIgnoreCase));
				TypeInfo propTypeInfo = prop.GetType().GetTypeInfo();
				if (prop == null)
					throw new KeyNotFoundException($"Unable to find specified property for {currentKey} on object type {outType.Name}");

				var method = typeInfo.Value.GetMethod("ReadValue", BindingFlags.Instance | BindingFlags.NonPublic);
				var genericMethod = method.MakeGenericMethod(prop.PropertyType);
				prop.SetValue(outValue, genericMethod.Invoke(this, new object[] { reader }));
			}

			return outValue;
		}

		private TObject[] DeserialiseArray<TObject>(TextReader reader) where TObject : class, new()
		{
			List<TObject> objectList = new List<TObject>();

			char currentChar;
			while ((currentChar = (char)reader.Read()) > -1)
			{
				if (currentChar == ']') // End of the array
					break;

				ReadKey(reader); // Consume the key, we don't need it.

				objectList.Add(Deserialise<TObject>(reader));
			}

			return objectList.ToArray();
		}

		private string ReadKey(TextReader reader)
		{
			string keyString = reader.ReadUntilDelimiter('=').Trim();

			return keyString;
		}

		private object ReadValue<TValue>(TextReader reader)
		{
			char currentCharacter;
			while( (currentCharacter = (char)reader.Read()) > -1)
			{
				if (currentCharacter == '=')
					continue;

				switch ((GenxyToken)currentCharacter)
				{
					case GenxyToken.StartProp:
						MethodInfo genericMethod;

						if (typeof(TValue).IsArray)
						{
							var method = typeInfo.Value.GetMethod("DeserialiseArray", BindingFlags.Instance | BindingFlags.NonPublic);
							genericMethod = method.MakeGenericMethod(typeof(TValue).GetElementType());
						}
						else
						{
							var method = typeInfo.Value.GetMethod("Deserialise", BindingFlags.Instance | BindingFlags.NonPublic);
							genericMethod = method.MakeGenericMethod(typeof(TValue));
						}

						return genericMethod.Invoke(this, new object[] { reader });
					case GenxyToken.String:
						return InternalReader.Instance.ReadEscapedString(reader);
					case GenxyToken.StringArray:
						return InternalReader.Instance.ReadEscapedStringArray(reader);
					case GenxyToken.Integer:
						return InternalReader.Instance.ReadInt(reader);
					case GenxyToken.IntegerArray:
						return InternalReader.Instance.ReadIntArray(reader);
					case GenxyToken.Long:
						return InternalReader.Instance.ReadLong(reader);
					case GenxyToken.LongArray:
						return InternalReader.Instance.ReadLongArray(reader);
					case GenxyToken.Decimal:
						return InternalReader.Instance.ReadDecimal(reader);
					case GenxyToken.DecimalArray:
						return InternalReader.Instance.ReadDecimalArray(reader);
					case GenxyToken.ByteArray:
						return InternalReader.Instance.ReadByteArray(reader);
					case GenxyToken.DecimalLong:
						return InternalReader.Instance.ReadDecimalLong(reader);
					case GenxyToken.Float:
						return (float)InternalReader.Instance.ReadFloat(reader);
					case GenxyToken.FloatBytes:
						return (float)InternalReader.Instance.ReadFloatBytes(reader);
					case GenxyToken.DoubleBytes:
						return InternalReader.Instance.ReadDoubleBytes(reader);
					case GenxyToken.Date:
						return InternalReader.Instance.ReadDate(reader);
					case GenxyToken.Color:
						return InternalReader.Instance.ReadColor(reader);
					//case GenxyToken.Area:
					//	return InternalReader.Instance.ReadArea();
					//case GenxyToken.AreaArray:
					//	return InternalReader.Instance.ReadAreaArray(reader);
					case GenxyToken.Point:
						return InternalReader.Instance.ReadPoint(reader);
					case GenxyToken.Position:
						return InternalReader.Instance.ReadPosition(reader);
					case GenxyToken.PositionArray:
						return InternalReader.Instance.ReadPositionArray(reader);
					default:
						return InternalReader.Instance.ReadValueAsString(reader);
				}
			}

			return default(TValue);
		}
	}
}
