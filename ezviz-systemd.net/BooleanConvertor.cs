﻿using ezviz_systemd.net.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ezviz_systemd.net
{
    public class BooleanConvertor : JsonConverter<bool>
    {
        private readonly JsonOptions options;

        public BooleanConvertor(JsonOptions options)
        {
            this.options = options;
        }

        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (this.options.SerializeBooleans)
            {
                case BooleanSerializationTypes.String:
                    return reader.GetString() == this.options.SerializeTrueAs;
                case BooleanSerializationTypes.Numbers:
                    return reader.GetInt32() == 1;
            }
            return reader.GetBoolean();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            switch (this.options.SerializeBooleans)
            {
                case BooleanSerializationTypes.Default:
                    writer.WriteBooleanValue(value);
                    break;
                case BooleanSerializationTypes.String:
                    writer.WriteStringValue(value ? this.options.SerializeTrueAs : this.options.SerializeFalseAs);
                    break;
                case BooleanSerializationTypes.Numbers:
                    writer.WriteNumberValue(value ? 1 : 0);
                    break;
            }            
        }
    }
}