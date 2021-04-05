using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Google.Protobuf;

namespace Flow.Sdk.Types
{
    public abstract class FlowValueType
    {

        public abstract string Type { get; }
        public abstract string AsJsonCadenceDataFormat();
        public ByteString ToByteString()
        {
            var format = this.AsJsonCadenceDataFormat();
            var bytes = Encoding.ASCII.GetBytes(format);
            return ByteString.CopyFrom(bytes);
        }
    }

    public abstract class FlowValueType<T> : FlowValueType
    {

        protected FlowValueType(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }

    //TODO: This should be a ValueType
    public class CompositeType
    {
        public string Type { get; set; }
        public string Id { get; set; }

        public Dictionary<string, string> Fields { get; set; }

        public string AsJsonCadenceDataFormat()
        {
            List<string> fields = new();
            foreach (var field in Fields)
            {
                var json = $"{{\"name\":\"{field.Key}\",\"value\":{field.Value}}}";
                fields.Add(json);
            }
            var result = $"{{\"type\":\"{Type}\",\"value\":{{\"id\":\"{Id}\",\"fields\":[{string.Join(',', fields)}]}}}}";
            return result;
        }
    }

    public class OptionalType : FlowValueType
    {
        public override string Type => "Optional";

        public FlowValueType Value { get; set; }

        public static OptionalType FromJson(string json)
        {
            var thing = JsonDocument.Parse(json);
            var attempt = thing.RootElement.GetProperty("value");
            var type = attempt.GetProperty("type").ToString();
            var value = attempt.GetProperty("value").ToString();
            var jsonObject = $"{{\"type\":\"{type}\",\"value\":\"{value}\"}}";
            var result = new OptionalType();
            result.Value = AddressType.FromJson(jsonObject);
            return result;
        }

        public override string AsJsonCadenceDataFormat()
         => $"{{\"type\":\"{Type}\",\"value\":{Value.AsJsonCadenceDataFormat()}}}";
    }

    //Not done
    public class AddressType : FlowValueType<string>
    {
        public AddressType(string value) : base(value)
        {
        }

        //TODO: I dont think this is correct
        public static string ConvertUInt64ToHex(ulong value)
        {
            return string.Format("0x{0:X}", value);
        }

        public override string Type
            => "Address";

        public override string AsJsonCadenceDataFormat()
            => $"{{\"type\":\"{Type}\",\"value\":\"{Value}\"}}";

        public static AddressType FromJson(string json)
        {
            var thing = JsonDocument.Parse(json);
            var attempt = thing.RootElement.GetProperty("value").ToString();
            return new AddressType(attempt);
        }
    }

    public class StringType : FlowValueType<string>
    {
        public StringType(string value) : base(value)
        {
        }

        public static StringType FromJson(string json)
        {
            var thing = JsonDocument.Parse(json);
            var value = thing.RootElement.GetProperty("value");
            return new StringType(value.GetString());
        }

        public override string Type
            => "String";

        public override string AsJsonCadenceDataFormat()
            => $"{{\"type\":\"{Type}\",\"value\":\"{Value}\"}}";
    }

    public class UInt64Type : FlowValueType<ulong>
    {

        //TODO: Clean this up
        public static UInt64Type FromJson(string json)
        {
            var thing = JsonDocument.Parse(json);
            //try and get the number from a string
            var value = thing.RootElement.GetProperty("value");
            //probably store as a string
            var attempt = UInt64.Parse(value.GetString());
            return new UInt64Type(attempt); ;
        }
        public UInt64Type(ulong value) : base(value)
        {
        }

        public override string Type
            => "UInt64";

        public override string AsJsonCadenceDataFormat()
            => $"{{\"type\":\"{Type}\",\"value\":\"{Value}\"}}";
    }

    public class UInt32Type : FlowValueType<uint>
    {

        //TODO: Clean this up
        public static UInt32Type FromJson(string json)
        {
            var thing = JsonDocument.Parse(json);
            //try and get the number from a string
            var value = thing.RootElement.GetProperty("value");
            //probably store as a string
            var attempt = UInt32.Parse(value.GetString());
            return new UInt32Type(attempt); ;
        }
        public UInt32Type(uint value) : base(value)
        {
        }

        public override string Type
            => "UInt32";

        public override string AsJsonCadenceDataFormat()
            => $"{{\"type\":\"{Type}\",\"value\":\"{Value}\"}}";
    }

     public class UFix64Type : FlowValueType<decimal>
    {

        //TODO: Clean this up
        public static UFix64Type FromJson(string json)
        {
            var thing = JsonDocument.Parse(json);
            //try and get the number from a string
            var value = thing.RootElement.GetProperty("value");
            //probably store as a string
            var attempt = decimal.Parse(value.GetString());
            return new UFix64Type(attempt); ;
        }
        public UFix64Type(decimal value) : base(value)
        {
        }

        public override string Type
            => "UFix64";

        public override string AsJsonCadenceDataFormat()
            => $"{{\"type\":\"{Type}\",\"value\":\"{Value}\"}}";
    }
}